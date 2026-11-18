<?php

// prepara las opciones basicas del tema de wordpress
function kamerequest_wordpress_configuracion()
{
    add_theme_support('title-tag');
    add_theme_support('custom-logo', array(
        'height' => 120,
        'width' => 120,
        'flex-height' => true,
        'flex-width' => true
    ));
    add_theme_support('post-thumbnails');

    register_nav_menus(array(
        'principal' => 'Menú principal'
    ));
}
add_action('after_setup_theme', 'kamerequest_wordpress_configuracion');

// carga los estilos que siguen la paleta de KameRequest
function kamerequest_wordpress_estilos()
{
    wp_enqueue_style(
        'kamerequest-wordpress',
        get_stylesheet_uri(),
        array(),
        '1.0.0'
    );
}
add_action('wp_enqueue_scripts', 'kamerequest_wordpress_estilos');
