using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Entorno
{
    [Table("MENU_SE", Schema = "ENTORNO")]
    public class ArbolDeMenuDtm : RegistroConNombreDtm
    {
        public string Padre { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public int? IdPadre { get; set; }
        public int? IdVistaMvc { get; set; }
        public string Vista { get; set; }
        public string Controlador { get; set; }
        public string accion { get; set; }
        public string parametros { get; set; }
    }

    public static class VistaMenuSe
    {
        public static void Definir(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArbolDeMenuDtm>()
                    .ToView(ApiDeRegistroDtm.NombreDeTabla(typeof(ArbolDeMenuDtm)), ApiDeRegistroDtm.EsquemaDeTabla(typeof(ArbolDeMenuDtm)))
                    .HasKey(x => new {
                        x.Id
                    });

            ApiDeNombreDtm.DefinirCampoNombreDtm<ArbolDeMenuDtm>(modelBuilder, conIndice: false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Padre).HasColumnName(ICampos.PADRE).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeNombreDtm.DefinirCampoDescripcion<ArbolDeMenuDtm>(modelBuilder); 
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Icono).HasColumnName(ICampos.ICONO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Orden).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired(true);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(menu => menu.IdPadre).HasColumnName(ICampos.IDPADRE).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(menu => menu.IdVistaMvc).HasColumnName(ICampos.IDVISTA_MVC).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Vista).HasColumnName(ICampos.VISTA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.Controlador).HasColumnName(ICampos.CONTROLADOR).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.accion).HasColumnName(ICampos.ACCION).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<ArbolDeMenuDtm>().Property(p => p.parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
        }
    }


    public static class ArbolDeMenuSql
    {

        public static List<ArbolDeMenuDtm> LeerArbolDeMenu(ContextoSe contexto)
        {
            string _leer = $@"
            SELECT T1.{ ICampos.ID} as {nameof(ArbolDeMenuDtm.Id)}
                  ,T1.{ ICampos.NOMBRE} as {nameof(ArbolDeMenuDtm.Nombre)}
                  ,T1.{ ICampos.DESCRIPCION} as {nameof(ArbolDeMenuDtm.Descripcion)}
                  ,T1.{ ICampos.ICONO} as {nameof(ArbolDeMenuDtm.Icono)}
                  ,T1.{ ICampos.ACTIVO} as {nameof(ArbolDeMenuDtm.Activo)}
                  ,T1.{ ICampos.IDPADRE} as {nameof(ArbolDeMenuDtm.IdPadre)}
                  ,T1.{ ICampos.IDVISTA_MVC} as {nameof(ArbolDeMenuDtm.IdVistaMvc)}
                  ,T1.{ ICampos.ORDEN} as {nameof(ArbolDeMenuDtm.Orden)}
                  ,T2.{ ICampos.NOMBRE} as {nameof(ArbolDeMenuDtm.Padre)}
                  ,T3.{ ICampos.NOMBRE} AS {nameof(ArbolDeMenuDtm.Vista)}
                  ,T3.{ ICampos.CONTROLADOR} as { nameof(ArbolDeMenuDtm.Controlador)}
                  ,T3.{ ICampos.ACCION} as { nameof(ArbolDeMenuDtm.accion)}
                  ,T1.{ ICampos.PARAMETROS} as { nameof(ArbolDeMenuDtm.parametros)}
            FROM ENTORNO.ARBOL_MENU_POR_USUARIO({contexto.DatosDeConexion.IdUsuario}) AS T1
            LEFT JOIN { ModeloDeEntorno.TablaMenus} T2 ON T2.{ ICampos.ID} = T1.{ ICampos.IDPADRE}
            LEFT JOIN { ModeloDeEntorno.TablaVistasMvc} T3 ON T3.{ ICampos.ID} = T1.{ ICampos.IDVISTA_MVC}
            WHERE T1.{ ICampos.ACTIVO} = 1
            order by t1.{ ICampos.IDPADRE}, T1.{ ICampos.ORDEN}, T1.{ ICampos.NOMBRE}
            ";

            var parametrosSql = new Dictionary<string, object>();
            var sentencia = new ConsultaSql<ArbolDeMenuDtm>(contexto, _leer);
            return sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
        }
    }

}
