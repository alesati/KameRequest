// mantiene los indices y deja como minimo un periodo en la propuesta
function actualizarPeriodos(contenedor) {
    var lineas = contenedor.querySelectorAll('[data-kr-linea]');
    lineas.forEach(function (linea, indice) {
        linea.querySelectorAll('[name]').forEach(function (campo) {
            campo.name = campo.name.replace(/desglose\[\d+\]/, 'desglose[' + indice + ']');
        });

    });
}

// muestra una validacion corta sin borrar la informacion que el proveedor ya escribio
function mostrarAvisoPeriodo(contenedor, mostrar) {
    var formulario = contenedor.closest('[data-kr-formulario]');
    var aviso = formulario ? formulario.querySelector('[data-kr-aviso-periodo]') : null;
    if (aviso) {
        aviso.hidden = !mostrar;
    }
}

// agrega o elimina lineas de propuesta y calcula los montos visibles para el proveedor
document.addEventListener('click', function (evento) {
    var agregar = evento.target.closest('[data-kr-agregar-detalle]');
    var eliminar = evento.target.closest('[data-kr-eliminar-detalle]');
    var formulario = document.querySelector('[data-kr-formulario]');

    if (agregar && formulario) {
        var contenedor = formulario.querySelector('[data-kr-lineas-propuesta]');
        var original = contenedor.querySelector('[data-kr-linea]');
        var nueva = original.cloneNode(true);
        nueva.querySelectorAll('input, select').forEach(function (campo) {
            campo.value = '';
        });
        contenedor.appendChild(nueva);
        actualizarTotales(nueva);
        actualizarPeriodos(contenedor);
        mostrarAvisoPeriodo(contenedor, false);
    }

    if (eliminar && formulario) {
        var linea = eliminar.closest('[data-kr-linea]');
        var contenedorEliminar = linea ? linea.parentElement : null;
        if (contenedorEliminar && contenedorEliminar.querySelectorAll('[data-kr-linea]').length > 1) {
            linea.remove();
            actualizarPeriodos(contenedorEliminar);
            mostrarAvisoPeriodo(contenedorEliminar, false);
        } else if (contenedorEliminar) {
            mostrarAvisoPeriodo(contenedorEliminar, true);
        }
    }
});

// el IVA y total se muestran automaticamente pero el WS vuelve a validar los datos
function actualizarTotales(linea) {
    var monto = parseFloat(linea.querySelector('[data-kr-monto]').value) || 0;
    var iva = Math.round(monto * 0.13 * 100) / 100;
    linea.querySelector('[data-kr-iva]').value = iva.toFixed(2);
    linea.querySelector('[data-kr-total]').value = (monto + iva).toFixed(2);
}

document.addEventListener('input', function (evento) {
    if (evento.target.matches('[data-kr-monto]')) {
        actualizarTotales(evento.target.closest('[data-kr-linea]'));
    }
});

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-kr-lineas-propuesta]').forEach(function (contenedor) {
        actualizarPeriodos(contenedor);
        contenedor.querySelectorAll('[data-kr-linea]').forEach(actualizarTotales);
    });
});
