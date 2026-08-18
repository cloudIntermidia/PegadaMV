using Acr.UserDialogs;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Services;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

using ItemTappedEventArgs = Syncfusion.ListView.XForms.ItemTappedEventArgs;

namespace Pegada.Core.ViewModels
{
    public class NovoAtendimentoViewModel : ViewModelBase, INovoAtendimentoViewModel
    {
        #region "Propriedades"
        private CarrinhoFechamentoCommandResult _carrinhoFechamento;
        public CarrinhoFechamentoCommandResult CarrinhoFechamento
        {
            get { return _carrinhoFechamento; }
            set { SetProperty(ref _carrinhoFechamento, value); }
        }

        private ClienteCommandResult _clienteSelecionado;
        public ClienteCommandResult ClienteSelecionado
        {
            get { return _clienteSelecionado ?? new ClienteCommandResult(); }
            set { SetProperty(ref _clienteSelecionado, value); }
        }

        private PessoaCommandResult _prepostoSelecionado;
        public PessoaCommandResult PrepostoSelecionado
        {
            get { return _prepostoSelecionado ?? new PessoaCommandResult(); }
            set { SetProperty(ref _prepostoSelecionado, value); }
        }

        private string _licenciamento;
        public string IsLicenciamento
        {
            get { return _licenciamento; }
            set { SetProperty(ref _licenciamento, value); }
        }

        private string _liberaClienteNovo;
        public string LiberaClienteNovo
        {
            get { return _liberaClienteNovo; }
            set { SetProperty(ref _liberaClienteNovo, value); }
        }

        private TabelaPrecoResult _tabelaPrecoSelecionada;
        public TabelaPrecoResult TabelaPrecoSelecionada
        {
            get { return _tabelaPrecoSelecionada ?? new TabelaPrecoResult(); }
            set { SetProperty(ref _tabelaPrecoSelecionada, value); }
        }

        private List<PessoaCommandResult> _listaPreposto;
        public List<PessoaCommandResult> ListaPreposto
        {
            get { return _listaPreposto; }
            set { SetProperty(ref _listaPreposto, value); }
        }

        private GenericComboResult _tipoAtendimento;
        public GenericComboResult TipoAtendimento
        {
            get { return _tipoAtendimento; }
            set { SetProperty(ref _tipoAtendimento, value); }
        }

        private GenericComboResult _ufSelecionada;
        public GenericComboResult UfSelecionada
        {
            get { return _ufSelecionada; }
            set { SetProperty(ref _ufSelecionada, value); }
        }

        private GenericComboResult _cidadeSelecionada;
        public GenericComboResult CidadeSelecionada
        {
            get { return _cidadeSelecionada; }
            set { SetProperty(ref _cidadeSelecionada, value); }
        }

        private GenericComboResult _situacaoSelecionada;
        public GenericComboResult SituacaoSelecionada
        {
            get { return _situacaoSelecionada; }
            set { SetProperty(ref _situacaoSelecionada, value); }
        }

        private GenericComboResult _classeRiscoSelecionada;
        public GenericComboResult ClasseRiscoSelecionada
        {
            get { return _classeRiscoSelecionada; }
            set { SetProperty(ref _classeRiscoSelecionada, value); }
        }

        private GenericComboResult _statusSelecionado;
        public GenericComboResult StatusSelecionado
        {
            get { return _statusSelecionado; }
            set { SetProperty(ref _statusSelecionado, value); }
        }

        private List<GenericComboResult> _listaCidade;
        public List<GenericComboResult> ListaCidade
        {
            get { return _listaCidade; }
            set { SetProperty(ref _listaCidade, value); }
        }

        private string filtroPesquisa;
        public string FiltroPesquisa
        {
            get { return filtroPesquisa; }
            set
            {
                SetProperty(ref filtroPesquisa, value);

                // Equivalente ao -updateClientesWithSearch: (EdicaoAtendimentoViewController, PegadaIOS):
                // filtro em memória sobre a lista já carregada, aplicado a cada tecla digitada.
                FiltrarClientes();
            }
        }

        public ObservableCollection<ClienteCommandResult> Clientes { get; set; }

