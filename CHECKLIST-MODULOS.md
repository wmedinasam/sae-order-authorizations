# Checklist · módulos de administración

Reglas: solo create y update; soft delete (`Estado = false`), nunca `DELETE`. Solo el hilo principal marca este archivo.

## Fase 0 · Fundación
- [x] F0.1 Migración `Estado` en `WIS_USER` y `WIS_CONFIGURATION` aplicada y revisada
- [x] F0.2 Login rechaza usuarios inactivos
- [x] F0.3 Registro automático de servicios en `Program.cs`
- [x] F0.4 `AuthExtensions` + `WisModal` compartido (Órdenes usa `WisModal`)
- [x] F0.5 Build limpio + commit base (`74da4ec`)

## Fase 1 · Módulos (subagentes en worktrees)
- [x] M1 Empresas: implementado y fusionado (`d11b400`). Probado: alta, duplicado, desactivar. Pendiente: editar, reactivar
- [x] M2 Roles: implementado y fusionado (`44b615e`). Probado: alta con empresa, salvaguarda de Admin/Aprobador, desactivar. Pendiente: último admin, nombre duplicado
- [x] M3 Usuarios: implementado y fusionado (`b6643df`). Probado: lista y permisos. Pendiente: crear, editar, desactivar, salvaguardas
- [x] M4 Configuración: implementado y fusionado (`2652e5e` + fix `76e1c86`). Probado: permisos. Pendiente: crear, editar, claves base

## Fase 2 · Integración
- [x] I1 Ramas revisadas y fusionadas (sin conflictos, sin archivos compartidos)
- [ ] I2 Build integrado + recorrido en navegador (admin) de los 4 módulos
- [ ] I3 Accesos denegados para operador/aprobador; pruebas cruzadas (usuario/rol/empresa inactivos)
- [ ] I4 `CLAUDE.md` actualizado
