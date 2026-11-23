# Hotel – Miniproyecto 2 (Lenguajes de Programación, USC)

Sistema de gestión de un hotel o hostal pequeño: habitaciones, huéspedes, reservas,
check-in / check-out y reportes. Incluye una **aplicación web (ASP.NET Core MVC)** y una
**aplicación de escritorio (WinForms)** que comparten la misma lógica y la misma base de datos.

## Arquitectura

| Proyecto | Tipo | Responsabilidad |
|---|---|---|
| `Core` | Biblioteca de clases | Entidades, reglas de negocio, interfaces, tarifas, servicios |
| `Data` | Biblioteca de clases | Entity Framework Core (SQLite), Identity, repositorios, datos iniciales |
| `Web` | ASP.NET Core MVC | Controladores, vistas Bootstrap (responsive), login con Identity |
| `Desktop` | WinForms | Aplicación de escritorio para el personal del hotel |

## Roles (ASP.NET Core Identity)

| Rol | Web | Escritorio |
|---|---|---|
| Admin | Habitaciones (crear/editar), reservas, reportes | Todo, incluido mantenimiento de habitaciones |
| Recepcionista | Ver habitaciones, gestionar reservas (check-in/out, cancelar) | Reservas y nuevas reservas |
| Huésped | Registrarse, buscar disponibilidad, reservar y cancelar sus reservas | No aplica |

Usuarios de prueba (se crean automáticamente):

- `admin@hotel.com` / `Admin123*`
- `recepcion@hotel.com` / `Recepcion123*`
- Los huéspedes se registran desde la web.

## Requisitos

- SDK de .NET (ver `Directory.Build.props` para ajustar la versión)
- Windows para la aplicación de escritorio (WinForms)
- Git

## Cómo ejecutarlo

```bash
dotnet build

# (una sola vez) crear la migración inicial
dotnet tool install --global dotnet-ef
dotnet ef migrations add Inicial --project Data --startup-project Web

# aplicación web
dotnet run --project Web          # http://localhost:5080

# aplicación de escritorio
dotnet run --project Desktop
```

La base de datos SQLite se crea sola en `%LOCALAPPDATA%\HotelApp\hotel.db` y es compartida
por la web y el escritorio.

## Características de C# y .NET utilizadas

Clases abstractas y herencia, interfaces, polimorfismo (tarifas), genéricos (repositorio),
colecciones (`List`, `Dictionary`, `HashSet`, `SortedDictionary`), LINQ, delegados y expresiones
lambda, eventos, `async/await`, excepciones propias, métodos de extensión, inyección de
dependencias, Entity Framework Core, ASP.NET Core Identity y patrón MVC.

## Estructura MVC y VS Code

La web sigue la estructura de `dotnet new mvc`: `Controllers/`, `Models/`, `Views/` (con `Shared/_Layout`,
`_ValidationScriptsPartial` y `Error`), `wwwroot/` (`css`, `js`), `Program.cs` y `appsettings*.json`.
Para VS Code: abrir la carpeta de la solución, usar `Terminal > Run Task` (build, web: run, web: watch,
desktop: run) o la depuración F5 (`.vscode/launch.json`). Bootstrap, la tipografía y jQuery se cargan por CDN
(se necesita internet).

## Diseño

Identidad compartida entre web y escritorio: pared encalada, azul puerta, latón y verde guadua. En la web las
habitaciones se muestran como llaves de recepción y las tablas se apilan en pantallas pequeñas.
