<?php

if (!defined('ABSPATH')) {
    exit;
}

// arma las pantallas publicas que se insertan con shortcodes de wordpress
class KameRequest_Externos_Shortcodes
{
    private $web_service;

    // registra los codigos cortos que se usan en las paginas de wordpress
    public function __construct()
    {
        $this->web_service = new KameRequest_Externo_WebService();
        add_shortcode('kamerequest_solicitudes', array($this, 'mostrar_solicitudes'));
        add_shortcode('kamerequest_detalle', array($this, 'mostrar_detalle'));
        add_shortcode('kamerequest_participaciones', array($this, 'mostrar_participaciones'));
    }

    // muestra un texto estandar cuando la solicitud no tiene un dato
    private function texto($valor)
    {
        return $valor === null || $valor === '' ? '<em>&lt;No indicado&gt;</em>' : esc_html($valor);
    }

    // crea las urls de detalle, participacion manual y carga por excel
    private function url_detalle($id_solicitud, $modo = '', $cedula = '')
    {
        $parametros = array('solicitud' => absint($id_solicitud));
        if ($modo !== '') {
            $parametros['kr_modo'] = $modo;
        }
        if ($cedula !== '') {
            $parametros['kr_cedula'] = $cedula;
        }
        return add_query_arg($parametros, get_permalink());
    }

    // muestra el listado publico de solicitudes publicadas
    public function mostrar_solicitudes()
    {
        // al seleccionar un oficio se reutiliza esta misma pagina para mostrar el detalle
        if (isset($_GET['solicitud'])) {
            return $this->mostrar_detalle();
        }

        $solicitudes = $this->web_service->listar_solicitudes();
        if (is_wp_error($solicitudes)) {
            return '<p class="kr-mensaje kr-error">' . esc_html($solicitudes->get_error_message()) . '</p>';
        }

        ob_start();
        echo '<div class="kr-contenedor"><h2>Solicitudes disponibles</h2>';
        if (empty($solicitudes)) {
            echo '<p>No hay solicitudes publicadas en este momento.</p>';
        } else {
            // solo muestra diez solicitudes para que el listado no se haga muy largo
            $registros_por_pagina = 10;
            $total_paginas = (int) ceil(count($solicitudes) / $registros_por_pagina);
            $pagina_actual = isset($_GET['kr_pagina']) ? absint($_GET['kr_pagina']) : 1;
            $pagina_actual = max(1, min($pagina_actual, $total_paginas));
            $inicio = ($pagina_actual - 1) * $registros_por_pagina;
            $solicitudes_pagina = array_slice($solicitudes, $inicio, $registros_por_pagina);

            echo '<table class="kr-tabla"><thead><tr><th>Oficio</th><th>Título</th><th>Representante</th></tr></thead><tbody>';
            foreach ($solicitudes_pagina as $solicitud) {
                $url = $this->url_detalle($solicitud['idSolicitud']);
                echo '<tr><td><a href="' . esc_url($url) . '">' . esc_html($solicitud['consecutivoOficio']) . '</a></td>';
                echo '<td>' . esc_html($solicitud['titulo']) . '</td><td>' . $this->texto($solicitud['representante'] ?? null) . '</td></tr>';
            }
            echo '</tbody></table>';

            if ($total_paginas > 1) {
                // conserva la misma pagina de wordpress al moverse entre resultados
                $url_base = remove_query_arg(array('solicitud', 'kr_pagina'));
                echo '<nav class="kr-paginacion" aria-label="Paginación de solicitudes">';

                if ($pagina_actual > 1) {
                    $url_anterior = add_query_arg('kr_pagina', $pagina_actual - 1, $url_base);
                    echo '<a class="kr-boton-secundario" href="' . esc_url($url_anterior) . '">Anterior</a>';
                }

                for ($pagina = 1; $pagina <= $total_paginas; $pagina++) {
                    $url_pagina = add_query_arg('kr_pagina', $pagina, $url_base);
                    $clase = $pagina === $pagina_actual ? ' kr-pagina-activa' : '';
                    echo '<a class="kr-pagina' . $clase . '" href="' . esc_url($url_pagina) . '">' . esc_html($pagina) . '</a>';
                }

                if ($pagina_actual < $total_paginas) {
                    $url_siguiente = add_query_arg('kr_pagina', $pagina_actual + 1, $url_base);
                    echo '<a class="kr-boton-secundario" href="' . esc_url($url_siguiente) . '">Siguiente</a>';
                }

                echo '</nav>';
            }
        }
        echo '</div>';
        return ob_get_clean();
    }

