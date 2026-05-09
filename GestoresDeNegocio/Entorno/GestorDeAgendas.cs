using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using Gestor.Errores;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeAgendas : GestorDeElementos<ContextoSe, AgendaDtm, AgendaDto>
    {
        public override enumNegocio Negocio => enumNegocio.Agenda;

        public class ltrAgendas
        {
        }

        public class MapearAgendas : Profile
        {
            public MapearAgendas()
            {
                CreateMap<AgendaDtm, AgendaDto>()
                .ForMember(x => x.Gestor, y => y.MapFrom(y => y.Gestor.Nombre))
                .ForMember(x => x.Consultor, y => y.MapFrom(y => y.Consultor.Nombre))
                .ForMember(x => x.Interventor, y => y.MapFrom(y => y.Interventor.Nombre));
                CreateMap<AgendaDto, AgendaDtm>()
                .ForMember(x => x.Gestor, y => y.Ignore())
                .ForMember(x => x.Consultor, y => y.Ignore())
                .ForMember(x => x.Interventor, y => y.Ignore());
            }
        }

        public GestorDeAgendas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAgendas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAgendas(contexto, mapeador);
        }

        protected override IQueryable<AgendaDtm> AplicarJoins(IQueryable<AgendaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Gestor);
            consulta = consulta.Include(x => x.Consultor);
            consulta = consulta.Include(x => x.Interventor);
            return consulta;
        }

        protected override IQueryable<AgendaDtm> AplicarSeguridad(IQueryable<AgendaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (ExtensorDeUsuarios.SePuedeParametrizar(Contexto)) return consulta;

            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!parametros.ValidarPermisosDePersistencia)
                return consulta;
            var permisosDeElUsuario = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(permisos => permisos.IdUsuario == Contexto.DatosDeConexion.IdUsuario);
            consulta = consulta.Where(agendas => permisosDeElUsuario.Any(p => p.IdPermiso == agendas.IdGestor || p.IdPermiso == agendas.IdConsultor || p.IdPermiso == agendas.IdInterventor));
            return consulta;
        }

        public override AgendaDtm PersistirRegistro(AgendaDtm registro, ParametrosDeNegocio parametros)
        {
            var agenda = base.PersistirRegistro(registro, parametros);
            if (parametros.Insertando)
            {
                agenda.Uri = agenda.GenerarUri().ToString();
                agenda = agenda.Modificar(Contexto, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
            }
            return agenda;
        }

        protected override void AntesDePersistir(AgendaDtm agenda, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(agenda, parametros);

            if (parametros.Insertando)
            {
                agenda.IdConsultor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.Agenda, $"{agenda.Nombre}", enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Consultor).Id;
                agenda.IdGestor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.Agenda, $"{agenda.Nombre}", enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Gestor).Id;
                agenda.IdInterventor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.Agenda, $"{agenda.Nombre}", enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Interventor).Id;
                agenda.Ics = agenda.NombreIcs(longitud: 100);
            }
            else if (parametros.Modificando)
            {
                agenda.IdConsultor = ((AgendaDtm)parametros.registroEnBd).IdConsultor;
                agenda.IdGestor = ((AgendaDtm)parametros.registroEnBd).IdGestor;
                agenda.IdInterventor = ((AgendaDtm)parametros.registroEnBd).IdInterventor;
                agenda.Uri = agenda.GenerarUri().ToString();
                agenda.Ics = agenda.NombreIcs(longitud: 100);
            }
            else if (parametros.Eliminando)
            {
                if (Contexto.SeleccionarPorFk<UsuarioDtm>(nameof(UsuarioDtm.IdAgenda), agenda.Id, errorSiNoHay: false) != null)
                {
                    GestorDeErrores.Emitir("No se pueden eliminar las agendas de usuario");
                }

                if (Contexto.SeleccionarPorFk<EventoDeAgendaDtm>(nameof(UsuarioDtm.IdAgenda), agenda.Id, errorSiNoHay: false, errorSiMasDeuno: false) != null)
                {
                    GestorDeErrores.Emitir("No se pueden eliminar la agenda por tener eventos");
                }
            }
        }

        protected override void DespuesDePersistir(AgendaDtm agenda, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(agenda, parametros);

            if (parametros.Modificando)
            {
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.Agenda, agenda.IdConsultor, agenda.Nombre, enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Consultor);
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.Agenda, agenda.IdGestor, agenda.Nombre, enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Gestor);
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.Agenda, agenda.IdInterventor, agenda.Nombre, enumClaseDePermiso.Agenda, enumModoDeAccesoDeDatos.Interventor);
                ((AgendaDtm)parametros.registroEnBd).EliminarAgendaIcs();
            }

            if (parametros.Eliminando)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, agenda.IdGestor, parametros: parametros.Parametros);
                GestorDePermisos.EliminarRegistroPorId(Contexto, agenda.IdConsultor, parametros: parametros.Parametros);
                GestorDePermisos.EliminarRegistroPorId(Contexto, agenda.IdInterventor, parametros: parametros.Parametros);
                ((AgendaDtm)parametros.registroEnBd).EliminarAgendaIcs();
            }
            else
            {
                agenda.GenerarAgendaIcs(Contexto);
            }


        }

        protected override void DespuesDeMapearElElemento(AgendaDtm registro, AgendaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.UrlDeAgenda = registro.UrlDeAgenda().ToString();
            elemento.Accion = registro.PuedeConsultarla(Contexto) ? "consultar" : "";
        }

    }
}

/*
 * // Obtener todas las agendas cuya propiedad Ruta no sea nula
List<AgendaDtm> agendasConRuta = ObtenerAgendasConRutaNoNula();

foreach (var agenda in agendasConRuta)
{
 // Obtener los eventos que cumplen con el rango de fechas
 List<EventoDeAgendaDtm> eventos = ObtenerEventosPorRangoDeFechas(agenda, DateTime.Today.AddDays(-30), DateTime.Today.AddMonths(3));

 // Generar el archivo XML para los eventos
 string xmlContent = GenerarXMLFromEventos(eventos);

 // Publicar el archivo XML en línea para su suscripción
 string xmlFilePath = "path/to/your/calendar_" + agenda.Nombre + ".xml";
 File.WriteAllText(xmlFilePath, xmlContent);

 // Proporcionar el enlace al archivo XML para su suscripción
 string xmlUrl = "https://example.com/path/to/your/calendar_" + agenda.Nombre + ".xml";
}




// Método para generar el archivo XML a partir de los eventos
public static string GenerarXMLFromEventos(List<EventoDeAgendaDtm> eventos)
{
XNamespace ns = "http://www.example.com/Agenda"; // Reemplaza con el namespace adecuado

// Crear el elemento raíz del documento XML
XElement root = new XElement(ns + "Eventos");

// Agregar cada evento como un elemento al documento XML
foreach (var evento in eventos)
{
    XElement eventoXml = new XElement(ns + "Evento",
        new XElement(ns + "Id", evento.IdAgenda),
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

public void AgregarEventosAGoogleCalendar(List<EventoDeAgendaDtm> eventos)
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

public async Task AgregarEventosAOffice365(List<EventoDeAgendaDtm> eventos)
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
