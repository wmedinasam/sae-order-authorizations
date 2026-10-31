# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> Idioma: este archivo y la comunicación con el usuario van en **español**.

## Contexto de la tarea (leer primero)

Este repo tiene dos partes:
- **Proyecto principal (nuevo):** `SAEOrderAuthorizations.Web/`, Blazor Web App (Interactive Server) en **.NET 10** con EF Core + SQL Server, dentro de `SAEOrderAuthorizations.Web.slnx` (raíz). Reemplaza a la app WPF.
- **Legacy (solo backup/referencia funcional):** `ORD_SAE/` y `OrdenesSAE/OrdenesSAE.sln`, WPF / .NET Framework 4.0 (autorización de órdenes de compra sobre Aspel SAE). **No modificarlos**; leerlos para extraer reglas de negocio. WPF no compila en Linux.

Reglas:
- Todo el SQL del proyecto nuevo va con **EF Core y migraciones**; nada de SQL armado con strings.
- La BD `ORDENES_COMPRA` **ya existe**: nunca sobrescribirla ni recrearla. `Program.cs` ejecuta `Database.MigrateAsync()` (crea la BD solo si no existe y aplica solo migraciones pendientes). Las migraciones sobre tablas legacy existentes deben ir con guardas (`IF OBJECT_ID(...) IS NULL`) o baseline manual en `__EFMigrationsHistory`; sin DDL destructivo.
- Cadena de conexión `ConnectionStrings:Default`: `appsettings.json` lleva servidor/BD/usuario **sin contraseña**; la contraseña va en **User Secrets** (dev) o en la variable `ConnectionStrings__Default` (prod).
- No hacer commits salvo que el usuario lo pida. Rama de trabajo: `1-upgrade-to-web`.

## Comandos

```bash
dotnet build SAEOrderAuthorizations.Web.slnx
dotnet run --project SAEOrderAuthorizations.Web
dotnet ef migrations add <Nombre> --project SAEOrderAuthorizations.Web
dotnet ef database update --project SAEOrderAuthorizations.Web
dotnet test SAEOrderAuthorizations.Web.slnx --filter <Nombre>   # aún no hay proyecto de tests
```

