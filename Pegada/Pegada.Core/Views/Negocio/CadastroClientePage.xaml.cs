using MobiliVendas.Core.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SegmentedControl = MobiliVendas.Core.CustomView.Controls.SegmentedControl.SegmentedControl;

namespace Pegada.Core.Views.Negocio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadastroClientePage : ContentPage, ICadastroClientePage
    {
        public CadastroClientePage()
        {
            InitializeComponent();
        }

        public View GetContent()
        {
            return this.Content;
        }

        public void SetBindingContext(object viewModel)
        {
            this.BindingContext = viewModel;
        }

        private void SegmentedControl_OnSegmentSelectedEndereco(object sender, SegmentedControl.SegmentSelectEventArgs e)
        {
            GridEnderecoPrincipal.IsVisible = e.NewValue == 0;
            GridEnderecoCobranca.IsVisible = e.NewValue == 1;
            GridEnderecoEntrega.IsVisible = e.NewValue == 2;
            GridDadosBancarios.IsVisible = e.NewValue == 3;
        }

        private void SegmentedControl_OnSegmentSelectedContato(object sender, SegmentedControl.SegmentSelectEventArgs e)
        {
            GridHeaderContato.IsVisible = e.NewValue == 0 || e.NewValue == 1;
            GridHeaderReferencia.IsVisible = e.NewValue == 2;

            ListViewContatos.IsVisible = e.NewValue == 0;
            ListViewContatosGrupo.IsVisible = e.NewValue == 1;
            ListViewReferencias.IsVisible = e.NewValue == 2;

            ButtonNovoContatoReferencia.Text = e.NewValue == 2 ? "NOVA REFERÊNCIA" : "NOVO CONTATO";
        }
    }
}