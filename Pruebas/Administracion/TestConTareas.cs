using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Tarea;
using GestoresDeNegocio.Terceros;
using Inicializador.Procesos;
using ModeloDeDto.Terceros;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.AytoBeniel;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    public class TestConTareas
    {
        //ContextoSe _contexto;
        //[SetUp]
        //public void Setup()
        //{
        //    _contexto = Inicializaciones.CrearContexto();
        //}

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void CrearUnaTarea()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearTarea(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, InzTareasRre.n_tipo_trr, InzAcromur.n_nif_acromur, "prueba tarea de acromur en Beniel", "tarea añadida posterioremente");
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        public static TareaDtm CrearTarea(ContextoSe contexto, string codigoCg, string nombreTipo, string dniInterlocutor, string asunto, string descripcion)
        {
            var fCg = new Dictionary<string, object> { { nameof(CentroGestorDtm.Codigo), codigoCg } };
            var cg = contexto.SeleccionarPorAk<CentroGestorDtm>(fCg);

            var fTipo = new Dictionary<string, object> { { nameof(TipoDeElementoDtm.Nombre), nombreTipo } };
            var tipoTarea = contexto.SeleccionarPorAk<TipoDeTareaDtm>(fTipo);

            var fInt = new Dictionary<string, object> { { nameof(InterlocutorDto.Expresion), dniInterlocutor } };
            var inter = contexto.SeleccionarPorAk<InterlocutorDtm>(fInt, errorSiNoHay: false);
            if (inter == null)
            {
                inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, dniInterlocutor);
            }

            //Cremos el registro y se le han de crear tres archivadore, con tres vínculos
            var r = new TareaDtm();
            r.IdCg = cg.Id;
            r.IdTipo = tipoTarea.Id;
            r.Nombre = asunto;
            r.Descripcion = descripcion;
            r.IdSolicitante = inter.Id;
            var gestor = GestorDeTareas.Gestor(contexto, contexto.Mapeador);
            r = gestor.PersistirRegistro(r, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return gestor.LeerRegistroPorId(r.Id, false, false, false, false);
        }
    }
}