    // muestra la informacion completa de una solicitud externa
    public function mostrar_detalle()
    {
        $id_solicitud = isset($_GET['solicitud']) ? absint($_GET['solicitud']) : 0;
        if (!$id_solicitud) {
            return '<p class="kr-mensaje">Seleccione una solicitud para consultar su detalle.</p>';
        }

        $solicitud = $this->web_service->detalle_solicitud($id_solicitud);
        if (is_wp_error($solicitud)) {
            return '<p class="kr-mensaje kr-error">La solicitud no está disponible o su publicación venció.</p>';
        }

        // el modo define si se muestra el formulario manual o de excel
        $modo = sanitize_text_field(wp_unslash($_GET['kr_modo'] ?? ''));
        $cedula = sanitize_text_field(wp_unslash($_GET['kr_cedula'] ?? ''));
        $borrador = null;
        $token_borrador = sanitize_text_field(wp_unslash($_GET['kr_borrador'] ?? ''));
        if ($token_borrador !== '') {
            $borrador_guardado = get_transient('kr_borrador_' . $token_borrador);
            if ($borrador_guardado !== false && $borrador_guardado['solicitud'] === $id_solicitud) {
                $borrador = $borrador_guardado['datos'];
                $modo = $borrador_guardado['modo'];
                $cedula = $cedula ?: ($borrador['proveedor']['cedulaJuridica'] ?? '');
            }
        }

        $consulta_proveedor = null;
        if (($modo === 'manual' || $modo === 'excel') && $cedula !== '') {
            $consulta_proveedor = $this->web_service->consultar_proveedor($cedula);
        }
        $volver = remove_query_arg(array('solicitud', 'kr_modo', 'kr_cedula', 'kr_previa', 'kr_borrador', 'kr_exito', 'kr_error'));
        ob_start();
        echo '<div class="kr-contenedor"><a class="kr-boton-secundario" href="' . esc_url($volver) . '">Regresar</a>';
        if (isset($_GET['kr_exito'])) {
            echo '<p class="kr-mensaje kr-exito">La participación fue registrada correctamente.</p>';
        }
        if (isset($_GET['kr_error'])) {
            echo '<p class="kr-mensaje kr-error">' . esc_html(wp_unslash($_GET['kr_error'])) . '</p>';
        }

        echo '<h2>' . esc_html($solicitud['consecutivoOficio']) . ' - ' . esc_html($solicitud['titulo']) . '</h2><dl class="kr-detalle">';
        foreach (array(
            'Documento de respuesta' => 'documentoRespuesta',
            'Documento de inicio' => 'documentoInicio',
            'Descripción' => 'descripcion',
            'Representante' => 'representante',
            'Observaciones' => 'observaciones',
            'Estado' => 'estado',
            'Fecha de ingreso' => 'fechaIngreso',
            'Fecha de respuesta' => 'fechaRespuesta',
            'Fecha de inicio' => 'fechaInicio',
            'Vencimiento de publicación' => 'fechaVencimientoPublicacion'
        ) as $etiqueta => $campo) {
            echo '<dt>' . esc_html($etiqueta) . '</dt><dd>' . $this->texto($solicitud[$campo] ?? null) . '</dd>';
        }
        echo '</dl>';

        // al cargar un excel se revisan las lineas antes de registrar la participacion
        if (isset($_GET['kr_previa'])) {
            $token = sanitize_text_field(wp_unslash($_GET['kr_previa']));
            $previamente_cargado = get_transient('kr_previa_' . $token);
            if ($previamente_cargado !== false) {
                echo $this->vista_previa_excel($id_solicitud, $token, $previamente_cargado['datos']);
            }
        } elseif ($modo === 'manual' || $modo === 'excel') {
            echo $this->selector_proveedor($id_solicitud, $modo, $cedula);
            if (is_wp_error($consulta_proveedor)) {
                echo '<p class="kr-mensaje kr-error">' . esc_html($consulta_proveedor->get_error_message()) . '</p>';
            } elseif ($consulta_proveedor !== null) {
                echo $modo === 'manual'
                    ? $this->formulario_manual($id_solicitud, $consulta_proveedor, $borrador)
                    : $this->formulario_excel($id_solicitud, $consulta_proveedor, $borrador);
            }
        } else {
            echo '<p><a class="kr-boton" href="' . esc_url($this->url_detalle($id_solicitud, 'manual')) . '">Participar</a></p>';
        }
        echo '</div>';
        return ob_get_clean();
    }

