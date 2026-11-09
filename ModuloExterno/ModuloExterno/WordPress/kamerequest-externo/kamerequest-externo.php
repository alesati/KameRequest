<?php
/**
 * Plugin Name: KameRequest Externo
 * Description: Consulta solicitudes publicadas y registra participaciones mediante el Web Service de KameRequest.
 * Version: 1.3.6
 */

if (!defined('ABSPATH')) {
    exit;
}

// ruta del plugin para cargar sus estilos y javascript
define('KAMEREQUEST_EXTERNO_URL', plugin_dir_url(__FILE__));

// archivos que contienen la comunicacion, excel y pantallas publicas
require_once plugin_dir_path(__FILE__) . 'includes/class-kamerequest-webservice.php';
require_once plugin_dir_path(__FILE__) . 'includes/class-kamerequest-shortcodes.php';
require_once plugin_dir_path(__FILE__) . 'includes/class-kamerequest-excel.php';

// guarda los campos temporalmente para no hacer que el proveedor los escriba de nuevo
function kamerequest_externo_guardar_borrador($solicitud, $modo, $datos)
{
    $token = wp_generate_uuid4();
    set_transient('kr_borrador_' . $token, array(
        'solicitud' => $solicitud,
        'modo' => $modo,
        'datos' => $datos
    ), 15 * MINUTE_IN_SECONDS);
    return $token;
}

// recibe el formulario publico y envia la participacion al web service
function kamerequest_externo_guardar_participacion()
{
    // confirma que el formulario viene del mismo sitio wordpress
    if (!isset($_POST['kr_token']) || !wp_verify_nonce(sanitize_text_field(wp_unslash($_POST['kr_token'])), 'kr_participacion')) {
        wp_die('La solicitud no es valida.');
    }

    // al reenviar el formulario se quitan mensajes de intentos anteriores
    $url = remove_query_arg(array('kr_error', 'kr_exito', 'kr_previa', 'kr_borrador'), wp_get_referer() ?: home_url('/'));
    $token_previa = sanitize_text_field(wp_unslash($_POST['kr_previa_token'] ?? ''));
    $modo = sanitize_text_field(wp_unslash($_POST['kr_modo'] ?? 'manual'));
    $modo = in_array($modo, array('manual', 'excel'), true) ? $modo : 'manual';

    // si viene de excel se recupera la vista previa que se guardo temporalmente
    if ($token_previa !== '') {
        $datos_guardados = get_transient('kr_previa_' . $token_previa);
        if ($datos_guardados === false) {
            wp_safe_redirect(add_query_arg('kr_error', rawurlencode('La vista previa expiró. Cargue el Excel nuevamente.'), $url));
            exit;
        }
        $solicitud = $datos_guardados['solicitud'];
        $datos = $datos_guardados['datos'];
        $modo = $datos_guardados['modo'] ?? $modo;
    } else {
        // obtiene primero los datos del proveedor para poder conservarlos ante un error
        $solicitud = absint($_POST['kr_solicitud'] ?? 0);
        $datos = array(
            'proveedor' => array(
                'cedulaJuridica' => sanitize_text_field(wp_unslash($_POST['cedula'] ?? '')),
                'nombreProveedor' => sanitize_text_field(wp_unslash($_POST['nombre'] ?? '')),
                'nombreRepresentante' => sanitize_text_field(wp_unslash($_POST['representante'] ?? '')),
                'telefono' => sanitize_text_field(wp_unslash($_POST['telefono'] ?? '')),
                'correo' => sanitize_email(wp_unslash($_POST['correo'] ?? ''))
            ),
            'desgloses' => array()
        );

        if (!empty($_FILES['archivo_excel']['tmp_name'])) {
            $desgloses_excel = KameRequest_Externo_Excel::leer($_FILES['archivo_excel']);
            if (is_wp_error($desgloses_excel)) {
                $borrador = kamerequest_externo_guardar_borrador($solicitud, 'excel', $datos);
                wp_safe_redirect(add_query_arg(array(
                    'kr_error' => rawurlencode($desgloses_excel->get_error_message()),
                    'kr_borrador' => $borrador,
                    'kr_modo' => 'excel',
                    'kr_cedula' => $datos['proveedor']['cedulaJuridica']
                ), $url));
                exit;
            }

            $datos['desgloses'] = $desgloses_excel;
        }

        foreach ((array) ($_POST['desglose'] ?? array()) as $fila) {
            if (empty($fila['anio']) && empty($fila['mes']) && empty($fila['horas']) && empty($fila['monto'])) {
                continue;
            }
            $datos['desgloses'][] = array(
                'anio' => absint($fila['anio'] ?? 0),
                'mes' => absint($fila['mes'] ?? 0),
                'horas' => (float) ($fila['horas'] ?? 0),
                'monto' => (float) ($fila['monto'] ?? 0),
                'porcentajeCobro' => (float) ($fila['porcentaje'] ?? 0),
                'observaciones' => sanitize_text_field($fila['observaciones'] ?? '')
            );
        }

        // pasa a la carga de Excel sin perder los datos escritos del proveedor
        if (isset($_POST['kr_cargar_excel'])) {
            $borrador = kamerequest_externo_guardar_borrador($solicitud, 'excel', $datos);
            wp_safe_redirect(add_query_arg(array(
                'kr_borrador' => $borrador,
                'kr_modo' => 'excel',
                'kr_cedula' => $datos['proveedor']['cedulaJuridica']
            ), $url));
            exit;
        }

        // el primer envio de excel solo muestra las lineas antes de confirmarlas
        if (!empty($_FILES['archivo_excel']['tmp_name'])) {
            $token_previa = wp_generate_uuid4();
            set_transient('kr_previa_' . $token_previa, array('solicitud' => $solicitud, 'modo' => 'excel', 'datos' => $datos), 15 * MINUTE_IN_SECONDS);
            wp_safe_redirect(add_query_arg('kr_previa', $token_previa, $url));
            exit;
        }
    }

    // llama al web service para registrar la participacion
    $web_service = new KameRequest_Externo_WebService();
    $resultado = $web_service->registrar_participacion($solicitud, $datos);
    if (is_wp_error($resultado)) {
        $borrador = kamerequest_externo_guardar_borrador($solicitud, $modo, $datos);
        wp_safe_redirect(add_query_arg(array(
            'kr_error' => rawurlencode($resultado->get_error_message()),
            'kr_borrador' => $borrador,
            'kr_modo' => $modo,
            'kr_cedula' => $datos['proveedor']['cedulaJuridica']
        ), $url));
    } else {
        if ($token_previa !== '') {
            delete_transient('kr_previa_' . $token_previa);
        }
        wp_safe_redirect(add_query_arg('kr_exito', '1', $url));
    }
    exit;
}
add_action('admin_post_nopriv_kr_registrar_participacion', 'kamerequest_externo_guardar_participacion');
add_action('admin_post_kr_registrar_participacion', 'kamerequest_externo_guardar_participacion');

