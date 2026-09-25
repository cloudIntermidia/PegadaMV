using Pegada.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Carrinho
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContatoClienteView : ContentView
    {
        public ContatoClienteView()
        {
            InitializeComponent();
        }

        // didSelectRowAtIndexPath do ContatoViewController: guarda o contato selecionado para "Editar Contato".
        private void OnContatoTapped(object sender, ItemTappedEventArgs e)
        {
            if (BindingContext is ContatoClienteViewModel vm)
                vm.ContatoSelecionado = e.Item as MobiliVendas.Core.Domain.Commands.Results.ContatoCommandResult;
        }
    }
}
