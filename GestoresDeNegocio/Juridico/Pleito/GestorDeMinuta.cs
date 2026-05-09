using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using Gestor.Errores;
using System;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeMinutas : GestorDeElementos<ContextoSe, MinutaDtm, MinutaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrMinutas
        {
        }

        public class MapearMinutas : Profile
        {
            public MapearMinutas()
            {
                CreateMap<MinutaDtm, MinutaDto>();
                CreateMap<MinutaDto, MinutaDtm>();
            }
        }

        public GestorDeMinutas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeMinutas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeMinutas(contexto, mapeador);
        }

        protected override void AntesDePersistir(MinutaDtm minuta, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(minuta, parametros);

            var pleito = Contexto.SeleccionarPorId<PleitoDtm>(minuta.IdElemento);
           
            if (parametros.Insertando) minuta.Negocio.ValidarUsaDetalleDe(Contexto, pleito.IdTipo, typeof(MinutaDtm));
            if (pleito.FechaCreacion > minuta.CreadoEl)
                GestorDeErrores.Emitir($"La fecha de cuando se crea el concepto debe ser posterior o igual a cuando se creo el pleito");

            if (minuta.CreadoEl > minuta.AbonadoEl)
                GestorDeErrores.Emitir($"La fecha de cuando se crea el concepto debe ser anterior o igual a cuando se abona");

            if (minuta.Valor <= minuta.Abonado)
                GestorDeErrores.Emitir($"Está intentando abonar mayor cantidad que el coste del concepto");

            new TrazasDeUnPleitoDtm
            {
                IdElemento = minuta.IdElemento,
                Nombre = $"Se ha {(parametros.Insertando? "creado": $"{(parametros.Modificando ? "modificado" : " eliminado")}")} un concepto de la minuta",
                Descripcion = MensajeDeTraza(minuta, parametros)
            }
            .Insertar(Contexto);
        }

        private string MensajeDeTraza(MinutaDtm minuta, ParametrosDeNegocio parametros)
        {
            var mensaje = "";
            if (parametros.Insertando)
                mensaje = $"El usuario {Contexto.DatosDeConexion.Login} a creado el concepto de pago '{minuta.Concepto}' por valor de '{minuta.Valor}' con fecha '{minuta.CreadoEl}'";
            if (parametros.Modificando) 
            {
                mensaje = $"El usuario {Contexto.DatosDeConexion.Login} a modificado el concepto de pago:";
                if (((MinutaDtm)parametros.registroEnBd).Concepto != minuta.Concepto)
                    mensaje = mensaje + $"{Environment.NewLine}Antes: '{((MinutaDtm)parametros.registroEnBd).Concepto}' ahora: '{minuta.Concepto}'";
                if (((MinutaDtm)parametros.registroEnBd).Valor != minuta.Valor)
                    mensaje = mensaje + $"{Environment.NewLine}Antes: {((MinutaDtm)parametros.registroEnBd).Valor} ahora: {minuta.Valor}'";
                if (((MinutaDtm)parametros.registroEnBd).Abonado != minuta.Abonado)
                    mensaje = mensaje + $"{Environment.NewLine}Antes: {((MinutaDtm)parametros.registroEnBd).Abonado} ahora: {minuta.Abonado}'";
            }
            if (parametros.Eliminando)
            {
                mensaje = $"El usuario {Contexto.DatosDeConexion.Login} a eliminado el concepto de pago:";
                mensaje = mensaje + $"{Environment.NewLine}Concepto: {minuta.Concepto}'";
                mensaje = mensaje + $"{Environment.NewLine}Valor: {minuta.Valor}'";
                mensaje = mensaje + $"{Environment.NewLine}Abonado: {minuta.Abonado}'";
            }
            return mensaje;
        }
    }
}
