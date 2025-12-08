using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobiliVendas.Core.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Carrinho
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CarrinhoPage : ContentPage
    {
        private ICarrinhoPageViewModel viewModel => BindingContext as ICarrinhoPageViewModel;
        public CarrinhoPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            await viewModel.Load().ConfigureAwait(false);
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}