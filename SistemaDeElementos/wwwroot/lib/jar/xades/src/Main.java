import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.PrintStream;
import java.util.List;

// Importaciones esenciales del motor eIDAS DSS de la Unión Europea
import eu.europa.esig.dss.enumerations.DigestAlgorithm;
import eu.europa.esig.dss.enumerations.SignatureLevel;
import eu.europa.esig.dss.enumerations.SignaturePackaging;
import eu.europa.esig.dss.model.DSSDocument;
import eu.europa.esig.dss.model.InMemoryDocument;
import eu.europa.esig.dss.model.SignatureValue;
import eu.europa.esig.dss.model.ToBeSigned;
import eu.europa.esig.dss.spi.validation.CommonCertificateVerifier;
import eu.europa.esig.dss.token.KeyStoreSignatureTokenConnection;
import eu.europa.esig.dss.token.DSSPrivateKeyEntry;
import eu.europa.esig.dss.xades.XAdESSignatureParameters;
import eu.europa.esig.dss.xades.signature.XAdESService;

// Para manipular el XML antes de firmar (inyectar la cuna UBL)
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;

import java.security.KeyStore.PasswordProtection;

public class Main {

    // Namespaces UBL — igual que en el C#
    private static final String NS_INVOICE = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static final String NS_EXT     = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    public static void main(String[] args) {
        // 1. Validar parámetros obligatorios
        if (args.length < 4) {
            System.err.println("Error: Parámetros insuficientes.");
            System.err.println("Uso: java -cp \"FirmaDss.jar;lib/*\" Main <xml_origen> <xml_destino> <ruta_pfx> <password_pfx>");
            System.exit(1);
        }

        String rutaXmlOrigen       = args[0];
        String rutaXmlDestino      = args[1];
        String rutaCertificado     = args[2];
        String passwordCertificado = args[3];

        // 2. Redirigir stdout/stderr al .log paralelo a la factura
        try {
            String rutaLog = cambiarExtensionALog(rutaXmlOrigen);
            PrintStream psLog = new PrintStream(new FileOutputStream(new File(rutaLog), false), true, "UTF-8");
            System.setOut(psLog);
            System.setErr(psLog);
        } catch (Exception e) {
            System.err.println("Fallo crítico inicializando el archivo de log: " + e.getMessage());
            System.exit(3);
        }

        System.out.println("====================================================");
        System.out.println("INICIO DEL PROCESO DE FIRMA FACTURAE/UBL CON DSS");
        System.out.println("====================================================");
        System.out.println("Fichero origen:      " + rutaXmlOrigen);
        System.out.println("Fichero destino:     " + rutaXmlDestino);
        System.out.println("Ruta Certificado:    " + rutaCertificado);

        FileInputStream fis = null;
        KeyStoreSignatureTokenConnection tokenConnection = null;

        try {
            // 3. Cargar el certificado digital (.pfx / .p12)
            System.out.println("Accediendo al almacén de claves del certificado...");
            File ficheroCert = new File(rutaCertificado);
            if (!ficheroCert.exists()) {
                throw new IllegalArgumentException("El archivo de certificado no existe en la ruta especificada.");
            }

            fis = new FileInputStream(ficheroCert);
            tokenConnection = new KeyStoreSignatureTokenConnection(
                    fis,
                    "PKCS12",
                    new PasswordProtection(passwordCertificado.toCharArray())
            );

            List<DSSPrivateKeyEntry> keys = tokenConnection.getKeys();
            if (keys == null || keys.isEmpty()) {
                throw new IllegalStateException("No se encontraron claves privadas válidas en el certificado suministrado.");
            }
            DSSPrivateKeyEntry privateKey = keys.get(0);
            System.out.println("Certificado cargado. Sujeto: " + privateKey.getCertificate().getSubject().getPrincipal());

            // 4. Cargar el XML origen y decidir si es UBL Invoice
            System.out.println("Cargando y analizando el XML origen...");
            DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
            dbf.setNamespaceAware(true);
            DocumentBuilder db = dbf.newDocumentBuilder();
            Document doc = db.parse(new File(rutaXmlOrigen));
            doc.normalizeDocument();

            Element root = doc.getDocumentElement();
            boolean esUbl = "Invoice".equals(root.getLocalName())
                         && NS_INVOICE.equals(root.getNamespaceURI());

            // XPath al nodo destino de la firma; por defecto ENVELOPED (al final del documento)
            String xpathDestino = null;

            if (esUbl) {
                System.out.println("Documento UBL Invoice detectado. Inyectando cuna UBL...");

                // Asegurar que el namespace ext está declarado en la raíz
                if (root.getAttributeNode("xmlns:ext") == null) {
                    root.setAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:ext", NS_EXT);
                }

                // Crear la cuna: <ext:UBLExtensions><ext:UBLExtension><ext:ExtensionContent/>
                Element ublExtensions   = doc.createElementNS(NS_EXT, "ext:UBLExtensions");
                Element ublExtension    = doc.createElementNS(NS_EXT, "ext:UBLExtension");
                Element extensionContent = doc.createElementNS(NS_EXT, "ext:ExtensionContent");

                ublExtension.appendChild(extensionContent);
                ublExtensions.appendChild(ublExtension);

                // CRÍTICO UBL: las extensiones deben ser el PRIMER hijo del elemento raíz
                Node primerHijo = root.getFirstChild();
                if (primerHijo != null) {
                    root.insertBefore(ublExtensions, primerHijo);
                } else {
                    root.appendChild(ublExtensions);
                }

                // XPath al ExtensionContent donde DSS colocará el nodo Signature
                xpathDestino = "/*[local-name()='Invoice']"
                             + "/*[local-name()='UBLExtensions']"
                             + "/*[local-name()='UBLExtension']"
                             + "/*[local-name()='ExtensionContent']";

                System.out.println("Cuna UBL inyectada como primer hijo de Invoice.");
            } else {
                System.out.println("Documento no UBL. Se usará firma ENVELOPED estándar al final.");
            }

            // 5. Serializar el DOM (con o sin cuna) a bytes para pasárselo a DSS
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            Transformer tf = TransformerFactory.newInstance().newTransformer();
            tf.setOutputProperty(OutputKeys.ENCODING, "UTF-8");
            tf.setOutputProperty(OutputKeys.INDENT, "yes");
            tf.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "2");
            tf.transform(new DOMSource(doc), new StreamResult(baos));

            DSSDocument toSignDocument = new InMemoryDocument(
                    new ByteArrayInputStream(baos.toByteArray()),
                    "documento.xml"
            );

            // 6. Configurar los parámetros XAdES-BASELINE-B
            System.out.println("Configurando parámetros XAdES-BASELINE-B...");
            XAdESSignatureParameters parameters = new XAdESSignatureParameters();
            parameters.setSignatureLevel(SignatureLevel.XAdES_BASELINE_B);
            parameters.setSignaturePackaging(SignaturePackaging.ENVELOPED);
            parameters.setDigestAlgorithm(DigestAlgorithm.SHA256);
            parameters.setSigningCertificate(privateKey.getCertificate());
            parameters.setCertificateChain(privateKey.getCertificateChain());

            // Si es UBL, indicar a DSS el XPath exacto donde debe colocar la firma.
            // En DSS 6.4 se usan dos métodos separados:
            //   setXPathLocationString  → el XPath del nodo contenedor destino
            //   setXPathElementPlacement → XPathFirstChildOf (insertar como primer hijo)
            if (xpathDestino != null) {
                parameters.setXPathLocationString(xpathDestino);
                parameters.setXPathElementPlacement(
                    XAdESSignatureParameters.XPathElementPlacement.XPathFirstChildOf
                );
                System.out.println("Destino de la firma establecido en: " + xpathDestino);
            }

            // 7. Ejecutar la firma
            System.out.println("Generando estructura criptográfica de la firma...");
            CommonCertificateVerifier certificateVerifier = new CommonCertificateVerifier();
            XAdESService service = new XAdESService(certificateVerifier);

            ToBeSigned dataToSign = service.getDataToSign(toSignDocument, parameters);
            SignatureValue signatureValue = tokenConnection.sign(dataToSign, parameters.getDigestAlgorithm(), privateKey);
            DSSDocument signedDocument = service.signDocument(toSignDocument, parameters, signatureValue);

            // 8. Guardar el XML firmado en disco
            System.out.println("Escribiendo el archivo firmado en disco...");
            try (FileOutputStream fos = new FileOutputStream(new File(rutaXmlDestino))) {
                signedDocument.writeTo(fos);
            }

            System.out.println("====================================================");
            System.out.println("¡FACTURA FIRMADA Y GUARDADA CON ÉXITO!");
            System.out.println("====================================================");
            System.exit(0);

        } catch (Exception e) {
            System.err.println("====================================================");
            System.err.println("ERROR CRÍTICO DURANTE EL PROCESO DE FIRMA:");
            System.err.println("Mensaje: " + e.getMessage());
            e.printStackTrace();
            System.err.println("====================================================");
            System.exit(2);

        } finally {
            if (tokenConnection != null) {
                try { tokenConnection.close(); } catch (Exception e) { /* ignorar */ }
            }
            if (fis != null) {
                try { fis.close(); } catch (Exception e) { /* ignorar */ }
            }
        }
    }

    private static String cambiarExtensionALog(String rutaFichero) {
        int posicionPunto = rutaFichero.lastIndexOf('.');
        if (posicionPunto != -1) {
            return rutaFichero.substring(0, posicionPunto) + ".log";
        }
        return rutaFichero + ".log";
    }
}