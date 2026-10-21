using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Infra.DataContext;
using MobiliVendas.Core.DataBase;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using SQLite;
using System.Collections.ObjectModel;

namespace Pegada.Core.Repositories
{
    public class PedidoRepository : MobiliVendas.Core.Infra.Repositories.PedidoRepository
    {
        protected readonly SQLiteAsyncConnection _sqlAsyncConnection;
        protected readonly ICarrinhoRepository _carrinhoRepository;

        public PedidoRepository(ISqliteConnection context, ICarrinhoRepository carrinhoRepository) : base(context, carrinhoRepository)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
            _carrinhoRepository = carrinhoRepository;
        }
        public override async Task<List<CarrinhoCommandResult>> GetPedidos(FiltrosPedidoCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_PEDIDO_GET", "Query", command);
            var pedidos = await _sqlAsyncConnection.QueryAsync<CarrinhoCommandResult>(sql);
            return pedidos;
        }
        // Situações sem o código na frente da descrição ("Aberto", não "(1) Aberto").
        public override async Task<List<GenericComboResult>> BuscarSituacoes(FiltrosPedidoCommand command)
        {
            string sql = ManagerQuery.MakeSql("COMBO_SITUACAO_PEDIDO", "Query.Filtros", command);
            return await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        }
        // PegadaIOS (FiltrosPedido setupWithTipos): todos os tipos de TIPO_PEDIDO ordenados por descrição.
        public override async Task<List<GenericComboResult>> BuscarTipoPedido(FiltrosPedidoCommand command)
        {
            string sql = ManagerQuery.MakeSql("COMBO_TIPO_PEDIDO", "Query.Filtros", command);
            var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
            return result;
        }
        // PegadaIOS (InformacoesPedidoViewController): prazo, semana, transportadora, faturamentos e observações do pedido.
        public override async Task<InformacoesPedidoResult> BuscarInformacoesPedido(CarrinhoCommandResult pedido)
        {
            bool carrinho = pedido.Origem == "C";
            string sql = ManagerQuery.MakeSql("PRO_PEDIDO_INFO", "Query", new { Origem = carrinho ? "C" : "P", Codigo = carrinho ? pedido.CodCarrinho : pedido.CodPedido });
            var result = await _sqlAsyncConnection.QueryAsync<InformacoesPedidoResult>(sql);
            return result.Count > 0 ? result[0] : null;
        }
        // Itens completos do pedido (usado pela impressão da PedidoPage). Carrinho: usa o PRO_ITEM_CARRINHO_GET do Pegada
        // (PVL = PrecoPVL), o mesmo da impressão pela tela Carrinho. O BuscarItensCarrinho do CarrinhoRepository do Pegada
        // é "virtual" (esconde, não sobrescreve), então pela interface ICarrinhoRepository cairia na query do MobiliVendas.Core.
        public override async Task<ObservableCollection<ItemCommandResult>> GetItensPedidos(CarrinhoCommandResult ped)
        {
            if (ped.Origem != "C" || !(_carrinhoRepository is CarrinhoRepository carrinhoRepositoryPegada))
            {
                // Pedido integrado: o PRO_ITEM_PEDIDO_GET do MobiliVendas.Core não traz PVL. PegadaIOS (ItemPedido.m):
                // ITEM_TABELA_PRECO.PrecoPVL da tabela de preço do pedido.
                var itensPedido = await base.GetItensPedidos(ped);
                if (!string.IsNullOrEmpty(ped.CodTabelaPreco) && itensPedido.Count > 0)
                {
                    var pvls = await _sqlAsyncConnection.QueryAsync<PvlRow>(
                        $"SELECT CodProduto, PrecoPVL FROM TBT_ITEM_TABELA_PRECO WHERE CodTabelaPreco = '{ped.CodTabelaPreco}' " +
                        $"AND CodProduto IN ({string.Join(",", itensPedido.Select(i => $"'{i.CodProduto}'").Distinct())})");
                    var pvlPorProduto = pvls.GroupBy(p => p.CodProduto).ToDictionary(g => g.Key, g => g.First().PrecoPVL ?? 0);
                    foreach (var item in itensPedido)
                        if (item.CodProduto != null && pvlPorProduto.TryGetValue(item.CodProduto, out var pvl))
                            item.PrecoSugestao = pvl;
                }

                // PegadaIOS (ItemPedido itensForPedido + carregaDescontoItemPedido): descontos do item = PerDs1..PerDs5 do
                // ITEM_PEDIDO (o PRO_ITEM_PEDIDO_GET do MobiliVendas.Core só traz PerDs1) e total = soma simples deles.
                // São o bloco "Descontos" do PDF e os PERCENTUALDESCONTO1..5 do Excel.
                if (ped.Origem != "C" && itensPedido.Count > 0)
                {
                    var descontos = (await _sqlAsyncConnection.QueryAsync<DescontoItemPedidoRow>(
                        "SELECT CodItemPedido, PerDs1, PerDs2, PerDs3, PerDs4, PerDs5 FROM TBT_ITEM_PEDIDO " +
                        "WHERE CodPedido = ? AND CodItemPedido NOT LIKE '%.%'", ped.CodPedido))
                        .GroupBy(d => d.CodItemPedido)
                        .ToDictionary(g => g.Key, g => g.First());
                    foreach (var item in itensPedido)
                    {
                        if (item.CodItemPedido == null || !descontos.TryGetValue(item.CodItemPedido, out var d))
                            continue;
                        item.PercDesc1 = d.PerDs1 ?? 0;
                        item.PercDesc2 = d.PerDs2 ?? 0;
                        item.PercDesc3 = d.PerDs3 ?? 0;
                        item.PercDesc4 = d.PerDs4 ?? 0;
                        item.PercDesc5 = d.PerDs5 ?? 0;
                        item.DescontoTotalUI = (item.PercDesc1 + item.PercDesc2 + item.PercDesc3 + item.PercDesc4 + item.PercDesc5)
                            .ToString("0.##########", CultureInfo.InvariantCulture);
                    }
                }

                // PegadaIOS (ItemPedido itensForPedido / itensForToImpressao): ValorTotal = ValorUnitario * (QtdTotal * QtdCaixa) e
                // ValorTotalLiquido = ValorUnitarioLiquido * (QtdTotal * QtdCaixa). O PRO_ITEM_PEDIDO_GET compartilhado traz o
                // líquido * QtdTotal (sem as caixas) e não traz o total líquido: coluna Valor Total e rodapé do PDF ficavam errados.
                if (ped.Origem != "C")
                {
                    foreach (var item in itensPedido)
                    {
                        decimal qt = item.QtdTotal * item.QtdCaixa;
                        item.ValorTotal = item.ValorUnitario * qt;
                        item.ValorTotalLiquido = item.PrecoCustoComDesconto * qt;
                    }
                }

                // PegadaIOS (GerarPdf geraPdfWithPedidoChecked / WebservicePedidoHelper): grade do item vem da DistribuicaoGrade;
                // só sem distribuição fica a da GRADE_ITEM_PEDIDO (já carregada pelo base).
                if (ped.Origem != "C" && itensPedido.Count > 0)
                {
                    var distribuicao = await BuscarGradesDistribuicaoPedido(ped, await BuscarCodPaisGrade(ped.CodPessoaCliente));
                    foreach (var item in itensPedido)
                    {
                        var gradesItem = distribuicao[item.CodItemPedido].ToList();
                        if (gradesItem.Count == 0)
                            continue;
                        item.Grades.Clear();
                        foreach (var grade in gradesItem)
                            item.Grades.Add(grade);
                    }
                }
                return itensPedido;
            }

            var itens = await carrinhoRepositoryPegada.BuscarItensCarrinho(new BuscarItensCarrinhoCommand(ped.CodCarrinho, ped.CodTabelaPreco, ped.CodigoSegmento));
            foreach (var item in itens)
            {
                var grades = await _carrinhoRepository.BuscarGradesDoItem(new BuscarGradesItemCommand(ped.CodCarrinho, item.CodItemCarrinho));
                foreach (var grade in grades)
                    item.Grades.Add(grade);
            }
            return itens;
        }