Legacy: solo Windows + Visual Studio; sin tests, linter ni CI. Depende de `Newtonsoft.Json 12.0.3` vía `..\packages\` (no versionada).

## Estado del proyecto nuevo

`Data/AppDbContext.cs` mapea las 7 tablas legacy de `ORDENES_COMPRA` (`EMPRESA`, `ROL`, `ROL_EMPRESA`, `WIS_USER`, `WIS_CONFIGURATION`, `WIS_Orders`, `WIS_WorkLog`) con entidades en `Data/Entities/`. La migración `InicialLegacy` crea esas tablas y `AgregarSemillaLegacy` inserta la semilla del script maestro (rol Admin, usuario `administrador` y 4 claves de correo vacías). `SemillaEmpresa` agrega la empresa 01 (`DB_SAE` = `SAE80Empre01`, sufijo `01`) y la asocia al rol Admin en `ROL_EMPRESA`. Ambas se aplicaron sobre una BD vacía. Pendiente: vistas y SPs legacy (`WIS_viewLogView`, `Sp_GetOrders`, `Sp_OrderHeader`, `Sp_OrderDetail`), autenticación (la contraseña semilla es el cifrado AES legacy, no un hash), páginas y correo.
- El script maestro es la fuente de verdad del esquema; los `SCRIPTS/TABLE/*.sql` apuntan a `SAE70Empre01` y no coinciden (p. ej. `WIS_USER` usa `ROL` ahí, `ROL_ID` en el maestro).
- Una BD legacy con tablas ya existentes no puede correr `InicialLegacy` tal cual: fallaría al crear tablas. Antes de aplicarla ahí, marcarla como aplicada en `__EFMigrationsHistory` o añadir guardas.

## Arquitectura del legacy (lo que hay que portar)

Capas en `ORD_SAE/` (namespace raíz `ORD_SAE`):

- `VIEW/` — ventanas WPF (XAML + code-behind): `InicioSesion`, `LoginAccess`, `MenuPrincipal`, `Autorizaciones`, `OrdenDetalle`, `CancelReason`, `Bitacora`, `Usuarios`, `Roles`, `Empresas`, `RolEmpresa`, `Configuracion`. `MainWindow` está vacío.
- `MODEL/` — clases que **mezclan entidad + acceso a datos** (p. ej. `UserModel.GetUser()`, `ConfigurationModel.GetConfiguration()`), devuelven `ResponseModel<T>` (`Success`, `Message`, `Object`).
- `SERVICE/DataAction.cs` — todas las consultas/comandos SQL como strings (ROL, usuarios, empresas, órdenes, bitácora). Las órdenes se autorizan/rechazan con `UPDATE WIS_Orders` + `UPDATE <BD_SAE>.dbo.COMPO<sufijo>` (`BLOQ`, `STATUS`).
- `DB/DataBaseAction.cs` + `DAO/` — ejecución ADO.NET (`SqlConnection`, `DataSet`) y soporte para SPs (`DaoProcedure`, `DaoParameter`).
- `SERVICE/MailService.cs` — notificaciones por `System.Net.Mail`; destinatarios = usuarios con `SendMail` que no son administradores ni rol `OC`/`CONTA`; asunto/cuerpo/credenciales vienen de la tabla de configuración.
- `SERVICE/Seguridad.cs` + `HandleCfg.cs` — cifrado AES (extensiones `EncriptarDatos()` / `DesencriptarDatos()`) y lectura/escritura del archivo de configuración.
- `SCRIPTS/` — DDL de la BD propia `ORDENES_COMPRA` (`Script Master MultiEmpresas.sql` crea todo), tablas `WIS_*`, vistas y SPs `WIS_OrderHeader` / `WIS_OrderDetail`.

### Modelo multiempresa
Una BD propia `ORDENES_COMPRA` (tablas `EMPRESA`, `ROL`, `ROL_EMPRESA`, `WIS_USER`, `WIS_Orders`, `WIS_WorkLog`, configuración) + **una BD de Aspel SAE por empresa** (`EMPRESA.DB_SAE`, `EMPRESA.Sufijo`). Las tablas SAE se arman dinámicamente (`{db}.dbo.COMPO{sufijo}`). Roles se asocian a empresas vía `ROL_EMPRESA`.

### Configuración
`Configuration.cfg` en el directorio actual, JSON cifrado con AES (usuario/clave SQL, instancia, `BdSae`). `DaoConnection` lo lee en **cada** conexión y arma el connection string a `ORDENES_COMPRA`.

## Deuda que NO se debe replicar en el port

- SQL por interpolación de strings en `DataAction` → inyección SQL. Usar EF Core o consultas parametrizadas.
- Clave e IV AES hardcodeados en `Seguridad.cs`; contraseñas en `WIS_USER.PASSWORD` (varchar(50), probablemente reversibles). Usar secretos/User Secrets y hash de contraseñas (Identity).
- Credenciales de BD y de correo en un archivo local cifrado → mover a configuración de .NET (`appsettings` + secretos).
- Lógica de datos dentro de los `MODEL` y code-behind → separar en servicios/componentes Blazor.
- Nombres con typos heredados (`PurshaseOrderModel`, `SystemConfugurationModel`): no copiarlos.

## Autenticación (proyecto nuevo)

- Cookie auth (`AddCookie`, `LoginPath = /login`) con `FallbackPolicy` que exige sesión: todo endpoint requiere login salvo `[AllowAnonymous]`.
- `Components/Pages/Login.razor` valida contra `Usuarios` + `Roles` (rol con `Estado = true`) y firma la cookie. Login es SSR estático (`ExcludeFromInteractiveRouting`) porque `SignInAsync` necesita `HttpContext`.
- `POST /auth/logout` cierra la sesión. El dashboard es `Components/Pages/Home.razor`.
- **Deuda temporal:** la contraseña se compara en claro (`WIS_USER.PASSWORD`). Pendiente: hash. El admin semilla usa `123` (migración `SeedAdminPasswordClaro`).
