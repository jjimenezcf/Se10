
namespace ModeloXml.eFactura.Facturae32
{


        /// <summary>
        /// Proporciona un método estático para convertir una cadena de texto
        /// que representa una unidad de medida (ej. "h", "kg", "ud")
        /// al tipo enumerado UnitOfMeasureType requerido por el esquema de Factura-E.
        /// </summary>
        public static class enumUnidadDeMedidaToXml
        {
            /// <summary>
            /// Convierte una sigla o nombre de unidad de medida al valor enum de Factura-E.
            /// Si no se encuentra una correspondencia, devuelve el valor por defecto '01' (Unidades).
            /// La comparación no distingue entre mayúsculas y minúsculas.
            /// </summary>
            /// <param name="sigla">La sigla o nombre de la unidad (ej. "h", "hora", "kg", "uds").</param>
            /// <returns>El valor de UnitOfMeasureType correspondiente.</returns>
            public static UnitOfMeasureType ObtenerDesdeSigla(string sigla)
            {
                // Si la entrada es nula o vacía, devolvemos el valor por defecto directamente.
                if (string.IsNullOrWhiteSpace(sigla))
                {
                    return UnitOfMeasureType.Item01; // Unidades
                }

                // Normalizamos la entrada a minúsculas y sin espacios para hacer la comparación más fiable.
                string siglaNormalizada = sigla.Trim().ToLowerInvariant();

                switch (siglaNormalizada)
                {
                    // Unidades (el valor por defecto, pero lo incluimos por si se especifica explícitamente)
                    case "ud":
                    case "uds":
                    case "unit":
                    case "unidades":
                    case "unidad":
                        return UnitOfMeasureType.Item01;

                    // Metros
                    case "m":
                    case "mts":
                    case "metro":
                    case "metros":
                        return UnitOfMeasureType.Item02;

                    // Kilos
                    case "kg":
                    case "kgs":
                    case "kilo":
                    case "kilos":
                        return UnitOfMeasureType.Item03;

                    // Litros
                    case "l":
                    case "lts":
                    case "litro":
                    case "litros":
                        return UnitOfMeasureType.Item04;

                    // Día
                    case "d":
                    case "dia":
                    case "dias":
                    case "day":
                    case "days":
                        return UnitOfMeasureType.Item05;

                    // Horas
                    case "h":
                    case "hr":
                    case "hrs":
                    case "hora":
                    case "horas":
                        return UnitOfMeasureType.Item06;

                    // Cajas / Embalajes
                    case "caja":
                    case "cajas":
                    case "box":
                        return UnitOfMeasureType.Item07;

                    // Toneladas
                    case "t":
                    case "ton":
                    case "tonelada":
                    case "toneladas":
                        return UnitOfMeasureType.Item08;

                    // Metros cúbicos
                    case "m3":
                    case "metro cubico":
                    case "metros cubicos":
                        return UnitOfMeasureType.Item09;

                    // Metros cuadrados
                    case "m2":
                    case "metro cuadrado":
                    case "metros cuadrados":
                        return UnitOfMeasureType.Item10;

                    // Mes
                    case "mes":
                    case "meses":
                        return UnitOfMeasureType.Item13;

                    // Lote
                    case "lote":
                        return UnitOfMeasureType.Item14;

                    // Si ninguna de las siglas anteriores coincide, se ejecuta el caso por defecto.
                    default:
                        return UnitOfMeasureType.Item01; // Devuelve 'Unidades' como fallback.
                }
            }
        }
    }

