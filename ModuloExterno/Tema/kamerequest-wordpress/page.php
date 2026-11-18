<?php get_header(); ?>

<?php while (have_posts()) { the_post(); ?>
    <article <?php post_class('kr-articulo'); ?>>
        <h1 class="kr-titulo-pagina"><?php the_title(); ?></h1>
        <div class="entry-content">
            <?php the_content(); ?>
        </div>
    </article>
<?php } ?>

<?php get_footer(); ?>
