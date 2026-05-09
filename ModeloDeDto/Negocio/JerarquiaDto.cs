using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Negocio
{
    public class NodoDto: IUsaNombreDto
    {

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Negocio { get; set; }
        public int? IdPadre { get; set; }
        public bool Activo { get; set; }
        public string TipoDto { get; set; }
        public string TipoDtm { get; set; }
        public enumModoDeAccesoDeDatos ModoAcceso { get; set; }

        public NodoDto(NodoDtm nodoDtm, string negocio, string tipoDto, enumModoDeAccesoDeDatos modoAcceso)
        {
            Id = nodoDtm.Id;
            Nombre = nodoDtm.Nombre;
            IdPadre = nodoDtm.IdPadre;
            Activo = nodoDtm.Activo;
            TipoDtm = nodoDtm.TipoDtm;
            Negocio = negocio;
            TipoDto = tipoDto;
            ModoAcceso = modoAcceso;
        }
    }


    public class JerarquiaDto
    {
        public List<NodoDeJerarquiaDto> Ramas { get; set; } = new List<NodoDeJerarquiaDto>();
    }

    public class NodoDeJerarquiaDto
    {
        public NodoDto Dto { get; }
        public List<NodoDeJerarquiaDto> Hijos { get; } 

        public NodoDeJerarquiaDto(NodoDto nodo)
        {
            Dto = nodo;
            Hijos = new List<NodoDeJerarquiaDto>();
        }

    }

}
