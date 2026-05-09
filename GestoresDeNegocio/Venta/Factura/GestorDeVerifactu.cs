using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeVerifactu : GestorDeElementos<ContextoSe, VerifactuDtm, VerifactuDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public class MapearIrpfsEmt : Profile
        {
            public MapearIrpfsEmt()
            {
                CreateMap<VerifactuDtm, VerifactuDto>();
                CreateMap<VerifactuDto, VerifactuDtm>();
            }
        }

        public GestorDeVerifactu(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeVerifactu Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVerifactu(contexto, mapeador);
        }

        protected override IQueryable<VerifactuDtm> AplicarJoins(IQueryable<VerifactuDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Archivador);
            consulta = consulta.Include(x => x.BlockChain);
            return consulta;
        }

        public override VerifactuDtm PersistirRegistro(VerifactuDtm verifactu, ParametrosDeNegocio parametros)
        {
            if (parametros.EsUnaPeticion)
                return null;
            parametros.ValidarPermisosDePersistencia = false;
            return base.PersistirRegistro(verifactu, parametros);
        }

        protected override void AntesDePersistir(VerifactuDtm verifactu, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(verifactu, parametros);

        }

        protected override void DespuesDePersistir(VerifactuDtm verifactu, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(verifactu, parametros);
        }

        protected override void DespuesDeMapearElElemento(VerifactuDtm verifactu, VerifactuDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(verifactu, elemento, parametros);
            elemento.Archivador = verifactu.ObtenerArchivador(Contexto)?.Expresion;
            elemento.BlockChain = verifactu.ObtenerBlockChain(Contexto)?.Expresion;
        }

    }
}
