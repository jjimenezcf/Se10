using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public class RegistroDtm : IRegistro
    {
        [Key]
        [Column(nameof(ICampos.ID), TypeName = nameof(IDominio.INT))]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
    public abstract class RegistroConNombreDtm : RegistroDtm, INombre
    {
        [Required]
        [Column(nameof(ICampos.NOMBRE), TypeName = nameof(IDominio.VARCHAR_250))]
        public string Nombre { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public virtual string Expresion => Nombre;
    }

    public static class ApiDeRegistroDtm
    {

        public static string EsquemaTabla(Type t) => $"{EsquemaDeTabla(t)}.{NombreDeTabla(t)}";

        public static string NombreDeTabla(Type t)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.NombreDeTabla);
            if (!cache.ContainsKey(t.FullName))
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(t);
                var b = false;
                foreach (var attr in attrs.Where(attr => attr is TableAttribute))
                {
                    var tabla = (TableAttribute)attr;
                    cache[t.FullName] = tabla.Name;
                    b = true;
                    break;
                }
                if (!b) throw new Exception($"No se ha definido el nombre de la tabla de la clase {t.Name}");
            }
            return (string)cache[t.FullName];
        }

        private static bool RepresentaUnaTabla(Type t)
        {
            var cache = ServicioDeCaches.Obtener(nameof(RepresentaUnaTabla));
            var cacheDeTablas = ServicioDeCaches.Obtener(nameof(NombreDeTabla));
            if (!cache.ContainsKey(t.FullName))
            {
                if (!t.Name.EndsWith("Dtm"))
                {
                    cache[t.FullName] = false;
                    return (bool)cache[t.FullName];
                }

                Attribute[] attrs = Attribute.GetCustomAttributes(t);
                foreach (var attr in attrs.Where(attr => attr is TableAttribute))
                {
                    var tabla = (TableAttribute)attr;
                    cache[t.FullName] = true;
                    cacheDeTablas[t.FullName] = tabla.Name;
                    return (bool)cache[t.FullName];
                }
                cache[t.FullName] = false;
            }
            return (bool)cache[t.FullName];
        }

        public static object CrearDtm<TEntity>(string tabla)
        {
            var ensamblado = Assembly.GetExecutingAssembly();
            if (tabla.Contains(".")) tabla = tabla.Split(".")[1];
            var cache = ServicioDeCaches.Obtener(nameof(CrearDtm));
            if (!cache.ContainsKey(tabla))
            {
                var tipos = ensamblado.GetTypes();
                foreach (var tipo in tipos)
                {
                    if (RepresentaUnaTabla(tipo))
                    {
                        var tablaDelTipo = NombreDeTabla(tipo);
                        if (tablaDelTipo == tabla)
                        {
                            cache[tabla] = tipo;
                            return Activator.CreateInstance((Type)cache[tabla]);
                        }
                    }
                }
                throw new Exception($"No se encuentra el tipo asociado a la tabla {tabla}");
            }
            return Activator.CreateInstance((Type)cache[tabla]);
        }

        public static string EsquemaDeTabla(Type t)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.EsquemaDeTabla);
            if (!cache.ContainsKey(t.FullName))
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(t);
                var b = false;
                foreach (var attr in attrs.Where(attr => attr is TableAttribute))
                {
                    var tabla = (TableAttribute)attr;
                    cache[t.FullName] = tabla.Schema;
                    b = true;
                    break;
                }
                if (!b)
                    throw new Exception($"No se ha definido el esquema de la tabla de la clase {t.Name}");
            }
            return (string)cache[t.FullName];
        }

        public static TRegistro RegistroVacio<TRegistro>()
        {
            var className = typeof(TRegistro).FullName;
            var assembly = Assembly.GetAssembly(Type.GetType(className));
            var type = assembly.GetType(className);

            //Constructor genérico           
            var constructorSinParametros = type.GetConstructor(Type.EmptyTypes);

            //Creamos el objeto de manera dinámica
            return (TRegistro)constructorSinParametros.Invoke(new object[] { });
        }
        public static IRegistro RegistroVacio(Type tipo)
        {
            var className = tipo.FullName;
            var assembly = Assembly.GetAssembly(Type.GetType(className));
            var type = assembly.GetType(className);

            //Constructor genérico           
            var constructorSinParametros = type.GetConstructor(Type.EmptyTypes);

            //Creamos el objeto de manera dinámica
            return (IRegistro)constructorSinParametros.Invoke(new object[] { });
        }

        public static Type ObtenerTypoDtm(string tipoDtm, bool emitirError = true)
        {
            var cache = ServicioDeCaches.Obtener(nameof(ObtenerTypoDtm));
            if (!cache.ContainsKey(tipoDtm))
                cache[tipoDtm] = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelServicioDeDatos, tipoDtm, emitirError);

            return (Type)cache[tipoDtm];
        }

        internal static void DefinirDependenciaConAkPorNombre<T, P>(ModelBuilder modelBuilder,
          string apuntadoPor,
          string idCampo)
        where T : RegistroDtm
        where P : RegistroDtm
        {
            var tabla = DefinirDependencia<T, P>(modelBuilder, apuntadoPor, idCampo, true, false);
            modelBuilder.Entity<T>().HasAlternateKey(new string[] { apuntadoPor, nameof(INombre.Nombre) }).HasName($"AK_{tabla}_{idCampo}_{ICampos.NOMBRE}");
        }

        internal static void DefinirDependenciaConIndPorNombre<T, P>(ModelBuilder modelBuilder,
          string apuntadoPor,
          string idCampo)
        where T : RegistroDtm
        where P : RegistroDtm
        {
            var tabla = DefinirDependencia<T, P>(modelBuilder, apuntadoPor, idCampo, true, false);
            modelBuilder.Entity<T>().HasIndex(new string[] { apuntadoPor, nameof(INombre.Nombre) }).HasDatabaseName($"I_{tabla}_{idCampo}_{ICampos.NOMBRE}").IsUnique(true);
        }

        internal static string DefinirDependencia<TEntity, TPadre>(ModelBuilder modelBuilder,
            string apuntadoPor,
            string idCampo,
            bool requerido = true,
            bool unico = false)
        where TEntity : RegistroDtm
        where TPadre : RegistroDtm
        {
            var nombreDeTabla = NombreDeTabla(typeof(TEntity));

            var dependeDe = apuntadoPor.Substring(2, apuntadoPor.Length - 2);

            modelBuilder.Entity<TEntity>().Property(apuntadoPor)
                .HasColumnName(idCampo)
                .HasColumnType(IDominio.INT)
                .IsRequired(requerido);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(apuntadoPor)
                        .IsUnique(unico)
                        .HasDatabaseName($"I_{nombreDeTabla}_{idCampo}");

            modelBuilder.Entity<TEntity>()
                        .HasOne<TPadre>(dependeDe)
                        .WithMany()
                        .HasForeignKey(apuntadoPor)
                        .HasConstraintName($"FK_{nombreDeTabla}_{idCampo}")
                        .OnDelete(DeleteBehavior.Restrict);

            return nombreDeTabla;
        }

        internal static string DefinirDependencia<TEntity>(ModelBuilder modelBuilder,
            string dependeDe,
            string apuntadoPor,
            string idCampo,
            bool requerido = true,
            bool unico = false)
        where TEntity : RegistroDtm
        {

            var nombreDeTabla = DefinirCampoFk<TEntity>(modelBuilder, dependeDe, apuntadoPor, idCampo, requerido, unico);

            //modelBuilder.Entity<TEntity>().Property(apuntadoPor)
            //    .HasColumnName(idCampo)
            //    .HasColumnType(IDominio.INT)
            //    .IsRequired(requerido);

            //modelBuilder.Entity<TEntity>()
            //            .HasIndex(apuntadoPor)
            //            .IsUnique(unico)
            //            .HasDatabaseName($"I_{nombreDeTabla}_{idCampo}");

            //modelBuilder.Entity<TEntity>()
            //            .HasOne(dependeDe)
            //            .WithMany()
            //            .HasForeignKey(apuntadoPor)
            //            .HasConstraintName($"FK_{nombreDeTabla}_{idCampo}")
            //            .OnDelete(DeleteBehavior.Restrict);

            return nombreDeTabla;
        }

        internal static void DefinirCampoIdDtm<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>().Property(p => p.Id).UseIdentityColumn().HasColumnName(ICampos.ID).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<TEntity>().HasKey(p => p.Id);
        }

        internal static void DefinirCampoBaja<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaBaja.Baja))
                .HasColumnName(ICampos.BAJA)
                .HasColumnType(IDominio.BIT).IsRequired(true)
                .HasDefaultValue(false);
        }

        internal static string DefinirCampoFk<TEntidad>(ModelBuilder modelBuilder, string propiedadReferenciada, string idReferenciado, string campo, bool requerida, bool unico)
            where TEntidad : RegistroDtm
        {
            modelBuilder.Entity<TEntidad>().Property(idReferenciado).HasColumnName(campo).HasColumnType(IDominio.INT).IsRequired(requerida);
            return DefinirFk<TEntidad>(modelBuilder, propiedadReferenciada, idReferenciado, campo, unico);
        }

        internal static void DefinirCampoFkConMuchos<TEntidad>(ModelBuilder modelBuilder, string propiedadReferenciada, string conMuchos, string idReferenciado, string campo, bool requerida, bool unico)
        where TEntidad : RegistroDtm
        {
            modelBuilder.Entity<TEntidad>().Property(idReferenciado).HasColumnName(campo).HasColumnType(IDominio.INT).IsRequired(requerida);
            DefinirFkConMuchos<TEntidad>(modelBuilder, propiedadReferenciada, conMuchos, idReferenciado, campo, unico);
        }

        internal static void DefinirFkConMuchos<TEntity>(ModelBuilder modelBuilder, string propiedadReferenciada, string idReferenciado, string conMuchos, string campo, bool unico) where TEntity : RegistroDtm
        {
            var tabla = NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedadReferenciada)
                        .WithMany(conMuchos)
                        .HasForeignKey(idReferenciado)
                        .HasConstraintName($"FK_{tabla}_{campo}")
                        .OnDelete(DeleteBehavior.Restrict);

            DefinirIndiceFk<TEntity>(modelBuilder, idReferenciado, campo, unico, tabla);
        }

        internal static void DefinirFkUnoUno<TEntity>(ModelBuilder modelBuilder, string elemento, string idReferenciado, string conUno, string campo) where TEntity : RegistroDtm
        {
            var tabla = NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>()
                .HasOne(elemento)
                .WithOne(conUno)
                .HasForeignKey(typeof(TEntity), idReferenciado)
                .HasConstraintName($"FK_{tabla}_{campo}")
                .OnDelete(DeleteBehavior.Restrict);

            DefinirIndiceFk<TEntity>(modelBuilder, idReferenciado, campo, unico: true, tabla);
        }

        internal static string DefinirFk<TEntity>(ModelBuilder modelBuilder, string propiedadReferenciada, string idReferenciado, string campo, bool unico) where TEntity : RegistroDtm
        {
            var tabla = NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedadReferenciada)
                        .WithMany()
                        .HasForeignKey(idReferenciado)
                        .HasConstraintName($"FK_{tabla}_{campo}")
                        .OnDelete(DeleteBehavior.Restrict);

            DefinirIndiceFk<TEntity>(modelBuilder, idReferenciado, campo, unico, tabla);
            return tabla;
        }


        internal static string DefinirCampoFk<TEntidad, TReferenciada>(ModelBuilder modelBuilder, string idReferenciado, string campo, bool requerida, bool unico)
        where TEntidad : RegistroDtm
        where TReferenciada : RegistroDtm
        {
            modelBuilder.Entity<TEntidad>().Property(idReferenciado).HasColumnName(campo).HasColumnType(IDominio.INT).IsRequired(requerida);
            return DefinirFk<TEntidad, TReferenciada>(modelBuilder, idReferenciado, campo, unico);
        }


        internal static string DefinirFk<TEntity, TReferenciada>(ModelBuilder modelBuilder, string idReferenciador, string campo, bool unico)
        where TEntity : RegistroDtm
        where TReferenciada : RegistroDtm
        {
            var tabla = NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>()
                        .HasOne<TReferenciada>()
                        .WithMany()
                        .HasForeignKey(idReferenciador)
                        .HasConstraintName($"FK_{tabla}_{campo}")
                        .OnDelete(DeleteBehavior.Restrict);

            DefinirIndiceFk<TEntity>(modelBuilder, idReferenciador, campo, unico, tabla);
            return tabla;
        }

        internal static void DefinirFkSobreUnaAmpliacion<TEntity, TAmpliacion>(ModelBuilder modelBuilder, string ampliacion)
        where TEntity : RegistroDtm
        where TAmpliacion : RegistroDtm
        {
            modelBuilder.Entity<TEntity>()
                        .HasOne<TAmpliacion>(ampliacion)
                        .WithOne(nameof(TrazaDtm.Elemento))
                        .HasForeignKey<TAmpliacion>(nameof(IAmpliacion.IdElemento));
        }

        internal static void DefinirIndiceFk<TEntity>(ModelBuilder modelBuilder, string idReferenciado, string campo, bool unico, string tabla)
        where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>()
                        .HasIndex(new string[] { idReferenciado })
                        .IsUnique(unico)
                        .HasDatabaseName($"I_{tabla}_{campo}");
        }

        internal static void DefinirCliente<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {

            if (!typeof(TEntity).ImplementaPuedeUsarCliente() && !typeof(TEntity).ImplementaUsaCliente())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa ni {nameof(IPuedeUsarCliente)} ni  {nameof(IUsaCliente)}");

            DefinirDependencia<TEntity>(modelBuilder, nameof(IPuedeUsarCliente.Cliente), nameof(IPuedeUsarCliente.IdCliente), ICampos.ID_CLIENTE,
                requerido: typeof(TEntity).ImplementaUsaCliente());

            if (typeof(TEntity).ImplementaPuedeUsarCliente())
                modelBuilder.Entity<TEntity>().Property(p => ((IPuedeUsarCliente)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250)
                .IsRequired(false);
            else
                modelBuilder.Entity<TEntity>().Property(p => ((IUsaCliente)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250)
                .IsRequired(true);

            DefinirDatosDeContacto<TEntity>(modelBuilder, requerido: typeof(TEntity).ImplementaUsaCliente());
        }

        internal static void DefinirProveedor<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {

            if (!typeof(TEntity).ImplementaPuedeUsarProveedor() && !typeof(TEntity).ImplementaUsaProveedor())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa ni {nameof(IPuedeUsarProveedor)} ni  {nameof(IUsaProveedor)}");


            DefinirDependencia<TEntity>(modelBuilder, nameof(IPuedeUsarProveedor.Proveedor), nameof(IPuedeUsarProveedor.IdProveedor), ICampos.ID_PROVEEDOR
                , requerido: typeof(TEntity).ImplementaUsaProveedor());

            //Para poder definir correctamente entidades que pueden usar un proveedor o un solicitante
            if (!typeof(TEntity).ImplementaUsaSolicitante())
            {
                if (typeof(TEntity).ImplementaPuedeUsarProveedor())
                    modelBuilder.Entity<TEntity>().Property(p => ((IPuedeUsarProveedor)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250)
                    .IsRequired(false);
                else
                    modelBuilder.Entity<TEntity>().Property(p => ((IUsaProveedor)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250)
                    .IsRequired(true);

                DefinirDatosDeContacto<TEntity>(modelBuilder, requerido: typeof(TEntity).ImplementaUsaProveedor());
            }
        }

        internal static void DefinirTrabajador<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {

            if (!typeof(TEntity).ImplementaPuedeUsarTrabajador() && !typeof(TEntity).ImplementaUsaTrabajador())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa ni {nameof(IPuedeUsarTrabajador)} ni  {nameof(IUsaTrabajador)}");


            DefinirDependencia<TEntity>(modelBuilder, nameof(IPuedeUsarTrabajador.Trabajador), nameof(IPuedeUsarTrabajador.IdTrabajador), ICampos.ID_TRABAJADOR,
                requerido: typeof(TEntity).ImplementaUsaTrabajador(),
                unico: false);

            //si implementa que usa un trabajador, entonces los datos del contacto son el propio trabajador
            if (!typeof(TEntity).ImplementaUsaTrabajador())
            {
                //Para poder definir correctamente entidades que pueden usar un trabajador o un solicitante
                if (!typeof(TEntity).ImplementaUsaSolicitante())
                {
                    if (typeof(TEntity).ImplementaPuedeUsarTrabajador())
                        modelBuilder.Entity<TEntity>().Property(p => ((IPuedeUsarTrabajador)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
                    else
                        modelBuilder.Entity<TEntity>().Property(p => ((IUsaProveedor)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

                    DefinirDatosDeContacto<TEntity>(modelBuilder, requerido: typeof(TEntity).ImplementaUsaTrabajador());
                }
            }
        }

        internal static void DefinirPantillaDeUsuario<TEntity>(ModelBuilder modelBuilder) where TEntity : PlantillaDeUsuario
        {
            var nombreDeTabla = NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<TEntity>().Property(p => p.Vista).HasColumnName(ICampos.VISTA).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<TEntity>().Property(p => p.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(true);

            DefinirCampoFk<TEntity>(modelBuilder, nameof(PlantillaDeUsuario.Negocio), nameof(PlantillaDeUsuario.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: false);
            DefinirCampoFk<TEntity>(modelBuilder, nameof(PlantillaDeUsuario.Usuario), nameof(PlantillaDeUsuario.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);

            modelBuilder.Entity<TEntity>()
            .HasIndex(p => new { p.IdNegocio, p.IdUsuario, p.Nombre, p.Vista })
            .IsUnique(true)
            .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.NOMBRE}_{ICampos.VISTA}");
        }

        internal static void DefinirDatosDeContacto<TEntity>(ModelBuilder modelBuilder, bool requerido = true) where TEntity : RegistroDtm
        {
            if (!typeof(TEntity).ImplementaDatosDeContacto())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa {nameof(IDatosDeContacto)}");

            modelBuilder.Entity<TEntity>().Property(p => ((IDatosDeContacto)p).Telefono).HasColumnName(ICampos.TELEFONO).HasColumnType(IDominio.VARCHAR_15).IsRequired(requerido);
            modelBuilder.Entity<TEntity>().Property(p => ((IDatosDeContacto)p).eMail).HasColumnName(ICampos.EMAIL).HasColumnType(IDominio.VARCHAR_50).IsRequired(requerido);
        }

        public static List<Type> EncontrarTiposQueReferencian<T>()
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fija_TiposReferenciados);
            var indice = typeof(T).FullName;
            if (!cache.ContainsKey(indice))
            {
                var ensamblado = Assembly.Load(nameof(ServicioDeDatos));
                cache[indice] = ensamblado.GetTypes()
                    .Where(p => p.IsClass && p.IsSubclassOf(typeof(RegistroDtm)))
                    .Where(t => t.GetProperties()
                        .Any(prop => prop.PropertyType == typeof(T) ||
                                     (prop.PropertyType.IsGenericType &&
                                      prop.PropertyType.GetGenericArguments().Contains(typeof(T)))))
                    .ToList();
            }

            return (List<Type>)cache[indice];
        }

        public static bool HayReferenciasA(this IRegistro registro, ContextoSe contexto)
        =>
        registro.NumeroDeReferencias(contexto) > 0;

        public static int NumeroDeReferencias(this IRegistro registro, ContextoSe contexto, bool excluirAuditoria = true)
        {
            Type tipo = registro.GetType();
            List<ReferenciaFk> referencias = GestorDeMetadatos.ReferenciasA(EsquemaDeTabla(tipo), NombreDeTabla(tipo));
            var consulta = "";
            var parametrosSql = new Dictionary<string, object>();
            foreach (ReferenciaFk referencia in referencias)
            {
                if (excluirAuditoria && referencia.Tabla.EndsWith(Sufijo.AUDITORIA))
                    continue;

                string query = $"SELECT COUNT(*) as {nameof(RegistrosAfectados.cantidad)} FROM {referencia.Esquema}.{referencia.Tabla} WHERE {referencia.Campo} = @{referencia.Campo}";
                parametrosSql[$"@{referencia.Campo}"] = registro.Id;
                consulta = consulta.IsNullOrEmpty() ? query : consulta + $"{Environment.NewLine}union{Environment.NewLine}" + query;
            }
            if (consulta.IsNullOrEmpty()) return 0;
            consulta = $"Select Sum({nameof(RegistrosAfectados.cantidad)}) as {nameof(RegistrosAfectados.cantidad)} from ({Environment.NewLine}{consulta}{Environment.NewLine}) t1";
            var registros = new ConsultaSql<RegistrosAfectados>(contexto, consulta).LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }
    }

    public static class ApiDeNombreDtm
    {
        internal static void DefinirCampoNombreDtm<TEntity>(ModelBuilder modelBuilder, int tamano = 250, string indice = "", bool unico = false, bool conIndice = true) where TEntity : RegistroConNombreDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType($"{IDominio.VARCHAR}({tamano})").IsRequired();

            if (conIndice) modelBuilder.Entity<TEntity>()
                   .HasIndex(p => p.Nombre)
                   .HasDatabaseName(indice.IsNullOrEmpty() ? $"I_{nombreDeTabla}_{ICampos.NOMBRE}" : indice).IsUnique(unico);
        }

        internal static void DefinirCampoDescripcion<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroConNombreDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaDescripcion.Descripcion))
                .HasColumnName(ICampos.DESCRIPCION)
                .HasColumnType(IDominio.VARCHAR)
                .HasMaxLength(IDominio.Longitud(IDominio.VARCHAR_2000))
                .IsRequired(false);
        }

        internal static void DefinirCampoDeSigla<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(ILibroDeRegistro.Sigla)).HasColumnName(ICampos.SIGLA).HasColumnType(IDominio.VARCHAR_5).IsRequired();
        }
    }

}