using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Infra.DataContext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pegada.Core.Repositories
{
    public class ModeloRepositoryExtends : MobiliVendas.Core.Infra.Repositories.ModeloRepository
    {
        public ModeloRepositoryExtends(ISqliteConnection context, INivelRepository nivel, IKitRepository kitRepository) : base(context, nivel, kitRepository)
        {
        }

        public override async Task<List<ModeloCommandResult>> BuscarModelosMesmaLinha(ModeloCommandResult command, AtendimentoCommandResult atendimento, string tabelaPrecoPadrao)
        {
            string restricaoLocal = null;

            if (atendimento != null) {
                restricaoLocal = atendimento.RestricaoLocal == "S" ? atendimento.RestricaoLocal : null;
            }

            string sql = ManagerQuery.MakeSql("PRO_MODELO_MESMA_LINHA_GET",
                                             "Query",
                                                new
                                                {
                                                    CodModelo = command.CodModelo,
                                                    CodProdutoModelo = command.CodProdutoModelo,
                                                    CodTabelaPreco = atendimento?.CodTabelaPreco == null ? "1" : atendimento?.CodTabelaPreco,
                                                    CodAtendimento = atendimento?.CodAtendimento,
                                                    ItensEmAtendimento = atendimento == null ? -1 : atendimento.ItensEmAtendimento,
                                                    ValidaEstoque = "-1",
                                                    Familia = command.Familia,
                                                    RestricaoLocal = restricaoLocal
                                                });
            var result = await _sqlAsyncConnection.QueryAsync<ModeloCommandResult>(sql);
            return result;
        }

        public override async Task<List<ModeloCommandResult>> BuscarModelosMesmaLinhaPDF(ModeloCommandResult command, AtendimentoCommandResult atendimento, string tabelaPrecoPadrao)
        {
            string sql = ManagerQuery.MakeSql("PRO_MODELO_MESMA_LINHA_PDF_GET",
                                             "Query",
                                                new
                                                {
                                                    CodModelo = command.CodModelo,
                                                    CodProdutoModelo = command.CodProdutoModelo,
                                                    CodTabelaPreco = atendimento?.CodTabelaPreco == null ? "1" : atendimento?.CodTabelaPreco,
                                                    CodAtendimento = atendimento?.CodAtendimento,
                                                    ItensEmAtendimento = atendimento == null ? -1 : atendimento.ItensEmAtendimento,
                                                    ValidaEstoque = "-1",
                                                    Familia = command.Familia
                                                });
            var result = await _sqlAsyncConnection.QueryAsync<ModeloCommandResult>(sql);
            return result;
        }
    }
}
