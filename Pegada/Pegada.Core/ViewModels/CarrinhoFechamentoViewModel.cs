using Acr.UserDialogs;
using Pegada.Core.Repositories;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Handlers;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Infra.Repositories;
using MobiliVendas.Core.Shared.Messages;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Pegada.Core.ViewModels
{
    public class CarrinhoFechamentoViewModel : ViewModelBase, ICarrinhoFechamentoViewModel
    {
        private CarrinhoFechamentoCommandResult _carrinhoFechamento;
        public CarrinhoFechamentoCommandResult CarrinhoFechamento
        {
            get { return _carrinhoFechamento; }
            set { SetProperty(ref _carrinhoFechamento, value); }
        }

        #region "Propriedades"

        private bool _isPedidoMae;
        public bool IsPedidoMae
        {
            get { return _isPedidoMae; }
            set { SetProperty(ref _isPedidoMae, value); }
        }

        private bool _isFatAntecipado;
        public bool IsFatAntecipado
        {
            get { return _isFatAntecipado; }
            set { SetProperty(ref _isFatAntecipado, value); }
        }

        private bool _isFatParcial;
        public bool IsFatParcial
        {
            get { return _isFatParcial; }
            set { SetProperty(ref _isFatParcial, value); }
        }

        private bool _mostraFlagPedidoMae;
        public bool MostraFlagPedidoMae
        {
            get { return _mostraFlagPedidoMae; }
            set { SetProperty(ref _mostraFlagPedidoMae, value); }
        }

        private bool _mostraObservacoes;
        public bool MostraObservacoes
        {
            get { return _mostraObservacoes; }
            set { SetProperty(ref _mostraObservacoes, value); }
        }

        private GenericComboResult _condicaoPagamento;
        public GenericComboResult CondicaoPagamento
        {
            get { return _condicaoPagamento; }
            set { SetProperty(ref _condicaoPagamento, value); }
        }

        private bool _transportadoraEstaVisivel;
        public bool TransportadoraEstaVisivel
        {
            get { return _transportadoraEstaVisivel; }
            set { SetProperty(ref _transportadoraEstaVisivel, value); }
        }

        private double _incrementDesconto1;
        public double IncrementDesconto1
        {
            get { return _incrementDesconto1; }
            set { SetProperty(ref _incrementDesconto1, value); }
        }

        private double _percentualDesconto1Max;
        public double PercentualDesconto1Max
        {
            get { return _percentualDesconto1Max; }
            set { SetProperty(ref _percentualDesconto1Max, value); }
        }

        private double _percentualDesconto2;
        public double PercentualDesconto2
        {
            get { return _percentualDesconto2; }
            set { SetProperty(ref _percentualDesconto2, value); }
        }

        private double _percentualDesconto3;
        public double PercentualDesconto3
        {
            get { return _percentualDesconto3; }
            set { SetProperty(ref _percentualDesconto3, value); }
        }

        private string _percentualDesconto4;
        public string PercentualDesconto4
        {
            get { return _percentualDesconto4; }
            set { SetProperty(ref _percentualDesconto4, value); }
        }

        public decimal PercentualMax;

        private string _percentualDesconto5;
        public string PercentualDesconto5
        {
            get { return _percentualDesconto5; }
            set { SetProperty(ref _percentualDesconto5, value); }
        }

        private DateTime? _dataEntrega;
        public DateTime? DataEntrega
        {
            get { return _dataEntrega; }
            set { SetProperty(ref _dataEntrega, value); }
        }

        private string _cifFob;
        public string CifFob
        {
            get { return _cifFob; }
            set { SetProperty(ref _cifFob, value); }
        }

        private GenericComboResult _deposito;
        public GenericComboResult Deposito
        {
            get { return _deposito; }
            set { SetProperty(ref _deposito, value); }
        }

        private GenericComboResult _transportadora;
        public GenericComboResult Transportadora
        {
            get { return _transportadora; }
            set { SetProperty(ref _transportadora, value); }
        }

        private string _ordemCompra;
        public string OrdemCompra
        {
            get { return _ordemCompra; }
            set { SetProperty(ref _ordemCompra, value); }
        }

        private string _observacoes;
        public string Observacoes
        {
            get { return _observacoes; }
            set { SetProperty(ref _observacoes, value); }
        }

        private int _aplicacaoComissao;
        public int AplicacaoComissao
        {
            get { return _aplicacaoComissao; }
            set { SetProperty(ref _aplicacaoComissao, value); }
        }

        private string _observacoesSeparacao;
        public string ObservacoesSeparacao
        {
            get { return _observacoesSeparacao; }
            set { SetProperty(ref _observacoesSeparacao, value); }
        }

        private CarrinhoCommandResult _pedidoSelecionado;
        public CarrinhoCommandResult PedidoSelecionado
        {
            get { return _pedidoSelecionado; }
            set { SetProperty(ref _pedidoSelecionado, value); }
        }

        //private GenericComboResult _tipoPedido;
        //public GenericComboResult TipoPedido
        //{
        //    get { return _tipoPedido ?? new GenericComboResult(); }
        //    set
        //    {
        //        SetProperty(ref _tipoPedido, value);
        //    }
        //}

        private GenericComboResult _tipoPedido;
        public GenericComboResult TipoPedido
        {
            get { return _tipoPedido; }
            set { SetProperty(ref _tipoPedido, value); }
        }

        private decimal _descontoAlterou;
        public decimal DescontoAlterou
        {
            get { return _descontoAlterou; }
            set { SetProperty(ref _descontoAlterou, value); }
        }

        private DateTime _dataMinimaEntrega;
        public DateTime DataMinimaEntrega
        {
            get { return _dataMinimaEntrega; }
            set { SetProperty(ref _dataMinimaEntrega, value); }
        }

        private ClienteCommandResult cliente;
        #endregion

        #region "Commands"
        public ICommand AplicaPedidoMaeCommand { get; set; }
        public ICommand AceitaFatAntCommand { get; set; }
        public ICommand AceitaFatParcialCommand { get; set; }
        public ICommand CancelarFechamentoCommand { get; set; }
        public ICommand SalvarFechamentoCommand { get; set; }
        public ICommand SelecionarCondicaoPagamentoCommand { get; set; }
        public ICommand SelecionarDataEntregaCommand { get; set; }
        public ICommand SelecionarDataLimiteCommand { get; set; }
        public ICommand SelecionarTipoFreteCommand { get; set; }
        public ICommand SelecionarTransportadoraCommand { get; set; }
        public ICommand SelecionarDepositoCommand { get; set; }
        public ICommand SelecionarTipoPedidoCommand { get; set; }
        #endregion

        #region "Repositorios"
        private CarrinhoCommandHandler _carrinhoCommandHandler;
        private ICarrinhoRepository _carrinhoRepository;
        private DataBaseRepository _dataBaseRepository;
        private readonly ICondicaoPagamentoRepository _condicaoPagamentoRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly ITransportadoraRepository _transportadoraRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ITipoPedidoRepository _tipoPedidoRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly IPoliticaComercialRepository _politicaComercialRepository;
        private readonly INivelRepository _nivelRepository;
        private readonly ICoeficienteRepository _coeficienteRepository;
        #endregion

        #region "Construtores"
        public CarrinhoFechamentoViewModel(CarrinhoCommandHandler carrinhoCommandHandler, ICondicaoPagamentoRepository condicaoPagamentoRepository,
                    ICarrinhoRepository carrinhoRepository,
                    ITipoPedidoRepository tipoPedidoRepository
                    , DataBaseRepository dataBaseRepository
                    , IParametroRepository parametroRepository
                    , ITransportadoraRepository transportadoraRepository
                    , IProdutoRepository produtoRepository
                    , IClienteRepository clienteRepository
                    , IAtendimentoRepository atendimentoRepository
                    , IPoliticaComercialRepository politicaComercialRepository
                    , INivelRepository nivelRepository
                    , ICoeficienteRepository coeficienteRepository
                    )
                   : base(null, null)
        {
            _atendimentoRepository = atendimentoRepository;
            _carrinhoCommandHandler = carrinhoCommandHandler;
            _carrinhoRepository = carrinhoRepository;
            _tipoPedidoRepository = tipoPedidoRepository;
            _condicaoPagamentoRepository = condicaoPagamentoRepository;
            _dataBaseRepository = dataBaseRepository;
            _parametroRepository = parametroRepository;
            _transportadoraRepository = transportadoraRepository;
            _produtoRepository = produtoRepository;
            _clienteRepository = clienteRepository;
            _politicaComercialRepository = politicaComercialRepository;
            _nivelRepository = nivelRepository;
            _coeficienteRepository = coeficienteRepository;
            CancelarFechamentoCommand = new Command(CancelarFechamento);
            SalvarFechamentoCommand = new Command(SalvarFechamento);
            SelecionarDataEntregaCommand = new Command(SelecionarDataEntrega);
            SelecionarTipoFreteCommand = new Command(SelecionarTipoFrete);
            SelecionarTransportadoraCommand = new Command(SelecionarTransportadora);
            SelecionarCondicaoPagamentoCommand = new Command(SelecionarCondicaoPagamento);
            SelecionarDepositoCommand = new Command(SelecionarDeposito);
            SelecionarTipoPedidoCommand = new Command(SelecionarTipoPedido);
            CarrinhoFechamento = new CarrinhoFechamentoCommandResult();
            buscaDescontoFrenteCliente();

            AplicaPedidoMaeCommand = new Command(ChangeIsPedidoMae);
            IsPedidoMae = false;

            AceitaFatAntCommand = new Command(ChangeIsAceitaFatAntecipado);
            IsFatAntecipado = true;

            AceitaFatParcialCommand = new Command(ChangeIsAceitaFatParcial);
            IsFatParcial = true;

            CifFob = "CIF";

        }
        #endregion

        #region "Metodos"
        public async void buscaDescontoFrenteCliente()
        {
            PercentualDesconto1Max = 0;
            IncrementDesconto1 = 10;
            cliente = await _clienteRepository.BuscarClientePorCode(new BuscarClienteCommand(null, null, Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

            if (cliente.PercDescFrete != null)
            {
                if (cliente.PercDescFrete <= 10)
                {
                    IncrementDesconto1 = 10 - (double)cliente.PercDescFrete;
                }
            }
        }

        private async void ChangeKeyIsVisibleObs() {
            MostraObservacoes = true;
        }

        private async void ChangeIsPedidoMae()
        {
            IsPedidoMae = !IsPedidoMae;
        }

        private async void ChangeIsAceitaFatAntecipado()
        {
            IsFatAntecipado = !IsFatAntecipado;
        }

        private async void ChangeIsAceitaFatParcial()
        {
            IsFatParcial = !IsFatParcial;
        }

        public async void SelecionarCondicaoPagamento()
        {
            try
            {
                var command = new BuscarCondicaoPagamentoCommand()
                {
                    PrazoMedio = -1
                };
                var condicoes = await _condicaoPagamentoRepository.BuscarCondicoesParaFechamento(command);
                //Projeto #22617
                //[...] "condição funcionou nos testes, mas o Layout no Ipad não está agradável a Leitura.
                // Sugestão de Aumentar a largura do box."
                await PopupNavigation.Instance.PushAsync(
                    RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(condicoes),
                    CondicaoPagamentoSelecionada,
                    new Rectangle(0.5, 0.5, 0.5, 0.5), true, true, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }

        public async void SelecionarTipoPedido()
        {
            try
            {
                var tiposPedido = await _tipoPedidoRepository.BuscarTiposPedido();
                await PopupNavigation.Instance.PushAsync(
                    RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(tiposPedido),
                    TipoPedidoSelecionado,
                    new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }

        private async void TipoPedidoSelecionado(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            TipoPedido = obj as GenericComboResult;

            MostraObservacoes = false;
            if (TipoPedido.Codigo != "1") {
                ChangeKeyIsVisibleObs();
            }

            CarrinhoFechamento.CodTipoPedido = (obj as GenericComboResult).Codigo;
            CarrinhoFechamento.TipoPedido = (obj as GenericComboResult).Descricao;

            PedidoSelecionado.TipoPedido = (obj as GenericComboResult).Descricao;
            PedidoSelecionado.CodTipoPedido = (obj as GenericComboResult).Codigo;
            await PopupNavigation.Instance.PopAsync();
        }

        private async void CondicaoPagamentoSelecionada(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            CondicaoPagamento = obj as GenericComboResult;


            var conCommand = new BuscarCondicaoPagamentoCommand()
            {
                CodCondicaoPagamento = CondicaoPagamento.Codigo
            };


            var condicaPgto =
                await _condicaoPagamentoRepository.BuscarCondicaoPagamento(conCommand);

            PercentualMax = condicaPgto.Desconto;

            await PopupNavigation.Instance.PopAsync();
        }

        public async void SelecionarDataEntrega()
        {
            var data = await UserDialogs.Instance.DatePromptAsync(new DatePromptConfig() { MinimumDate = DataMinimaEntrega != null ? DataMinimaEntrega : DateTime.Now.AddDays(1) });
            if (data.Ok)
            {

                DateTime dateErrada = new DateTime(0001, 1, 1, 0, 0, 0);
                int result = DateTime.Compare(data.Value, dateErrada);
                if (result == 0)
                {
                    DataEntrega = DateTime.Now.AddDays(1);
                }
                else
                {
                    DataEntrega = data.Value;
                }
            }

        }

        public async void SelecionarTipoFrete()
        {
            var result = await UserDialogs.Instance.ActionSheetAsync("Selecione o tipo de frete", "Cancelar", null, null, "CIF", "FOB");
            if (result != "Cancelar")
            {
                CifFob = result;
                TransportadoraEstaVisivel = this.CifFob == "FOB";
            }
            if (!TransportadoraEstaVisivel)
            {
                buscaDescontoFrenteCliente();
            }
            else
            {
                IncrementDesconto1 = 10;
                PercentualDesconto1Max = 0;
            }
        }

        public async void SelecionarDeposito()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Carregando dados");
                var command = new BuscarCDProdutoCommand() { CodProduto = null, CodPessoa = Session.USUARIO_LOGADO.CodPessoa };
                var dados = await _produtoRepository.BuscarCDProduto(command);
                UserDialogs.Instance.HideLoading();
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(dados), DepositoSelecionada, new Rectangle(0.5, 0.5, 0.25, 0.25), false, false, false));
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void DepositoSelecionada(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            Deposito = obj as GenericComboResult;

            await PopupNavigation.Instance.PopAsync();
        }

        public async void SelecionarTransportadora()
        {
            try
            {
                var dados = await _transportadoraRepository.BuscarTransportadoras(new BuscarTransportadoraCommand());
                await PopupNavigation.Instance.PushAsync(
                      RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(dados),
                      TransportadoraSelecionada,
                      new Rectangle(0.5, 0.5, 0.4, 0.3), true, false, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void TransportadoraSelecionada(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }
            Transportadora = obj as GenericComboResult;
            await PopupNavigation.Instance.PopAsync();
        }

        public async void Init()
        {
            try
            {
                //TAREFA 30891
                DateTime dataMinima = await _parametroRepository.BuscarDataMinimaPedido(PedidoSelecionado.CodCarrinho);
                DataMinimaEntrega = dataMinima;

                string liberaImportacao = await _parametroRepository.BuscarValorParametro(ParametrosSistema.LIBERA);
                string usuarioLiberadoTeste = await _parametroRepository.BuscarValorParametro(ParametrosSistema.USUARIOPARAMETRO);

                if (liberaImportacao == "N" && Session.USUARIO_LOGADO.CodPessoa == usuarioLiberadoTeste)
                {
                    MostraFlagPedidoMae = true;
                    if (PedidoSelecionado.PedidoMae != null)
                    {
                        MostraFlagPedidoMae = false;
                    }
                }

                if (liberaImportacao == "S")
                {
                    MostraFlagPedidoMae = true;
                    if (PedidoSelecionado.PedidoMae != null)
                    {
                        MostraFlagPedidoMae = false;
                    }
                }

                DescontoAlterou = Convert.ToDecimal(PedidoSelecionado.PercentualDesconto);

                List<string> camposFora = new List<string>();
                var camposWhere = new List<TableInfo> { new TableInfo("CodCarrinho", PedidoSelecionado.CodCarrinho) };
                var dados = await _dataBaseRepository.BuscarDadosTabela("TBT_CARRINHO", camposWhere);

                var properties = this.GetType().GetRuntimeProperties();
                foreach (var item in dados)
                {
                    var prop = properties.Where(x => x.Name == item.ColumnName).FirstOrDefault();
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(System.DateTime?))
                        {
                            DateTime data;
                            if (DateTime.TryParse(item.ColumnValue, out data))
                            {
                                prop.SetValue(this, data);
                            }
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(this, item.ColumnValue);
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            prop.SetValue(this, item.ColumnValue == "1");
                        }
                        //Defeito #23179 - apresentar os descontos
                        else if (prop.PropertyType == typeof(double))
                        {
                            prop.SetValue(this, Convert.ToDouble(item.ColumnValue));
                        }
                    }
                    else
                    {
                        camposFora.Add(item.ColumnName);
                        if (item.ColumnName == "CodCondicaoPagamento")
                        {
                            var conCommand = new BuscarCondicaoPagamentoCommand()
                            {
                                CodCondicaoPagamento = item.ColumnValue,
                                PrazoMedio = -1
                            };
                            CondicaoPagamento = (await _condicaoPagamentoRepository.BuscarCondicaoPagamento(conCommand));

                            var condicaPgto =
                                await _condicaoPagamentoRepository.BuscarCondicaoPagamento(conCommand);

                            if (condicaPgto != null) PercentualMax = condicaPgto.Desconto;
                        }
                        else if (item.ColumnName == "CodTransportadora")
                        {
                            Transportadora = (await _transportadoraRepository.BuscarTransportadoras(new BuscarTransportadoraCommand() { CodTransportadora = item.ColumnValue })).FirstOrDefault();
                        }
                        else if (item.ColumnName == "CodDeposito")
                        {
                            //Deposito = (await _produtoRepository.BuscarCDProduto(new BuscarCDProdutoCommand() { CodDeposito = item.ColumnValue, CodPessoa = Session.USUARIO_LOGADO.CodPessoa })).FirstOrDefault();
                            //Defeito #23178 No momento so trazer MAXLOG NAVEGANTES
                            Deposito = (await _produtoRepository.BuscarCDProduto(new BuscarCDProdutoCommand() { CodDeposito = "MAXLOG NAVEGANTES", CodPessoa = Session.USUARIO_LOGADO.CodPessoa })).FirstOrDefault();

                        }
                        else if (item.ColumnName == "PercentualDesconto1")
                        {
                            PercentualDesconto1Max = (Double)PedidoSelecionado.PercentualDesconto1;
                        }
                        else if (item.ColumnName == "IndPedidoMae")
                        {
                            IsPedidoMae = item.ColumnValue == "1" ? true : false;
                        }
                        else if (item.ColumnName == "AceitaFaturamentoAntecipado")
                        {
                            if (item.ColumnValue == null) {
                                IsFatAntecipado = true;
                            }
                            else {
                                IsFatAntecipado = item.ColumnValue == "1" ? true : false;
                            }
                        }
                        else if (item.ColumnName == "AceitaFaturamentoParcial")
                        {
                            if (item.ColumnValue == null)
                            {
                                IsFatParcial = true;
                            }
                            else
                            {
                                IsFatParcial = item.ColumnValue == "1" ? true : false;
                            }
                        }
                        else if (item.ColumnName == "CodTipoPedido")
                        {
                            MostraObservacoes = false;
                            if (item.ColumnValue != "1")
                            {
                                ChangeKeyIsVisibleObs();
                            }
                        }
                    }
                }

                AplicacaoComissao = PedidoSelecionado.PercentualComissaoRep == null ? 100 : Convert.ToInt32(PedidoSelecionado.PercentualComissaoRep);
                this.CifFob = this.CifFob == "F" ? "FOB" : "CIF";
                if (CifFob == "FOB")
                {
                    TransportadoraEstaVisivel = true;
                }
                if (!string.IsNullOrEmpty(PercentualDesconto4))
                {
                    PercentualDesconto4 = PercentualDesconto4.Replace(".", ",");
                }

                if (!string.IsNullOrEmpty(PercentualDesconto5))
                {
                    PercentualDesconto5 = PercentualDesconto5.Replace(".", ",");
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void CancelarFechamento()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void SalvarFechamento()
        {
            try
            {
                decimal percDesc5 = 0;
                decimal percDesc4 = 0;
                if (!string.IsNullOrEmpty(PercentualDesconto5))
                    decimal.TryParse(PercentualDesconto5.Replace(",", "."), NumberStyles.Any, new CultureInfo("en-US"), out percDesc5);

                if (!string.IsNullOrEmpty(PercentualDesconto4))
                    decimal.TryParse(PercentualDesconto4.Replace(",", "."), NumberStyles.Any, new CultureInfo("en-US"), out percDesc4);

                if (!DataEntrega.HasValue)
                {
                    await UserDialogs.Instance.AlertAsync("A data de entrega é obrigatoria.");
                    return;
                }

                if (AplicacaoComissao > 100)
                {
                    await UserDialogs.Instance.AlertAsync("A comissão não deve ser maior que 100.");
                    return;
                }

                if (DataEntrega.Value < DateTime.Now)
                {
                    await UserDialogs.Instance.AlertAsync("A data de entrega não pode ser inferior a hoje.");
                    return;
                }

                if (PedidoSelecionado.CodTipoPedido == "1")
                {
                    this.Observacoes = null;
                }

                //Regra Politica Escolar
                var dataHoje = DateTime.Now.Date.ToString("yyyy-MM-ddTHH:mm:ss");
                //busca as politicas vigentes
                var dataPoliticaEscolar = await _politicaComercialRepository.BuscarDataPoliticaEscolar(dataHoje);

                //verifica se existe alguma vigente
                if (dataPoliticaEscolar.Count > 0)
                {

                    //dentre as vigentes, verificar se escolheu é MiniVA(Mini Voltas Aulas).
                    var politicaMiniVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento == CondicaoPagamento.Codigo && x.CodCondicaoPagamento == "MV3").FirstOrDefault();

                    //se escolheu
                    if (politicaMiniVA != null)
                    {
                        //valida se o carrinho esta certo
                        var validaItensMiniVA = await _carrinhoRepository.GetItensPoliticaMiniVACarrinho(PedidoSelecionado.CodCarrinho);
                        //se não tiver, informa
                        if (!validaItensMiniVA)
                        {
                            await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
                            return;
                        }
                    }

                    //dentre as vigentes, verificar se escolheu é VA(Voltas Aulas).
                    var politicaVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento.Contains(CondicaoPagamento.Codigo)).FirstOrDefault();

                    //se escolheu
                    if (politicaVA != null)
                    {
                        //valida se o carrinho esta certo
                        var validaItensVA = await _carrinhoRepository.GetItensPoliticaVACarrinho(PedidoSelecionado.CodCarrinho);
                        //se não tiver, informa
                        if (!validaItensVA)
                        {
                            await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
                            return;
                        }
                    }


                    //bool temCondicao = false;
                    //foreach (var politica in dataPoliticaEscolar) {
                    //    if (politica.CodCondicaoPagamento == CondicaoPagamento.Codigo) {
                    //        temCondicao = true;
                    //        break;
                    //    }
                    //}

                    //if (temCondicao)
                    //{
                    //    if (!existeItemPolitica)
                    //    {
                    //        await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
                    //        return;
                    //    }
                    //}
                    //else {

                    //    if (existeItemPolitica) {

                    //        var resposta = await UserDialogs.Instance.ConfirmAsync("O período de digitação da Política Escolar está vigente e você não selecionou a condição de pagamento da política. Deseja continuar?", null, "Sim", "Não");

                    //        if (!resposta)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                }

                //########################################################

                PedidoSelecionado.JustificativaDesconto = await _carrinhoRepository.GetJustificarivaDesconto(PedidoSelecionado);
                if (PedidoSelecionado.PercentualDesconto > 0 && string.IsNullOrEmpty(PedidoSelecionado.JustificativaDesconto))
                {
                    var clienteAtendimento = await _clienteRepository.BuscarClientePorCode(new BuscarClienteCommand(null, null, Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

                    var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorCliente(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

                    if (coefiDesconto != null)
                    {
                        TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = CondicaoPagamento?.Codigo });
                        string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

                        var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", clienteAtendimento.CodigoSegmento, null, prazoMedio));

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
                        var des = PedidoSelecionado.PercentualDesconto / 100;
                        if (des > coeficiente)
                        {
                            var descontoInteiro = coeficiente != null ? Convert.ToInt32(coeficiente * 100) : coeficiente;
                            var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto Informado é maior que o desconto máximo permitido de {descontoInteiro}% para condição de  {condicaoPagamento.Descricao.Trim()}, seu pedido será enviado para aprovação, deseja continuar?", "Desconto Excedido", "Sim", "Não");
                            if (!desconto)
                            {
                                return;
                            }
                            else
                            {
                                //abrir popup
                                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupDescontoJustificativa("Carrinho", PedidoSelecionado));
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

                //if (dataPoliticaEscolar.Count > 0 && existeItemPolitica)
                //{
                //    foreach (var politica in dataPoliticaEscolar) {

                //        foreach (var item in PedidoSelecionado.Itens)
                //        {
                //            var detalhesDoProduto = await _nivelRepository.GetNiveisProduto(item.CodProduto);
                //            var categoriaDoProduto = detalhesDoProduto[1].CodAtributo;
                //            var infoPoliticaComercialNiveis = await _politicaComercialRepository.BuscarPoliticaEscolarNiveis(categoriaDoProduto, politica.CodPoliticaComercial);

                //            if (infoPoliticaComercialNiveis.Count > 0)
                //            {
                //                string condPagamentoPoliticaEscolar = politica.CodCondicaoPagamento;
                //                var condPagamento = condPagamentoPoliticaEscolar.Split(',');
                //                bool temCondicao = false;

                //                foreach (string cond in condPagamento)
                //                {
                //                    temCondicao = CondicaoPagamento.Codigo.Equals(cond);
                //                    if (temCondicao)
                //                        break;
                //                }

                //                if (!temCondicao)
                //                {
                //                    var resposta = await UserDialogs.Instance.ConfirmAsync("O período de digitação da Política Escolar está vigente e você não selecionou a condição de pagamento da política. Deseja continuar?", null, "Sim", "Não");

                //                    if (!resposta)
                //                    {
                //                        return;
                //                    }
                //                    else
                //                    {
                //                        break;
                //                    }
                //                }
                //            }

                //        }
                //    }
                //}

                bool sucesso = false;
                bool sucessoDesconto = false;
                bool condicaoAlterou = false;
                if (Session.ATENDIMENTO_ATUAL.CodCondicaoPagamento != CondicaoPagamento.Codigo)
                {
                    //await UserDialogs.Instance.AlertAsync("A condição de pagamento escolhida é diferente da selecionado na abertura do atendimento, os precos será recalculados!");
                    condicaoAlterou = true;
                    if (CondicaoPagamento.Codigo == "59")
                        percDesc4 = 10;

                    if (CondicaoPagamento.Codigo == "107")
                        percDesc5 = 10;

                    sucesso = await _atendimentoRepository.TrocarCondPgtoAtendimento(Session.ATENDIMENTO_ATUAL.CodAtendimento, CondicaoPagamento.Codigo);
                    if (sucesso == true)
                    {
                        Session.ATENDIMENTO_ATUAL.CodCondicaoPagamento = CondicaoPagamento.Codigo;
                    }
                }

                if (DescontoAlterou != PedidoSelecionado.PercentualDesconto)
                {
                    sucessoDesconto = await _atendimentoRepository.AtualizarDesconto(Session.ATENDIMENTO_ATUAL.CodAtendimento, PedidoSelecionado.PercentualDesconto ?? 0);
                    if (sucessoDesconto == true)
                    {
                        Session.ATENDIMENTO_ATUAL.PercentualDesconto1 = PedidoSelecionado.PercentualDesconto ?? 0;
                    }
                }

                if (sucesso == true || sucessoDesconto == true)
                {
                    MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
                }

                var model = new
                {
                    CodCarrinho = PedidoSelecionado.CodCarrinho,
                    CodCondicaoPagamento = CondicaoPagamento?.Codigo,
                    PercentualDesconto = PedidoSelecionado.PercentualDesconto,
                    PercentualDesconto1 = 0,//PercentualDesconto1Max,
                    PercentualDesconto2 = PercentualDesconto2,
                    PercentualDesconto3 = PercentualDesconto3,
                    PercentualDesconto4 = percDesc4,
                    PercentualDesconto5 = percDesc5,
                    DataEntrega = DataEntrega.HasValue ? DataEntrega.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    CifFob = this.CifFob == "CIF" ? "C" : "F",
                    CodTransportadora = Transportadora?.Codigo,
                    Observacoes = this.Observacoes,
                    ObservacoesSeparacao = this.ObservacoesSeparacao,
                    // CodDeposito = Deposito?.Codigo,
                    OrdemCompra = this.OrdemCompra,
                    IndPedidoMae = IsPedidoMae == true ? 1 : 0,
                    AceitaFaturamentoAntecipado = IsFatAntecipado == true ? 1 : 0,
                    AceitaFaturamentoParcial = IsFatParcial == true ? 1 : 0,
                    CodTipoPedido = PedidoSelecionado.CodTipoPedido,
                    PercentualComissaoRep = Convert.ToDecimal(AplicacaoComissao)

                };

                var columnsName = model.GetType().GetRuntimeProperties().Select(x => x.Name).ToList();
                int rows = await _dataBaseRepository.ExecutaUpdate("TBT_CARRINHO", columnsName, new List<string>() { "CodCarrinho" }, model);
                var atualizouCarrinho = await _carrinhoRepository.AtualizaQtdCarrinho(PedidoSelecionado.CodCarrinho);

                if (DescontoAlterou != PedidoSelecionado.PercentualDesconto || condicaoAlterou == true)
                {
                    var listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1"));
                    var ItemSelecionado = new List<ItemCommandResult>();
                    foreach (var item in listCarrinhos.Where(x => x.CodCarrinho == PedidoSelecionado.CodCarrinho))
                    {
                        ItemSelecionado.AddRange(item.Itens);
                    }

                    AtualizarCarrinhoCommand command = new AtualizarCarrinhoCommand(
                                                                                PedidoSelecionado.CodCarrinho,
                                                                                Session.USUARIO_LOGADO,
                                                                                Session.ATENDIMENTO_ATUAL,
                                                                                ItemSelecionado);

                    var result = await _carrinhoCommandHandler.Handle(command) as HandlerResult;
                }


                if (rows > 0 && atualizouCarrinho)
                    await UserDialogs.Instance.AlertAsync("Carrinho salvo", AppName);
                else
                {
                    await UserDialogs.Instance.AlertAsync("Ocorreu um erro ao tentar persistir as informações", AppName);
                    return;
                }

                MessagingCenter.Send<object>(this, "LoadCarrinho");
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        //versão 35446
        //private async void SalvarFechamento()
        //{
        //    try
        //    {
        //        decimal percDesc5 = 0;
        //        decimal percDesc4 = 0;
        //        if (!string.IsNullOrEmpty(PercentualDesconto5))
        //            decimal.TryParse(PercentualDesconto5.Replace(",", "."), NumberStyles.Any, new CultureInfo("en-US"), out percDesc5);

        //        if (!string.IsNullOrEmpty(PercentualDesconto4))
        //            decimal.TryParse(PercentualDesconto4.Replace(",", "."), NumberStyles.Any, new CultureInfo("en-US"), out percDesc4);

        //        if (!DataEntrega.HasValue)
        //        {
        //            await UserDialogs.Instance.AlertAsync("A data de entrega é obrigatoria.");
        //            return;
        //        }

        //        if (AplicacaoComissao > 100)
        //        {
        //            await UserDialogs.Instance.AlertAsync("A comissão não deve ser maior que 100.");
        //            return;
        //        }

        //        if (DataEntrega.Value < DateTime.Now)
        //        {
        //            await UserDialogs.Instance.AlertAsync("A data de entrega não pode ser inferior a hoje.");
        //            return;
        //        }

        //        if (PedidoSelecionado.CodTipoPedido == "1")
        //        {
        //            this.Observacoes = null;
        //        }

        //        //Regra Politica Escolar
        //        var dataHoje = DateTime.Now.Date.ToString("yyyy-MM-ddTHH:mm:ss");
        //        //busca as politicas vigentes
        //        var dataPoliticaEscolar = await _politicaComercialRepository.BuscarDataPoliticaEscolar(dataHoje);

        //        //verifica se existe alguma vigente
        //        if (dataPoliticaEscolar.Count > 0) {

        //            //dentre as vigentes, verificar se escolheu é MiniVA(Mini Voltas Aulas).
        //            var politicaMiniVA =  dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento == CondicaoPagamento.Codigo && x.CodCondicaoPagamento == "MV3").FirstOrDefault();

        //            //se escolheu
        //            if (politicaMiniVA != null) {
        //                //valida se o carrinho esta certo
        //                var validaItensMiniVA = await _carrinhoRepository.GetItensPoliticaMiniVACarrinho(PedidoSelecionado.CodCarrinho);
        //                //se não tiver, informa
        //                if (!validaItensMiniVA)
        //                {
        //                    await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
        //                    return;
        //                }
        //            }

        //            //dentre as vigentes, verificar se escolheu é VA(Voltas Aulas).
        //            var politicaVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento.Contains(CondicaoPagamento.Codigo)).FirstOrDefault();

        //            //se escolheu
        //            if (politicaVA != null)
        //            {
        //                //valida se o carrinho esta certo
        //                var validaItensVA = await _carrinhoRepository.GetItensPoliticaVACarrinho(PedidoSelecionado.CodCarrinho);
        //                //se não tiver, informa
        //                if (!validaItensVA)
        //                {
        //                    await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
        //                    return;
        //                }
        //            }


        //            //bool temCondicao = false;
        //            //foreach (var politica in dataPoliticaEscolar) {
        //            //    if (politica.CodCondicaoPagamento == CondicaoPagamento.Codigo) {
        //            //        temCondicao = true;
        //            //        break;
        //            //    }
        //            //}

        //            //if (temCondicao)
        //            //{
        //            //    if (!existeItemPolitica)
        //            //    {
        //            //        await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
        //            //        return;
        //            //    }
        //            //}
        //            //else {

        //            //    if (existeItemPolitica) {

        //            //        var resposta = await UserDialogs.Instance.ConfirmAsync("O período de digitação da Política Escolar está vigente e você não selecionou a condição de pagamento da política. Deseja continuar?", null, "Sim", "Não");

        //            //        if (!resposta)
        //            //        {
        //            //            return;
        //            //        }
        //            //    }
        //            //}
        //        }

        //        //########################################################

        //        PedidoSelecionado.JustificativaDesconto = await _carrinhoRepository.GetJustificarivaDesconto(PedidoSelecionado);
        //        if (PedidoSelecionado.PercentualDesconto > 0 && string.IsNullOrEmpty(PedidoSelecionado.JustificativaDesconto))
        //        {
        //            var clienteAtendimento = await _clienteRepository.BuscarClientePorCode(new BuscarClienteCommand(null, null, Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

        //            var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorProduto(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", clienteAtendimento.CodigoSegmento, null));

        //            if (coefiDesconto != null)
        //            {
        //                TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = CondicaoPagamento?.Codigo });
        //                string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

        //                var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", clienteAtendimento.CodigoSegmento, null, prazoMedio));

        //                decimal coeficiente = coefiDesconto.Coeficiente;
        //                if (coefiPrazoMedio != null)
        //                {
        //                    if (coefiPrazoMedio.Coeficiente > 0)
        //                    {
        //                        if (Convert.ToDecimal(prazoMedio) >= 60)
        //                        {
        //                            if (!string.IsNullOrEmpty(clienteAtendimento.Prazo))
        //                            {
        //                                if (Convert.ToDecimal(prazoMedio) > Convert.ToDecimal(clienteAtendimento.Prazo))
        //                                {
        //                                    coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                                }
        //                                else
        //                                {
        //                                    coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                                }
        //                            }
        //                            else {
        //                                coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                        }
        //                    }
        //                }
        //                var des = PedidoSelecionado.PercentualDesconto / 100;
        //                if (des > coeficiente)
        //                {
        //                    var descontoInteiro = coeficiente != null ? Convert.ToInt32(coeficiente * 100) : coeficiente;
        //                    var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto Informado é maior que o desconto máximo permitido de {descontoInteiro}% para condição de  {condicaoPagamento.Descricao.Trim()}, seu pedido será enviado para aprovação, deseja continuar?", "Desconto Excedido", "Sim", "Não");
        //                    if (!desconto)
        //                    {
        //                        return;
        //                    }
        //                    else {
        //                        //abrir popup
        //                        await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupDescontoJustificativa(PedidoSelecionado));
        //                        return;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                await UserDialogs.Instance.AlertAsync("Não foi encontrado um segmento válido no cliente para o desconto. Sincronize ou entre em contato com a Pegada.", AppName, "OK");
        //                return;
        //            }
        //        }
        //        //########################################################

        //        //if (dataPoliticaEscolar.Count > 0 && existeItemPolitica)
        //        //{
        //        //    foreach (var politica in dataPoliticaEscolar) {

        //        //        foreach (var item in PedidoSelecionado.Itens)
        //        //        {
        //        //            var detalhesDoProduto = await _nivelRepository.GetNiveisProduto(item.CodProduto);
        //        //            var categoriaDoProduto = detalhesDoProduto[1].CodAtributo;
        //        //            var infoPoliticaComercialNiveis = await _politicaComercialRepository.BuscarPoliticaEscolarNiveis(categoriaDoProduto, politica.CodPoliticaComercial);

        //        //            if (infoPoliticaComercialNiveis.Count > 0)
        //        //            {
        //        //                string condPagamentoPoliticaEscolar = politica.CodCondicaoPagamento;
        //        //                var condPagamento = condPagamentoPoliticaEscolar.Split(',');
        //        //                bool temCondicao = false;

        //        //                foreach (string cond in condPagamento)
        //        //                {
        //        //                    temCondicao = CondicaoPagamento.Codigo.Equals(cond);
        //        //                    if (temCondicao)
        //        //                        break;
        //        //                }

        //        //                if (!temCondicao)
        //        //                {
        //        //                    var resposta = await UserDialogs.Instance.ConfirmAsync("O período de digitação da Política Escolar está vigente e você não selecionou a condição de pagamento da política. Deseja continuar?", null, "Sim", "Não");

        //        //                    if (!resposta)
        //        //                    {
        //        //                        return;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        break;
        //        //                    }
        //        //                }
        //        //            }

        //        //        }
        //        //    }
        //        //}

        //        bool sucesso = false;
        //        bool sucessoDesconto = false;
        //        bool condicaoAlterou = false;
        //        if (Session.ATENDIMENTO_ATUAL.CodCondicaoPagamento != CondicaoPagamento.Codigo)
        //        {
        //            //await UserDialogs.Instance.AlertAsync("A condição de pagamento escolhida é diferente da selecionado na abertura do atendimento, os precos será recalculados!");
        //            condicaoAlterou = true;
        //            if (CondicaoPagamento.Codigo == "59")
        //                percDesc4 = 10;

        //            if (CondicaoPagamento.Codigo == "107")
        //                percDesc5 = 10;

        //            sucesso = await _atendimentoRepository.TrocarCondPgtoAtendimento(Session.ATENDIMENTO_ATUAL.CodAtendimento, CondicaoPagamento.Codigo);
        //            if (sucesso == true)
        //            {
        //                Session.ATENDIMENTO_ATUAL.CodCondicaoPagamento = CondicaoPagamento.Codigo;
        //            }
        //        }

        //        if (DescontoAlterou != PedidoSelecionado.PercentualDesconto)
        //        {
        //            sucessoDesconto = await _atendimentoRepository.AtualizarDesconto(Session.ATENDIMENTO_ATUAL.CodAtendimento, PedidoSelecionado.PercentualDesconto ?? 0);
        //            if (sucessoDesconto == true)
        //            {
        //                Session.ATENDIMENTO_ATUAL.PercentualDesconto1 = PedidoSelecionado.PercentualDesconto ?? 0;
        //            }
        //        }

        //        if (sucesso == true || sucessoDesconto == true)
        //        {
        //            MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
        //        }

        //            var model = new
        //        {
        //            CodCarrinho = PedidoSelecionado.CodCarrinho,
        //            CodCondicaoPagamento = CondicaoPagamento?.Codigo,
        //            PercentualDesconto = PedidoSelecionado.PercentualDesconto,
        //            PercentualDesconto1 = 0,//PercentualDesconto1Max,
        //            PercentualDesconto2 = PercentualDesconto2,
        //            PercentualDesconto3 = PercentualDesconto3,
        //            PercentualDesconto4 = percDesc4,
        //            PercentualDesconto5 = percDesc5,
        //            DataEntrega = DataEntrega.HasValue ? DataEntrega.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
        //            CifFob = this.CifFob == "CIF" ? "C" : "F",
        //            CodTransportadora = Transportadora?.Codigo,
        //            Observacoes = this.Observacoes,
        //            ObservacoesSeparacao = this.ObservacoesSeparacao,
        //           // CodDeposito = Deposito?.Codigo,
        //            OrdemCompra = this.OrdemCompra,
        //            IndPedidoMae = IsPedidoMae == true ? 1 : 0,
        //            AceitaFaturamentoAntecipado = IsFatAntecipado == true ? 1 : 0,
        //            AceitaFaturamentoParcial = IsFatParcial == true ? 1 : 0,
        //            CodTipoPedido = PedidoSelecionado.CodTipoPedido,
        //            PercentualComissaoRep = Convert.ToDecimal(AplicacaoComissao)

        //        };

        //        var columnsName = model.GetType().GetRuntimeProperties().Select(x => x.Name).ToList();
        //        int rows = await _dataBaseRepository.ExecutaUpdate("TBT_CARRINHO", columnsName, new List<string>() { "CodCarrinho" }, model);
        //        var atualizouCarrinho = await _carrinhoRepository.AtualizaQtdCarrinho(PedidoSelecionado.CodCarrinho);

        //        if (DescontoAlterou != PedidoSelecionado.PercentualDesconto || condicaoAlterou == true)
        //        {
        //            var listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1"));
        //            var ItemSelecionado = new List<ItemCommandResult>();
        //            foreach (var item in listCarrinhos.Where(x => x.CodCarrinho == PedidoSelecionado.CodCarrinho))
        //            {
        //                ItemSelecionado.AddRange(item.Itens);
        //            }

        //            AtualizarCarrinhoCommand command = new AtualizarCarrinhoCommand(
        //                                                                        PedidoSelecionado.CodCarrinho,
        //                                                                        Session.USUARIO_LOGADO,
        //                                                                        Session.ATENDIMENTO_ATUAL,
        //                                                                        ItemSelecionado);

        //            var result = await _carrinhoCommandHandler.Handle(command) as HandlerResult;
        //        }


        //        if (rows > 0 && atualizouCarrinho)
        //            await UserDialogs.Instance.AlertAsync("Carrinho salvo", AppName);
        //        else
        //        {
        //            await UserDialogs.Instance.AlertAsync("Ocorreu um erro ao tentar persistir as informações", AppName);
        //            return;
        //        }

        //        MessagingCenter.Send<object>(this, "LoadCarrinho");
        //        await PopupNavigation.Instance.PopAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
        //    }
        //}
        public void SetCarrinhoFechamento(CarrinhoCommandResult pedidoSelecionado)
        {
            PedidoSelecionado = pedidoSelecionado;

        }

        public void SetIsVisibleObs1(bool flagObservacao)
        {
            //throw new NotImplementedException();
        }
        #endregion

    }
}
