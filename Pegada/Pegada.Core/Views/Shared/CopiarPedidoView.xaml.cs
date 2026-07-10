using MobiliVendas.Core.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CopiarPedidoView : ContentView, ICopiaPedidoView
    {
        private ICopiaPedidoViewModel Viewmodel;

        public CopiarPedidoView()
        {
            InitializeComponent();
        }

        public View GetContent()
        {
            return this.Content;
        }

        public void SetBindingContext(ICopiaPedidoViewModel viewModel)
        {
            BindingContext = viewModel;
            Viewmodel = viewModel;
        }
    }
}
