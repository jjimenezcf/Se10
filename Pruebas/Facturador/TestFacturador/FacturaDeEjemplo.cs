namespace TestFacturador;

public class LineaFactura
{
    public int Orden { get; set; }
    public string? TipoDeLinea { get; set; }
    public string? Concepto { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? Precio { get; set; }
    public string? Anotacion { get; set; }
    public decimal? Descuento { get; set; }
    public string? Iva { get; set; }
    public string? Unidad { get; set; }
    public string? Naturaleza { get; set; }
    public string? Clase { get; set; }
}

public class FacturaBody
{
    public string? NifDelCliente { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? eMail { get; set; }
    public List<LineaFactura> Lineas { get; set; } = new();
}

public static class FacturaDeEjemplo
{
    public static FacturaBody Construir() => new()
    {
        NifDelCliente = "27485405Z",
        Nombre = "Prueba del facturador",
        Descripcion = "Factura por servicios de consultoría y licencia.",
        Contacto = "Juan",
        Telefono = "915551234",
        eMail = "juan@ejemplo.com",
        Lineas = new List<LineaFactura>
        {
            new()
            {
                Orden = 1,
                TipoDeLinea = "Alzada",
                Concepto = "Licencia anual de software de gestión (QLIK)",
                Cantidad = 1.00m,
                Precio = 1250.00m,
                Anotacion = "Licencia del 01/01 al 31/12",
                Descuento = 0.00m,
                Iva = "21",
                Unidad = "Unidad",
                Naturaleza = "Servicios",
                Clase = "Servicio"
            },
            new()
            {
                Orden = 2,
                TipoDeLinea = "Alzada",
                Concepto = "Servicio de consultoría e implementación",
                Cantidad = 20.00m,
                Precio = 85.00m,
                Anotacion = "20 horas a 85€/hora",
                Descuento = 0.00m,
                Iva = "21",
                Unidad = "Hora",
                Naturaleza = "Servicios",
                Clase = "Servicio"
            },
            new()
            {
                Orden = 3,
                TipoDeLinea = "Comentario",
                Concepto = "NOTA: Todos los precios son sin IVA.",
                Cantidad = null,
                Precio = null,
                Anotacion = null,
                Descuento = null,
                Iva = null,
                Unidad = null,
                Naturaleza = null,
                Clase = null
            }
        }
    };
}
