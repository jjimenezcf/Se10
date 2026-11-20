using System.Text.Json;

namespace TestFacturador;

// Opción "Definición de las llamadas": enseña cómo debe montar un cliente externo cada petición
// del facturador y qué JSON devuelve. No ejecuta nada contra el servidor.
// El apiKey se muestra siempre como XXXXXXXX para poder copiar el texto y enviárselo a un tercero.
public static class DefinicionDeLlamadas
{
    private const string ApiKey = "XXXXXXXX";
    private const string NumeroFactura = "2026-S-123";
    private const string GuidPdf = "6f9619ff-8b86-d011-b42d-00c04fc964ff";
    private const string GuidXml = "7c9e6679-7425-40de-944b-e07fc1f90ae7";
    private const string NifCliente = "B73954455";
    private const string GuidOperacional = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    public static void Mostrar(Uri urlBase, string nif, JsonSerializerOptions jsonOpciones)
    {
        var baseUrl = urlBase.ToString().TrimEnd('/');
        var llamadas = new (string Titulo, Action Mostrar)[]
        {
            ("Convenciones comunes (formato de respuesta y errores)", () => Convenciones(baseUrl, nif)),
            ("Crear cliente            POST epCrearCliente", () => CrearCliente(baseUrl, nif, jsonOpciones)),
            ("Crear factura (directo)  POST epCrearFactura", () => CrearFactura(baseUrl, nif, jsonOpciones)),
            ("Crear factura (2 pasos)  GET epSolicitarFacturador + POST epCrearFacturaConGuid", () => CrearFacturaConGuid(baseUrl, nif, jsonOpciones)),
            ("Solicitar PDF            GET epSolicitarPdf", () => SolicitarDocumento(baseUrl, nif, "epSolicitarPdf", GuidPdf, "GuidDeConsultaPdf")),
            ("Solicitar XML            GET epSolicitarXml", () => SolicitarDocumento(baseUrl, nif, "epSolicitarXml", GuidXml, "GuidDeConsultaXml")),
            ("Descargar el fichero     GET epDescargaConGuid", () => Descargar(baseUrl)),
            ("Rectificar por datos erróneos  POST epRectificarPorDe", () => Rectificar(baseUrl)),
        };

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("--- Definición de las llamadas ---");
            for (var i = 0; i < llamadas.Length; i++)
                Console.WriteLine($"{i + 1}. {llamadas[i].Titulo}");
            Console.WriteLine("T. Todas");
            Console.WriteLine("0. Volver al menú principal");
            Console.Write("Opción: ");
            var opcion = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (opcion == "0")
                return;

            if (opcion == "T")
            {
                foreach (var llamada in llamadas)
                {
                    Cabecera(llamada.Titulo);
                    llamada.Mostrar();
                }
                continue;
            }

            if (int.TryParse(opcion, out var n) && n >= 1 && n <= llamadas.Length)
            {
                Cabecera(llamadas[n - 1].Titulo);
                llamadas[n - 1].Mostrar();
            }
            else
                Console.WriteLine("Opción no válida.");
        }
    }

    private static void Cabecera(string titulo)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 78));
        Console.WriteLine(titulo);
        Console.WriteLine(new string('=', 78));
    }

    private static void Seccion(string titulo)
    {
        Console.WriteLine();
        Console.WriteLine($"## {titulo}");
        Console.WriteLine();
    }

    private static string E(string valor) => Uri.EscapeDataString(valor);

    private static void Convenciones(string baseUrl, string nif)
    {
        Console.WriteLine($$"""
            - Url base: {{baseUrl}}  (en producción https://biwe.femdek.com)
            - No hay login: todas las llamadas se autentican con el apiKey del facturador de la sociedad emisora.
            - Los valores de la query string deben ir codificados (URL-encoded), p. ej. el número de factura.
            - El número de factura tiene el formato 'Año-Serie-Número' (p. ej. {{NumeroFactura}}).
            - El servidor responde SIEMPRE HTTP 200 con un JSON "sobre"; el éxito o el error se lee en "estado".
            - Los nombres de las propiedades de la respuesta vienen en camelCase. Se recomienda leerlas sin
              distinguir mayúsculas y minúsculas e ignorar las que no se conozcan.
            - Los JSON que se envían (body) deben usar exactamente los nombres de propiedad documentados:
              una propiedad desconocida provoca un error.
            """);

        Seccion("Respuesta correcta (estructura)");
        Console.WriteLine("""
            {
              "estado": "Ok",
              "mensaje": null,
              "consola": "Texto descriptivo del resultado",
              "total": 0,
              "datos": <depende de la llamada: texto u objeto>,
              "modoDeAcceso": "Consultor",
              "logout": false
            }
            """);

        Seccion("Respuesta con error");
        Console.WriteLine($$"""
            {
              "estado": "Error",
              "mensaje": "La sociedad '{{nif}}' no tiene ningún facturador para el ApiKey '{{ApiKey}}' proporcionado",
              "consola": "Detalle técnico del error",
              "total": 0,
              "datos": null,
              "modoDeAcceso": null,
              "logout": false
            }
            """);
        Console.WriteLine("""
            "mensaje" es el texto a mostrar al usuario; "consola" trae el detalle técnico.
            Excepción: en las llamadas que crean factura, si la prefactura llegó a crearse pero no se pudo emitir,
            "datos" trae igualmente el objeto de la factura y su campo "mensaje" explica qué falló.
            """);
    }

    private static void CrearCliente(string baseUrl, string nif, JsonSerializerOptions jsonOpciones)
    {
        var cliente = new ClienteJson
        {
            NIF = NifCliente,
            Nombre = "Cliente Ejemplo SL",
            eMail = "administracion@ejemplo.com",
            Telefono = "915559876",
            Municipio = "Madrid",
            CodigoPostal = "28001",
            TipoDeVia = "Calle",
            Calle = "Serrano",
            Numero = 45,
            ValidarEnLaAeat = false,
            SustituirDatosIdentificativos = true,
            SustituirDatosDeContacto = true,
            CrearCalleSiNoExiste = true,
            ValidarEnCatastro = true
        };
        var opciones = new JsonSerializerOptions(jsonOpciones) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

        Seccion("Petición");
        Console.WriteLine($"POST {baseUrl}/Facturador/epCrearCliente?nif={E(nif)}&apiKey={ApiKey}");
        Console.WriteLine("Content-Type: application/json");
        Console.WriteLine();
        Console.WriteLine(JsonSerializer.Serialize(cliente, opciones));
        Console.WriteLine();
        Console.WriteLine("""
            Persona física: añadir "Apellidos" e indicar en "Nombre" el nombre de pila.
            "TipoDeCliente" ("F"/"J") es opcional; si no se indica se deduce del formato del NIF.
            Los campos de dirección (Municipio, CodigoPostal, TipoDeVia, Calle, Numero) son opcionales en bloque.
            """);

        Seccion("Respuesta");
        Console.WriteLine($$"""
            {
              "estado": "Ok",
              "mensaje": null,
              "consola": "Cliente '({{NifCliente}}) Cliente Ejemplo SL' disponible para facturar",
              "total": 0,
              "datos": {
                "id": 4521,
                "nif": "{{NifCliente}}",
                "nombre": "({{NifCliente}}) Cliente Ejemplo SL"
              },
              "modoDeAcceso": "Consultor",
              "logout": false
            }
            """);
    }

    private static void CrearFactura(string baseUrl, string nif, JsonSerializerOptions jsonOpciones)
    {
        Seccion("Petición");
        Console.WriteLine($"POST {baseUrl}/Facturador/epCrearFactura?nif={E(nif)}&apiKey={ApiKey}");
        Console.WriteLine("Content-Type: application/json");
        Console.WriteLine();
        Console.WriteLine(JsonSerializer.Serialize(FacturaDeEjemplo.Construir(NifCliente), jsonOpciones));

        Seccion("Respuesta");
        Console.WriteLine(RespuestaDeFactura(baseUrl, "CrearFactura", NumeroFactura, GuidPdf, GuidXml, 456));
        NotasDeFactura();
    }

    private static void CrearFacturaConGuid(string baseUrl, string nif, JsonSerializerOptions jsonOpciones)
    {
        Seccion("Paso 1 - Petición: reservar un guid operacional");
        Console.WriteLine($"GET {baseUrl}/Facturador/epSolicitarFacturador?nifEmisor={E(nif)}&apiKey={ApiKey}&peticion=CrearFactura");

        Seccion("Paso 1 - Respuesta");
        Console.WriteLine($$"""
            {
              "estado": "Ok",
              "mensaje": null,
              "consola": "Solicitud de operación registrada correctamente",
              "total": 0,
              "datos": "{{GuidOperacional}}",
              "modoDeAcceso": "Consultor",
              "logout": false
            }
            """);
        Console.WriteLine("El guid de \"datos\" caduca al minuto, solo sirve para crear una factura y solo se puede usar una vez.");

        Seccion("Paso 2 - Petición: crear la factura con ese guid (sin apiKey)");
        Console.WriteLine($"POST {baseUrl}/Facturador/epCrearFacturaConGuid?nif={E(nif)}&guid={GuidOperacional}");
        Console.WriteLine("Content-Type: application/json");
        Console.WriteLine();
        Console.WriteLine(JsonSerializer.Serialize(FacturaDeEjemplo.Construir(NifCliente), jsonOpciones));

        Seccion("Paso 2 - Respuesta");
        Console.WriteLine(RespuestaDeFactura(baseUrl, "CrearFactura", NumeroFactura, GuidPdf, GuidXml, 456));
        NotasDeFactura();
    }

    private static void SolicitarDocumento(string baseUrl, string nif, string accion, string guid, string nombreDelGuid)
    {
        Seccion("Petición");
        Console.WriteLine($"GET {baseUrl}/Facturador/{accion}?nif={E(nif)}&apiKey={ApiKey}&numeroFactura={E(NumeroFactura)}&guid={guid}");
        Console.WriteLine();
        Console.WriteLine($"numeroFactura y guid ({nombreDelGuid}) son los recibidos al crear la factura. El guid no caduca.");

        Seccion("Respuesta");
        Console.WriteLine($$"""
            {
              "estado": "Ok",
              "mensaje": null,
              "consola": "Url de descarga generada correctamente",
              "total": 0,
              "datos": "{{baseUrl}}/Archivos/epDescargaConGuid?guid=9b2e1a34-1c2d-4e5f-8a9b-0c1d2e3f4a5b&id=789",
              "modoDeAcceso": "Consultor",
              "logout": false
            }
            """);
        Console.WriteLine("\"datos\" es una url de descarga válida durante 1 hora (ver la opción 'Descargar el fichero').");
    }

    private static void Descargar(string baseUrl)
    {
        Seccion("Petición");
        Console.WriteLine($"GET {baseUrl}/Archivos/epDescargaConGuid?guid=9b2e1a34-1c2d-4e5f-8a9b-0c1d2e3f4a5b&id=789");
        Console.WriteLine();
        Console.WriteLine("Es la url recibida en \"datos\" de epSolicitarPdf / epSolicitarXml, tal cual, sin apiKey.");

        Seccion("Respuesta");
        Console.WriteLine("""
            No es JSON: es el propio fichero (application/pdf o XML) con la cabecera Content-Disposition
            que indica el nombre del fichero. Pasada 1 hora hay que volver a pedir la url.
            """);
    }

    private static void Rectificar(string baseUrl)
    {
        Seccion("Petición");
        Console.WriteLine($"POST {baseUrl}/Facturador/epRectificarPorDe?apiKey={ApiKey}&numeroFactura={E(NumeroFactura)}");
        Console.WriteLine("Content-Type: text/plain");
        Console.WriteLine();
        Console.WriteLine("Se ha detectado un error en el NIF del cliente");
        Console.WriteLine();
        Console.WriteLine("El body es texto libre con el motivo. No lleva nif: se obtiene de la propia factura.");

        Seccion("Respuesta");
        Console.WriteLine(RespuestaDeFactura(baseUrl, "RectificarPorDe", "2026-R-7", "1a2b3c4d-5e6f-7890-abcd-ef1234567890", "0f1e2d3c-4b5a-6978-fedc-ba0987654321", 789));
        Console.WriteLine("Los datos devueltos son los de la RECTIFICATIVA creada, no los de la factura original.");
        NotasDeFactura();
    }

    private static string RespuestaDeFactura(string baseUrl, string peticion, string numero, string guidPdf, string guidXml, int idFactura) => $$"""
        {
          "estado": "Ok",
          "mensaje": null,
          "consola": "Factura emitida y sometido su envío",
          "total": 0,
          "datos": {
            "solicitadaEl": "2026-11-20T10:15:00",
            "peticion": "{{peticion}}",
            "facturador": "Nombre del facturador",
            "idFactura": {{idFactura}},
            "numeroFactura": "{{numero}}",
            "mensaje": "Factura emitida y sometido su envío",
            "guidDeConsultaPdf": "{{guidPdf}}",
            "guidDeConsultaXml": "{{guidXml}}",
            "urlDeLaFactura": "{{baseUrl}}/FacturaEmt/Consultar?id={{idFactura}}"
          },
          "modoDeAcceso": "Consultor",
          "logout": false
        }
        """;

    private static void NotasDeFactura()
    {
        Console.WriteLine();
        Console.WriteLine("""
            "datos" incluye además campos técnicos internos (id, rowVersion, estaCancelada...) que se deben ignorar.
            Guardar numeroFactura, guidDeConsultaPdf y guidDeConsultaXml: hacen falta para pedir el PDF/XML.
            "estado" es "Ok" si datos.mensaje es uno de estos textos:
              - Factura emitida y sometido su envío
              - Se ha sometido un lote de envío de factura, acceda a la aplicación para cercionar el envío
              - La factura no se ha enviado a la AEAT ya que la sociedad emisora no usa verifatu o el sistema no tiene activado el envío
            En cualquier otro caso "estado" es "Error" y datos.mensaje explica la causa.
            """);
    }
}
