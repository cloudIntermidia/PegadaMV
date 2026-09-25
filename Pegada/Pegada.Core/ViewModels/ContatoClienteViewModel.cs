using Acr.UserDialogs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using MobiliVendas.Core.Views.Tablet.Shared;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Pegada.Core.ViewModels
{
    // Tela "Contatos" aberta pelo botão "Atualizar Contatos" do fechamento do carrinho
    // (ContatoViewController do PegadaIOS): contatos do cliente, e-mail do cliente e acesso ao cadastro/edição de contato.
    public class ContatoClienteViewModel : ViewModelBase
    {
        private readonly IContatoRepository _contatoRepository;
        private string _codPessoaCliente;
        private string _razaoSocial;
        private Func<Task> _aoVoltar;

        private ObservableCollection<ContatoCommandResult> _contatos = new ObservableCollection<ContatoCommandResult>();
        public ObservableCollection<ContatoCommandResult> Contatos { get => _contatos; set => SetProperty(ref _contatos, value); }

        private ContatoCommandResult _contatoSelecionado;
        public ContatoCommandResult ContatoSelecionado { get => _contatoSelecionado; set => SetProperty(ref _contatoSelecionado, value); }

        private string _emailCliente;
        public string EmailCliente { get => _emailCliente; set => SetProperty(ref _emailCliente, value); }

        // lblFlagEmail ("*Preencha o campo E-mail."): visível enquanto o cliente não tem e-mail.
        private bool _emailPendente = true;
        public bool EmailPendente { get => _emailPendente; set => SetProperty(ref _emailPendente, value); }

        public ICommand VoltarCommand { get; }
        public ICommand AdicionarContatoCommand { get; }
        public ICommand EditarContatoCommand { get; }
        public ICommand SalvarEmailCommand { get; }

        public ContatoClienteViewModel(IContatoRepository contatoRepository) : base()
        {
            _contatoRepository = contatoRepository;

            VoltarCommand = new Command(Voltar);
            AdicionarContatoCommand = new Command(AdicionarContato);
            EditarContatoCommand = new Command(EditarContato);
            SalvarEmailCommand = new Command(SalvarEmail);

            // Contato salvo pelo cadastro (aberto por cima desta tela): recarrega a lista (delegate reloadContatos do iOS).
            MessagingCenter.Subscribe<object>(this, "ContatoSalvo", async (sender) => await CarregarTela());
        }

        public async void Init(string codPessoaCliente, string razaoSocial, Func<Task> aoVoltar)
        {
            _codPessoaCliente = codPessoaCliente;
            _razaoSocial = razaoSocial;
            _aoVoltar = aoVoltar;
            await CarregarTela();
        }

        // -loadTela do ContatoViewController.
        private async Task CarregarTela()
        {
            try
            {
                var contatos = await _contatoRepository.BuscarContatos(_codPessoaCliente);
                Contatos = new ObservableCollection<ContatoCommandResult>(contatos);
                ContatoSelecionado = null;

                var email = await _contatoRepository.BuscarEmailCliente(_codPessoaCliente);
                EmailPendente = email == null;
                EmailCliente = email;

                if (contatos.Count == 0)
                {
                    await UserDialogs.Instance.AlertAsync($"Cadastre um contato para o cliente {_razaoSocial}.", "Atenção", "OK");
                }
                else if (contatos.TrueForAll(c => c.TelefonePendente))
                {
                    // O iOS tem este aviso, mas ele nunca dispara lá (a query troca telefone vazio por "Pendente", que conta como
                    // preenchido). Aqui vale a intenção: nenhum contato com telefone.
                    await UserDialogs.Instance.AlertAsync("Cadastre o telefone para um contato.", "Atenção", "OK");
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
        }

        // touchVoltar: avisa o fechamento (reloadContato) para revalidar e fecha.
        private async void Voltar()
        {
            MessagingCenter.Unsubscribe<object>(this, "ContatoSalvo");
            await PopupNavigation.Instance.PopAsync();
            if (_aoVoltar != null)
                await _aoVoltar();
        }

        private async void SalvarEmail()
        {
            try
            {
                if (string.IsNullOrEmpty(EmailCliente))
                {
                    await UserDialogs.Instance.AlertAsync("Digite um e-mail.", "Atenção", "OK");
                    return;
                }

                if (await _contatoRepository.AtualizarEmailCliente(_codPessoaCliente, EmailCliente))
                {
                    await CarregarTela();
                    await UserDialogs.Instance.AlertAsync("E-mail salvo com sucesso!", "Sucesso", "OK");
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("ocorreu um erro ao salvar e-mail.", "Atenção", "OK");
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
        }

        private async void AdicionarContato()
        {
            await AbrirCadastro(null);
        }

        // Segue "EditaContatoModal": exige um contato selecionado na lista.
        private async void EditarContato()
        {
            if (ContatoSelecionado == null)
            {
                await UserDialogs.Instance.AlertAsync("Selecione um contato", "Atenção", "OK");
                return;
            }

            await AbrirCadastro(ContatoSelecionado);
        }

        // CadastroContatoViewController (modal por cima desta tela): aqui como popup, pois esta tela também é um popup.
        private async Task AbrirCadastro(ContatoCommandResult contato)
        {
            var form = new FormCadastroContatoView();
            form.SetContexto(_codPessoaCliente, contato, emPopup: true);

            var conteudo = form.Content;
            form.Content = null;

            var popup = new PopupPage
            {
                Content = new Frame
                {
                    WidthRequest = 720,
                    HeightRequest = 640,
                    Padding = 0,
                    CornerRadius = 10,
                    HasShadow = true,
                    IsClippedToBounds = true,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = conteudo
                }
            };
            await PopupNavigation.Instance.PushAsync(popup);
        }
    }
}
