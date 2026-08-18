using Prism.Commands;
using System;
using Prism.Navigation;
using Prism.Services;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Commands.Inputs;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MobiliVendas.Core.Domain.Commands.Handlers;
using Rg.Plugins.Popup.Services;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.Domain.StaticObject;
using Acr.UserDialogs;
using System.Windows.Input;
using Xamarin.Forms;
using MobiliVendas.Core.ViewModels;
using MobiliVendas.Core;
using System.Linq;
using MobiliVendas.Core.Services;
using MobiliVendas.Core.Views.Tablet.Shared;

namespace Pegada.Core.ViewModels
{
    public class CadastroClientePageViewModel : ViewModelBase
    {
        #region "Propriedades"
        private ClienteCommandResult _cliente;
        public ClienteCommandResult Cliente { get => _cliente; set => SetProperty(ref _cliente, value); }
        public ObservableCollection<ClienteCommandResult> Clientes { get; set; }
        public ObservableCollection<ContatoCommandResult> Contatos { get; set; }
        public ObservableCollection<ContatoCommandResult> ContatosGrupo { get; set; }

        private string _filtroPesquisa;
        public string FiltroPesquisa
        {
            get { return _filtroPesquisa; }
            set { SetProperty(ref _filtroPesquisa, value); }
        }

        private bool _mantemCadastro;
        public bool MantemCadastro
        {
            get { return _mantemCadastro; }
            set { SetProperty(ref _mantemCadastro, value); }
        }

        private string _categoriaCliente;
        public string CategoriaCliente
        {
            get { return _categoriaCliente; }
            set { SetProperty(ref _categoriaCliente, value); }
        }

        #endregion

        #region "Commands"
        public ICommand SelecionarClienteCommand { get; set; }
        public ICommand CadastrarClienteNovoCommand { get; set; }
        public ICommand EditarClienteCommand { get; set; }
        public ICommand TransmitirClienteCommand { get; set; }
        public ICommand NovoContatoCommand { get; set; }
        public ICommand EditarContatoCommand { get; set; }

        #endregion

        #region "Repositorios"
        private readonly IClienteRepository _clienteRepository;
        private readonly IContatoRepository _contatoRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly ClienteCommandHandler _clienteCommandHandler;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaRepository;
        #endregion


        public CadastroClientePageViewModel(INavigationService navigationService, IPageDialogService dialogService,
                                            IClienteRepository clienteRepository, IContatoRepository contatoRepository, IParametroRepository parametroRepository,
                                            ClienteCommandHandler clienteCommandHandler,
                                            IParametroSincronizacaoRepository parametroSincronizacaRepository)
            : base(navigationService, dialogService)
        {
            _clienteRepository = clienteRepository;
            _contatoRepository = contatoRepository;
            _clienteCommandHandler = clienteCommandHandler;
            _parametroSincronizacaRepository = parametroSincronizacaRepository;
            _parametroRepository = parametroRepository;

            Cliente = new ClienteCommandResult();
            Clientes = new ObservableCollection<ClienteCommandResult>();
            Contatos = new ObservableCollection<ContatoCommandResult>();
            ContatosGrupo = new ObservableCollection<ContatoCommandResult>();
            MantemCadastro = false;
            SelecionarClienteCommand = new Command(SelecionarCliente);
            CadastrarClienteNovoCommand = new Command(CadastrarClienteNovo);
            EditarClienteCommand = new DelegateCommand(EditarCliente);
            TransmitirClienteCommand = new DelegateCommand(TransmitirClientes);
            NovoContatoCommand = new Command(NovoContato);
            EditarContatoCommand = new Command<object>(EditarContato);

            MessagingCenter.Subscribe<object>(this, "ContatoSalvo", async (sender) => await CarregarContatos());

            // Equivalente ao delegate "atualizaCliente:" do CadastroViewController (PegadaIOS): sem
            // isso, campos derivados de JOIN (Condição de Pagamento, Classe de Risco etc.) continuavam
            // mostrando o valor antigo depois de editar - eles só vêm de uma consulta nova, não do
            // objeto que a tela de edição devolve. FormCadastroClienteViewModel manda essa mensagem
            // ao salvar uma edição, mas nada estava escutando.
            MessagingCenter.Subscribe<object>(this, "EditCadastroCliente", async (sender) => await RecarregarClienteEditado());

            Init();
        }

        #region "Metodos"
        private async void SelecionarCliente(object obj)
        {
            try
            {
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupSelecaoCliente(SelecionarClienteEvent));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }
        private async void SelecionarClienteEvent(object obj)
        {
            try
            {
                if (obj != null)
                {
                    Cliente = obj as ClienteCommandResult;
                    await CarregarDadosCliente();
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }

            await PopupNavigation.Instance.PopAsync();
        }

        private async Task CarregarDadosCliente()
        {
            var condicoes = await _clienteRepository.BuscarCategoriaCliente();
            var categoria = condicoes.Where(x => x.Codigo == Cliente.CodCategoriaCliente).FirstOrDefault();
            if (categoria != null)
                CategoriaCliente = categoria.Descricao;
            var cnpj = Cliente.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "");
            Cliente.CNPJ = StringHelper.FormatCNPJ(cnpj);
            Cliente.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(Cliente.CodPessoaCliente);
            Cliente.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(Cliente.CodPessoaCliente);

            // Equivalente a "mc.nomeRepresentante" em -atualizaTela (CadastroViewController, PegadaIOS).
            Cliente.Representante = await _clienteRepository.BuscarRepresentantePorCliente(Cliente);

            if (Cliente.EnderecoPrincipal != null)
            {
                var cep = Cliente.EnderecoPrincipal.CEP.Replace("-", "");
                Cliente.EnderecoPrincipal.CEP = StringHelper.FormatCEP(cep);
            }

            if (Cliente.EnderecoCobranca != null)
            {
                var cep = Cliente.EnderecoCobranca.CEP.Replace("-", "");
                Cliente.EnderecoCobranca.CEP = StringHelper.FormatCEP(cep);
            }

            await CarregarContatos();
        }

