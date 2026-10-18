# Manual: crear un sitio web nuevo de Se

Esta guía recoge, en orden, todo lo que hay que hacer para dar de alta un cliente/sitio nuevo en el servidor.
Los ejemplos usan el sitio **`jhm`**; sustituye ese nombre por el del sitio nuevo.

## Resumen

| # | Paso | Dónde | Quién lo hace |
|---|---|---|---|
| 0 | Decidir los nombres y revisar requisitos | — | tú |
| 1 | Tener la publicación descomprimida en `C:\Prepublicacion\Se` | servidor | scripts `01`/`20` o a mano |
| 2 | Crear usuario, pool, sitio https, copiar la publicación y dar permisos | servidor | [`CrearSitioWeb.ps1`](CrearSitioWeb.ps1) |
| 3 | Crear el `appsettings.json` del sitio | servidor | tú |
| 4 | Arrancar el sitio: la aplicación crea la base de datos | servidor | automático |
| 5 | Scripts SQL de después de crear la BD | SQL Server | tú |
| 6 | Añadir el sitio a los scripts de despliegue | repositorio | tú |
| 7 | DNS, certificado y logo | varios | tú |
| 8 | Verificación final | navegador | tú |

---

## 0. Antes de empezar

### Decide estos nombres

| Dato | Regla | Ejemplo |
|---|---|---|
| **Nombre del sitio** (y del pool) | Letras, números, `.`, `-` o `_`. **Máximo 16 caracteres** (Windows limita el usuario del pool a 20 y se le antepone `www-`). | `jhm` |
| **Usuario del pool** | Lo crea el script: `www-<sitio>`. | `www-jhm` |
| **Directorio del sitio** | `C:\inetpub\<sitio>` | `C:\inetpub\jhm` |
| **Base de datos** | `SE_<SITIO>` en mayúsculas (hay alguna excepción histórica: `AB_ALBERTO_MARTINEZ`). | `SE_JHM` |
| **Sitio origen** | Un sitio que ya funcione y tenga enlace https. De él se copian el certificado y la configuración del pool. Por defecto `acromur`. | `acromur` |

### Requisitos del servidor

- Windows Server con **IIS** y el módulo de administración de IIS (PowerShell `WebAdministration`).
- **SQL Server** accesible desde el servidor (`.\SQLEXPRESS` en los scripts de despliegue).
- La publicación completa en `C:\Prepublicacion\Publicacion.zip` (se genera con `Z_EmpaquetarPublicacionCompleta`).
- **LibreOffice** recortado en `C:\LibreOffice` (ver [`RecortarLibreOffice.ps1`](RecortarLibreOffice.ps1)). Es opcional: sin él,
  la conversión a PDF usa ConvertApi si hay clave.
- Todo lo del servidor se ejecuta en una consola de **PowerShell como administrador**.

---

## 1. Dejar la publicación descomprimida en `C:\Prepublicacion\Se`

El script del paso 2 copia esa carpeta al sitio nuevo. Puede que ya esté descomprimida de un despliegue anterior: comprueba que
la fecha de `C:\Prepublicacion\Se\SistemaDeElementos.dll` es la de la publicación que quieres.

Si no lo está, descomprímela **sin tocar los sitios en producción**:

```powershell
# OJO: no uses 01-Descomprimir y parar servicios.cmd para esto: bloquea las colas y PARA todos los sitios.
Remove-Item C:\Prepublicacion\Se -Recurse -Force -ErrorAction SilentlyContinue
Expand-Archive -LiteralPath C:\Prepublicacion\Publicacion.zip -DestinationPath C:\Prepublicacion\Se
```

> La publicación **no incluye `appsettings.json`** (el empaquetador lo borra a propósito). Se crea a mano en el paso 3.

---

## 2. Crear el sitio con `CrearSitioWeb.ps1`

Está en esta misma carpeta. Se ejecuta en el servidor, como administrador.

### 2.1 Comprobar primero (no cambia nada)

```powershell
cd C:\...\Despliegues      # la carpeta donde esté el script
powershell -ExecutionPolicy Bypass -File .\CrearSitioWeb.ps1 -Sitio jhm -SoloComprobar
```

Muestra el plan y comprueba que existe el sitio origen, su certificado, la prepublicación y LibreOffice, que el puerto https
no tiene otro certificado y que el sitio y el pool nuevos no existen todavía. Si algo falla, lo dice y no se toca nada.