        private class DescontoItemPedidoRow
        {
            public string CodItemPedido { get; set; }
            public decimal? PerDs1 { get; set; }
            public decimal? PerDs2 { get; set; }
            public decimal? PerDs3 { get; set; }
            public decimal? PerDs4 { get; set; }
            public decimal? PerDs5 { get; set; }
        }

        private class PvlRow
        {
            public string CodProduto { get; set; }
            public decimal? PrecoPVL { get; set; }
        }

        // Linhas das consultas de itens/grade da tela de pedidos (colunas extras usadas só aqui).
        private class ItemTelaRow : ItemCommandResult
        {
            public string DescTipo { get; set; }
            public string DescPerc { get; set; }
            public string DistribuicaoGrade { get; set; }
        }

        private class DescricaoDerivacaoRow
        {
            public string CodDerivacao { get; set; }
            public string Descricao { get; set; }
            public string DescricaoOriginal { get; set; }
        }

        public class GradeTelaRow : DerivacaoGradeResult
        {
            public string ItemChave { get; set; }
            public string CodGradeItem { get; set; }
        }

        // PegadaIOS (ItemPedido itensForPedido:filtros: + GradeItemPedido gradesForItemPedidoProducao:andClientePais:):
        // itens do pedido/carrinho filtrados pelo painel "+ Filtros", com desconto total, situação, NF e grade por país do cliente.
        public override async Task<ObservableCollection<ItemCommandResult>> GetItensPedidosTela(CarrinhoCommandResult pedido, FiltrosPedidoCommand filtros)
        {
            bool carrinho = pedido.Origem == "C";
            string codigo = carrinho ? pedido.CodCarrinho : pedido.CodPedido;
            string origem = carrinho ? "C" : "P";

            string sqlItens = ManagerQuery.MakeSql("PRO_ITEM_PEDIDO_TELA", "Query", new
            {
                Origem = origem,
                Codigo = codigo,
                FiltroGrupoSegmento = filtros?.FiltroGrupoSegmento,
                FiltroSegmento = filtros?.FiltroSegmento,
                FiltroGenero = filtros?.FiltroGenero,
                FiltroPublico = filtros?.FiltroPublico,
                FiltroLinha = filtros?.FiltroLinha,
                Referencia = filtros?.Referencia
            });
            var itens = await _sqlAsyncConnection.QueryAsync<ItemTelaRow>(sqlItens);

            string codPais = await BuscarCodPaisGrade(pedido.CodPessoaCliente);
            string sqlGrades = ManagerQuery.MakeSql("PRO_GRADE_ITEM_PEDIDO_TELA", "Query", new { Origem = origem, Codigo = codigo, CodPais = codPais });
            var grades = (await _sqlAsyncConnection.QueryAsync<GradeTelaRow>(sqlGrades)).ToLookup(g => g.ItemChave);
            var descricoes = await BuscarDescricoesDerivacao(itens.Select(i => i.DistribuicaoGrade), codPais);

            foreach (var item in itens)
            {
                item.DescontoTotalUI = CalcularDescontoTotal(item.DescTipo, item.DescPerc);

                // PegadaIOS (ItemPedidoTableViewController): usuario.indControleComissao == 1 esconde o % de comissão do item.
                if (MobiliVendas.Core.Session.USUARIO_LOGADO?.IndControleComissao == true)
                    item.PercComissaoItem = null;

                // PegadaIOS (ItemPedidoTableViewController loadGradeForItem:): usa a grade montada da DistribuicaoGrade;
                // sem distribuição (carrinho ou item sem a coluna), cai na GRADE_ITEM_PEDIDO/GRADE_ITEM_CARRINHO.
                var gradesItem = MontarGradePorDistribuicao(item.DistribuicaoGrade, item.CodItemPedido, descricoes);
                if (gradesItem.Count == 0)
                    gradesItem = grades[item.CodItemPedido].ToList();

                foreach (var grade in gradesItem)
                    item.Grades.Add(grade);
            }

            return new ObservableCollection<ItemCommandResult>(itens);
        }