        private List<ClienteCommandResult> ListaClientesOriginal;

        // Equivalente a "_atualizaClienteCheked" (EdicaoAtendimentoViewController, PegadaIOS): liga/desliga
        // o modo de seleção com checkbox para atualização de situação comercial.
        private bool _modoAtualizacaoCadastro;
        public bool ModoAtualizacaoCadastro
        {
            get { return _modoAtualizacaoCadastro; }
            set { SetProperty(ref _modoAtualizacaoCadastro, value); }
        }

        public string TextoBotaoAtualizarCadastro => ModoAtualizacaoCadastro ? "Cancelar" : "Atualizar Cadastro";
        public string TextoBotaoTransmitir => ModoAtualizacaoCadastro ? "Atualizar Selecionados" : "Transmitir Cliente";

        #endregion

        #region "Repositorios"
        private readonly IClienteRepository _clienteRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly ITabelaPrecoRepository _tabelaPrecoRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaRepository;
        private readonly AtendimentoUtility _atendimentoUtility;
        #endregion


        #region "Commands"
        public ICommand SalvarAtendimentoCommand { get; set; }
        public ICommand CancelarAtendimentoCommand { get; set; }
        public ICommand TipoVendaVendorCommand { get; set; }
        public DelegateCommand<object> SelecionarClienteCommand { get; set; }
        public ICommand SelecionarPrepostoCommand { get; set; }
        public ICommand SelecionarTipoAtendimentoCommand { get; set; }
        public ICommand InfoCommand { get; set; }
        public ICommand SelecionarTabelaPrecoCommand { get; set; }
        public ICommand SelecionarCondicaoPagamentoCommand { get; set; }
        public ICommand SelecionarTipoPedidoCommand { get; set; }

        public ICommand PesquisarClienteCommand { get; set; }

        public ICommand FiltroUFCommand { get; set; }
        public ICommand FiltroCidadeCommand { get; set; }
        public ICommand FiltroSituacaoCommand { get; set; }
        public ICommand FiltroClasseRiscoCommand { get; set; }
        public ICommand FiltroStatusCommand { get; set; }

        // Equivalentes aos botões da toolbar do EdicaoAtendimentoViewController (PegadaIOS):
        // "Novo Cliente" (abre o cadastro) e "Transmitir Cliente" (envia cadastros novos pendentes, ou
        // atualiza a situação comercial dos selecionados quando ModoAtualizacaoCadastro está ligado).
        public ICommand NovoClienteCommand { get; set; }
        public ICommand TransmitirClienteCommand { get; set; }

        // Equivalentes a "touchAtualizarCancelarSituacaoCadastroClientes" e "cell:didCheckEdicaoAtendimento:"
        // (EdicaoAtendimentoViewController / EdicaoAtendimentoTableViewCell, PegadaIOS).
        public ICommand AtualizarCadastroCommand { get; set; }
        public ICommand MarcarClienteCommand { get; set; }

        private readonly ICoeficienteRepository _coeficienteRepository;
        private readonly ICondicaoPagamentoRepository _condicaoPagamentoRepository;
        #endregion

