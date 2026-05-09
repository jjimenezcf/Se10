using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.RegistroEs;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using Gestor.Errores;

namespace GestoresDeNegocio.RegistroEs
{

    public class GestorDeRegistrosEs : GestorDeElementos<ContextoSe, RegistroEsDtm, RegistroEsDto>, IImportadorDelCorreo
    {

        public class MapearRegistroEs : Profile
        {
            public MapearRegistroEs()
            {
                CreateMap<RegistroEsDtm, RegistroEsDto>()
                .DtmToDto();

                CreateMap<RegistroEsDto, RegistroEsDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.ArchivadorInterno, dto => dto.Ignore())
                .ForMember(dtm => dtm.ArchivadorDeSalida, dto => dto.Ignore())
                .ForMember(dtm => dtm.ArchivadorDeEntrada, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Registro;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();
        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeRegistroEs.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeRegistrosEs(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeRegistrosEs Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRegistrosEs(contexto, mapeador);
        }


        protected override IQueryable<RegistroEsDtm> AplicarJoins(IQueryable<RegistroEsDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.ArchivadorDeEntrada)
                .Include(x => x.ArchivadorDeSalida)
                .Include(x => x.ArchivadorInterno)
                .Include(x => x.Solicitante);
            return consulta;
        }

        protected override IQueryable<RegistroEsDtm> AplicarFiltros(IQueryable<RegistroEsDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(ltrFiltros.VincularCon, StringComparison.CurrentCultureIgnoreCase))
                {
                    var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).RegistrosEs(Contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                    consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(Contexto, filtro, vinculos);
                }
            }
            return consulta;
        }

        protected override void AntesDePersistir(RegistroEsDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            registro.ClaseDeRegistro = ((TipoDeRegistroEsDtm)parametros.TipoConFujo).ClaseDeRegistro;
            if (parametros.Insertando)
            {
                var tipo = (TipoDeRegistroEsDtm)parametros.Parametros[nameof(TipoConFlujoDtm)];
                CrearArchivadores(registro, tipo);
            }
        }

        protected override void DespuesDePersistir(RegistroEsDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Insertando)
            {
                var presentador = (InterlocutorDtm)parametros.Parametros[nameof(InterlocutorDtm)];
                var direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Interlocutor, presentador.Id).ToList();
                var b = false;
                var direccionFiscal = direcciones.Find(d => d.Calificador == enumCalificadorDireccion.fiscal);
                if (direccionFiscal != null)
                {
                    direccionFiscal.Calificador = enumCalificadorDireccion.contacto;
                    GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Registro, registro.Id, direccionFiscal);
                    b = true;
                }
                else
                    foreach (var direccion in direcciones)
                    {
                        direccion.Calificador = enumCalificadorDireccion.contacto;
                        GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Registro, registro.Id, direccion);
                        b = true;
                        break;
                    }
                if (!b)
                {
                    if (presentador.EsPersona)
                        direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Persona, (int)presentador.IdPersona).ToList();
                    else
                        direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Sociedad, (int)presentador.IdSociedad).ToList();
                    foreach (var direccion in direcciones)
                    {
                        direccion.Calificador = enumCalificadorDireccion.contacto;
                        GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Registro, registro.Id, direccion);
                        b = true;
                        break;
                    }
                }

                GestorDeVinculos.Vincular(Contexto, enumNegocio.Registro, enumNegocio.Archivador, registro.Id, (int)registro.IdArchivadorDeEntrada);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.Registro, enumNegocio.Archivador, registro.Id, (int)registro.IdArchivadorDeSalida);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.Registro, enumNegocio.Archivador, registro.Id, (int)registro.IdArchivadorInterno);
            }
        }

        private void CrearArchivadores(RegistroEsDtm registro, TipoDeRegistroEsDtm tipo)
        {
            if (tipo.IdTipoArchivadorInterno != null)
                registro.IdArchivadorInterno = CrearArchivador(registro, "interna", (int)tipo.IdTipoArchivadorInterno);

            if (tipo.IdTipoArchivadorDeEntrada != null)
                registro.IdArchivadorDeEntrada = CrearArchivador(registro, "entrada", (int)tipo.IdTipoArchivadorDeEntrada);

            if (tipo.IdTipoArchivadorDeSalida != null)
                registro.IdArchivadorDeSalida = CrearArchivador(registro, "salida", (int)tipo.IdTipoArchivadorDeSalida);

        }

        private int CrearArchivador(RegistroEsDtm registro, string tipo, int idTipo) => ExtensorDeArchivadores.CrearArchivador(Contexto, enumNegocio.Registro, registro, tipo, idTipo).Id;

        protected override RegistroEsDto DespuesDePersistirElementoDto(RegistroEsDto elemento, RegistroEsDtm registro, ParametrosDeNegocio parametros)
        {
            return base.DespuesDePersistirElementoDto(elemento, registro, parametros);
            //Si es inserción:
            //Someter el envío del mensaje a las direcciones de correo indicadas
            //Anotar traza de envío de mensaje(lo debe hacer el trabajo sometido)
        }

        protected override IQueryable<RegistroEsDtm> AplicarSeguridad(IQueryable<RegistroEsDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<RegistroEsDtm, TipoDeRegistroEsDtm, PermisoDelRegistroEsDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<RegistroEsDtm, PermisoDelRegistroEsDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idRegistro = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var registro = entorno.Contexto.SeleccionarPorId<RegistroEsDtm>(idRegistro);
            if (vinculado == enumNegocio.Archivador)
            {
                var idArchivador = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                var archivador = entorno.Contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
                if (registro.IdArchivadorDeEntrada.Entero() == idArchivador || registro.IdArchivadorDeSalida.Entero() == idArchivador || registro.IdArchivadorInterno.Entero() == idArchivador)
                    GestorDeErrores.Emitir($"No puede quitar del {enumNegocio.Registro.Singular(true)} '{registro.Referencia}' el {enumNegocio.Archivador.Singular(true)} '{archivador.Referencia}'");
            }
        }

        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var registroEs = (RegistroEsDtm)ExtensorDeElementosDeUnProceso.NuevoDtm(Negocio.TipoDtm(), idCg, idTipo, nombre, descripcion, parametros);
            return registroEs;
        }
    }


}
