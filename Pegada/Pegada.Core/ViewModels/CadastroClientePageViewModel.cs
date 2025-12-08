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

namespace Pegada.Core.ViewModels
{
    public class CadastroClientePageViewModel : ViewModelBase
    {
        #region "Propriedades"
        private ClienteCommandResult _cliente;
        public ClienteCommandResult Cliente { get => _cliente; set => SetProperty(ref _cliente, value); }
        public ObservableCollection<ClienteCommandResult> Clientes { get; set; }

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
        
        #endregion

        #region "Repositorios"
        private readonly IClienteRepository _clienteRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly ClienteCommandHandler _clienteCommandHandler;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaRepository;
        #endregion


        public CadastroClientePageViewModel(INavigationService navigationService, IPageDialogService dialogService,
                                            IClienteRepository clienteRepository, IParametroRepository parametroRepository,
                                            ClienteCommandHandler clienteCommandHandler,
                                            IParametroSincronizacaoRepository parametroSincronizacaRepository)
            : base(navigationService, dialogService)
        {
            _clienteRepository = clienteRepository;
            _clienteCommandHandler = clienteCommandHandler;
            _parametroSincronizacaRepository = parametroSincronizacaRepository;
            _parametroRepository = parametroRepository;

            Cliente = new ClienteCommandResult();
            Clientes = new ObservableCollection<ClienteCommandResult>();
            MantemCadastro = false;
            SelecionarClienteCommand = new Command(SelecionarCliente);
            CadastrarClienteNovoCommand = new Command(CadastrarClienteNovo);
            EditarClienteCommand = new DelegateCommand(EditarCliente);
            TransmitirClienteCommand = new DelegateCommand(TransmitirClientes);

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
                    var condicoes = await _clienteRepository.BuscarCategoriaCliente();
                    var categoria = condicoes.Where(x => x.Codigo == Cliente.CodCategoriaCliente).FirstOrDefault();
                    if (categoria != null)
                        CategoriaCliente = categoria.Descricao;
                    var cnpj = Cliente.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "");
                    Cliente.CNPJ = StringHelper.FormatCNPJ(cnpj);
                    Cliente.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(Cliente.CodPessoaCliente);
                    Cliente.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(Cliente.CodPessoaCliente);

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

                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }

            await PopupNavigation.Instance.PopAsync();
        }
        private async void Init()
        {
            var param = await _parametroRepository.BuscarValorParametro(ParametrosSistema.MANTEMCADASTRO);
            MantemCadastro = string.IsNullOrEmpty(param) || param == "S";
        }

        private async void CadastrarClienteNovo()
        {
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupCadastroCliente());
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

                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupEdicaoCliente(Cliente));
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