        #region "Construtores"
        public NovoAtendimentoViewModel(
            IClienteRepository clienteRepository,
            IAtendimentoRepository atendimentoRepository,
            ITabelaPrecoRepository tabelaPrecoRepository,
            IParametroRepository parametroRepository,
            AtendimentoUtility atendimentoUtility,
            ICoeficienteRepository coeficienteRepository,
            ICondicaoPagamentoRepository condicaoPagamento,
            IPessoaRepository pessoaRepository,
            IParametroSincronizacaoRepository parametroSincronizacaRepository
            )
        {
            CarrinhoFechamento = new CarrinhoFechamentoCommandResult();
            CarrinhoFechamento.TabelaPreco = "Tabela de Preço";

            _clienteRepository = clienteRepository;
            _atendimentoRepository = atendimentoRepository;
            _tabelaPrecoRepository = tabelaPrecoRepository;
            _atendimentoUtility = atendimentoUtility;
            _parametroRepository = parametroRepository;
            _atendimentoUtility.CarrinhoFechamento = CarrinhoFechamento;
            _coeficienteRepository = coeficienteRepository;
            _condicaoPagamentoRepository = condicaoPagamento;
            _pessoaRepository = pessoaRepository;
            _parametroSincronizacaRepository = parametroSincronizacaRepository;

            SalvarAtendimentoCommand = new DelegateCommand<object>(SalvarAtendimento);
            CancelarAtendimentoCommand = new DelegateCommand<object>(FecharPopupAtendimento);
            InfoCommand = new Command(VisualizarInfoCliente);
            SelecionarCondicaoPagamentoCommand = new Command(async () => await _atendimentoUtility.SelecionarCondicaoPagamento());
            //SelecionarTabelaPrecoCommand = new Command(async () => await _atendimentoUtility.SelecionarTabelaPreco(true));
            SelecionarTabelaPrecoCommand = new Command(async () => await _atendimentoUtility.SelecionarTabelaPrecoCliente(ClienteSelecionado?.CodPessoaCliente));
            SelecionarTipoPedidoCommand = new Command(async () => await _atendimentoUtility.SelecionarTipoPedido());
            SelecionarClienteCommand = new DelegateCommand<object>(async (obj) => await ClienteTapped(obj));
            SelecionarPrepostoCommand = new Command(SelecionarPreposto);

            FiltroUFCommand = new Command(SelecionarUF);
            FiltroCidadeCommand = new Command(SelecionarCidade);
            FiltroSituacaoCommand = new Command(SelecionarSituacao);
            FiltroClasseRiscoCommand = new Command(SelecionarClasseRisco);
            FiltroStatusCommand = new Command(SelecionarStatus);

            PesquisarClienteCommand = new DelegateCommand(PesquisarCliente);

            SelecionarTipoAtendimentoCommand = new Command(SelecionarTipoAtendimento);

            NovoClienteCommand = new Command(async () => await NovoCliente());
            TransmitirClienteCommand = new Command(async () => await TransmitirClientes());
            AtualizarCadastroCommand = new Command(AlternarModoAtualizacaoCadastro);
            MarcarClienteCommand = new Command<object>(MarcarCliente);

            Clientes = new ObservableCollection<ClienteCommandResult>();

            Init();
        }
        #endregion

