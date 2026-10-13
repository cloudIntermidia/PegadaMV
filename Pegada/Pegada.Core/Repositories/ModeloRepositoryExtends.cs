using MobiliVendas.Core;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Infra.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
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

            //precisa aplicar algumas regras aqui
            //string where = await MontarWhereNiveis(command);
            //command.WhereNiveis = where;

            string sql = ManagerQuery.MakeSql("PRO_MODELO_MESMA_LINHA_GET",
                                             "Query",
                                                new
                                                {
                                                    CodModelo = command.CodModelo,
                                                    CodProdutoModelo = command.CodProdutoModelo,
                                                    CodTabelaPreco = atendimento?.CodTabelaPreco == null ? "-1" : atendimento?.CodTabelaPreco,
                                                    CodAtendimento = atendimento?.CodAtendimento,
                                                    ItensEmAtendimento = atendimento == null ? -1 : atendimento.ItensEmAtendimento,
                                                    ValidaEstoque = "-1",
                                                    Familia = command.Familia,
                                                    RestricaoLocal = restricaoLocal,
                                                    CodPessoa = atendimento?.CodTabelaPreco == null ? command.CodPessoa : "-1",
                                                    CodPessoaCD = command.CodPessoaCD,
                                                });
            var result = await _sqlAsyncConnection.QueryAsync<ModeloCommandResult>(sql);
            return result;
        }

        // Lista da Pronta Entrega = ItemProntaEntrega.m setFiltersPE:(NSDictionary *) do PegadaIOS (query própria do
        // Pegada em DataBase/Query/PRO_MODELO_PE_GET). Níveis (segmento, linha, estilo, cabedal...), promocional,
        // novidade, mídia, mídia aux., recomendação e campeão continuam no MontarWhereNiveis; estoque (EF/ED/AVUL),
        // favoritos e CD são montados aqui com as regras do iOS.
        public override async Task<List<ModeloCommandResult>> BuscarModelosProntaEntrega(BuscarModeloCommand command)
        {
            var filtrosOriginais = command.FiltrosSelecionados;
            bool estoqueFuturo = command.SomenteEstoqueFuturoMF;
            bool estoqueDisponivel = command.SomenteEstoqueDisponivelMF;
            bool estoqueAvulso = command.EstoqueAvulsoMF;

            // CD: TBT_FABRICA do estoque (fabrica => VEP.codFabrica, Defeito #22312), fora do filtro de níveis do produto.
            string whereCD = string.Empty;
            if (command.FiltroCDPeloEstoquePE && filtrosOriginais?.Any(f => f.CodNivel == "7") == true)
            {
                var cds = filtrosOriginais.Where(f => f.CodNivel == "7" && !string.IsNullOrEmpty(f.CodAtributo))
                                          .Select(f => $"'{f.CodAtributo.Replace("'", "''")}'").Distinct();
                whereCD = $" AND VEP.CodFabrica IN ({string.Join(",", cds)}) ";
                command.FiltrosSelecionados = filtrosOriginais.Where(f => f.CodNivel != "7").ToList();
            }

            string where;
            command.IsQueryPE = true;
            command.SomenteEstoqueFuturoMF = command.SomenteEstoqueDisponivelMF = command.EstoqueAvulsoMF = false;
            try
            {
                where = await MontarWhereNiveis(command);
            }
            finally
            {
                command.IsQueryPE = false;
                command.FiltrosSelecionados = filtrosOriginais;
                command.SomenteEstoqueFuturoMF = estoqueFuturo;
                command.SomenteEstoqueDisponivelMF = estoqueDisponivel;
                command.EstoqueAvulsoMF = estoqueAvulso;
            }

            // Na query do Pegada o nível 4 é LEFT JOIN (NIVEL4 do iOS); os níveis filtram pelo produto.
            where = where.Replace("AND NP.CodProduto IN", "AND P.CodProduto IN");

            // EF / ED / AVUL marcados juntos se somam (OU), cada um olhando o próprio lote do VEP.
            var estoques = new List<string>();
            if (estoqueAvulso)
                estoques.Add("VEP.IndEstoqueAvulso > 0");
            if (estoqueFuturo)
                estoques.Add("VEP.IndEstoqueFuturo > 0");
            if (estoqueDisponivel)
                estoques.Add("(VEP.IndEstoqueDisponivel > 0 AND IFNULL(VEP.IndEstoqueAvulso, 0) = 0)");
            if (estoques.Count > 0)
                where += $" AND ({string.Join(" OR ", estoques)}) ";

            // Favoritos (WISHLIST do atendimento).
            if (command.FavoritosMF)
                where += " AND IA.CodProduto IS NOT NULL ";

            where += whereCD;

            var usuario = Session.USUARIO_LOGADO;
            string representante = usuario?.CodTipoPessoa == "3" ? usuario.CodRepresentantePreposto : usuario?.CodPessoa;
            bool temAtendimento = Session.ATENDIMENTO_ATUAL != null;

            string sql = ManagerQuery.MakeSql("PRO_MODELO_PE_GET", "Query", new
            {
                PeTemAtendimento = temAtendimento ? "1" : "0",
                PeTabelaPreco = temAtendimento ? Session.ATENDIMENTO_ATUAL.CodTabelaPreco : null,
                PeAtendimento = temAtendimento ? Session.ATENDIMENTO_ATUAL.CodAtendimento : "1",
                PePessoaCD = command.CodigoPessoaCD,
                PeRepresentante = representante,
                PeUsuario = usuario?.CodPessoa,
                PePesquisa = string.IsNullOrWhiteSpace(command.FiltroPesquisa) ? null : command.FiltroPesquisa.Replace("'", "''"),
                PeWhere = where
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
