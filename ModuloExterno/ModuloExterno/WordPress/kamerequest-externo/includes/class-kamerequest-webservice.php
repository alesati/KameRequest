<?php

if (!defined('ABSPATH')) {
    exit;
}

// cliente SOAP que usa mensajes XML para no depender de la extension soap de PHP
class KameRequest_Externo_WebService
{
    private const NAMESPACE_SERVICIO = 'http://tempuri.org/';
    private const NAMESPACE_MODELOS = 'http://schemas.datacontract.org/2004/07/KameRequest.WebService.Modelos';
    private const NAMESPACE_ENVELOPE = 'http://schemas.xmlsoap.org/soap/envelope/';

    // toma la direccion del servicio configurada desde el panel de wordpress
    private function obtener_url_servicio()
    {
        return untrailingslashit((string) get_option('kamerequest_externo_ws_url', ''));
    }

    // protege valores que se van a escribir dentro del XML
    private function texto_xml($valor)
    {
        return htmlspecialchars((string) $valor, ENT_XML1 | ENT_QUOTES, 'UTF-8');
    }

    // transforma un nodo XML en arreglo usando el nombre de cada elemento
    private function nodo_a_arreglo(DOMElement $nodo)
    {
        $resultado = array();
        $tiene_hijos = false;

        foreach ($nodo->childNodes as $hijo) {
            if ($hijo->nodeType !== XML_ELEMENT_NODE) {
                continue;
            }

            $tiene_hijos = true;
            $nombre = $hijo->localName;
            $valor = $this->nodo_a_arreglo($hijo);

            if (array_key_exists($nombre, $resultado)) {
                if (!is_array($resultado[$nombre]) || !array_is_list($resultado[$nombre])) {
                    $resultado[$nombre] = array($resultado[$nombre]);
                }
                $resultado[$nombre][] = $valor;
            } else {
                $resultado[$nombre] = $valor;
            }
        }

        return $tiene_hijos ? $resultado : trim($nodo->textContent);
    }

    // envia un sobre SOAP usando wp_remote_post que esta disponible en wordpress
    private function ejecutar($operacion, $contenido = '')
    {
        $url = $this->obtener_url_servicio();
        if (empty($url)) {
            return new WP_Error('kamerequest_sin_url', 'No se ha configurado la URL del Web Service.');
        }

        if (!class_exists('DOMDocument')) {
            return new WP_Error('kamerequest_sin_xml', 'El servidor no tiene habilitado el soporte XML de PHP.');
        }

        $sobre = '<?xml version="1.0" encoding="UTF-8"?>'
            . '<s:Envelope xmlns:s="' . self::NAMESPACE_ENVELOPE . '"><s:Body>'
            . $contenido
            . '</s:Body></s:Envelope>';

        $respuesta = wp_remote_post($url, array(
            'timeout' => 45,
            'headers' => array(
                'Content-Type' => 'text/xml; charset=utf-8',
                'SOAPAction' => '"' . self::NAMESPACE_SERVICIO . 'IServicioSolicitudesExternas/' . $operacion . '"'
            ),
            'body' => $sobre
        ));

        if (is_wp_error($respuesta)) {
            return new WP_Error('kamerequest_ws', 'No fue posible conectar con el Web Service: ' . $respuesta->get_error_message());
        }

        $xml = new DOMDocument();
        if (!@$xml->loadXML(wp_remote_retrieve_body($respuesta))) {
            return new WP_Error('kamerequest_ws', 'El Web Service devolvio una respuesta no valida.');
        }

        $cuerpo = $xml->getElementsByTagNameNS(self::NAMESPACE_ENVELOPE, 'Body')->item(0);
        $nodo_respuesta = $cuerpo instanceof DOMElement ? $cuerpo->firstChild : null;
        while ($nodo_respuesta !== null && $nodo_respuesta->nodeType !== XML_ELEMENT_NODE) {
            $nodo_respuesta = $nodo_respuesta->nextSibling;
        }

        if (!$nodo_respuesta instanceof DOMElement) {
            return new WP_Error('kamerequest_ws', 'El Web Service no devolvio datos.');
        }

        if ($nodo_respuesta->localName === 'Fault') {
            return new WP_Error('kamerequest_ws', trim($nodo_respuesta->textContent));
        }

        return $this->nodo_a_arreglo($nodo_respuesta);
    }

    // obtiene una propiedad de la respuesta aunque venga vacia
    private function propiedad($respuesta, $nombre, $defecto = null)
    {
        return is_array($respuesta) && array_key_exists($nombre, $respuesta)
            ? $respuesta[$nombre]
            : $defecto;
    }

