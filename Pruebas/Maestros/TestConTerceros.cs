using System;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Terceros;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using Utilidades;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    public class TestConTerceros
    {
        [Test]
        public void ValidarNif()
        {
            var valido = ApiDeTerceros.ValidarNif("27485405Z");
            if (valido.IsNullOrEmpty())
                Assert.Pass("Nif válido");
            else
                Assert.Fail(valido);
        }

        [Test]
        public void CrearContacto()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {

                var sociedad = CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.correspondencia);
                ContactoDto contactoDto = new ContactoDto();
                contactoDto.Nombre = "Pepe el de las bombas";
                contactoDto.IdElemento = sociedad.Id;
                contactoDto.eMail = "jj@gmail.com";
                contactoDto.Telefono = "619702547";
                contactoDto.CrearInterlocutor = true;
                GestorDeContactos.Gestor(contexto, contexto.Mapeador).PersistirElementoDto(contactoDto, new ParametrosDeNegocio(enumTipoOperacion.Insertar));

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        [Test]
        public void CrearSociedad()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.correspondencia);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba); ;
        }

        public static SociedadDto CrearSociedadConInterlocutorYCalleDto(ContextoSe contexto, enumCalificadorDireccion calificadorDireccion)
        {
            SociedadDto sociedadDto = new SociedadDto();
            sociedadDto.Nombre = "Pepe el de las bombas";
            sociedadDto.Nif = "27485405Z";
            sociedadDto.eMail = "jj@gmail.com";
            sociedadDto.Telefono = "619702547";
            sociedadDto.CrearInterlocutor = true;
            var soc = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), sociedadDto.Nif, errorSiNoHay: false);
            if (soc == null)
                sociedadDto = GestorDeSociedades.Gestor(contexto, contexto.Mapeador).PersistirElementoDto(sociedadDto, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            else
                sociedadDto = soc.MapearDto<SociedadDto>(contexto);

            AsociarDireccionDe(contexto, sociedadDto, calificadorDireccion);

            return sociedadDto;
        }

        private static DireccionDtm AsociarDireccionDe(ContextoSe contexto, SociedadDto sociedadDto, enumCalificadorDireccion calificador)
        {
            var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
            if (calles.Count == 0) return null;
            DireccionDtm direccion = calles[0].CrearDireccion(calificador);

            var sociedadDtm = sociedadDto.MapearDtm<SociedadDtm>(contexto);
            sociedadDtm.AsignarDireccion(contexto, direccion);

            return direccion;
        }

        public static DireccionDtm CrearDireccion(CalleDtm calle, enumCalificadorDireccion calificadorDireccion )
        {
            DireccionDtm direccion = new DireccionDtm();
            direccion.IdPais = calle.Municipio.Provincia.IdPais;
            direccion.IdProvincia = calle.Municipio.IdProvincia;
            direccion.IdMunicipio = calle.IdMunicipio;
            direccion.IdCalle = calle.Id;
            direccion.Calificador = calificadorDireccion;
            return direccion;
        }

        [Test]
        public void NoDarAltaInterlocutorConPersonaDeBaja()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            var tran = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza(nameof(NoDarAltaInterlocutorConPersonaDeBaja));
                var persona = contexto.SeleccionarPorPropiedad<PersonaDtm>(nameof(PersonaDtm.NIF), "27485405Z", false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });
                if (persona == null)
                {
                    persona = new PersonaDtm();
                    persona.Nombre = "Juan";
                    persona.Apellidos = "Jiménez";
                    persona.NIF = "27485405Z";
                    persona.eMail = "jjimenezc@gmail.com";
                    persona.Telefono = "619702547";
                    persona = persona.Insertar(contexto);
                }
                else
                {
                    if (persona.Baja)
                    {
                        persona.Baja = false;
                        persona = persona.Modificar(contexto);
                    }
                }

                var interlocutor = persona.CrearInterlocutor(contexto, errorSihay: false);  //ExtensorDeInterlocutores.CrearInterlocutor(contexto, persona.NIF);
                if (!persona.Baja)
                {
                    persona.Baja = true;
                    try
                    {
                        persona = persona.Modificar(contexto);
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Equals("No se puede dar de baja un Trabajador con partes de trabajo pendientes"))
                            throw;
                    }
                }
                interlocutor.Baja = false;
                try
                {
                    interlocutor.Modificar(contexto);
                }
                catch (Exception e)
                {
                    if (!e.Message.Equals("No se puede dar de alta un interlocutor por estar la persona asociada de baja") &&
                        !e.Message.Equals("No se puede dar de baja un Trabajador con partes de trabajo pendientes"))
                        throw;
                }
                contexto.Rollback(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                Assert.Fail(e.Message);
            }
            finally
            {
                contexto.CerrarTraza();
            }
            Assert.IsTrue(true);
        }

        public static TrabajadorDtm CrearTrabajador(ContextoSe contexto, CentroGestorDtm cg, int? idUsuario, string nif, string nombre, string apellidos, string email, string telefono)
        {
            var personaDto = GestorDePersonas.CrearPersona(contexto, nif, nombre, apellidos, email, telefono);
            var personaDtm = personaDto.MapearDtm<PersonaDtm>(contexto);
            var interlocutor = ExtensorDePersonas.Interlocutor(personaDtm, contexto, crearSiNoLoHay: false);
            return interlocutor.Trabajador(contexto, cg, idUsuario, crearTrabajador: true);
        }

    }
}