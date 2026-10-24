using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores.Contabilidad
{
    using Gestor.Errores;
    using ModeloDeDto.Negocio;
    using OfficeOpenXml;
    using ServicioDeDatos.Elemento;
    using ServicioDeDatos.Gastos;
    using ServicioDeDatos.Terceros;
    using ServicioDeDatos.Ventas;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Utilidades;

    // Genera los ficheros de importación de ContaSol (ver Documentación/Preasientos/ContaSol/CONTASOL_Importación Excel_Calc.pdf)
    //   APU: apuntes del diario de todos los preasientos (el nº de asiento lo asigna ContaSol)
    //   IVS / IVR: registros de iva soportado / repercutido de las facturas recibidas / emitidas, enlazados con el apunte
    //              del tercero en el APU por el código de iva (columnas M y N)
    //   PRO / CLI: proveedores y clientes aún no migrados a ContaSol
    // ContaSol importa los ficheros con su nombre fijo desde una carpeta, por eso se entregan todos en un zip
    public class LoteContableConContasol : GeneradorDePreasiento
    {
        private const int MaximoDeBases = 5;
        private const string IvaSoportado = "S";
        private const string IvaRepercutido = "R";
        private const string MonedaDeLaEmpresa = "E";

        private List<PreasientoDtm> Preasientos { get; set; }
        private int Digitos { get; set; }
        private string CarpetaDeTrabajo { get; set; }
        public string LoteZip { get; private set; }

        public LoteContableConContasol(ContextoSe contexto, List<PreasientoDtm> preasientos, SociedadDtm sociedad, int ejercicio, bool respetarFechaContable, DateTime? fechaContable)
        {
            Contexto = contexto;
            Sociedad = sociedad;
            Preasientos = preasientos;
            FechaContable = fechaContable;
            RespetarFechacontable = respetarFechaContable;
            Ejercicio = fechaContable is null ? ejercicio : ((DateTime)fechaContable).Year;

            Digitos = sociedad.ContabilizarEn().Digitos;
            if (Digitos < 4 || Digitos > 10)
                GestorDeErrores.Emitir($"Ha de indicar en el parámetro '{enumParametrosDePreasiento.SPR_Contabilizar_En}' de la sociedad '{sociedad.NIF}' los dígitos de las cuentas de ContaSol (\"Digitos\": entre 4 y 10)");

            var nombre = $"Contasol{Sociedad.PlanContable().IdPlanContable.PadLeft(4, '0')}_{Ejercicio}";
            CarpetaDeTrabajo = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{nombre}_{Guid.NewGuid():N}");
            LoteZip = ApiDeArchivos.ObtenerNombreUnico(Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{nombre}.zip"));
        }

        public void GenerarLoteContable((List<ProveedorDtm> proveedores, List<ClienteDtm> clientes) terceros)
        {
            Directory.CreateDirectory(CarpetaDeTrabajo);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var apuntes = new List<object[]>();
            var ivaSoportado = new List<object[]>();
            var ivaRepercutido = new List<object[]>();
            foreach (var preasiento in Preasientos)
            {
                try
                {
                    apuntes.AddRange(FilasDeApuntes(preasiento));
                    if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
                        ivaSoportado.Add(FilaDeIvaSoportado(preasiento));
                    if (preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida)
                        ivaRepercutido.Add(FilaDeIvaRepercutido(preasiento));
                }
                catch (Exception ex)
                {
                    throw Excepciones.Emitir($"No se ha podido generar el asiento contable del preasiento '{preasiento.Referencia}'{Environment.NewLine}{GestorDeErrores.Detalle(ex)}");
                }
            }

            Guardar("APU", ColumnasApu, apuntes);
            if (ivaSoportado.Count > 0) Guardar("IVS", ColumnasIvs, ivaSoportado);
            if (ivaRepercutido.Count > 0) Guardar("IVR", ColumnasIvr, ivaRepercutido);
            if (terceros.proveedores.Count > 0) Guardar("PRO", ColumnasPro, terceros.proveedores.Select(p => FilaDeTercero(p, p.NIF(Contexto))).ToList());
            if (terceros.clientes.Count > 0) Guardar("CLI", ColumnasCli, terceros.clientes.Select(c => FilaDeTercero(c, c.NIF(Contexto))).ToList());

            ZipFile.CreateFromDirectory(CarpetaDeTrabajo, LoteZip);
            Directory.Delete(CarpetaDeTrabajo, recursive: true);
        }

        public void Deshacer()
        {
            if (Directory.Exists(CarpetaDeTrabajo)) Directory.Delete(CarpetaDeTrabajo, recursive: true);
            if (File.Exists(LoteZip)) File.Delete(LoteZip);
        }

        // APU: una fila por apunte del preasiento, el importe en la columna del debe o del haber según su posición (con su signo,
        // las rectificativas van en negativo). El asiento va en blanco, ContaSol lo numera y detecta su inicio por el orden 1
        private List<object[]> FilasDeApuntes(PreasientoDtm preasiento)
        {
            var filas = new List<object[]>();
            var fecha = FechaDelAsiento(preasiento);
            var documento = DocumentoDelPreasiento(preasiento);
            var orden = 0;
            foreach (var apunte in preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).OrderBy(a => a.Orden))
            {
                var importe = Math.Round(apunte.Importe, 2);
                var esDelTercero = apunte.Clase == enumClaseDeApunte.Far_Proveedor || apunte.Clase == enumClaseDeApunte.Fae_Cliente;
                var fila = new object[17];
                fila[0] = Diario(preasiento);
                fila[1] = fecha;
                fila[2] = null;
                fila[3] = ++orden;
                fila[4] = Cuenta(apunte.Cuenta);
                fila[6] = (apunte.Concepto.IsNullOrEmpty() ? preasiento.Nombre : apunte.Concepto).Left(60);
                fila[7] = documento;
                fila[8] = apunte.Posicion == enumPosicionContable.Debe ? importe : 0m;
                fila[9] = apunte.Posicion == enumPosicionContable.Haber ? importe : 0m;
                fila[10] = MonedaDeLaEmpresa;
                fila[11] = 0;
                if (esDelTercero)
                {
                    fila[12] = apunte.Clase == enumClaseDeApunte.Far_Proveedor ? IvaSoportado : IvaRepercutido;
                    fila[13] = CodigoDeIva(preasiento);
                }
                filas.Add(fila);
            }

            var debe = filas.Sum(f => (decimal)f[8]);
            var haber = filas.Sum(f => (decimal)f[9]);
            if (debe != haber)
                GestorDeErrores.Emitir($"El preasiento '{preasiento.Referencia}' está descuadrado (debe: {debe}, haber: {haber})");

            return filas;
        }

        private object[] FilaDeIvaSoportado(PreasientoDtm preasiento)
        {
            var factura = (FacturaRecDtm)preasiento.Referenciado(Contexto);
            var proveedor = preasiento.Proveedor(Contexto);
            var direccion = factura.DireccionFiscal(Contexto, errorSiNoHay: false);
            var apuntes = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto);

            var bases = apuntes.Where(a => a.Clase == enumClaseDeApunte.Far_Gasto)
                .GroupBy(a => a.IdIva.Entero())
                .Select(g => (iva: Contexto.SeleccionarPorId<IvaSoportadoDtm>(g.Key), bi: g.Sum(a => a.Importe), cuota: g.Sum(a => a.IvaDelImporte ?? 0)))
                .Select(b => (porcentaje: b.iva.Exento || b.iva.Clase == enumClasesDeIvaSop.NSJ ? 0 : b.iva.Porcentaje, b.bi, b.cuota, esIsp: b.iva.Clase == enumClasesDeIvaSop.ISP))
                .ToList();
            ValidarBases(preasiento, bases.Count);

            var fila = new object[109]; // hasta la columna DE
            fila[0] = CodigoDeIva(preasiento);                                             // A  código
            fila[1] = 1;                                                                    // B  libro de iva
            fila[2] = FechaDelAsiento(preasiento);                                          // C  fecha
            fila[3] = Cuenta(apuntes.First(a => a.Clase == enumClaseDeApunte.Far_Proveedor).Cuenta); // D cuenta
            fila[4] = factura.Numero.Left(30);                                              // E  factura
            fila[5] = proveedor.Interlocutor(Contexto).RazonSocial(Contexto).Left(100);     // F  nombre
            fila[6] = proveedor.NIF(Contexto).Left(18);                                     // G  cif
            fila[7] = direccion is not null && direccion.IntraComunitaria ? 2 : direccion is not null && direccion.ExtraComunitaria ? 1 : 0; // H tipo de operación
            fila[8] = 0;                                                                    // I  deducible
            EscribirBases(fila, bases, columnasBase: new[] { 9, 10, 11, 88, 98 }, columnasPorcentaje: new[] { 12, 13, 14, 89, 99 }, columnasCuota: new[] { 18, 19, 20, 90, 100 });
            fila[24] = Math.Round(factura.TotalDelPago, 2);                                 // Y  total
            EscribirRetencion(fila, apuntes.Where(a => a.Clase == enumClaseDeApunte.Far_Irpf), columnaPorcentaje: 45, columnaImporte: 46); // AT, AU
            fila[49] = factura.FacturadaEl.Date;                                            // AX fecha de expedición
            fila[53] = bases.Any(b => b.esIsp) ? 8 : factura.EsRectificativa ? 4 : 0;       // BB clave de operación
            fila[54] = 1;                                                                   // BC identificación fiscal: NIF
            fila[55] = 0;                                                                   // BD tipo de impuesto: IVA
            fila[84] = factura.FacturadaEl.Date;                                            // CG fecha de registro contable

            if (factura.EsRectificativa)
            {
                var rectificada = factura.Rectificada(Contexto, errorSiNoHay: false);
                if (rectificada is not null)
                {
                    fila[26] = rectificada.Numero.Left(30);                                 // AA factura rectificada
                    fila[27] = rectificada.FacturadaEl.Date;                                // AB fecha factura rectificada
                    fila[77] = rectificada.FacturadaEl.Year;                                // BZ ejercicio de la factura rectificada
                }
                fila[81] = 9;                                                               // CD tipo de rectificativa: por diferencias (resto)
            }
            return fila;
        }

        private object[] FilaDeIvaRepercutido(PreasientoDtm preasiento)
        {
            var factura = (FacturaEmtDtm)preasiento.Referenciado(Contexto);
            var cliente = preasiento.Cliente(Contexto);
            var direccion = factura.DireccionFiscal(Contexto, erroSiNoHay: false);
            var apuntes = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto);

            var bases = apuntes.Where(a => a.Clase == enumClaseDeApunte.Fae_Ingreso)
                .GroupBy(a => a.IdIva.Entero())
                .Select(g => (iva: Contexto.SeleccionarPorId<IvaRepercutidoDtm>(g.Key), bi: g.Sum(a => a.Importe), cuota: g.Sum(a => a.IvaDelImporte ?? 0)))
                .Select(b => (porcentaje: b.iva.Exento || b.iva.Clase == enumClasesDeIvaRep.NSJ ? 0 : b.iva.Porcentaje, b.bi, b.cuota, esIsp: b.iva.Clase == enumClasesDeIvaRep.ISP))
                .ToList();
            ValidarBases(preasiento, bases.Count);

            var fila = new object[103]; // hasta la columna CY
            fila[0] = CodigoDeIva(preasiento);                                             // A  código
            fila[1] = 1;                                                                    // B  libro de iva
            fila[2] = FechaDelAsiento(preasiento);                                          // C  fecha
            fila[3] = Cuenta(apuntes.First(a => a.Clase == enumClaseDeApunte.Fae_Cliente).Cuenta); // D cuenta
            fila[4] = factura.NumeroDeFactura.Left(30);                                     // E  factura
            fila[5] = cliente.Interlocutor(Contexto).RazonSocial(Contexto).Left(40);        // F  nombre
            fila[6] = cliente.NIF(Contexto).Left(18);                                       // G  cif
            fila[7] = direccion is not null && direccion.IntraComunitaria ? 1
                    : direccion is not null && direccion.ExtraComunitaria ? 2
                    : bases.All(b => b.porcentaje == 0) ? 3 : 0;                            // H  tipo de operación
            EscribirBases(fila, bases, columnasBase: new[] { 8, 9, 10, 82, 92 }, columnasPorcentaje: new[] { 11, 12, 13, 83, 93 }, columnasCuota: new[] { 17, 18, 19, 84, 94 });
            fila[23] = Math.Round(apuntes.First(a => a.Clase == enumClaseDeApunte.Fae_Cliente).Importe, 2); // X total
            EscribirRetencion(fila, apuntes.Where(a => a.Clase == enumClaseDeApunte.Fae_Irpf), columnaPorcentaje: 43, columnaImporte: 44); // AR, AS
            fila[47] = factura.FacturadaEl?.Date;                                           // AV fecha de expedición
            fila[49] = bases.Any(b => b.esIsp) ? 8 : factura.EsRectificativa ? 4 : 0;       // AX clave de operación
            fila[50] = 1;                                                                   // AY identificación fiscal: NIF
            fila[51] = 0;                                                                   // AZ tipo de impuesto: IVA

            if (factura.EsRectificativa)
            {
                var rectificada = factura.RectificaA(Contexto, errorSiNoHay: false);
                if (rectificada is not null)
                {
                    fila[24] = rectificada.NumeroDeFactura.Left(30);                        // Y  factura rectificada
                    fila[25] = rectificada.FacturadaEl?.Date;                               // Z  fecha factura rectificada
                    fila[71] = rectificada.FacturadaEl?.Year;                               // BT ejercicio de la factura rectificada
                }
                fila[75] = 9;                                                               // BX tipo de rectificativa: por diferencias (resto)
            }
            return fila;
        }

        private object[] FilaDeTercero(ITerceroContable tercero, string nif)
        {
            var fila = new object[38]; // hasta la columna AL
            fila[0] = Cuenta(tercero.CodigoDeCtaContable(Contexto));  // A  código
            fila[1] = tercero.Interlocutor(Contexto).RazonSocial(Contexto).Left(100); // B nombre
            fila[2] = nif.Left(18);                                   // C  cif
            fila[36] = 1;                                             // AK identificación fiscal: NIF
            fila[37] = 0;                                             // AL tipo de impuesto: IVA
            return fila;
        }

        private static void ValidarBases(PreasientoDtm preasiento, int numeroDeBases)
        {
            if (numeroDeBases > MaximoDeBases)
                GestorDeErrores.Emitir($"El preasiento '{preasiento.Referencia}' tiene {numeroDeBases} tipos de iva y ContaSol admite como máximo {MaximoDeBases} por registro de iva");
        }

        private static void EscribirBases(object[] fila, List<(decimal porcentaje, decimal bi, decimal cuota, bool esIsp)> bases, int[] columnasBase, int[] columnasPorcentaje, int[] columnasCuota)
        {
            for (var i = 0; i < bases.Count; i++)
            {
                fila[columnasBase[i]] = Math.Round(bases[i].bi, 2);
                fila[columnasPorcentaje[i]] = bases[i].porcentaje;
                // la cuota lleva el signo de su base (en las rectificativas ambas en negativo)
                fila[columnasCuota[i]] = Math.Round(bases[i].bi < 0 ? -Math.Abs(bases[i].cuota) : Math.Abs(bases[i].cuota), 2);
            }
        }

        private static void EscribirRetencion(object[] fila, IEnumerable<ApunteDeUnPreasientoDtm> retenciones, int columnaPorcentaje, int columnaImporte)
        {
            var importe = retenciones.Sum(r => r.Importe);
            if (importe == 0)
                return;

            var baseDeLaRetencion = retenciones.Sum(r => r.BaseDelImporte ?? 0);
            fila[columnaPorcentaje] = baseDeLaRetencion == 0 ? 0 : Math.Round(importe * 100 / baseDeLaRetencion, 2);
            fila[columnaImporte] = Math.Round(importe, 2);
        }

        private DateTime FechaDelAsiento(PreasientoDtm preasiento)
        {
            var fecha = FechaContable ?? preasiento.FechaContable;
            return (RespetarFechacontable ? fecha : LoteContableConNcs.ObtenerFechaContableParaElTrimestre(fecha)).Date;
        }

        private static int Diario(PreasientoDtm preasiento)
        {
            var diario = preasiento.CodigoDiario.Entero();
            return diario > 0 ? diario : 1;
        }

        // El código del registro de iva es un índice de 5 dígitos en ContaSol
        private static int CodigoDeIva(PreasientoDtm preasiento) => preasiento.Id % 100000;

        private string DocumentoDelPreasiento(PreasientoDtm preasiento)
        {
            switch (preasiento.NegocioReferenciado)
            {
                case enumNegocio.FacturaRecibida: return ((FacturaRecDtm)preasiento.Referenciado(Contexto)).Numero.Left(30);
                case enumNegocio.FacturaEmitida: return ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).NumeroDeFactura.Left(30);
            }
            return preasiento.Referencia.Left(30);
        }

        // ContaSol exige que las cuentas tengan exactamente los dígitos de las cuentas auxiliares de la empresa: se alarga
        // o se acorta el último bloque de ceros de la cuenta (4000000001 con 8 dígitos --> 40000001, 475101 --> 47510001)
        private long Cuenta(string cuenta)
        {
            if (cuenta.IsNullOrEmpty() || !cuenta.All(char.IsDigit))
                GestorDeErrores.Emitir($"La cuenta '{cuenta}' no es una cuenta contable válida para ContaSol");

            if (cuenta.Length != Digitos)
            {
                var fin = cuenta.LastIndexOf('0');
                if (fin < 0)
                {
                    if (cuenta.Length > Digitos)
                        GestorDeErrores.Emitir($"La cuenta '{cuenta}' tiene más de {Digitos} dígitos y no se puede ajustar a las cuentas de ContaSol");
                    cuenta = cuenta.PadRight(Digitos, '0');
                }
                else if (cuenta.Length < Digitos)
                {
                    cuenta = cuenta.Insert(fin, new string('0', Digitos - cuenta.Length));
                }
                else
                {
                    var inicio = fin;
                    while (inicio > 0 && cuenta[inicio - 1] == '0') inicio--;
                    var sobran = cuenta.Length - Digitos;
                    if (sobran > fin - inicio + 1)
                        GestorDeErrores.Emitir($"La cuenta '{cuenta}' no se puede ajustar a los {Digitos} dígitos de las cuentas de ContaSol");
                    cuenta = cuenta.Remove(inicio, sobran);
                }
            }
            return long.Parse(cuenta);
        }

        private void Guardar(string fichero, string[] columnas, List<object[]> filas)
        {
            using (var libro = new ExcelPackage())
            {
                var hoja = libro.Workbook.Worksheets.Add(fichero);
                for (var c = 0; c < columnas.Length; c++)
                    hoja.Cells[1, c + 1].Value = columnas[c];

                // la fila 1 es el título, en ContaSol se indica "Fila inicial: 2"
                for (var f = 0; f < filas.Count; f++)
                {
                    for (var c = 0; c < filas[f].Length; c++)
                    {
                        var celda = hoja.Cells[f + 2, c + 1];
                        celda.Value = filas[f][c];
                        if (filas[f][c] is DateTime) celda.Style.Numberformat.Format = "dd/mm/yyyy";
                        else if (filas[f][c] is decimal) celda.Style.Numberformat.Format = "0.00";
                    }
                }
                libro.SaveAs(new FileInfo(Path.Combine(CarpetaDeTrabajo, $"{fichero}.{enumExtensiones.xlsx}")));
            }
        }

        private static readonly string[] ColumnasApu = { "Diario", "Fecha", "Asiento", "Orden", "Cuenta", "Importe pesetas", "Concepto", "Documento", "Importe DEBE en euros", "Importe HABER en euros", "Moneda", "Punteo", "Tipo de IVA", "Código de IVA", "Departamento", "Subdepartamento", "Archivo de imagen" };
        private static readonly string[] ColumnasIvs = { "Código", "Libro de IVA", "Fecha", "Cuenta", "Factura", "Nombre", "C.I.F.", "Tipo de operación", "Deducible", "Base 1", "Base 2", "Base 3", "% de IVA 1", "% de IVA 2", "% de IVA 3" };
        private static readonly string[] ColumnasIvr = { "Código", "Libro de IVA", "Fecha", "Cuenta", "Factura", "Nombre", "C.I.F.", "Tipo de operación", "Base 1", "Base 2", "Base 3", "% de IVA 1", "% de IVA 2", "% de IVA 3" };
        private static readonly string[] ColumnasPro = { "Código", "Nombre", "C.I.F." };
        private static readonly string[] ColumnasCli = { "Código", "Nombre", "C.I.F." };
    }
}
