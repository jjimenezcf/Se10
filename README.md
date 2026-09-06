# Sistema de Elementos (SE10)

Aplicación web ASP.NET Core 10 de gestión empresarial modular. Incluye funcionalidades de contabilidad, expedientes, facturación, guarderías, gestión documental, firma digital XAdES, OCR, envío/lectura de correo y cola de trabajos en background.

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB o instancia completa)
- [Node.js](https://nodejs.org/) (para la compilación de TypeScript)
- JDK 17+ (solo si se usa el componente de firma XAdES; ver `README_FIRMA_JAVA.md`)

## Configuración

Crea el archivo `appsettings.json` en la raíz de `SistemaDeElementos/` con la siguiente estructura:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "UsarBundle": "false",

  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Se10;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },

  "ServidorDeCorreo": {
    "MiServidorSmtp": {
      "sistema": "SMTP",
      "usuario": "tucuenta@tudominio.com",
      "servidor_smtp": "smtp.tudominio.com",
      "servidor_imap": "imap.tudominio.com",
      "puerto_smtp": "587",
      "puerto_imap": "993",
      "clave": "TuPasswordSeguro"
    },
    "MiServidorOffice365": {
      "sistema": "GRAPH",
      "usuario": "tucuenta@tudominio.com",
      "tenantId": "00000000-0000-0000-0000-000000000000",
      "grantType": "client_credentials",
      "clientId": "tu-client-id",
      "clientSecret": "tu-client-secret",
      "scope": "https://graph.microsoft.com/.default"
    },
    "MiServidorMailJet": {
      "sistema": "MAILJET",
      "usuario": "tucuenta@tudominio.com",
      "clientId": "tu-api-key",
      "clientSecret": "tu-api-secret"
    },
    "MiServidorSinCredenciales": {
      "sistema": "SIN_CREDENCIALES",
      "usuario": "noreply@tudominio.com",
      "servidor": "localhost"
    },
    "MiServidorConCredenciales": {
      "sistema": "CON_CREDENCIALES",
      "usuario": "tucuenta@tudominio.com",
      "servidor": "smtp.tudominio.com",
      "sslActivo": "true",
      "puerto": "587",
      "clave": "TuPasswordSeguro"
    }
  },

  "DatosIniciales": {
    "urlBase": "https://localhost:5001",
    "servidorDeCorreo": "MiServidorSmtp",
    "host": "imap.tudominio.com",
    "protocolo": "IMAP",
    "miCorreo": "tucuenta@tudominio.com",
    "clienteSecreto": "TuPasswordSeguro"
  }
}
```

> **Nota:** El nombre de la clave en `ConnectionStrings` debe coincidir con el valor definido en `Literal.CadenaDeConexion` del proyecto. Si tu código usa otro nombre, ajústalo.

### Descripción de las secciones

| Sección | Propósito |
|---------|-----------|
| `ConnectionStrings` | Cadena de conexión a SQL Server para Entity Framework Core. |
| `ServidorDeCorreo` | Configuración para **envío** de correos. Puedes definir tantos servidores como necesites; cada uno es una subsección con nombre libre. |
| `DatosIniciales` | Configuración para **lectura** de correos (buzón del usuario) y datos de arranque de la aplicación. |

### Sistemas de correo soportados para envío

| Valor de `sistema` | Descripción | Campos requeridos |
|--------------------|-------------|-------------------|
| `SMTP` | Envío vía SMTP + copia a "Enviados" por IMAP (MailKit) | `usuario`, `servidor_smtp`, `servidor_imap`, `puerto_smtp`, `puerto_imap`, `clave` |
| `GRAPH` | Microsoft Graph API (Office 365 / Exchange Online) | `usuario`, `tenantId`, `grantType`, `clientId`, `clientSecret`, `scope` |
| `MAILJET` | Envío vía API de Mailjet | `usuario`, `clientId` (ApiKey), `clientSecret` (ApiSecret) |
| `CON_CREDENCIALES` | SMTP estándar con autenticación | `usuario`, `servidor`, `sslActivo`, `puerto`, `clave` |
| `SIN_CREDENCIALES` | Relay SMTP local sin auth | `usuario`, `servidor` |

### Protocolos de lectura de correo soportados

El campo `protocolo` dentro de `DatosIniciales` indica cómo se lee el buzón:

| Valor | Descripción |
|-------|-------------|
| `IMAP` | Lectura directa por IMAP (usa `host`, `miCorreo` y `clienteSecreto` como password). |
| `Auth2` | OAuth2 de Google Gmail (usa `clienteSecreto` como nombre del archivo JSON de credenciales). |
| `ApiKey` | Gmail vía API Key (usa `clienteSecreto` como API Key). |

## Cómo ejecutar

1. Clona el repositorio.
2. Crea el archivo `appsettings.json` con tu configuración.
3. Asegúrate de que SQL Server está accesible.
4. Desde la carpeta `SistemaDeElementos`, ejecuta:

```bash
dotnet restore
dotnet ef database update   # si usas CLI de EF Core
dotnet run
```

5. Abre `https://localhost:5001` (o el puerto que aparezca en consola).

## Estructura del repositorio

```
Se10/
├── SistemaDeElementos/          # Proyecto web ASP.NET Core MVC
│   ├── Controllers/             # Controladores por módulo funcional
│   ├── Servicios/               # Servicios de correo, errores, elementos
│   ├── GestoresDeNegocio/       # Lógica de negocio
│   ├── ServicioDeDatos/         # DbContext y repositorios
│   ├── ModeloDeDto/             # Objetos de transferencia
│   ├── Migraciones/             # Migrations de EF Core
│   ├── wwwroot/                 # Assets estáticos y TypeScript
│   └── ...
├── ServicioDeReportes/          # Generación de PDFs (QuestPDF / iText)
├── ProcesadorOcr/             # OCR con Tesseract
├── ServicioSepa/              # XML SEPA
├── ServiciosLexnet/           # Integración LexNet
└── Ayudas/Extensiones/        # Utilidades compartidas
```

## Notas adicionales

- El proyecto usa **TypeScript** compilado a `wwwroot/js`. Revisa `tsconfig.json` si modificas los fuentes `.ts`.
- El **bundle de JavaScript** se puede activar con `"UsarBundle": "true"`. En desarrollo se recomienda `false`.
- La **firma digital XAdES** requiere un JRE portable y librerías DSS que no están en Git. Consulta `README_FIRMA_JAVA.md` para generarlas.
- La aplicación inicializa automáticamente la base de datos en el primer arranque si no detecta datos previos.

## Licencia

Proyecto privado – todos los derechos reservados.
