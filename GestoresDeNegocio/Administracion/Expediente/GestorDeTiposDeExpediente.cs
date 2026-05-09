using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using Gestor.Errores;
using ServicioDeDatos.Expediente;
using ModeloDeDto.Expediente;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;

namespace GestoresDeNegocio.Expediente
{
    public class GestorDeTiposDeExpediente : GestorDeTiposDeElemento<ContextoSe, TipoDeExpedienteDtm, TipoDeExpedienteDto>
    {
        public class ltrDeUnTipoDeExpediente
        {

        }

        public class MapearTipoDeExpediente : MapearTipoDeElemento
        {
            public MapearTipoDeExpediente()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeExpedienteDtm, TipoDeExpedienteDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Expediente))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeExpedienteDto, TipoDeExpedienteDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }

        public GestorDeTiposDeExpediente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Expediente)
        {

        }

        public static GestorDeTiposDeExpediente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeExpediente(contexto, mapeador);
        }

        public static TipoDeExpedienteDtm PersistirTipo(ContextoSe contexto, enumClaseDeExpediente clsExpediente, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool usaPpts, bool scDeVenta, bool scDeCompra, bool usaDatosJuridicos)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeExpedienteDtm();
                tipo.ClaseDeExpediente = clsExpediente;
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.UsaTareas = true;
                tipo.UsaPpts = usaPpts;
                tipo.ScDeVenta = scDeVenta;
                tipo.ScDeCompra = scDeCompra;
                tipo.UsaDatosJuridicos = usaDatosJuridicos;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.ClaseDeExpediente != clsExpediente || leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro || leido.IdEstado != idEstado)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; leido.IdEstado = idEstado;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeExpedienteDtm> AplicarJoins(IQueryable<TipoDeExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.Estado);
        }

        protected override IQueryable<TipoDeExpedienteDtm> AplicarFiltros(IQueryable<TipoDeExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroParaSeleccionarTipo(parametros.Parametros);
            return consulta;
        }

        protected override void AntesDePersistir(TipoDeExpedienteDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeElemento, parametros);
        }

        protected override int ValidarNoHayElementos(TipoDeExpedienteDtm tipoDeExpediente, ParametrosDeNegocio parametros)
        {
            var cantidad =  base.ValidarNoHayElementos(tipoDeExpediente, parametros);
            if (tipoDeExpediente.ClaseDeExpediente == enumClaseDeExpediente.ConValoracion)
                tipoDeExpediente.UsaPpts = true;

            if (tipoDeExpediente.ClaseDeExpediente == enumClaseDeExpediente.juridico)
            {
                tipoDeExpediente.UsaDatosJuridicos = true;
                tipoDeExpediente.UsaPpts = true;
            }

            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDeExpedienteDtm)parametros.registroEnBd).ClaseDeExpediente != tipoDeExpediente.ClaseDeExpediente)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDeExpediente.Nombre}' ya que tiene elementos asociados");

                    if (tipoDeExpediente.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeExpedienteDtm.UsaTareas)))
                    {
                        if(VinculoSql.ContarVinculosConElTipo(Contexto,typeof(ExpedienteDtm), enumNegocio.Tarea,tipoDeExpediente.Id)>0)
                            GestorDeErrores.Emitir($"No se puede indicar que el tipo '{tipoDeExpediente.Nombre}' ya no usa tareas, ya que tiene tareas asociadas");
                    }

                    if (tipoDeExpediente.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeExpedienteDtm.UsaPpts)))
                    {
                      var expedientes =  Contexto.Set<ExpedienteDtm>().Where(x => x.IdTipo == tipoDeExpediente.Id &&
                         Contexto.Set<PresupuestoDtm>().Any(y => y.IdExpediente == x.Id));

                        if (expedientes.Count() > 0)
                            GestorDeErrores.Emitir($"No se puede indicar que el tipo '{tipoDeExpediente.Nombre}' ya no usa ppts, ya que tiene ppts asociados");
                    }

                    if (tipoDeExpediente.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeExpedienteDtm.UsaDatosJuridicos)))
                    {
                        var expedientes = Contexto.Set<ExpedienteDtm>().Where(x => x.IdTipo == tipoDeExpediente.Id);

                        if (expedientes.Count() > 0)
                            GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDeExpediente.Nombre}' el flag de si usa datos jurídicos, ya que ya hay expedientes de este tipo creados");
                    }
                    if (tipoDeExpediente.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeExpedienteDtm.ScDeVenta)))
                    {
                        var expedientes = Contexto.Set<ExpedienteDtm>().Where(x => x.IdTipo == tipoDeExpediente.Id &&
                           Contexto.Set<ContratoDtm>().Any(y => y.IdExpediente == x.Id));

                        if (expedientes.Count() > 0)
                            GestorDeErrores.Emitir($"No se puede indicar que el tipo '{tipoDeExpediente.Nombre}' ya no usa contrato, ya que tiene contratos de venta asociados");
                    }

                    if (tipoDeExpediente.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeExpedienteDtm.ScDeCompra)))
                    {
                        var expedientes = Contexto.Set<ExpedienteDtm>().Where(x => x.IdTipo == tipoDeExpediente.Id &&
                           Contexto.Set<ContratoDtm>().Any(y => y.IdExpediente == x.Id));

                        if (expedientes.Count() > 0)
                            GestorDeErrores.Emitir($"No se puede indicar que el tipo '{tipoDeExpediente.Nombre}' ya no usa contrato, ya que tiene contratos de compra asociados");
                    }
                }
            }
            return cantidad;
        }

        protected override void DespuesDePersistir(TipoDeExpedienteDtm tipo, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(tipo, parametros);
            if (parametros.Modificando)
            {
                if (tipo.UsaPpts && !((TipoDeExpedienteDtm)parametros.registroEnBd).UsaPpts)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaPpts), propagarALosHijos: true, propagarALosPadres: false, tipo.UsaPpts);

                if (!tipo.UsaPpts && ((TipoDeExpedienteDtm)parametros.registroEnBd).UsaPpts)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaPpts), propagarALosHijos: false, propagarALosPadres: true, tipo.UsaPpts);


                if (tipo.UsaDatosJuridicos && !((TipoDeExpedienteDtm)parametros.registroEnBd).UsaDatosJuridicos)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaDatosJuridicos), propagarALosHijos: true, propagarALosPadres: false, tipo.UsaDatosJuridicos);


                if (!tipo.UsaDatosJuridicos && ((TipoDeExpedienteDtm)parametros.registroEnBd).UsaDatosJuridicos)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaDatosJuridicos), propagarALosHijos: false, propagarALosPadres: true, tipo.UsaDatosJuridicos);


                if (tipo.UsaTareas && !((TipoDeExpedienteDtm)parametros.registroEnBd).UsaTareas)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaTareas), propagarALosHijos: true, propagarALosPadres: false, tipo.UsaTareas);

                if (!tipo.UsaTareas && ((TipoDeExpedienteDtm)parametros.registroEnBd).UsaTareas)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.UsaTareas), propagarALosHijos: false, propagarALosPadres: true, tipo.UsaTareas);

                if (tipo.ScDeVenta && !((TipoDeExpedienteDtm)parametros.registroEnBd).ScDeVenta)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.ScDeVenta), propagarALosHijos: true, propagarALosPadres: false, tipo.ScDeVenta);

                if (!tipo.ScDeVenta && ((TipoDeExpedienteDtm)parametros.registroEnBd).ScDeVenta)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.ScDeVenta), propagarALosHijos: false, propagarALosPadres: true, tipo.ScDeVenta);

                if (tipo.ScDeCompra && !((TipoDeExpedienteDtm)parametros.registroEnBd).ScDeCompra)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.ScDeCompra), propagarALosHijos: true, propagarALosPadres: false, tipo.ScDeCompra);

                if (!tipo.ScDeCompra && ((TipoDeExpedienteDtm)parametros.registroEnBd).ScDeCompra)
                    PropagarFlag(tipo, nameof(TipoDeExpedienteDtm.ScDeCompra), propagarALosHijos: false, propagarALosPadres: true, tipo.ScDeCompra);

            }
        }

    }
}
