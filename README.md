# KameRequest

Proyecto de administracion de solicitudes hecho con .NET 10, Razor Pages, MySQL y Dapper.

## Preparar el proyecto

1. Clonar el repositorio y cambiar a develop.

```bash
git clone https://github.com/alesati/KameRequest.git
cd KameRequest
git checkout develop
git pull origin develop
```

2. Abrir MySQL Workbench y ejecutar el archivo `Scripts/BaseDatos_KameRequest.sql`.

3. Crear un archivo local llamado `appsettings.json` en la carpeta principal. Este archivo no se sube a Git.

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Port=3306;Database=kamerequest;User ID=root;Password=SU_CLAVE;SslMode=None"
  },
  "Seguridad": {
    "ClaveAes": "CLAVE_BASE64_COMPARTIDA_POR_EL_EQUIPO"
  },
  "UsuarioInicial": {
    "Nombre": "Administrador",
    "Usuario": "admin",
    "Correo": "admin@kamerequest.local",
    "Contrasena": "Admin123*"
  }
}
```

La clave AES debe ser la misma si el equipo utiliza una misma base de datos. No se deben subir claves ni contraseñas al repositorio.

4. Ejecutar el proyecto.

```bash
dotnet restore
dotnet run
```

## Trabajar un modulo

Cada persona parte de `develop` y crea su propia rama.

```bash
git checkout develop
git pull origin develop
git checkout -b feature/nombre-del-modulo
```

Cada modulo debe agregar solo sus propias paginas, modelos, servicios y cambios de base de datos. Si tiene operaciones de crear, editar o eliminar, debe usar `BitacoraService` para registrar la accion.

Antes de entregar una rama se debe compilar y probar.

```bash
dotnet build
git add .
git commit -m "agrego modulo de nombre"
git push -u origin feature/nombre-del-modulo
```

Al terminar, la persona encargada de integrar une la rama en `develop`. `main` se usa solamente para la entrega final aprobada.
