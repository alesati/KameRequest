<!doctype html>
<html <?php language_attributes(); ?>>
<head>
    <meta charset="<?php bloginfo('charset'); ?>">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <?php wp_head(); ?>
</head>
<body <?php body_class(); ?>>
<?php wp_body_open(); ?>
<a class="kr-skip-link" href="#contenido">Ir al contenido</a>

<header class="kr-header">
    <div class="kr-barra">
        <div class="kr-marca">
            <?php
            if (has_custom_logo()) {
                the_custom_logo();
            }
            ?>
            <a class="kr-marca-texto" href="<?php echo esc_url(home_url('/')); ?>">Kame<span>Request</span></a>
        </div>

        <nav class="kr-navegacion" aria-label="Menú principal">
            <?php
            wp_nav_menu(array(
                'theme_location' => 'principal',
                'container' => false,
                'fallback_cb' => false
            ));
            ?>
        </nav>
    </div>
</header>

<main id="contenido" class="kr-contenido">