    // primero se consulta la cedula y solo se muestran los datos extra si es proveedor nuevo
    private function selector_proveedor($id_solicitud, $modo, $cedula)
    {
        $id_pagina = get_queried_object_id();
        ob_start(); ?>
        <form class="kr-formulario kr-consulta-proveedor" method="get" action="<?php echo esc_url(get_permalink()); ?>">
            <input type="hidden" name="solicitud" value="<?php echo esc_attr($id_solicitud); ?>">
            <input type="hidden" name="kr_modo" value="<?php echo esc_attr($modo); ?>">
            <?php if ($id_pagina) { ?>
                <input type="hidden" name="page_id" value="<?php echo esc_attr($id_pagina); ?>">
            <?php } ?>
            <label>
                Cédula jurídica
                <input required name="kr_cedula" value="<?php echo esc_attr($cedula); ?>" placeholder="3-000-000000" pattern="3-[0-9]{3}-[0-9]{6}" title="Use el formato 3-000-000000">
            </label>
            <button class="kr-boton" type="submit">Siguiente</button>
        </form>
        <?php return ob_get_clean();
    }

    // agrega campos nuevos o reutiliza el proveedor que ya se encontro
    private function campos_proveedor($consulta, $borrador = null)
    {
        $proveedor = $borrador['proveedor'] ?? $consulta['proveedor'];
        ob_start();
        if ($consulta['existe']) { ?>
            <div class="kr-proveedor-encontrado">
                <strong>Proveedor encontrado.</strong>
            </div>
            <label>
                Cédula jurídica
                <input readonly name="cedula" value="<?php echo esc_attr($consulta['proveedor']['cedulaJuridica']); ?>">
            </label>
            <label>
                Nombre del proveedor
                <input readonly name="nombre" value="<?php echo esc_attr($consulta['proveedor']['nombreProveedor']); ?>">
            </label>
        <?php } else { ?>
            <div class="kr-proveedor-nuevo">
                <strong>Proveedor no encontrado.</strong> Complete los datos para registrarlo.
            </div>
            <label>
                Cédula jurídica
                <input readonly name="cedula" value="<?php echo esc_attr($consulta['proveedor']['cedulaJuridica']); ?>">
            </label>
            <h4>Datos del nuevo proveedor</h4>
            <label>
                Nombre del proveedor
                <input required name="nombre" value="<?php echo esc_attr($proveedor['nombreProveedor'] ?? ''); ?>">
            </label>
            <label>
                Nombre del representante
                <input required name="representante" value="<?php echo esc_attr($proveedor['nombreRepresentante'] ?? ''); ?>">
            </label>
            <label>
                Teléfono
                <input required name="telefono" value="<?php echo esc_attr($proveedor['telefono'] ?? ''); ?>">
            </label>
            <label>
                Correo electrónico
                <input required type="email" name="correo" value="<?php echo esc_attr($proveedor['correo'] ?? ''); ?>">
            </label>
        <?php }
        return ob_get_clean();
    }