        private class DistribuicaoItemRow
        {
            public string CodItemPedido { get; set; }
            public string DistribuicaoGrade { get; set; }
            public decimal? QtdCaixa { get; set; }
        }

        // Grades de todos os itens do pedido (TBT_ITEM_PEDIDO) montadas da DistribuicaoGrade, indexadas por CodItemPedido.
        // PegadaIOS: produção usa itensForPedido (qtdCaixa 1 = quantidade por caixa); Pronta Entrega (tipo 7) usa
        // itensForToImpressao (quantidade × caixas do item). Item sem distribuição não aparece no lookup.
        private async Task<ILookup<string, GradeTelaRow>> BuscarGradesDistribuicaoPedido(CarrinhoCommandResult pedido, string codPais)
        {
            bool prontaEntrega = pedido.CodTipoPedido == "7";
            var linhas = await _sqlAsyncConnection.QueryAsync<DistribuicaoItemRow>(
                "SELECT CodItemPedido, DistribuicaoGrade, QtdCaixa FROM TBT_ITEM_PEDIDO " +
                "WHERE CodPedido = ? AND (Excluido IS NULL OR Excluido = 0) AND IFNULL(DistribuicaoGrade, '') <> ''", pedido.CodPedido);
            var descricoes = await BuscarDescricoesDerivacao(linhas.Select(l => l.DistribuicaoGrade), codPais);

            return linhas
                .SelectMany(l => MontarGradePorDistribuicao(l.DistribuicaoGrade, l.CodItemPedido, descricoes,
                    prontaEntrega ? (int)(l.QtdCaixa ?? 0) : 1))
                .ToLookup(g => g.ItemChave);
        }

