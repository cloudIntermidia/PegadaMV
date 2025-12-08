using Acr.UserDialogs;
using MobiliVendas.Core.Contracts;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Pegada.Core.Views.Carrinho
{
    public partial class CarrinhoFechamentoView : ContentView, ICarrinhoFechamentoView
    {
        public CarrinhoFechamentoView()
        {
            InitializeComponent();
        }

        public View GetContent()
        {
            return this.Content;
        }

        public void SetBindingContext(ICarrinhoFechamentoViewModel viewModel)
        {
            BindingContext = viewModel;
        }

        private string _valorAntigo;
        private async void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex(@"^([0-9]*)?([,.][0-9]{0,2})?$");
            if (!regex.IsMatch(EdtDesconto.Text))
            {
                EdtDesconto.TextChanged -= Editor_TextChanged;
                EdtDesconto.Text = _valorAntigo;
                EdtDesconto.TextChanged += Editor_TextChanged;
                await UserDialogs.Instance.AlertAsync("Formato do desconto inválido", "OK");
            }
            else
            {
                _valorAntigo = EdtDesconto.Text;
            }
        }
    }
}