### 2.2 Crearlo

```powershell
powershell -ExecutionPolicy Bypass -File .\CrearSitioWeb.ps1 -Sitio jhm
```

Pide dos veces la **contraseña del usuario `www-jhm`** (mínimo 12 caracteres). Guárdala en tu gestor de contraseñas: no se escribe
en ningún fichero y solo se necesita si hay que cambiar la identidad del pool.

### 2.3 Qué hace

1. Crea el usuario local `www-jhm` (contraseña que no caduca) y lo añade al grupo `IIS_IUSRS`.
2. Crea el **pool** `jhm` con la configuración del pool origen (versión, pipeline, reciclado, tiempo de inactividad, `Suspend` y
   `AlwaysRunning`) y como identidad el usuario `www-jhm`.
3. Copia `C:\Prepublicacion\Se` a `C:\inetpub\jhm` (solo si esa carpeta está vacía o no existe, para no pisar nada).
4. Crea el **sitio** `jhm` con enlace **https** y el mismo certificado que el sitio origen.
5. Da permiso de **modificación** al usuario del pool sobre `wwwroot\Agendas`, `Archivos`, `Plantillas`, `js` y `token.json`.
6. Crea las carpetas de datos con permiso de modificación para el pool:
   `C:\AlmacenDocumental\jhm`, `C:\Temp\Certificados\jhm`, `Excepciones\jhm`, `Firmar\jhm`, `Trazas\jhm` y `Zip\jhm`.
7. Da permiso de **lectura y ejecución** sobre `C:\LibreOffice` (no necesita escribir: su perfil va a la carpeta temporal).
8. Muestra la verificación y recuerda lo pendiente.

### 2.4 Parámetros

| Parámetro | Para qué | Por defecto |
|---|---|---|
| `-Sitio` | Nombre del sitio y del pool (obligatorio) | — |
| `-Origen` | Sitio del que se copian el certificado y el pool | `acromur` |
| `-Ruta` | Directorio del sitio | `C:\inetpub\<sitio>` |
| `-PuertoHttps` | Puerto del enlace https | `100` |
| `-RutaPrepublicacion` | Dónde está la publicación descomprimida | `C:\Prepublicacion\Se` |
| `-NoCopiarPrepublicacion` | No copia los ficheros (el directorio ya los tiene) | — |
| `-RutaLibreOffice` | Carpeta a la que se da lectura y ejecución | `C:\LibreOffice` |
| `-Contrasena` | Contraseña como `SecureString` (si no, se pide) | — |
| `-Recrear` | Si el sitio o el pool **ya existen**, los borra y los crea de nuevo | — |
| `-SoloComprobar` | Revisa y muestra el plan sin cambiar nada | — |

### 2.5 Qué NO hace el script

- No crea el `appsettings.json` ni la base de datos (pasos 3 a 5).
- No actualiza tus scripts de despliegue (paso 6).
- No da de alta el DNS (paso 7).
- **No borra nada sin permiso**: si el sitio o el pool ya existen, se detiene. Solo `-Recrear` los borra, y lo avisa antes.

> **Puerto https.** El script crea el enlace como `0.0.0.0:<puerto>` con ese certificado. Mira en el administrador de IIS
> (*Sitios → Enlaces*) cómo publican los demás sitios (puerto, nombre de host, SNI) y, si son distintos, ajusta el enlace a mano.
> Si el puerto ya tiene un certificado distinto del origen, el script se detiene en lugar de sustituirlo.

---

## 3. Crear el `appsettings.json` del sitio

Cópialo de otro sitio que funcione y cámbialo. Está en `C:\inetpub\<sitio origen>\appsettings.json`. Se guarda en
`C:\inetpub\jhm\appsettings.json`. **No lo subas a git**: está en `.gitignore` y lleva credenciales.

Lo que hay que cambiar:

| Clave | Qué poner |
|---|---|
| `ConnectionStrings:CadenaDeConexion` | La misma cadena que los demás sitios, con `Database=SE_JHM`. |
| `ServidorDeCorreo` | Añadir una sección con el nombre `jhm.gmx` (u otro proveedor: `jhm.mailjet`, `jhm.gmail`...) copiando la de otro sitio con sus credenciales. El nombre debe coincidir con la variable `CFG_Servidor_De_Correo` (lo hace el script `04`, paso 5). |
| `App:CorsOrigins` | Añadir la URL del sitio nuevo si procede. |
| `BackgroundCola:ColaActiva` | Comprueba que está como en los demás servidores: la cola de trabajos es la que envía los correos y comunica con Hacienda. |