    // SOAP devuelve true y false como texto, por eso se convierten antes de validar
    private function booleano($valor)
    {
        return $valor === true || $valor === 1 || $valor === '1' || strtolower((string) $valor) === 'true';
    }

    // normaliza listas con una sola fila o varias filas
    private function lista($datos, $nombre)
    {
        if (is_array($datos) && isset($datos[$nombre])) {
            $datos = $datos[$nombre];
        }

        if (empty($datos)) {
            return array();
        }

        return is_array($datos) && array_is_list($datos) ? $datos : array($datos);
    }

    // adapta los nombres del contrato al formato que usa wordpress
    private function solicitud_publicada($fila)
    {
        return array(
            'idSolicitud' => $fila['IdSolicitud'] ?? 0,
            'consecutivoOficio' => $fila['ConsecutivoOficio'] ?? '',
            'titulo' => $fila['Titulo'] ?? '',
            'representante' => $fila['Representante'] ?? null,
            'fechaVencimientoPublicacion' => $fila['FechaVencimientoPublicacion'] ?? ''
        );
    }

    private function detalle_solicitud_convertido($fila)
    {
        return array(
            'idSolicitud' => $fila['IdSolicitud'] ?? 0,
            'consecutivoOficio' => $fila['ConsecutivoOficio'] ?? '',
            'documentoRespuesta' => $fila['DocumentoRespuesta'] ?? null,
            'documentoInicio' => $fila['DocumentoInicio'] ?? null,
            'titulo' => $fila['Titulo'] ?? '',
            'descripcion' => $fila['Descripcion'] ?? null,
            'representante' => $fila['Representante'] ?? null,
            'observaciones' => $fila['Observaciones'] ?? null,
            'estado' => $fila['Estado'] ?? '',
            'fechaIngreso' => $fila['FechaIngreso'] ?? '',
            'fechaRespuesta' => $fila['FechaRespuesta'] ?? null,
            'fechaInicio' => $fila['FechaInicio'] ?? null,
            'fechaVencimientoPublicacion' => $fila['FechaVencimientoPublicacion'] ?? ''
        );
    }

    private function participacion_convertida($fila)
    {
        return array(
            'idParticipacion' => $fila['IdParticipacion'] ?? 0,
            'consecutivoOficio' => $fila['ConsecutivoOficio'] ?? '',
            'titulo' => $fila['Titulo'] ?? '',
            'fechaParticipacion' => $fila['FechaParticipacion'] ?? '',
            'estado' => $fila['Estado'] ?? ''
        );
    }

    // EXT2 consulta las solicitudes externas publicadas
    public function listar_solicitudes()
    {
        $respuesta = $this->ejecutar('ListarSolicitudesPublicadas', '<ListarSolicitudesPublicadas xmlns="' . self::NAMESPACE_SERVICIO . '" />');
        if (is_wp_error($respuesta)) {
            return $respuesta;
        }

        $datos = $this->propiedad($respuesta, 'ListarSolicitudesPublicadasResult', array());
        return array_map(array($this, 'solicitud_publicada'), $this->lista($datos, 'SolicitudPublicadaDto'));
    }

    // EXT3 consulta los datos completos de una solicitud
    public function detalle_solicitud($id_solicitud)
    {
        $contenido = '<ObtenerDetalleSolicitud xmlns="' . self::NAMESPACE_SERVICIO . '"><idSolicitud>'
            . absint($id_solicitud)
            . '</idSolicitud></ObtenerDetalleSolicitud>';
        $respuesta = $this->ejecutar('ObtenerDetalleSolicitud', $contenido);
        if (is_wp_error($respuesta)) {
            return $respuesta;
        }

        $datos = $this->propiedad($respuesta, 'ObtenerDetalleSolicitudResult');
        return empty($datos)
            ? new WP_Error('kamerequest_ws', 'La solicitud no está disponible o su publicación venció.')
            : $this->detalle_solicitud_convertido($datos);
    }

