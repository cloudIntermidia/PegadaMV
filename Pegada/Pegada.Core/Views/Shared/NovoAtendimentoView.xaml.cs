using MobiliVendas.Core.Contracts;
using Xamarin.Forms;

namespace Pegada.Core.Views.Shared
{
    public partial class NovoAtendimentoView : ContentView, INovoAtendimentoView
    {
        public NovoAtendimentoView()
        {
            InitializeComponent();
        }

        public View GetContent()
        {
            return this.Content;
        }

        public void SetBindingContext(INovoAtendimentoViewModel viewModel)
        {
            BindingContext = viewModel;
        }
    }
}
