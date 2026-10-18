<#
.SYNOPSIS
  Recorta una copia de LibreOffice (descomprimida con msiexec /a) dejando solo lo necesario para convertir
  docx, xlsx, txt y html a pdf en modo consola (soffice --headless --convert-to pdf).

.DESCRIPTION
  De unos 1.510 MB (19.460 ficheros) deja unos 470 MB (unos 2.850 ficheros). Lo que se quita son los diccionarios,
  la ayuda, Python, Impress/Draw/Base, la interfaz grafica, los idiomas (se dejan es y en), tipografias de otros
  alfabetos, el actualizador, etc.

  Se ha comprobado con la conversion de un docx, un xlsx, un txt y un html: los pdf salen iguales (mismo tamano,
  texto y tipografias) y con el primer arranque mas rapido.

  LO QUE NO SE DEBE QUITAR (probado: sin ello falla o cambia el resultado):
    - share\template      : estilos por defecto (sin ello cambia la tipografia de los titulos de un html)
    - share\config\soffice.cfg : configuracion de los modulos (sin ella "source file could not be loaded")
    - share\registry (salvo res\) y program\mergedlo.dll y las librerias que referencia (filtros de otros formatos,
      solver, Firebird): son dependencias de carga; sin ellas el arranque falla con el codigo -1073741515
    - Fonts\opens___.ttf  : OpenSymbol, tipografia de vinetas y simbolos

.PARAMETER Ruta
  Carpeta de LibreOffice a recortar (la que contiene program\soffice.exe). SE MODIFICA: hacerlo sobre una copia.

.PARAMETER SoloMostrar
  No borra nada: muestra lo que se borraria.

.EXAMPLE
  .\RecortarLibreOffice.ps1 -Ruta C:\LibreOffice_recortado
#>
param(
    [Parameter(Mandatory = $true)][string]$Ruta,
    [switch]$SoloMostrar
)

$ErrorActionPreference = 'Stop'
$Ruta = (Resolve-Path -LiteralPath $Ruta).Path.TrimEnd('\')

if (-not (Test-Path -LiteralPath "$Ruta\program\soffice.exe")) {
    throw "No se encuentra program\soffice.exe en '$Ruta': indique la carpeta donde se descomprimio LibreOffice."
}

$antes = Get-ChildItem -LiteralPath $Ruta -Recurse -File -Force
$borrados = New-Object System.Collections.Generic.List[string]

function Quitar([string]$ruta) {
    if (-not (Test-Path -LiteralPath $ruta)) { return }
    $borrados.Add($ruta)
    if (-not $SoloMostrar) { Remove-Item -LiteralPath $ruta -Recurse -Force }
}

# --- Carpetas completas
foreach ($d in 'help', 'readmes',
               'share\gallery', 'share\wizards', 'share\autotext', 'share\xpdfimport',
               'share\autocorr', 'share\glade', 'share\tipoftheday', 'share\wordbook', 'share\firebird',
               'share\palette', 'share\toolbarmode', 'share\Scripts',
               'program\wizards', 'program\classes', 'program\__pycache__') {
    Quitar "$Ruta\$d"
}

# Python (se registra en el primer arranque solo si existe): sin la carpeta ni las librerias no hace falta
Get-ChildItem -LiteralPath "$Ruta\program" -Directory -Filter 'python-core*' -ErrorAction SilentlyContinue | ForEach-Object { Quitar $_.FullName }

# Extensiones incluidas: diccionarios (dict-*), wiki-publisher, nlpsolver
if (Test-Path -LiteralPath "$Ruta\share\extensions") {
    Get-ChildItem -LiteralPath "$Ruta\share\extensions" -Force | ForEach-Object { Quitar $_.FullName }
}

# --- Idiomas: se dejan es y en*
Get-ChildItem -LiteralPath "$Ruta\program\resource" -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne 'es' -and $_.Name -notlike 'en*' } | ForEach-Object { Quitar $_.FullName }
Get-ChildItem -LiteralPath "$Ruta\share\registry\res" -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notmatch '_(es|en-US|en-GB|en-ZA)\.xcd$' } | ForEach-Object { Quitar $_.FullName }

# --- Interfaz grafica: iconos y modulos que no son Writer ni Calc (se deja soffice.cfg y sus modulos de Writer, Calc y Web)
Get-ChildItem -LiteralPath "$Ruta\share\config" -File -Filter 'images_*.zip' -ErrorAction SilentlyContinue | ForEach-Object { Quitar $_.FullName }
foreach ($m in 'sdraw', 'simpress', 'BasicIDE', 'dbapp', 'dbbrowser', 'dbquery', 'dbrelation', 'dbreport', 'dbtable', 'dbtdata',
               'sabpilot', 'sbibliography', 'scanner', 'spropctrlr') {
    Quitar "$Ruta\share\config\soffice.cfg\modules\$m"
}

# --- Librerias que ninguna otra referencia: Impress/Draw, bases de datos, Python, importador de pdf, VBA, actualizador, accesibilidad
foreach ($f in 'sdlo.dll', 'sduilo.dll', 'Engine12.dll', 'postgresql-sdbc-impllo.dll', 'libpq.dll', 'jdbclo.dll', 'hsqldb.dll', 'dbplo.dll', 'abplo.dll',
               'python313.dll', 'pyuno.pyd', 'xpdfimport.exe', 'pdfimportlo.dll', 'vbaobjlo.dll', 'vbaswobjlo.dll',
               'PresentationMinimizerlo.dll', 'updchklo.dll', 'updater.exe', 'update_service.exe', 'so_activex.dll',
               'UAccCOM.dll', 'winaccessibility.dll', 'unopkg.exe', 'unopkg.com', 'unopkgapp.dll', 'unopkg.bin', 'deploymentgui.dll') {
    Quitar "$Ruta\program\$f"
}

# --- Tipografias: se dejan las latinas y sus sustitutas metricas (Liberation, Carlito, Caladea, DejaVu, Noto Sans/Serif) y OpenSymbol
foreach ($p in 'LinLibertine*', 'LinBiolinum*', 'Amiri*', 'Scheherazade*', 'NotoNaskhArabic*', 'NotoSansArabic*', 'NotoKufiArabic*',
               'FrankRuehl*', 'FrankRuhl*', 'Nachlieli*', 'Rubik*', 'DejaVuMath*', 'NotoSerifGeorgian*', 'NotoSansGeorgian*',
               'NotoSerifLao*', 'NotoSansLao*', 'David*', 'Alef*', 'Miriam*', 'NotoSerifHebrew*', 'NotoSansHebrew*',
               'NotoSansArmenian*', 'NotoSerifArmenian*', 'ReemKufi*', 'NotoSansLisu*',
               'DejaVuSansCondensed*', 'DejaVuSerifCondensed*') {
    Get-ChildItem -LiteralPath "$Ruta\Fonts" -Recurse -File -Filter $p -ErrorAction SilentlyContinue | ForEach-Object { Quitar $_.FullName }
}

if ($SoloMostrar) {
    $borrados | ForEach-Object { $_.Substring($Ruta.Length + 1) }
    "Se borrarian {0} elementos (carpetas o ficheros)." -f $borrados.Count
    return
}

$despues = Get-ChildItem -LiteralPath $Ruta -Recurse -File -Force
"Antes:   {0,6} ficheros, {1,6:N0} MB" -f $antes.Count, (($antes | Measure-Object Length -Sum).Sum / 1MB)
"Despues: {0,6} ficheros, {1,6:N0} MB" -f $despues.Count, (($despues | Measure-Object Length -Sum).Sum / 1MB)
