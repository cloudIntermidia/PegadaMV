using MobiliVendas.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    // Aba "Títulos" do Pegada com o layout do TituloViewController do PegadaIOS (a TitulosPage compartilhada segue
    // nos outros apps). Criada pela NegociosPage via Session.CriarTitulosPage; só o Content vai para a aba, por isso
    // o BindingContext é fixado também no Content (senão ele herdaria o da NegociosPage).
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TitulosPage : ContentPage
    {
        public TitulosPage()
        {
            InitializeComponent();

            // ViewModel criado pelo AutowireViewModel do Prism (mapeado no App.xaml.cs): resolver direto pelo
            // container falha no INavigationService, que precisa da página.
            var viewModel = (TitulosPageViewModel)BindingContext;

            // PegadaIOS: não há checkbox de período; as datas começam vazias ("Início"/"Fim") e só filtram quando escolhidas.
            viewModel.FiltrarDataEmissao = true;
            viewModel.FiltrarDataVencimento = true;
            viewModel.Filtro.DataEmissaoInicial = null;
            viewModel.Filtro.DataEmissaoFinal = null;
            viewModel.Filtro.DataVencimentoInicial = null;
            viewModel.Filtro.DataVencimentoFinal = null;

            Content.BindingContext = viewModel;
        }
    }
}