        // Cópia do pedido (CopiaCarrinhoViewController → itensForPedido): grade do item do pedido pela DistribuicaoGrade
        // (quantidade por caixa, tamanho BR); sem distribuição, a da GRADE_ITEM_PEDIDO.
        public async Task<List<DerivacaoGradeResult>> BuscarGradesItemPedidoCopia(string codPedido, string codItemPedido)
        {
            string distribuicao = await _sqlAsyncConnection.ExecuteScalarAsync<string>(
                "SELECT DistribuicaoGrade FROM TBT_ITEM_PEDIDO WHERE CodPedido = ? AND CodItemPedido = ? AND IFNULL(DistribuicaoGrade, '') <> '' LIMIT 1",
                codPedido, codItemPedido);

            var grades = MontarGradePorDistribuicao(distribuicao, codItemPedido,
                await BuscarDescricoesDerivacao(new[] { distribuicao }, "1"));
            if (grades.Count > 0)
                return grades.Cast<DerivacaoGradeResult>().ToList();

            return await BuscarGradesDoItemPedido(new BuscarGradesItemCommand(codPedido, codItemPedido));
        }

        // PegadaIOS (GradeItemPedido montaGradePorItem:andPedido:andItem:andQtdCaixa:andClientePais:):
        // "5:1|6:1|7:2" = CodDerivacao:Qtd por caixa, na ordem da string; CodGradeItemPedido = posição (1, 2, ...); Qtd × qtdCaixa.
        private static List<GradeTelaRow> MontarGradePorDistribuicao(string distribuicaoGrade, string codItem, Dictionary<string, DescricaoDerivacaoRow> descricoes, int qtdCaixa = 1)
        {
            var grades = new List<GradeTelaRow>();
            if (string.IsNullOrWhiteSpace(distribuicaoGrade))
                return grades;

            var tamanhos = distribuicaoGrade.Split('|');
            for (int index = 0; index < tamanhos.Length; index++)
            {
                var partes = tamanhos[index].Split(':');
                string codDerivacao = partes[0].Trim();
                decimal.TryParse(partes.Length > 1 ? partes[1].Trim() : null, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd);

                descricoes.TryGetValue(codDerivacao, out var descricao);
                grades.Add(new GradeTelaRow
                {
                    ItemChave = codItem,
                    CodGradeItem = (index + 1).ToString(),
                    CodGradeItemCarrinho = index + 1,
                    CodDerivacao = codDerivacao,
                    Qtd = Math.Truncate(qtd) * qtdCaixa,
                    Descricao = descricao?.Descricao ?? " ",
                    DescricaoOriginal = descricao?.DescricaoOriginal
                });
            }
            return grades;
        }

