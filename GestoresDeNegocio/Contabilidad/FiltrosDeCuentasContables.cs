using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Logistica;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    public static class FiltrosDeCuentasContables
    {
        // Busca por código o nombre (filtro del mantenimiento y listas dinámicas de cuentas): si lo buscado es un número, las cuentas cuyo código empieza así;
        // si no, las que lo contienen en el código o en el nombre.
        // El valor se copia a una variable propia: el predicado captura la variable y se evalúa al lanzar la consulta
        public static IQueryable<CuentaDtm> FiltrarPorCodigoONombre(this IQueryable<CuentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(CuentaDto.Expresion).ToLower());
            if (filtro != null)
            {
                var buscado = filtro.Valor ?? "";
                if (buscado.EsEntero())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.StartsWith(buscado));
                else
                    consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.Contains(buscado) || x.Nombre.Contains(buscado));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Restrictor fijo de las listas dinámicas de cuentas: sólo las cuentas utilizables para el uso indicado (ver ltrUsoDeCuenta).
        // La variación y las compras de un almacén dependen del grupo de su cuenta de existencias: si en el formulario ya se ha seleccionado,
        // llega como restrictor (IdCuentaDeExistencias) y sólo se muestran las de su grupo (31x -> 611 y 601)
        public static IQueryable<CuentaDtm> FiltrarPorUso(this IQueryable<CuentaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorExistencias = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(AlmacenDtm.IdCuentaDeExistencias).ToLower());
            char? grupo = null;
            if (filtroPorExistencias != null)
            {
                var existencias = ExtensorDeCuentas.LeerCuenta(contexto, filtroPorExistencias.Valor.Entero() > 0 ? filtroPorExistencias.Valor.Entero() : null);
                if (existencias != null && EsDeExistencias(existencias.Codigo))
                    grupo = existencias.Codigo[1];
                filtroPorExistencias.Aplicado = true;
            }

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrUsoDeCuenta.FiltroPorUso.ToLower());
            if (filtro == null)
                return consulta;

            var prefijos = filtro.Valor switch
            {
                ltrUsoDeCuenta.Existencias => new[] { ltrCuenta.GrupoDeExistenciasDeMercaderias, ltrCuenta.GrupoDeExistenciasDeMateriasPrimas, ltrCuenta.GrupoDeOtrosAprovisionamientos },
                ltrUsoDeCuenta.Variacion => grupo is null
                    ? new[] { ltrCuenta.SubgrupoDeVariacionDeMercaderias, ltrCuenta.SubgrupoDeVariacionDeMateriasPrimas, ltrCuenta.SubgrupoDeVariacionDeOtrosAprovisionamientos }
                    : new[] { ExtensorDeCuentas.PrefijoDeVariacion((char)grupo) },
                ltrUsoDeCuenta.Compras => grupo is null
                    ? new[] { ltrCuenta.SubgrupoDeComprasDeMercaderias, ltrCuenta.SubgrupoDeComprasDeMateriasPrimas, ltrCuenta.SubgrupoDeComprasDeOtrosAprovisionamientos }
                    : new[] { ExtensorDeCuentas.PrefijoDeCompras((char)grupo) },
                ltrUsoDeCuenta.Devoluciones => new[] { ExtensorDeCuentas.PrefijoDeDevoluciones },
                ltrUsoDeCuenta.GastoDeMateriales => new[] { ExtensorDeCuentas.PrefijoDeGastoDeMateriales },
                _ => null
            };
            if (prefijos is null)
                GestorDeErrores.Emitir($"No se ha definido qué cuentas se pueden usar para '{filtro.Valor}'");

            // como mucho tres prefijos: se encadenan con OR para que se traduzca a LIKE 'xx%' OR ...
            var p0 = prefijos[0];
            var p1 = prefijos.Length > 1 ? prefijos[1] : p0;
            var p2 = prefijos.Length > 2 ? prefijos[2] : p0;
            consulta = consulta.Where(x => x.Codigo.StartsWith(p0) || x.Codigo.StartsWith(p1) || x.Codigo.StartsWith(p2));
            filtro.Aplicado = true;
            return consulta;
        }

        private static bool EsDeExistencias(string codigo) =>
            codigo is not null && codigo.Length >= 3 && codigo[0] == '3' && "012".Contains(codigo[1]);

        // Busca la cuenta definida en una variable (sueldos, proveedores o clientes)
        public static IQueryable<CuentaDtm> FiltrarPorVariable(this IQueryable<CuentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrVariables.FiltroPorVariable);
            if (filtro != null)
            {
                var codigo = "";
                if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Sueldos).ToLower())
                    codigo = VariablesDeCuentas.Sueldos;
                else if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Proveedores).ToLower())
                    codigo = VariablesDeCuentas.Proveedores;
                else if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Clientes).ToLower())
                    codigo = VariablesDeCuentas.Clientes;
                else GestorDeErrores.Emitir($"No hay variable de cuenta contable del tipo : {filtro.Valor}");

                consulta = consulta.AplicarPredicado(filtro, x => x.Codigo == codigo);
            }
            return consulta;
        }
    }
}
