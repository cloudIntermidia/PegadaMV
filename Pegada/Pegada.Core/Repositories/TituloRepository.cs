using System.Collections.Generic;
using System.Threading.Tasks;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;

namespace Pegada.Core.Repositories
{
    public class TituloRepository : MobiliVendas.Core.Infra.Repositories.TituloRepository
    {
        private readonly SQLiteAsyncConnection _sqlAsyncConnection;

        public TituloRepository(ISqliteConnection context) : base(context)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
        }

        // Situações sem o código na frente da descrição ("Aberto", não "(1) Aberto").
        public override async Task<List<GenericComboResult>> BuscarSituacoes(FiltrosTituloCommand command)
        {
            string sql = ManagerQuery.MakeSql("COMBO_SITUACAO_TITULO", "Query.Filtros", command);
            return await _sqlAsyncConnection.QueryAsync<GenericComboResult>(sql);
        }
    }
}
