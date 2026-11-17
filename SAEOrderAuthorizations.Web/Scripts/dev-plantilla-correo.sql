-- Solo dev: plantilla de prueba del correo de autorización (MailBody).
-- Ejecutar a mano en la BD ORDENES_COMPRA. Sobrescribe la plantilla actual.
UPDATE WIS_CONFIGURATION
SET [VALUE] = N'La Orden de compra con número: {OrdenCompra.IdOrder} de la empresa {Empresa.Nombre} ha sido {OrdenCompra.Status} por {Usuario.Nombre} observacion: {BitacoraEntrada.Descr}'
WHERE [KEY] = 'MailBody';