    // formulario para registrar los periodos de la propuesta manualmente
    private function formulario_manual($id_solicitud, $consulta, $borrador = null)
    {
        ob_start(); ?>
        <form class="kr-formulario" method="post" action="<?php echo esc_url(admin_url('admin-post.php')); ?>" data-kr-formulario>
            <input type="hidden" name="action" value="kr_registrar_participacion">
            <?php wp_nonce_field('kr_participacion', 'kr_token'); ?>
            <input type="hidden" name="kr_solicitud" value="<?php echo esc_attr($id_solicitud); ?>">
            <input type="hidden" name="kr_modo" value="manual">
            <h3>Registrar participación</h3>
            <?php echo $this->campos_proveedor($consulta, $borrador); ?>
            <h4>Detalle de la propuesta</h4>
            <p>Este detalle es independiente del desglose oficial de la solicitud.</p>
            <!-- contenedor donde javascript agrega mas lineas de propuesta -->
            <div data-kr-lineas-propuesta>
                <?php
                $lineas = !empty($borrador['desgloses'])
                    ? $borrador['desgloses']
                    : array(array());

                foreach ($lineas as $indice => $linea) {
                ?>
                    <div class="kr-detalle-propuesta" data-kr-linea>
                        <label>
                            Mes
                            <select required name="desglose[<?php echo esc_attr($indice); ?>][mes]">
                                <option value="">Seleccione</option>
                                <?php
                                for ($mes = 1; $mes <= 12; $mes++) {
                                    $seleccionado = (int) ($linea['mes'] ?? 0) === $mes ? ' selected' : '';
                                    echo '<option value="' . esc_attr($mes) . '"' . $seleccionado . '>' . esc_html($mes) . '</option>';
                                }
                                ?>
                            </select>
                        </label>
                        <label>
                            Año
                            <input required type="number" min="2024" name="desglose[<?php echo esc_attr($indice); ?>][anio]" value="<?php echo esc_attr($linea['anio'] ?? ''); ?>">
                        </label>
                        <label>
                            Horas
                            <input required type="number" min="0" step="0.01" name="desglose[<?php echo esc_attr($indice); ?>][horas]" value="<?php echo esc_attr($linea['horas'] ?? ''); ?>">
                        </label>
                        <label>
                            Monto
                            <input required type="number" min="0" step="0.01" name="desglose[<?php echo esc_attr($indice); ?>][monto]" value="<?php echo esc_attr($linea['monto'] ?? ''); ?>" data-kr-monto>
                        </label>
                        <label>IVA 13% <input readonly data-kr-iva></label>
                        <label>Total <input readonly data-kr-total></label>
                        <label>
                            % de cobro
                            <input required type="number" min="0" max="100" step="0.01" name="desglose[<?php echo esc_attr($indice); ?>][porcentaje]" value="<?php echo esc_attr($linea['porcentajeCobro'] ?? ''); ?>">
                        </label>
                        <label>
                            Observaciones
                            <input required name="desglose[<?php echo esc_attr($indice); ?>][observaciones]" value="<?php echo esc_attr($linea['observaciones'] ?? ''); ?>">
                        </label>
                        <div class="kr-acciones-periodo">
                            <button class="kr-boton-secundario kr-boton-eliminar-periodo" type="button" data-kr-eliminar-detalle>Eliminar período</button>
                        </div>
                    </div>
                <?php } ?>
            </div>
            <p class="kr-aviso-periodo" data-kr-aviso-periodo hidden>Debe incluir al menos un período en la propuesta.</p>
            <div class="kr-acciones">
                <button class="kr-boton kr-boton-turquesa" type="button" data-kr-agregar-detalle>Agregar otro período</button>
                <button class="kr-boton kr-boton-azul" type="submit" name="kr_cargar_excel" value="1" formnovalidate>Cargar Excel</button>
                <button class="kr-boton" type="submit">Confirmar participación</button>
                <a class="kr-boton kr-boton-cancelar" href="<?php echo esc_url($this->url_detalle($id_solicitud)); ?>">Cancelar</a>
            </div>
        </form>
        <?php return ob_get_clean();
    }

    // formulario que recibe un archivo xlsx para generar una propuesta
    private function formulario_excel($id_solicitud, $consulta, $borrador = null)
    {
        ob_start(); ?>
        <form class="kr-formulario" method="post" enctype="multipart/form-data" action="<?php echo esc_url(admin_url('admin-post.php')); ?>">
            <input type="hidden" name="action" value="kr_registrar_participacion">
            <?php wp_nonce_field('kr_participacion', 'kr_token'); ?>
            <input type="hidden" name="kr_solicitud" value="<?php echo esc_attr($id_solicitud); ?>">
            <input type="hidden" name="kr_modo" value="excel">
            <h3>Cargar propuesta por Excel</h3>
            <?php echo $this->campos_proveedor($consulta, $borrador); ?>
            <label>Archivo Excel (.xlsx) <input required accept=".xlsx" type="file" name="archivo_excel"></label>
            <small>Columnas: Mes, Año, Horas, Monto, IVA, Total, Observaciones, Porcentaje de cobro.</small>
            <div class="kr-acciones">
                <a class="kr-boton-secundario" href="<?php echo esc_url($this->url_detalle($id_solicitud)); ?>">Cancelar</a>
                <button class="kr-boton" type="submit">Cargar Excel</button>
            </div>
        </form>
        <?php return ob_get_clean();
    }

