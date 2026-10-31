# Checklist · módulos de administración

Reglas: solo create y update; soft delete (`Estado = false`), nunca `DELETE`. Solo el hilo principal marca este archivo.

## Fase 0 · Fundación
- [x] F0.1 Migración `Estado` en `WIS_USER` y `WIS_CONFIGURATION` aplicada y revisada
- [x] F0.2 Login rechaza usuarios inactivos
- [x] F0.3 Registro automático de servicios en `Program.cs`
- [x] F0.4 `AuthExtensions` + `WisModal` compartido (Órdenes usa `WisModal`)
- [x] F0.5 Build limpio + commit base (`74da4ec`)

## Fase 1 · Módulos (subagentes en worktrees)
- [ ] M1 Empresas: listar, crear, editar, desactivar/reactivar, validaciones, build, prueba
- [ ] M2 Roles: listar, crear, editar, asignar empresas, desactivar/reactivar, salvaguardas, build, prueba
- [ ] M3 Usuarios: listar, crear, editar, un solo rol, desactivar/reactivar, salvaguardas, build, prueba
- [ ] M4 Configuración: listar, crear, editar, claves base protegidas, desactivar/reactivar, build, prueba

## Fase 2 · Integración
- [ ] I1 Ramas revisadas y fusionadas
- [ ] I2 Build integrado + recorrido en navegador (admin) de los 4 módulos
- [ ] I3 Accesos denegados para operador/aprobador; pruebas cruzadas (usuario/rol/empresa inactivos)
- [ ] I4 `CLAUDE.md` actualizado
