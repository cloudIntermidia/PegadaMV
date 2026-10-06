using Acr.UserDialogs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.ViewModels;
using Pegada.Core.Repositories;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Pegada.Core.ViewModels
{
    // Percentuais do quadro de descontos (% Financeiro, % Nivel2..5 e % Total) usado na "Edição de Preço do Item"
    // e no "Visualizar Desconto" (EdicaoItemCarrinhoViewController / VisualizaDescontoICViewController do PegadaIOS).
    public class DescontosItemCarrinho
    {
        private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

        public string Financeiro { get; private set; } = "0";
        public string Nivel2 { get; private set; } = "0";
        public string Nivel3 { get; private set; } = "0";
        public string Nivel4 { get; private set; } = "0";
        public string Nivel5 { get; private set; } = "0";
        public string Total { get; private set; } = "0";

        public static async Task<DescontosItemCarrinho> Carregar(ItemCarrinhoAcoesRepository repositorio, string codCarrinho, decimal codItemCarrinho)
        {
            var percFinanceiro = await repositorio.BuscarPercDescFinanceiro(codCarrinho, codItemCarrinho);
            var descontos = await repositorio.BuscarDescontosItem(codCarrinho, codItemCarrinho);

            var resultado = new DescontosItemCarrinho { Financeiro = Formatar(percFinanceiro) };
            foreach (var desc in descontos)
            {
                string valor = Formatar(desc.PercentualDesconto ?? 0);
                switch (desc.Acumulador)
                {
                    case 2: resultado.Nivel2 = valor; break;
                    case 3: resultado.Nivel3 = valor; break;
                    case 4: resultado.Nivel4 = valor; break;
                    case 5: resultado.Nivel5 = valor; break;
                }
            }

            // Desconto total em cascata: 1 - (1 - D1) * (1 - Dn)...
            decimal multiplicador = 1 - percFinanceiro / 100;
            foreach (var desc in descontos.Where(d => d.PercentualDesconto.HasValue))
                multiplicador *= 1 - desc.PercentualDesconto.Value / 100;

            resultado.Total = ((1 - multiplicador) * 100).ToString("N2", PtBr);
            return resultado;
        }

        private static string Formatar(decimal valor) => valor.ToString("0.##", PtBr);
    }

    // Tela "Edição de Preço do Item" aberta pela ação "Editar preço líquido" dos itens do carrinho
    // (EdicaoItemCarrinhoViewController do PegadaIOS).
    public class EdicaoPrecoItemViewModel : ViewModelBase
    {
        private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

        private readonly ItemCarrinhoAcoesRepository _repositorio;
        private readonly ICarrinhoRepository _carrinhoRepository;

        private CarrinhoCommandResult _carrinho;
        private ItemCommandResult _item;
        private PrecoItemCarrinhoResult _dadosPreco;
        private decimal _valorMinimo;

        private string _descricao;
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }

        private string _markup;
        public string Markup { get => _markup; set => SetProperty(ref _markup, value); }

        private string _precoBruto;
        public string PrecoBruto { get => _precoBruto; set => SetProperty(ref _precoBruto, value); }

        private string _precoLiquidoAntesAjuste;
        public string PrecoLiquidoAntesAjuste { get => _precoLiquidoAntesAjuste; set => SetProperty(ref _precoLiquidoAntesAjuste, value); }

        private string _ajustePreco;
        public string AjustePreco { get => _ajustePreco; set => SetProperty(ref _ajustePreco, value); }

        private string _precoLiquidoAposAjuste;
        public string PrecoLiquidoAposAjuste { get => _precoLiquidoAposAjuste; set => SetProperty(ref _precoLiquidoAposAjuste, value); }

        private string _precoPVL;
        public string PrecoPVL { get => _precoPVL; set => SetProperty(ref _precoPVL, value); }

        private string _preco;
        public string Preco { get => _preco; set => SetProperty(ref _preco, value); }

        private DescontosItemCarrinho _descontos = new DescontosItemCarrinho();
        public DescontosItemCarrinho Descontos { get => _descontos; set => SetProperty(ref _descontos, value); }

        public ICommand AplicarCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EdicaoPrecoItemViewModel(ItemCarrinhoAcoesRepository repositorio, ICarrinhoRepository carrinhoRepository) : base()
        {
            _repositorio = repositorio;
            _carrinhoRepository = carrinhoRepository;
            AplicarCommand = new Command(async () => await Aplicar());
            SalvarCommand = new Command(async () => await Salvar());
            CancelarCommand = new Command(async () => await PopupNavigation.Instance.PopAsync());
        }

        public async Task<bool> Init(CarrinhoCommandResult carrinho, ItemCommandResult item)
        {
            _carrinho = carrinho;
            _item = item;
            _dadosPreco = await _repositorio.BuscarPrecoItem(carrinho.CodCarrinho, item.CodItemCarrinho, carrinho.CodTabelaPreco, carrinho.CodPessoaCliente);
            if (_dadosPreco == null)
                return false;

            // carregaTela: valor mínimo = preço líquido original menos o % de ajuste permitido ao cliente (PercentualAjustePrecoItem).
            decimal original = _dadosPreco.ValorUnitarioLiquidoOriginal ?? 0;
            _valorMinimo = original - (original * PercentualAjusteCliente() / 100);

            // carregaDescricoesPreco
            Descricao = item.Descricao;
            Markup = FormatarValor(_dadosPreco.Markup);
            PrecoBruto = FormatarValor(_dadosPreco.ValorUnitario);
            PrecoLiquidoAntesAjuste = FormatarValor(_dadosPreco.ValorUnitarioLiquidoOriginal);
            AjustePreco = FormatarValor(_dadosPreco.ValorUnitarioLiquidoDif);
            PrecoLiquidoAposAjuste = FormatarValor(_dadosPreco.ValorUnitarioLiquido);
            PrecoPVL = FormatarValor(_dadosPreco.PrecoPVL);

            Descontos = await DescontosItemCarrinho.Carregar(_repositorio, carrinho.CodCarrinho, item.CodItemCarrinho);
            return true;
        }

        // touchAplicar: só pré-visualiza o ajuste (diferença, preço após o ajuste e novo markup) sem gravar.
        private async Task Aplicar()
        {
            if (!TryLerPreco(out decimal valor))
                return;

            if (!ValorPermitido(valor))
            {
                await ExibirValorInvalido(valor);
                return;
            }

            decimal original = _dadosPreco.ValorUnitarioLiquidoOriginal ?? 0;
            AjustePreco = FormatarValor(Math.Round(original - valor, 2));
            PrecoLiquidoAposAjuste = FormatarValor(valor);

            if (_dadosPreco.PrecoPVL.HasValue && valor != 0)
                Markup = (_dadosPreco.PrecoPVL.Value / valor).ToString("0.00", PtBr);
        }

        // touchSalvar: grava o novo markup (quando há PVL) e o preço líquido do item e recalcula os totais do carrinho.
        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Preco))
                {
                    await PopupNavigation.Instance.PopAsync();
                    return;
                }

                if (!TryLerPreco(out decimal valor))
                    return;

                if (!ValorPermitido(valor))
                {
                    await ExibirValorInvalido(valor);
                    return;
                }

                decimal? novoMarkup = _dadosPreco.PrecoPVL.HasValue && valor != 0 ? _dadosPreco.PrecoPVL.Value / valor : (decimal?)null;

                await _repositorio.SalvarPrecoItem(_carrinho.CodCarrinho, _item.CodItemCarrinho, valor, novoMarkup);
                await _carrinhoRepository.AtualizaQtdCarrinho(_carrinho.CodCarrinho);

                MessagingCenter.Send<object>(this, "LoadCarrinho");
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, T("GlobalTituloAtencao"));
            }
        }

        private bool ValorPermitido(decimal valor) => _valorMinimo < valor;

        private async Task ExibirValorInvalido(decimal valor)
        {
            decimal liquido = _dadosPreco.ValorUnitarioLiquido ?? 0;
            decimal percentualAlterado = liquido != 0 ? Math.Round((liquido - valor) / liquido * 100, 2) : 0;

            string mensagem = $"{T("EdicaoPrecoItemMsgDescontoMaximo")} ({PercentualAjusteCliente().ToString("0.##", PtBr)}%)\n" +
                              $" {T("EdicaoPrecoItemMsgPercentualAlterado")} ({percentualAlterado.ToString("0.00", PtBr)}%)";
            await UserDialogs.Instance.AlertAsync(mensagem, T("GlobalTituloAtencao"));
        }

        private bool TryLerPreco(out decimal valor)
        {
            string texto = (Preco ?? string.Empty).Trim().Replace(",", ".");
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        // Parametro descontoPrecoLiquidoItem: PercentualAjustePrecoItem do cliente (0 quando não informado).
        private decimal PercentualAjusteCliente()
        {
            string texto = (_dadosPreco?.PercentualAjustePrecoItem ?? string.Empty).Replace(",", ".");
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal perc) ? perc : 0;
        }

        private static string FormatarValor(decimal? valor) => valor.HasValue ? valor.Value.ToString("N2", PtBr) : string.Empty;

        private static string T(string chave) => new MobiliVendas.Core.Helpers.TranslateExtension().GetMessage(chave);
    }

    // Tela "Descontos" aberta pela ação "Visualizar Desconto" dos itens do carrinho (VisualizaDescontoICViewController do PegadaIOS).
    public class DescontoItemViewModel : ViewModelBase
    {
        private readonly ItemCarrinhoAcoesRepository _repositorio;

        private DescontosItemCarrinho _descontos = new DescontosItemCarrinho();
        public DescontosItemCarrinho Descontos { get => _descontos; set => SetProperty(ref _descontos, value); }

        public ICommand VoltarCommand { get; }

        public DescontoItemViewModel(ItemCarrinhoAcoesRepository repositorio) : base()
        {
            _repositorio = repositorio;
            VoltarCommand = new Command(async () => await PopupNavigation.Instance.PopAsync());
        }

        public async Task Init(CarrinhoCommandResult carrinho, ItemCommandResult item)
        {
            Descontos = await DescontosItemCarrinho.Carregar(_repositorio, carrinho.CodCarrinho, item.CodItemCarrinho);
        }
    }
}
