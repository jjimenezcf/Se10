using System;

namespace ValidacionesBase
{
    // Marca un método público de instancia, sin parámetros, como prueba: MenuDeTests lo descubre por este atributo y lo ejecuta
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PruebaAttribute : Attribute
    {
    }

    // Lo que lanza una validación que no se cumple: el menú la muestra como el error de la prueba (sin la pila, sólo el mensaje)
    public class FalloDeValidacion : Exception
    {
        public FalloDeValidacion(string mensaje) : base(mensaje)
        {
        }
    }

    // Validaciones de las pruebas: una prueba es correcta si termina sin excepciones, no hace falta indicarlo
    public static class Validar
    {
        public static void Fallo(string mensaje) => throw new FalloDeValidacion(mensaje);

        public static void Que(bool condicion, string mensaje)
        {
            if (!condicion)
                Fallo(mensaje);
        }
    }
}
