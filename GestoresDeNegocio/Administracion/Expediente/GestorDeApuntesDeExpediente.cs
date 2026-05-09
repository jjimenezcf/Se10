using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Expediente;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Expediente
{
    public class GestorDeApuntesDeExpediente : GestorDeElementos<ContextoSe, ApunteDeExpedienteDtm, ApunteDeExpedienteDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrApuntes
        {
        }

        public class MapearApuntes : Profile
        {
            public MapearApuntes()
            {
                CreateMap<ApunteDeExpedienteDtm, ApunteDeExpedienteDto>()
                .ForMember(dto => dto.Elemento, x => x.MapFrom(dtm => dtm.Elemento != null ? dtm.Elemento.Expresion : null))
                .ForMember(dto => dto.Naturaleza, x => x.MapFrom(dtm => dtm.Naturaleza != null ? dtm.Naturaleza.Expresion : null));
                CreateMap<ApunteDeExpedienteDto, ApunteDeExpedienteDtm>()
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore());
            }
        }

        public GestorDeApuntesDeExpediente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeApuntesDeExpediente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeApuntesDeExpediente(contexto, mapeador);
        }

        protected override IQueryable<ApunteDeExpedienteDtm> AplicarJoins(IQueryable<ApunteDeExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            //consulta = consulta.Include(x => x.Elemento);
            consulta = consulta.Include(x => x.Naturaleza);
            return consulta;
        }

        protected override void AntesDePersistir(ApunteDeExpedienteDtm apunte, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(apunte, parametros);
            if (parametros.Modificando)
            {
                if (apunte.IdArchivo is null) apunte.IdArchivo = ((ApunteDeExpedienteDtm)parametros.registroEnBd).IdArchivo;
                if (apunte.PropiedadCambiada<int?>(nameof(ApunteDeExpedienteDtm.IdArchivo), parametros))
                {
                    var idArchivoAnterior = ((ApunteDeExpedienteDtm)parametros.registroEnBd).IdArchivo.Entero();
                    if (idArchivoAnterior > 0 && GestorDeVinculos.Existe(Contexto, enumNegocio.Expediente, enumNegocio.Archivos, apunte.IdElemento, idArchivoAnterior))
                    {
                        var expediente = apunte.DetalleDe<ExpedienteDtm>(Contexto);
                        expediente.QuitarAnexado(Contexto, idArchivoAnterior);
                    }
                }
            }

            if (parametros.Eliminando)
            {
                var idArchivoAnterior = ((ApunteDeExpedienteDtm)parametros.registroEnBd).IdArchivo.Entero();
                if (idArchivoAnterior > 0 && GestorDeVinculos.Existe(Contexto, enumNegocio.Expediente, enumNegocio.Archivos, apunte.IdElemento, idArchivoAnterior))
                {
                    var expediente = apunte.DetalleDe<ExpedienteDtm>(Contexto);
                    expediente.QuitarAnexado(Contexto, idArchivoAnterior);
                }
            }
        }

        protected override void DespuesDePersistir(ApunteDeExpedienteDtm apunte, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(apunte, parametros);
            if (!parametros.Eliminando)
            {
                var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(apunte.IdArchivo.Entero(), errorSiNoHay: false);
                var actualizarNombreArchivo = false;
                var ordenCambiado = false;
                var ordenAnterior = 0;
                if (parametros.Insertando)
                {
                    actualizarNombreArchivo = archivo is not null;
                }
                else
                {
                    if (apunte.PropiedadCambiada<int?>(nameof(ApunteDeExpedienteDtm.IdArchivo), parametros) && apunte.IdArchivo.HasValue)
                    {
                        actualizarNombreArchivo = true;
                    }
                    if (apunte.PropiedadCambiada<int>(nameof(ApunteDeExpedienteDtm.Orden), parametros) && apunte.IdArchivo.HasValue)
                    {
                        ordenAnterior = ((ApunteDeExpedienteDtm)parametros.registroEnBd).Orden;
                        ordenCambiado = true;
                    }
                }

                if (archivo is not null && !GestorDeVinculos.Existe(Contexto, enumNegocio.Expediente, enumNegocio.Archivos, apunte.IdElemento, (int)apunte.IdArchivo))
                    GestorDeVinculos.Vincular(Contexto, enumNegocio.Expediente, enumNegocio.Archivos, apunte.IdElemento, (int)apunte.IdArchivo);

                if (actualizarNombreArchivo || ordenCambiado)
                {
                    archivo.Nombre = $"Jtf.{apunte.Orden}-{(ordenCambiado ? archivo.Nombre.Replace($"Jtf.{ordenAnterior}-", "") : archivo.Nombre)}".Left(249);
                    archivo.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                }
            }
        }

        protected override void EliminarCaches(ApunteDeExpedienteDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Exp_Ingresos, registro.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Exp_Gastos, registro.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Exp_Presupuestado, registro.IdElemento.ToString());
        }

        protected override void DespuesDeMapearElElemento(ApunteDeExpedienteDtm apunte, ApunteDeExpedienteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(apunte, elemento, parametros);

            var gestor = GestorDeArchivos.Gestor(Contexto, Contexto.Mapeador);
            if (parametros.LeerPorIdParaEditar)
            {
                if (apunte.IdArchivo.HasValue)
                {
                    elemento.GuidDeDescarga = gestor.RegistrarDescargaConGuid((int)apunte.IdArchivo, caducaEl: null, maximoDeDescargas: null, auditar: false);
                    elemento.NombreDeAccion = $"Descargar: {Contexto.SeleccionarPorId<ArchivoDtm>((int)apunte.IdArchivo).Nombre}";
                }
                else
                {
                    elemento.NombreDeAccion = $"No se ha indicado el justificante del gasto";
                }
            }
        }

    }
}
