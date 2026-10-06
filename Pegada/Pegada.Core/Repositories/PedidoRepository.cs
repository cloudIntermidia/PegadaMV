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
        }

        private class GradeTelaRow : DerivacaoGradeResult
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

            foreach (var item in itens)
            {
                item.DescontoTotalUI = CalcularDescontoTotal(item.DescTipo, item.DescPerc);
                foreach (var grade in grades[item.CodItemPedido])
                    item.Grades.Add(grade);
            }

            return new ObservableCollection<ItemCommandResult>(itens);
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
