# Certificados

**Certificados digitales** de la instalación: clase de certificado, el fichero del certificado y su **contraseña**, y el permiso de gestor que
da acceso a usarlo. Se usan para **firmar** (facturas emitidas, pedidos, documentos) y para **comunicarse con la AEAT** (Verifactu / SII).
Los certificados de una sociedad se asocian desde *Gestión de sociedades* → *Asociar certificado*; cada usuario puede subir el suyo con
*Añadir mi certificado*.

## ¿Cómo hago...?

### ...para subir un certificado?

**Crear**: clase, fichero (.pfx / .p12) y contraseña. Después asócielo a la sociedad.

### ...para que otro usuario pueda firmar con él?

Dele el permiso de gestor del certificado (*P. de Gestor*) en un rol o en su puesto.

---

#### Detalle técnico

- Controlador: `CertificadosController` (fichero `Controllers/Entorno/CertificadoController.cs`). Vista:
  `Views/Entorno/CrudCertificados.cshtml`. Dto: `CertificadoDto`. Descriptor: `DescriptorDeCertificados`.
