# Checklist · logo, favicon y nombre de empresa personalizables

Rama de trabajo: `dev_marca` (desde `main`). Los agentes parten de ella en worktrees.
Reglas: solo el hilo principal marca este archivo. Sin `DELETE` (soft delete, `Estado = false`). Sin commits salvo que se pida en la integración.

Plan completo: `/home/wally/.claude/plans/lively-giggling-wave.md`

## Fase 0 · Contrato (hilo principal, secuencial)
- [x] F0.1 Rama `dev_marca` desde `main`
- [x] F0.2 Entidad `Data/Entities/Marca.cs` (`WIS_BRANDING`: Slot, Contenido, TipoMime, NombreArchivo, Actualizado, Estado)
- [x] F0.3 `Services/MarcaService.cs`: `MarcaInfo` y firmas de `ObtenerAsync`, `GuardarAsync`, `RestaurarAsync`, `GuardarNombreAsync` (cuerpos `NotImplementedException`)
- [x] F0.4 Build limpio con el contrato (0 errores)

## Fase 1 · En paralelo (agentes en worktrees, archivos disjuntos)
- [x] M1 Backend (commit `7dc7845` en `marca-m1`; migración aplicada en dev; ver I4 para lo verificado): `AppDbContext` + migración `AgregarMarca`, cuerpo de `MarcaService` (firma por bytes, límites, soft delete, caché), endpoint `GET /branding/{slot}` con `AllowAnonymous`, `AddMemoryCache`
- [x] M2 Consumo (commit `196978f` en `marca-m2`; build 0 errores; sin verificar en ejecución: depende de M1 porque `ObtenerAsync` aún lanza NotImplementedException): `App.razor` (favicon dinámico), `AuthLayout.razor` y `NavMenu.razor` (logo y alt), `Components/Shared/TituloPagina.razor` y 7 páginas con `PageTitle` (sin `Configuracion.razor`), `wwwroot/app.css` (tamaños libres)
- [x] M3 UI (commit `04b696c` en `marca-m3`; build 0 errores; sin verificar en ejecución. Pendiente en integración: clase CSS `wis-marca-preview` en lugar del `style` inline de la vista previa; no hay variante de restaurar, se usa `reactivar`): `Components/Configuracion/MarcaSeccion.razor` (vista previa, InputFile, Guardar/Restaurar con IconButton, nombre de empresa, aviso de fondo oscuro) y enganche en `Configuracion.razor`. Dueño de `Configuracion.razor` (también su `PageTitle`, que se unifica con `TituloPagina` en la integración)

## Fase 2 · Integración (hilo principal)
- [x] I1 Merge de M1, M2 y M3 en `dev_marca` sin conflictos
- [x] I2 Build limpio (0 errores, 8 warnings previos)
- [x] I3 Migración `AgregarMarca` aplicada sobre la BD de dev (`WIS_BRANDING` creada)
- [ ] I4 Pruebas de navegador (plan, verificación 1 a 10). Hecho: 2 (subida logo sidebar), 3 (`/branding/LogoSidebar` anónimo 200; login con fallback), 4 (favicon PNG, tipo dinámico), 5 (nombre de empresa en título y alt), 6 (restaurar: 404 y vuelve el logo WIS), 7 (SVG, falso PNG y >512 KB rechazados con mensaje), 8 (`operador` denegado en Configuración; `nosniff` presente), 9 (390px: sin scroll, logo dentro del panel abierto), 10 (BD devuelta a defaults). Pendiente: subida de logo de login con imagen real visible en `/login` (solo se probó el fallback)
- [x] I4a Correcciones de integración: `App`, `AuthLayout` y `NavMenu` con `_marca` anulable (NRE en el render del `<head>`); `/branding` excluido de `UseStatusCodePagesWithReExecute` (404 real en vez de 302); clase `wis-marca-preview` en lugar de estilo inline; `Configuracion.razor` con `TituloPagina`
- [x] I5 `CLAUDE.md` actualizado (sección Marca) (sección Marca: deja de ser fija)
- [ ] I6 PR a `main` y merge

## Pendientes conocidos (fuera de alcance)
- Sin proyecto de tests: la validación por firma se prueba a mano.
- `brand/web/manifest.webmanifest` y `favicon-head.html` no se enlazan.