    // enseña las lineas importadas antes de enviarlas definitivamente
    private function vista_previa_excel($id_solicitud, $token, $datos)
    {
        $desgloses = $datos['desgloses'] ?? array();
        $cedula = $datos['proveedor']['cedulaJuridica'] ?? '';
        ob_start();
        ?>
        <section class="kr-previsualizacion">
            <h3>Vista previa de la propuesta</h3>
            <p>Revise las líneas importadas antes de confirmar la participación.</p>
            <table class="kr-tabla">
                <thead>
                    <tr>
                        <th>Mes</th><th>Año</th><th>Horas</th><th>Monto</th>
                        <th>IVA</th><th>Total</th><th>% cobro</th><th>Observaciones</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($desgloses as $fila) {
                        $iva = $fila['iva'] ?? round($fila['monto'] * 0.13, 2);
                        $total = $fila['total'] ?? ($fila['monto'] + $iva);
                    ?>
                        <tr>
                            <td><?php echo esc_html($fila['mes']); ?></td>
                            <td><?php echo esc_html($fila['anio']); ?></td>
                            <td><?php echo esc_html($fila['horas']); ?></td>
                            <td><?php echo esc_html($fila['monto']); ?></td>
                            <td><?php echo esc_html($iva); ?></td>
                            <td><?php echo esc_html($total); ?></td>
                            <td><?php echo esc_html($fila['porcentajeCobro']); ?></td>
                            <td><?php echo esc_html($fila['observaciones']); ?></td>
                        </tr>
                    <?php } ?>
                </tbody>
            </table>
            <form class="kr-acciones" method="post" action="<?php echo esc_url(admin_url('admin-post.php')); ?>">
                <input type="hidden" name="action" value="kr_registrar_participacion">
                <?php wp_nonce_field('kr_participacion', 'kr_token'); ?>
                <input type="hidden" name="kr_modo" value="excel">
                <input type="hidden" name="kr_previa_token" value="<?php echo esc_attr($token); ?>">
                <a class="kr-boton-secundario" href="<?php echo esc_url($this->url_detalle($id_solicitud, 'excel', $cedula)); ?>">Regresar</a>
                <button class="kr-boton" type="submit">Confirmar participación</button>
            </form>
        </section>
        <?php
        return ob_get_clean();
    }

    // consulta participaciones anteriores por cedula juridica
    public function mostrar_participaciones()
    {
        $cedula = isset($_GET['cedula']) ? sanitize_text_field(wp_unslash($_GET['cedula'])) : '';
        $pagina_actual = get_permalink();
        $id_pagina = get_queried_object_id();
        ob_start();
        // se mantiene la pagina actual para que no redireccione al blog de wordpress
        echo '<div class="kr-contenedor"><h2>Mis participaciones</h2><form method="get" action="' . esc_url($pagina_actual) . '">';
        if ($id_pagina) {
            echo '<input type="hidden" name="page_id" value="' . esc_attr($id_pagina) . '">';
        }
        echo '<label>Cédula jurídica <input required name="cedula" value="' . esc_attr($cedula) . '" placeholder="3-000-000000" pattern="3-[0-9]{3}-[0-9]{6}" title="Use el formato 3-000-000000"></label><button class="kr-boton">Consultar</button></form>';
        if ($cedula !== '') {
            $participaciones = $this->web_service->consultar_participaciones($cedula);
            if (is_wp_error($participaciones)) {
                echo '<p class="kr-mensaje kr-error">' . esc_html($participaciones->get_error_message()) . '</p>';
            } else {
                echo '<table class="kr-tabla"><thead><tr><th>Oficio</th><th>Título</th><th>Fecha</th><th>Estado</th></tr></thead><tbody>';
                foreach ($participaciones as $item) {
                    echo '<tr><td>' . esc_html($item['consecutivoOficio']) . '</td><td>' . esc_html($item['titulo']) . '</td><td>' . esc_html($item['fechaParticipacion']) . '</td><td>' . esc_html($item['estado']) . '</td></tr>';
                }
                echo '</tbody></table>';
            }
        }
        echo '</div>';
        return ob_get_clean();
    }
}
