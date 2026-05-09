using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public enum enumParametrosDeGuarderia
    {
        [Description("Valor boleano que indica si se usa el módulo de guardería")]
        INF_MODULO_ACTIVO,
        [Description("Id del rol que contiene los permisos de gestión de las agendas de los niños de la guardería")]
        INF_ROL_DE_ADMINISTRADOR_DE_AGENDAS,
        [Description("Indica el CG de los puestos de trabajo de los curso de guardería")]
        CURSO_CG_PARA_PUESTOS_DE_TRABAJO
    }

    public static class ltrDeGuarderias
    {
        public static readonly string ModuloNoActivo = "El módulo de guarderías no está activo";
    }

    public static class ExtensorDeGuarderias
    {
        public static async Task<bool> ModuloActivoAsync()
        {
            return await Task.Run(() => ParametroDeNegocioSql.Parametro(enumNegocio.Infante, enumParametrosDeGuarderia.INF_MODULO_ACTIVO, crearParametro: true, valorPorDefecto: false).Valor.EsTrue());

        }

        public static bool ModuloActivo(ContextoSe contexto)
        {
            var negocio = NegociosDeSe.LeerNegocioPorEnumerado(enumNegocio.Infante, errorSiNoHay: false);
            if (negocio == null) return false;
            return enumNegocio.Infante.LeerCrearParametro(contexto, enumParametrosDeGuarderia.INF_MODULO_ACTIVO, valor: "N").Valor.EsTrue();
        }

        public static CentroGestorDtm CgParaPtsDeCursosDeGuarderia(ContextoSe contexto)
        {
            var parametro = ParametroDeNegocioSql.Parametro(enumNegocio.CursoDeGuarderia, enumParametrosDeGuarderia.CURSO_CG_PARA_PUESTOS_DE_TRABAJO, emitirError: false, crearParametro: true, valorPorDefecto: "0");
            if (parametro.Valor.Entero() == 0)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.CursoDeGuarderia.Descripcion()}' el valor del parámetro '{enumParametrosDeGuarderia.CURSO_CG_PARA_PUESTOS_DE_TRABAJO}' para poder crear los puestos de trabajo");
            }
            var cg = contexto.SeleccionarPorId<CentroGestorDtm>(parametro.Valor.Entero(), errorSiNoHay: false);
            if (cg == null)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.CursoDeGuarderia.Descripcion()}' un id válido para el CG por defecto, el indicado es: {parametro.Valor.Entero()}");
            }
            return cg;
        }

        public static AulaDeGuarderiaDtm Aula(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        =>
        curso.Aula ??= contexto.SeleccionarPorId<AulaDeGuarderiaDtm>(curso.IdAula, aplicarJoin: true);

        public static TrabajadorDtm Trabajador(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        =>
        curso.Trabajador ??= contexto.SeleccionarPorId<TrabajadorDtm>(curso.IdTrabajador, aplicarJoin: true);

        public static TrabajadorDtm Trabajador(this ProfeDeCursoDeGuarderiaDtm profe, ContextoSe contexto)
        =>
        profe.Trabajador ??= contexto.SeleccionarPorId<TrabajadorDtm>(profe.IdTrabajador, aplicarJoin: true);

        public static InfanteDtm Infante(this InfanteDeUnCursoDtm infanteDelCurso, ContextoSe contexto)
        =>
        infanteDelCurso.Infante ??= contexto.SeleccionarPorId<InfanteDtm>(infanteDelCurso.IdInfante, aplicarJoin: true);

        public static CursoDeGuarderiaDtm Curso(this InfanteDeUnCursoDtm infanteDelCurso, ContextoSe contexto)
        =>
        infanteDelCurso.Elemento ??= contexto.SeleccionarPorId<CursoDeGuarderiaDtm>(infanteDelCurso.IdElemento, aplicarJoin: true);

        public static AgendaDtm Agenda(this InfanteDtm infante, ContextoSe contexto)
        =>
        infante.Agenda ??= contexto.SeleccionarPorId<AgendaDtm>(infante.IdAgenda, aplicarJoin: true);

        public static AgendaDtm Agenda(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        =>
        curso.Agenda ??= contexto.SeleccionarPorId<AgendaDtm>(curso.IdAgenda, aplicarJoin: true);

        public static PuestoDtm Gestor(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        =>
        curso.Gestor ??= contexto.SeleccionarPorId<PuestoDtm>(curso.IdGestor, aplicarJoin: true);

        public static PuestoDtm Consultor(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        =>
        curso.Consultor ??= contexto.SeleccionarPorId<PuestoDtm>(curso.IdConsultor, aplicarJoin: true);


        public static PuestoDtm PuestoDeGestor(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        {
            var puestos = contexto.SeleccionarTodos<PuestoDtm>(new Dictionary<string, object> {
                {nameof(PuestoDtm.IdCg), ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Id },
                {nameof(PuestoDtm.Nombre), curso.NombrePt(enumModoDeAccesoDeDatos.Gestor)}
            });

            if (puestos.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado el puesto '{curso.NombrePt(enumModoDeAccesoDeDatos.Gestor)}' " +
                                       $"en el CG '{ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Expresion}', defínalo");

            if (puestos.Count > 1)
                GestorDeErrores.Emitir($"Se ha localizado para el curso '{curso.Nombre}' más de un puesto de trabajo. " +
                $"Puesto: {curso.NombrePt(enumModoDeAccesoDeDatos.Gestor)}' " +
                    $"CG: '{ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Expresion}'");

            return puestos[0];
        }

        public static PuestoDtm PuestoDeConsultor(this CursoDeGuarderiaDtm curso, ContextoSe contexto)
        {
            var puestos = contexto.SeleccionarTodos<PuestoDtm>(new Dictionary<string, object> {
                {nameof(PuestoDtm.IdCg), ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Id },
                {nameof(PuestoDtm.Nombre), curso.NombrePt(enumModoDeAccesoDeDatos.Consultor)}
            });

            if (puestos.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado el puesto '{curso.NombrePt(enumModoDeAccesoDeDatos.Consultor)}' " +
                                       $"en el CG '{ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Expresion}', defínalo");

            if (puestos.Count > 1)
                GestorDeErrores.Emitir($"Se ha localizado para el curso '{curso.Nombre}' más de un puesto de trabajo. " +
                $"Puesto: {curso.NombrePt(enumModoDeAccesoDeDatos.Consultor)}' " +
                    $"CG: '{ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(contexto).Expresion}'");

            return puestos[0];
        }

        public static UsuarioDtm UsuarioDeSuProfesor(this InfanteDtm infante, ContextoSe contexto)
        {
            var cursos = contexto.SeleccionarTodos<CursoDeGuarderiaDtm>(new Dictionary<string, object> {
                {ltrDeCursosDeGuarderia.FiltrarPorActivo, true },
                {ltrDeCursosDeGuarderia.FiltrarPorInfante, infante.Id }
            });
            if (cursos.Count == 0) return null;
            if (cursos.Count > 1) GestorDeErrores.Emitir($"El niño/a '{infante.Expresion}' no tiene curso activo asignado el '{DateTime.Now.ToString("dd-MM-yyyy")}'");

            var profesor = cursos[0].Trabajador(contexto);
            return profesor.IdUsuario == null ? null : profesor.Usuario(contexto);
        }

        public static void ActualizarGestorDeAgendas(this InfanteDtm infante, ContextoSe contexto)
        {
            var valor = ParametroDeNegocioSql.Parametro(enumNegocio.Infante, enumParametrosDeGuarderia.INF_ROL_DE_ADMINISTRADOR_DE_AGENDAS, emitirError: false, crearParametro: true, 0.ToString()).Valor;
            if (valor.Entero() == 0)
                GestorDeErrores.Emitir($"Debe indicar el id correspondiente al rol que será el gestor de las agendas de la guardería en el parámetro {enumParametrosDeGuarderia.INF_ROL_DE_ADMINISTRADOR_DE_AGENDAS}");

            var rol = contexto.SeleccionarPorId<RolDtm>(valor.Entero());
            contexto.CrearRelacionComoAdministrador<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, infante.Agenda(contexto).IdGestor);
        }

        public static CursoDeGuarderiaDtm CursoEnElQueEsta(this InfanteDtm infante, ContextoSe contexto, bool errorSiNoHay = false)
        =>
       CursoEnElQueEsta(contexto, infante.Id);

        public static InterlocutorDtm Contacto(this InfanteDtm infante, ContextoSe contexto)
        =>
        infante.Contacto ??= contexto.SeleccionarPorId<InterlocutorDtm>(infante.IdContacto);

        public static PersonaDtm Madre(this InfanteDtm infante, ContextoSe contexto)
        =>
        infante.Madre ??= infante.IdMadre is null ? null : contexto.SeleccionarPorId<PersonaDtm>((int)infante.IdMadre);

        public static PersonaDtm Padre(this InfanteDtm infante, ContextoSe contexto)
        =>
        infante.Padre ??= infante.IdPadre is null ? null : contexto.SeleccionarPorId<PersonaDtm>((int)infante.IdPadre);

        public static CursoDeGuarderiaDtm CursoEnElQueEsta(ContextoSe contexto, int idInfante, bool errorSiNoHay = false)
        {
            var curso = contexto.SeleccionarPorFk<InfanteDeUnCursoDtm>(nameof(InfanteDeUnCursoDtm.IdInfante), idInfante, errorSiNoHay: errorSiNoHay, aplicarJoin: true);            
            return curso == null ? null : contexto.SeleccionarPorId<CursoDeGuarderiaDtm>(curso.IdElemento);
        }

    }
}
