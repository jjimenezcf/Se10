using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Tarea;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Expediente;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace GestoresDeNegocio.Expediente
{

    public class GestorDeExpedientes : GestorDeElementos<ContextoSe, ExpedienteDtm, ExpedienteDto>, IImportadorDelCorreo, ITotalizador<TotalesDeExpedientes>
    {

        public class MapearExpediente : Profile
        {
            public MapearExpediente()
            {
                CreateMap<ExpedienteDtm, ExpedienteDto>()
                .DtmToDto()
                .ForMember(dto => dto.ValoradoEn, dtm => dtm.MapFrom(dtm => dtm.ValoradoEn))
                .ForMember(dto => dto.Horas, dtm => dtm.Ignore())
                .ForMember(dto => dto.Planificacion, dtm => dtm.Ignore())
                .ForMember(dto => dto.Ejecucion, dtm => dtm.Ignore());

                CreateMap<ExpedienteDto, ExpedienteDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.ValoradoEn, dto => dto.Ignore());
            }

        }


        public override enumNegocio Negocio => enumNegocio.Expediente;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeExpediente.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeExpedientes(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeExpedientes Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeExpedientes(contexto, mapeador);
        }


        protected override IQueryable<ExpedienteDtm> AplicarJoins(IQueryable<ExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Solicitante)
                .Include(x => x.Responsable);
            return consulta;
        }

        protected override IQueryable<ExpedienteDtm> AplicarFiltros(IQueryable<ExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            FiltrosDeExpedientes.FiltrarPorTipoDeExpedienteSegunDescriptor(Contexto, filtros, parametros.Parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;
                consulta = consulta.FiltrosDeClientes(Contexto, filtro);
                consulta = consulta.ExpedientesConPpts(Contexto, filtro);
                consulta = consulta.ExpedienteConContratos(Contexto, filtro);
                consulta = consulta.ExpedientesConValoracion(filtro);
                consulta = consulta.SeleccionarParaAsociarAUnContrato(filtro);
                consulta = consulta.SeleccionarParaAsociarAUnPpt(filtro);
                consulta = consulta.RegistrosEsRelacionados(Contexto, filtro);
            }
            return consulta;
        }

        protected override void AntesDePersistir(ExpedienteDtm expediente, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(expediente, parametros);
            expediente.ClaseDeExpediente = ((TipoDeExpedienteDtm)parametros.TipoConFujo).ClaseDeExpediente;

            if (expediente.ClaseDeExpediente == enumClaseDeExpediente.DeCliente)
            {
                var cliente = expediente.Solicitante(Contexto).Cliente(Contexto);
                if (cliente == null)
                {
                    if (expediente.Solicitante(Contexto).EsContacto)
                        GestorDeErrores.Emitir($"Un expediente de cliente necesita un solicitante que sea cliente y solicitante '{expediente.Solicitante(Contexto).Referencia(Contexto)}' es un contacto, un contacto nunca puede ser un cliente");

                    GestorDeErrores.Emitir($"Un expediente de cliente necesita un solicitante que sea cliente y solicitante '{expediente.Solicitante(Contexto).Referencia(Contexto)}' no es aun cliente, creelo en como cliente");
                }
                if (cliente.Baja)
                    GestorDeErrores.Emitir($"No se puede operar con el expediente por estar el cliente '{cliente.Referencia(Contexto)}' de baja");
            }

        }

        protected override void DespuesDePersistir(ExpedienteDtm expediente, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(expediente, parametros);
            if (parametros.Insertando)
            {
                expediente.CopiarDireccionDelSolicitante(Contexto, enumCalificadorDireccion.contacto);
                var solicitante = expediente.Solicitante(Contexto);

                if (expediente.ClaseDeExpediente == enumClaseDeExpediente.DeCliente && !solicitante.EsCliente(Contexto))
                    solicitante.CrearCliente(Contexto);

                if (expediente.ClaseDeExpediente == enumClaseDeExpediente.juridico || expediente.Tipo<TipoDeExpedienteDtm>(Contexto).UsaDatosJuridicos)
                {
                    new DatosJuridicosDtm { IdElemento = expediente.Id }.InsertarComoAdministrador(Contexto);
                }

            }
            var cliente = expediente.Solicitante(Contexto).Cliente(Contexto);
            if (cliente is not null) ServicioDeCaches.EliminarElemento(CacheDe.Ter_ExpedientesDeClientes, cliente.Id.ToString());
        }

        protected override ExpedienteDtm AntesDeTransitar(ExpedienteDtm expediente, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            expediente = base.AntesDeTransitar(expediente, transicion, parametros);

            if (expediente.ClaseDeExpediente == enumClaseDeExpediente.DeCliente)
            {
                var cliente = expediente.Solicitante(Contexto).Cliente(Contexto);
                if (cliente.Baja)
                    GestorDeErrores.Emitir($"No se puede aplicar la transición '{transicion.Nombre}' en '{expediente.Referencia}' por estar el cliente '{cliente.Referencia(Contexto)}' de baja");
            }

            if (transicion.EntreEtapas(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Estados(), enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Estados()))
                expediente.AntesDeIniciar(Contexto);

            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos) && transicion.EsCancelado)
                expediente.AntesDeCancelar(Contexto);

            if (transicion.EsTerminado)
                expediente.AntesDeTerminar(Contexto);

            return expediente;
        }

        protected override ExpedienteDtm DespuesDeTransitar(ExpedienteDtm expediente, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            expediente = base.DespuesDeTransitar(expediente, transicion, parametros);
            if (transicion.EntreEtapas(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Estados(), enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Estados()))
                expediente.TrasIniciar(Contexto);

            return expediente;
        }

        public static void PersistirDatosJuridicos(ContextoSe contexto, DatosJuridicosDtm datosJuridicos)
        {
            //creo el servicio de datos extendidos
            //busco si existen los datos para jurídicos para este expediente
            //si existen los modifico
            //si no los creo
        }

        protected override void DespuesDeMapearElElemento(ExpedienteDtm expediente, ExpedienteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(expediente, elemento, parametros);

            var tipo = Contexto.SeleccionarPorId<TipoDeExpedienteDtm>(expediente.IdTipo);
            elemento.UsaPpts = tipo.UsaPpts;
            elemento.UsaTareas = tipo.UsaTareas;
            elemento.ScDeVenta = tipo.ScDeVenta;
            elemento.ScDeCompra = tipo.ScDeCompra;
            elemento.UsaDatosJuridicos = tipo.UsaDatosJuridicos;

            if (parametros.CargarLista == true)
                return;

            if (parametros.Parametros.ContainsKey(ltrParametrosNeg.Peticion) && (enumPeticion)parametros.Parametros[ltrParametrosNeg.Peticion] == enumPeticion.epLeerPorId)
                elemento.Etapas = expediente.Lista();

            if (elemento.UsaTareas)
            {
                var datos = expediente.Tiempos(Contexto);
                elemento.Horas = datos.Horas;
                elemento.Planificacion = datos.Planificacion;
                elemento.Ejecucion = datos.Ejecucion;
            }

            if (ModoDeAcceso.SoyGestor(elemento.ModoDeAcceso))
            {
                if (expediente.ClaseDeExpediente == enumClaseDeExpediente.DeCliente)
                {
                    var cliente = expediente.Solicitante(Contexto).Cliente(Contexto);
                    if (cliente.Baja)
                    {
                        elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                        elemento.informacion = $"El cliente '{cliente.Referencia(Contexto)}' está de baja, por tanto no es editable";
                    }
                }
            }

            if (parametros.LeerDatosParaElGridOParaExportar)
            {
                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Gastos).ToLower()))
                    elemento.Gastos = expediente.Gastos(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Ingresos).ToLower()))
                    elemento.Ingresos = expediente.Ingresos(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Pagado).ToLower()))
                    elemento.Pagado = expediente.Pagos(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Cobrado).ToLower()))
                    elemento.Cobrado = expediente.Cobros(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Margen).ToLower()))
                    elemento.Margen = elemento.Ingresos == 0 ? 0 : 100 * (elemento.Ingresos - elemento.Gastos) / elemento.Ingresos;

                if (parametros.ColumnasDelGrid.Contains(nameof(ExpedienteDto.Rentabilidad).ToLower()))
                    elemento.Rentabilidad = elemento.Gastos == 0 ? 0 : 100 * (elemento.Ingresos - elemento.Gastos) / elemento.Gastos;
            }
        }

        protected override IQueryable<ExpedienteDtm> AplicarSeguridad(IQueryable<ExpedienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<ExpedienteDtm, TipoDeExpedienteDtm, PermisoDelExpedienteDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<ExpedienteDtm, PermisoDelExpedienteDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void HistorialDeUnElemento(IElementoDeProcesoDtm expediente, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros)
        {
            base.HistorialDeUnElemento(expediente, sucesos, filtros);
            ((ExpedienteDtm)expediente).HistorialDePresupuestos(Contexto, sucesos, filtros);
        }

        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (expediente.ClaseDeExpediente == enumClaseDeExpediente.DeCliente && vinculado == enumNegocio.Archivador && entorno.Parametros.LeerValor(ltrCliente.OtorgarPermisos, false))
            {
                var contexto = entorno.Contexto;
                var cliente = expediente.Solicitante(contexto).Cliente(contexto);
                if (!cliente.Baja)
                {
                    var usuariosDeCliente = cliente.Detalles<UsuarioDeClienteDtm>(contexto, aplicarJoin: true).Where(x => x.Usuario.Activo);
                    if (usuariosDeCliente.Count() == 1)
                    {
                        List<int> usuarios = new List<int>();
                        foreach (var usuarioDeCliente in usuariosDeCliente.Where(x => x.Usuario.Activo)) usuarios.Add(usuarioDeCliente.IdUsuario);
                        GestorDePemisosDelElemento.OtorgarPermisoDe(contexto, vinculado, entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)), usuarios, enumModoDeAccesoDeDatos.Gestor);
                    }
                }
            }

            if (vinculado == enumNegocio.Tarea)
            {
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Tareas, expediente.Id.ToString());
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));

            if (vinculado == enumNegocio.Tarea)
            {
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Tareas, expediente.Id.ToString());
            }
        }


        public static void ValidarPuedeImputarFacturas(ContextoSe contexto, List<int> ids)
        {
            if (ids.Count != 1) GestorDeErrores.Emitir(ids.Count == 0 ? "No ha de indicar el expediente al que se quiere imputar las facturas" : $"Solo se pueden imputar facturas a un expediente, ha seleccionado {ids.Count} expedientes");
            var expediente = contexto.SeleccionarPorId<ExpedienteDtm>(ids[0]);

            if (!expediente.EsInterventor<TipoDeExpedienteDtm>(contexto))
                GestorDeErrores.Emitir($"Ha de ser interventor del expediente '{expediente.Referencia}' para poder imputarle facturas");

            if (!expediente.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeExpedientes> { enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enumEtapasDeExpedientes.EXP_Etapa_Terminada }))
                GestorDeErrores.Emitir($"El expediente '{expediente.Referencia}' debe estar estar en la etapa de '{enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Nombre()}' o '{enumEtapasDeExpedientes.EXP_Etapa_Terminada.Nombre()}' para poder imputarle facturas");
        }


        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var expediente = (ExpedienteDtm)ExtensorDeElementosDeUnProceso.NuevoDtm(Negocio.TipoDtm(), idCg, idTipo, nombre, descripcion, parametros);
            return expediente;
        }

        public Task<TotalesDeExpedientes> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDeExpedientes ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var expedientes = Contexto.SeleccionarTodos<ExpedienteDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });
            var totales = new TotalesDeExpedientes();
            totales.Presupuestado = expedientes.Sum(exp => exp.PresupuestadoEn(Contexto));
            totales.Gastos = expedientes.Sum(exp => exp.Gastos(Contexto));
            totales.Ingresos = expedientes.Sum(exp => exp.Ingresos(Contexto));
            totales.Pagos = expedientes.Sum(exp => exp.Pagos(Contexto));
            totales.Cobros = expedientes.Sum(exp => exp.Cobros(Contexto));
            var diferencia = 100 * (totales.Ingresos - totales.Gastos);
            totales.Margen = totales.Ingresos == 0 ? 0 : diferencia / totales.Ingresos;
            totales.Rentabilidad = totales.Gastos == 0 ? 0 : diferencia / totales.Gastos;

            var listaDeIdsDeTareas = expedientes
                .SelectMany(exp => exp.Tareas(Contexto))
                .Select(tarea => tarea.Id)
                .ToList().Distinct();

            var ids = string.Join(Simbolos.separadorDeEnteros, listaDeIdsDeTareas);
            var filtro = new ClausulaDeFiltrado
            {
                Clausula = nameof(IRegistro.Id),
                Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                Valor = ids
            };

            totales.Totales = GestorDeTareas.Gestor(Contexto, Contexto.Mapeador).ObtenerTotales(new List<ClausulaDeFiltrado> { filtro }, 0, -1).Totales;

            totales.Procesados = expedientes.Count();
            return totales;
        }
    }


}
