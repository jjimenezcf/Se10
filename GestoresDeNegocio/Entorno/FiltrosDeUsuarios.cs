using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Entorno
{
    internal static class FiltrosDeUsuarios
    {
        public static IQueryable<UsuarioDtm> FiltrarPorUsuarioDeCliente(this IQueryable<UsuarioDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrUsuariosDeUnCliente.FiltroPorUsuarioDeCliente.ToLower());
            if (filtro != null)
            {
                //consulta = consulta.Where(x => !contexto.Set<UsuarioDeClienteDtm>().Any());
                consulta = filtro.Criterio == enumCriteriosDeFiltrado.contiene
                ? consulta.Where(x => contexto.Set<UsuarioDeClienteDtm>().Any(uc => uc.IdUsuario == x.Id))
                : consulta.Where(x => !contexto.Set<UsuarioDeClienteDtm>().Any(uc => uc.IdUsuario == x.Id));
                consulta = filtroInternoPorNombreLogin(consulta, filtro);
                consulta = consulta.Where(x => x.Activo);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<UsuarioDtm> ExcluirUsuarioDeCliente(this IQueryable<UsuarioDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrUsuariosDeUnCliente.FiltroPorUsuarioDeCliente.ToLower());
            if (filtro == null)
            {
                return consulta.Where(x => !contexto.Set<UsuarioDeClienteDtm>().Any(uc => uc.IdUsuario == x.Id));
            }


            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrUsuariosDeUnCliente.ExcluirUsuariosDeCliente.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => !contexto.Set<UsuarioDeClienteDtm>().Any(uc => uc.IdUsuario == x.Id));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<UsuarioDtm> FiltrarPorNombreCompleto(this IQueryable<UsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == UsuariosPor.NombreCompleto.ToLower());
            if (filtro != null)
            {
                consulta = filtroInternoPorNombreLogin(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        private static IQueryable<UsuarioDtm> filtroInternoPorNombreLogin(IQueryable<UsuarioDtm> consulta, ClausulaDeFiltrado filtro)
        {
            var partesDelNombre = filtro.Valor.Split('(', ')', ',');
            if (partesDelNombre.Length == 4)
                consulta = consulta.AplicarPredicado(filtro, x => x.Login == partesDelNombre[1].Trim()
                                              && x.Apellido == partesDelNombre[2].Trim()
                                              && x.Nombre == partesDelNombre[3].Trim());
            else
                consulta = consulta.AplicarPredicado(filtro, x => x.Apellido.Contains(filtro.Valor)
                                              || x.Nombre.Contains(filtro.Valor)
                                              || x.Login.Contains(filtro.Valor));
            return consulta;
        }

        public static IQueryable<UsuarioDtm> FiltrarPorPermisos(this IQueryable<UsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(UsuariosPor.Permisos).ToLower());
            if (filtro != null)
            {
                var listaIds = filtro.Valor.ToLista<int>();
                foreach (int id in listaIds)
                {
                    consulta = consulta.Where(u => u.Permisos.Any(up => up.IdPermiso == id && up.IdUsuario == u.Id));
                }
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(PermisosDeUnUsuarioDto.IdPermiso).ToLower());
            if (filtro != null)
            {
                consulta = consulta.AplicarPredicado(filtro, u => u.Permisos.Any(x => x.IdPermiso == filtro.Valor.Entero()));
                filtro.Aplicado = true;
                return consulta;
            }



            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(PermisosDeUnUsuarioDto.IdPermiso).ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<UsuarioDtm> FiltrarPorRol(this IQueryable<UsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(RolesDeUnPuestoDto.IdRol).ToLower());
            if (filtro != null)
            {
                consulta = consulta.AplicarPredicado(filtro, u => u.Puestos.Any(x => x.Puesto.Roles.Any(y => y.IdRol == filtro.Valor.Entero())));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<UsuarioDtm> FiltrarPorPuesto(this IQueryable<UsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDtm.IdPuesto).ToLower());
            if (filtro != null)
            {
                if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                    consulta = consulta.AplicarPredicado(filtro, i => !i.Puestos.Any(r => r.IdPuesto.Equals(filtro.Valor.Entero())));

                if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                    consulta = consulta.AplicarPredicado(filtro, i => i.Puestos.Any(r => r.IdPuesto.Equals(filtro.Valor.Entero())));
                
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<PuestosDeUnUsuarioDtm> FiltrarPorNombreDePt(this IQueryable<PuestosDeUnUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDto.Puesto).ToLower() && !x.Valor.IsNullOrEmpty());
            if (filtro != null)
            {
                if (filtro.Criterio == enumCriteriosDeFiltrado.contiene)
                    consulta = consulta.Where(x => x.Puesto.Nombre.Contains(filtro.Valor));

                if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                    consulta = consulta.Where(x => x.Puesto.Nombre == filtro.Valor);

                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<PuestosDeUnUsuarioDtm> FiltrarPorNombreDeUsuario(this IQueryable<PuestosDeUnUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDto.Usuario).ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.Usuario.Apellido.Contains(filtro.Valor)
                                              || x.Usuario.Nombre.Contains(filtro.Valor)
                                              || x.Usuario.Login.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }

            return consulta;
        }
    }
}
