using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;

namespace Pegada.Core.Repositories
{
    // Acesso a dados da tela "Alteração de Grade" (EdicaoGradeItemCarrinho do PegadaIOS), exclusivo do Pegada.
    public class EdicaoGradeItemRepository
    {
        private readonly SQLiteAsyncConnection _sqlAsyncConnection;

        public EdicaoGradeItemRepository(ISqliteConnection context)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
        }

        // Todas as numerações ativas da grade do item (com a quantidade atual do item; 0 quando ainda não há linha).
        // O carrinho do MV só guarda as numerações com quantidade, então sem isso não seria possível incluir um tamanho novo.
        public async Task<List<DerivacaoGradeResult>> BuscarDerivacoesEditaveis(string codCarrinho, decimal codItemCarrinho, string codGrade)
        {
            const string sql =
                "SELECT DG.CodDerivacao AS CodDerivacao, " +
                "       D.Descricao AS Descricao, " +
                "       IFNULL(GIC.Qtd, 0) AS Qtd, " +
                "       IFNULL(GIC.CodGradeItemCarrinho, 0) AS CodGradeItemCarrinho, " +
                "       IFNULL(DG.Ordem, 0) AS Ordem " +
                "FROM TBT_DERIVACAO_GRADE DG " +
                "INNER JOIN TBT_DERIVACAO D ON D.CodDerivacao = DG.CodDerivacao " +
                "LEFT JOIN TBT_GRADE_ITEM_CARRINHO GIC ON GIC.CodCarrinho = ? AND GIC.CodItemCarrinho = ? AND GIC.CodDerivacao = DG.CodDerivacao " +
                "WHERE DG.CodGrade = ? " +
                "AND (DG.IndAtivo = 1 OR GIC.CodGradeItemCarrinho IS NOT NULL) " +
                "ORDER BY DG.Ordem";

            return await _sqlAsyncConnection.QueryAsync<DerivacaoGradeResult>(sql, codCarrinho, (int)codItemCarrinho, codGrade);
        }

        // ProdutoAtendimento tamanhoForModelo: (iOS) — tamanhos de corrugado do modelo, ex.: "6|9|10|12|15".
        public async Task<string> BuscarTamanhosPossiveis(string codProduto)
        {
            return await _sqlAsyncConnection.ExecuteScalarAsync<string>(
                "SELECT M.TamanhosPossiveis FROM TBT_PRODUTO P " +
                "INNER JOIN TBT_MODELO M ON M.CodModelo = P.CodModelo " +
                "WHERE P.CodProduto = ? LIMIT 1", codProduto);
        }

        // Parâmetro 6 do tipo de pedido: 'S' = o pedido é encaixotado em corrugados.
        public async Task<bool> TipoPedidoExigeCorrugado(string codTipoPedido)
        {
            var valor = await _sqlAsyncConnection.ExecuteScalarAsync<string>(
                "SELECT Valor FROM TBT_PARAMETRO_TIPO_PEDIDO WHERE CodParametro = 6 AND CodTipoPedido = ?", codTipoPedido);
            return string.Equals(valor, "S", StringComparison.OrdinalIgnoreCase);
        }

        // +editarGradeFromItemCarrinho: + -atualizaCarrinho (iOS): grava a grade do item, zera "sem grade", recalcula os totais.
        public async Task SalvarGrade(string codCarrinho, decimal codItemCarrinho, IList<KeyValuePair<string, int>> quantidades)
        {
            int codItem = (int)codItemCarrinho;
            int total = quantidades.Sum(q => q.Value);

            await _sqlAsyncConnection.RunInTransactionAsync(conn =>
            {
                foreach (var q in quantidades)
                {
                    int alteradas = conn.Execute(
                        "UPDATE TBT_GRADE_ITEM_CARRINHO SET Qtd = ?, CtrlDataOperacao = ? " +
                        "WHERE CodCarrinho = ? AND CodItemCarrinho = ? AND CodDerivacao = ?",
                        q.Value, DateTime.Now, codCarrinho, codItem, q.Key);

                    if (alteradas == 0 && q.Value > 0)
                    {
                        int codGradeItem = conn.ExecuteScalar<int>(
                            "SELECT IFNULL(MAX(CodGradeItemCarrinho), 0) + 1 FROM TBT_GRADE_ITEM_CARRINHO WHERE CodCarrinho = ? AND CodItemCarrinho = ?",
                            codCarrinho, codItem);

                        conn.Execute(
                            "INSERT INTO TBT_GRADE_ITEM_CARRINHO (CodCarrinho, CodItemCarrinho, CodGradeItemCarrinho, CodDerivacao, Qtd, QtdNaGrade, CtrlDataOperacao) " +
                            "VALUES (?, ?, ?, ?, ?, 0, ?)",
                            codCarrinho, codItem, codGradeItem, q.Key, q.Value, DateTime.Now);
                    }
                }

                // O carrinho do MV não guarda numeração sem quantidade.
                conn.Execute(
                    "DELETE FROM TBT_GRADE_ITEM_CARRINHO WHERE CodCarrinho = ? AND CodItemCarrinho = ? AND Qtd = 0",
                    codCarrinho, codItem);

                conn.Execute(
                    "UPDATE TBT_ITEM_CARRINHO SET QtdTotal = ?, ItemSemGrade = 0 WHERE CodCarrinho = ? AND CodItemCarrinho = ?",
                    total, codCarrinho, codItem);
            });
        }

        // -atualizaCarrinho (iOS): só quantidade e valores do cabeçalho; não mexe em preço líquido nem em descontos dos itens.
        public async Task AtualizarTotaisCarrinho(string codCarrinho)
        {
            await _sqlAsyncConnection.RunInTransactionAsync(conn =>
            {
                conn.Execute(
                    "UPDATE TBT_CARRINHO SET " +
                    " QtdTotal = (SELECT SUM(IC.QtdCaixa * IC.QtdTotal) FROM TBT_ITEM_CARRINHO IC WHERE IC.CodCarrinho = ? AND IC.StatusFilho IS NULL), " +
                    " ValorTotal = (SELECT SUM((IC.QtdCaixa * IC.QtdTotal) * IC.ValorUnitario) FROM TBT_ITEM_CARRINHO IC WHERE IC.CodCarrinho = ? AND IC.StatusFilho IS NULL), " +
                    " ValorTotalLiquido = (SELECT SUM((IC.QtdCaixa * IC.QtdTotal) * IC.ValorUnitarioLiquido) FROM TBT_ITEM_CARRINHO IC WHERE IC.CodCarrinho = ? AND IC.StatusFilho IS NULL) " +
                    "WHERE CodCarrinho = ?",
                    codCarrinho, codCarrinho, codCarrinho, codCarrinho);
            });
        }
    }
}