    // EXT4 transforma los datos del formulario a XML y registra la participacion
    public function registrar_participacion($id_solicitud, $datos)
    {
        $proveedor = $datos['proveedor'] ?? array();
        $desgloses_xml = '';

        foreach ((array) ($datos['desgloses'] ?? array()) as $fila) {
            $desgloses_xml .= '<d:DesglosePropuestaDto>'
                . '<d:Anio>' . absint($fila['anio'] ?? 0) . '</d:Anio>'
                . '<d:Horas>' . (float) ($fila['horas'] ?? 0) . '</d:Horas>'
                . '<d:Mes>' . absint($fila['mes'] ?? 0) . '</d:Mes>'
                . '<d:Monto>' . (float) ($fila['monto'] ?? 0) . '</d:Monto>'
                . '<d:Observaciones>' . $this->texto_xml($fila['observaciones'] ?? '') . '</d:Observaciones>'
                . '<d:PorcentajeCobro>' . (float) ($fila['porcentajeCobro'] ?? 0) . '</d:PorcentajeCobro>'
                . '</d:DesglosePropuestaDto>';
        }

        $contenido = '<RegistrarParticipacion xmlns="' . self::NAMESPACE_SERVICIO . '">'
            . '<idSolicitud>' . absint($id_solicitud) . '</idSolicitud>'
            . '<participacion xmlns:d="' . self::NAMESPACE_MODELOS . '">'
            . '<d:Desgloses>' . $desgloses_xml . '</d:Desgloses>'
            . '<d:Proveedor>'
            . '<d:CedulaJuridica>' . $this->texto_xml($proveedor['cedulaJuridica'] ?? '') . '</d:CedulaJuridica>'
            . '<d:Correo>' . $this->texto_xml($proveedor['correo'] ?? '') . '</d:Correo>'
            . '<d:NombreProveedor>' . $this->texto_xml($proveedor['nombreProveedor'] ?? '') . '</d:NombreProveedor>'
            . '<d:NombreRepresentante>' . $this->texto_xml($proveedor['nombreRepresentante'] ?? '') . '</d:NombreRepresentante>'
            . '<d:Telefono>' . $this->texto_xml($proveedor['telefono'] ?? '') . '</d:Telefono>'
            . '</d:Proveedor></participacion></RegistrarParticipacion>';
        $respuesta = $this->ejecutar('RegistrarParticipacion', $contenido);
        if (is_wp_error($respuesta)) {
            return $respuesta;
        }

        $resultado = $this->propiedad($respuesta, 'RegistrarParticipacionResult', array());
        if (!$this->booleano($resultado['Exito'] ?? false)) {
            return new WP_Error('kamerequest_ws', $resultado['Mensaje'] ?? 'No fue posible registrar la participación.');
        }

        return array('exito' => true, 'id' => $resultado['Id'] ?? null, 'mensaje' => $resultado['Mensaje'] ?? '');
    }

    // EXT4 revisa si el proveedor ya existe antes de mostrar los campos adicionales
    public function consultar_proveedor($cedula_juridica)
    {
        $contenido = '<ConsultarProveedor xmlns="' . self::NAMESPACE_SERVICIO . '"><cedulaJuridica>'
            . $this->texto_xml(sanitize_text_field($cedula_juridica))
            . '</cedulaJuridica></ConsultarProveedor>';
        $respuesta = $this->ejecutar('ConsultarProveedor', $contenido);
        if (is_wp_error($respuesta)) {
            return $respuesta;
        }

        $consulta = $this->propiedad($respuesta, 'ConsultarProveedorResult', array());
        if (!$this->booleano($consulta['FormatoValido'] ?? false)) {
            return new WP_Error('kamerequest_cedula', $consulta['Mensaje'] ?? 'La cedula juridica no es valida.');
        }

        $proveedor = $consulta['Proveedor'] ?? array();
        return array(
            'existe' => $this->booleano($consulta['Existe'] ?? false),
            'mensaje' => $consulta['Mensaje'] ?? '',
            'proveedor' => array(
                'cedulaJuridica' => $proveedor['CedulaJuridica'] ?? sanitize_text_field($cedula_juridica),
                'nombreProveedor' => $proveedor['NombreProveedor'] ?? '',
                'nombreRepresentante' => $proveedor['NombreRepresentante'] ?? '',
                'telefono' => $proveedor['Telefono'] ?? '',
                'correo' => $proveedor['Correo'] ?? ''
            )
        );
    }

    // EXT6 consulta las participaciones de un proveedor por cedula juridica
    public function consultar_participaciones($cedula_juridica)
    {
        $contenido = '<ConsultarParticipacionesProveedor xmlns="' . self::NAMESPACE_SERVICIO . '"><cedulaJuridica>'
            . $this->texto_xml(sanitize_text_field($cedula_juridica))
            . '</cedulaJuridica></ConsultarParticipacionesProveedor>';
        $respuesta = $this->ejecutar('ConsultarParticipacionesProveedor', $contenido);
        if (is_wp_error($respuesta)) {
            return $respuesta;
        }

        $consulta = $this->propiedad($respuesta, 'ConsultarParticipacionesProveedorResult', array());
        $resultado = $consulta['Resultado'] ?? array();
        if (!$this->booleano($resultado['Exito'] ?? false)) {
            return new WP_Error('kamerequest_ws', $resultado['Mensaje'] ?? 'Datos no encontrados.');
        }

        return array_map(array($this, 'participacion_convertida'), $this->lista($consulta['Participaciones'] ?? array(), 'ParticipacionProveedorDto'));
    }
}


