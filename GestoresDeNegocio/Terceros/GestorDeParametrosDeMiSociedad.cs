using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using System;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Juridico;

namespace GestoresDeNegocio.Terceros
{

    public class GestorDeParametrosDeMiSociedad : GestorDeElementos<ContextoSe, ParametrosDeMiSociedadDtm, ParametrosDeMiSociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearParametrosDeMiSociedad : Profile
        {
            public MapearParametrosDeMiSociedad()
            {
                CreateMap<ParametrosDeMiSociedadDtm, ParametrosDeMiSociedadDto>();
                CreateMap<ParametrosDeMiSociedadDto, ParametrosDeMiSociedadDtm>();
            }
        }

        public GestorDeParametrosDeMiSociedad(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeParametrosDeMiSociedad Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeParametrosDeMiSociedad(contexto, mapeador);
        }

        protected override void AntesDePersistir(ParametrosDeMiSociedadDtm parametrosDeMiSociedad, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametrosDeMiSociedad, parametros);

            var sociedad = (SociedadDtm)parametrosDeMiSociedad.AmpliacionDe(Contexto);
            if (!sociedad.EsGestionada(Contexto))
                Emitir($"La sociedad '{sociedad.Referencia}' no es gestionada por el sistema");

            if (sociedad.Baja)
                Emitir($"No se pueden modificar los datos de la '{sociedad.Referencia}' por estar de baja");

           if (!sociedad.EsInterventor(Contexto))
                Emitir($"No se pueden modificar los datos de la '{sociedad.Referencia}' por no ser interventor de la sociedad");

        }

        protected override void DespuesDePersistir(ParametrosDeMiSociedadDtm parametrosDeMiSociedad, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametrosDeMiSociedad, parametros);
            var sociedad = parametrosDeMiSociedad.Elemento<SociedadDtm>(Contexto);
            if (parametros.Modificando) 
            {
                var anterior = (ParametrosDeMiSociedadDtm)parametros.registroEnBd;

                if (!parametrosDeMiSociedad.InscritoEn.Equals(anterior.InscritoEn)) sociedad.CrearTraza(Contexto,
                        "Se ha modificado la información de inscrición societaria",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha modificado la información, antes era: {anterior.InscritoEn}, ahora es: {parametrosDeMiSociedad.InscritoEn}");

                if (!parametrosDeMiSociedad.PieDeFactura.Equals(anterior.PieDeFactura)) sociedad.CrearTraza(Contexto,
                        "Se ha modificado el pié de impresión de facturas",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha modificado el pié, antes era: {anterior.PieDeFactura}, ahora es: {parametrosDeMiSociedad.PieDeFactura}");

                if (!parametrosDeMiSociedad.PieDePresupuesto.Equals(anterior.PieDePresupuesto)) sociedad.CrearTraza(Contexto,
                        "Se ha modificado el pié de impresión de presupuestos",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha modificado el pié, antes era: {anterior.PieDePresupuesto}, ahora es: {parametrosDeMiSociedad.PieDePresupuesto}");

            }
        }
    }
}
