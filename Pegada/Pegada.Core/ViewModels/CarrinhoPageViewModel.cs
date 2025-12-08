using Acr.UserDialogs;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Handlers;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Infra.Repositories;
using MobiliVendas.Core.Services;
using MobiliVendas.Core.Services.Contracts;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Plugin.Connectivity;
using Prism.Navigation;
using Prism.Services;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Pegada.Core.ViewModels
{
    public class CarrinhoPageViewModel : ViewModelBase, ICarrinhoPageViewModel
    {
        #region "Propriedades"

        private ObservableCollection<CarrinhoCommandResult> _pedidos;
        public ObservableCollection<CarrinhoCommandResult> Pedidos
        {
            get { return _pedidos; }
            set { SetProperty(ref _pedidos, value); }
        }

        private CarrinhoCommandResult _pedidoSelecionado;

        public CarrinhoCommandResult PedidoSelecionado
        {
            get { return _pedidoSelecionado; }
            set { SetProperty(ref _pedidoSelecionado, value); }
        }

        private ItemCommandResult _itemSelecionado;
        public ItemCommandResult ItemSelecionado
        {
            get { return _itemSelecionado; }
            set { SetProperty(ref _itemSelecionado, value); }
        }

        private bool _todosItensChecados;
        public bool TodosItensChecados
        {
            get { return _todosItensChecados; }
            set { SetProperty(ref _todosItensChecados, value); }
        }

        private bool _todosCarrinhosChecados;
        public bool TodosCarrinhosChecados
        {
            get { return _todosCarrinhosChecados; }
            set { SetProperty(ref _todosCarrinhosChecados, value); }
        }

        public Command<object> ItemTappedCommand { get; set; }
        public Command<object> PedidoTappedCommand { get; set; }

        public Command MarcarCarrinhosCommand { get; set; }
        public Command MarcarItensCommand { get; set; }

        public ICommand ExcluirItemCommand { get; set; }
        public ICommand EditarItemCommand { get; set; }
        public ICommand DesmembrarItemCommand { get; set; }
        public ICommand MarcarItemCommand { get; set; }

        public ICommand CopiarCommand { get; set; }
        public ICommand BloquearCommand { get; set; }
        public ICommand EditarCommand { get; set; }
        public ICommand ImportPlanilhaCommand { get; set; }
        public ICommand TransmitirCommand { get; set; }
        public ICommand CancelarCommand { get; set; }
        public ICommand ImprimirCommand { get; set; }
        public ICommand MarcarCarrinhoCommand { get; set; }
        public ICommand TrocarClienteCommand { get; set; }
        public ICommand AgruparCommand { get; set; }
        private readonly IKitRepository _kitRepository;
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly CarrinhoCommandHandler _carrinhoHandler;
        private readonly ICondicaoPagamentoRepository _condicaoPagamentoRepository;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly IPrintService _printService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IModeloRepository _modeloRepository;
        private readonly ICoeficienteRepository _coeficienteRepository;
        private readonly INivelRepository _nivelRepository;
        private readonly IPoliticaComercialRepository _politicaComercialRepository;
        private DataBaseRepository _dataBaseRepository;
        #endregion

        #region "Ctor"
        public CarrinhoPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
                                        IAtendimentoRepository atendimentoRepository, ICarrinhoRepository carrinhoRepository,
                                        CarrinhoCommandHandler carrinhoHandler, ICondicaoPagamentoRepository condicaoPagamento,
                                        IParametroSincronizacaoRepository parametroSincronizacaRepository, IParametroRepository parametroRepository,
                                        IPrintService printService, IFotoRepository fotoRepository,
                                        IClienteRepository clienteRepository, IProdutoRepository produtoRepository, IKitRepository kitRepository, IModeloRepository modeloRepository
            , ICoeficienteRepository coeficienteRepository, INivelRepository nivelRepository, IPoliticaComercialRepository politicaComercialRepository, DataBaseRepository dataBaseRepository)
            : base(navigationService, dialogService)
        {
            _fotoRepository = fotoRepository;
            _carrinhoRepository = carrinhoRepository;
            _condicaoPagamentoRepository = condicaoPagamento;
            _carrinhoHandler = carrinhoHandler;
            _atendimentoRepository = atendimentoRepository;
            _parametroSincronizacaRepository = parametroSincronizacaRepository;
            _parametroRepository = parametroRepository;
            _printService = printService;
            _clienteRepository = clienteRepository;
            _produtoRepository = produtoRepository;
            _kitRepository = kitRepository;
            _modeloRepository = modeloRepository;
            _coeficienteRepository = coeficienteRepository;
            _nivelRepository = nivelRepository;
            _politicaComercialRepository = politicaComercialRepository;
            _dataBaseRepository = dataBaseRepository;

            PedidoTappedCommand = new Command<object>(PedidoTapped);
            CopiarCommand = new Command<CarrinhoCommandResult>(Copiar);
            EditarCommand = new Command<CarrinhoCommandResult>(Editar);
            ImportPlanilhaCommand = new Command<CarrinhoCommandResult>(ImportarPlanilha);
            TransmitirCommand = new Command<CarrinhoCommandResult>(Transmitir);
            CancelarCommand = new Command(Cancelar);
            ImprimirCommand = new Command<CarrinhoCommandResult>(Imprimir);
            MarcarCarrinhoCommand = new Command<CarrinhoCommandResult>(MarcarCarrinho);
            TrocarClienteCommand = new Command(TrocarClienteDoCarrinho);
            BloquearCommand = new Command(Bloquear);
            AgruparCommand = new Command(Agrupar);

            ItemTappedCommand = new Command<object>(ItemTapped);
            ExcluirItemCommand = new Command<ItemCommandResult>(async (obj) => await ExcluirItem(obj));
            EditarItemCommand = new Command<ItemCommandResult>(EditarItem);
            DesmembrarItemCommand = new Command<ItemCommandResult>(DesmembrarItem);
            MarcarItemCommand = new Command<ItemCommandResult>(MarcarItem);

            MarcarCarrinhosCommand = new Command(MarcarCarrinhos);
            MarcarItensCommand = new Command(MarcarItens);
            Pedidos = new ObservableCollection<CarrinhoCommandResult>();

            MessagingCenter.Subscribe<object>(this, "ClienteSelecionado", CopiarCarrinho, null);
            MessagingCenter.Subscribe<object>(this, "AtendimentoFoiAlterado", TrocarAtendimento, null);
            MessagingCenter.Subscribe<object>(this, "LoadCarrinho", Load, null);
            MessagingCenter.Subscribe<ICarrinhoFechamentoViewModel, CarrinhoFechamentoCommandResult>(this, "CarrinhoAlterado", ModificarCarrinhoAlterado);
            MessagingCenter.Subscribe<ICarrinhoImpressaoViewModel, string>(this, "ExibirImpressao", ExibirImpressao);
        }

        ~CarrinhoPageViewModel()
        {
            MessagingCenter.Unsubscribe<object>(this, "ExibirImpressao");
            MessagingCenter.Unsubscribe<object>(this, "LoadCarrinho");
            MessagingCenter.Unsubscribe<object>(this, "ClienteSelecionado");
            MessagingCenter.Unsubscribe<object>(this, "CarrinhoAlterado");
            MessagingCenter.Unsubscribe<object>(this, "AtendimentoFoiAlterado");
        }
        #endregion

        #region "Metodos"
        private async void Bloquear()
        {
            if (!Pedidos.Any(x => x.CarrinhoChecado))
            {
                await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 pedido para Bloquear", AppName, "OK");
                return;
            }

            var confirm = await UserDialogs.Instance.ConfirmAsync($"Deseja realmente bloquear o(os) pedido(os) selecionado(os)?", "Cancelar", "Sim", "Não");
            if (!confirm)
            {
                return;
            }

            var list = Pedidos.Where(x => x.CarrinhoChecado).ToList();
            await _carrinhoRepository.BloquearCarrinhos(list);

            await UserDialogs.Instance.AlertAsync("Os pedidos foram alterados com sucesso.", AppName, "OK");
            await Load();
        }

        private void MarcarCarrinhos()
        {
            if (Pedidos?.Count > 0)
            {
                TodosCarrinhosChecados = !TodosCarrinhosChecados;
                foreach (var pedido in Pedidos)
                    pedido.CarrinhoChecado = TodosCarrinhosChecados;
            }
        }

        private void MarcarItens()
        {
            if (PedidoSelecionado != null)
            {
                TodosItensChecados = !TodosItensChecados;
                foreach (var item in PedidoSelecionado.Itens)
                    item.ItemChecado = TodosItensChecados;
            }
        }

        private void MarcarCarrinho(CarrinhoCommandResult obj)
        {
            obj.CarrinhoChecado = !obj.CarrinhoChecado;
        }

        private async void DesmembrarItem(ItemCommandResult obj)
        {
            try
            {
                if (!PedidoSelecionado.Itens.Any(x => x.ItemChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 item para desmembrar", AppName, "OK");
                    return;
                }

                if (!PedidoSelecionado.Itens.Any(x => !x.ItemChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você não pode desmembrar todos os itens do carrinho", AppName, "OK");
                    return;
                }

                var itensFuturo = PedidoSelecionado.Itens.Where(x => x.ItemChecado && x.CodDeposito != "IMEDIATO").ToList();

                if (itensFuturo.Count > 0)
                {
                    await DesmembraEF();
                }
                else
                {
                    await DesmembraPE();
                }

            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async Task DesmembraPE()
        {

            string DiasEntregaInicio = await _parametroRepository.BuscarValorParametro("DIASENTREGAINI");
            if (string.IsNullOrEmpty(DiasEntregaInicio))
            {
                DiasEntregaInicio = "-30 day";
            }

            string DiasEntregaFim = await _parametroRepository.BuscarValorParametro("DIASENTREGAFIM");
            if (string.IsNullOrEmpty(DiasEntregaFim))
            {
                DiasEntregaFim = "+30 day";
            }

            string opcaoSelecionada = string.Empty;
            var quebrarCommand = new QuebrarPedidoCommand(
                Session.ATENDIMENTO_ATUAL.CodAtendimento, Session.ATENDIMENTO_ATUAL.CodPessoaCliente,
                Session.ATENDIMENTO_ATUAL.CodPessoaRepresentante, Session.ATENDIMENTO_ATUAL.CodMarca,
                PedidoSelecionado.Itens.Where(x => x.ItemChecado).Select(x => x.CodProduto).FirstOrDefault(),
                PedidoSelecionado.CodTipoPedido, PedidoSelecionado.CodPoliticaComercial,
                PedidoSelecionado.CodTabelaPreco, null, null, DiasEntregaInicio, DiasEntregaFim);

            var carrinhos = await _atendimentoRepository.BuscarCarrinhosParaDesmembrar(quebrarCommand);
            if (carrinhos?.Count > 1)
            {
                carrinhos.Remove(PedidoSelecionado.CodCarrinho);
                carrinhos.Insert(0, "NOVO CARRINHO");
                opcaoSelecionada = await UserDialogs.Instance.ActionSheetAsync(AppName, "Cancelar", null, null, carrinhos.ToArray());
            }

            if (opcaoSelecionada == "Cancelar")
                return;
            else
            {
                var itens = PedidoSelecionado.Itens.Where(x => x.ItemChecado).ToList();
                itens.ForEach(x => x.Grades = PedidoSelecionado.Itens.Where(o => o.CodProduto == x.CodProduto).FirstOrDefault().Grades);

                DesmembrarCarrinhoCommand command = new DesmembrarCarrinhoCommand(
                    string.IsNullOrEmpty(opcaoSelecionada) || opcaoSelecionada == "NOVO CARRINHO" ? string.Empty : opcaoSelecionada,
                    Session.USUARIO_LOGADO,
                    Session.ATENDIMENTO_ATUAL,
                    itens,
                    PedidoSelecionado,
                    "1");

                var retorno = await _carrinhoHandler.Handle(command);
                if (
                    retorno is CarrinhoCommandResult    // Retorno de criação de carrinho novo
                    || (retorno is HandlerResult && ((HandlerResult)retorno).Sucesso) // Retorno de inserção em carrinho existente
                    )
                {
                    itens.ForEach(x => x.Grades.ToList().ForEach(o => o.Qtd = 0));
                    AtualizarCarrinhoCommand excluirCommand =
                        new AtualizarCarrinhoCommand(
                                PedidoSelecionado.CodCarrinho,
                                Session.USUARIO_LOGADO,
                                Session.ATENDIMENTO_ATUAL,
                                itens);

                    var result = await _carrinhoHandler.Handle(excluirCommand) as HandlerResult;
                    if (result.Sucesso)
                    {
                        await UserDialogs.Instance.AlertAsync($"Desmembramento concluído com sucesso.", AppName, "OK");
                        await Load();
                    }
                    else
                    {
                        string message = string.Join("\n", result.ListaErros);
                        await UserDialogs.Instance.AlertAsync($"Ocorreu um erro ao desmembrar itens. \n{message}", AppName, "OK");
                    }
                }
            }
        }

        private async Task DesmembraEF()
        {

            string DiasAgrupoInicio = await _parametroRepository.BuscarValorParametro("DIASAGRUPAINI");
            if (string.IsNullOrEmpty(DiasAgrupoInicio))
            {
                DiasAgrupoInicio = "-30 day";
            }

            string DiasAgrupaFim = await _parametroRepository.BuscarValorParametro("DIASAGRUPAFIM");
            if (string.IsNullOrEmpty(DiasAgrupaFim))
            {
                DiasAgrupaFim = "+30 day";
            }

            string opcaoSelecionada = string.Empty;
            var quebrarCommand = new QuebrarPedidoCommand(
                Session.ATENDIMENTO_ATUAL.CodAtendimento, Session.ATENDIMENTO_ATUAL.CodPessoaCliente,
                Session.ATENDIMENTO_ATUAL.CodPessoaRepresentante, Session.ATENDIMENTO_ATUAL.CodMarca,
                PedidoSelecionado.Itens.Where(x => x.ItemChecado).Select(x => x.CodProduto).FirstOrDefault(),
                PedidoSelecionado.CodTipoPedido, PedidoSelecionado.CodPoliticaComercial,
                PedidoSelecionado.CodTabelaPreco, null, "2", DiasAgrupoInicio, DiasAgrupaFim);

            var carrinhos = await _atendimentoRepository.BuscarCarrinhosParaDesmembrar(quebrarCommand);
            if (carrinhos?.Count > 1)
            {
                carrinhos.Remove(PedidoSelecionado.CodCarrinho);
                carrinhos.Insert(0, "NOVO CARRINHO");
                opcaoSelecionada = await UserDialogs.Instance.ActionSheetAsync(AppName, "Cancelar", null, null, carrinhos.ToArray());
            }

            if (opcaoSelecionada == "Cancelar")
                return;
            else
            {
                var itens = PedidoSelecionado.Itens.Where(x => x.ItemChecado).ToList();
                itens.ForEach(x => x.Grades = PedidoSelecionado.Itens.Where(o => o.CodProduto == x.CodProduto).FirstOrDefault().Grades);

                DesmembrarCarrinhoCommand command = new DesmembrarCarrinhoCommand(
                    string.IsNullOrEmpty(opcaoSelecionada) || opcaoSelecionada == "NOVO CARRINHO" ? string.Empty : opcaoSelecionada,
                    Session.USUARIO_LOGADO,
                    Session.ATENDIMENTO_ATUAL,
                    itens,
                    PedidoSelecionado,
                    "2");

                var retorno = await _carrinhoHandler.Handle(command);
                if (
                    retorno is CarrinhoCommandResult    // Retorno de criação de carrinho novo
                    || (retorno is HandlerResult && ((HandlerResult)retorno).Sucesso) // Retorno de inserção em carrinho existente
                    )
                {
                    itens.ForEach(x => x.Grades.ToList().ForEach(o => o.Qtd = 0));
                    AtualizarCarrinhoCommand excluirCommand =
                        new AtualizarCarrinhoCommand(
                                PedidoSelecionado.CodCarrinho,
                                Session.USUARIO_LOGADO,
                                Session.ATENDIMENTO_ATUAL,
                                itens);

                    var result = await _carrinhoHandler.Handle(excluirCommand) as HandlerResult;
                    if (result.Sucesso)
                    {
                        await UserDialogs.Instance.AlertAsync($"Desmembramento concluído com sucesso.", AppName, "OK");
                        await Load();
                    }
                    else
                    {
                        string message = string.Join("\n", result.ListaErros);
                        await UserDialogs.Instance.AlertAsync($"Ocorreu um erro ao desmembrar itens. \n{message}", AppName, "OK");
                    }
                }
            }
        }

        private async void EditarItem(ItemCommandResult obj)
        {
            try {
                 if (PedidoSelecionado != null) {

                    var itens = PedidoSelecionado.Itens.Where(x => x.ItemChecado).ToList();

                    if (itens.Count == 0)
                    {
                        await UserDialogs.Instance.AlertAsync("Marque um item para edição.", AppName);
                        return;
                    }

                    ItemSelecionado = itens[0];
                    if (itens.Count == 1)
                    {
                        if (ItemSelecionado.CodKit != null && ItemSelecionado.PedidoMae == null)
                        {
                            var filtros = new BuscarKitCommand
                            {
                                CodKit = ItemSelecionado.CodKit,
                                CodTabelaPreco = Session.ATENDIMENTO_ATUAL == null ? ItemSelecionado.CodTabelaPreco : Session.ATENDIMENTO_ATUAL?.CodTabelaPreco
                            };
                            var kits = await _kitRepository.BuscarKits(filtros);
                            var estoques = await _produtoRepository.BuscarEstoques(new BuscarEstoquesCommand() { CodProduto = ItemSelecionado.CodKit, CodDeposito = null });
                            if (estoques.Count == 0) {
                                throw new Exception("Produto" + ItemSelecionado.CodKit + " não possui estoque.");
                            }
                            ItemSelecionado.QtdTotal = ItemSelecionado.QtdTotal / kits.Where(x => x.CodProduto == ItemSelecionado.CodProduto).FirstOrDefault().Qtd;
                            ItemSelecionado.Grades.FirstOrDefault().Qtd = (int)ItemSelecionado.QtdTotal;
                            ItemSelecionado.QtdNaCaixa = kits.Where(x => x.CodProduto == ItemSelecionado.CodProduto).FirstOrDefault().Qtd;
                            ItemSelecionado.CodModelo = kits.FirstOrDefault().CodKit;
                            ItemSelecionado.CodProduto = kits.FirstOrDefault().CodKit;
                            ItemSelecionado.Descricao = kits.FirstOrDefault().Descricao;
                            ItemSelecionado.ItemEKit = 1;
                            ItemSelecionado.QtdEstoque = estoques.FirstOrDefault().QtdEstoque;
                            ItemSelecionado.PrecoCusto = kits.Sum(x => x.Valor);
                            ItemSelecionado.Grades.FirstOrDefault().Estoque = estoques.FirstOrDefault().QtdEstoque;
                        }

                        if (ItemSelecionado.PedidoMae != null)
                        {
                            if (!CrossConnectivity.Current.IsConnected)
                            {
                                await UserDialogs.Instance.AlertAsync($"Identificamos você está sem conexão a internet,\nfavor conectar a internet para buscar o estoque do pedido mãe.", AppName, "OK");
                                return;
                            }
                        }

                        await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupEdicaoItem(ItemSelecionado));
                    }
                    else
                    {
                        if (ItemSelecionado.CodKit != null)
                        {

                            if (ItemSelecionado.CodKit != null && ItemSelecionado.PedidoMae == null)
                            {
                                var filtros = new BuscarKitCommand
                                {
                                    CodKit = ItemSelecionado.CodKit,
                                    CodTabelaPreco = Session.ATENDIMENTO_ATUAL == null ? ItemSelecionado.CodTabelaPreco : Session.ATENDIMENTO_ATUAL?.CodTabelaPreco
                                };
                                var kits = await _kitRepository.BuscarKits(filtros);
                                decimal qtdNoKit = 1;
                                if (kits.Count > 0)
                                {
                                    qtdNoKit = kits.Where(x => x.CodProduto == ItemSelecionado.CodProduto).FirstOrDefault().Qtd;
                                }
                                var estoques = await _produtoRepository.BuscarEstoques(new BuscarEstoquesCommand() { CodProduto = ItemSelecionado.CodKit, CodDeposito = ItemSelecionado.CodDeposito });
                                if (estoques.Count == 0)
                                {
                                    throw new Exception("Produto" + ItemSelecionado.CodKit + " não possui estoque.");
                                }

                                ItemSelecionado.QtdTotal = ItemSelecionado.QtdTotal / qtdNoKit;
                                ItemSelecionado.Grades.FirstOrDefault().Qtd = (int)ItemSelecionado.QtdTotal;
                                ItemSelecionado.QtdNaCaixa = qtdNoKit;
                                ItemSelecionado.CodModelo = kits.FirstOrDefault().CodKit;
                                ItemSelecionado.CodProduto = kits.FirstOrDefault().CodKit;
                                ItemSelecionado.Descricao = kits.FirstOrDefault().Descricao;
                                ItemSelecionado.ItemEKit = 1;
                                ItemSelecionado.QtdEstoque = estoques.Count > 0 ? estoques.FirstOrDefault().QtdEstoque : 0;
                                ItemSelecionado.PrecoCusto = kits.Sum(x => x.Valor);
                                ItemSelecionado.Grades.FirstOrDefault().Estoque = estoques.Count > 0 ? estoques.FirstOrDefault().QtdEstoque : 0;
                            }

                            if (ItemSelecionado.PedidoMae != null)
                            {
                                if (!CrossConnectivity.Current.IsConnected)
                                {
                                    await UserDialogs.Instance.AlertAsync($"Identificamos você está sem conexão a internet,\nfavor conectar a internet para buscar o estoque do pedido mãe.", AppName, "OK");
                                    return;
                                }
                            }

                            await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupEdicaoItem(ItemSelecionado));

                        }
                        else
                        {
                            await UserDialogs.Instance.AlertAsync("Marque somente 1 item para edição.", AppName);
                        }
                    }
                }
            }
            catch (Exception ex) {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async Task ExcluirItem(ItemCommandResult obj)
        {
            try
            {
                if (!PedidoSelecionado.Itens.Any(x => x.ItemChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 item para excluir", AppName, "OK");
                    return;
                }

                string messageKits = string.Empty;
                string strProdKits = string.Empty;
                List<string> lstProdutosDoKit = null;
                var resp = await UserDialogs.Instance.ConfirmAsync("Deseja realmente excluir o item ?", AppName, "Sim", "Não");
                if (resp)
                {
                    var itensMarcados = PedidoSelecionado.Itens.Where(x => x.ItemChecado).ToList();
                    foreach (var item in itensMarcados)
                    {
                        if (item.CodKit != null)
                        {
                            if (PedidoSelecionado.Itens.Any(x => x.CodKit == item.CodKit && x.CodDeposito == item.CodDeposito && !x.ItemChecado))
                            {
                                lstProdutosDoKit = PedidoSelecionado.Itens.Where(x => x.CodKit == item.CodKit && x.CodDeposito == item.CodDeposito && !x.ItemChecado).Select(x => x.CodProduto).ToList();
                                strProdKits = string.Join(",", lstProdutosDoKit);
                                messageKits += $"Você não marcou o(os) item(ns) '({strProdKits})' que pertencem ao kit/pack '{item.CodKit}' do produto '{item.CodProduto}'.\n";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(messageKits))
                    {
                        var confirm = await UserDialogs.Instance.ConfirmAsync($"{messageKits}. Para excluir itens, o kit/pack todo deve excluído, confirma a exclusão de todos os itens do kit?", AppName, "SIM", "NÃO");
                        if (!confirm)
                        {
                            return;
                        }

                        foreach (var item in PedidoSelecionado.Itens)
                        {
                            if (itensMarcados.Any(x => x.CodKit == item.CodKit && x.CodDeposito == item.CodDeposito) && !item.ItemChecado)
                            {
                                item.ItemChecado = true;
                            }
                        }
                        itensMarcados = PedidoSelecionado.Itens.Where(x => x.ItemChecado).ToList();
                    }

                    itensMarcados.ForEach(x => x.QtdTotal = 0);
                    itensMarcados.ForEach(x => x.Grades = PedidoSelecionado.Itens.Where(o => o.CodProduto == x.CodProduto).FirstOrDefault().Grades);
                    itensMarcados.ForEach(x => x.Grades.ToList().ForEach(o => o.Qtd = 0));

                    if (!PedidoSelecionado.Itens.Any(x => !x.ItemChecado))
                    {
                        await UserDialogs.Instance.AlertAsync("Para excluir todos os itens do carrinho vá em cancelar carrinho", AppName, "OK");
                        return;
                    }

                    AtualizarCarrinhoCommand command =
                        new AtualizarCarrinhoCommand(
                                PedidoSelecionado.CodCarrinho,
                                Session.USUARIO_LOGADO,
                                Session.ATENDIMENTO_ATUAL,
                                itensMarcados);

                    var result = await _carrinhoHandler.HandleExclusaoItens(command);
                    if (result is CarrinhoCommandResult || (result is HandlerResult && ((HandlerResult)result).Sucesso))
                    {
                        await UserDialogs.Instance.AlertAsync($"Item(ns) excluído(os) com sucesso.", AppName, "OK");
                        await Load();
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync($"Ocorreu um erro ao excluir item.", AppName, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private void MarcarItem(ItemCommandResult obj)
        {
            if (obj.CodKit != null)
            {
                foreach (var item in PedidoSelecionado.Itens.Where(x => x.CodKit == obj.CodKit && x.CodDeposito == obj.CodDeposito))
                    item.ItemChecado = !item.ItemChecado;
            }
            else
            {
                obj.ItemChecado = !obj.ItemChecado;
            }
        }

        public async Task Load()
        {
            try
            {
                Pedidos.Clear();
                if (PedidoSelecionado != null)
                {
                    PedidoSelecionado = new CarrinhoCommandResult();
                    ItemSelecionado = new ItemCommandResult();
                }

                if (Session.ATENDIMENTO_ATUAL != null)
                {
                    var listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1", null, null, null, Session.USUARIO_LOGADO.CodPessoa));
                    if (listCarrinhos?.Count > 0)
                    {
                        foreach (var atualizaCarrinhos in listCarrinhos)
                        {
                            var atualizouCarrinho = await _carrinhoRepository.AtualizaQtdCarrinho(atualizaCarrinhos.CodCarrinho);
                        }

                        listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1", null, null, null, Session.USUARIO_LOGADO.CodPessoa));

                        foreach (var item in listCarrinhos)
                        {
                            Pedidos.Add(item);
                        }
                        PedidoSelecionado = Pedidos.FirstOrDefault();
                        ItemSelecionado = PedidoSelecionado.Itens.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"{ex.Message}", AppName, "OK");
            }

        }

        private void PedidoTapped(object Item)
        {
            if (Item == null) return;

            PedidoSelecionado = Item as CarrinhoCommandResult;
            PedidoSelecionado.CarrinhoChecado = !PedidoSelecionado.CarrinhoChecado;
            ItemSelecionado = PedidoSelecionado.Itens.FirstOrDefault();
        }

        private void ItemTapped(object Item)
        {
            if (Item == null) return;

            ItemSelecionado = Item as ItemCommandResult;
        }

        private async void CopiarCarrinho(object obj)
        {
            try
            {
                ClienteCommandResult ClienteSelecionado = (obj as ClienteCommandResult);
                CriarAtendimentoCommand atendimento = new CriarAtendimentoCommand(ClienteSelecionado.CodPessoaCliente,
                                                    Session.USUARIO_LOGADO.CodUsuario, Session.USUARIO_LOGADO.CodMarca, Session.USUARIO_LOGADO.CodInstalacao,
                                                    ClienteSelecionado.Apelido, 1, null, null, null, null, null, 0, 0, null, null, 0, 0, 0, 0, PedidoSelecionado.TipoPedido);

                var command = new CopiarCarrinhoCommand()
                {
                    CodCarrinhoOrigem = PedidoSelecionado.CodCarrinho,
                    CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
                    Usuario = Session.USUARIO_LOGADO,
                    NovoAtendimento = atendimento
                };

                var result = await _carrinhoHandler.Handle(command) as HandlerResult;
                if (result.Sucesso)
                {
                    await UserDialogs.Instance.AlertAsync($"Carrinho copiado com sucesso. Carrinho {result.Result}", AppName, "OK");
                }
                else
                {
                    string message = string.Join("\n", result.ListaErros);
                    await UserDialogs.Instance.AlertAsync($"Não foi possível copiar o pedido. {message}", AppName, "OK");
                }

                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"Não foi possível copiar o pedido. {ex.Message}", AppName, "OK");
            }
        }

        private async void Imprimir(CarrinhoCommandResult obj)
        {
            try
            {
                if (!Pedidos.Any(x => x.CarrinhoChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos '1' pedido para imprimir", AppName, "OK");
                    return;
                }
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupImpressao(Pedidos.Where(x => x.CarrinhoChecado).ToList()));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void Cancelar()
        {
            try
            {
                if (!Pedidos.Any(x => x.CarrinhoChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 pedido para Cancelar", AppName, "OK");
                    return;
                }

                var confirm = await UserDialogs.Instance.ConfirmAsync($"Deseja realmente cancelar o(s) pedido(s) selecionado(s)?", "Cancelar", "Sim", "Não");
                if (!confirm)
                {
                    return;
                }

                List<HandlerResult> results = new List<HandlerResult>();
                foreach (var item in Pedidos.Where(x => x.CarrinhoChecado))
                {
                    if (item.IdServidor > 0)
                    {
                        var resultTransmissao = await ServiceUtility.CancelarPedido(_carrinhoRepository, _parametroSincronizacaRepository, item.CodCarrinho);
                        if (resultTransmissao.SUCCESS.ToString().ToUpper() == "FALSE")
                        {
                            if (resultTransmissao.EXCEPTION.ToString() == "Carrinho não encontrado! ")
                            {
                                var result = await _carrinhoHandler.Handle(new CancelarCarrinhoCommand(item.CodCarrinho, Session.ATENDIMENTO_ATUAL.CodAtendimento)) as HandlerResult;
                                results.Add(result);
                            }
                            else
                            {
                                await UserDialogs.Instance.AlertAsync($"Não foi possível cancelar o carrinho {item.CodCarrinho}, tente novamente mais tarde.", AppName, "OK");
                                return;
                            }
                        }
                        else
                        {
                            var result = await _carrinhoHandler.Handle(new CancelarCarrinhoCommand(item.CodCarrinho, Session.ATENDIMENTO_ATUAL.CodAtendimento)) as HandlerResult;
                            results.Add(result);
                        }
                    }
                    else
                    {
                        var result = await _carrinhoHandler.Handle(new CancelarCarrinhoCommand(item.CodCarrinho, Session.ATENDIMENTO_ATUAL.CodAtendimento)) as HandlerResult;
                        results.Add(result);
                    }
                }

                //if (!Pedidos.Any(x => !x.CarrinhoChecado)
                //    && !results.Any(x => !x.Sucesso)
                //    )
                //{
                //    MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
                //    Session.ATENDIMENTO_ATUAL = null;
                //}

                if (results.Any(x => !x.Sucesso))
                {
                    string message = string.Empty;
                    foreach (var item in results.Where(x => !x.Sucesso))
                    {
                        message += string.Join("\n", item.ListaErros);
                    }

                    await UserDialogs.Instance.AlertAsync($"Não foi possível cancelar o(os) pedido(os). {message}", AppName, "OK");
                }

                await UserDialogs.Instance.AlertAsync("Carrinho cancelado com sucesso.", AppName, "OK");
                await Load();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"Não foi possível cancelar o pedido. {ex.Message}", AppName, "OK");
            }

        }

        private async void Editar(CarrinhoCommandResult obj)
        {
            try
            {
                if (PreventDoubleClick.VerifyClickInterval() <= 0)
                {
                    return;
                }


                if (Pedidos.Where(x => x.CarrinhoChecado).Count() != 1)
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar 1 pedido para editar", AppName, "OK");
                    return;
                }

                PedidoSelecionado = Pedidos.Where(x => x.CarrinhoChecado).FirstOrDefault();
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupFechamento(PedidoSelecionado));

            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void ImportarPlanilha(CarrinhoCommandResult obj)
        {
            try
            {
                if (PreventDoubleClick.VerifyClickInterval() <= 0)
                {
                    return;
                }

                string liberaImportacao = await _parametroRepository.BuscarValorParametro(ParametrosSistema.LIBERA);
                string usuarioLiberadoTeste = await _parametroRepository.BuscarValorParametro(ParametrosSistema.USUARIOPARAMETRO);
                if (liberaImportacao == "N" && Session.USUARIO_LOGADO.CodPessoa != usuarioLiberadoTeste)
                {
                    await UserDialogs.Instance.AlertAsync("Este recurso não está habilitado", AppName, "OK");
                    return;
                }

                if (Device.RuntimePlatform != Device.UWP)
                {
                    await UserDialogs.Instance.AlertAsync("Este recurso está habilitado somente para ambiente Windows", AppName, "OK");
                    return;
                }

                if (Session.ATENDIMENTO_ATUAL == null)
                {
                    await UserDialogs.Instance.AlertAsync("Você precisa de um atendimento em aberto para importar uma planilha", AppName, "OK");
                    return;
                }
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupImportarPlanilha());

            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void Transmitir(CarrinhoCommandResult obj)
        {
            try
            {
                if (!Pedidos.Any(x => x.CarrinhoChecado))
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 pedido para transmitir", AppName, "OK");
                    return;
                }

                var confirm = await UserDialogs.Instance.ConfirmAsync($"Deseja realmente transmitir o(os) pedido(os) selecionado(os)?", "Transmitir", "Sim", "Não");
                if (!confirm)
                {
                    return;
                }

                List<string> lstPedidosComErros = new List<string>();
                List<string> lstPedidosComErrosTransmissao = new List<string>();
                List<string> lstPedidosSemErros = new List<string>();
                List<string> itensPoliticaEscolar = new List<string>();
                List<string> itensSemPoliticaEscolar = new List<string>();
                List<WcfModelResult> lstPedidosModel = new List<WcfModelResult>();
                foreach (var pedido in Pedidos.Where(x => x.CarrinhoChecado))
                {

                    string validaValorZerado = await _parametroRepository.BuscarValorParametro(ParametrosSistema.VALIDAZERADO);
                    if (validaValorZerado == "S")
                    {

                        //valida se ha itens sem preço no carrinho.
                        var buscarItemCommand = new BuscarItensCarrinhoCommand() { CodCarrinho = pedido.CodCarrinho };
                        var itens = await _carrinhoRepository.BuscarItensCarrinho(buscarItemCommand);

                        if (itens.Count > 0)
                        {

                            string message = "";
                            foreach (var item in itens)
                            {
                                if (item.ValorUnitario == 0)
                                {
                                    message += $"Item {item.CodItemCarrinho} Produto {item.CodProduto} está com o valor zerado, favor corrigir o item para reenviar o pedido.\n";
                                }

                            }
                            if (message.Length > 1)
                            {
                                await UserDialogs.Instance.AlertAsync($"Pedido {pedido.CodCarrinho} com erro na transmissão.\n" + message, AppName, "OK");
                                return;
                            }
                        }
                    }


                    var result = await PedidoEValido(pedido);
                    if (!result.Sucesso)
                    {
                        lstPedidosComErros.Add(pedido.CodCarrinho);
                        lstPedidosComErros.AddRange(result.ListaErros);
                        continue;
                    }

                    var clienteAtendimento = await _clienteRepository.BuscarClientePorCode(new BuscarClienteCommand(null, null, Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

                    UserDialogs.Instance.ShowLoading($"Transmitindo o cadastro do cliente novo {pedido.CodPessoaCliente} - {pedido.RazaoSocial}");
                    if (pedido.CodPessoaCliente.Contains("."))
                    {
                        var clienteERP = await _clienteRepository.BuscarClienteIntegrado(pedido.CNPJ);
                        if (clienteERP == null)
                        {
                            var resultTransmissaoCliente = await ServiceUtility.TransmitirCliente(_clienteRepository, _parametroSincronizacaRepository, pedido.CodPessoaCliente);
                            if (resultTransmissaoCliente.SUCCESS.ToString().ToUpper() == "TRUE")
                            {
                                await _clienteRepository.AtualizarClienteIntegrado(pedido.CodPessoaCliente, resultTransmissaoCliente.CODIGO.ToString());
                                if (!string.IsNullOrEmpty(resultTransmissaoCliente.CODIGO.ToString()))
                                {
                                    //Atualiza o carrinho com o codigo do cliente do erp, para não dar problema de integração.
                                    //await _clienteRepository.AtualizarClienteNoCarrinho(pedido.CodCarrinho, resultTransmissaoCliente.CODIGO.ToString());
                                }
                            }
                            else
                            {
                                if (resultTransmissaoCliente.CODMENSAGEM.ToString().ToUpper() == "ITM002")
                                {
                                    lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
                                    lstPedidosComErrosTransmissao.Add($"Cliente {clienteAtendimento.RazaoSocial}, já esta cadastrado na Pegada, sincronize o app para receber o cliente ou entre em contato com a Pegada.");
                                    continue;
                                }
                                else
                                {

                                    lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
                                    lstPedidosComErrosTransmissao.Add("Erro na transmissão do cliente novo: EX: " + resultTransmissaoCliente.EXCEPTION.ToString());
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(clienteERP.CodPessoaCliente))
                            {
                                string prefix = clienteERP.CodPessoaCliente.Substring(1);
                                if (prefix != "C")
                                {
                                    await _clienteRepository.AtualizarClienteIntegrado(pedido.CodPessoaCliente, clienteERP.CodPessoaCliente);
                                }
                            }
                        }
                    }
                    UserDialogs.Instance.HideLoading();
                    bool precisaAprovacao = false;

                    //####################################################################################
                    //Regra Politica Escolar
                    //Verifica a data em digitação com a data de hoje
                    bool temCondicao = false;

                    var dataHoje = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    var dataPoliticaEscolar = await _politicaComercialRepository.BuscarDataPoliticaEscolar(dataHoje);

                    //verifica se existe alguma vigente
                    if (dataPoliticaEscolar.Count > 0)
                    {
                        //dentre as vigentes, verificar se escolheu é MiniVA(Mini Voltas Aulas).
                        var politicaMiniVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento == pedido.CodCondicaoPagamento && x.CodCondicaoPagamento == "MV3").FirstOrDefault();

                        //se escolheu
                        if (politicaMiniVA != null)
                        {
                            //valida se o carrinho esta certo
                            var validaItensMiniVA = await _carrinhoRepository.GetItensPoliticaMiniVACarrinho(pedido.CodCarrinho);
                            //se não tiver, informa
                            if (!validaItensMiniVA)
                            {
                                await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
                                return;
                            }
                        }

                        //dentre as vigentes, verificar se escolheu é VA(Voltas Aulas).
                        var politicaVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento.Contains(pedido.CodCondicaoPagamento)).FirstOrDefault();

                        //se escolheu
                        if (politicaVA != null)
                        {
                            //valida se o carrinho esta certo
                            var validaItensVA = await _carrinhoRepository.GetItensPoliticaVACarrinho(pedido.CodCarrinho);
                            //se não tiver, informa
                            if (!validaItensVA)
                            {
                                await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
                                return;
                            }
                        }
                    }


                    //bool isCodPoliticaAtualizado = false;
                    //var codPoliticaComercialUtilizado = string.Empty;
                    //if(dataPoliticaEscolar.Count > 0)
                    //{
                    //    codPoliticaComercialUtilizado = dataPoliticaEscolar[0].CodPoliticaComercial;
                    //}
                    //else
                    //{
                    //    codPoliticaComercialUtilizado = string.Empty;
                    //}


                    //if (dataPoliticaEscolar.Count > 0)
                    //{
                    //    //Verifica se a categoria está inclusa na TBT_POLITICA_COMERCIAL_NIVEL
                    //    var detalhesDoProduto = await _nivelRepository.GetNiveisProduto(item.CodProduto);
                    //    var categoriaDoProduto = detalhesDoProduto[1].CodAtributo;
                    //    var infoPoliticaComercialNiveis = await _politicaComercialRepository.BuscarPoliticaEscolarNiveis(categoriaDoProduto, codPoliticaComercialUtilizado);

                    //    if (infoPoliticaComercialNiveis.Count > 0)
                    //    {
                    //        itensPoliticaEscolar.Add(item.Descricao);

                    //        string condPagamentoPoliticaEscolar = dataPoliticaEscolar[0].CodCondicaoPagamento;
                    //        var condPagamento = condPagamentoPoliticaEscolar.Split(',');

                    //        foreach (string cond in condPagamento)
                    //        {
                    //            temCondicao = pedido.CodCondicaoPagamento.Equals(cond);
                    //            if (temCondicao)
                    //                break;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        itensSemPoliticaEscolar.Add(item.Descricao);
                    //    }

                    //}
                    //if (itensPoliticaEscolar.Count > 0 && itensSemPoliticaEscolar.Count > 0)
                    //{
                    //    string mensagem = "O(s) item(s) ";
                    //    foreach (var msgitem in itensSemPoliticaEscolar)
                    //    {
                    //        mensagem += msgitem.ToString() + ", ";
                    //    }
                    //    mensagem += "não estão vinculados a Política Escolar. Para continuar, faça o desmembramento de itens!";

                    //    await UserDialogs.Instance.AlertAsync(mensagem, AppName, "OK");
                    //    return;
                    //}
                    //else if (itensPoliticaEscolar.Count > 0 && itensSemPoliticaEscolar.Count == 0 && temCondicao)
                    //{
                    //    if (isCodPoliticaAtualizado == false)
                    //    {
                    //        isCodPoliticaAtualizado = await _carrinhoRepository.AtualizarCodPoliticaComercialCarrinho(pedido.CodCarrinho, codPoliticaComercialUtilizado) == true;
                    //    }
                    //}
                    //####################################################################################
                    //valida desconto
                    foreach (var item in pedido.Itens)
                    {
                        if (item.PercDesc > 0)
                        {
                            var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorCliente(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", clienteAtendimento.CodPessoaCliente, item.CodProduto));

                            if (coefiDesconto != null)
                            {

                                TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = pedido.CodCondicaoPagamento });
                                string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

                                var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", clienteAtendimento.CodigoSegmento, item.CodProduto, prazoMedio));

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
                                var des = item.PercDesc / 100;
                                if (des > coeficiente)
                                {
                                    precisaAprovacao = true;
                                    break;
                                }
                            }
                            else
                            {
                                await UserDialogs.Instance.AlertAsync("Não foi encontrado um segmento válido no cliente para o desconto. Sincronize ou entre em contato com a Pegada.", AppName, "OK");
                                precisaAprovacao = true;
                                break;
                            }

                        }
                    }


                    if (precisaAprovacao)
                    {
                        List<string> listCarrinhoPendente = new List<string>();
                        var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto do pedido { pedido.CodCarrinho} é maior que o desconto maximo, seu pedido será enviado para aprovação.", "Deseja continuar", "Sim", "Não");
                        if (!desconto)
                        {
                            return;
                        }
                        else
                        {
                            UserDialogs.Instance.ShowLoading($"Transmitindo o pedido {pedido.CodCarrinho}");
                            var resultTransmissao = await ServiceUtility.TransmitirPedidoAprovacao(_carrinhoRepository, _parametroSincronizacaRepository, pedido.CodCarrinho);
                            if (resultTransmissao.SUCCESS.ToString().ToUpper() == "FALSE")
                            {
                                lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
                                lstPedidosComErrosTransmissao.Add(resultTransmissao.EXCEPTION.ToString());
                            }
                            else
                            {
                                lstPedidosSemErros.Add(pedido.CodCarrinho);
                                lstPedidosSemErros.Add(resultTransmissao.CODIGO.ToString());

                                //SALVA CARRINHO TRANSMITIDO PARA ABATIMENTO DE ESTOQUE
                                await _carrinhoRepository.CadastraCarrinhoHistorico(pedido.CodCarrinho);

                                resultTransmissao.CodCarrinho = pedido.CodCarrinho;
                                lstPedidosModel.Add(resultTransmissao);
                                listCarrinhoPendente.Add(pedido.CodCarrinho);
                            }
                        }

                        var service = new Pegada.Core.Services.ServiceUtility();
                        var erros = await service.EnviarEmailAprovacao(listCarrinhoPendente);
                        if (erros.Count() > 0)
                        {
                            // throw new Exception("Pedidos com erros no envio de e-mail.\n" + erros);
                            string message = string.Join("\n", erros);
                            await UserDialogs.Instance.AlertAsync("Erro no envio de e-mail.\n" + message, AppName, "OK");
                        }

                    }
                    else
                    {
                        UserDialogs.Instance.ShowLoading($"Transmitindo o pedido {pedido.CodCarrinho}");
                        var resultTransmissao = await ServiceUtility.TransmitirPedido(_carrinhoRepository, _parametroSincronizacaRepository, pedido.CodCarrinho);
                        if (resultTransmissao.SUCCESS.ToString().ToUpper() == "FALSE")
                        {
                            lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
                            lstPedidosComErrosTransmissao.Add(resultTransmissao.EXCEPTION.ToString());
                        }
                        else
                        {
                            lstPedidosSemErros.Add(pedido.CodCarrinho);
                            lstPedidosSemErros.Add(resultTransmissao.CODIGO.ToString());

                            //SALVA CARRINHO TRANSMITIDO PARA ABATIMENTO DE ESTOQUE
                            await _carrinhoRepository.CadastraCarrinhoHistorico(pedido.CodCarrinho);

                            resultTransmissao.CodCarrinho = pedido.CodCarrinho;
                            lstPedidosModel.Add(resultTransmissao);
                        }
                    }
                }

                UserDialogs.Instance.HideLoading();

                if (lstPedidosComErros.Count() > 0)
                {
                    string message = string.Join("\n", lstPedidosComErros);
                    await UserDialogs.Instance.AlertAsync("Pedidos incompletos.\n" + message, AppName, "OK");
                }


                if (lstPedidosComErrosTransmissao.Count() > 0)
                {
                    string message = string.Join("\n", lstPedidosComErrosTransmissao);
                    await UserDialogs.Instance.AlertAsync("Pedidos com erro na transmissão.\n" + message, AppName, "OK");
                }

                if (lstPedidosSemErros.Count() > 0)
                {

                    foreach (var result in lstPedidosModel)
                    {
                        await _carrinhoRepository.AtualizarPedidoImplantado(result.CODIGO.ToString(), result.SITUACAO.ToString(), result.CodCarrinho);

                        //Atendimento não tem mais carrinho em edição,
                        //devemos fechar.
                        var carrinhoEmDigitacao = await _atendimentoRepository.BuscarCarrinhosAbertos(Session.ATENDIMENTO_ATUAL.CodAtendimento);
                        if (carrinhoEmDigitacao == null || carrinhoEmDigitacao.Count == 0)
                        {
                            await _atendimentoRepository.FecharAtendimento(Session.ATENDIMENTO_ATUAL.CodAtendimento);
                            Session.ATENDIMENTO_ATUAL = null;
                        }
                    }

                    string message = string.Join("Pedido(s) enviado(s) com sucesso.\n", lstPedidosSemErros);
                    await UserDialogs.Instance.AlertAsync(message, AppName);
                    await Load();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
        }


        //versão 35446
        //private async void Transmitir(CarrinhoCommandResult obj)
        //{
        //    try
        //    {
        //        if (!Pedidos.Any(x => x.CarrinhoChecado))
        //        {
        //            await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 1 pedido para transmitir", AppName, "OK");
        //            return;
        //        }

        //        var confirm = await UserDialogs.Instance.ConfirmAsync($"Deseja realmente transmitir o(os) pedido(os) selecionado(os)?", "Transmitir", "Sim", "Não");
        //        if (!confirm)
        //        {
        //            return;
        //        }

        //        List<string> lstPedidosComErros = new List<string>();
        //        List<string> lstPedidosComErrosTransmissao = new List<string>();
        //        List<string> lstPedidosSemErros = new List<string>();
        //        List<string> itensPoliticaEscolar = new List<string>();
        //        List<string> itensSemPoliticaEscolar = new List<string>();
        //        List<WcfModelResult> lstPedidosModel = new List<WcfModelResult>();
        //        foreach (var pedido in Pedidos.Where(x => x.CarrinhoChecado))
        //        {

        //            string validaValorZerado = await _parametroRepository.BuscarValorParametro(ParametrosSistema.VALIDAZERADO);
        //            if (validaValorZerado == "S") {

        //                //valida se ha itens sem preço no carrinho.
        //                var buscarItemCommand = new BuscarItensCarrinhoCommand() { CodCarrinho = pedido.CodCarrinho };
        //                var itens = await _carrinhoRepository.BuscarItensCarrinho(buscarItemCommand);

        //                if (itens.Count > 0)
        //                {

        //                    string message = "";
        //                    foreach (var item in itens)
        //                    {
        //                        if (item.ValorUnitario == 0)
        //                        {
        //                            message += $"Item {item.CodItemCarrinho} Produto {item.CodProduto} está com o valor zerado, favor corrigir o item para reenviar o pedido.\n";
        //                        }

        //                    }
        //                    if (message.Length > 1)
        //                    {
        //                        await UserDialogs.Instance.AlertAsync($"Pedido {pedido.CodCarrinho} com erro na transmissão.\n" + message, AppName, "OK");
        //                        return;
        //                    }
        //                }
        //            }


        //            var result = await PedidoEValido(pedido);
        //            if (!result.Sucesso)
        //            {
        //                lstPedidosComErros.Add(pedido.CodCarrinho);
        //                lstPedidosComErros.AddRange(result.ListaErros);
        //                continue;
        //            }

        //            var clienteAtendimento = await _clienteRepository.BuscarClientePorCode(new BuscarClienteCommand(null, null, Session.ATENDIMENTO_ATUAL.CodPessoaCliente, null));

        //            UserDialogs.Instance.ShowLoading($"Transmitindo o cadastro do cliente novo {pedido.CodPessoaCliente} - {pedido.RazaoSocial}");
        //            if (pedido.CodPessoaCliente.Contains("."))
        //            {
        //                var clienteERP = await _clienteRepository.BuscarClienteIntegrado(pedido.CNPJ);
        //                if (clienteERP == null)
        //                {
        //                    var resultTransmissaoCliente = await ServiceUtility.TransmitirCliente(_clienteRepository, _parametroSincronizacaRepository, pedido.CodPessoaCliente);
        //                    if (resultTransmissaoCliente.SUCCESS.ToString().ToUpper() == "TRUE")
        //                    {
        //                        await _clienteRepository.AtualizarClienteIntegrado(pedido.CodPessoaCliente, resultTransmissaoCliente.CODIGO.ToString());
        //                        if (!string.IsNullOrEmpty(resultTransmissaoCliente.CODIGO.ToString()))
        //                        {
        //                            //Atualiza o carrinho com o codigo do cliente do erp, para não dar problema de integração.
        //                            //await _clienteRepository.AtualizarClienteNoCarrinho(pedido.CodCarrinho, resultTransmissaoCliente.CODIGO.ToString());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (resultTransmissaoCliente.CODMENSAGEM.ToString().ToUpper() == "ITM002")
        //                        {
        //                            lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
        //                            lstPedidosComErrosTransmissao.Add($"Cliente {clienteAtendimento.RazaoSocial}, já esta cadastrado na Pegada, sincronize o app para receber o cliente ou entre em contato com a Pegada.");
        //                            continue;
        //                        }
        //                        else
        //                        {

        //                            lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
        //                            lstPedidosComErrosTransmissao.Add("Erro na transmissão do cliente novo: EX: " + resultTransmissaoCliente.EXCEPTION.ToString());
        //                            continue;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(clienteERP.CodPessoaCliente))
        //                    {
        //                        string prefix = clienteERP.CodPessoaCliente.Substring(1);
        //                        if (prefix != "C")
        //                        {
        //                            await _clienteRepository.AtualizarClienteIntegrado(pedido.CodPessoaCliente, clienteERP.CodPessoaCliente);
        //                        }
        //                    }
        //                }
        //            }
        //            UserDialogs.Instance.HideLoading();
        //            bool precisaAprovacao = false;

        //            //####################################################################################
        //            //Regra Politica Escolar
        //            //Verifica a data em digitação com a data de hoje
        //            bool temCondicao = false;

        //            var dataHoje = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        //            var dataPoliticaEscolar = await _politicaComercialRepository.BuscarDataPoliticaEscolar(dataHoje);

        //            //verifica se existe alguma vigente
        //            if (dataPoliticaEscolar.Count > 0)
        //            {
        //                //dentre as vigentes, verificar se escolheu é MiniVA(Mini Voltas Aulas).
        //                var politicaMiniVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento == pedido.CodCondicaoPagamento && x.CodCondicaoPagamento == "MV3").FirstOrDefault();

        //                //se escolheu
        //                if (politicaMiniVA != null)
        //                {
        //                    //valida se o carrinho esta certo
        //                    var validaItensMiniVA = await _carrinhoRepository.GetItensPoliticaMiniVACarrinho(pedido.CodCarrinho);
        //                    //se não tiver, informa
        //                    if (!validaItensMiniVA)
        //                    {
        //                        await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
        //                        return;
        //                    }
        //                }

        //                //dentre as vigentes, verificar se escolheu é VA(Voltas Aulas).
        //                var politicaVA = dataPoliticaEscolar.Where(x => x.CodCondicaoPagamento.Contains(pedido.CodCondicaoPagamento)).FirstOrDefault();

        //                //se escolheu
        //                if (politicaVA != null)
        //                {
        //                    //valida se o carrinho esta certo
        //                    var validaItensVA = await _carrinhoRepository.GetItensPoliticaVACarrinho(pedido.CodCarrinho);
        //                    //se não tiver, informa
        //                    if (!validaItensVA)
        //                    {
        //                        await UserDialogs.Instance.AlertAsync($"A condição de pagamento escolhida não pode ser utilizada no carrinho {PedidoSelecionado.CodCarrinho}, por conta da política comercial vigente, favor escolher outra condição de pagamento");
        //                        return;
        //                    }
        //                }
        //            }


        //                //bool isCodPoliticaAtualizado = false;
        //                //var codPoliticaComercialUtilizado = string.Empty;
        //                //if(dataPoliticaEscolar.Count > 0)
        //                //{
        //                //    codPoliticaComercialUtilizado = dataPoliticaEscolar[0].CodPoliticaComercial;
        //                //}
        //                //else
        //                //{
        //                //    codPoliticaComercialUtilizado = string.Empty;
        //                //}


        //                //if (dataPoliticaEscolar.Count > 0)
        //                //{
        //                //    //Verifica se a categoria está inclusa na TBT_POLITICA_COMERCIAL_NIVEL
        //                //    var detalhesDoProduto = await _nivelRepository.GetNiveisProduto(item.CodProduto);
        //                //    var categoriaDoProduto = detalhesDoProduto[1].CodAtributo;
        //                //    var infoPoliticaComercialNiveis = await _politicaComercialRepository.BuscarPoliticaEscolarNiveis(categoriaDoProduto, codPoliticaComercialUtilizado);

        //                //    if (infoPoliticaComercialNiveis.Count > 0)
        //                //    {
        //                //        itensPoliticaEscolar.Add(item.Descricao);

        //                //        string condPagamentoPoliticaEscolar = dataPoliticaEscolar[0].CodCondicaoPagamento;
        //                //        var condPagamento = condPagamentoPoliticaEscolar.Split(',');

        //                //        foreach (string cond in condPagamento)
        //                //        {
        //                //            temCondicao = pedido.CodCondicaoPagamento.Equals(cond);
        //                //            if (temCondicao)
        //                //                break;
        //                //        }
        //                //    }
        //                //    else
        //                //    {
        //                //        itensSemPoliticaEscolar.Add(item.Descricao);
        //                //    }

        //                //}
        //                //if (itensPoliticaEscolar.Count > 0 && itensSemPoliticaEscolar.Count > 0)
        //                //{
        //                //    string mensagem = "O(s) item(s) ";
        //                //    foreach (var msgitem in itensSemPoliticaEscolar)
        //                //    {
        //                //        mensagem += msgitem.ToString() + ", ";
        //                //    }
        //                //    mensagem += "não estão vinculados a Política Escolar. Para continuar, faça o desmembramento de itens!";

        //                //    await UserDialogs.Instance.AlertAsync(mensagem, AppName, "OK");
        //                //    return;
        //                //}
        //                //else if (itensPoliticaEscolar.Count > 0 && itensSemPoliticaEscolar.Count == 0 && temCondicao)
        //                //{
        //                //    if (isCodPoliticaAtualizado == false)
        //                //    {
        //                //        isCodPoliticaAtualizado = await _carrinhoRepository.AtualizarCodPoliticaComercialCarrinho(pedido.CodCarrinho, codPoliticaComercialUtilizado) == true;
        //                //    }
        //                //}
        //                //####################################################################################
        //                //valida desconto
        //            foreach (var item in pedido.Itens)
        //            {
        //                if (item.PercDesc > 0)
        //                {
        //                    var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorProduto(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", clienteAtendimento.CodigoSegmento, item.CodProduto));

        //                    if (coefiDesconto != null)
        //                    {

        //                        TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = pedido.CodCondicaoPagamento });
        //                        string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

        //                        var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", clienteAtendimento.CodigoSegmento, item.CodProduto, prazoMedio));

        //                        decimal coeficiente = coefiDesconto.Coeficiente;
        //                        if (coefiPrazoMedio != null)
        //                        {
        //                            if (coefiPrazoMedio.Coeficiente > 0)
        //                            {
        //                                if (Convert.ToDecimal(prazoMedio) >= 60)
        //                                {
        //                                    if (!string.IsNullOrEmpty(clienteAtendimento.Prazo))
        //                                    {
        //                                        if (Convert.ToDecimal(prazoMedio) > Convert.ToDecimal(clienteAtendimento.Prazo))
        //                                        {
        //                                            coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                                        }
        //                                        else
        //                                        {
        //                                            coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                                }
        //                            }
        //                        }
        //                        var des = item.PercDesc / 100;
        //                        if (des > coeficiente)
        //                        {
        //                            precisaAprovacao = true;
        //                            break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        await UserDialogs.Instance.AlertAsync("Não foi encontrado um segmento válido no cliente para o desconto. Sincronize ou entre em contato com a Pegada.", AppName, "OK");
        //                        precisaAprovacao = true;
        //                        break;
        //                    }

        //                } 
        //            }


        //            if (precisaAprovacao)
        //            {
        //                List<string> listCarrinhoPendente = new List<string>();
        //                var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto do pedido { pedido.CodCarrinho} é maior que o desconto maximo, seu pedido será enviado para aprovação.", "Deseja continuar", "Sim", "Não");
        //                if (!desconto)
        //                {
        //                    return;
        //                }
        //                else
        //                {
        //                    UserDialogs.Instance.ShowLoading($"Transmitindo o pedido {pedido.CodCarrinho}");
        //                    var resultTransmissao = await ServiceUtility.TransmitirPedidoAprovacao(_carrinhoRepository, _parametroSincronizacaRepository, pedido.CodCarrinho);
        //                    if (resultTransmissao.SUCCESS.ToString().ToUpper() == "FALSE")
        //                    {
        //                        lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
        //                        lstPedidosComErrosTransmissao.Add(resultTransmissao.EXCEPTION.ToString());
        //                    }
        //                    else
        //                    {
        //                        lstPedidosSemErros.Add(pedido.CodCarrinho);
        //                        lstPedidosSemErros.Add(resultTransmissao.CODIGO.ToString());

        //                        //SALVA CARRINHO TRANSMITIDO PARA ABATIMENTO DE ESTOQUE
        //                        await _carrinhoRepository.CadastraCarrinhoHistorico(pedido.CodCarrinho);

        //                        resultTransmissao.CodCarrinho = pedido.CodCarrinho;
        //                        lstPedidosModel.Add(resultTransmissao);
        //                        listCarrinhoPendente.Add(pedido.CodCarrinho);
        //                    }
        //                }

        //                var service = new Pegada.Core.Services.ServiceUtility();
        //                var erros = await service.EnviarEmailAprovacao(listCarrinhoPendente);
        //                if (erros.Count() > 0)
        //                {
        //                    // throw new Exception("Pedidos com erros no envio de e-mail.\n" + erros);
        //                    string message = string.Join("\n", erros);
        //                    await UserDialogs.Instance.AlertAsync("Erro no envio de e-mail.\n" + message, AppName, "OK");
        //                }

        //            }
        //            else
        //            {
        //                UserDialogs.Instance.ShowLoading($"Transmitindo o pedido {pedido.CodCarrinho}");
        //                var resultTransmissao = await ServiceUtility.TransmitirPedido(_carrinhoRepository, _parametroSincronizacaRepository, pedido.CodCarrinho);
        //                if (resultTransmissao.SUCCESS.ToString().ToUpper() == "FALSE")
        //                {
        //                    lstPedidosComErrosTransmissao.Add(pedido.CodCarrinho);
        //                    lstPedidosComErrosTransmissao.Add(resultTransmissao.EXCEPTION.ToString());
        //                }
        //                else
        //                {
        //                    lstPedidosSemErros.Add(pedido.CodCarrinho);
        //                    lstPedidosSemErros.Add(resultTransmissao.CODIGO.ToString());

        //                    //SALVA CARRINHO TRANSMITIDO PARA ABATIMENTO DE ESTOQUE
        //                    await _carrinhoRepository.CadastraCarrinhoHistorico(pedido.CodCarrinho);

        //                    resultTransmissao.CodCarrinho = pedido.CodCarrinho;
        //                    lstPedidosModel.Add(resultTransmissao);
        //                }
        //            }
        //        }

        //        UserDialogs.Instance.HideLoading();

        //        if (lstPedidosComErros.Count() > 0)
        //        {
        //            string message = string.Join("\n", lstPedidosComErros);
        //            await UserDialogs.Instance.AlertAsync("Pedidos incompletos.\n" + message, AppName, "OK");
        //        }


        //        if (lstPedidosComErrosTransmissao.Count() > 0)
        //        {
        //            string message = string.Join("\n", lstPedidosComErrosTransmissao);
        //            await UserDialogs.Instance.AlertAsync("Pedidos com erro na transmissão.\n" + message, AppName, "OK");
        //        }

        //        if (lstPedidosSemErros.Count() > 0)
        //        {

        //            foreach (var result in lstPedidosModel)
        //            {
        //                await _carrinhoRepository.AtualizarPedidoImplantado(result.CODIGO.ToString(), result.SITUACAO.ToString(), result.CodCarrinho);

        //                //Atendimento não tem mais carrinho em edição,
        //                //devemos fechar.
        //                var carrinhoEmDigitacao = await _atendimentoRepository.BuscarCarrinhosAbertos(Session.ATENDIMENTO_ATUAL.CodAtendimento);
        //                if (carrinhoEmDigitacao == null || carrinhoEmDigitacao.Count == 0)
        //                {
        //                    await _atendimentoRepository.FecharAtendimento(Session.ATENDIMENTO_ATUAL.CodAtendimento);
        //                    Session.ATENDIMENTO_ATUAL = null;
        //                }
        //            }

        //            string message = string.Join("Pedido(s) enviado(s) com sucesso.\n", lstPedidosSemErros);
        //            await UserDialogs.Instance.AlertAsync(message, AppName);
        //            await Load();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        UserDialogs.Instance.HideLoading();
        //        await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
        //    }
        //}
        private async Task<HandlerResult> PedidoEValido(CarrinhoCommandResult pedido)
        {
            HandlerResult result = new HandlerResult(true);

            //validar o cupom
            if (!String.IsNullOrEmpty(pedido.CupomChave))
            {
                WcfValidarCupomInput input = new WcfValidarCupomInput(PedidoSelecionado.CupomChave, PedidoSelecionado.CodCarrinho, PedidoSelecionado.CodPessoaCliente, PedidoSelecionado.CodUsuario.ToString());
                WcfModelResult cupomResult = await ServiceUtility.ValidarCupom(input);

                bool sucesso = cupomResult.SUCCESS.ToString().ToLower().Equals("true");
                string chaveCupom = cupomResult.CODIGO.ToString();
                string mensagem = cupomResult.CODMENSAGEM.ToString();

                if (!sucesso)
                {
                    result.ListaErros.Add($"Cupom inválido");
                    result.Sucesso = false;
                    return result;
                }
            }

            /*Validação de campos*/
            if (string.IsNullOrEmpty(pedido.CodCondicaoPagamento))
                result.ListaErros.Add($"Condição de pagamento inválida");

            if (pedido.DataEntrega == null || pedido.DataEntrega <= DateTime.Now.Date)
                result.ListaErros.Add($"Data de entrega inválida.");

            if (result.ListaErros.Count > 0)
            {
                result.Sucesso = result.ListaErros.Count == 0;
                return result;
            }

            bool isCarrinhoAlocado = false;
            if (pedido.Itens.Count > 0) {

                string codCarrinhoAlocado = pedido.Itens[0].CodCarrinhoAlocado;
                if (!string.IsNullOrEmpty(codCarrinhoAlocado)) {
                    isCarrinhoAlocado = true;
                }
            }

            //tarefa 31823
            if (isCarrinhoAlocado == false) {
                /*Validação de regras de negócio*/
                decimal valorMinimo = await _parametroRepository.BuscarMinimoPorTipoPedido(pedido.CodTipoPedido);
                var minimoDuplicata = await _parametroRepository.BuscarValorParametro(ParametrosSistema.MINIMODUPLICATA);
                decimal valorMinimoDuplicata = !string.IsNullOrEmpty(minimoDuplicata) ? decimal.Parse(minimoDuplicata) : 0;
                TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = pedido.CodCondicaoPagamento });
                if (valorMinimo > 0 && pedido.ValorTotal < valorMinimo)
                    result.ListaErros.Add($"Valor mínimo do pedido é R$ {valorMinimo.ToString("N2")}");

                if (valorMinimoDuplicata > 0 && condicaoPagamento.QtdParcela > 0)
                {
                    if ((pedido.ValorTotal / condicaoPagamento.QtdParcela) < valorMinimoDuplicata)
                        result.ListaErros.Add($"Valor mínimo da duplicata é R$ {valorMinimoDuplicata.ToString("N2")}");
                }

                var validacoes = await _carrinhoRepository.ValidacoesDoCarrinho(PedidoSelecionado.CodCarrinho);
                result.ListaErros.AddRange(validacoes);
            }
            

            result.Sucesso = result.ListaErros.Count() == 0;
            return result;
        }
        private async void Copiar(CarrinhoCommandResult obj)
        {
            //await UserDialogs.Instance.AlertAsync("Função indisponível.", AppName, "OK");
            //return;

            if (!Pedidos.Any(x => x.CarrinhoChecado))
            {
                await UserDialogs.Instance.AlertAsync("Você precisa marcar um pedido para copiar.", AppName, "OK");
                return;
            }
            var page = RgPopupUtility.GerarPopupCopiaPedido(PedidoSelecionado);
            await PopupNavigation.Instance.PushAsync(page);
        }

        public async void TrocarAtendimento(object obj)
        {
            await Load();
        }

        private async void Load(object obj)
        {
            await Load();
        }

        private async void ModificarCarrinhoAlterado(ICarrinhoFechamentoViewModel sender, CarrinhoFechamentoCommandResult parameter)
        {
            var listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1", null, parameter.CodCarrinho));
            if (listCarrinhos?.Count > 0)
            {
                var pedido = listCarrinhos.Where(x => x.CodCarrinho == parameter.CodCarrinho).FirstOrDefault();
                foreach (var item in Pedidos)
                {
                    if (item.CodCarrinho == parameter.CodCarrinho)
                    {
                        item.CifFob = parameter.TipoFrete == "FOB" ? "F" : "C";
                        item.CodCondicaoPagamento = parameter.CodCondicaoPagamento;
                        item.CondicaoPagamento = parameter.CondicaoPagamento;
                        item.CodTabelaPreco = parameter.CodTabelaPreco;
                        item.TabelaPreco = parameter.TabelaPreco;
                        item.OrdemCompra = parameter.OrdemCompra;
                        item.Observacoes = parameter.Observacao;
                        item.DataEntrega = parameter.DataEntrega;
                        item.PrazoMedio = parameter.PrazoMedio;
                        item.PrazoAdicional = parameter.PrazoAdicional;
                        item.PedidoBonificado = parameter.PedidoBonificado;
                        item.CodEvento = parameter.CodEvento;

                        item.CupomChave = parameter.CupomChave;

                        item.PercentualDesconto = pedido.PercentualDesconto;
                        item.PercentualDesconto1 = pedido.PercentualDesconto1;
                        item.PercentualDesconto2 = pedido.PercentualDesconto2;
                        item.PercentualDesconto3 = pedido.PercentualDesconto3;
                        item.ValorTotalLiquido = pedido.ValorTotalLiquido;
                        item.ValorTotal = pedido.ValorTotal;
                        item.Itens = new ObservableCollection<ItemCommandResult>(pedido.Itens);
                    }
                }
            }

        }
        private async void TrocarClienteDoCarrinho(object obj)
        {
            try
            {
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupAtendimento(PedidoSelecionado?.CodCarrinho));
            }
            catch (Exception ex)
            {

            }
        }

        private async void Agrupar(object obj)
        {
            try
            {
                string DiasEntregaInicio = await _parametroRepository.BuscarValorParametro("DIASENTREGAINI");
                if (string.IsNullOrEmpty(DiasEntregaInicio))
                {
                    DiasEntregaInicio = "-30 day";
                }

                string DiasEntregaFim = await _parametroRepository.BuscarValorParametro("DIASENTREGAFIM");
                if (string.IsNullOrEmpty(DiasEntregaFim))
                {
                    DiasEntregaFim = "+30 day";
                }

                List<CarrinhoCommandResult> pedidosChecados = Pedidos.Where(x => x.CarrinhoChecado).ToList<CarrinhoCommandResult>();

                if (pedidosChecados.Count > 1)
                {
                    ClienteCommandResult ClienteSelecionado = (obj as ClienteCommandResult);
                    List<CarrinhoCommandResult> listaPedidos = new List<CarrinhoCommandResult>();

                    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    //VALIDA PEDIDO FILHO NO AGRUPAMENTO
                    var pedidoFilho = pedidosChecados.Where(x => x.PedidoMae != null).FirstOrDefault();
                    if (pedidoFilho != null) {

                        foreach (var car in pedidosChecados) {
                            if (car.CodCarrinho != pedidoFilho.CodCarrinho) {
                                foreach (var item in car.Itens)
                                {
                                    var possui = pedidoFilho.Itens.Where(x => x.CodProduto == item.CodProduto && x.CodDeposito == item.CodDeposito).FirstOrDefault();
                                    if (possui != null)
                                    {
                                        await UserDialogs.Instance.AlertAsync($"Não foi possível realizar o agrupamento dos pedidos selecionados, pois dentre eles há um pedido distribuído de um pedido mãe com a mesma referência {item.CodProduto}.", "Aviso", "OK");
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    string CodTipoPedido = pedidosChecados[0].CodTipoPedido;
                    string codCarrinhoQuebra = "";
                    string pedidoMae = null;

                    var pedidoFuturo = pedidosChecados.Where(x => x.CodDeposito == "2").FirstOrDefault();

                    if (pedidoFuturo != null)
                    {
                        CodTipoPedido = pedidoFuturo.CodDeposito;
                        listaPedidos.Add(pedidoFuturo);

                        string DiasAgrupoInicio = await _parametroRepository.BuscarValorParametro("DIASAGRUPAINI");
                        if (string.IsNullOrEmpty(DiasAgrupoInicio))
                        {
                            DiasAgrupoInicio = "-30 day";
                        }

                        string DiasAgrupaFim = await _parametroRepository.BuscarValorParametro("DIASAGRUPAFIM");
                        if (string.IsNullOrEmpty(DiasAgrupaFim))
                        {
                            DiasAgrupaFim = "+30 day";
                        }

                        string carrinhosCheckedStg = "";
                        foreach (var car in pedidosChecados)
                        {
                            if (carrinhosCheckedStg.Length == 0)
                            {
                                carrinhosCheckedStg = $"'{car.CodCarrinho}'";
                            }
                            else
                            {
                                carrinhosCheckedStg += $",'{car.CodCarrinho}'";
                            }
                        }


                        var respeitaQuebra = true;
                        foreach (var pedido in pedidosChecados)
                        {
                            if (string.IsNullOrEmpty(pedidoMae) && pedido.PedidoMae != null) {
                                pedidoMae = pedido.PedidoMae;
                            }
                            foreach (var itemPedido in pedido.Itens)
                            {
                                codCarrinhoQuebra = await _carrinhoRepository.QuebrarPedidoAgrupamento(
                                new QuebrarPedidoCommand(Session.ATENDIMENTO_ATUAL.CodAtendimento, Session.ATENDIMENTO_ATUAL.CodPessoaCliente,
                                Session.ATENDIMENTO_ATUAL.CodPessoaRepresentante, Session.ATENDIMENTO_ATUAL.CodMarca, null, pedido.CodTipoPedido, null, null, itemPedido.DataEntrega, pedidoFuturo.CodDeposito
                                , DiasAgrupoInicio, DiasAgrupaFim, $"'{pedidoFuturo.CodCarrinho}'", pedido.CodPedidoOrigem/*carrinhosCheckedStg*/));

                                if (codCarrinhoQuebra == pedidoFuturo.CodCarrinho)
                                {
                                    respeitaQuebra = true;
                                }
                                else
                                {
                                    respeitaQuebra = false;
                                    break;
                                }
                            }

                            if (respeitaQuebra && pedidoFuturo.CodCarrinho != pedido.CodCarrinho)
                            {
                                listaPedidos.Add(pedido);
                            }
                        }

                        if (listaPedidos.Count > 1)
                        {
                            AgruparCarrinhoCommand command = new AgruparCarrinhoCommand
                            (
                                Session.USUARIO_LOGADO,
                                Session.ATENDIMENTO_ATUAL,
                                listaPedidos,
                                listaPedidos[0].CodTipoPedido,
                                listaPedidos[0].CodDeposito
                            );

                            var result = await _carrinhoHandler.Handle(command) as HandlerResult;

                            if (result != null && result.Sucesso)
                            {
                                //cancela carrinho no servidor
                                foreach (var pedido in pedidosChecados)
                                {
                                    if (pedido.IndEmAlocacao == 1)
                                    {
                                        var car = await _carrinhoRepository.GetCarrinhoForCode(pedido.CodCarrinho);
                                        if (car.CodSituacaoPedido == "7")
                                        {
                                            var resultTransmissao = await ServiceUtility.CancelarPedido(_carrinhoRepository, _parametroSincronizacaRepository, pedido.CodCarrinho);
                                        }
                                    }
                                }

                                //atualiza itens carrinho
                                await _carrinhoRepository.AtualizaItensImediatosAgrupados(result.Result);
                                await UserDialogs.Instance.AlertAsync("Os pedidos foram agrupados com sucesso", AppName, "OK");
                                await Load();
                                return;
                            }
                            else
                            {
                                var model = result as HandlerResult;
                                string message = string.Join("\n", result.ListaErros);
                                await UserDialogs.Instance.AlertAsync($"Ocorreu um erro ao agrupar os pedidos. \n{message}", AppName, "OK");
                            }
                        }
                        else
                        {
                            await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 2 pedidos validos", AppName, "OK");
                        }

                    }
                    else
                    {
                        //O primeiro pedido é a base entao ja inclui
                        listaPedidos.Add(pedidosChecados[0]);
                        
                        for (int i = 1; i < pedidosChecados.Count; i++)
                        {
                            var pedido = pedidosChecados[i];
                            //1 - Verificar se pedidos sao do mesmo tipo
                            if (CodTipoPedido == pedido.CodTipoPedido)
                            {
                                
                                var respeitaQuebra = true;

                                if (string.IsNullOrEmpty(pedidoMae) && pedido.PedidoMae != null)
                                {
                                    pedidoMae = pedido.PedidoMae;
                                }

                                // 2 - Verificar se itens do pedido respeitam regra de quebra
                                foreach (var itemPedido in pedido.Itens)
                                {
                                    codCarrinhoQuebra = await _carrinhoRepository.QuebrarPedido(
                                    new QuebrarPedidoCommand(Session.ATENDIMENTO_ATUAL.CodAtendimento, Session.ATENDIMENTO_ATUAL.CodPessoaCliente,
                                    Session.ATENDIMENTO_ATUAL.CodPessoaRepresentante, Session.ATENDIMENTO_ATUAL.CodMarca, null, pedido.CodTipoPedido, null, null, itemPedido.DataEntrega, pedido.CodDeposito
                                    , DiasEntregaInicio, DiasEntregaFim, null, pedido.CodPedidoOrigem));

                                    if (codCarrinhoQuebra == pedidosChecados[0].CodCarrinho)
                                    {
                                        respeitaQuebra = true;
                                    }
                                    else
                                    {
                                        respeitaQuebra = false;
                                        break;
                                    }
                                }
                                if (respeitaQuebra)
                                {
                                    listaPedidos.Add(pedido);
                                }
                            }
                            else
                            {
                                await UserDialogs.Instance.AlertAsync("Os pedidos precisam ser do mesmo tipo", AppName, "OK");
                            }
                        }
                        if (listaPedidos.Count > 1)
                        {
                            AgruparCarrinhoCommand command = new AgruparCarrinhoCommand
                            (
                                Session.USUARIO_LOGADO,
                                Session.ATENDIMENTO_ATUAL,
                                listaPedidos,
                                listaPedidos[0].CodTipoPedido,
                                null,
                                pedidoMae
                            );

                            var result = await _carrinhoHandler.Handle(command) as HandlerResult;

                            if (result != null && result.Sucesso)
                            {

                                await UserDialogs.Instance.AlertAsync("Os pedidos foram agrupados com sucesso", AppName, "OK");
                                await Load();
                                return;
                            }
                            else
                            {
                                var model = result as HandlerResult;
                                string message = string.Join("\n", result.ListaErros);
                                await UserDialogs.Instance.AlertAsync($"Ocorreu um erro ao agrupar os pedidos. \n{message}", AppName, "OK");
                            }
                        }
                        else
                        {
                            await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 2 pedidos validos", AppName, "OK");
                        }
                    }
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("Você deve selecionar pelo menos 2 pedidos para Agrupar", AppName, "OK");
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        //Recebe o path do PDF pelo popup(atraves do message center)
        private async void ExibirImpressao(ICarrinhoImpressaoViewModel sender, string path)
        {
            if (!string.IsNullOrEmpty(path))
                _printService.OpenPdf(path);
        }
        #endregion
    }
}