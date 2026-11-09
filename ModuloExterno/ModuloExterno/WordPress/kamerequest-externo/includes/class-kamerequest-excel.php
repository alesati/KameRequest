<?php

if (!defined('ABSPATH')) {
    exit;
}

// lee la primera hoja de un xlsx sin agregar librerias externas
class KameRequest_Externo_Excel {
    // valida y transforma las filas del excel al formato que usa el web service
    public static function leer($archivo)
    {
        if (($archivo['error'] ?? UPLOAD_ERR_NO_FILE) !== UPLOAD_ERR_OK) {
            return new WP_Error('kr_excel', 'No se pudo cargar el archivo Excel.');
        }
        if (strtolower(pathinfo($archivo['name'], PATHINFO_EXTENSION)) !== 'xlsx') {
            return new WP_Error('kr_excel', 'El archivo debe tener formato .xlsx.');
        }
        $archivos = self::extraer_archivos($archivo['tmp_name'], array(
            'xl/worksheets/sheet1.xml',
            'xl/sharedStrings.xml'
        ));
        if (is_wp_error($archivos)) {
            return $archivos;
        }

        $xml = $archivos['xl/worksheets/sheet1.xml'] ?? false;
        $compartidas = $archivos['xl/sharedStrings.xml'] ?? false;
        if ($xml === false) {
            return new WP_Error('kr_excel', 'No se encontró la primera hoja del archivo Excel.');
        }

        // excel guarda algunos textos en una lista compartida
        $textos = self::textos_compartidos($compartidas);
        $hoja = simplexml_load_string($xml);
        $espacioExcel = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main';
        $hoja->registerXPathNamespace('x', $espacioExcel);
        $filas = $hoja->xpath('//x:sheetData/x:row');
        $resultado = array();

        // columnas requeridas: Mes, Año, Horas, Monto, IVA, Total, Observaciones, Porcentaje de cobro
        foreach (array_slice($filas, 1) as $fila) {
            $valores = array();
            // se indica el espacio XML porque excel guarda sus celdas dentro de un namespace
            foreach ($fila->children($espacioExcel) as $celda) {
                $atributos = $celda->attributes();
                $columna = preg_replace('/\d+/', '', (string) ($atributos['r'] ?? ''));
                $indice = self::indice_columna($columna);
                $contenido = $celda->children($espacioExcel);
                $valor = (string) $contenido->v;
                $tipo = (string) ($atributos['t'] ?? '');

                if ($tipo === 's') {
                    $valor = $textos[(int) $valor] ?? '';
                }
                // algunos archivos modernos guardan el texto dentro de la propia celda
                if ($tipo === 'inlineStr') {
                    $valor = self::texto_integrado($contenido, $espacioExcel);
                }
                $valores[$indice] = $valor;
            }
            if (empty(array_filter($valores, static fn($valor) => $valor !== ''))) {
                continue;
            }

            $mes = absint($valores[0] ?? 0);
            $anio = absint($valores[1] ?? 0);
            $horas = (float) ($valores[2] ?? 0);
            $monto = (float) ($valores[3] ?? 0);
            $iva = (float) ($valores[4] ?? 0);
            $total = (float) ($valores[5] ?? 0);
            $observaciones = sanitize_text_field($valores[6] ?? '');
            $porcentaje = (float) ($valores[7] ?? 0);

            // valida que cada linea tenga los valores requeridos por la propuesta
            if ($mes < 1 || $mes > 12 || $anio < 2024 || $horas < 0 || $monto < 0 || $porcentaje < 0 || $porcentaje > 100 || $observaciones === '') {
                return new WP_Error('kr_excel', 'Cada línea del Excel debe tener todos los datos requeridos y valores válidos.');
            }

            $ivaEsperado = round($monto * 0.13, 2);
            $totalEsperado = round($monto + $ivaEsperado, 2);
            if (abs($iva - $ivaEsperado) > 0.01 || abs($total - $totalEsperado) > 0.01) {
                return new WP_Error('kr_excel', 'El IVA o el total no coincide con el monto indicado en el Excel.');
            }

            $resultado[] = array(
                'anio' => $anio,
                'mes' => $mes,
                'horas' => $horas,
                'monto' => $monto,
                'iva' => $iva,
                'total' => $total,
                'porcentajeCobro' => $porcentaje,
                'observaciones' => $observaciones
            );
        }

        return empty($resultado) ? new WP_Error('kr_excel', 'El Excel no contiene filas de propuesta.') : $resultado;
    }

