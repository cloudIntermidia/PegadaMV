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
            BtnImprimir.IsEnabled = false;
            MessagingCenter.Subscribe<object, bool>(this, "BtnImprimirPedido", EnableBtnImprimir, null);

            MessagingCenter.Subscribe<object, bool>(this, "BtnDistribuirPedido", EnableBtnDistribuir, null);

            MessagingCenter.Subscribe<object, bool>(this, "BtnCancelarPedido", EnableBtnCancelar, null);
            //EnableBtnForcarImplantação();
        }

        private void EnableBtnImprimir(object arg1, bool flag)
        {
            BtnImprimir.IsEnabled = flag;
        }
        
        private void EnableBtnDistribuir(object arg1, bool flag)
        {
            BtnDistribuir.IsEnabled = flag;
            BtnDistribuir.IsVisible = flag;
        }

        private void EnableBtnCancelar(object arg1, bool flag)
        {
            BtnDistribuir.IsVisible = !flag;
            BtnCancelarAlocado.IsVisible = flag;
        }
        //private void EnableBtnForcarImplantação()
        //{
        //    BtnForcarImplantação.IsVisible = Session.USUARIO_LOGADO.PermiteForcarImplantacao;
        //    //BtnForcarImplantação.IsVisible = true;
        //}

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