        // PegadaIOS (DerivacaoGrade descricoesForCode:andClientePais:): BR = DERIVACAO.Descricao; outro país =
        // DERIVACAO_PAIS.DescricaoDerivacao (sem tradução fica em branco, como no iOS) e a BR em DescricaoOriginal.
        private async Task<Dictionary<string, DescricaoDerivacaoRow>> BuscarDescricoesDerivacao(IEnumerable<string> distribuicoes, string codPais)
        {
            var codigos = distribuicoes
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .SelectMany(d => d.Split('|'))
                .Select(t => t.Split(':')[0].Trim().Replace("'", "''"))
                .Where(c => c.Length > 0)
                .Distinct()
                .ToList();
            if (codigos.Count == 0)
                return new Dictionary<string, DescricaoDerivacaoRow>();

            string sql = codPais == "1"
                ? "SELECT D.CodDerivacao, D.Descricao, NULL AS DescricaoOriginal FROM TBT_DERIVACAO D "
                : "SELECT D.CodDerivacao, IFNULL(DP.DescricaoDerivacao, ' ') AS Descricao, D.Descricao AS DescricaoOriginal FROM TBT_DERIVACAO D " +
                  $"LEFT JOIN TBT_DERIVACAO_PAIS DP ON DP.CodDerivacao = D.CodDerivacao AND DP.CodPais = '{codPais}' ";
            sql += $"WHERE D.CodDerivacao IN ({string.Join(",", codigos.Select(c => $"'{c}'"))})";

            var linhas = await _sqlAsyncConnection.QueryAsync<DescricaoDerivacaoRow>(sql);
            return linhas.GroupBy(l => l.CodDerivacao).ToDictionary(g => g.Key, g => g.First());
        }

        // PegadaIOS (GradeItemPedido gradesForItemPedidoProducao:andClientePais:): grade de todos os itens do pedido/carrinho
        // no país do cliente, indexada pelo item (CodItemPedido/CodItemCarrinho). Usada no Excel do pedido.
        // Pedido (WebservicePedidoHelper): item com DistribuicaoGrade usa a grade montada dela; os demais, a GRADE_ITEM_PEDIDO.
        public async Task<ILookup<string, GradeTelaRow>> BuscarGradesPorPaisCliente(CarrinhoCommandResult pedido)
        {
            bool carrinho = pedido.Origem == "C";
            string codPais = await BuscarCodPaisGrade(pedido.CodPessoaCliente);
            string sql = ManagerQuery.MakeSql("PRO_GRADE_ITEM_PEDIDO_TELA", "Query", new
            {
                Origem = carrinho ? "C" : "P",
                Codigo = carrinho ? pedido.CodCarrinho : pedido.CodPedido,
                CodPais = codPais
            });
            var grades = await _sqlAsyncConnection.QueryAsync<GradeTelaRow>(sql);
            if (carrinho)
                return grades.ToLookup(g => g.ItemChave);

            var distribuicao = await BuscarGradesDistribuicaoPedido(pedido, codPais);
            return grades
                .Where(g => !distribuicao.Contains(g.ItemChave))
                .Concat(distribuicao.SelectMany(g => g))
                .ToLookup(g => g.ItemChave);
        }

        // PegadaIOS: país do cliente (sem país = BR "1"); país sem nenhuma derivação cadastrada usa a grade BR (Defeito #21271).
        private async Task<string> BuscarCodPaisGrade(string codPessoaCliente)
        {
            string codPais = await _sqlAsyncConnection.ExecuteScalarAsync<string>(
                "SELECT CodPais FROM TBT_CLIENTE WHERE CodPessoaCliente = ?", codPessoaCliente);
            if (string.IsNullOrEmpty(codPais) || codPais == "1")
                return "1";

            int derivacoes = await _sqlAsyncConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM TBT_DERIVACAO_PAIS WHERE CodPais = ?", codPais);
            return derivacoes > 0 ? codPais : "1";
        }

        // Pedido: soma simples dos percentuais (carregaDescontoItemPedido). Carrinho: desconto composto em % com 2 casas
        // (carregaDescontoItemCarrinho).
        private static string CalcularDescontoTotal(string tipo, string percentuais)
        {
            var valores = (percentuais ?? string.Empty)
                .Split(';')
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (decimal?)null)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .ToList();

            if (tipo == "C")
            {
                decimal multiplicador = 1m;
                foreach (var d in valores)
                    multiplicador *= 1m - d / 100m;
                return ((1m - multiplicador) * 100m).ToString("N2", new CultureInfo("pt-BR"));
            }

