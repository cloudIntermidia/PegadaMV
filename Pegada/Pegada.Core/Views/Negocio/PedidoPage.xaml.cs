using System;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PedidoPage : ContentPage, IPedidoPage
    {
        public PedidoPage()
        {
            InitializeComponent();

            // "+ Filtros": abre o painel lateral (o botão fica desabilitado enquanto o painel está aberto,
            // igual ao PegadaIOS, e volta ao fechar com o gesto de deslizar para cima).
            BtnFiltros.ClickedCommand = new Command(AbrirPainelFiltros);
        }

        private void AbrirPainelFiltros()
        {
            PainelFiltros.IsVisible = true;
            BtnFiltros.IsEnabled = false;
        }

        private void FecharPainelFiltros()
        {
            PainelFiltros.IsVisible = false;
            BtnFiltros.IsEnabled = true;
        }

        private void OnPainelFiltrosSwipedUp(object sender, SwipedEventArgs e)
        {
            FecharPainelFiltros();
        }

        public View GetContent()
        {
            return this.Content;
        }

        public void SetBindingContext(object viewModel)
        {
            BindingContext = viewModel;
        }
    }
}
