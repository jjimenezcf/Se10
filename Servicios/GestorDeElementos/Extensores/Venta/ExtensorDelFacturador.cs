using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDelFacturador
    {
        public static FacturadorDeSociedadDtm  FacturadorDeUnGuid(this ContextoSe contexto, Guid guid, bool errorSiNoHay = true)
        {
           var peticion = contexto.SeleccionarPorPropiedad<PeticionDeFacturaEmtDtm>(nameof(PeticionDeFacturaEmtDtm.Guid), guid, errorSiNoHay: false);
            if (peticion == null)
                GestorDeErrores.Emitir("o se ha localizado el facturador asociado al Guid proporcionado");
            return contexto.SeleccionarPorId<FacturadorDeSociedadDtm>(peticion.IdFacturador);
        }


        public static FacturadorDeSociedadDtm Facturador(this PeticionDeFacturaEmtDtm peticion, ContextoSe contexto)
        {
            if (peticion.Facturador is not null && peticion.Facturador.Id == peticion.IdFacturador)
                return peticion.Facturador;

            peticion.Facturador = contexto.SeleccionarPorId<FacturadorDeSociedadDtm>(peticion.IdFacturador);
            return peticion.Facturador;
        }

        public static SociedadDtm Sociedad(this PeticionDeFacturaEmtDtm peticion, ContextoSe contexto)
        {
            if (peticion.Facturador is not null && peticion.Facturador.Id == peticion.IdFacturador)
                return peticion.Facturador.Sociedad(contexto);

            peticion.Facturador = contexto.SeleccionarPorId<FacturadorDeSociedadDtm>(peticion.IdFacturador);
            return peticion.Facturador.Sociedad(contexto);
        }

        public static SociedadDtm Sociedad(this FacturadorDeSociedadDtm facturador, ContextoSe contexto)
        {
            if (facturador.Elemento != null && facturador.Elemento.Id == facturador.IdElemento) 
                return facturador.Elemento;

            return facturador.Elemento = contexto.SeleccionarPorId<SociedadDtm>(facturador.IdElemento);
        }

        public static CentroGestorDtm Cg(this FacturadorDeSociedadDtm facturador, ContextoSe contexto)
        {
            if (facturador.Cg is not null && facturador.Cg.Id == facturador.IdCg)
                return facturador.Cg;

            facturador.Cg = contexto.SeleccionarPorId<CentroGestorDtm>(facturador.IdCg);
            return facturador.Cg;
        }


        public static TipoDeFacturaEmtDtm TipoDeFactura(this FacturadorDeSociedadDtm facturador, ContextoSe contexto)
        {
            if (facturador.TipoDeFactura != null && facturador.TipoDeFactura.Id == facturador.IdTipoDeFactura)
                return facturador.TipoDeFactura;
            return facturador.TipoDeFactura = contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>(facturador.IdTipoDeFactura);
        }

        public static string Nombre(this FacturadorDeSociedadDtm facturador, ContextoSe contexto)
        =>
        facturador.Sociedad(contexto).NIF + "." + facturador.Cg(contexto).Codigo + "." + facturador.TipoDeFactura(contexto).Nombre;

        public static FacturaEmtDtm Factura(this PeticionDeFacturaEmtDtm peticion, ContextoSe contexto)
        {
            if (peticion.Factura is not null && peticion.Factura.Id == peticion.IdFactura)
                return peticion.Factura;
            
            if (peticion.IdFactura is null)
                return null;

            peticion.Factura = contexto.SeleccionarPorId<FacturaEmtDtm>((int)peticion.IdFactura);
            return peticion.Factura;
        }

        public static void RegistrarExcepcion(ContextoSe contexto, Guid guid, Exception e)
        {
            var facturador = FacturadorDeUnGuid(contexto, guid);
            var peticion = contexto.SeleccionarPorPropiedad<PeticionDeFacturaEmtDtm>(nameof(PeticionDeFacturaEmtDtm.Guid), guid, errorSiNoHay: false);
            peticion?.RegistrarError(contexto, e.MensajeCompleto());
        }

        public static void RegistrarErrorDePeticion(this FacturaEmtDtm factura, ContextoSe contexto, string error)
        {
            var peticion = contexto.SeleccionarPorPropiedad<PeticionDeFacturaEmtDtm>(nameof(PeticionDeFacturaEmtDtm.IdFactura), factura.Id, errorSiNoHay: false);
            peticion?.RegistrarError(contexto, error);

        }

        public static void RegistrarError(this PeticionDeFacturaEmtDtm peticion, ContextoSe contexto, string error)
        {
            if (peticion.Error is null)
            {
                peticion.Error = error;
                peticion.Modificar(contexto);
            }
        }
    }
}