        #region "Metodos"
        private async void Init()
        {
            try
            {
                await CarregaPrepostos();

                await CarregarClientes(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, null, Session.USUARIO_LOGADO.CodTipoPessoa, FiltroPesquisa)).ConfigureAwait(false);

                TipoAtendimento = new GenericComboResult();
                TipoAtendimento.Codigo = "1";
                TipoAtendimento.Descricao = "Cliente";
                RaisePropertyChanged("TipoAtendimento");

                LiberaClienteNovo = await _parametroRepository.BuscarValorParametro(ParametrosSistema.BLOQCLIENOVO);
                if (string.IsNullOrEmpty(LiberaClienteNovo)) {
                    LiberaClienteNovo = "N";
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void PesquisarCliente()
        {

            var command = new BuscarClienteCommand(
                                                    Session.USUARIO_LOGADO.CodPessoa,
                                                    Session.USUARIO_LOGADO.CodMarca,
                                                    null,
                                                    Session.USUARIO_LOGADO.CodTipoPessoa,
                                                    FiltroPesquisa,
                                                    UfSelecionada?.Codigo,
                                                    CidadeSelecionada?.Descricao,
                                                    SituacaoSelecionada?.Codigo,
                                                    ClasseRiscoSelecionada?.Codigo,
                                                    StatusSelecionado?.Codigo);


            CarregarClientes(command);
        }

        // Equivalente a -updateClientesWithSearch: (EdicaoAtendimentoViewController, PegadaIOS): filtra
        // em memória a lista já carregada (ListaClientesOriginal), sem nova consulta ao banco.
        private void FiltrarClientes()
        {
            if (ListaClientesOriginal == null)
                return;

            if (string.IsNullOrWhiteSpace(FiltroPesquisa))
            {
                Clientes.Clear();

                foreach (var item in ListaClientesOriginal)
                    Clientes.Add(item);

                return;
            }

            string texto = FiltroPesquisa.ToLower();

            var filtrados = ListaClientesOriginal.Where(x =>
                   (!string.IsNullOrEmpty(x.RazaoSocial) &&
                    x.RazaoSocial.ToLower().Contains(texto))

                || (!string.IsNullOrEmpty(x.NomeFantasia) &&
                    x.NomeFantasia.ToLower().Contains(texto))

                || (!string.IsNullOrEmpty(x.CNPJ) &&
                    x.CNPJ.ToLower().Contains(texto))

                || (!string.IsNullOrEmpty(x.Cidade) &&
                    x.Cidade.ToLower().Contains(texto))
            ).ToList();

            Clientes.Clear();

            foreach (var item in filtrados)
                Clientes.Add(item);
        }



        private async Task CarregaPrepostos() {
            if (Session.USUARIO_LOGADO.CodTipoPessoa == "3")
            {
                ListaPreposto = await _pessoaRepository.BuscarPrepostos(new BuscarPessoaCommand() { CodPessoaVendedor = Session.USUARIO_LOGADO.CodPessoa });
                if (ListaPreposto.Count > 0) {
                    PrepostoSelecionado = ListaPreposto[0];
                }
                //_comboPreposto.enabled = NO;
            }
            else
            {
                ListaPreposto = await _pessoaRepository.BuscarPrepostosPorRep(new BuscarPessoaCommand() { CodPessoaVendedor = Session.USUARIO_LOGADO.CodPessoa });
                if (ListaPreposto.Count > 0)
                {
                    PrepostoSelecionado = ListaPreposto[0];
                }
            }
        }

        public async Task CarregarClientes(BuscarClienteCommand command)
        {
            try
            {
                Clientes.Clear();
                UserDialogs.Instance.ShowLoading(!string.IsNullOrEmpty(ConfiguracaoVisual.LabelCliente) ? "Carregando dados de Loja" : "Carregando dados de clientes");

                //CASO ESTEJA EM FEIRA PODERA SER DISTRIBUIDO A COMISSAO AO REP LOGADO E AO REP DO CLIENTE
                var chaveLiberaClienteFeira = await _parametroRepository.BuscarValorParametro(ParametrosSistema.LIBERACLIENTEFEIRA);

                var listaClientes = new List<ClienteCommandResult>();
                if (chaveLiberaClienteFeira == "TRUE" || Session.USUARIO_LOGADO.CodTipoPessoa == "1")
                {
                    listaClientes = await _clienteRepository.BuscarTodosClientes(command).ConfigureAwait(false);
                }
                else
                {
                    listaClientes = await _clienteRepository.BuscarClientes(command).ConfigureAwait(false);
                }

                if (listaClientes?.Count > 0)
                {
                    listaClientes = listaClientes.ToList();
                    //Alterações em tela precisam da MainThread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var cliente in listaClientes)
                        {
                            cliente.InfoCommand = InfoCommand;
                            //cliente.TituloAbertoCommand = TituloAbertoCommand;
                            Clientes.Add(cliente);
                        }
                    });
                }

                // Equivalente a "self.allClientes" (EdicaoAtendimentoViewController, PegadaIOS): base usada
                // pelo filtro de pesquisa em memória (FiltrarClientes), separada da consulta ao banco.
                ListaClientesOriginal = listaClientes ?? new List<ClienteCommandResult>();

                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async Task ClienteTapped(object item)
        {
            try
            {
                if (item is ItemTappedEventArgs eventArgs)
                {
                    _clienteSelecionado = eventArgs.ItemData as ClienteCommandResult;
                    _clienteSelecionado.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(_clienteSelecionado.CodPessoaCliente);
                    _clienteSelecionado.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(_clienteSelecionado.CodPessoaCliente);
                    SelecionarClienteEvent(_clienteSelecionado);
                }
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
                ClienteSelecionado = obj as ClienteCommandResult;
                await SelecaoTabelaPrecoFromInit();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }



        private async void SelecionarPreposto(object obj) {
            var genricCombo = ListaPreposto.Select(x => new GenericComboResult() { Codigo = x.CodPessoa, Descricao = x.Nome }).ToList();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(genricCombo), PrepSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
        }


        //######################## filtro ###################
        private async void SelecionarUF(object obj)
        {
            var lista = ObterUFs();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(lista), SetUFSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.40), false, false, false));
        }

