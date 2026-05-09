using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace ServicioDeDatos.Elemento
{

    public class RelacionDtm : RegistroDtm, IRelacion
    {
        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelIdElemento1 { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelIdElemento2 { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelElemento1 => PropiedadDelIdElemento1.Replace(nameof(IRegistro.Id), "");
        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelElemento2 => PropiedadDelIdElemento2.Replace(nameof(IRegistro.Id), "");


        [IgnoreDataMember]
        [NotMapped]
        public int IdElemento1 => ObtenerIdElemento(enumDtmsDeRelacion.Negocio1);

        [IgnoreDataMember]
        [NotMapped]
        public int IdElemento2 => ObtenerIdElemento(enumDtmsDeRelacion.Negocio2);

        [IgnoreDataMember]
        [NotMapped]
        public IRegistro Elemento1 => ObtenerElemento(enumDtmsDeRelacion.Negocio1);

        [IgnoreDataMember]
        [NotMapped]
        public IRegistro Elemento2 => ObtenerElemento(enumDtmsDeRelacion.Negocio1);

        private IRegistro ObtenerElemento(enumDtmsDeRelacion negocio)
        {
            var propiedad = negocio == enumDtmsDeRelacion.Negocio1 ? this.PropiedadDelElemento1 : this.PropiedadDelElemento2;
            var p = ApiDeRelaciones.ObtenerPropiedad(this, propiedad);
            if (p != null)
                return p.GetValue(this) as IRegistro;

            throw new Exception($"No se puede localizar el elemento asociado a la propiedad {propiedad}");
        }

        private int ObtenerIdElemento(enumDtmsDeRelacion negocio)
        {
            var propiedad = negocio == enumDtmsDeRelacion.Negocio1 ? this.PropiedadDelIdElemento1 : this.PropiedadDelIdElemento2;
            var p = ApiDeRelaciones.ObtenerPropiedad(this, propiedad);
            if (p != null)
                return p.GetValue(this).ToString().Entero();

            throw new Exception($"No se puede localizar el id elemento asociado a la propiedad {propiedad}");
        }

    }

    public static class ApiDeRelaciones
    {
        public static Type ObtenerTipoDtm<TRelacion>(enumDtmsDeRelacion negocio) where TRelacion : RelacionDtm
        {
            var registro = ApiDeRegistroDtm.RegistroVacio<TRelacion>();
            var propiedad = negocio == enumDtmsDeRelacion.Negocio1 
                ? registro.PropiedadDelElemento1 
                : registro.PropiedadDelElemento2;
            var p = ObtenerPropiedad(registro, propiedad);
            if (p != null)
                return p.PropertyType;

            throw new Exception($"No se puede localizar el tipo dtm del elemento asociado a la propiedad {propiedad}");
        }

        public static Type ObtenerTipoDtm(Type tipoDtm, enumDtmsDeRelacion negocio)
        {
            var registro = ApiDeRegistroDtm.RegistroVacio(tipoDtm);
            var propiedad = negocio == enumDtmsDeRelacion.Negocio1 
                ? ((IRelacion) registro).PropiedadDelElemento1 
                : ((IRelacion) registro).PropiedadDelElemento2;
            var p = ObtenerPropiedad((IRelacion) registro, propiedad);
            if (p != null)
                return p.PropertyType;

            throw new Exception($"No se puede localizar el tipo dtm del elemento asociado a la propiedad {propiedad}");
        }

        internal static PropertyInfo ObtenerPropiedad<TRelacion>(TRelacion objeto, string propiedad) where TRelacion : RelacionDtm
        {
            var propiedades = ApiDeEnsamblados.PropiedadesDelObjeto(objeto);

            foreach (var p in propiedades)
                if (p.Name.Equals(propiedad, StringComparison.CurrentCultureIgnoreCase))
                    return p;

            return null;
        }
        internal static PropertyInfo ObtenerPropiedad(IRelacion objeto, string propiedad)
        {
            var propiedades = ApiDeEnsamblados.PropiedadesDelObjeto(objeto);

            foreach (var p in propiedades)
                if (p.Name.Equals(propiedad, StringComparison.CurrentCultureIgnoreCase))
                    return p;

            return null;
        }

        internal static void DefinirRelacion<T>(ModelBuilder modelBuilder, string propiedad1, string propiedad2, string campo1, string campo2)
        where T : RelacionDtm
        {
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(T));
            var id1 = $"Id{propiedad1}";
            var id2 = $"Id{propiedad2}";
            ApiDeRegistroDtm.DefinirCampoFk<T>(modelBuilder,
                propiedad1,
                id1,
                campo1,
                requerida: true,
                unico: false); ;

            ApiDeRegistroDtm.DefinirCampoFk<T>(modelBuilder,
                propiedad2,
                id2,
                campo2,
                requerida: true,
                unico: false); ;

            modelBuilder.Entity<T>().HasAlternateKey(new string[] {id1,id2 }).HasName($"AK_{tabla}_{campo1}_{campo2}");

        }
}

}
