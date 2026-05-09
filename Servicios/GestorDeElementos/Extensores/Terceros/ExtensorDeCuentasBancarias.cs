using Gestor.Errores;
using IbanNet;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeCuentasBancarias
    {
        public static CuentaBancariaDtm Validar(this CuentaBancariaDtm cuenta)
        {
            if (cuenta.IsoPais.Length != 2)
            {
                GestorDeErrores.Emitir($"El iso del país no es válido: '{cuenta.IsoPais}, ha de ser longitud 2'");
            }
            if (cuenta.DcIban.Length != 2)
            {
                GestorDeErrores.Emitir($"El Dc del Iban no es válido: '{cuenta.DcIban}, ha de ser longitud 2'");
            }
            if (cuenta.Entidad.Length != 4)
            {
                GestorDeErrores.Emitir($"La entidad no es válida: '{cuenta.DcIban}, ha de ser longitud 4'");
            }
            if (cuenta.Oficina.Length != 4)
            {
                GestorDeErrores.Emitir($"La oficina no es válida: '{cuenta.DcIban}, ha de ser longitud 4");
            }
            if (cuenta.DcCcc.Length != 2)
            {
                GestorDeErrores.Emitir($"El Dc de la cuenta no es válido: '{cuenta.DcIban}, ha de ser longitud 2");
            }
            if (cuenta.Numero.Length != 10)
            {
                GestorDeErrores.Emitir($"El Número de cuenta no es válido: '{cuenta.DcIban}, ha de ser longitud 10");
            }

            IIbanValidator validator = new IbanValidator();
            ValidationResult validationResult = validator.Validate(cuenta.NumeroIban.Replace("-", ""));
            if (!validationResult.IsValid)
            {
                GestorDeErrores.Emitir($"El Número de cuenta no es válido: {validationResult.Error.ErrorMessage}");
            }

            return cuenta;
        }


        public static string IbanDeCuentaDeCargo(this TarjetaDeMiSociedadDtm tarjeta, ContextoSe contexto) => tarjeta.CuentaDeCargo(contexto).Cuenta(contexto).NumeroIban;
       

        public static CuentaDeMiSociedadDtm CuentaDeCargo(this TarjetaDeMiSociedadDtm tarjeta, ContextoSe contexto)
        {
            if (tarjeta.CuentaDeCargo is not null && tarjeta.CuentaDeCargo.Id == tarjeta.IdCuentaDeCargo)
                return tarjeta.CuentaDeCargo;

            return tarjeta.CuentaDeCargo = contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(tarjeta.IdCuentaDeCargo, aplicarJoin: true);
        }

        public static CuentaBancariaDtm Cuenta(this IUsaCuentaBancaria cc, ContextoSe contexto)
        {
            if (cc.Cuenta is not null && cc.Cuenta.Id == cc.IdCuenta)
                return cc.Cuenta;
            return cc.Cuenta = contexto.SeleccionarPorId<CuentaBancariaDtm>(cc.IdCuenta);
        }

        public static void MapearCuentaBancaria(this IUsaCuentaBancariaDto elemento, CuentaBancariaDtm cuenta)
        {
            elemento.Iban = cuenta.IsoPais + cuenta.DcIban;
            elemento.Entidad = cuenta.Entidad;
            elemento.Oficina = cuenta.Oficina;
            elemento.DcCcc = cuenta.DcCcc;
            elemento.Numero = cuenta.Numero;
        }

        public static BancoDtm Banco(this CuentaBancariaDtm cuenta, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (cuenta.Banco is not null)
                return cuenta.Banco;

            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_Bancos);
            var indice = $"{cuenta.IsoPais}-{cuenta.Entidad}";
            if (cache.ContainsKey(indice))
            {
                if (errorSiNoHay && (BancoDtm)cache[indice] == null)
                    GestorDeErrores.Emitir($"Debe de definir en la BD el Banco asociado al nº de cuenta '{cuenta.NumeroIban}'");
                return (BancoDtm)cache[indice];
            }

            var filtro = new Dictionary<string, object>
                 {
                    { nameof(BancoDtm.Codigo), cuenta.Entidad },
                    { nameof(BancoDtm.Pais.ISO2), cuenta.IsoPais }
                 };
            var bancos = contexto.SeleccionarTodos<BancoDtm>(filtro);
            if (bancos.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"Debe de definir en la BD el Banco asociado al nº de cuenta '{cuenta.NumeroIban}'");

            if (bancos.Count() > 1)
                GestorDeErrores.Emitir($"Hay más de un banco asociado a '{cuenta.IsoPais}-{cuenta.Entidad}'");


            cache[indice] = bancos.Count() == 0 ? null : bancos[0];
            cuenta.Banco = (BancoDtm)cache[indice];
            return cuenta.Banco;
        }

        public static string Expresion(this BancoDtm banco, ContextoSe contexto)
        {
            if (banco == null) return ltrBanco.BancoNoDefinido;

            var pais = contexto.SeleccionarPorId<PaisDtm>(banco.IdPais);
            return $"({pais.ISO2}:{banco.Codigo}) {banco.Nombre}";
        }


        public static CuentaBancariaDtm CrearSiNoExiste(this CuentaBancariaDtm cuenta, ContextoSe contexto)
        =>
        Leer(contexto, cuenta.IsoPais, cuenta.DcIban, cuenta.Entidad, cuenta.Oficina, cuenta.DcCcc, cuenta.Numero, crearSiNoExiste: true);

        public static bool Existe(ContextoSe contexto,
            string isoPais,
            string dcIban,
            string entidad,
            string oficina,
            string dc,
            string numero)
        =>
        Leer(contexto, isoPais, dcIban, entidad, oficina, dc, numero, crearSiNoExiste: false) == null;


        public static CuentaBancariaDtm Leer(ContextoSe contexto, 
            string isoPais, 
            string dcIban, 
            string entidad, 
            string oficina, 
            string dc, 
            string numero, 
            bool crearSiNoExiste = true)
        {
            var filtro = new Dictionary<string, object>
            {
                { nameof(CuentaBancariaDtm.IsoPais), isoPais},
                { nameof(CuentaBancariaDtm.DcIban), dcIban },
                { nameof(CuentaBancariaDtm.Entidad), entidad},
                { nameof(CuentaBancariaDtm.Oficina),oficina },
                { nameof(CuentaBancariaDtm.DcCcc), dc },
                { nameof(CuentaBancariaDtm.Numero),numero}
            };

            var cuenta = contexto.SeleccionarPorAk<CuentaBancariaDtm>(filtro, errorSiNoHay: false);
            if (cuenta == null && crearSiNoExiste) 
                return Crear(contexto, isoPais, dcIban, entidad, oficina, dc, numero);            
            return cuenta;
        }

        public static CuentaBancariaDtm Crear(ContextoSe contexto, string isoPais, string dcIban, string entidad, string oficina, string dc, string numero)
        {
            return new CuentaBancariaDtm
            {
                IsoPais = isoPais,
                DcIban = dcIban,
                Entidad = entidad,
                Oficina = oficina,
                DcCcc = dc,
                Numero = numero,
            }.Validar().Insertar(contexto);
        }

        public static void AsignarNombreAlCertificado(this IUsaCuentaBancaria cb, ContextoSe contexto, enumNegocio negocio, int idTercero, IUsaCuentaBancaria anterior)
        {
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idTercero, (int)cb.IdArchivo);

            var archivo = contexto.SeleccionarPorId<ArchivoDtm>((int)cb.IdArchivo);
            if (anterior is not null && anterior.IdArchivo is not null)
            {
                var antiguo = contexto.SeleccionarPorId<ArchivoDtm>((int)anterior.IdArchivo);
                var extension = Path.GetExtension(archivo.Nombre);
                var nombre = antiguo.Nombre.Replace(extension, "");
                var pos = nombre.IndexOf("_");
                var numeroDeCopia = (pos > 0) ? nombre.Substring(pos + 1).Entero() : 0;
                archivo.Nombre = $"Crtfdo. {cb.Cuenta(contexto).Numero}_{numeroDeCopia + 1}{extension}";
                archivo.Modificar(contexto);
            }
            else
            {
                var extension = Path.GetExtension(archivo.Nombre);
                archivo.Nombre = $"Crtfdo. {cb.Cuenta(contexto).Numero}{extension}";
                archivo.Modificar(contexto);
            }
        }
    }
}
