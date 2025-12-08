using System.Collections.Generic;
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
