using Acr.UserDialogs;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Infra.Repositories;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static Dropbox.Api.TeamLog.SharingMemberPolicy;

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

        #endregion

        #region "Repositorios"
        private readonly IClienteRepository _clienteRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly ITabelaPrecoRepository _tabelaPrecoRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly IPessoaRepository _pessoaRepository;
        private readonly AtendimentoUtility _atendimentoUtility;
        #endregion


        #region "Commands"
        public ICommand SalvarAtendimentoCommand { get; set; }
        public ICommand CancelarAtendimentoCommand { get; set; }
        public ICommand TipoVendaVendorCommand { get; set; }
        public ICommand SelecionarClienteCommand { get; set; }
        public ICommand SelecionarPrepostoCommand { get; set; }
        public ICommand SelecionarTipoAtendimentoCommand { get; set; }
        public ICommand InfoCommand { get; set; }
        public ICommand SelecionarTabelaPrecoCommand { get; set; }
        public ICommand SelecionarCondicaoPagamentoCommand { get; set; }
        public ICommand SelecionarTipoPedidoCommand { get; set; }
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
            IPessoaRepository pessoaRepository
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

            SalvarAtendimentoCommand = new DelegateCommand<object>(SalvarAtendimento);
            CancelarAtendimentoCommand = new DelegateCommand<object>(FecharPopupAtendimento);
            InfoCommand = new Command(VisualizarInfoCliente);
            SelecionarCondicaoPagamentoCommand = new Command(async () => await _atendimentoUtility.SelecionarCondicaoPagamento());
            //SelecionarTabelaPrecoCommand = new Command(async () => await _atendimentoUtility.SelecionarTabelaPreco(true));
            SelecionarTabelaPrecoCommand = new Command(async () => await _atendimentoUtility.SelecionarTabelaPrecoCliente(ClienteSelecionado?.CodPessoaCliente));
            SelecionarTipoPedidoCommand = new Command(async () => await _atendimentoUtility.SelecionarTipoPedido());
            SelecionarClienteCommand = new Command(SelecionarCliente);
            SelecionarPrepostoCommand = new Command(SelecionarPreposto);
            SelecionarTipoAtendimentoCommand = new Command(SelecionarTipoAtendimento);

            Init();
        }
        #endregion

        #region "Metodos"
        private async void Init()
        {
            try
            {
                await CarregaPrepostos();

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
                //self.prepostos = [NSArray arrayWithObject:[[IMPlaceholderWithTitleInUI alloc] initWithTitle:[Language getStringFromKey: @"Global_Todos"]]];
                //self.prepostos = [MarcaCliente prepostoForRepresentante: _pessoa.code];
                //self.prepostoSelecionado = [self.prepostos objectAtIndex: 0];
            }
        }

        private async void SelecionarPreposto(object obj) {
            var genricCombo = ListaPreposto.Select(x => new GenericComboResult() { Codigo = x.CodPessoa, Descricao = x.Nome }).ToList();
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(genricCombo), PrepSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
        }

        private async void SelecionarTipoAtendimento(object obj)
        {
            var genricCombo = new List<GenericComboResult> { new GenericComboResult { Codigo = "1", Descricao = "Cliente" }, new GenericComboResult { Codigo = "2", Descricao = "Grupo" } };
            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(genricCombo), TipoAtendimentoSelecionado, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
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
            IsLicenciamento = "";
            try
            {
                ClienteSelecionado = obj as ClienteCommandResult;

                if (ClienteSelecionado.CodSituacaoCliente == "50" && LiberaClienteNovo != "S")
                {
                    ClienteSelecionado = new ClienteCommandResult();
                    await UserDialogs.Instance.AlertAsync("Cliente bloqueado para venda.");
                    return;
                }

                if (ClienteSelecionado.CodSituacaoCliente == "2")
                {
                    ClienteSelecionado = new ClienteCommandResult();
                    await UserDialogs.Instance.AlertAsync("Selecione cliente valido.");
                    return;
                }

               await SelecaoTabelaPrecoFromInit();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }

            await PopupNavigation.Instance.PopAsync();
        }

        private async Task SelecaoTabelaPrecoFromInit()
        {
            var tabelaSelecionada = await _atendimentoUtility.SelecionarTabelaPrecoInit(ClienteSelecionado.CodPessoaCliente);
            CarrinhoFechamento.TabelaPreco = tabelaSelecionada.Descricao;
            CarrinhoFechamento.CodTabelaPreco = tabelaSelecionada.Codigo;
        }


        private async void VisualizarInfoCliente(object obj)
        {
            //await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupInfoCliente(_clienteSelecionado));
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
                else if (CarrinhoFechamento.CodTabelaPreco == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione uma Tabela de Preço para abrir o atendimento.");
                    return;
                }
                else if (Session.USUARIO_LOGADO.CodTipoPessoa != "1" && CarrinhoFechamento.CodTipoPedido == "5")
                {
                    await UserDialogs.Instance.AlertAsync("Somente gerente pode abrir atendimento do tipo RESERVA!");
                    return;
                }
                else
                {
                    //########################################################
                    if (CarrinhoFechamento.PercentualDesconto > 0)
                    {
                        var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorCliente(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", ClienteSelecionado.CodPessoaCliente, null));

                        if (coefiDesconto != null)
                        {
                            TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = CarrinhoFechamento.CodCondicaoPagamento });
                            string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

                            var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", ClienteSelecionado.CodigoSegmento, null, prazoMedio));

                            decimal coeficiente = coefiDesconto.Coeficiente;
                            if (coefiPrazoMedio != null)
                            {
                                if (coefiPrazoMedio.Coeficiente > 0)
                                {
                                    if (Convert.ToDecimal(prazoMedio) >= 60)
                                    {
                                        coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
                                    }
                                    else
                                    {
                                        coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
                                    }
                                }
                            }
                            var des = CarrinhoFechamento.PercentualDesconto / 100;
                            if (des > coeficiente)
                            {
                                var descontoInteiro = coeficiente != null ? Convert.ToInt32(coeficiente * 100) : coeficiente;
                                var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto Informado é maior que o desconto máximo permitido de {descontoInteiro}% para condição de  {condicaoPagamento.Descricao.Trim()}, seu pedido será enviado para aprovação, deseja continuar?", "Desconto Excedido", "Sim", "Não");
                                if (!desconto)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            await UserDialogs.Instance.AlertAsync("Não foi encontrado um segmento válido no cliente para o desconto. Sincronize ou entre em contato com a Pegada.", AppName, "OK");
                            return;
                        }
                    }
                    //########################################################

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
                    };

                    Session.ATENDIMENTO_ATUAL = await _atendimentoUtility.CriarAtendimento(command);
                    Session.ATENDIMENTO_ATUAL.Markup = Session.MarkupPadrao;
                    MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
                    await PopupNavigation.Instance.PopAllAsync();
                }
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
    }
}