            return valores.Sum().ToString("0.##########", CultureInfo.InvariantCulture);
        }

        // PegadaIOS (Preposto prepostoPedidoForRepresentante): prepostos com pedidos dos clientes do representante.
        public override async Task<List<GenericComboResult>> BuscarVendedores(FiltrosPedidoCommand command)
        {
            string sql = ManagerQuery.MakeSql("COMBO_VENDEDORES_PEDIDO", "Query.Filtros", command);
            return await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        }

        public override async Task<List<GenericComboResult>> BuscarAllVendedores(FiltrosPedidoCommand command)
        {
            return await BuscarVendedores(command);
        }

        // PegadaIOS (GrupoCliente gruposForUsuario): grupos dos clientes do usuário (vendedor ou representante) em MARCA_CLIENTE.
        public override async Task<List<GenericComboResult>> BuscarGrupoDeClientes(FiltrosPedidoCommand command)
        {
            string sql = ManagerQuery.MakeSql("COMBO_GRUPO_CLIENTES_PEDIDO", "Query.Filtros", command);
            return await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        }

        public override async Task<List<DerivacaoGradeResult>> BuscarGradesDoItemPedido(BuscarGradesItemCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_GRADE_ITEM_PEDIDO_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<DerivacaoGradeResult>(sql);
            return result;
        }
        //public virtual async Task<ObservableCollection<ItemCommandResult>> GetItensPedidos(CarrinhoCommandResult ped)
        //{
        //    ObservableCollection<ItemCommandResult> itens;
        //    if (ped.Origem == "C")
        //    {
        //        itens = await _carrinhoRepository.BuscarItensCarrinho(new BuscarItensCarrinhoCommand(ped.CodCarrinho, ped.CodTabelaPreco));
        //        foreach (var item in itens)
        //        {
        //            var grades = await _carrinhoRepository.BuscarGradesDoItem(new BuscarGradesItemCommand(ped.CodCarrinho, item.CodItemCarrinho));
        //            foreach (var grade in grades)
        //            {
        //                item.Grades.Add(grade);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        itens = await BuscarItensPedido(new BuscarItensCarrinhoCommand(ped.CodPedido, ped.CodTabelaPreco));
        //        foreach (var item in itens)
        //        {
        //            var grades = await BuscarGradesDoItemPedido(new BuscarGradesItemCommand(ped.CodPedido, item.CodItemPedido));
        //            foreach (var grade in grades)
        //            {
        //                item.Grades.Add(grade);
        //            }
        //        }
        //    }

        //    return itens;
        //}
        //public virtual async Task<ObservableCollection<ItemCommandResult>> BuscarItensPedido(BuscarItensCarrinhoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("PRO_ITEM_PEDIDO_GET", "Query", command);
        //    var itens = await _sqlAsyncConnection.QueryAsync<ItemCommandResult>(sql);
        //    return new ObservableCollection<ItemCommandResult>(itens);
        //}
        //public virtual async Task<List<DerivacaoGradeResult>> BuscarGradesDoItemPedido(BuscarGradesItemCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("PRO_GRADE_ITEM_PEDIDO_GET", "Query", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<DerivacaoGradeResult>(sql);
        //    return result;
        //}

        //public virtual async Task<List<GenericComboResult>> BuscarTipoPedido(FiltrosPedidoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("COMBO_TIPO_PEDIDO", "Query.Filtros", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        //    return result;
        //}
        //public virtual async Task<List<GenericComboResult>> BuscarClientes(FiltrosPedidoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("COMBO_CLIENTES_PEDIDO", "Query.Filtros", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        //    return result;
        //}
        //public virtual async Task<List<GenericComboResult>> BuscarVendedores(FiltrosPedidoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("COMBO_VENDEDORES_PEDIDO", "Query.Filtros", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        //    return result;
        //}
        //public virtual async Task<List<GenericComboResult>> BuscarSituacoes(FiltrosPedidoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("COMBO_SITUACAO_PEDIDO", "Query.Filtros", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        //    return result;
        //}
        //public virtual async Task<List<GenericComboResult>> BuscarGrupoDeClientes(FiltrosPedidoCommand command)
        //{
        //    string sql = ManagerQuery.MakeSql("COMBO_GRUPO_CLIENTES_PEDIDO", "Query.Filtros", command);
        //    var result = await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        //    return result;
        //}
    }
}
