using System;
using Acr.UserDialogs;
using CommonServiceLocator;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Services.Contracts;
using MobiliVendas.Core.ViewModels;
using Pegada.Core.ViewModels;
using Pegada.Core.Views.Carrinho;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
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

            // "Visualizar Desconto" do item selecionado: mesma tela "Descontos" dos itens do Carrinho.
            BtnVisualizarDesconto.ClickedCommand = new Command(VisualizarDescontoItem);

            // Datas do "+ Filtros": o parâmetro leva o próprio botão, pro calendário abrir em popover ancorado nele
            // (IMDatePickerViewController do PegadaIOS) em vez do DatePromptAsync de tela cheia.
            BtnDataEmissaoInicio.ClickedCommandParameter = new SelecaoDataParametro("INICIO", BtnDataEmissaoInicio);
            BtnDataEmissaoFim.ClickedCommandParameter = new SelecaoDataParametro("FIM", BtnDataEmissaoFim);
            BtnDataEntregaInicio.ClickedCommandParameter = new SelecaoDataParametro("INICIO", BtnDataEntregaInicio);
            BtnDataEntregaFim.ClickedCommandParameter = new SelecaoDataParametro("FIM", BtnDataEntregaFim);
            BtnDataCadastroInicio.ClickedCommandParameter = new SelecaoDataParametro("INICIO", BtnDataCadastroInicio);
            BtnDataCadastroFim.ClickedCommandParameter = new SelecaoDataParametro("FIM", BtnDataCadastroFim);
        }

        private async void VisualizarDescontoItem()
        {
            try
            {
                var vm = BindingContext as PedidoPageViewModel;
                var pedido = vm?.PedidoSelecionado;
                var item = vm?.ItemSelecionado;
                if (pedido == null || item == null || string.IsNullOrEmpty(item.CodItemPedido))
                    return;

                var desconto = ServiceLocator.Current.GetInstance<DescontoItemViewModel>();
                await desconto.InitPedido(pedido, item);
                await PopupNavigation.Instance.PushAsync(new PopupPage { Content = new DescontoItemView { BindingContext = desconto } });
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
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

        // touchConsultar do FiltroPedidoViewController (PegadaIOS): o Consultar do painel fecha o "+ Filtros"
        // (closeSearchPedido) e a consulta segue pelo ConsultarCommand do próprio botão.
        private void OnConsultarFiltrosClicked(object sender, EventArgs e)
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
