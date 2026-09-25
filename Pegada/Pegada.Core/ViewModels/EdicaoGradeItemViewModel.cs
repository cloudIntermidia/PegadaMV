using Acr.UserDialogs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.ViewModels;
using Pegada.Core.Repositories;
using Prism.Mvvm;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Pegada.Core.ViewModels
{
    // Uma numeração da grade (coluna com "+", tamanho, quantidade e "-" do EdicaoGradeItemCarrinho do PegadaIOS).
    public class GradeItemLinha : BindableBase
    {
        private int _qtd;

        public string CodDerivacao { get; set; }
        public string Descricao { get; set; }
        public int Qtd { get => _qtd; set => SetProperty(ref _qtd, value); }

        public ICommand MaisCommand { get; set; }
        public ICommand MenosCommand { get; set; }
    }

    // Tela "Alteração de Grade" aberta pelo botão EDITAR dos itens do carrinho (EdicaoGradeItemCarrinho do PegadaIOS).
    public class EdicaoGradeItemViewModel : ViewModelBase
    {
        private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

        private readonly EdicaoGradeItemRepository _repositorio;
        private CarrinhoCommandResult _carrinho;
        private List<ItemCommandResult> _itens;
        private List<int> _tamanhosPossiveis = new List<int>();

        private string _descricao;
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }

        private string _valorUnitario;
        public string ValorUnitario { get => _valorUnitario; set => SetProperty(ref _valorUnitario, value); }

        private string _tamanhos;
        public string Tamanhos { get => _tamanhos; set => SetProperty(ref _tamanhos, value); }

        private bool _tamanhosVisivel;
        public bool TamanhosVisivel { get => _tamanhosVisivel; set => SetProperty(ref _tamanhosVisivel, value); }

        private int _qtdCaixas = 1;
        public int QtdCaixas { get => _qtdCaixas; set { SetProperty(ref _qtdCaixas, value); RaisePropertyChanged(nameof(QtdTotal)); } }

        public int QtdTotal => Linhas.Sum(l => l.Qtd) * QtdCaixas;

        public ObservableCollection<GradeItemLinha> Linhas { get; } = new ObservableCollection<GradeItemLinha>();

        public ICommand CancelarCommand { get; }
        public ICommand SalvarCommand { get; }

        public EdicaoGradeItemViewModel(EdicaoGradeItemRepository repositorio) : base()
        {
            _repositorio = repositorio;
            CancelarCommand = new Command(Cancelar);
            SalvarCommand = new Command(Salvar);
        }

        // Retorna false quando a edição não deve abrir (itens de grades diferentes marcados).
        public async Task<bool> Init(CarrinhoCommandResult carrinho, List<ItemCommandResult> itensMarcados)
        {
            _carrinho = carrinho;
            _itens = itensMarcados;
            var primeiro = _itens[0];

            var derivacoesPrimeiro = await CarregarDerivacoes(primeiro);

            // Vários itens: só edita junto quem tem a mesma grade; os incompatíveis são desmarcados (iOS: ICarrinho_TB_NaoEdicaoGrademultipla).
            if (_itens.Count > 1)
            {
                string gradePrimeiro = string.Join("|", derivacoesPrimeiro.Select(d => d.Descricao));
                var incompativeis = new List<ItemCommandResult>();
                foreach (var item in _itens.Skip(1))
                {
                    var derivacoes = await CarregarDerivacoes(item);
                    if (string.Join("|", derivacoes.Select(d => d.Descricao)) != gradePrimeiro)
                        incompativeis.Add(item);
                }

                if (incompativeis.Count > 0)
                {
                    foreach (var item in incompativeis)
                        item.ItemChecado = false;

                    await UserDialogs.Instance.AlertAsync("Não é possível edição de multiplas grandes. Os itens com grades diferentes serão desselecionados.", "Atenção");
                    return false;
                }
            }

            if (derivacoesPrimeiro.Count == 0)
            {
                await UserDialogs.Instance.AlertAsync("Não foi encontrada a grade do item.", "Atenção");
                return false;
            }

            Descricao = _itens.Count > 1
                ? "Produtos: " + string.Join(", ", _itens.Select(i => i.CodProduto))
                : primeiro.Descricao;
            ValorUnitario = primeiro.ValorUnitario.ToString("C2", PtBr);
            QtdCaixas = primeiro.QtdCaixa > 0 ? (int)primeiro.QtdCaixa : 1;

            // Tamanhos de corrugado do modelo (ex.: "Tamanhos: 6-9-10-12-15").
            string possiveis = await _repositorio.BuscarTamanhosPossiveis(primeiro.CodProduto);
            _tamanhosPossiveis = (possiveis ?? string.Empty).Split('|')
                .Select(t => int.TryParse(t, out int v) ? v : 0).Where(v => v > 0).OrderBy(v => v).ToList();
            TamanhosVisivel = _tamanhosPossiveis.Count > 0;
            Tamanhos = "Tamanhos: " + string.Join("-", _tamanhosPossiveis);

            Linhas.Clear();
            foreach (var d in derivacoesPrimeiro)
            {
                var linha = new GradeItemLinha { CodDerivacao = d.CodDerivacao, Descricao = d.Descricao, Qtd = (int)d.Qtd };
                linha.MaisCommand = new Command(() => Alterar(linha, +1));
                linha.MenosCommand = new Command(() => Alterar(linha, -1));
                Linhas.Add(linha);
            }
            RaisePropertyChanged(nameof(QtdTotal));

            return true;
        }

        private async Task<List<DerivacaoGradeResult>> CarregarDerivacoes(ItemCommandResult item)
        {
            return await _repositorio.BuscarDerivacoesEditaveis(item.CodCarrinho, item.CodItemCarrinho, item.CodGrade);
        }

        // tapAddItemGrade / tapSubItemGrade: "-" não passa de zero.
        private void Alterar(GradeItemLinha linha, int delta)
        {
            if (delta < 0 && linha.Qtd == 0)
                return;

            linha.Qtd += delta;
            RaisePropertyChanged(nameof(QtdTotal));
        }

        private async void Cancelar()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        // aBtnSalvar
        private async void Salvar()
        {
            try
            {
                int qtdGrade = Linhas.Sum(l => l.Qtd);
                if (qtdGrade == 0)
                {
                    await UserDialogs.Instance.AlertAsync("Informe a quantidade de ao menos uma numeração da grade.", "Atenção");
                    return;
                }

                // iOS: a soma da grade precisa ser um dos tamanhos de corrugado do modelo (senão abre o encaixotamento).
                if (_tamanhosPossiveis.Count > 0 && !_tamanhosPossiveis.Contains(qtdGrade)
                    && await _repositorio.TipoPedidoExigeCorrugado(_carrinho.CodTipoPedido))
                {
                    await UserDialogs.Instance.AlertAsync(
                        $"A quantidade da grade ({qtdGrade}) precisa ser um dos tamanhos de corrugado do modelo: {string.Join("-", _tamanhosPossiveis)}.",
                        "Atenção");
                    return;
                }

                foreach (var item in _itens)
                {
                    // Cada item recebe as quantidades pela descrição da numeração (mesma grade, códigos podem variar).
                    var derivacoes = await CarregarDerivacoes(item);
                    var quantidades = derivacoes
                        .Select(d => new KeyValuePair<string, int>(
                            d.CodDerivacao,
                            Linhas.FirstOrDefault(l => l.Descricao == d.Descricao)?.Qtd ?? 0))
                        .ToList();

                    await _repositorio.SalvarGrade(item.CodCarrinho, item.CodItemCarrinho, quantidades);
                }

                foreach (var codCarrinho in _itens.Select(i => i.CodCarrinho).Distinct())
                    await _repositorio.AtualizarTotaisCarrinho(codCarrinho);

                await UserDialogs.Instance.AlertAsync("A grade foi alterada com sucesso.", "Edição");
                await PopupNavigation.Instance.PopAsync();
                MessagingCenter.Send<object>(this, "LoadCarrinho");
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"Problema na edição de item no carrinho\nErro: {ex.Message}", "Atenção");
            }
        }
    }
}
