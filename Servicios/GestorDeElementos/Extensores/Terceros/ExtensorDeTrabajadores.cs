using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Terceros;
using Gestor.Errores;
using ServicioDeDatos.Contabilidad;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Negocio;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTrabajadores
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(CuentaDeTrabajadorDtm))
                return true;

            return false;
        }
        public static IQueryable<VinculoDtm> Trabajadores(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los proveedores vinculados al negocio: {negocio}");
        }


        public static DireccionDto DireccionDeContacto(this TrabajadorDtm Trabajador, ContextoSe contexto)
        =>
        Trabajador.DireccionDto(contexto, enumCalificadorDireccion.contacto, true);

        private static DireccionDto DireccionDto(this TrabajadorDtm trabajador, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direccion = trabajador.Direccion(contexto, calificador, errorSiNoHay);
            return direccion.MapearDto(contexto, direccion.Negocio);
        }

        private static DireccionDtm Direccion(this TrabajadorDtm trabajador, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Trabajador, trabajador.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion is not null)
            {
                direccion.Negocio = enumNegocio.Trabajador;
                return direccion;
            }
            return trabajador.Interlocutor(contexto).Direccion(contexto, calificador, errorSiNoHay);
        }

        public static InterlocutorDtm Interlocutor(this TrabajadorDtm trabajador, ContextoSe contexto)
        =>
        trabajador.Interlocutor != null
        ? trabajador.Interlocutor
        : contexto.SeleccionarPorId<InterlocutorDtm>(trabajador.IdInterlocutor, aplicarJoin: true);

        public static string NIF(this TrabajadorDtm trabajador, ContextoSe contexto) => trabajador.Interlocutor(contexto).NIF(contexto);

        public static CuentaDeTrabajadorDtm AsociarCuenta(this TrabajadorDtm trabajador, ContextoSe contexto, string alias, enumClaseDeCuentaBancaria clase, CuentaBancariaDtm cuenta)
        {
            var ct = trabajador.CuentaDeTrabajador(contexto, clase, errorSiNoHay: false);
            if (ct is not null)
            {
                if (ct.IdCuenta == cuenta.Id && ct.Alias == alias) return ct;
                if (ct.IdCuenta == cuenta.Id && ct.Alias != alias)
                {
                    ct.Alias = alias;
                    ct.Clase = clase;
                    return ct.Modificar(contexto);
                };
                ct.Activa = false;
                ct.Modificar(contexto);
            }

            return new CuentaDeTrabajadorDtm
            {
                Activa = true,
                Alias = alias,
                IdCuenta = cuenta.Id,
                IdElemento = trabajador.Id,
                Clase = clase
            }.Insertar(contexto);
        }
        public static CuentaDeTrabajadorDtm CuentaDeTrabajador(this TrabajadorDtm trabajador, ContextoSe contexto, enumClaseDeCuentaBancaria clase, bool errorSiNoHay = true)
        {
            var cuentas = trabajador.Detalles<CuentaDeTrabajadorDtm>(contexto);

            var cuentasActivas = clase != enumClaseDeCuentaBancaria.Ambas
            ? cuentas.Where(x => x.Activa && (x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == clase)).ToList()
            : cuentas.Where(x => x.Activa).ToList();

            if (cuentasActivas.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"El trabajador '{trabajador.NIF(contexto)}' no tiene ninguna cuenta bancaria activa o le falta el certificado");

            if (cuentasActivas.Count() > 1)
                GestorDeErrores.Emitir($"El trabajador '{trabajador.NIF(contexto)}' tiene más de una cuenta bancaria activa");

            return cuentasActivas.Count() == 0 ? null : cuentasActivas[0];
        }


        public static UsuarioDtm Usuario(this TrabajadorDtm trabajador, ContextoSe contexto)
        =>
        trabajador.Usuario ??= trabajador.IdUsuario == null ? null : contexto.SeleccionarPorId<UsuarioDtm>((int)trabajador.IdUsuario, aplicarJoin: true);

        public static CircuitoDocDtm UltimaFichada(this TrabajadorDtm trabajador, ContextoSe contexto, int idTipoFichada)
        {
            var circuitos = enumNegocio.Trabajador.CircuitosDoc(contexto).Where(ct => ct.idElemento1 == trabajador.Id);
            var cancelados = enumNegocio.CircuitoDoc.Estados(contexto).Where(e => e.Cancelado).Select(e => e.Id).ToList();
            
            var fichada = contexto.Set<CircuitoDocDtm>().Where(f=> circuitos.Any(x => x.idElemento2 == f.Id) 
            && f.IdTipo == idTipoFichada
            && f.IdCg == trabajador.IdCg 
            && f.FechaCreacion.Date == DateTime.Today
            && !cancelados.Contains(f.IdEstado)).OrderByDescending(f => f.FechaCreacion).FirstOrDefault();


            if (fichada is not null && !cancelados.Contains(fichada.IdEstado))
                return fichada;

            return null;
        }


        public static bool? HayFichadas(ContextoSe contexto)
        {
            var tipos = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.ToLista<int>(quitarNegativos: true);
            int? idtipo = tipos.Count == 1 ? tipos.ToList()[0] : null;
            var hayFichada = false;
            if (idtipo != null)
            {
                var trabajador = contexto.Trabajador();
                if (trabajador == null || trabajador.Baja) idtipo = null;
                else
                {
                    var ultima = trabajador.UltimaFichada(contexto, idtipo.Entero());
                    hayFichada = ultima is not null && ultima.Estado(contexto).Inicial;
                }
            }
            return idtipo == null ? null : (bool?)hayFichada;
        }

    }
}
