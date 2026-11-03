using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Results;
using Pegada.Core.ViewModels;
using Syncfusion.ListView.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SegmentedControl = MobiliVendas.Core.CustomView.Controls.SegmentedControl.SegmentedControl;

namespace Pegada.Core.Views.Negocio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadastroClientePage : ContentPage, ICadastroClientePage
    {
        // Posição da tabela de contatos (Margin 35, início em 492) e altura das linhas, em relação ao bloco de 1024.
        private const double TabelaX = 35;
        private const double TabelaY = 492;
        private const double TabelaLargura = 950;
        private const double AlturaLinha = 20;
        private const double AlturaMinimaTabela = 85;
        private const double FolgaAbaixoTabela = 9;
        private const double LarguraBloco = 1024;

        private ContatoCommandResult _contatoMenu;

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

        // Telas desenhadas para o iPad de 9,7": no iPad maior o bloco inteiro cresce na mesma proporção, sem cortar a
        // tabela de contatos, que fica com o que sobrar da altura. Nunca diminui.
        private void RaizCadastro_SizeChanged(object sender, System.EventArgs e)
        {
            if (RaizCadastro.Width <= 0 || RaizCadastro.Height <= 0)
                return;

            var fator = System.Math.Max(1, System.Math.Min(RaizCadastro.Width / LarguraBloco, RaizCadastro.Height / (TabelaY + AlturaMinimaTabela)));
            var altura = RaizCadastro.Height / fator;

            BlocoCadastro.Scale = fator;
            BlocoCadastro.TranslationX = (RaizCadastro.Width - LarguraBloco * fator) / 2;
            BlocoCadastro.HeightRequest = altura;
            TabelaContatos.HeightRequest = System.Math.Max(AlturaMinimaTabela, altura - TabelaY - FolgaAbaixoTabela);
        }

        // -longPress: (CadastroViewController, PegadaIOS): contato inativo não abre o menu. Windows ainda não tem essa opção.
        private void ListViewContato_ItemHolding(object sender, ItemHoldingEventArgs e)
        {
            if (Device.RuntimePlatform == Device.UWP || !(e.ItemData is ContatoCommandResult contato) || contato.IndExcluido == 1)
                return;

            _contatoMenu = contato;

            var indice = ((System.Collections.IList)((SfListView)sender).ItemsSource).IndexOf(contato);
            var linhaY = TabelaY + System.Math.Min(indice * AlturaLinha, TabelaContatos.Height - AlturaLinha);
            var centroX = TabelaX + TabelaLargura / 2;
            AbsoluteLayout.SetLayoutBounds(BalaoMenuContato, new Rectangle(centroX - 80, linhaY - 48, 160, 40));
            AbsoluteLayout.SetLayoutBounds(SetaMenuContato, new Rectangle(centroX - 7, linhaY - 15, 14, 14));
            MenuContato.IsVisible = true;
        }

        private void FecharMenuContato_Tapped(object sender, System.EventArgs e)
        {
            MenuContato.IsVisible = false;
        }

        private void EditarContato_Tapped(object sender, System.EventArgs e)
        {
            MenuContato.IsVisible = false;
            (BindingContext as CadastroClientePageViewModel)?.EditarContato(_contatoMenu);
        }

        private void InativarContato_Tapped(object sender, System.EventArgs e)
        {
            MenuContato.IsVisible = false;
            (BindingContext as CadastroClientePageViewModel)?.InativarContato(_contatoMenu);
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

            ButtonNovoContatoReferencia.Text = e.NewValue == 2
                ? new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteBtnNovaReferencia")
                : new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteBtnNovoContato");
        }
    }
}