---

## 4. Primer arranque: la aplicación crea la base de datos

Arranca el sitio:

```powershell
Start-WebAppPool -Name jhm
Start-WebSite -Name jhm
```

Abre la URL una vez para que arranque. En el **primer arranque** la aplicación (`Program.cs`) hace sola:

1. Crea la base de datos `SE_JHM` si no existe y le aplica todas las migraciones.
2. Si es nueva (no tiene la variable `CFG_UrlBase`), carga los **datos iniciales** (`InzEntorno.PersistirDatosIniciales`).
3. Crea los registros de entorno (menús, vistas, etapas...).

Tarda un rato. El usuario SQL de la cadena de conexión debe poder **crear bases de datos**. Si algo falla en la creación, la
aplicación borra la base de datos a medias, para que se pueda reintentar limpio: mira el motivo en `C:\Temp\Trazas\jhm` o en el
registro de eventos y vuelve a arrancar.

Comprueba en SQL Server que existe `SE_JHM`.

---

## 5. Scripts SQL de después de crear la BD

Están en `Documentación\ScriptSql\Tras crear una nueva BD`. Se ejecutan **contra la base de datos nueva** (`USE SE_JHM`),
en este orden:

| Script | Qué hace | Cómo se usa |
|---|---|---|
| `00.-CapitalizarFrase.sql` | Crea la función `ENTORNO.CapitalizarFrase`, que necesitan los siguientes. | Ejecutarlo. |
| `01.-Copiar Maestros Callejero.sql` | Crea `dbo.CopiarMaestrosDeCallejero`: copia países, provincias, municipios, tipos de vía y códigos postales de otra BD. | Ejecutarlo y luego `EXEC dbo.CopiarMaestrosDeCallejero @BdOrigen = 'SE_SECIFY';` |
| `02.-Copiar Maestros Bancos.sql` | Crea `dbo.CopiarMaestrosBancarios`: bancos, cuentas contables, naturalezas y clases de permiso. | Ejecutarlo y luego `EXEC dbo.CopiarMaestrosBancarios @BdOrigen = 'SE_SECIFY';` |
| `03.-Copiar Calles.sql` | Crea `dbo.CopiarCallesDeCallejero`: barrios, zonas y calles (llama antes al `01`). | Ejecutarlo y luego `EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_SECIFY';` |
| `04.-Actualizar variables.sql` | Pone las variables propias del sitio (ver abajo). | **Editar `@sitioweb = 'jhm'`** y ejecutarlo. |
| `05.-Capitalizar municipios y provincias.sql` | Normaliza mayúsculas de municipios y provincias. | Ejecutarlo (es seguro relanzarlo). |
| `10.-Bloquear y desbloquear Cola.sql` | Crea `dbo.Bloquear_Cola` y `dbo.Desbloquear_Cola`. | Ejecutarlo. **Imprescindible**: los scripts de despliegue los llaman en cada base de datos. |

La BD origen (`@BdOrigen`) debe ser una que ya tenga esos datos y esté en línea. Los procedimientos se pueden relanzar sin duplicar.

### Qué fija el script `04`

Con `@sitioweb = 'jhm'`:

| Variable | Valor |
|---|---|
| `CFG_Ruta_Ficheros_De_Debug` | `C:\Temp\Trazas\jhm` |
| `CFG_Servidor_Archivos` | `c:\AlmacenDocumental\jhm` |
| `CFG_UrlBase` | `https://jhm.femdek.com/` ← el dominio `femdek.com` está escrito en el script: **cámbialo si el sitio usa otro** |
| `CFG_Servidor_De_Correo` | `jhm.gmx` (debe existir esa sección en `appsettings.json`) |
| `LYT_Imagen_De_Fondo` | `jhm-logo.png` |
| `LYT_Opacidad` | `0.01` |

Las carpetas de trazas y de almacén documental ya las creó `CrearSitioWeb.ps1`.

---

## 6. Añadir el sitio a los scripts de despliegue

Los scripts llevan listas fijas. Si no añades el sitio nuevo, **no se desplegará con el resto** y no se le bloqueará la cola.

