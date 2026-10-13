using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MobiliVendas.Core.Domain.Commands.Results;
using Pegada.Core.Services;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    // EnvioPedidoExcelViewController (PegadaIOS).
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnvioPedidoExcelView : PopupPage
    {
        private readonly List<CarrinhoCommandResult> _pedidos;
        private readonly EnvioPedidoExcelService _servico;
        private bool _enviando;

        public EnvioPedidoExcelView(List<CarrinhoCommandResult> pedidos, EnvioPedidoExcelService servico)
        {
            InitializeComponent();
            _pedidos = pedidos;
            _servico = servico;
        }

        // touchSwichEnviaEmailInserido: o campo de e-mail só aparece com a chave ligada; desligar limpa o campo.
        private void OnSomenteDigitadosToggled(object sender, ToggledEventArgs e)
        {
            LblEmail.IsVisible = e.Value;
            TxtEmail.IsVisible = e.Value;
            if (!e.Value)
                TxtEmail.Text = string.Empty;
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        // enviaPedido: manda os pedidos marcados, avisa sucesso/erro e fecha a tela no OK.
        private async void OnEnviarClicked(object sender, EventArgs e)
        {
            if (_enviando)
                return;
            _enviando = true;
            BtnEnviar.IsEnabled = false;

            string titulo = T("EnvioPedidoExcelTitulo");
            string mensagem;
            try
            {
                UserDialogs.Instance.ShowLoading();
                bool sucesso = await _servico.EnviarAsync(_pedidos, TxtEmail.Text, SwitchSomenteDigitados.IsToggled);
                mensagem = sucesso ? T("EnvioPedidoExcelSucesso") : T("EnvioPedidoExcelErro");
            }
            catch (Exception ex)
            {
                mensagem = ex.Message;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }

            await UserDialogs.Instance.AlertAsync(mensagem, titulo, "OK");
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        private static string T(string chave) => new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage(chave);
    }
}
