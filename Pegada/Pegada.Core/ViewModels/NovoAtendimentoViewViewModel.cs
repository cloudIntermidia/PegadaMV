using Acr.UserDialogs;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.ViewModels;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

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
        #endregion

        #region "Repositorios"
        private readonly IClienteRepository _clienteRepository;
        private readonly IAtendimentoRepository _atendimentoRepository;
        private readonly ITabelaPrecoRepository _tabelaPrecoRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly AtendimentoUtility _atendimentoUtility;
        #endregion


        #region "Commands"
        public ICommand SalvarAtendimentoCommand { get; set; }
        public ICommand CancelarAtendimentoCommand { get; set; }
        public ICommand TipoVendaVendorCommand { get; set; }
        public ICommand SelecionarClienteCommand { get; set; }
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
            ICondicaoPagamentoRepository condicaoPagamento
            )
        {
            CarrinhoFechamento = new CarrinhoFechamentoCommandResult();

            _clienteRepository = clienteRepository;
            _atendimentoRepository = atendimentoRepository;
            _tabelaPrecoRepository = tabelaPrecoRepository;
            _atendimentoUtility = atendimentoUtility;
            _parametroRepository = parametroRepository;
            _atendimentoUtility.CarrinhoFechamento = CarrinhoFechamento;
            _coeficienteRepository = coeficienteRepository;
            _condicaoPagamentoRepository = condicaoPagamento;
            SalvarAtendimentoCommand = new DelegateCommand<object>(SalvarAtendimento);
            CancelarAtendimentoCommand = new DelegateCommand<object>(FecharPopupAtendimento);
            InfoCommand = new Command(VisualizarInfoCliente);
            SelecionarCondicaoPagamentoCommand = new Command(async () => await _atendimentoUtility.SelecionarCondicaoPagamento());
            SelecionarTabelaPrecoCommand = new Command(async () => await _atendimentoUtility.SelecionarTabelaPreco(true));
            SelecionarTipoPedidoCommand = new Command(async () => await _atendimentoUtility.SelecionarTipoPedido());
            SelecionarClienteCommand = new Command(SelecionarCliente);
        }
        #endregion

        #region "Metodos"
        private async void Init()
        {
            try
            {
                //CarrinhoFechamento.PrazoMedio = await _atendimentoUtility.PrazoMedioPadrao();
                //CarrinhoFechamento.PrazoAdicional = await _atendimentoUtility.PrazoAdicionalEmFeira();
                LiberaClienteNovo = await _parametroRepository.BuscarValorParametro(ParametrosSistema.BLOQCLIENOVO);
                if (string.IsNullOrEmpty(LiberaClienteNovo)) {
                    LiberaClienteNovo = "N";
                }
            }
            catch (Exception ex)
            {
            }
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

        //versão 35446
        //private async void SelecionarClienteEvent(object obj)
        //{
        //    IsLicenciamento = "";
        //    try
        //    {
        //        ClienteSelecionado = obj as ClienteCommandResult;

        //        if (ClienteSelecionado.CodSituacaoCliente == "50" && LiberaClienteNovo != "S")
        //        {
        //            ClienteSelecionado = new ClienteCommandResult();
        //            await UserDialogs.Instance.AlertAsync("Cliente bloqueado para venda.");
        //            return;
        //        }

        //        //chamado 35446
        //        if (!string.IsNullOrEmpty(ClienteSelecionado.Prazo))
        //        {
        //            CarrinhoFechamento.PrazoMedio = Convert.ToDecimal(ClienteSelecionado.Prazo);
        //        }

        //        if (ClienteSelecionado.IsCategoriaValida == 0) {
        //            await UserDialogs.Instance.AlertAsync($"Não é permitido abrir atendimento para este cliente, pois a categoria {ClienteSelecionado.CodigoSegmento} não possui política configurada.");
        //            ClienteSelecionado = new ClienteCommandResult();
        //            return;
        //        }

        //        if (ClienteSelecionado.Licenciamento != null)
        //        {
        //            if (ClienteSelecionado.Licenciamento == "S")
        //            {
        //                IsLicenciamento = "Cliente pertencente ao modelo de licenciamento!";
        //            }
        //            else if (ClienteSelecionado.Licenciamento == "N")
        //            {
        //                IsLicenciamento = "O CNPJ não faz parte do licenciamento!";
        //            }
        //        }
        //        else { IsLicenciamento = "Informações de licenciamento está vazio!"; }

        //        if (ClienteSelecionado.CodSituacaoCliente == "2")
        //        {
        //            ClienteSelecionado = new ClienteCommandResult();
        //            await UserDialogs.Instance.AlertAsync("Selecione cliente valido.");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
        //    }

        //    await PopupNavigation.Instance.PopAsync();
        //}

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

                if (ClienteSelecionado.IsCategoriaValida == 0)
                {
                    await UserDialogs.Instance.AlertAsync($"Não é permitido abrir atendimento para este cliente, pois a categoria {ClienteSelecionado.CodigoSegmento} não possui política configurada.");
                    ClienteSelecionado = new ClienteCommandResult();
                    return;
                }

                if (ClienteSelecionado.Licenciamento != null)
                {
                    if (ClienteSelecionado.Licenciamento == "S")
                    {
                        IsLicenciamento = "Cliente pertencente ao modelo de licenciamento!";
                    }
                    else if (ClienteSelecionado.Licenciamento == "N")
                    {
                        IsLicenciamento = "O CNPJ não faz parte do licenciamento!";
                    }
                }
                else { IsLicenciamento = "Informações de licenciamento está vazio!"; }

                if (ClienteSelecionado.CodSituacaoCliente == "2")
                {
                    ClienteSelecionado = new ClienteCommandResult();
                    await UserDialogs.Instance.AlertAsync("Selecione cliente valido.");
                    return;
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, AppName);
            }

            await PopupNavigation.Instance.PopAsync();
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
                else if (CarrinhoFechamento.CodCondicaoPagamento == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione uma Condição de Pagamento para abrir o atendimento.");
                    return;
                }
                else if (CarrinhoFechamento.CodTipoPedido == null)
                {
                    await UserDialogs.Instance.AlertAsync("Selecione um Tipo de Pedido para abrir o atendimento.");
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


        //versão 35446
        //private async void SalvarAtendimento(object obj)
        //{
        //    try
        //    {

        //        if (ClienteSelecionado == null || ClienteSelecionado.CodPessoaCliente == null)
        //        {
        //            await UserDialogs.Instance.AlertAsync("Selecione um cliente para abrir o atendimento.");
        //            return;
        //        }
        //        else if (CarrinhoFechamento.CodCondicaoPagamento == null)
        //        {
        //            await UserDialogs.Instance.AlertAsync("Selecione uma Condição de Pagamento para abrir o atendimento.");
        //            return;
        //        }
        //        else if (CarrinhoFechamento.CodTipoPedido == null)
        //        {
        //            await UserDialogs.Instance.AlertAsync("Selecione um Tipo de Pedido para abrir o atendimento.");
        //            return;
        //        } else if(Session.USUARIO_LOGADO.CodTipoPessoa != "1" && CarrinhoFechamento.CodTipoPedido == "5")
        //        {
        //            await UserDialogs.Instance.AlertAsync("Somente gerente pode abrir atendimento do tipo RESERVA!");
        //            return;
        //        }
        //        else
        //        {
        //            //########################################################
        //            if (CarrinhoFechamento.PercentualDesconto > 0)
        //            {
        //                var coefiDesconto = await _coeficienteRepository.BuscarCoeficientePorProduto(new BuscarCoeficienteCommand("DESCONTO_MAXIMO", ClienteSelecionado.CodigoSegmento, null));

        //                if (coefiDesconto != null)
        //                {
        //                    TabelaPrecoResult condicaoPagamento = await _condicaoPagamentoRepository.BuscarCondicaoPagamento(new BuscarCondicaoPagamentoCommand() { CodCondicaoPagamento = CarrinhoFechamento.CodCondicaoPagamento });
        //                    string prazoMedio = condicaoPagamento.PrazoMedio > 0 ? condicaoPagamento.PrazoMedio.ToString() : "0";

        //                    var coefiPrazoMedio = await _coeficienteRepository.BuscarCoeficientePrazoMedio(new BuscarCoeficienteCommand("PRAZO", ClienteSelecionado.CodigoSegmento, null, prazoMedio));

        //                    decimal coeficiente = coefiDesconto.Coeficiente;
        //                    if (coefiPrazoMedio != null)
        //                    {
        //                        if (coefiPrazoMedio.Coeficiente > 0)
        //                        {
        //                            if (Convert.ToDecimal(prazoMedio) >= 60)
        //                            {
        //                                if (!string.IsNullOrEmpty(ClienteSelecionado.Prazo))
        //                                {
        //                                    if (Convert.ToDecimal(prazoMedio) > Convert.ToDecimal(ClienteSelecionado.Prazo))
        //                                    {
        //                                        coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                                    }
        //                                    else
        //                                    {
        //                                        coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    coeficiente = coefiDesconto.Coeficiente - coefiPrazoMedio.Coeficiente;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                coeficiente = coefiDesconto.Coeficiente + coefiPrazoMedio.Coeficiente;
        //                            }
        //                        }
        //                    }
        //                    var des = CarrinhoFechamento.PercentualDesconto / 100;
        //                    if (des > coeficiente)
        //                    {
        //                        var descontoInteiro = coeficiente != null ? Convert.ToInt32(coeficiente * 100) : coeficiente;
        //                        var desconto = await UserDialogs.Instance.ConfirmAsync($"O Desconto Informado é maior que o desconto máximo permitido de {descontoInteiro}% para condição de  {condicaoPagamento.Descricao.Trim()}, seu pedido será enviado para aprovação, deseja continuar?", "Desconto Excedido", "Sim", "Não");
        //                        if (!desconto)
        //                        {
        //                            return;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    await UserDialogs.Instance.AlertAsync("Não foi encontrado um segmento válido no cliente para o desconto. Sincronize ou entre em contato com a Pegada.", AppName, "OK");
        //                    return;
        //                }
        //            }
        //            //########################################################

        //            CriarAtendimentoCommand command = new CriarAtendimentoCommand()
        //            {

        //                CodPessoaCliente = ClienteSelecionado.CodPessoaCliente,
        //                CodUsuario = Session.USUARIO_LOGADO.CodUsuario,
        //                CodMarca = Session.USUARIO_LOGADO.CodMarca,
        //                CodInstalacao = Session.USUARIO_LOGADO.CodInstalacao,
        //                Descricao = ClienteSelecionado.RazaoSocial,
        //                ConfiguracaoAtendimento = ClienteSelecionado.EnderecoPrincipalCompleto,
        //                IndAberto = 1,
        //                CodTabelaPreco = CarrinhoFechamento.CodTabelaPreco,
        //                PrazoMedio = CarrinhoFechamento.PrazoMedio,
        //                CodCondicaoPagamento = CarrinhoFechamento.CodCondicaoPagamento,
        //                PercentualDesconto1 = CarrinhoFechamento.PercentualDesconto,
        //                Controle = CarrinhoFechamento.Controle,
        //                TipoPedido = CarrinhoFechamento.CodTipoPedido,
        //            };

        //            Session.ATENDIMENTO_ATUAL = await _atendimentoUtility.CriarAtendimento(command);
        //            Session.ATENDIMENTO_ATUAL.Markup = Session.MarkupPadrao;
        //            MessagingCenter.Send<object>(this, "AtendimentoFoiAlterado");
        //            await PopupNavigation.Instance.PopAllAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await UserDialogs.Instance.AlertAsync($"{ex.Message}");
        //    }
        //}
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