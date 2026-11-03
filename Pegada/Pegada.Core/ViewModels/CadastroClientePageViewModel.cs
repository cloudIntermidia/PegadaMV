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

        // Campos de -atualizaTela (CadastroViewController, PegadaIOS) formatados só para a tela: Cliente.EnderecoPrincipal.CEP
        // e Cliente.Agencia/Conta seguem sem formatação porque o formulário de edição salva esses valores.
        private string _paisDescricao;
        public string PaisDescricao { get => _paisDescricao; set => SetProperty(ref _paisDescricao, value); }

        private string _cepUI;
        public string CepUI { get => _cepUI; set => SetProperty(ref _cepUI, value); }

        private string _telefoneUI;
        public string TelefoneUI { get => _telefoneUI; set => SetProperty(ref _telefoneUI, value); }

        private string _agenciaUI;
        public string AgenciaUI { get => _agenciaUI; set => SetProperty(ref _agenciaUI, value); }

        private string _contaUI;
        public string ContaUI { get => _contaUI; set => SetProperty(ref _contaUI, value); }

        private EnderecoCommandResult _enderecoEntrega;
        public EnderecoCommandResult EnderecoEntrega { get => _enderecoEntrega; set => SetProperty(ref _enderecoEntrega, value); }

        #endregion

        #region "Commands"
        public ICommand SelecionarClienteCommand { get; set; }
        public ICommand CadastrarClienteNovoCommand { get; set; }
        public ICommand EditarClienteCommand { get; set; }
        public ICommand TransmitirClienteCommand { get; set; }
        public ICommand NovoContatoCommand { get; set; }

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

            // -modalDismissed: (CadastroViewController, PegadaIOS): salvar contato põe o cliente em situação 50, então recarrega a tela toda.
            MessagingCenter.Subscribe<object>(this, "ContatoSalvo", async (sender) => await RecarregarClienteEditado());

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
            // PegadaIOS (-atualizaTela): cliente sem CNPJ ou CEP aparece com o campo em branco, sem erro.
            if (!string.IsNullOrEmpty(Cliente.CNPJ))
            {
                var cnpj = Cliente.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "");
                Cliente.CNPJ = StringHelper.FormatCNPJ(cnpj);
            }
            Cliente.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(Cliente.CodPessoaCliente);
            Cliente.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(Cliente.CodPessoaCliente);

            // Equivalente a "mc.nomeRepresentante" em -atualizaTela (CadastroViewController, PegadaIOS).
            Cliente.Representante = await _clienteRepository.BuscarRepresentantePorCliente(Cliente);

            CepUI = FormatarCepUI(Cliente.EnderecoPrincipal?.CEP);

            if (!string.IsNullOrEmpty(Cliente.EnderecoPrincipal?.CEP))
            {
                var cep = Cliente.EnderecoPrincipal.CEP.Replace("-", "");
                Cliente.EnderecoPrincipal.CEP = StringHelper.FormatCEP(cep);
            }

            if (!string.IsNullOrEmpty(Cliente.EnderecoCobranca?.CEP))
            {
                var cep = Cliente.EnderecoCobranca.CEP.Replace("-", "");
                Cliente.EnderecoCobranca.CEP = StringHelper.FormatCEP(cep);
            }

            // PegadaIOS: "ee" é o endereço principal do cliente de entrega (c.codClienteEntrega).
            EnderecoEntrega = string.IsNullOrEmpty(Cliente.CodClienteEntrega)
                ? null
                : await _clienteRepository.BuscarEnderecoPrincipal(Cliente.CodClienteEntrega);

            PaisDescricao = string.IsNullOrEmpty(Cliente.CodPais) ? null : await _clienteRepository.BuscarDescricaoPais(Cliente.CodPais);

            // PegadaIOS: "_pFone = c.telefone" (Telefone1). O Pegada_download ainda não envia esse campo; enquanto isso
            // mostra o telefone do endereço principal para não deixar em branco.
            TelefoneUI = !string.IsNullOrWhiteSpace(Cliente.Telefone1) ? Cliente.Telefone1 : Cliente.EnderecoPrincipal?.Telefone;

            AgenciaUI = JuntarComDigito(Cliente.Agencia, Cliente.DigitoAgencia);
            ContaUI = JuntarComDigito(Cliente.Conta, Cliente.DigitoConta);

            await CarregarContatos();
        }

        // Regra do CEP de -atualizaTela (CadastroViewController, PegadaIOS, Defeito #19015).
        private static string FormatarCepUI(string cep)
        {
            var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());

            if (digitos.Length > 7)
                return $"{digitos.Substring(0, 2)}.{digitos.Substring(2, 3)}-{digitos.Substring(5, 3)}";

            if (digitos.Length == 7)
                return $"0{digitos.Substring(0, 1)}.{digitos.Substring(1, 3)}-{digitos.Substring(4, 3)}";

            return string.Empty;
        }

        // PegadaIOS: "%@ - %@" (agência/conta - dígito), sem o "(null)" que o iOS mostra quando falta o dígito.
        private static string JuntarComDigito(string valor, string digito)
        {
            return string.Join(" - ", new[] { valor, digito }.Where(x => !string.IsNullOrWhiteSpace(x)));
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
                ? await _contatoRepository.BuscarContatosGrupoCadastro(Cliente.CodGrupoCliente)
                : contatos;

            foreach (var contato in contatosGrupo)
                ContatosGrupo.Add(contato);
        }

        private async void NovoContato()
        {
            if (Cliente == null || Cliente.CodPessoaCliente == null)
            {
                await UserDialogs.Instance.AlertAsync(new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteMsgSelecioneClientePrimeiro"), AppName, "OK");
                return;
            }

            // PegadaIOS: CadastroContatoViewController em formSheet (popup), não em tela cheia.
            await Views.Negocio.CadastroContatoPopupView.AbrirAsync(Cliente.CodPessoaCliente);
        }

        // Opção "Editar" do toque longo na lista de contatos (-longPress:, CadastroViewController, PegadaIOS). Na aba Contatos
        // Grupo o contato pode ser de outro cliente da rede, então vale o cliente do próprio contato.
        public async void EditarContato(ContatoCommandResult contato)
        {
            try
            {
                await Views.Negocio.CadastroContatoPopupView.AbrirAsync(contato.CodPessoaCliente, contato);
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        // Opção "Inativar" do mesmo menu (ContatoMenuOptionExcluir no PegadaIOS).
        public async void InativarContato(ContatoCommandResult contato)
        {
            try
            {
                var t = new MobiliVendas.Core.Helpers.TranslateExtension();
                if (!await UserDialogs.Instance.ConfirmAsync(t.GetMessage("PegCadClienteMsgInativarContato"), t.GetMessage("GlobalTituloAtencao"), t.GetMessage("PegCadClienteBtnSim"), t.GetMessage("PegCadClienteBtnNao")))
                    return;

                await _contatoRepository.InativarContato(contato);
                await RecarregarClienteEditado();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
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
                    await UserDialogs.Instance.AlertAsync(new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteMsgSelecioneClientePrimeiro"), AppName, "OK");
                    return;
                }

                // -touchEditaClienteBtn: e -prepareForSegue: (CadastroViewController, PegadaIOS): abre para qualquer cliente selecionado,
                // com uma cópia nova do banco; assim Cancelar não muda o que está na tela.
                var clienteEdicao = await _clienteRepository.BuscarCliente(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, Cliente.CodPessoaCliente, Session.USUARIO_LOGADO.CodTipoPessoa));
                if (clienteEdicao == null)
                    return;

                clienteEdicao.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(clienteEdicao.CodPessoaCliente);
                clienteEdicao.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(clienteEdicao.CodPessoaCliente);

                await RgPopupUtility.AbrirEdicaoCliente(clienteEdicao);
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
                var confirm = await UserDialogs.Instance.ConfirmAsync(new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteMsgConfirmTransmitir"), new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteBtnConfirmTransmitirTitulo"), new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteBtnSim"), new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteBtnNao"));
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
                        // -transmiteCadastros (PegadaIOS): todo cliente em situação 50 é transmitido, inclusive cliente do ERP editado.
                        var clienteERP = cliente.CodPessoaCliente.Contains(".") ? await _clienteRepository.BuscarClienteIntegrado(cliente.CNPJ) : null;
                        if (clienteERP == null)
                        {
                            var resultTransmissaoCliente = await ServiceUtility.TransmitirCliente(_clienteRepository, _parametroSincronizacaRepository, cliente.CodPessoaCliente);
                            if (resultTransmissaoCliente.SUCCESS.ToString().ToUpper() == "TRUE")
                            {
                                await _clienteRepository.AtualizarClienteIntegrado(cliente.CodPessoaCliente, resultTransmissaoCliente.CODIGO.ToString());
                            }
                        }
                    }
                    UserDialogs.Instance.HideLoading();

                    string msg = new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteMsgCadastrosEnviados");
                    await UserDialogs.Instance.AlertAsync(msg, AppName);
                }
                else {
                    string msg = new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage("PegCadClienteMsgNenhumCadastro");
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
