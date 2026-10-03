# Integración de Bitácora

Esta guía explica lo que ya aporta la historia BIT y lo que debe hacer cada responsable al integrar su módulo.

## Lo que BIT ya deja resuelto

- Tabla `Bitacoras` en `Scripts/BaseDatos_KameRequest.sql` y en el script incremental `Scripts/02_bitacoras.sql`.
- Modelo `Bitacora` y pantalla `/Bitacora/Index` con paginación de diez registros, orden descendente, fecha, usuario y descripción.
- `BitacoraService`, registrado en `Program.cs` mediante inyección de dependencias.
- Serialización JSON para Create, Update y Delete.
- `RegistrarConsulta` para que los Read sigan exactamente el formato solicitado.
- Middleware global que intenta registrar errores técnicos no controlados.
- Auditoría integrada en Usuarios, sin enviar nunca la contraseña al servicio.

Por tanto, **no se debe crear otra tabla, otro servicio ni volver a registrar `BitacoraService`** en `Program.cs`.

## Paso obligatorio en cada PageModel

Agregar `BitacoraService` al constructor primario de la página:

```csharp
using KameRequest.Services;

public class IndexModel(
    IConfiguration configuracion,
    BitacoraService bitacora) : PageModel
{
}
```

Si la página ya tiene otros servicios, simplemente se agrega `BitacoraService bitacora` como otro parámetro. ASP.NET lo resuelve automáticamente.

## Llamadas que se deben usar

### Read

Al cargar un listado, detalle o reporte, usar únicamente:

```csharp
bitacora.RegistrarConsulta(User, "solicitudes");
```

No agregar JSON ni datos de filtros en una consulta. Ejemplos de elemento: `estados de solicitudes`, `representantes`, `reporte de tareas por usuario`.

### Create

Después de guardar exitosamente:

```csharp
bitacora.RegistrarCreacion(User, "representante", Representante);
```

El tercer parámetro debe ser el registro nuevo que quedó guardado, con su id real si aplica.

### Update

Consultar y guardar la versión anterior **antes** del `UPDATE`; luego, después de actualizar, registrar ambas:

```csharp
var anterior = ObtenerRegistro(id);

// UPDATE exitoso

var actual = ObtenerRegistro(id);
bitacora.RegistrarActualizacion(User, "solicitud", anterior, actual!);
```

### Delete

Consultar el registro antes de eliminarlo; solo registrar si el `DELETE` fue exitoso:

```csharp
var eliminado = ObtenerRegistro(id);

// DELETE exitoso

bitacora.RegistrarEliminacion(User, "tarea", eliminado);
```

### Errores técnicos controlados

El middleware cubre excepciones no controladas. Si un bloque `try/catch` consume una excepción para mostrar un mensaje o continuar el proceso, se debe registrar explícitamente:

```csharp
catch (Exception excepcion)
{
    bitacora.RegistrarErrorTecnico(User, "envio de correo", excepcion);
    // Manejo propio de la pantalla.
}
```

Para procesos automáticos sin usuario autenticado, como SOL5, usar:

```csharp
bitacora.RegistrarSistema("La solicitud 25 fue marcada como vencida.");
bitacora.RegistrarErrorTecnicoSistema("proceso de vencimiento", excepcion);
```

## Regla crítica de Usuarios

Nunca enviar a bitácora un objeto que incluya `Contrasena`, `ContrasenaCifrada`, `Nonce` o `EtiquetaAutenticacion`.

Para usuarios se debe construir un objeto seguro, como ya se hace en `Pages/Usuarios`:

```csharp
new
{
    usuario.IdUsuario,
    usuario.NombreCompleto,
    usuario.NombreUsuario,
    usuario.Correo,
    usuario.TipoUsuario,
    usuario.Estado
}
```

## Matriz por historia

| Responsable | Historia | Llamadas necesarias |
| --- | --- | --- |
| Diego | SOL1 | Read + Create/Update/Delete de solicitudes. |
| Diego | SOL3 | Read + Create/Update/Delete de representantes. |
| Diego | SOL5 | `RegistrarSistema` al vencer y `RegistrarErrorTecnicoSistema` si falla. |
| Alejandro | USR1–USR4 | USR4 ya está integrado; mantener la exclusión de contraseña. |
| Steve | SOL4 | Read + Create/Update/Delete de estados. |
| Steve | SOL6 | Registrar el Update de solicitud; registrar errores técnicos del correo si se capturan. |
| Steve | SOL7 | `RegistrarConsulta` al generar el reporte. |
| Luis | SOL2 | Read + Create/Update/Delete de desgloses. |
| Luis | TAR1 | Read + Create/Update/Delete de tareas. |
| Luis | TAR2/TAR3 | `RegistrarConsulta` al generar cada reporte. |

## Verificación antes de fusionar

1. Confirmar que cada PageModel de CRUD recibe `BitacoraService`.
2. Confirmar que Create/Update/Delete registran únicamente después de éxito.
3. Confirmar que Update conserva el objeto anterior antes del cambio.
4. Confirmar que Read usa `RegistrarConsulta` y no JSON.
5. Confirmar que no se registra ninguna contraseña o dato criptográfico.
6. Ejecutar `dotnet build` y probar la pantalla `/Bitacora/Index`.
