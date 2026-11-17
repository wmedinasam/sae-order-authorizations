-- Solo dev: plantilla HTML de prueba del correo de autorización (MailBody).
-- Ejecutar a mano en la BD ORDENES_COMPRA. Sobrescribe la plantilla actual.
UPDATE WIS_CONFIGURATION
SET [VALUE] = N'<div style="font-family: Arial, sans-serif; color: #222222;">
  <h2 style="color: #1F3A5F;">Orden de compra {Estado de la Orden}</h2>
  <p>La Orden de compra con número <b>{Número de Orden de Compra}</b> de la empresa <b>{Empresa}</b> ha sido <b>{Estado de la Orden}</b> por {Usuario que Procesó}.</p>
  <table cellpadding="6" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #cccccc;">
    <tr><td><b>Proveedor</b></td><td>{Proveedor}</td></tr>
    <tr><td><b>Total</b></td><td>{Total de la Orden}</td></tr>
    <tr><td><b>Fecha de la orden</b></td><td>{Fecha de la Orden}</td></tr>
    <tr><td><b>Fecha de resolución</b></td><td>{Fecha de Resolución}</td></tr>
    <tr><td><b>Observación</b></td><td>{Observación}</td></tr>
  </table>
</div>'
WHERE [KEY] = 'MailBody';
