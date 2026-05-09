using System.Collections.Generic;
using GestoresDeNegocio.Terceros;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeProcuradores : DescriptorDeCrud<ProcuradorDto>
    {
        public DescriptorDeProcuradores(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(ProcuradoresController)
              , vista: nameof(ProcuradoresController.CrudProcuradores)
              , modo: modo
               , rutaBase: enumNameSpaceTs.Terceros)
        {

            Mnt.OrdenacionInicial = @$"{nameof(ProcuradorDto.Expresion)}:{nameof(ProcuradorDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos(ltrProcurador.Procurador, nameof(ProcuradorDto.Expresion), "Buscar por nif, apellido, nombre, mail, teléfono");

            var listaPersona = new ListasDinamicas<ProcuradorDto>(Mnt.BloqueGeneral,
                etiqueta: "Persona",
                filtrarPor: nameof(ProcuradorDto.IdPersona),
                ayuda: "seleccione una persona",
                seleccionarDe: nameof(PersonaDto),
                buscarPor: nameof(PersonaDto.Expresion),
                mostrarExpresion: $"[{nameof(PersonaDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PersonasController),
                navegarA: nameof(PersonasController.CrudPersonas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaPersona.LongitudMinimaParaBuscar = 1;

            var listaSociedad = new ListasDinamicas<ProcuradorDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                filtrarPor: nameof(ProcuradorDto.IdSociedad),
                ayuda: "seleccione una sociedad",
                seleccionarDe: nameof(SociedadDto),
                buscarPor: nameof(SociedadDto.Expresion),
                mostrarExpresion: $"[{nameof(SociedadDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 1),
                controlador: nameof(SociedadesController),
                navegarA: nameof(SociedadesController.CrudSociedades),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaSociedad.LongitudMinimaParaBuscar = 1;

            var listaInter = new ListasDinamicas<ProcuradorDto>(Mnt.BloqueGeneral,
                etiqueta: "Interlocutor",
                filtrarPor: nameof(ProcuradorDto.IdInterlocutor),
                ayuda: "seleccione un interlocutor",
                seleccionarDe: nameof(InterlocutorDto),
                buscarPor: nameof(InterlocutorDto.Expresion),
                mostrarExpresion: $"[{nameof(InterlocutorDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 2),
                controlador: nameof(InterlocutoresController),
                navegarA: nameof(InterlocutoresController.CrudInterlocutores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaSociedad.LongitudMinimaParaBuscar = 1;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Procuradores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeProcuradores('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                     ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
			return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }

    }
}

/*
 * 
                    
*/
