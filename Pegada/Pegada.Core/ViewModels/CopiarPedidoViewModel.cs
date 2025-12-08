using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Pegada.Core.Entities;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Handlers;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Entities;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Infra.Repositories;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pegada.Core.ViewModels
{
    public class CopiarPedidoViewModel : ViewModelBase, ICopiaPedidoViewModel
    {
        #region "Propriedades"
        private string filtroPesquisaCliente;
        public string FiltroPesquisaCliente
        {
            get { return filtroPesquisaCliente; }
            set { SetProperty(ref filtroPesquisaCliente, value); }
        }

        private bool _produtoNaoTemEstoque;
        public bool ProdutoNaoTemEstoque
        {
            get { return _produtoNaoTemEstoque; }
            set { SetProperty(ref _produtoNaoTemEstoque, value); }
        }

        private bool _isSemEstoque;
        public bool IsSemEstoque
        {
            get { return _isSemEstoque; }
            set { SetProperty(ref _isSemEstoque, value); }
        }

        private bool _isComDesconto;
        public bool IsComDesconto
        {
            get { return _isComDesconto; }
            set { SetProperty(ref _isComDesconto, value); }
        }

        private string filtroPesquisaReferencia;
        public string FiltroPesquisaReferencia
        {
            get { return filtroPesquisaReferencia; }
            set { SetProperty(ref filtroPesquisaReferencia, value); }
        }

        public ObservableCollection<ItemCommandResult> itensFull { get; set; }
        public ObservableCollection<ItemCommandResult> itensUI { get; set; }
        public ObservableCollection<ItemCommandResult> itensSemEstoque { get; set; }
        public ObservableCollection<ItemCommandResult> produtosSemEstoque { get; set; }
        public ObservableCollection<ClienteCommandResult> Clientes { get; set; }
        
        public CarrinhoCommandResult PedidoSelecionado { get; set; }
        public ClienteCommandResult ClienteSelecionado { get; set; }

        private GenericComboResult _politicaComercialSelecionada;
        public GenericComboResult PoliticaComercialSelecionada { get => _politicaComercialSelecionada; set => SetProperty(ref _politicaComercialSelecionada, value); }
        private GenericComboResult _tabelaPrecoSelecionada;
        public GenericComboResult TabelaPrecoSelecionada { get => _tabelaPrecoSelecionada; set => SetProperty(ref _tabelaPrecoSelecionada, value); }
        #endregion

        #region "Commands"
        public ICommand SelecionarPoliticaComercialCommand { get; set; }
        public ICommand SelecionarTabelaPrecoCommand { get; set; }
        public ICommand CancelarCopiaCommand { get; set; }
        public ICommand SalvarCopiaCommand { get; set; }
        public ICommand InfoCommand { get; set; }
        public ICommand PesquisarReferenciaCommand { get; set; }
        public ICommand PesquisarPorReferenciaCommand { get; set; }
        public ICommand PesquisarClienteCommand { get; set; }
        public ICommand ClienteTappedCommand { get; set; }
        public ICommand ItemTappedCommand { get; set; }
        public ICommand ComDescontoCommand { get; set; }
        public ICommand SemEstoqueCommand { get; set; }
        public ICommand AtualizaEstoqueCommand { get; set; }
        #endregion

        #region "Repositorios"
        private readonly DataBaseRepository _databaseRepository;
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IPoliticaComercialRepository _politicaComercialRepository;
        private readonly ITabelaPrecoRepository _tabelaPrecoRepository;
        private readonly IModeloRepository _modeloRepository;
        private readonly INivelRepository _nivelRepository;
        private readonly AtendimentoUtility _atendimentoUtility;
        private readonly CarrinhoCommandHandler _carrinhoHandler;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        #endregion


        #region "Construtor"
        public CopiarPedidoViewModel(IPoliticaComercialRepository politicaComercialRepository, ITabelaPrecoRepository tabelaPrecoRepository
                                    , AtendimentoUtility atendimentoUtility, IModeloRepository modeloRepository, INivelRepository nivelRepository
                                    , CarrinhoCommandHandler carrinhoHandler, DataBaseRepository databaseRepository, ICarrinhoRepository carrinhoRepository
                                    , IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository)
        {
            Clientes = new ObservableCollection<ClienteCommandResult>();
            PoliticaComercialSelecionada = new GenericComboResult();
            TabelaPrecoSelecionada = new GenericComboResult();

            _produtoRepository = produtoRepository;
            _databaseRepository = databaseRepository;
            _carrinhoRepository = carrinhoRepository;
            _nivelRepository = nivelRepository;
            _modeloRepository = modeloRepository;
            _politicaComercialRepository = politicaComercialRepository;
            _tabelaPrecoRepository = tabelaPrecoRepository;
            _atendimentoUtility = atendimentoUtility;
            _atendimentoUtility.Clientes = Clientes;
            _carrinhoHandler = carrinhoHandler;
            _pedidoRepository = pedidoRepository;

            ClienteTappedCommand = new DelegateCommand<object>(ClienteTapped);
            SelecionarPoliticaComercialCommand = new Command(GetPoliticaComercial);
            SelecionarTabelaPrecoCommand = new Command(GetTabelaPreco);
            CancelarCopiaCommand = new Command(CancelarCopia);
            SalvarCopiaCommand = new Command(SalvarCopia);

            PesquisarClienteCommand = new Command(async () => await PesquisarClientes());
            PesquisarReferenciaCommand = new Command(async () => await PesquisarReferencias());
            PesquisarPorReferenciaCommand = new Command(PesquisarPorReferencias);
            IsComDesconto = true;
            ComDescontoCommand = new Command(ChangeIsComDesconto);
            SemEstoqueCommand = new Command(ChangeIsSemEstoque);
            AtualizaEstoqueCommand = new Command(AtualizaEstoque);
        }
        #endregion

        #region "Metodos"
        private void PesquisarPorReferencias(object obj)
        {
            //var itens = itensFull.Where(x => x.CodProduto.Contains(FiltroPesquisaReferencia)).ToList();
            //foreach (var item in itens)
            //{
            //    PedidoSelecionado.Itens.
            //}

        }

        private async void AtualizaEstoque()
        {
            try
            {
                itensUI.Clear();
                itensUI.Clear();
                IsSemEstoque = false;

                var itensTmp = new ObservableCollection<ItemCommandResult>();
                foreach (var item in itensFull) {
                    itensTmp.Add(item);
                }

                foreach (var item in itensTmp) {
                    if (!item.TemEstoque) {
                        var estoques = await _produtoRepository.BuscarEstoques(new BuscarEstoquesCommand() { CodProduto = item.CodProduto, CodDeposito = null });
                        if (estoques.Count() > 0) {
                            foreach (var estoque in estoques) {
                                if (estoque.QtdEstoque >= item.QtdTotal)
                                {
                                    item.QtdSaldo = estoque.QtdEstoque;
                                    item.CodDeposito = estoque.Estoque;
                                    item.DataEntrega = (DateTime)estoque.DataDisponibilidade;
                                    item.DataEstoque = (DateTime)estoque.DataDisponibilidade;
                                    item.TemEstoque = true;
                                    break;
                                }
                            }
                        }
                    }
                    itensUI.Add(item);
                }

                if (!(new ObservableCollection<ItemCommandResult>(itensUI.Where(i => i.TemEstoque == false)).Any())) {
                    ProdutoNaoTemEstoque = false;
                }
                RaisePropertyChanged("itensUI");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
        }

        private async void ChangeIsComDesconto()
        {
            IsComDesconto = !IsComDesconto;
        }

        private async void ChangeIsSemEstoque()
        {
            var possui = new ObservableCollection<ItemCommandResult>(itensFull.Where(i => i.TemEstoque == false)).Any();

            if (possui)
            {
                itensUI.Clear();
                itensUI.Clear();

                if (!IsSemEstoque)
                {
                    itensUI = new ObservableCollection<ItemCommandResult>(itensFull.Where(i => i.TemEstoque == false));
                }
                else {
                    foreach (var item in itensFull) {
                        itensUI.Add(item);
                    }
                }

                RaisePropertyChanged("itensUI");
                IsSemEstoque = !IsSemEstoque;
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("Não há produtos sem estoque para a data atual do carrinho.");
                return;
            }
            
        }

        private void ClienteTapped(object item)
        {
            ClienteSelecionado = item as ClienteCommandResult;
        }
        private async void GetPoliticaComercial(object obj)
        {
            try
            {
                List<PoliticaComercialCommandResult> politicas = await _politicaComercialRepository.BuscarPoliticas(new PoliticaComercialCommand() { CodPessoa = Session.USUARIO_LOGADO.CodPessoa, CodTipoPessoa = Session.USUARIO_LOGADO.CodTipoPessoa });
                var politicasCollection = new ObservableCollection<GenericComboResult>(politicas.Select(x => new GenericComboResult() { Codigo = x.CodPoliticaComercial, Descricao = x.Descricao }).ToList());
                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(politicasCollection, SelecionarPoliticaComercial, new Rectangle(0.5, 0.5, 0.7, 0.4), false, false, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void SelecionarPoliticaComercial(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            PoliticaComercialSelecionada = obj as GenericComboResult;
            await PopupNavigation.Instance.PopAsync();

            TabelaPrecoSelecionada = new GenericComboResult();
        }

        private async void GetTabelaPreco(object obj)
        {
            try
            {
                if (PoliticaComercialSelecionada == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione primeiro a Politica Comercial");
                    return;
                }

                List<TabelaPrecoResult> tabelas = await _tabelaPrecoRepository.BuscarTabelasDePreco(new BuscarTabelaPrecoCommand(null, null, null, PoliticaComercialSelecionada?.Codigo));
                var TabelaPrecoCollection = new ObservableCollection<GenericComboResult>();
                foreach (var item in tabelas.Select(x => new GenericComboResult() { Codigo = x.CodTabelaPreco, Descricao = x.Descricao }))
                {
                    TabelaPrecoCollection.Add(item);
                }

                await PopupNavigation.Instance.PushAsync(RgPopupUtility.GerarPopupGenerico(TabelaPrecoCollection, SelecionarTabelaPreco, new Rectangle(0.5, 0.5, 0.7, 0.4), true, true, false));
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }
        }

        private async void SelecionarTabelaPreco(object obj)
        {
            if (obj == null)
            {
                await PopupNavigation.Instance.PopAsync();
                return;
            }

            TabelaPrecoSelecionada = obj as GenericComboResult;

            await PesquisarReferencias();
            await PopupNavigation.Instance.PopAsync();
        }

        private async Task PesquisarReferencias()
        {
            if (PoliticaComercialSelecionada == null || string.IsNullOrEmpty(PoliticaComercialSelecionada.Codigo))
            {
                await UserDialogs.Instance.AlertAsync("Selecione a Politica Comercial primeiro!", AppName);
                return;
            }

            if (TabelaPrecoSelecionada == null || string.IsNullOrEmpty(TabelaPrecoSelecionada.Codigo))
            {
                await UserDialogs.Instance.AlertAsync("Selecione a tabela de preço primeiro!", AppName);
                return;
            }

            var codProdutos = string.Join(",", PedidoSelecionado.Itens.Select(x => x.CodProduto).ToArray());
            var command = new BuscarModeloCommand()
            {
                ItensEmAtendimento = -1,
                CodProdutoIn = codProdutos,
                CodTabelaPreco = TabelaPrecoSelecionada.Codigo,
                ValidaEstoque = "S"
            };
            var produtos = await _modeloRepository.BuscarModelos(command);
            foreach (var item in PedidoSelecionado.Itens)
            {
                item.ItemChecado = produtos.Any(x => x.CodProdutoModelo == item.CodProduto);
                item.ItemBloqueado = !produtos.Any(x => x.CodProdutoModelo == item.CodProduto);
            }


        }

        private async Task PesquisarClientes()
        {
            await _atendimentoUtility.CarregarClientes(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, null, Session.USUARIO_LOGADO.CodTipoPessoa, FiltroPesquisaCliente)).ConfigureAwait(false);
        }

        private async void SalvarCopia()
        {
            try
            { 
                if (ClienteSelecionado == null)
                {
                    await UserDialogs.Instance.AlertAsync($"Selecione primeiro o cliente.", AppName);
                    return;
                }

                if (!itensUI.Any(x => x.ItemChecado))
                {
                    await UserDialogs.Instance.AlertAsync($"É necessário selecionar pelo menos 1 item para ser copiado.", AppName);
                    return;
                }

                if (itensUI.Any(x => x.ItemChecado && x.ItemBloqueado))
                {
                    string message = string.Join("\n", itensUI.Where(x => x.ItemChecado && x.ItemBloqueado).Select(x => x.CodProduto).ToArray());

                    await UserDialogs.Instance.AlertAsync($"Os produtos listados abaixo estão bloqueados para venda. \n{message}", AppName);
                    return;
                }

                string mesgPrdExclusivos = string.Join("\n", itensUI.Where(x => x.ItemChecado && x.RestricaoLocal == ClienteSelecionado.RestricaoLocal && ClienteSelecionado.RestricaoLocal == "S").Select(x => x.CodProduto).ToArray());
                if (!string.IsNullOrEmpty(mesgPrdExclusivos))
                {
                    await UserDialogs.Instance.AlertAsync($"Os produtos listados abaixo estão bloqueados para venda para o cliente selecionado {ClienteSelecionado.RazaoSocial}. \n{mesgPrdExclusivos}", AppName);
                    return;
                }

                foreach (var itemCheck in itensUI.Where(x => x.ItemChecado).ToList()) {
                    if (itemCheck.QtdTotal > itemCheck.QtdSaldo) {
                        await UserDialogs.Instance.AlertAsync($"Produto {itemCheck.Descricao} não possui saldo para a quantidade {itemCheck.QtdTotal} digitada.", AppName);
                        return;
                    }
                }

                if (ClienteSelecionado.RestricaoLocal == "S"){

                    var itensRestrito = itensUI.Where(x => x.ItemChecado && x.RestricaoLocal == "S").ToList();
                    if (itensRestrito != null) {
                        string desprodutos = "";
                        foreach (var dp in itensRestrito) {
                            desprodutos += $"{dp.CodProduto}, ";
                        }
                        await UserDialogs.Instance.AlertAsync($"Produto(s) {desprodutos} estão bloqueados para venda.", AppName);
                        return;
                    }
                }

                if (!IsComDesconto) {
                    foreach (var item in itensUI) {
                        item.PercDesc = 0;
                        item.PercDesc1 = 0;
                    }
                }

                if (IsSemEstoque)
                {
                    await UserDialogs.Instance.AlertAsync($"Não é possível salvar o pedido na visualização de itens sem estoque.", AppName);
                    return;
                }

                //if (ProdutoNaoTemEstoque)
                //{
                //    await UserDialogs.Instance.AlertAsync($"Não é possível salvar o pedido se a opção de atualizar o estoque estiver disponivel.", AppName);
                //    return;
                //}

                //if (produtosSemEstoque.Count == itensUI.Count)
                //{
                //    await UserDialogs.Instance.AlertAsync($"Não é possível salvar o pedido, todos os itens estão sem estoques.", AppName);
                //    return;
                //}

                var listaItensChecados = itensUI.Where(x => x.ItemChecado).ToList();

                string codProdutosSemEstoque = string.Empty;

                codProdutosSemEstoque = string.Join(", ", listaItensChecados.Where(x => x.TemEstoque == false).Select(item => item.CodProduto));

                if (!string.IsNullOrEmpty(codProdutosSemEstoque))
                {
                    var response = await UserDialogs.Instance.ConfirmAsync($"Não há estoques para os produtos {codProdutosSemEstoque}, deseja salvar o pedido sem esses itens?", "Produto(s) sem estoque(s)", "Sim", "Não");
                    if (!response)
                        return;
                }

                UserDialogs.Instance.ShowLoading("Copiando pedido");
                CriarAtendimentoCommand atendimento = new CriarAtendimentoCommand(ClienteSelecionado.CodPessoaCliente,
                                        Session.USUARIO_LOGADO.CodUsuario, Session.USUARIO_LOGADO.CodMarca, Session.USUARIO_LOGADO.CodInstalacao,
                                        ClienteSelecionado.Apelido, 1, PedidoSelecionado.CodCondicaoPagamento, null, null, null, null, 0, 0, null, null, 0, 0, 0, 0,PedidoSelecionado.TipoPedido);

                var command = new CopiarCarrinhoCommand()
                {
                    CodCarrinhoOrigem = PedidoSelecionado.CodPedido == null ? PedidoSelecionado.CodCarrinho : PedidoSelecionado.CodPedido,
                    CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
                    Usuario = Session.USUARIO_LOGADO,
                    NovoAtendimento = atendimento,
                    CodPoliticaComercial = PoliticaComercialSelecionada?.Codigo,
                    CodTabelaPreco = PedidoSelecionado.CodTabelaPreco,
                    Itens = itensUI.Where(x => x.ItemChecado).ToList(),
                    CodTipoPedido = PedidoSelecionado.CodTipoPedido,
                    CodCondicaoPagamento = PedidoSelecionado.CodCondicaoPagamento
                };

                //var commandDesconto = new BuscarNivelDescontoCommand()
                //{
                //    CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
                //    CodMarca = Session.USUARIO_LOGADO.CodMarca,
                //    CodProduto = itensFull[0].CodProduto
                //};

                //command.PercentualDesconto = await BuscarDescontoCliente(commandDesconto);
                var result = await _carrinhoHandler.Handle(command) as HandlerResult;
                if (result.Sucesso)
                {
                    //Projeto #21793
                    //Add parametros especificos do projeto para validar a copia
                    String codCarrinhoNovo = (result as HandlerResult).Result;
                    WcfPedidoModelInput modelRequest = await _carrinhoRepository.BuscarCarrinhoParaTransmissao(PedidoSelecionado.CodCarrinho != null ? PedidoSelecionado.CodCarrinho : codCarrinhoNovo);
                    var carrinhoCopia = (Integracao_TBT_CARRINHO)(modelRequest.PEDIDOVENDA.Carrinho);
                    //Atualiza Percentual de Desconto
                    await _databaseRepository.ExecutaUpdate("TBT_CARRINHO",
                        new List<string>() {  "CodDeposito", "PercentualDesconto",
                                              "PercentualDesconto1", "PercentualDesconto2", "PercentualDesconto3",
                                              "PercentualDesconto4", "PercentualDesconto5", "ObservacoesSeparacao",
                                              "CodTransportadora", "DataLimite",
                                              "DataEntrega", "AceitaFaturamentoAntecipado", "CodCondicaoPagamento",
                                               "PrazoMedio", "Observacoes","OrdemCompra"},
                        new List<string>() { "CodCarrinho" },
                        new
                        {
                            CodCarrinho = codCarrinhoNovo,
                            Observacoes = carrinhoCopia.Observacoes,
                            OrdemCompra = carrinhoCopia.OrdemCompra,
                            CodDeposito = carrinhoCopia.CodDeposito,
                            DataEntrega = carrinhoCopia.DataEntrega,
                            AceitaFaturamentoAntecipado = carrinhoCopia.AceitaFaturamentoAntecipado,
                            CodCondicaoPagamento = carrinhoCopia.CodCondicaoPagamento,
                            PrazoMedio = carrinhoCopia.PrazoMedio,

                            PercentualDesconto = IsComDesconto == true ? carrinhoCopia.PercentualDesconto : 0,
                            PercentualDesconto1 = 0,
                            PercentualDesconto2 = 0,
                            PercentualDesconto3 = 0,
                            PercentualDesconto4 = 0,
                            PercentualDesconto5 = 0,
                            CodTransportadora = carrinhoCopia.CodTransportadora,
                            DataLimite = carrinhoCopia.DataLimite,
                            ObservacoesSeparacao = carrinhoCopia.ObservacoesSeparacao
                        });


                    //Atualiza itens do novo carrinho com info especificas
                    var itensLista = itensUI.Where(x => x.ItemChecado).ToList();
                    foreach (var itemUpdate in itensLista)
                    {
                        await _databaseRepository.ExecutaUpdate("TBT_ITEM_CARRINHO",
                        new List<string>() { "DataEntrega" },
                        new List<string>() { "CodCarrinho", "CodProduto" },
                        new
                        {
                            CodCarrinho = codCarrinhoNovo,
                            CodProduto = itemUpdate.CodProduto,
                            DataEntrega = itemUpdate.DataEntrega
                        });
                    }


                    MessagingCenter.Send<object>(this, "LoadCarrinho");

                    UserDialogs.Instance.HideLoading();
                    await UserDialogs.Instance.AlertAsync($"Carrinho copiado com sucesso. {result.Result}", AppName, "OK");
                }
                else
                {
                    UserDialogs.Instance.HideLoading();
                    string message = string.Join("\n", result.ListaErros);
                    await UserDialogs.Instance.AlertAsync($"Não foi possível copiar o pedido. {message}", AppName, "OK");
                }

                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
        }

        private async Task<decimal> BuscarDescontoCliente(BuscarNivelDescontoCommand buscarDesconto)
        {
            var descontos = await _nivelRepository.GetDescontos(buscarDesconto);
            decimal percentualDesconto = 0M;
            if (descontos.Count > 0)
            {
                percentualDesconto = descontos[0].Desconto;
            }
            return percentualDesconto;
        }

        private async void CancelarCopia()
        {
            await PopupNavigation.Instance.PopAsync();
        }
        #endregion


        #region "Metodos da Interface"
        public async Task Init()
        {
            IsSemEstoque = false;
            ProdutoNaoTemEstoque = false;

            itensFull = new ObservableCollection<ItemCommandResult>();
            itensUI = new ObservableCollection<ItemCommandResult>();
            itensFull = await _pedidoRepository.GetItensPedidosCopia(PedidoSelecionado);

            if ((new ObservableCollection<ItemCommandResult>(itensFull.Where(i => i.TemEstoque == false)).Any())) {
                ProdutoNaoTemEstoque = true;
            }

            foreach (var item in itensFull) {
                var grades = await _carrinhoRepository.BuscarGradesDoItem(new BuscarGradesItemCommand(PedidoSelecionado.CodCarrinho, item.CodItemCarrinho));
                foreach (var grade in grades)
                {
                    item.Grades.Add(grade);
                }
                itensUI.Add(item);
            }  

            RaisePropertyChanged("itensUI");
        }

        public void SetTodos(bool value)
        {
            foreach (var itemCheck in itensUI)
            {
                itemCheck.ItemChecado = value;
            }
        }

        public void SetItem(bool value, ItemCommandResult item)
        {
            if (item.CodKit != null) {
                foreach (var itemCheck in itensUI)
                {
                    if (itemCheck.CodKit == item.CodKit) {
                        itemCheck.ItemChecado = value;
                    }
                }
            }
        }

        public void SetPedidoSelecionado(CarrinhoCommandResult pedidoSelecionado)
        {
            PedidoSelecionado = pedidoSelecionado;
        }
        #endregion

    }
}
