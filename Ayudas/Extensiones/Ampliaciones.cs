namespace Utilidades
{
    public static class Ampliaciones
    {
        public static class Comunes
        {
            public static readonly string DireccionAlCrear = nameof(DireccionAlCrear);
        }
        public static class Tareas
        {
            public static readonly string planificaciones = nameof(planificaciones);
        }

        public static class Presupuestos
        {
            public static readonly string PptDeVenta = nameof(PptDeVenta);
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Pleitos
        {
            public static readonly string recobro = nameof(recobro);
        }


        public static class FacturasEmt
        {
            public static readonly string irpfEmt = nameof(irpfEmt);
            public static readonly string periodoEmt = nameof(periodoEmt);
            public static readonly string verifactu = nameof(verifactu);
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Sociedad
        {
            public static readonly string Parametros = nameof(Parametros);
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Persona
        {
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Interlocutor
        {
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }
        public static class Cliente
        {
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Proveedor
        {
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }
        public static class Contratos
        {
            public static readonly string Importes = nameof(Importes);
            public static readonly string CtrVenta = nameof(CtrVenta);
            public static readonly string CtrCompra = nameof(CtrCompra);
            public static readonly string avalsolicitado = nameof(avalsolicitado);
            public static readonly string avance = nameof(avance);
            public static readonly string prorroga = nameof(prorroga);
            public static readonly string MatriculaDeGuarderia = nameof(MatriculaDeGuarderia);            
        }

        public static class Pedidos
        {
            public static readonly string DireccionAlCrear = Comunes.DireccionAlCrear;
        }

        public static class Expedientes
        {
            public static readonly string DatosJuridicos = nameof(DatosJuridicos);
        }

        public static class CircuitosDoc
        {
            public static readonly string DatosDeActividadFormativa = nameof(DatosDeActividadFormativa);
        }

    }
}
