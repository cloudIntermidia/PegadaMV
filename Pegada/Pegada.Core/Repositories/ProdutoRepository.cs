using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Infra.DataContext;

namespace Pegada.Core.Repositories
{
    public class ProdutoRepository : MobiliVendas.Core.Infra.Repositories.ProdutoRepository
    {
        public ProdutoRepository(ISqliteConnection context) : base(context)
        {
        }

        public override async Task<List<ItemCommandResult>> BuscarProdutosDoModelo(ModeloCommandResult command, AtendimentoCommandResult atendimento, string CodEstoque)
        {
            string sql = Pegada.Core.ManagerQuery.MakeSql("PRO_PRODUTOS_MODELO_GET", "Query", new
            {
                CodModelo = command.CodModelo,
                CodProdutoModelo = command.CodProdutoModelo,
                CodTabelaPreco = atendimento?.CodTabelaPreco,
                CodAtendimento = atendimento?.CodAtendimento,
                ItensEmAtendimento = atendimento == null ? -1 : atendimento.ItensEmAtendimento
            });
            var result = await _sqlAsyncConnection.QueryAsync<ItemCommandResult>(sql);
            if (result != null)
            {
                foreach (var prod in result)
                {
                    var lstGrades = await BuscarGradesDoProduto(new BuscarGradesProdutoCommand(atendimento?.CodAtendimento, prod.CodProduto, CodEstoque));
                    foreach (var grade in lstGrades)
                    {
                        grade.CodProduto = prod.CodProduto;
                        prod.Grades.Add(grade);
                    }

                    prod.QtdTotal = prod.Grades.Sum(x => x.Qtd);
                }
            }

            return result;
        }

        public override async Task<List<DerivacaoGradeResult>> BuscarGradesDoProduto(BuscarGradesProdutoCommand command)
        {
            string sql = Pegada.Core.ManagerQuery.MakeSql("PRO_GRADES_PRODUTO_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<DerivacaoGradeResult>(sql);
            return result;
        }

        public override async Task<List<DerivacaoGradeResult>> BuscarTamanhosPossiveisDoProduto(BuscarGradesProdutoCommand command)
        {
            string sql = Pegada.Core.ManagerQuery.MakeSql("PRO_TAMANHOS_POSSIVEIS_PRODUTO_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<DerivacaoGradeResult>(sql);
            return result;
        }

        public override async Task<List<EstoqueResult>> BuscarEstoques(BuscarEstoquesCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_ESTOQUES_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<EstoqueResult>(sql);
            return result;
        }

        public virtual async Task<List<EstoqueResult>> BuscarEstoquesUnificado(BuscarEstoquesCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_ESTOQUES_UNIFICADO_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<EstoqueResult>(sql);
            return result;
        }
        public override async Task<List<GenericComboResult>> BuscarCDProduto(BuscarCDProdutoCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_CD_PRODUTO_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
            return result;
        }
    }
}
