using MobiliVendas.Core.Services.Contracts;
using MobiliVendas.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    // Aba "Notas" do Pegada com o layout do NotaFiscalViewController do PegadaIOS (a NotasPage compartilhada segue
    // nos outros apps). Criada pela NegociosPage via Session.CriarNotasPage; ViewModel pelo AutowireViewModel
    // (mapeado no App.xaml.cs) e fixado também no Content, que é o que vai para a aba.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotasPage : ContentPage
    {
        public NotasPage()
        {
            InitializeComponent();

            var viewModel = (NotasPageViewModel)BindingContext;

            // PegadaIOS: não há checkbox de período; as datas começam vazias ("Início"/"Fim") e só filtram quando escolhidas.
            viewModel.FiltrarDataEmissao = true;
            viewModel.Filtro.DataEmissaoInicial = null;
            viewModel.Filtro.DataEmissaoFinal = null;


            // Datas: o parâmetro leva o próprio botão, pro calendário abrir em popover ancorado nele (como no filtro
            // do Pedido / IMDatePickerViewController do PegadaIOS) em vez do DatePromptAsync de tela cheia.
            BtnDataEmissaoInicio.CommandParameter = new SelecaoDataParametro("INICIO", BtnDataEmissaoInicio);
            BtnDataEmissaoFim.CommandParameter = new SelecaoDataParametro("FIM", BtnDataEmissaoFim);

            Content.BindingContext = viewModel;
        }
    }
}