// registra la opcion donde se guarda la url del web service
function kamerequest_externo_registrar_configuracion()
{
    register_setting('kamerequest_externo', 'kamerequest_externo_ws_url', array(
        'sanitize_callback' => 'esc_url_raw'
    ));
}
add_action('admin_init', 'kamerequest_externo_registrar_configuracion');

// agrega la pantalla de configuracion dentro de ajustes de wordpress
function kamerequest_externo_menu()
{
    add_options_page(
        'KameRequest Externo',
        'KameRequest Externo',
        'manage_options',
        'kamerequest-externo',
        'kamerequest_externo_pantalla_configuracion'
    );
}
add_action('admin_menu', 'kamerequest_externo_menu');

// muestra el formulario para guardar la url del web service
function kamerequest_externo_pantalla_configuracion()
{
    ?>
    <div class="wrap">
        <h1>KameRequest Externo</h1>
        <form method="post" action="options.php">
            <?php settings_fields('kamerequest_externo'); ?>
            <table class="form-table">
                <tr>
                    <th scope="row"><label for="kamerequest_externo_ws_url">URL del Web Service</label></th>
                    <td>
                        <input class="regular-text" type="url" id="kamerequest_externo_ws_url"
                            name="kamerequest_externo_ws_url"
                            value="<?php echo esc_attr(get_option('kamerequest_externo_ws_url', '')); ?>"
                            placeholder="https://sitio.com/ServicioSolicitudesExternas.svc" required>
                        <p class="description">Ejemplo: https://dominio.com/ServicioSolicitudesExternas.svc</p>
                    </td>
                </tr>
            </table>
            <?php submit_button('Guardar configuración'); ?>
        </form>
    </div>
    <?php
}

// carga los estilos y comportamiento de las pantallas externas
function kamerequest_externo_estilos()
{
    wp_enqueue_style('kamerequest-externo', KAMEREQUEST_EXTERNO_URL . 'assets/kamerequest-externo.css', array(), '1.3.6');
    wp_enqueue_script('kamerequest-externo', KAMEREQUEST_EXTERNO_URL . 'assets/kamerequest-externo.js', array(), '1.3.6', true);
}
add_action('wp_enqueue_scripts', 'kamerequest_externo_estilos');

// registra los shortcodes que se colocan en las paginas de wordpress
new KameRequest_Externos_Shortcodes();

