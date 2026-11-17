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
- [ ] M1 Backend: `AppDbContext` + migración `AgregarMarca`, cuerpo de `MarcaService` (firma por bytes, límites, soft delete, caché), endpoint `GET /branding/{slot}` con `AllowAnonymous`, `AddMemoryCache`
- [ ] M2 Consumo: `App.razor` (favicon dinámico), `AuthLayout.razor` y `NavMenu.razor` (logo y alt), `Components/Shared/TituloPagina.razor` y las 8 páginas con `PageTitle`, `wwwroot/app.css` (tamaños libres)
- [ ] M3 UI: `Components/Configuracion/MarcaSeccion.razor` (vista previa, InputFile, Guardar/Restaurar con IconButton, nombre de empresa aviso de fondo oscuro) y enganche en `Configuracion.razor`

## Fase 2 · Integración (hilo principal)
- [ ] I1 Merge de M1, M2 y M3 en `dev_marca` sin conflictos
- [ ] I2 Build limpio (0 errores)
- [ ] I3 Migración `AgregarMarca` aplicada sobre la BD de dev
- [ ] I4 Pruebas de navegador (plan, verificación 1 a 10)
- [ ] I5 `CLAUDE.md` actualizado (sección Marca: deja de ser fija)
- [ ] I6 PR a `main` y merge

## Pendientes conocidos (fuera de alcance)
- Sin proyecto de tests: la validación por firma se prueba a mano.
- `brand/web/manifest.webmanifest` y `favicon-head.html` no se enlazan.