        private async void SelecionarCidade(object obj)
        {
            if (ListaCidade != null)
            {
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(ListaCidade), SetCidadeSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.5), false, false, false));
            }
            else {
                await UserDialogs.Instance.AlertAsync("Selecione uma UF para buscar por Cidade!");
                return;
            }
        }

        private async void SelecionarSituacao(object obj)
        {
            var listaGenerica = await _clienteRepository.BuscarSituacao();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(listaGenerica), SetSituacaoSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
        }

        private async void SelecionarClasseRisco(object obj)
        {
            var listaGenerica = await _clienteRepository.BuscarClasseRisco();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(listaGenerica), SetClasseRiscoSelecionado, new Rectangle(0.5, 0.5, 0.75, 0.40), false, false, false));
        }

        private async void SelecionarStatus(object obj)
        {
            var listaGenerica = ObterStatus();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(listaGenerica), SetStatusSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
        }

        private async void SetUFSelecionado(object obj)
        {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            UfSelecionada = obj as GenericComboResult;

            await CarregaCidadePorUF(UfSelecionada);

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async void SetCidadeSelecionado(object obj)
        {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            CidadeSelecionada = obj as GenericComboResult;

            // Equivalente a -setCidadeSelecionada: (EdicaoAtendimentoViewController, PegadaIOS): recarrega
            // a lista de clientes assim que a cidade é escolhida.
            PesquisarCliente();

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async void SetSituacaoSelecionado(object obj)
        {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            SituacaoSelecionada = obj as GenericComboResult;

            // Equivalente a -setSituacaoClienteSelecionada: (EdicaoAtendimentoViewController, PegadaIOS).
            PesquisarCliente();

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async void SetClasseRiscoSelecionado(object obj)
        {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            ClasseRiscoSelecionada = obj as GenericComboResult;

            // Equivalente a -setClasseRiscoSelecionada: (EdicaoAtendimentoViewController, PegadaIOS).
            PesquisarCliente();

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async void SetStatusSelecionado(object obj)
        {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            StatusSelecionado = obj as GenericComboResult;

            // Equivalente a -setStatusSelecionado: (EdicaoAtendimentoViewController, PegadaIOS).
            PesquisarCliente();

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async Task CarregaCidadePorUF(GenericComboResult ufSelecionada) {

            ListaCidade = await _clienteRepository.BuscarMunicipios(ufSelecionada.Codigo);
        }
        //####################################################

        private async void SelecionarTipoAtendimento(object obj)
        {
            var genricCombo = new List<GenericComboResult> { new GenericComboResult { Codigo = "1", Descricao = "Cliente" }, new GenericComboResult { Codigo = "2", Descricao = "Grupo" } };
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(genricCombo), SetUFSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
        }


        private async void PrepSelecionado(object obj) {

            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            var genericPreposto = obj as GenericComboResult;

            PrepostoSelecionado = new PessoaCommandResult();
            PrepostoSelecionado.CodPessoa = genericPreposto.Codigo;
            PrepostoSelecionado.Nome = genericPreposto.Descricao;

            RaisePropertyChanged("PrepostoSelecionado");
            await PopupNavigation.Instance.PopAsync();
        }

        private async void TipoAtendimentoSelecionado(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            TipoAtendimento = obj as GenericComboResult;

            RaisePropertyChanged("TipoAtendimento");
            await PopupNavigation.Instance.PopAsync();
        }


        private async Task SelecaoTabelaPrecoFromInit()
        {
            var tabelaSelecionada = await _atendimentoUtility.SelecionarTabelaPrecoInit(ClienteSelecionado.CodPessoaCliente);
            CarrinhoFechamento.TabelaPreco = tabelaSelecionada.Descricao;
            CarrinhoFechamento.CodTabelaPreco = tabelaSelecionada.Codigo;
        }


        // Equivalente a "touchInfoBtn"/segue "InfoCliente" (EdicaoAtendimentoViewController, PegadaIOS).
        private async void VisualizarInfoCliente(object obj)
        {
            try
            {
                var cliente = obj as ClienteCommandResult;
                if (cliente == null)
                    return;

                cliente.EnderecoPrincipal = await _clienteRepository.BuscarEnderecoPrincipal(cliente.CodPessoaCliente);
                cliente.EnderecoCobranca = await _clienteRepository.BuscarEnderecoCobranca(cliente.CodPessoaCliente);
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupInfoCliente(cliente));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        // Equivalente ao botão "Novo Cliente" da toolbar (EdicaoAtendimentoViewController, PegadaIOS).
        // Fecha o popup de Novo Atendimento antes de abrir o cadastro: FormCadastroClienteView é aberto via
        // PushModalAsync (pilha padrão do Xamarin.Forms), que fica visualmente ABAIXO da camada do
        // Rg.Plugins.Popup - com o popup ainda aberto, a tela de cadastro nasce atrás dele. Mesmo padrão já
        // usado em SelecaoClienteViewModel.NovoCliente. Ao fechar o cadastro (cancelar ou salvar), reabre o
        // Novo Atendimento - como a tela é recriada do zero, ela já carrega a lista de clientes atualizada
        // do banco, então um cliente recém-cadastrado já aparece nela sem passo extra.
        private async Task NovoCliente()
        {
            await PopupNavigation.Instance.PopAsync();
            await RgPopupUtility.AbrirCadastroCliente(ReabrirNovoAtendimento);
        }

        private async void ReabrirNovoAtendimento()
        {
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupAtendimento());
        }

        // Equivalente a "touchTransmitirClientes" (EdicaoAtendimentoViewController, PegadaIOS): fora do modo
        // de seleção, transmite os cadastros de clientes novos pendentes; dentro do modo "Atualizar Cadastro",
        // atualiza a situação comercial dos clientes marcados.
        private async Task TransmitirClientes()
        {
            if (ModoAtualizacaoCadastro)
            {
                await AtualizarSituacaoComercialSelecionados();
                return;
            }

            try
            {
                var confirm = await UserDialogs.Instance.ConfirmAsync("Esta ação irá transmitir todos os cadastros de clientes novos, deseja prosseguir?", "Transmitir", "Sim", "Não");
                if (!confirm)
                    return;

                var listClientes = await _clienteRepository.BuscarClientesTransmitir(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, null, Session.USUARIO_LOGADO.CodTipoPessoa, FiltroPesquisa)).ConfigureAwait(false);

                if (listClientes.Count > 0)
                {
                    UserDialogs.Instance.ShowLoading("Transmitindo o cadastros...");
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

                    await UserDialogs.Instance.AlertAsync("Cadastros enviados com sucesso!", AppName);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("Nenhum cadastro encontrado para transmissão.", AppName);
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        // Equivalente a "touchAtualizarCancelarSituacaoCadastroClientes" (EdicaoAtendimentoViewController,
        // PegadaIOS): liga/desliga o modo de seleção com checkbox na lista.
        private void AlternarModoAtualizacaoCadastro()
        {
            ModoAtualizacaoCadastro = !ModoAtualizacaoCadastro;

            if (!ModoAtualizacaoCadastro)
                LimparSelecaoClientes();

            RaisePropertyChanged(nameof(TextoBotaoAtualizarCadastro));
            RaisePropertyChanged(nameof(TextoBotaoTransmitir));
        }

        // Equivalente a "limpaSelecao" (EdicaoAtendimentoViewController, PegadaIOS).
        private void LimparSelecaoClientes()
        {
            foreach (var cliente in Clientes)
                cliente.UiChecked = false;

            if (ListaClientesOriginal != null)
                foreach (var cliente in ListaClientesOriginal)
                    cliente.UiChecked = false;
        }

        // Equivalente a "cell:didCheckEdicaoAtendimento:" (EdicaoAtendimentoTableViewCell, PegadaIOS).
        private void MarcarCliente(object obj)
        {
            if (obj is ClienteCommandResult cliente)
                cliente.UiChecked = !cliente.UiChecked;
        }

        // Equivalente ao ramo "_atualizaClienteCheked == YES" de "touchTransmitirClientes"
        // (EdicaoAtendimentoViewController, PegadaIOS): atualiza a situação comercial dos clientes marcados
        // via webservice e grava o retorno no banco local.
        private async Task AtualizarSituacaoComercialSelecionados()
        {
            try
            {
                var selecionados = Clientes.Where(c => c.UiChecked).ToList();
                if (selecionados.Count == 0)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione pelo menos um cliente para atualizar a situação comercial.", "Atualização de situação comercial");
                    return;
                }

                UserDialogs.Instance.ShowLoading("Atualizando situação comercial...");

                var resultado = await ServiceUtility.AtualizarSituacaoComercialClientes(_parametroSincronizacaRepository, selecionados.Select(c => c.CodPessoaCliente).ToList());

                bool sucesso = resultado != null && resultado.Any(item =>
                    item.TryGetValue("SUCCESS", out var valor) && string.Equals(valor?.ToString(), "TRUE", StringComparison.OrdinalIgnoreCase));

                UserDialogs.Instance.HideLoading();

                if (sucesso)
                {
                    foreach (var item in resultado)
                    {
                        if (!item.TryGetValue("CODPESSOACLIENTE", out var codObj) || codObj == null)
                            continue;

                        int.TryParse(item.TryGetValue("CREDITOLIBERADO", out var cred) ? cred?.ToString() : null, out int creditoLiberado);
                        int.TryParse(item.TryGetValue("INDATIVO", out var ativo) ? ativo?.ToString() : null, out int indAtivo);
                        int.TryParse(item.TryGetValue("USACLASSERISCO", out var usa) ? usa?.ToString() : null, out int usaClasseRisco);
                        string codSituacaoCliente = item.TryGetValue("CODSITUACAOCLIENTE", out var sit) ? sit?.ToString() : null;
                        string codClasseRisco = item.TryGetValue("CODCLASSERISCO", out var classe) ? classe?.ToString() : null;

                        await _clienteRepository.AtualizarSituacaoComercialCliente(codObj.ToString(), codSituacaoCliente, creditoLiberado, indAtivo, codClasseRisco, usaClasseRisco);
                    }

                    await UserDialogs.Instance.AlertAsync("Atualização realizada com sucesso.", "Atualização de situação comercial");

                    ModoAtualizacaoCadastro = false;
                    RaisePropertyChanged(nameof(TextoBotaoAtualizarCadastro));
                    RaisePropertyChanged(nameof(TextoBotaoTransmitir));
                    LimparSelecaoClientes();

                    PesquisarCliente();
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("Ocorreu um erro ao realizar a atualização.", "Atualização de situação comercial");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void SalvarAtendimento(object obj)
        {
            try
            {

                if (ClienteSelecionado == null || ClienteSelecionado.CodPessoaCliente == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione um cliente para abrir o atendimento.");
                    return;
                }

                if (CarrinhoFechamento.CodTabelaPreco == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione uma Tabela de Preço para abrir o atendimento.");
                    return;
                }

                var podeFazerPedido = ClienteSelecionado.IndAtivo == 1 && ClienteSelecionado.IdExternoSituacao == 1 && ClienteSelecionado.CreditoLiberado == 1 &&
                                      (ClienteSelecionado.UsaClasseRisco == 0 || (ClienteSelecionado.UsaClasseRisco  == 1 && ClienteSelecionado.PermiteDigitacao == 1));


                if (!podeFazerPedido)
                {
                    await UserDialogs.Instance.AlertAsync("O Cliente selecionado está bloqueado para digitar pedido!");
                    return;
                }

                //########################################################

                //########################################################

                decimal markupCliente = ClienteSelecionado.Markup;
                if (ClienteSelecionado.PermiteAjustePreco == 1 && ClienteSelecionado.Markup == 0) {
                    markupCliente = 1;
                }

                CriarAtendimentoCommand command = new CriarAtendimentoCommand()
                {

                    CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
                    CodUsuario = Session.USUARIO_LOGADO.CodUsuario,
                    CodMarca = Session.USUARIO_LOGADO.CodMarca,
                    CodInstalacao = Session.USUARIO_LOGADO.CodInstalacao,
                    Descricao = ClienteSelecionado.RazaoSocial,
                    ConfiguracaoAtendimento = ClienteSelecionado.EnderecoPrincipalCompleto,
                    IndAberto = 1,
                    CodTabelaPreco = CarrinhoFechamento.CodTabelaPreco,
                    PrazoMedio = CarrinhoFechamento.PrazoMedio,
                    CodCondicaoPagamento = CarrinhoFechamento.CodCondicaoPagamento,
                    PercentualDesconto1 = CarrinhoFechamento.PercentualDesconto,
                    Controle = CarrinhoFechamento.Controle,
                    TipoPedido = CarrinhoFechamento.CodTipoPedido,
                    Markup = markupCliente,
                    IndAplicaMarkup = ClienteSelecionado.PermiteAjustePreco == 1 ? 1 : 0
                };

                Session.ATENDIMENTO_ATUAL = await _atendimentoUtility.CriarAtendimento(command);
                Session.ATENDIMENTO_ATUAL.Markup = Session.MarkupPadrao;

                await _atendimentoRepository.InativarAtendimentoAberto(Session.ATENDIMENTO_ATUAL.CodAtendimento);

                MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
                await PopupNavigation.Instance.PopAllAsync();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"{ex.Message}");
            }
        }

        private async void FecharPopupAtendimento(object obj)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
        #endregion

        public void SetCarrinho(string codCarrinho)
        {

        }

        private List<GenericComboResult> ObterUFs()
        {
            return new List<GenericComboResult>
            {
                new GenericComboResult { Codigo = "-1", Descricao = "Todos" },
                new GenericComboResult { Codigo = "AC", Descricao = "AC" },
                new GenericComboResult { Codigo = "AL", Descricao = "AL" },
                new GenericComboResult { Codigo = "AM", Descricao = "AM" },
                new GenericComboResult { Codigo = "AP", Descricao = "AP" },
                new GenericComboResult { Codigo = "BA", Descricao = "BA" },
                new GenericComboResult { Codigo = "CE", Descricao = "CE" },
                new GenericComboResult { Codigo = "DF", Descricao = "DF" },
                new GenericComboResult { Codigo = "ES", Descricao = "ES" },
                new GenericComboResult { Codigo = "GO", Descricao = "GO" },
                new GenericComboResult { Codigo = "MA", Descricao = "MA" },
                new GenericComboResult { Codigo = "MG", Descricao = "MG" },
                new GenericComboResult { Codigo = "MS", Descricao = "MS" },
                new GenericComboResult { Codigo = "MT", Descricao = "MT" },
                new GenericComboResult { Codigo = "PA", Descricao = "PA" },
                new GenericComboResult { Codigo = "PB", Descricao = "PB" },
                new GenericComboResult { Codigo = "PE", Descricao = "PE" },
                new GenericComboResult { Codigo = "PI", Descricao = "PI" },
                new GenericComboResult { Codigo = "PR", Descricao = "PR" },
                new GenericComboResult { Codigo = "RJ", Descricao = "RJ" },
                new GenericComboResult { Codigo = "RN", Descricao = "RN" },
                new GenericComboResult { Codigo = "RO", Descricao = "RO" },
                new GenericComboResult { Codigo = "RR", Descricao = "RR" },
                new GenericComboResult { Codigo = "RS", Descricao = "RS" },
                new GenericComboResult { Codigo = "SC", Descricao = "SC" },
                new GenericComboResult { Codigo = "SE", Descricao = "SE" },
                new GenericComboResult { Codigo = "SP", Descricao = "SP" },
                new GenericComboResult { Codigo = "TO", Descricao = "TO" }
            };
        }

        private List<GenericComboResult> ObterStatus()
        {
            return new List<GenericComboResult>
            {
                new GenericComboResult { Codigo = "-1", Descricao = "Todos" },
                new GenericComboResult { Codigo = "Ativo", Descricao = "Ativo" },
                new GenericComboResult { Codigo = "Inativo", Descricao = "Inativo" }
            };
        }
    }
}