        // Equivalente ao delegate "atualizaCliente:" do CadastroViewController (PegadaIOS): busca o
        // cliente de novo do banco (mesma consulta usada na seleção) para trazer os campos derivados
        // de JOIN (Condição de Pagamento, Classe de Risco etc.) atualizados após uma edição.
        private async Task RecarregarClienteEditado()
        {
            try
            {
                if (Cliente?.CodPessoaCliente == null)
                    return;

                var clienteAtualizado = await _clienteRepository.BuscarCliente(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, Cliente.CodPessoaCliente, Session.USUARIO_LOGADO.CodTipoPessoa));
                if (clienteAtualizado == null)
                    return;

                Cliente = clienteAtualizado;
                await CarregarDadosCliente();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async Task CarregarContatos()
        {
            Contatos.Clear();
            ContatosGrupo.Clear();

            if (Cliente?.CodPessoaCliente == null)
                return;

            var contatos = await _contatoRepository.BuscarContatos(Cliente.CodPessoaCliente);
            foreach (var contato in contatos)
                Contatos.Add(contato);

            // Equivalente ao trecho "if(c.codGrupoCliente){ contatosGrupoWithCodPessoaCliente: }
            // else{ contatosWithCodPessoaCliente: }" de -atualizaTela (CadastroViewController, PegadaIOS):
            // sem grupo, "Contatos Grupo" mostra os mesmos contatos do próprio cliente.
            var contatosGrupo = !string.IsNullOrEmpty(Cliente.CodGrupoCliente)
                ? await _contatoRepository.BuscarContatosGrupo(Cliente.CodGrupoCliente)
                : contatos;

            foreach (var contato in contatosGrupo)
                ContatosGrupo.Add(contato);
        }

        private async void NovoContato()
        {
            if (Cliente == null || Cliente.CodPessoaCliente == null)
            {
                await UserDialogs.Instance.AlertAsync("Selecione o cliente primeiro.", AppName, "OK");
                return;
            }

            var page = new FormCadastroContatoView();
            page.SetContexto(Cliente.CodPessoaCliente);
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        private async void EditarContato(object obj)
        {
            var contato = (obj as Syncfusion.ListView.XForms.ItemTappedEventArgs)?.ItemData as ContatoCommandResult;
            if (contato == null)
                return;

            var page = new FormCadastroContatoView();
            page.SetContexto(Cliente.CodPessoaCliente, contato);
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }
        private async void Init()
        {
            var param = await _parametroRepository.BuscarValorParametro(ParametrosSistema.MANTEMCADASTRO);
            MantemCadastro = string.IsNullOrEmpty(param) || param == "S";
        }

        private async void CadastrarClienteNovo()
        {
            await RgPopupUtility.AbrirCadastroCliente();
        }

        private async void EditarCliente()
        {
            try
            {
                if (Cliente == null || Cliente.CodPessoaCliente == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione o cliente primeiro.", AppName, "OK");
                    return;
                }

                if (Cliente.CodSituacaoCliente != "50")
                {
                    await UserDialogs.Instance.AlertAsync("Edição de cliente ainda não está disponivel.", AppName, "OK");
                    return;
                }

                await RgPopupUtility.AbrirEdicaoCliente(Cliente);
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void TransmitirClientes()
        {
            try
            {
                var confirm = await UserDialogs.Instance.ConfirmAsync($"Esta ação irá transmitir todos os cadastros de clientes novos, deseja prosseguir?", "Transmitir", "Sim", "Não");
                if (!confirm)
                {
                    return;
                }

                var listClientes = await _clienteRepository.BuscarClientesTransmitir(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, null, Session.USUARIO_LOGADO.CodTipoPessoa, FiltroPesquisa)).ConfigureAwait(false);

                if (listClientes.Count > 0)
                {

                    UserDialogs.Instance.ShowLoading($"Transmitindo o cadastros...");
                    foreach (var cliente in listClientes)
                    {
                        if (cliente.CodPessoaCliente.Contains("."))
                        {
                            var clienteERP = await _clienteRepository.BuscarClienteIntegrado(cliente.CNPJ);
                            if (clienteERP == null)
                            {
                                var resultTransmissaoCliente = await ServiceUtility.TransmitirCliente(_clienteRepository, _parametroSincronizacaRepository, cliente.CodPessoaCliente);
                                if (resultTransmissaoCliente.SUCCESS.ToString().ToUpper() == "TRUE")
                                {
                                    await _clienteRepository.AtualizarClienteIntegrado(cliente.CodPessoaCliente, resultTransmissaoCliente.CODIGO.ToString());
                                }
                            }
                        }
                    }
                    UserDialogs.Instance.HideLoading();

                    string msg = "Cadastros enviados com sucesso!";
                    await UserDialogs.Instance.AlertAsync(msg, AppName);
                }
                else {
                    string msg = "Nenhum cadastro encontrado para transmissão.";
                    await UserDialogs.Instance.AlertAsync(msg, AppName);
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }
        #endregion
    }
}
