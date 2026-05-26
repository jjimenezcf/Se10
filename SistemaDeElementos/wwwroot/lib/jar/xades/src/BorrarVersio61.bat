@echo off
setlocal
set LIB=C:\Users\jjimenez\source\repos\Se10\SistemaDeElementos\wwwroot\lib\jar\xades\lib

echo =====================================================
echo  LIMPIEZA DE JARS DUPLICADOS / VERSIONES ANTIGUAS
echo =====================================================
echo Carpeta: %LIB%
echo.

set ERRORES=0

call :borrar bcpkix-jdk18on-1.78.1.jar
call :borrar bcprov-jdk18on-1.78.1.jar
call :borrar dss-document-6.1.jar
call :borrar dss-enumerations-6.1.jar
call :borrar dss-model-6.1.jar
call :borrar dss-spi-6.1.jar
call :borrar dss-token-6.1.jar
call :borrar dss-utils-6.1.jar
call :borrar dss-xades-6.1.jar
call :borrar dss-xml-utils-6.1.jar
call :borrar slf4j-api-2.0.13.jar
call :borrar xmlsec-3.0.4.jar

echo.
if %ERRORES%==0 (
    echo Limpieza completada sin errores.
) else (
    echo Limpieza completada con %ERRORES% archivo(s) no encontrado(s).
)
echo.
pause
exit /b 0

:borrar
set FICHERO=%LIB%\%1
if exist "%FICHERO%" (
    del /q "%FICHERO%"
    echo [OK]      Eliminado: %1
) else (
    echo [OMITIDO] No encontrado: %1
    set /a ERRORES+=1
)
exit /b 0