using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Infra.DataContext;

namespace Pegada.Core.Repositories
{
    public class ClienteRepository : MobiliVendas.Core.Infra.Repositories.ClienteRepository
    {
        public ClienteRepository(ISqliteConnection context) : base(context)
        {
        }


        public override async Task<ClienteCommandResult> BuscarCliente(BuscarClienteCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_CLIENTES_GET", "Query", command);
            var result = await _sqlAsyncConnection.QueryAsync<ClienteCommandResult>(sql);
            return result.FirstOrDefault();
        }

    }
}
