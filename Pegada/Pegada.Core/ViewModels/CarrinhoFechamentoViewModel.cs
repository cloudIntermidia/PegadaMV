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
using ZXing.QrCode.Internal;
using static Dropbox.Api.TeamLog.SharingMemberPolicy;
using Syncfusion.SfDataGrid.XForms;

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

        private List<SemanaResult> _semanas;
        public List<SemanaResult> Semanas
        {
            get { return _semanas; }
            set { SetProperty(ref _semanas, value); }
        }

        private List<GenericComboResult> _clientesEntrega;
        public List<GenericComboResult> ClientesEntrega
        {
            get { return _clientesEntrega; }
            set { SetProperty(ref _clientesEntrega, value); }
        }

        private SemanaResult _semanaSelecionada;
        public SemanaResult SemanaSelecionada
        {
            get { return _semanaSelecionada; }
            set { SetProperty(ref _semanaSelecionada, value); }
        }

        private List<GenericComboResult> _condicoesPagamento;
        public List<GenericComboResult> CondicoesPagamento
        {
            get { return _condicoesPagamento; }
            set { SetProperty(ref _condicoesPagamento, value); }
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


        private bool _isPrecoMostraPrecoLiquido;
        public bool IsPrecoMostraPrecoLiquido
        {
            get { return _isPrecoMostraPrecoLiquido; }
            set { SetProperty(ref _isPrecoMostraPrecoLiquido, value); }
        }


        private bool _isMostraSemana;
        public bool IsMostraSemana
        {
            get { return _isMostraSemana; }
            set { SetProperty(ref _isMostraSemana, value); }
        }

        private bool _isHabilitaEdicao;
        public bool IsHabilitaEdicao
        {
            get { return _isHabilitaEdicao; }
            set { SetProperty(ref _isHabilitaEdicao, value); }
        }

        private string _ordemCompra;
        public string OrdemCompra
        {
            get { return _ordemCompra; }
            set { SetProperty(ref _ordemCompra, value); }
        }

        private GenericComboResult _condicaoPagamento;
        public GenericComboResult CondicaoPagamento
        {
            get { return _condicaoPagamento; }
            set { SetProperty(ref _condicaoPagamento, value); }
        }

        private GenericComboResult _clienteEntregaSelecionado;
        public GenericComboResult ClienteEntregaSelecionado
        {
            get { return _clienteEntregaSelecionado; }
            set { SetProperty(ref _clienteEntregaSelecionado, value); }
        }

        private decimal _percentualComissaoRep;
        public decimal PercentualComissaoRep
        {
            get { return _percentualComissaoRep; }
            set { SetProperty(ref _percentualComissaoRep, value); }
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
        public ICommand SelecionarSemanaCommand { get; set; }

        public ICommand SelecionarClienteEntregaCommand { get; set; }

        public ICommand SwitchFaturamentoAntecipadoCommand { get; }
        #endregion

        #region "Repositorios"
        private CarrinhoCommandHandler _carrinhoCommandHandler;
        private ICarrinhoRepository _carrinhoRepository;
        private readonly ISemanaRepository _semanaRepository;
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
        private readonly IPrazoAdicionalRepository _prazoAdicionalRepository;

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
                    , ISemanaRepository semanaRepository
                    , IPrazoAdicionalRepository prazoAdicionalRepository
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
            _semanaRepository = semanaRepository;
            _prazoAdicionalRepository = prazoAdicionalRepository;


            CancelarFechamentoCommand = new Command(CancelarFechamento);
            SalvarFechamentoCommand = new Command(SalvarFechamento);
            SelecionarDataEntregaCommand = new Command(SelecionarDataEntrega);
            SelecionarTipoFreteCommand = new Command(SelecionarTipoFrete);
            SelecionarTransportadoraCommand = new Command(SelecionarTransportadora);
            SelecionarCondicaoPagamentoCommand = new Command(SelecionarCondicaoPagamento);
            SelecionarSemanaCommand = new Command(SelecionarSemana);
            SelecionarDepositoCommand = new Command(SelecionarDeposito);
            SelecionarTipoPedidoCommand = new Command(SelecionarTipoPedido);
            CarrinhoFechamento = new CarrinhoFechamentoCommandResult();

            SelecionarClienteEntregaCommand = new Command(SelecionarClienteEntrega);

            SwitchFaturamentoAntecipadoCommand = new Command<bool>(OnSwitchFaturamentoAntecipado);
            // buscaDescontoFrenteCliente();

            AceitaFatAntCommand = new Command(ChangeIsAceitaFatAntecipado);
            IsFatAntecipado = true;

            AceitaFatParcialCommand = new Command(ChangeIsAceitaFatParcial);
            IsFatParcial = true;

            CifFob = "CIF";

        }
        #endregion
        #region "Metodos Pegada"

        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        public void SetCarrinhoFechamento(CarrinhoCommandResult pedidoSelecionado)
        {
            PedidoSelecionado = pedidoSelecionado;
        }

        public async void Init()
        {
            try
            {
                IsMostraSemana = true;
                if (PedidoSelecionado.TipoPedidoValida == "PE") {
                    IsMostraSemana = false;
                }

                await CarregaCondicaoPagamento();

                await CarregaSemanas();

                IsFatAntecipado = false;
                if (PedidoSelecionado.AceitaFaturamentoAntecipado == 1 && PedidoSelecionado.TipoPedidoValida != "PE")
                {
                    IsFatAntecipado = true;
                }
                else {
                    PedidoSelecionado.AceitaFaturamentoAntecipado = 0;
                }

                IsHabilitaEdicao = true;
                if (PedidoSelecionado.ClientePermiteAlterarCondi != 1) {
                    IsHabilitaEdicao = false;
                }

                await CarregaClientesEntrega();

                await CarregaPrecoLiquido();


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
                        if (item.ColumnName == "CodTransportadora")
                        {
                            Transportadora = (await _transportadoraRepository.BuscarTransportadoras(new BuscarTransportadoraCommand() { CodTransportadora = item.ColumnValue })).FirstOrDefault();
                        }
                        //else if (item.ColumnName == "AceitaFaturamentoAntecipado")
                        //{
                        //    if (item.ColumnValue == null)
                        //    {
                        //        IsFatAntecipado = true;
                        //    }
                        //    else
                        //    {
                        //        IsFatAntecipado = item.ColumnValue == "1" ? true : false;
                        //    }
                        //}
                        //else if (item.ColumnName == "AceitaFaturamentoParcial")
                        //{
                        //    if (item.ColumnValue == null)
                        //    {
                        //        IsFatParcial = true;
                        //    }
                        //    else
                        //    {
                        //        IsFatParcial = item.ColumnValue == "1" ? true : false;
                        //    }
                        //}
                        else if (item.ColumnName == "CodTipoPedido")
                        {
                            if (item.ColumnValue != "1")
                            {
                                //ChangeKeyIsVisibleObs();
                            }
                        }
                        else if (item.ColumnName == "CodCondicaoPagamento")
                        {
                            if (item.ColumnValue != null)
                            {
                                CondicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamentoCode(item.ColumnValue);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async Task CarregaPrecoLiquido()
        {
            IsPrecoMostraPrecoLiquido = false;
            if (PedidoSelecionado.CompraPrecoLiquido == 1) {
                IsPrecoMostraPrecoLiquido = true;
            }
        }

        private async Task CarregaCondicaoPagamento()
        {

            var condicoesTmp = new List<GenericComboResult>();
            var command = new BuscarCondicaoPagamentoCommand()
            {
                CodTabelaPreco = Session.ATENDIMENTO_ATUAL.CodTabelaPreco
            };
            CondicoesPagamento = await _condicaoPagamentoRepository.BuscarCondicoesParaFechamento(command);

            command = new BuscarCondicaoPagamentoCommand()
            {
                CodPessoaCliente = Session.ATENDIMENTO_ATUAL.CodPessoaCliente
            };
            var condicaoDoCliente = await _condicaoPagamentoRepository.BuscarCondicaoPagamentoCliente(command);

            //#############################

            if (condicaoDoCliente != null && CondicaoPagamento == null)
            {

                CondicaoPagamento = condicaoDoCliente;

                bool contem = false;
                foreach (var c in CondicoesPagamento)
                {
                    //verifica se na consulta padrão ja existe a condiçao do cliente, para que não fique duplicada.
                    if (condicaoDoCliente.Codigo == c.Codigo)
                    {
                        contem = true;
                        break;
                    }
                }

                //se não tiver a condicao do cliente na lista original
                if (!contem)
                {
                    //adiciona
                    condicoesTmp.Add(condicaoDoCliente);
                }

                //se for um cliente novo, só mostra a opção de condição do cadastro dele.
                if (!PedidoSelecionado.CodPessoaCliente.Contains("."))
                {
                    foreach (var c in CondicoesPagamento)
                        condicoesTmp.Add(c);
                }
                else
                {
                    if (contem)
                    {
                        //adiciona
                        condicoesTmp.Add(condicaoDoCliente);
                    }
                }

                CondicoesPagamento.Clear();
                CondicoesPagamento = condicoesTmp;
            }
        }

        private async Task CarregaClientesEntrega() {

            var command = new BuscarClienteCommand(Session.ATENDIMENTO_ATUAL.CodPessoaCliente);

            var listaClientes = await _clienteRepository.BuscarClientesPorGrupo(command).ConfigureAwait(false);

            ClientesEntrega = new List<GenericComboResult>();

            foreach (var cliente in listaClientes) {

                var clienteGene = new GenericComboResult();
                clienteGene.Codigo = cliente.CodPessoaCliente;
                clienteGene.Descricao = cliente.RazaoSocial;

                ClientesEntrega.Add(clienteGene);
            }
        }

        private async Task CarregaSemanas() {

            var dataMinima = await _carrinhoRepository.GetDataMinimaPorPedido(PedidoSelecionado.CodCarrinho, PedidoSelecionado.CodTipoPedido);

            var fabricas = await _carrinhoRepository.BuscarFabricasPorCarrinho(PedidoSelecionado.CodCarrinho);

            var semanaFabricas = new List<SemanaResult>();
            foreach (var f in fabricas) {

                var sf = new SemanaResult();
                sf.CodLinha = f.CodLinha;
                sf.CodFabrica = f.CodFabrica;
                sf.Descricao = f.Descricao;

                semanaFabricas.Add(sf);
            }

            var commandFabrica = new FabricaCommand(semanaFabricas, dataMinima, DateTime.Now, PedidoSelecionado.IndValidaPrazo, null, Session.ATENDIMENTO_ATUAL?.CodTabelaPreco, PedidoSelecionado.CodCarrinho);
            this.Semanas = await _semanaRepository.BuscarSemanasPorFabrica(commandFabrica);

            if (PedidoSelecionado.CodSemana != null && PedidoSelecionado.CodSemana != "0")
            {
                if (PedidoSelecionado.IndValidaPrazo == 0)
                {
                    SemanaSelecionada = await _semanaRepository.GetSemanaPorCode(PedidoSelecionado.CodSemana);
                }
                else
                {
                    bool achou = false;
                    foreach (var s in this.Semanas) {
                        if (s.CodSemana == PedidoSelecionado.CodSemana) {
                            SemanaSelecionada = await _semanaRepository.GetSemanaPorCode(PedidoSelecionado.CodSemana);
                            achou = true;
                            break;
                        }
                    }
                    if (achou == false) {
                        SemanaSelecionada = null;
                    }
                }
            }
        }

        public async void SelecionarSemana()
        {
            try
            {

                await PopupNavigation.Instance.PushAsync(
                    RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(this.Semanas),
                    SetSemanaSelecionada,
                    new Rectangle(0.5, 0.5, 0.5, 0.5), true, true, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }

        private async void SetSemanaSelecionada(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            var gc = obj as GenericComboResult;

            SemanaSelecionada = await _semanaRepository.GetSemanaPorCode(gc.Codigo);

            DataEntrega = SemanaSelecionada.DataInicial;

            await PopupNavigation.Instance.PopAsync();
        }

        public async void SelecionarDataEntrega()
        {

            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupCalendario(SelecionarDataEvent, PedidoSelecionado.TipoPedidoValida == "PE" ? DateTime.Now.AddDays(1) : SemanaSelecionada?.DataInicial, SemanaSelecionada?.DataFinal));
            //var data = await UserDialogs.Instance.DatePromptAsync(new DatePromptConfig() { MinimumDate = PedidoSelecionado.TipoPedidoValida == "PE" ? DateTime.Now.AddDays(1) : SemanaSelecionada?.DataInicial, MaximumDate = SemanaSelecionada?.DataFinal });
            //if (data.Ok)
            //{

            //    DateTime dateErrada = new DateTime(0001, 1, 1, 0, 0, 0);
            //    int result = DateTime.Compare(data.Value, dateErrada);
            //    if (result == 0)
            //    {
            //        DataEntrega = DateTime.Now.AddDays(1);
            //    }
            //    else
            //    {
            //        DataEntrega = data.Value;
            //    }
            //}

        }

        private async void SelecionarDataEvent(object obj)
        {
            if (obj is DateTime dataSelecionada)
            {
                DataEntrega = dataSelecionada;
            }
            await PopupNavigation.Instance.PopAsync();
        }

        public async void SelecionarCondicaoPagamento()
        {
            try
            {

                await PopupNavigation.Instance.PushAsync(
                    RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(CondicoesPagamento),
                    CondicaoPagamentoSelecionada,
                    new Rectangle(0.5, 0.5, 0.5, 0.5), true, true, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }

        private async void CondicaoPagamentoSelecionada(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            CondicaoPagamento = obj as GenericComboResult;

            await PopupNavigation.Instance.PopAsync();
        }

        //####################

        public async void SelecionarClienteEntrega()
        {
            try
            {

                await PopupNavigation.Instance.PushAsync(
                    RgPopupUtility.GerarPopupGenerico(new ObservableCollection<GenericComboResult>(ClientesEntrega),
                    SetClienteEntregaSelecionado,
                    new Rectangle(0.5, 0.5, 0.75, 0.5), true, true, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }

        private async void SetClienteEntregaSelecionado(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            ClienteEntregaSelecionado = obj as GenericComboResult;

            await PopupNavigation.Instance.PopAsync();
        }

        //####################

        private void OnSwitchFaturamentoAntecipado(bool ligado)
        {
            if (ligado)
            {
                IsFatAntecipado = true;
            }
            else
            {
                IsFatAntecipado = false;
            }
        }

        private async void SalvarFechamento()
        {
            try
            {

                if (CondicaoPagamento == null)
                {
                    await UserDialogs.Instance.AlertAsync("Prazo obrigatório.");
                    return;
                }

                if (PedidoSelecionado.TipoPedidoValida != "PE")
                {
                    if (SemanaSelecionada == null)
                    {
                        await UserDialogs.Instance.AlertAsync("Favor selecionar uma semana!");
                        return;
                    }
                }

                if (!DataEntrega.HasValue)
                {
                    await UserDialogs.Instance.AlertAsync("A data de entrega é obrigatoria.");
                    return;
                }

                if (DataEntrega.Value < DateTime.Now)
                {
                    await UserDialogs.Instance.AlertAsync("A data de entrega não pode ser inferior a hoje.");
                    return;
                }

                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                int qtdMinimaParcela = 1;

                if (CondicaoPagamento != null)
                {
                    qtdMinimaParcela = CondicaoPagamento.QtdParcela;
                }
                else
                {
                    qtdMinimaParcela = Convert.ToInt32(PedidoSelecionado.qtdParcela);
                }

                //Solicitação #17523 Agrupar por famila de Vendas
                //[...]"Ver a possibilidade tirar a validade de valor mínimo nesse tipo de cliente também."
                //if ([[Cliente paisForCliente:_carrinhoSelecionado.codPessoaCliente] intValue] == 1)
                if (PedidoSelecionado.CodTipoPedido != "23")
                {
                    var valorDuplicata = PedidoSelecionado.ValorTotalLiquido / qtdMinimaParcela;

                    var valorMinimoParcela = await _parametroRepository.BuscarMinimoParcelaPorTipoPedido(PedidoSelecionado.CodTipoPedido);

                    if (valorDuplicata < valorMinimoParcela)
                    {
                        await UserDialogs.Instance.AlertAsync($"O pedido não alcançou o valor mínimo por duplicata de R${valorMinimoParcela}");
                        return;
                    }
                }

                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                if (!string.IsNullOrEmpty(PedidoSelecionado.DiasBonificacao)) {

                    var prazoAdicional = await _prazoAdicionalRepository.BuscaPrazoAdicional(new BuscarPrazoAdicionalCommand(PedidoSelecionado.CodTipoPedido, PedidoSelecionado.DiasBonificacao));

                    if (prazoAdicional?.AbatimentoComissao > 0)
                    {
                        PercentualComissaoRep = prazoAdicional.AbatimentoComissao;
                    }
                    else {
                        await UserDialogs.Instance.AlertAsync("Prazo extra não permitido!");
                        return;
                    }
                }

                if (PedidoSelecionado?.AceitaFaturamentoAntecipado == 1) {
                    if (PedidoSelecionado.DiasFatAntecipado != 0) {
                        if (PedidoSelecionado.DiasFatAntecipadoCliente >= PedidoSelecionado.DiasFatAntecipado)
                        {
                            DateTime now = DateTime.Today;

                            // Converte o texto para int
                            int daysToSub = (int)PedidoSelecionado.DiasFatAntecipado;

                            // Subtrai os dias da data selecionada
                            DateTime toDate = DataEntrega.Value.Date.AddDays(-daysToSub);

                            // Comparação (equivalente ao NSOrderedDescending)
                            if (toDate > now)
                            {
                                //PedidoSelecionado.DiasFatAntecipado = PedidoSelecionado.DiasFatAntecipado;
                            }
                            else
                            {
                                await UserDialogs.Instance.AlertAsync($"O prazo de dias de faturamento é inválido, o prazo informado é maior do que o permitido para o cliente!");
                                return;
                            }
                        }
                        else {
                            await UserDialogs.Instance.AlertAsync($"O prazo de dias de faturamento é inválido, o prazo informado é maior do que o permitido para o cliente!");
                            return;
                        }
                    }
                    else {
                        await UserDialogs.Instance.AlertAsync($"O campo Dia(s) de Faturamento Antecipado deve ser preenchido!");
                        return;
                    }
                }


                if (CondicaoPagamento.Codigo != PedidoSelecionado.CodCondicaoPagamento && PedidoSelecionado.IndPrecoLiquido == 1) {

                    var confirm = await UserDialogs.Instance.ConfirmAsync($"Há pedidos com preço líquido editado. Todos valores digitados serão perdidos. Deseja prosseguir?", "Atenção", "Sim", "Não");
                    if (!confirm)
                    {
                        return;
                    }

                    AtualizarCarrinho();
                }
                else {

                    AtualizarCarrinho();
                }

            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void AtualizarCarrinho() {

            var model = new
            {
                CodCarrinho = PedidoSelecionado.CodCarrinho,
                CodCondicaoPagamento = CondicaoPagamento?.Codigo,
                DataEntrega = DataEntrega.HasValue ? DataEntrega.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                CifFob = "F",
                CodTransportadora = Transportadora?.Codigo,
                AceitaFaturamentoAntecipado = PedidoSelecionado.AceitaFaturamentoAntecipado,
                AceitaFaturamentoParcial = PedidoSelecionado.AceitaFaturamentoParcial,
                CodSemana = SemanaSelecionada.CodSemana,
                PercentualComissaoRep = PercentualComissaoRep,
                DiasBonificacao = PedidoSelecionado.DiasBonificacao,
                OrdemCompra = PedidoSelecionado.OrdemCompra,
                DiasFatAntecipado = PedidoSelecionado?.AceitaFaturamentoAntecipado == 1 ? PedidoSelecionado.DiasFatAntecipado : 0,
                IndPrecoLiquido = PedidoSelecionado.IndPrecoLiquido,
                CodClienteEntrega = ClienteEntregaSelecionado?.Codigo
            };

            var columnsName = model.GetType().GetRuntimeProperties().Select(x => x.Name).ToList();
            int rows = await _dataBaseRepository.ExecutaUpdate("TBT_CARRINHO", columnsName, new List<string>() { "CodCarrinho" }, model);
            var atualizouCarrinho = await _carrinhoRepository.AtualizaQtdCarrinho(PedidoSelecionado.CodCarrinho);


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

        private async void CancelarFechamento()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        #endregion

        #region "Metodos Antigo"
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

        private async void ChangeIsAceitaFatAntecipado()
        {
            IsFatAntecipado = !IsFatAntecipado;
        }

        private async void ChangeIsAceitaFatParcial()
        {
            IsFatParcial = !IsFatParcial;
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


            CarrinhoFechamento.CodTipoPedido = (obj as GenericComboResult).Codigo;
            CarrinhoFechamento.TipoPedido = (obj as GenericComboResult).Descricao;

            PedidoSelecionado.TipoPedido = (obj as GenericComboResult).Descricao;
            PedidoSelecionado.CodTipoPedido = (obj as GenericComboResult).Codigo;
            await PopupNavigation.Instance.PopAsync();
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

        

        

        #endregion

    }
}