    // usa ZipArchive cuando existe y un lector basico cuando el hosting no lo tiene habilitado
    private static function extraer_archivos($ruta, $nombres)
    {
        if (class_exists('ZipArchive')) {
            $zip = new ZipArchive();
            if ($zip->open($ruta) !== true) {
                return new WP_Error('kr_excel', 'El archivo Excel no es válido.');
            }

            $resultado = array();
            foreach ($nombres as $nombre) {
                $resultado[$nombre] = $zip->getFromName($nombre);
            }
            $zip->close();
            return $resultado;
        }

        if (!function_exists('gzinflate')) {
            return new WP_Error('kr_excel', 'El servidor no tiene soporte para abrir archivos Excel.');
        }

        // xlsx es un ZIP, se leen solo los XML necesarios sin instalar librerias extras
        $contenido = file_get_contents($ruta);
        $fin = strrpos($contenido, "PK\x05\x06");
        if ($contenido === false || $fin === false) {
            return new WP_Error('kr_excel', 'El archivo Excel no es válido.');
        }

        $central = unpack('vdisk/vinicio/ventradasDisco/ventradas/Vtamano/Vposicion/vcomentario', substr($contenido, $fin + 4, 18));
        $posicion = $central['posicion'];
        $resultado = array_fill_keys($nombres, false);

        for ($indice = 0; $indice < $central['entradas'] && $posicion < strlen($contenido); $indice++) {
            if (substr($contenido, $posicion, 4) !== "PK\x01\x02") {
                break;
            }

            $cabecera = unpack(
                'vcreado/vnecesario/vbanderas/vmetodo/vhora/vfecha/Vcrc/Vcomprimido/VsinComprimir/vnombre/vextra/vcomentario/vdisco/vinterno/Vexterno/Vlocal',
                substr($contenido, $posicion + 4, 42)
            );
            $nombre = substr($contenido, $posicion + 46, $cabecera['nombre']);
            $posicion += 46 + $cabecera['nombre'] + $cabecera['extra'] + $cabecera['comentario'];

            if (!array_key_exists($nombre, $resultado) || substr($contenido, $cabecera['local'], 4) !== "PK\x03\x04") {
                continue;
            }

            $local = unpack('vnecesario/vbanderas/vmetodo/vhora/vfecha/Vcrc/Vcomprimido/VsinComprimir/vnombre/vextra', substr($contenido, $cabecera['local'] + 4, 26));
            $inicioDatos = $cabecera['local'] + 30 + $local['nombre'] + $local['extra'];
            $comprimido = substr($contenido, $inicioDatos, $cabecera['comprimido']);
            $resultado[$nombre] = $cabecera['metodo'] === 0 ? $comprimido : gzinflate($comprimido);
        }

        return $resultado;
    }

    // obtiene la lista de textos que excel guarda en sharedStrings.xml
    private static function textos_compartidos($xml)
    {
        if ($xml === false) {
            return array();
        }
        $documento = simplexml_load_string($xml);
        $documento->registerXPathNamespace('x', 'http://schemas.openxmlformats.org/spreadsheetml/2006/main');
        return array_map(static fn($texto) => (string) $texto, $documento->xpath('//x:si/x:t'));
    }

    // obtiene texto de celdas que excel guarda dentro de la etiqueta is
    private static function texto_integrado($contenido, $espacioExcel)
    {
        $textoFinal = '';
        foreach ($contenido->children($espacioExcel) as $nodo) {
            if ($nodo->getName() !== 'is') {
                continue;
            }
            foreach ($nodo->children($espacioExcel) as $parte) {
                if ($parte->getName() === 't') {
                    $textoFinal .= (string) $parte;
                    continue;
                }
                // tambien admite texto dividido en varias partes con formato
                if ($parte->getName() === 'r') {
                    foreach ($parte->children($espacioExcel) as $texto) {
                        if ($texto->getName() === 't') {
                            $textoFinal .= (string) $texto;
                        }
                    }
                }
            }
        }
        return $textoFinal;
    }

    // convierte letras de columna como A o AB en una posicion numerica
    private static function indice_columna($letras)
    {
        $resultado = 0;
        foreach (str_split($letras) as $letra) {
            $resultado = $resultado * 26 + (ord($letra) - 64);
        }
        return $resultado - 1;
    }
}

