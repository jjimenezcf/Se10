using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class GestorDeInscritosEnActividades : GestorDeElementos<ContextoSe, InscritosEnActividadDtm, InscritosEnActividadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrInscritos
        {
        }

        public class MapearInscritos : Profile
        {
            public MapearInscritos()
            {
                CreateMap<InscritosEnActividadDtm, InscritosEnActividadDto>()
                .ForMember(dto => dto.Elemento, x => x.MapFrom(dtm => dtm.Elemento != null ? dtm.Elemento.Expresion : null))
                .ForMember(dto => dto.Interlocutor, x => x.MapFrom(dtm => dtm.Interlocutor != null ? dtm.Interlocutor.Expresion : null));
                CreateMap<InscritosEnActividadDto, InscritosEnActividadDtm>()
                .ForMember(dtm => dtm.Interlocutor, dto => dto.Ignore());
            }
        }

        public GestorDeInscritosEnActividades(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeInscritosEnActividades Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeInscritosEnActividades(contexto, mapeador);
        }

        protected override IQueryable<InscritosEnActividadDtm> AplicarJoins(IQueryable<InscritosEnActividadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Interlocutor);
            return consulta;
        }

        protected override void AntesDePersistir(InscritosEnActividadDtm inscrito, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(inscrito, parametros);
            if (parametros.Modificando)
            {
                if (inscrito.IdArchivo is null) inscrito.IdArchivo = ((InscritosEnActividadDtm)parametros.registroEnBd).IdArchivo;
                if (inscrito.PropiedadCambiada<int?>(nameof(InscritosEnActividadDtm.IdArchivo), parametros))
                {
                    var idArchivoAnterior = ((InscritosEnActividadDtm)parametros.registroEnBd).IdArchivo.Entero();
                    if (idArchivoAnterior > 0 && GestorDeVinculos.Existe(Contexto, enumNegocio.CircuitoDoc, enumNegocio.Archivos, inscrito.IdElemento, idArchivoAnterior))
                    {
                        var expediente = inscrito.DetalleDe<CircuitoDocDtm>(Contexto);
                        expediente.QuitarAnexado(Contexto, idArchivoAnterior);
                    }
                }
            }

            if (parametros.Eliminando)
            {
                var idArchivoAnterior = ((InscritosEnActividadDtm)parametros.registroEnBd).IdArchivo.Entero();
                if (idArchivoAnterior > 0 && GestorDeVinculos.Existe(Contexto, enumNegocio.CircuitoDoc, enumNegocio.Archivos, inscrito.IdElemento, idArchivoAnterior))
                {
                    var expediente = inscrito.DetalleDe<CircuitoDocDtm>(Contexto);
                    expediente.QuitarAnexado(Contexto, idArchivoAnterior);
                }
            }
        }

        protected override void DespuesDePersistir(InscritosEnActividadDtm inscrito, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(inscrito, parametros);
            if (!parametros.Eliminando)
            {
                var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(inscrito.IdArchivo.Entero(), errorSiNoHay: false);
                var actualizarNombreArchivo = false;
                if (parametros.Insertando)
                {
                    actualizarNombreArchivo = archivo is not null;
                }
                else
                {
                    if (inscrito.PropiedadCambiada<int?>(nameof(InscritosEnActividadDtm.IdArchivo), parametros) && inscrito.IdArchivo.HasValue)
                    {
                        actualizarNombreArchivo = true;
                    }
                }

                if (archivo is not null && !GestorDeVinculos.Existe(Contexto, enumNegocio.CircuitoDoc, enumNegocio.Archivos, inscrito.IdElemento, (int)inscrito.IdArchivo))
                    GestorDeVinculos.Vincular(Contexto, enumNegocio.CircuitoDoc, enumNegocio.Archivos, inscrito.IdElemento, (int)inscrito.IdArchivo);

                if (actualizarNombreArchivo )
                {
                    archivo.Nombre = $"Jtf.{archivo.Nombre}".Left(249);
                    archivo.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                }
            }
        }

        protected override void EliminarCaches(InscritosEnActividadDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
        }

        protected override void DespuesDeMapearElElemento(InscritosEnActividadDtm inscrito, InscritosEnActividadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(inscrito, elemento, parametros);

            var gestor = GestorDeArchivos.Gestor(Contexto, Contexto.Mapeador);
            if (parametros.LeerPorIdParaEditar)
            {
                if (inscrito.IdArchivo.HasValue)
                {
                    elemento.GuidDeDescarga = gestor.RegistrarDescargaConGuid((int)inscrito.IdArchivo, caducaEl: null, maximoDeDescargas: null, auditar: false);
                    elemento.NombreDeAccion = $"Descargar: {Contexto.SeleccionarPorId<ArchivoDtm>((int)inscrito.IdArchivo).Nombre}";
                }
                else
                {
                    elemento.NombreDeAccion = $"No se ha indicado el justificante del pago";
                }
            }
        }

    }
}
