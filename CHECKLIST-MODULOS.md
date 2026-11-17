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
- [x] I2 Build integrado + recorrido en navegador (admin) de los 4 módulos (sin errores de consola)
- [ ] I3 Accesos denegados para operador/aprobador; pruebas cruzadas (usuario/rol/empresa inactivos). Hecho: accesos denegados (operador y aprobador, carga directa y navegación en circuito), usuario inactivo (cookie rechazada, login rechazado, reactivado), empresa inactiva (aprobador deja de verla, reactivada). Pendiente: rol inactivo con usuarios activos y revalidación de circuito (1 min)
- [x] I4 `CLAUDE.md` actualizado (estado de autenticación/páginas, migración `Estado`)

## Fase 3 · Pulido de UI
- [x] U1 Componente `IconButton` + tokens de color y tooltip
- [x] U2 Íconos en Empresas, Roles, Usuarios y Configuración (Nuevo, Editar, Inactivar/Reactivar, Guardar)
- [x] U3 Órdenes: Consultar con ícono; Autorizar y Rechazar con ícono + texto + color (código revisado; visual pendiente de datos de prueba)
- [x] U4 Cerrar del `WisModal` con ícono (Escape cierra; foco inicial en el diálogo)
- [x] U5 Logo responsivo en el sidebar (no excede el ancho del panel, también en móvil)
- [ ] U6 Verificación visual: escritorio y móvil, tooltips, teclado. Hecho: móvil 390px sin scroll horizontal en Inicio, Órdenes, Empresas, Roles, Usuarios y Configuración; tooltips y aria-label en los íconos; Escape cierra el modal de Empresas. Pendiente: Autorizar/Rechazar en modal (requiere órdenes pendientes con `dev-ordenes-prueba.sql`); foco no vuelve al disparador en modales admin; foco inicial en Empresa y Usuario no aplicado (InputText); tabs de OrdenModal sin aria-selected ni flechas

Datos de prueba conservados en la BD (inactivos, no se borran):
- Empresa `TEST-M1-Empresa` (base `TEST_M1_BASE`, sufijo `TM1`)
- Rol `TEST-M2-rol` (con su fila en `ROL_EMPRESA`)
- Usuario `test-u1` (nombre `TEST-U1 Editado`, rol Operador)
- Clave de configuración `TEST-K1`

Estado de Fase 3 (aplicado en esta ronda, verificado en navegador en escritorio y móvil): U1 a U5 completos. U6 parcial: tooltips y colores verificados en Empresas; Órdenes (Autorizar y Rechazar) y el resto de módulos quedan por revisar visualmente.
