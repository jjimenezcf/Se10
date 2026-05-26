:: 1. Borrar los JARs antiguos de la versión 6.1 para evitar conflictos
del /Q "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\*.jar"

:: 2. Copiar todo el bloque DSS y especificaciones de la versión 6.4
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\dss-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\specs-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y

:: 3. Copiar el motor criptográfico BouncyCastle actualizado
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\bc*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y

:: 4. Copiar las dependencias de validación, xmlsec, jaxb y utilidades que pide el compilador
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\xmlsec-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\jakarta.xml.bind-api-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\jaxb-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\txw2-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\stax2-api-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\woodstox-core-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\commons-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\guava-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y
xcopy "C:\Users\jjimenez\Downloads\dss-demo-bundle-6.4+20260415\dss-demo-bundle-6.4\apache-tomcat-11.0.21\webapps\ROOT\WEB-INF\lib\slf4j-api-*.jar" "C:\Users\jjimenez\source\repos\FirmaDss\FirmaDss\lib\" /Y