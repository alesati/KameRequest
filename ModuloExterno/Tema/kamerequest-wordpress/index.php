<?php get_header(); ?>

<?php if (have_posts()) { ?>
    <?php while (have_posts()) { the_post(); ?>
        <article <?php post_class('kr-articulo'); ?>>
            <h1 class="kr-titulo-pagina"><a href="<?php the_permalink(); ?>"><?php the_title(); ?></a></h1>
            <div class="entry-content">
                <?php the_excerpt(); ?>
            </div>
        </article>
    <?php } ?>
<?php } else { ?>
    <article class="kr-articulo">
        <h1 class="kr-titulo-pagina">KameRequest</h1>
        <p>No hay contenido para mostrar.</p>
    </article>
<?php } ?>

<?php get_footer(); ?>
