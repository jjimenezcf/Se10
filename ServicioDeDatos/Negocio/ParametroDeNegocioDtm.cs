using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Negocio
{

    [Table(Tablas.PARAMETRO, Schema = Esquemas.NEGOCIO)]
    public class ParametroDeNegocioDtm : RegistroConNombreDtm, IRegistroDeParametrizacion, ITieneCampoNegocio, INecesitaSerParametrizador
    {
        public string Valor { get; set; }
        public int IdNegocio { get; set; }
        public NegocioDtm Negocio { get; set; }

    }

    public static class ParametroDeNegocioSql
    {
        private const string _consultaSql = @$"
                                SELECT {ICampos.ID} as {nameof(ParametroDeNegocioDtm.Id)}, 
                                       {ICampos.ID_NEGOCIO} as {nameof(ParametroDeNegocioDtm.IdNegocio)},
                                       {ICampos.NOMBRE} as {nameof(ParametroDeNegocioDtm.Nombre)}, 
                                       {ICampos.VALOR} as {nameof(ParametroDeNegocioDtm.Valor)}
                                FROM {Esquemas.NEGOCIO}.{Tablas.PARAMETRO} with(NOLOCK)
                                WHERE {ICampos.NOMBRE} LIKE '[parametro]'
                                AND {ICampos.ID_NEGOCIO} = (SELECT {ICampos.ID} FROM {Esquemas.NEGOCIO}.{Tablas.NEGOCIO} WHERE {ICampos.ENUMERADO} LIKE '[negocio]')";


        public static ParametroDeNegocioDtm Parametro(this enumNegocio negocio, System.Enum parametro, bool emitirError = true, bool crearParametro = false, object valorPorDefecto = null)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_ParametrosDeNegocio);
            var indice = $"{negocio}-{parametro}";

            if (!cache.ContainsKey(indice))
            {
                var consultaSql = _consultaSql
                   .Replace($"[{nameof(parametro)}]", parametro.ToString())
                   .Replace($"[{nameof(negocio)}]", negocio.ToString());

                var consulta = new ConsultaSql<ParametroDeNegocioDtm>(consultaSql);
                var resultado = consulta.LanzarConsulta(new DynamicParameters());

                if (resultado.Count == 0)
                {
                    if (crearParametro || valorPorDefecto != null)
                    {
                        negocio.Definir(parametro, valorPorDefecto.ToString());
                        return negocio.Parametro(parametro, true);
                    }

                    if (emitirError && !CacheDeVariable.Cfg_CrearEtapa)
                        GestorDeErrores.Emitir($"No se localiza el parámetro '{parametro}' en el negocio {negocio}");

                    if (parametro.ToString().Contains("_Etapa_") && CacheDeVariable.Cfg_CrearEtapa)
                    {
                        negocio.Definir(parametro, ltrEstados.EstadoNulo);
                        return negocio.Parametro(parametro, emitirError);
                    }
                    else
                        return null;
                }

                if (resultado.Count > 1)
                    GestorDeErrores.Emitir($"Hay más de un registros para el '{parametro}' en el negocio {negocio}");

                cache[indice] = resultado[0];
            }
            return (ParametroDeNegocioDtm)cache[indice];
        }

        public static void Crear(int idNegocio, System.Enum parametro, string valor)
        {
            string CrearParametro = $"INSERT INTO  {Esquemas.NEGOCIO}.{Tablas.PARAMETRO} ({ICampos.VALOR}, {ICampos.ID_NEGOCIO}, {ICampos.NOMBRE}) VALUES(@{ICampos.VALOR}, @{ICampos.ID_NEGOCIO}, @{ICampos.NOMBRE})";
            var valores = new Dictionary<string, object> { { $"@{ICampos.VALOR}", valor }, { $"@{ICampos.ID_NEGOCIO}", idNegocio }, { $"@{ICampos.NOMBRE}", parametro.ToString() } };
            var sentencia = new ConsultaSql<ParametroDeNegocioDtm>(CrearParametro);
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
        }

        public static void Actualizar(this enumNegocio negocio, System.Enum parametro, object valor)
        {
            var negocioDtm = NegocioSqls.LeerNegocioPorEnumerado(negocio);
            Actualizar(negocioDtm.Id, parametro, valor.ToString());
        }

        public static void Actualizar(int idNegocio, System.Enum parametro, string valor)
        {
            string CrearParametro =
                  $@"UPDATE {Esquemas.NEGOCIO}.{Tablas.PARAMETRO} 
                     SET {ICampos.VALOR} = @{ICampos.VALOR}
                     WHERE {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO} 
                      AND  {ICampos.NOMBRE} = @{ICampos.NOMBRE}";
            var valores = new Dictionary<string, object> { { $"@{ICampos.VALOR}", valor }, { $"@{ICampos.ID_NEGOCIO}", idNegocio }, { $"@{ICampos.NOMBRE}", parametro.ToString() } };
            var sentencia = new ConsultaSql<ParametroDeNegocioDtm>(CrearParametro);
            sentencia.EjecutarSentencia(new DynamicParameters(valores));

            var negocio = NegocioSqls.LeerPorId(idNegocio);
            var indice = $"{negocio.Enumerado}-{parametro}";
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_ParametrosDeNegocio, indice);
        }

        public static string DefinirEtapaSiLoIndicaConfiguracion(this enumNegocio negocio, Enum etapa, string valor)
        {
            if (CacheDeVariable.Cfg_CrearEtapa)
                return negocio.Definir(etapa, valor);

            throw new Exception($"No se ha definido la etapa '{etapa}'");
        }

        public static string Definir(this enumNegocio negocio, System.Enum parametro, string valor)
        {
            var negocioDtm = NegocioSqls.LeerNegocioPorEnumerado(negocio);
            Crear(negocioDtm.Id, parametro, valor);
            return valor;
        }

        public static string Resetear(this enumNegocio negocio, System.Enum parametro, string valor)
        {
            var consultaSql = _consultaSql
               .Replace($"[{nameof(parametro)}]", parametro.ToString())
               .Replace($"[{nameof(negocio)}]", negocio.ToString());

            var consulta = new ConsultaSql<ParametroDeNegocioDtm>(consultaSql);
            var resultado = consulta.LanzarConsulta(new DynamicParameters());

            var negocioDtm = NegocioSqls.LeerNegocioPorEnumerado(negocio);
            if (resultado.Count == 0)
            {
                Crear(negocioDtm.Id, parametro, valor);
            }
            else
            {
                Actualizar(negocioDtm.Id, parametro, valor);
            }
            return valor;
        }

        public static void IndicarQueFaltaDefinirElParámetro<T>(this enumNegocio negocio, T parametro)
        where T : struct, Enum
        =>
        GestorDeErrores.Emitir($"Defina el parametro '{parametro}' que sirve para indicar '{parametro.Descripcion()}' en el negocio de '{negocio.Singular()}'");

    }

    public static partial class ModeloDeNegocio
    {
        public static void ParametrosDeNegocio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParametroDeNegocioDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<ParametroDeNegocioDtm>().Property(p => p.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(true);

            //modelBuilder.Entity<ParametroDeNegocioDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired(true);

            DefinirCampoNegocio<ParametroDeNegocioDtm>(modelBuilder, unico: false);

            //modelBuilder.Entity<ParametroDeNegocioDtm>()
            //.HasOne(p => p.Negocio)
            //.WithMany()
            //.IsRequired(true)
            //.HasForeignKey(p => p.IdNegocio)
            //.HasConstraintName("FK_NEGOCIO_PARAMETRO_ID_NEGOCIO")
            //.OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParametroDeNegocioDtm>()
               .HasIndex(p => new { p.IdNegocio, p.Nombre })
               .IsUnique(true)
               .HasDatabaseName("IX_NEGOCIO_ID_NEGOCIO_NOMBRE");

        }
    }

}
