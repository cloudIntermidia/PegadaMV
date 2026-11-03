using CommonServiceLocator;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.ViewModels;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadastroContatoPopupView : ContentView
    {
        public CadastroContatoPopupView()
        {
            InitializeComponent();
        }

        // Novo contato (contato == null) ou edição, em popup do tamanho do formSheet do PegadaIOS. Voltar/Salvar fecham o popup
        // (FormCadastroContatoViewModel em modo popup) e o Salvar avisa "ContatoSalvo" para a tela recarregar a lista.
        public static async Task AbrirAsync(string codPessoaCliente, ContatoCommandResult contato = null)
        {
            var viewModel = ServiceLocator.Current.GetInstance<FormCadastroContatoViewModel>();
            viewModel.SetContexto(codPessoaCliente, contato, emPopup: true);

            await PopupNavigation.Instance.PushAsync(new PopupPage
            {
                Content = new CadastroContatoPopupView { BindingContext = viewModel }
            });
        }

        // Novo Cliente / Editar Cadastro: o contato vai para a lista temporária do formulário (gravada junto com o cliente).
        // Salvar adiciona e limpa a tela para o próximo contato; Voltar fecha.
        public static async Task AbrirTemporarioAsync(ObservableCollection<ContatoCommandResult> contatosTemporarios)
        {
            var viewModel = ServiceLocator.Current.GetInstance<FormCadastroContatoViewModel>();
            viewModel.SetContextoTemporario(contatosTemporarios, emPopup: true);

            await PopupNavigation.Instance.PushAsync(new PopupPage
            {
                Content = new CadastroContatoPopupView { BindingContext = viewModel }
            });
        }
    }
}
