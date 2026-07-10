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
            await CarregarClientesDaTela();
        }

        //Centraliza o carregamento de clientes (usado tanto ao abrir a tela quanto na busca).
        //Reatribui _atendimentoUtility.Clientes = Clientes antes de cada chamada porque AtendimentoUtility
        //é compartilhado por outras telas (NovoAtendimento, DistribuirPedido etc.) que também apontam essa
        //mesma propriedade para as próprias coleções; sem isso, a busca pode acabar preenchendo uma
        //coleção diferente da que está de fato ligada (bind) na tela de cópia.
        private async Task CarregarClientesDaTela()
        {
            _atendimentoUtility.Clientes = Clientes;
            await _atendimentoUtility.CarregarClientes(new BuscarClienteCommand(Session.USUARIO_LOGADO.CodPessoa, Session.USUARIO_LOGADO.CodMarca, null, Session.USUARIO_LOGADO.CodTipoPessoa, FiltroPesquisaCliente));
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

                //Regra do Objective-C (CopiaCarrinhoViewController.touchSaveBtn): valida se o cliente
                //selecionado pode efetuar pedido (mesma regra usada para bloquear a seleção do cliente).
                bool podeFazerPedido = ClienteSelecionado.IndAtivo == 1 &&
                                       ClienteSelecionado.IdExternoSituacao == 1 &&
                                       ClienteSelecionado.CreditoLiberado == 1 &&
                                       (ClienteSelecionado.UsaClasseRisco == 0 ||
                                        (ClienteSelecionado.UsaClasseRisco == 1 && ClienteSelecionado.PermiteDigitacao == 1));

                if (!podeFazerPedido)
                {
                    await UserDialogs.Instance.AlertAsync($"Cliente bloqueado para efetuar pedido.", AppName, "OK");
                    return;
                }

                //Regra do Objective-C (CopiaCarrinhoViewController.touchSaveBtn): pedido/carrinho de origem
                //com CodSituacaoPedido "7" (cancelado) não pode ser copiado.
                if (PedidoSelecionado.CodSituacaoPedido == "7")
                {
                    await UserDialogs.Instance.AlertAsync($"Não é possível copiar um pedido cancelado.", AppName, "OK");
                    return;
                }

                if (!IsComDesconto) {
                    foreach (var item in itensUI) {
                        item.PercDesc = 0;
                        item.PercDesc1 = 0;
                    }
                }

                UserDialogs.Instance.ShowLoading("Copiando pedido");

                //Regra do Objective-C (CopiaCarrinhoViewController.touchSaveBtn): se o cliente permite ajuste
                //de preço, o atendimento passa a aplicar markup; se o cliente ainda não tem markup definido
                //(markup == 0), tanto o atendimento quanto o cliente recebem markup 1.
                decimal markup = 0;
                int indAplicaMarkup = 0;
                if (ClienteSelecionado.PermiteAjustePreco == 1)
                {
                    indAplicaMarkup = 1;
                    markup = ClienteSelecionado.Markup;
                    if (markup == 0)
                    {
                        markup = 1;
                        ClienteSelecionado.Markup = 1;
                    }
                }

                //Regra do Objective-C: o atendimento de destino sempre herda a tabela de preço do
                //pedido/carrinho de origem (atendimentoCopia.codTabelaPreco = _pedidoCopia.codTabelaPreco).
                //A reutilização de um atendimento já aberto para o cliente (em vez de criar um novo) já é
                //feita pelo AtendimentoRepository.AbrirAtendimento no lado do servidor/handler.
                CriarAtendimentoCommand atendimento = new CriarAtendimentoCommand(ClienteSelecionado.CodPessoaCliente,
                                        Session.USUARIO_LOGADO.CodUsuario, Session.USUARIO_LOGADO.CodMarca, Session.USUARIO_LOGADO.CodInstalacao,
                                        ClienteSelecionado.Apelido, 1, PedidoSelecionado.CodCondicaoPagamento, null, PedidoSelecionado.CodTabelaPreco, null, null, 0, 0, null, null, 0, 0, 0, 0, PedidoSelecionado.CodTipoPedido, markup, indAplicaMarkup);

                var command = new CopiarCarrinhoCommand()
                {
                    CodCarrinhoOrigem = PedidoSelecionado.CodPedido == null ? PedidoSelecionado.CodCarrinho : PedidoSelecionado.CodPedido,
                    CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
                    Usuario = Session.USUARIO_LOGADO,
                    NovoAtendimento = atendimento,
                    CodPoliticaComercial = PoliticaComercialSelecionada?.Codigo,
                    CodTabelaPreco = PedidoSelecionado.CodTabelaPreco,
                    Itens = itensUI.ToList(),
                    CodTipoPedido = PedidoSelecionado.CodTipoPedido,
                    CodCondicaoPagamento = PedidoSelecionado.CodCondicaoPagamento
                };

                var result = await _carrinhoHandler.Handle(command) as HandlerResult;
                if (result.Sucesso)
                {
                    //Projeto #21793
                    //Add parametros especificos do projeto para validar a copia
                    //Observação: o código antigo buscava esses valores via BuscarCarrinhoParaTransmissao/
                    //PEDIDOVENDA.Carrinho, convertendo o resultado para "Integracao_TBT_CARRINHO". Essa
                    //estrutura não existe mais (WcfPedidoModelInput.PEDIDOVENDA é hoje um DTO plano, sem a
                    //propriedade "Carrinho"), então os valores são obtidos diretamente do carrinho/pedido de
                    //origem já carregado (PedidoSelecionado), que é a mesma fonte de dados.
                    String codCarrinhoNovo = (result as HandlerResult).Result;

                    //Atualiza Percentual de Desconto e demais campos herdados do carrinho/pedido de origem
                    await _databaseRepository.ExecutaUpdate("TBT_CARRINHO",
                        new List<string>() {  "CodDeposito", "PercentualDesconto",
                                              "PercentualDesconto1", "PercentualDesconto2", "PercentualDesconto3",
                                              "PercentualDesconto4", "PercentualDesconto5",
                                              "DataEntrega", "AceitaFaturamentoAntecipado", "CodCondicaoPagamento",
                                               "PrazoMedio", "Observacoes","OrdemCompra"},
                        new List<string>() { "CodCarrinho" },
                        new
                        {
                            CodCarrinho = codCarrinhoNovo,
                            Observacoes = PedidoSelecionado.Observacoes,
                            OrdemCompra = PedidoSelecionado.OrdemCompra,
                            CodDeposito = PedidoSelecionado.CodDeposito,
                            DataEntrega = PedidoSelecionado.DataEntrega,
                            AceitaFaturamentoAntecipado = PedidoSelecionado.AceitaFaturamentoAntecipado,
                            CodCondicaoPagamento = PedidoSelecionado.CodCondicaoPagamento,
                            PrazoMedio = PedidoSelecionado.PrazoMedio,

                            PercentualDesconto = IsComDesconto == true ? PedidoSelecionado.PercentualDesconto : 0,
                            PercentualDesconto1 = 0,
                            PercentualDesconto2 = 0,
                            PercentualDesconto3 = 0,
                            PercentualDesconto4 = 0,
                            PercentualDesconto5 = 0,
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
            //Todo o carregamento fica dentro de um try/catch porque o Init() agora é aguardado
            //(await) por quem abre esse popup (RgPopupUtility.GerarPopupCopiaPedido) antes de exibir a
            //tela. Sem esse try/catch, qualquer erro aqui dentro impede o popup de aparecer (a tela
            //simplesmente não abre); com ele, mostramos o erro ao usuário e ainda assim deixamos a tela
            //abrir (mesmo que com a lista de itens/clientes incompleta).
            try
            {
                IsSemEstoque = false;
                ProdutoNaoTemEstoque = false;

                //Carrega a lista de clientes automaticamente ao abrir a tela, igual ao Objective-C
                //(que já mostra a lista completa assim que a tela de cópia abre, sem exigir uma busca antes).
                FiltroPesquisaCliente = null;
                await CarregarClientesDaTela();

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
                    //Regra do Objective-C (CopiaCarrinhoViewController): ao copiar, a grade do item copiado
                    //sempre recebe o codGrade fixo "999", independente da grade de origem.
                    grade.CodGrade = "999";
                    item.Grades.Add(grade);
                }
                itensUI.Add(item);
            }

            //A tela de cópia do Pegada mostra somente a lista de clientes (igual ao Objective-C),
            //sem etapa manual de seleção de política comercial/tabela de preço nem checkbox por item.
            //Por isso a política comercial e a tabela de preço usadas na validação dos itens são as
            //do próprio pedido/carrinho de origem (mesma regra do ObjC: atendimentoCopia.codTabelaPreco
            //= _pedidoCopia.codTabelaPreco), e todo item compatível já sai marcado para cópia.
            //Observação: essa validação usa itensUI (a lista efetivamente copiada) em vez de
            //PedidoSelecionado.Itens (usada pelo antigo PesquisarReferencias), pois PedidoSelecionado.Itens
            //não é preenchido no fluxo de cópia.
            PoliticaComercialSelecionada = new GenericComboResult { Codigo = PedidoSelecionado.CodPoliticaComercial };
            TabelaPrecoSelecionada = new GenericComboResult { Codigo = PedidoSelecionado.CodTabelaPreco };

            if (!string.IsNullOrEmpty(TabelaPrecoSelecionada.Codigo) && itensUI.Any())
            {
                var codProdutos = string.Join(",", itensUI.Select(x => x.CodProduto).ToArray());
                var command = new BuscarModeloCommand()
                {
                    ItensEmAtendimento = -1,
                    CodProdutoIn = codProdutos,
                    CodTabelaPreco = TabelaPrecoSelecionada.Codigo,
                    ValidaEstoque = "S"
                };
                //var produtos = await _modeloRepository.BuscarModelos(command);
                //foreach (var item in itensUI)
                //{
                //    item.ItemBloqueado = !produtos.Any(x => x.CodProdutoModelo == item.CodProduto);
                //    item.ItemChecado = !item.ItemBloqueado;
                //}
            }
            else
            {
                foreach (var item in itensUI)
                {
                    item.ItemChecado = true;
                }
            }

                RaisePropertyChanged("itensUI");
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName, "OK");
            }
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
