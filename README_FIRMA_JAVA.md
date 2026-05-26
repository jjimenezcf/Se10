# Componente de Firma XAdES (FirmaDss)

Este componente firma documentos XML en formato XAdES-BASELINE-B usando la librería
europea DSS (Digital Signature Services) ejecutada desde C# como subproceso Java.

## Estructura esperada en disco

```
SistemaDeElementos/wwwroot/lib/
├── jre/                        ← JRE portable (NO está en Git, genera con jlink)
│   └── bin/
│       └── java.exe
└── jar/
    └── xades/
        ├── FirmaDss.jar        ← JAR compilado (NO está en Git, genera con el .bat)
        ├── lib/                ← Dependencias DSS (~25MB, NO están en Git)
        │   └── *.jar
        └── src/
            └── Main.java       ← Fuente Java (SÍ está en Git)
```

## Lo que SÍ está en Git

- `src/Main.java` — código fuente Java
- `src/importarLibrerias.bat` — script para copiar las libs al compilar
- Este `README`

## Lo que NO está en Git y hay que regenerar

### 1. Dependencias DSS (`lib/*.jar`) — ~25 MB

Descarga las librerías DSS 6.4 desde:
https://ec.europa.eu/digital-building-blocks/DSS/webapp-demo/downloads

O cópialas desde otro desarrollador del equipo. Colócalas en:
```
wwwroot/lib/jar/xades/lib/
```

### 2. FirmaDss.jar — compilar y desplegar

Con las libs ya en su sitio, ejecuta desde la raíz del proyecto Java
(donde está el `build-y-desplegar.bat`):

```bat
build-y-desplegar.bat
```

Esto compila `Main.java` y copia el JAR resultante a `wwwroot/lib/jar/xades/`.

### 3. JRE portable — generarlo con jlink

Requiere tener instalado el **JDK 17** en la máquina de desarrollo.

**JDK 17:**
```bat
jlink --no-header-files --no-man-pages --compress=2 ^
      --add-modules java.base,java.xml,java.logging,java.naming,java.security.jgss ^
      --output ruta_absoluta_al_repo\SistemaDeElementos\wwwroot\lib\jre
```

**JDK 21 o superior** (compress cambió de sintaxis):
```bat
jlink --no-header-files --no-man-pages --compress=zip-6 ^
      --add-modules java.base,java.xml,java.logging,java.naming,java.security.jgss ^
      --output ruta_absoluta_al_repo\SistemaDeElementos\wwwroot\lib\jre
```

Resultado: ~35 MB en `wwwroot/lib/jre/bin/java.exe`.
El JRE es compartido para todos los JARs que se añadan bajo `wwwroot/lib/jar/`.

## Verificar que todo funciona

Desde C#, el método `FirmadorXadesService.PuedeFirmarConJar()` devuelve `true`
si el entorno está correctamente configurado.
