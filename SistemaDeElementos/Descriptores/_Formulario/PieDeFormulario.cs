namespace MVCSistemaDeElementos.Descriptores
{
    public class PieDeFormulario : ControlHtml
    {
        public DescriptorDeFormulario Formulario => (DescriptorDeFormulario)Padre;

        public MenuDePie Menu { get; private set; }

        public bool RenderizarMenu { get; set; } = false;


        public PieDeFormulario(DescriptorDeFormulario formulario)
            : base(formulario, $"pie-{formulario.Id}", "", "", "", null)
        {
            Menu = new MenuDePie(this);
        }

        public string RenderPie()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            return !RenderizarMenu ? "" : $@"{Menu.RenderMenu()}";
        }
    }
}