using System.Collections.Generic;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;
using System.Threading.Tasks;

namespace Pegada.Core.Repositories
{
    // Carrinho criticado no retorno da transmissão (Carrinho updateSituacao:codPedido:mensagemCritica:andSeqCritica:andFirest:
    // do PegadaIOS): grava a situação e o código do pedido no carrinho e troca as críticas da TBT_CRITICA_CARRINHO.
    public class CriticaCarrinhoRepository
    {
        private readonly SQLiteAsyncConnection _sqlAsyncConnection;

        public CriticaCarrinhoRepository(ISqliteConnection context)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
        }

        public Task GravarCriticas(string codCarrinho, string codSituacaoPedido, string codPedido, IList<string> criticas)
        {
            return _sqlAsyncConnection.RunInTransactionAsync(conexao =>
            {
                conexao.Execute("UPDATE TBT_CARRINHO SET CodSituacaoPedido = ?, CodPedido = ? WHERE CodCarrinho = ?",
                    codSituacaoPedido, string.IsNullOrEmpty(codPedido) ? null : codPedido, codCarrinho);

                // iOS: apaga as críticas anteriores só na primeira e insere uma linha por crítica (SeqCritica 1, 2, ...).
                conexao.Execute("DELETE FROM TBT_CRITICA_CARRINHO WHERE CodCarrinho = ?", codCarrinho);

                int seq = 1;
                foreach (var critica in criticas)
                {
                    if (!string.IsNullOrEmpty(critica))
                    {
                        // iOS troca aspas simples por aspas duplas; a coluna aceita até 500 caracteres.
                        string texto = critica.Replace("'", "\"");
                        if (texto.Length > 500)
                            texto = texto.Substring(0, 500);

                        conexao.Execute("INSERT INTO TBT_CRITICA_CARRINHO (CodCarrinho, SeqCritica, Critica, ID) VALUES (?, ?, ?, 0)",
                            codCarrinho, seq, texto);
                    }
                    seq++;
                }
            });
        }
    }
}
