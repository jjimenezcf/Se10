using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Utilidades.Ampliaciones;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDePersonas : GestorDeElementos<ContextoSe, PersonaDtm, PersonaDto>
    {
        public override enumNegocio Negocio => enumNegocio.Persona;

        public class MapearPersonas : Profile
        {
            public MapearPersonas()
            {
                CreateMap<PersonaDtm, PersonaDto>();
                CreateMap<PersonaDto, PersonaDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore());
            }
        }

        public GestorDePersonas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDePersonas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePersonas(contexto, mapeador); ;
        }


        public static PersonaDto CrearPersona(ContextoSe contexto, string nif, string nombre, string apellidos, string email, string telefono, bool crearInterlocutor = true)
        {
            var parametros = new Dictionary<string, object> { { nameof(PersonaDto.CrearInterlocutor), crearInterlocutor } };
            var persona = CrearPersona(contexto, nif, nombre, apellidos, email, telefono, parametros);

            return persona.MapearDto<PersonaDto>(contexto);

        }

        private static PersonaDtm CrearPersona(ContextoSe contexto, string nif, string nombre, string apellidos, string email, string telefono, Dictionary<string, object> parametros)
        {
            var persona = contexto.SeleccionarPorPropiedad<PersonaDtm>(nameof(PersonaDtm.NIF), nif, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });
            if (persona == null)
            {
                persona = new PersonaDtm();
                persona.NIF = nif;
                persona.Nombre = nombre;
                persona.Apellidos = apellidos;
                persona.Baja = false;
                persona.eMail = email;
                persona.Telefono = telefono;
                persona = persona.Insertar(contexto, parametros);
            }
            else
            {
                if (persona.Baja)
                {
                    persona.Baja = false;
                    persona = persona.Modificar(contexto, parametros);
                }
            }

            return persona;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(PersonaDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearInterlocutor)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearInterlocutor), elemento.CrearInterlocutor);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearProcurador)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearProcurador), elemento.CrearProcurador);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearAbogado)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearAbogado), elemento.CrearAbogado);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearCliente)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearCliente), elemento.CrearCliente);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearProveedor)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearProveedor), elemento.CrearProveedor);

            if (!opciones.Parametros.ContainsKey(nameof(PersonaDto.CrearTrabajador)))
                opciones.Parametros.Add(nameof(PersonaDto.CrearTrabajador), elemento.CrearTrabajador);
        }

        protected override void AntesDePersistir(PersonaDtm persona, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(persona, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
            {
                var nifValido = persona.EsNie ? ApiDeTerceros.ValidarNie(persona.NIF) : ApiDeTerceros.ValidarNif(persona.NIF);
                if (!nifValido.IsNullOrEmpty())
                    GestorDeErrores.Emitir(nifValido);
            }
            if (persona.SeHaModificadoElCampo<string>(x => x.Name == nameof(persona.NIF), parametros) && persona.TieneFacturas(Contexto))
                GestorDeErrores.Emitir($"No se puede modificar el NIF de un cliente que tiene facturas emitidas");
        }

        protected override void DespuesDePersistir(PersonaDtm persona, ParametrosDeNegocio parametros)
        {
            var crearDireccionDto = ExtensorDeDirecciones.HayQueCrearDireccion(parametros.Parametros);
            var negocioAQuienAsociar = crearDireccionDto.DeQuienEsLaDireccion(parametros.Parametros);

            base.DespuesDePersistir(persona, parametros);
            if (parametros.Modificando)
            {
                persona.TrazarModificaciones(Contexto, (PersonaDtm)parametros.registroEnBd);
                persona.SincronizarConInterlocutor(Contexto, (PersonaDtm)parametros.registroEnBd);
            }

            if (parametros.Insertando && parametros.Parametros.LeerValor(nameof(PersonaDto.CrearInterlocutor), false))
            {
                var interlocutor = persona.CrearInterlocutor(Contexto);
                parametros.Parametros[nameof(ltrDePersonas.Interlocutores)] = new List<InterlocutorDtm> { interlocutor };
                if (negocioAQuienAsociar == enumNegocio.Interlocutor) interlocutor.CrearDireccion(Contexto, crearDireccionDto);

                if (parametros.Parametros.LeerValor(nameof(PersonaDto.CrearProcurador), false))
                    parametros.Parametros[nameof(ltrDePersonas.Procuradores)] =
                    GestorDeProcuradores.CrearProcuradores(Contexto, new List<int> { interlocutor.Id });

                if (parametros.Parametros.LeerValor(nameof(PersonaDto.CrearAbogado), false))
                    parametros.Parametros[nameof(ltrDePersonas.Abogados)] =
                    GestorDeAbogados.CrearAbogados(Contexto, new List<int> { interlocutor.Id });

                if (parametros.Parametros.LeerValor(nameof(PersonaDto.CrearProveedor), false))
                {
                    var cuenta = Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores);
                    parametros.Parametros[nameof(ltrDePersonas.Proveedores)] =
                    GestorDeProveedores.CrearProveedores(Contexto, new List<int> { interlocutor.Id }, cuenta.Id);
                }
                if (parametros.Parametros.LeerValor(nameof(PersonaDto.CrearCliente), false))
                {
                    var cuenta = Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes);
                    var direccion = negocioAQuienAsociar == enumNegocio.Cliente ? crearDireccionDto : null;
                    parametros.Parametros[nameof(ltrDePersonas.Clientes)] = GestorDeClientes.CrearClientes(Contexto, new List<int> { interlocutor.Id }, cuenta.Id, direccion);
                }
                if (parametros.Parametros.LeerValor(nameof(PersonaDto.CrearTrabajador), false))
                {
                    var numero = ExtensorDeSociedades.NumeroDeSociedadesGestionadas(Contexto);
                    
                    if (numero > 1)
                        GestorDeErrores.Emitir("No se puede crear el trabajador porque hay más de una sociedad gestionada, hagalo desde el mantenimiento de trabajadore.");

                    if (numero == 0)
                        GestorDeErrores.Emitir("No se puede crear el trabajador porque no hay ninguna sociedad gestionada, cree el cg de RRHH en la sociedad que quiere gestionar.");

                    var sociedadSe = ExtensorDeSociedades.Cfg_Sociedad_Del_Sistema(Contexto);
                    var cuenta = Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Sueldos);
                    GestorDeTrabajadores.CrearTrabajadores(Contexto, sociedadSe.CgDeRRHH(Contexto), new List<int> { interlocutor.Id }, cuenta.Id);
                }
            }
        }

        protected override void EliminarCaches(PersonaDtm persona, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(persona, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Persona, $"{persona.Id}");
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeProveedor);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeCliente);
        }

        protected override void DespuesDeMapearElElemento(PersonaDtm registro, PersonaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);

            if (parametros.Parametros.LeerValor(ltrDePersonas.Interlocutores, new List<InterlocutorDtm>()).Count() == 1)
            {
                elemento.IdInterlocutor = parametros.Parametros.LeerValor(ltrDePersonas.Interlocutores, new List<InterlocutorDtm>())[0].Id;
                if (parametros.Parametros.LeerValor(ltrDePersonas.Clientes, new List<ClienteDtm>()).Count() == 1)
                    elemento.IdCliente = parametros.Parametros.LeerValor(ltrDePersonas.Clientes, new List<ClienteDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDePersonas.Proveedores, new List<ProveedorDtm>()).Count() == 1)
                    elemento.IdProveedor = parametros.Parametros.LeerValor(ltrDePersonas.Proveedores, new List<ProveedorDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDePersonas.Abogados, new List<AbogadoDtm>()).Count() == 1)
                    elemento.IdAbogado = parametros.Parametros.LeerValor(ltrDePersonas.Abogados, new List<AbogadoDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDePersonas.Procuradores, new List<ProcuradorDtm>()).Count() == 1)
                    elemento.IdProcurador = parametros.Parametros.LeerValor(ltrDePersonas.Procuradores, new List<ProcuradorDtm>())[0].Id;
            }
        }

        protected override void AlDarDeBaja(PersonaDtm persona, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(persona, parametros);
            if (persona.EsInterlocutor) persona.DarDeBajaElInterlocutor<PersonaDtm>(Contexto);
        }

        protected override IQueryable<PersonaDtm> AplicarOrden(IQueryable<PersonaDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            for (var i = 0; i < ordenacion.Count; i++)
            {
                var orden = ordenacion[i];
                if (orden.OrdenarPor.Equals(nameof(PersonaDtm.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    orden.OrdenarPor = nameof(PersonaDtm.Apellidos);
                    ordenacion.Insert(i + 1, new ClausulaDeOrdenacion { Modo = orden.Modo, OrdenarPor = nameof(PersonaDtm.Nombre) });
                }
            }
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<PersonaDtm> AplicarFiltros(IQueryable<PersonaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorNombreMadrePadre(Contexto, filtros);
            foreach (var filtro in filtros.Where(filtro => filtro.Aplicado== false))
            {
                if (filtro.Clausula.Equals(nameof(PersonaDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                        consulta = consulta.AplicarPredicado(filtro, x => x.NIF.ToLower() == filtro.Valor);
                    else if (filtro.Valor.EsEntero())
                        consulta = consulta.AplicarPredicado(filtro, x => x.eMail.Contains(filtro.Valor) || x.Telefono.Contains(filtro.Valor));
                    else if (filtro.Valor.Contains("@"))
                        consulta = consulta.AplicarPredicado(filtro, x => x.eMail.Contains(filtro.Valor));
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Apellidos.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor) || x.eMail.Contains(filtro.Valor));
                }

                if (filtro.Clausula.Equals(nameof(PersonaDto.EsInterlocutor), StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => Contexto.Set<InterlocutorDtm>().Where(i => i.IdPersona == x.Id).Any());
                    filtro.Aplicado = true;
                }
            }
            return consulta;
        }

        protected override IQueryable<PersonaDtm> AplicarSeguridad(IQueryable<PersonaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio(Contexto, Negocio, consulta);
            return consulta;

        }

    }
}
