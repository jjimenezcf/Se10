using AutoMapper;
using ServicioDeDatos;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;
using GestorDeElementos;
using Utilidades;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using System.IO;
using System.Collections.Generic;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeMiCorreo :  GestorDeElementos<ContextoSe, MiCorreoDtm, MiCorreoDto>
    {

        public class ltrMiCorreo
        {
        }

        public class MapearMiCorreo : Profile
        {
            public MapearMiCorreo()
            {
                CreateMap<MiCorreoDtm, MiCorreoDto>();
                CreateMap<MiCorreoDto, MiCorreoDtm>();
            }
        }

        public GestorDeMiCorreo(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)  
        {
        }

        public static GestorDeMiCorreo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeMiCorreo(contexto, mapeador);
        }

        protected override void DespuesDePersistir(MiCorreoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Procesados);
            cache[registro.IdMensaje] = true;
        }

        public static void AdjuntarCuerpoHtml(ContextoSe contexto, enumNegocio negocio, INombre elemento, MiCorreoDto correo, List<string> rutas)
        {
            var fichero = Path.Combine(enumRutas.RutaDeDescarga, ExtensorDeMiCorreo.NombreFicheroCurpoHtml);
            
            correo.CuerpoHtml = extHtml.ReemplazarImagenesEnHtml(correo.CuerpoHtml, rutas);

            File.WriteAllText(fichero, $"<div>Usuario incorporador: {contexto.DatosDeConexion.Login} </div>" + correo.CuerpoHtml);
            var archivo = ServidorDocumental.AnexarArchivo(contexto, negocio, elemento.Id, fichero, copiar: false, quitarExtensionHtml: false, sanitizar: true);
            ServidorDocumental.BloquearArchivo(contexto, negocio.IdNegocio(), elemento.Id, archivo.Id, ltrDeUnArchivo.CorreoImportado);
        }

    }
}

        /*
         * // Obtener todas las MiCorreo cuya propiedad Ruta no sea nula
     List<MiCorreoDtm> MiCorreoConRuta = ObtenerMiCorreoConRutaNoNula();
     
     foreach (var MiCorreo in MiCorreoConRuta)
     {
         // Obtener los eventos que cumplen con el rango de fechas
         List<EventoDeMiCorreoDtm> eventos = ObtenerEventosPorRangoDeFechas(MiCorreo, DateTime.Today.AddDays(-30), DateTime.Today.AddMonths(3));
     
         // Generar el archivo XML para los eventos
         string xmlContent = GenerarXMLFromEventos(eventos);
     
         // Publicar el archivo XML en línea para su suscripción
         string xmlFilePath = "path/to/your/calendar_" + MiCorreo.Nombre + ".xml";
         File.WriteAllText(xmlFilePath, xmlContent);
     
         // Proporcionar el enlace al archivo XML para su suscripción
         string xmlUrl = "https://example.com/path/to/your/calendar_" + MiCorreo.Nombre + ".xml";
     }




    // Método para generar el archivo XML a partir de los eventos
    public static string GenerarXMLFromEventos(List<EventoDeMiCorreoDtm> eventos)
    {
        XNamespace ns = "http://www.example.com/MiCorreo"; // Reemplaza con el namespace adecuado

        // Crear el elemento raíz del documento XML
        XElement root = new XElement(ns + "Eventos");

        // Agregar cada evento como un elemento al documento XML
        foreach (var evento in eventos)
        {
            XElement eventoXml = new XElement(ns + "Evento",
                new XElement(ns + "Id", evento.IdMiCorreo),
                new XElement(ns + "Inicio", evento.Inicio),
                new XElement(ns + "Fin", evento.Fin),
                // Agregar otras propiedades del evento como elementos XML
            );

            root.Add(eventoXml);
        }

        // Crear el documento XML con el elemento raíz y el namespace
        XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);

        // Devolver el contenido del documento XML como una cadena
        return xmlDocument.ToString();
    }

        // Instalar el paquete NuGet "Google.Apis.Calendar.v3"
// Importar los espacios de nombres necesarios

public void AgregarEventosAGoogleCalendar(List<EventoDeMiCorreoDtm> eventos)
{
    // Autenticación con la API de Google Calendar
    UserCredential credential;
    using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
    {
        string credPath = "token.json";
        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
            new[] { CalendarService.Scope.Calendar },
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;
    }

    // Crear el servicio de la API de Google Calendar
    var service = new CalendarService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "Nombre de tu aplicación",
    });

    // Agregar cada evento a Google Calendar
    foreach (var evento in eventos)
    {
        Event googleEvent = new Event()
        {
            Summary = evento.Descripcion,
            Location = "Ubicación del evento",
            Start = new EventDateTime()
            {
                DateTime = evento.Inicio,
                TimeZone = "Tu zona horaria",
            },
            End = new EventDateTime()
            {
                DateTime = evento.Fin,
                TimeZone = "Tu zona horaria",
            },
        };

        // Insertar el evento en Google Calendar
        string calendarId = "primary"; // ID del calendario
        EventsResource.InsertRequest request = service.Events.Insert(googleEvent, calendarId);
        Event createdEvent = request.Execute();
    }
}

        // Instalar el paquete NuGet "Microsoft.Graph"
// Importar los espacios de nombres necesarios

public async Task AgregarEventosAOffice365(List<EventoDeMiCorreoDtm> eventos)
{
    // Autenticación con la API de Microsoft Graph
    IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create("YourClientId")
        .WithClientSecret("YourClientSecret")
        .WithAuthority(new Uri("https://login.microsoftonline.com/YourTenantId"))
        .Build();

    string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
    AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

    // Crear el cliente de la API de Microsoft Graph
    GraphServiceClient graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
    {
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        return Task.FromResult(0);
    }));

    // Agregar cada evento a Office 365
    foreach (var evento in eventos)
    {
        Event newEvent = new Event
        {
            Subject = evento.Descripcion,
            Start = new DateTimeTimeZone
            {
                DateTime = evento.Inicio.ToString("o"),
                TimeZone = "Tu zona horaria",
            },
            End = new DateTimeTimeZone
            {
                DateTime = evento.Fin.ToString("o"),
                TimeZone = "Tu zona horaria",
            },
        };

        await graphClient.Me.Events.Request().AddAsync(newEvent);
    }
}

*/
