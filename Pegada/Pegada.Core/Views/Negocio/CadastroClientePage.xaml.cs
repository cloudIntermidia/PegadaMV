using MobiliVendas.Core.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
    }
}