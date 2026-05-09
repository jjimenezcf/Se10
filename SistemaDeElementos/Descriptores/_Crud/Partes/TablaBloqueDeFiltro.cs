using System.Collections.Generic;

namespace MVCSistemaDeElementos.Descriptores
{

    public class TablaBloqueDeFiltro : TablaFiltro
    {
        public TablaBloqueDeFiltro(ControlFiltroHtml padre, Dimension dimension, ICollection<ControlFiltroHtml> controles)
        : base(
          padre: padre,
          dimension: dimension,
          controles: controles
        )
        {

        }

    }

}
