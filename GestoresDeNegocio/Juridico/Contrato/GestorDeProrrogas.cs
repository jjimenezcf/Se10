using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using GestorDeElementos.Extensores;
using System;
using static Gestor.Errores.GestorDeErrores;

namespace GestoresDeNegocio.Juridico
{
    public static class ltrProrrogas
    {
        public static string OpcionPorSeleccionar = nameof(OpcionPorSeleccionar);
        public static string contratosProrrogados = nameof(contratosProrrogados);
        public static string contratosFinalizados = nameof(contratosFinalizados);
    }

    public class GestorDeProrrogas : GestorDeElementos<ContextoSe, ProrrogaDtm, ProrrogaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearProrrogas : Profile
        {
            public MapearProrrogas()
            {
                CreateMap<ProrrogaDtm, ProrrogaDto>();
                CreateMap<ProrrogaDto, ProrrogaDtm>();
            }
        }

        public GestorDeProrrogas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeProrrogas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeProrrogas(contexto, mapeador);
        }

        protected override void AntesDePersistir(ProrrogaDtm prorroga, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(prorroga, parametros);
            if (prorroga.FechaUltimaProrroga != default)
                prorroga.FechaUltimaProrroga = ((DateTime) prorroga.FechaUltimaProrroga).Date;
            DateTime? fechaAnterior = parametros.Insertando 
            ? prorroga.FechaUltimaProrroga 
            : ((ProrrogaDtm)parametros.registroEnBd).FechaUltimaProrroga == null 
            ? null 
            :((DateTime)((ProrrogaDtm)parametros.registroEnBd).FechaUltimaProrroga).Date;
            int? mesesParaProrrogarAnterior = parametros.Insertando ? prorroga.Meses : ((ProrrogaDtm)parametros.registroEnBd).Meses;

            // solo puede modificarse los meses de prorrogación y la fecha de finde prorrogación en la etapa inicial
            var contrato = prorroga.Elemento<ContratoDtm>(Contexto);
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && !prorroga.FechaUltimaProrroga.Equals(fechaAnterior))
                Emitir($"No se puede modificar la fecha de última prórroga si el contrato no está en elaboración");
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && !prorroga.Meses.Equals(mesesParaProrrogarAnterior))
                Emitir($"No se puede modificar los meses de prorrogación si el contrato no está en elaboración");

            // Si no hay fecha de fin de contrato no puede haber prorrogación
            //contrato.SiNoHayFechaFinNoPuedeSerProrrogable(Contexto);

            // si es no prorrogable, no puede haber prorrogación ni lo contrario
            if (prorroga.ClaseDeProrroga == enumClaseDeProrroga.noProrrogable && (prorroga.Meses.Entero() > 0 || !prorroga.FechaUltimaProrroga.Equals(default)))
                Emitir($"No se puede indicar que un contrato no es prorrogable y indicar meses de proorogación o fecha de última prórroga");

            if (prorroga.ClaseDeProrroga != enumClaseDeProrroga.noProrrogable && (prorroga.Meses.Entero() == 0 || prorroga.FechaUltimaProrroga.Equals(default)))
                Emitir($"No se puede indicar que un contrato es prorrogable y no indicar ni meses de proorogación ni fecha de última prórroga");

            // Trabajo que transita el contrato y actualiza las fechas de fin de prórroga 
            // añadir a los cambios de fecha de inicio fin y meses de contratos la traza de dichos cambios
        }

        protected override void DespuesDePersistir(ProrrogaDtm prorroga, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(prorroga, parametros);
            var contrato = prorroga.Elemento<ContratoDtm>(Contexto);
            var prorrogaBd = parametros.Insertando ? prorroga : (ProrrogaDtm)parametros.registroEnBd;

            if (!prorroga.ClaseDeProrroga.Equals(prorrogaBd.ClaseDeProrroga)) contrato.CrearTraza(Contexto,
                    "Se ha modificado la clase de prórroga",
                    $"El usuario {Contexto.DatosDeConexion.Login} ha modificado la clase de próoroga del contrato, antes era: {prorrogaBd.ClaseDeProrroga.Descripcion()}, ahora es: {prorroga.ClaseDeProrroga.Descripcion()}");

            if (prorroga.Meses.Entero() > 0 && prorrogaBd.Meses.Entero() > 0 &&!prorroga.Meses.Equals(prorrogaBd.Meses)) contrato.CrearTraza(Contexto,
                    "Se ha modificado los meses de duración de la prórroga",
                    $"El usuario {Contexto.DatosDeConexion.Login} ha modificado los meses de duración de la prórroga, antes era: {prorrogaBd.Meses}, ahora es: {prorroga.Meses}");

            if (prorroga.FechaUltimaProrroga != default && prorrogaBd.FechaUltimaProrroga != default && !prorroga.FechaUltimaProrroga.Equals(prorrogaBd.FechaUltimaProrroga)) 
                contrato.CrearTraza(Contexto,
                    "Se ha modificado la fecha de última prórroga",
                    $"El usuario {Contexto.DatosDeConexion.Login} ha modificado la fecha de última prórroga, antes era: {prorrogaBd.FechaUltimaProrroga}, ahora es: {prorroga.FechaUltimaProrroga}");
        }
    }
}
