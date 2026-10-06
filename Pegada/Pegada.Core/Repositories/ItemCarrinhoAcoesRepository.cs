using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;

namespace Pegada.Core.Repositories
{
    // Dados da tela "Edição de Preço do Item" (EdicaoItemCarrinhoViewController do PegadaIOS).
    public class PrecoItemCarrinhoResult
    {
        public string Descricao { get; set; }
        public decimal? ValorUnitario { get; set; }
        public decimal? ValorUnitarioLiquido { get; set; }
        public decimal? ValorUnitarioLiquidoOriginal { get; set; }
        public decimal? ValorUnitarioLiquidoDif { get; set; }
        public decimal? Markup { get; set; }
        public decimal? PrecoPVL { get; set; }
        public decimal? PercDesc1 { get; set; }
        public string PercentualAjustePrecoItem { get; set; }
    }

    // Linha do ITEM_CARRINHO_DESC (descForItemCarrinho do PegadaIOS).
    public class DescontoItemCarrinhoResult
    {
        public int Acumulador { get; set; }
        public decimal? PercentualDesconto { get; set; }
    }

    // Acesso a dados das ações do menu do item do carrinho (didSelectItemCarrinhoMenuOption, ItemCarrinhoTableViewController do PegadaIOS).
    public class ItemCarrinhoAcoesRepository
    {
        private readonly SQLiteAsyncConnection _sqlAsyncConnection;

        public ItemCarrinhoAcoesRepository(ISqliteConnection context)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
        }

        // PercDesc1 vem da condição de pagamento e pode estar gravado com vírgula ("1,5").
        private const string PercDesc1Sql = "CAST(REPLACE(IFNULL(I.PercDesc1, 0), ',', '.') AS REAL)";

        public async Task<PrecoItemCarrinhoResult> BuscarPrecoItem(string codCarrinho, decimal codItemCarrinho, string codTabelaPreco, string codPessoaCliente)
        {
            string sql =
                "SELECT P.CodProduto || ' - ' || P.Descricao AS Descricao, " +
                "       I.ValorUnitario, " +
                "       I.ValorUnitarioLiquido, " +
                "       I.ValorUnitarioLiquidoOriginal, " +
                "       I.ValorUnitarioLiquidoDif, " +
                "       I.Markup, " +
                "       ITP.PrecoPVL, " +
                "       " + PercDesc1Sql + " AS PercDesc1, " +
                "       CLI.PercentualAjustePrecoItem " +
                "FROM TBT_ITEM_CARRINHO I " +
                "LEFT JOIN TBT_PRODUTO P ON P.CodProduto = I.CodProduto " +
                "LEFT JOIN TBT_ITEM_TABELA_PRECO ITP ON ITP.CodProduto = I.CodProduto AND ITP.CodTabelaPreco = ? " +
                "LEFT JOIN TBT_CLIENTE CLI ON CLI.CodPessoaCliente = ? " +
                "WHERE I.CodCarrinho = ? AND I.CodItemCarrinho = ?";

            var result = await _sqlAsyncConnection.QueryAsync<PrecoItemCarrinhoResult>(sql, codTabelaPreco, codPessoaCliente, codCarrinho, (int)codItemCarrinho);
            return result.FirstOrDefault();
        }

        public async Task<decimal> BuscarPercDescFinanceiro(string codCarrinho, decimal codItemCarrinho)
        {
            var result = await _sqlAsyncConnection.QueryAsync<PrecoItemCarrinhoResult>(
                "SELECT " + PercDesc1Sql + " AS PercDesc1 FROM TBT_ITEM_CARRINHO I WHERE I.CodCarrinho = ? AND I.CodItemCarrinho = ?",
                codCarrinho, (int)codItemCarrinho);
            return result.FirstOrDefault()?.PercDesc1 ?? 0;
        }

        public async Task<List<DescontoItemCarrinhoResult>> BuscarDescontosItem(string codCarrinho, decimal codItemCarrinho)
        {
            const string sql =
                "SELECT Acumulador, PercentualDesconto " +
                "FROM TBT_ITEM_CARRINHO_DESC " +
                "WHERE CodCarrinho = ? " +
                "AND SeqItemPedido NOT LIKE '%.%' " +
                "AND SeqItemPedido = ?";

            return await _sqlAsyncConnection.QueryAsync<DescontoItemCarrinhoResult>(sql, codCarrinho, ((int)codItemCarrinho).ToString());
        }

        // touchSalvar (EdicaoItemCarrinhoViewController): updateMarkup (quando há PVL) + updateValorItemCarrinho.
        // IndPrecoEditado impede o recálculo do carrinho (CARRINHO_UPDATE_QTD) de voltar o markup do atributo.
        public async Task SalvarPrecoItem(string codCarrinho, decimal codItemCarrinho, decimal valor, decimal? novoMarkup)
        {
            if (novoMarkup.HasValue)
            {
                await _sqlAsyncConnection.ExecuteAsync(
                    "UPDATE TBT_ITEM_CARRINHO SET Markup = ? WHERE CodCarrinho = ? AND CodItemCarrinho = ?",
                    novoMarkup.Value, codCarrinho, (int)codItemCarrinho);
            }

            await _sqlAsyncConnection.ExecuteAsync(
                "UPDATE TBT_ITEM_CARRINHO SET ValorUnitarioLiquido = ?, IndPrecoEditado = 1 WHERE CodCarrinho = ? AND CodItemCarrinho = ?",
                valor, codCarrinho, (int)codItemCarrinho);
        }

        // editaLoja (ItemCarrinhoTableViewController): ItemCarrinho updateNomeLoja.
        public async Task AtualizarNomeLoja(string codCarrinho, decimal codItemCarrinho, string nomeLoja)
        {
            await _sqlAsyncConnection.ExecuteAsync(
                "UPDATE TBT_ITEM_CARRINHO SET NomeLoja = ? WHERE CodCarrinho = ? AND CodItemCarrinho = ?",
                nomeLoja, codCarrinho, (int)codItemCarrinho);
        }
    }
}