| Script | Qué añadir |
|---|---|
| `01-Descomprimir y parar servicios.cmd` | La BD en el bucle de bloquear colas; el sitio en `SITIOS`; el pool en `APPPOOLS`. |
| `02-Desplegar y arrancar servicios.cmd` | Un `DIRn` nuevo con su `xcopy`, su `findstr`, su `Start-WebSite`, su `Start-WebAppPool` y la BD en el bucle de desbloquear colas. |
| `91-DespliegueEnTodosLosSitiosWeb.bat` | El par `SE_JHM:jhm` en la lista `SITIOS` (el comentario del script dice que sigue la lista del `01`). |
| `90-BorrarBinario.bat` | El directorio en la lista de carpetas. |

`90-DesplegarCompleta.bat` y `90-RestaurarSitio.bat` no necesitan cambios: reciben la BD y el sitio como parámetros.

---

## 7. DNS, certificado y logo

- **DNS**: da de alta el nombre del sitio (el de `CFG_UrlBase`).
- **Certificado**: se reutiliza el del sitio origen. Si el nombre del sitio nuevo no está cubierto por ese certificado
  (por ejemplo, si no es un comodín), el navegador avisará: habrá que poner uno válido en el enlace.
- **Logo**: copia `jhm-logo.png` a `C:\inetpub\jhm\wwwroot\images\` (la variable `LYT_Imagen_De_Fondo` lo busca en `../images/`).

---

## 8. Verificación final

- [ ] La URL abre con candado (certificado correcto).
- [ ] El inicio de sesión funciona y llega el **correo del doble factor** (comprueba la sección de correo).
- [ ] Se puede **subir un archivo** y se guarda en `C:\AlmacenDocumental\jhm`.
- [ ] Un **docx** anexado se convierte a PDF con el botón rojo. Si sale *«no hay acceso a LibreOffice ni licencia de ConvertApi»*,
      revisa `C:\LibreOffice` y su permiso (ver más abajo).
- [ ] La cola de trabajos funciona (un correo enviado desde la aplicación llega).
- [ ] Se pueden **firmar** documentos (certificados en `C:\Temp\Certificados\jhm`).
- [ ] Hay trazas en `C:\Temp\Trazas\jhm`.
- [ ] Ejecuta `90-DesplegarCompleta SE_JHM jhm` una vez: confirma que el sitio entra en el proceso de despliegue.

---

## Problemas frecuentes

| Síntoma | Causa y solución |
|---|---|
| `Access is denied` al escribir ficheros | La identidad real del sitio es la del **pool**, no el usuario de «Conectar como». Revisa *Pool → Configuración avanzada → Identidad* y dale permiso NTFS a `www-<sitio>`. |
| El sitio da error 500.30 o no arranca | Falta `web.config`, `appsettings.json` o la cadena de conexión es incorrecta. Mira el registro de eventos y `C:\Temp\Trazas\<sitio>`. |
| El script dice que ya existe el sitio o el pool | Es una protección. Si estás seguro de que quieres rehacerlo, usa `-Recrear`; si no, elige otro nombre. |
| *«El puerto 100 ya tiene un certificado distinto»* | Otro sitio usa ese puerto con otro certificado. Elige otro `-PuertoHttps` o resuelve el conflicto en IIS. |
| *«No hay acceso a LibreOffice ni licencia de ConvertApi»* | Existe el parámetro del archivador `ARC_Ruta_LibreOffice` (por defecto `C:\LibreOffice\program\soffice.exe`) y la carpeta no existe o el usuario del pool no puede leerla. Comprueba con `icacls C:\LibreOffice`. |
| Se olvida la contraseña de `www-<sitio>` | `Set-LocalUser -Name www-jhm -Password (Read-Host -AsSecureString)` y, además, ponerla en *Pool → Configuración avanzada → Identidad*. |
| No se bloquea la cola en el despliegue | Falta ejecutar `10.-Bloquear y desbloquear Cola.sql` en la BD nueva, o el sitio no está en los scripts `01`/`02`. |

---

## Deshacer un sitio creado por error

```powershell
Remove-Website -Name jhm
Remove-WebAppPool -Name jhm
Remove-LocalUser -Name www-jhm
# Si quieres borrar también sus ficheros (comprueba antes las rutas):
#   C:\inetpub\jhm, C:\AlmacenDocumental\jhm, C:\Temp\Certificados\jhm, C:\Temp\Excepciones\jhm,
#   C:\Temp\Firmar\jhm, C:\Temp\Trazas\jhm, C:\Temp\Zip\jhm
# La base de datos SE_JHM, si existe, se borra desde SQL Server.
```
