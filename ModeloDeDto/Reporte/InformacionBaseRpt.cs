using Gestor.Errores;
using Microsoft.IdentityModel.Tokens;
using ModeloDeDto.Negocio;
using Newtonsoft.Json;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utilidades;

namespace ModeloDeDto.Reporte
{
    public class InformacionBaseRpt<T> : IInformacionRpt<T>
    {
        public T Datos { get; set; }
        public Dictionary<string, object> Parametros { get; set; }
        public bool IndicarFila => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.IndicarFila, false);
        public bool MostrarCalificadorDireccion => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.MostrarCalificadorDireccion, false);
        public bool MostrarImpresoEl => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.MostrarImpresoEl, false);
        public bool ImprimirFechaCreacion => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.ImprimirFechaCreacion, false);
        public string ColorTitulo => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.ColorTitulo, "#2196f3");
        public float Marco => extDiccionarios.LeerValor(Parametros, ltrParametrosRpt.Marco, 50F);
        public float AnchoLogo => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.AnchoLogo, 100);
        public float AltoLogo => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.AltoLogo, 50);
        public float PaddingMargenIzquierdo => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.PaddingMargenIzquierdo, 20);
        public float PaddingTopPie => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.PaddingTopPie, 25);
        public float TamanoTitulo => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.TamanoTitulo, 16);
        public float TamanoPieDePagina => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.TamanoPieDePagina, 5);
        public float TamanoEncabezado => extDiccionarios.LeerValor<float>(Parametros, ltrParametrosRpt.TamanoEncabezado, 8);

        public virtual bool VerificarVersionDeParametros()
        {
            return Parametros.ContieneClave(ltrParametrosRpt.IndicarFila) &&
                   Parametros.ContieneClave(ltrParametrosRpt.MostrarCalificadorDireccion) &&
                   Parametros.ContieneClave(ltrParametrosRpt.MostrarImpresoEl) &&
                   Parametros.ContieneClave(ltrParametrosRpt.ImprimirFechaCreacion) &&
                   Parametros.ContieneClave(ltrParametrosRpt.ColorTitulo) &&
                   Parametros.ContieneClave(ltrParametrosRpt.Marco) &&
                   Parametros.ContieneClave(ltrParametrosRpt.AnchoLogo) &&
                   Parametros.ContieneClave(ltrParametrosRpt.PaddingMargenIzquierdo) &&
                   Parametros.ContieneClave(ltrParametrosRpt.PaddingTopPie) &&
                   Parametros.ContieneClave(ltrParametrosRpt.TamanoTitulo) &&
                   Parametros.ContieneClave(ltrParametrosRpt.TamanoPieDePagina) &&
                   Parametros.ContieneClave(ltrParametrosRpt.TamanoEncabezado);
        }

        public virtual void ActualizarVersionDeParametros(int idNegocio, System.Enum parametro)
        {
            if (Parametros is null) Parametros = new Dictionary<string, object> ();
            if (!Parametros.ContieneClave(ltrParametrosRpt.IndicarFila)) Parametros[ltrParametrosRpt.IndicarFila] = IndicarFila;
            if (!Parametros.ContieneClave(ltrParametrosRpt.MostrarCalificadorDireccion)) Parametros[ltrParametrosRpt.MostrarCalificadorDireccion] = MostrarCalificadorDireccion;
            if (!Parametros.ContieneClave(ltrParametrosRpt.MostrarImpresoEl)) Parametros[ltrParametrosRpt.MostrarImpresoEl] = MostrarImpresoEl;
            if (!Parametros.ContieneClave(ltrParametrosRpt.ImprimirFechaCreacion)) Parametros[ltrParametrosRpt.ImprimirFechaCreacion] = ImprimirFechaCreacion;
            if (!Parametros.ContieneClave(ltrParametrosRpt.ColorTitulo)) Parametros[ltrParametrosRpt.ColorTitulo] = ColorTitulo;
            if (!Parametros.ContieneClave(ltrParametrosRpt.Marco)) Parametros[ltrParametrosRpt.Marco] = Marco;
            if (!Parametros.ContieneClave(ltrParametrosRpt.AnchoLogo)) Parametros[ltrParametrosRpt.AnchoLogo] = AnchoLogo;
            if (!Parametros.ContieneClave(ltrParametrosRpt.PaddingMargenIzquierdo)) Parametros[ltrParametrosRpt.PaddingMargenIzquierdo] = PaddingMargenIzquierdo;
            if (!Parametros.ContieneClave(ltrParametrosRpt.PaddingTopPie)) Parametros[ltrParametrosRpt.PaddingTopPie] = PaddingTopPie;
            if (!Parametros.ContieneClave(ltrParametrosRpt.TamanoTitulo)) Parametros[ltrParametrosRpt.TamanoTitulo] = TamanoTitulo;
            if (!Parametros.ContieneClave(ltrParametrosRpt.TamanoPieDePagina)) Parametros[ltrParametrosRpt.TamanoPieDePagina] = TamanoPieDePagina;
            if (!Parametros.ContieneClave(ltrParametrosRpt.TamanoEncabezado)) Parametros[ltrParametrosRpt.TamanoEncabezado] = TamanoEncabezado;
            string json = JsonConvert.SerializeObject(Parametros, Formatting.Indented);
            ParametroDeNegocioSql.Actualizar(idNegocio, parametro, valor: json);
        }
    }

    public static class ExtensorDeRpt
    {
        public static string Imprimir(this DireccionDto direccion, bool mostrarCalificador, bool errorSiNoHay = false, DireccionDto deDefecto = null)
        {
            return Imprimirla(direccion?.Expresion ?? "", mostrarCalificador, errorSiNoHay, deDefecto?.Expresion ?? null);
        }

        public static string Imprimirla(string direccion, bool mostrarCalificador, bool errorSiNoHay = false, string deDefecto = null)
        {
            if (direccion.IsNullOrEmpty())
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"No ha indicado la dirección.");

                return deDefecto == null ? ltrDireccion.NoIndicada : deDefecto;
            }
            Regex _regex = new Regex(@"^\s*\([^)]+\)\s*");

            return mostrarCalificador ? direccion : _regex.Replace(direccion, "");
        }

    }
}
