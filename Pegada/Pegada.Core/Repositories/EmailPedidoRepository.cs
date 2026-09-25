using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;

namespace Pegada.Core.Repositories
{
    // Destinatários do e-mail automático do pedido transmitido (Contato getEmailsContatoForCliente: do PegadaIOS).
    public class EmailPedidoRepository
    {
        private readonly SQLiteAsyncConnection _sqlAsyncConnection;

        public EmailPedidoRepository(ISqliteConnection context)
        {
            _sqlAsyncConnection = context.DbConnectionAsync();
        }

        // Contatos do cliente marcados para receber cópia do pedido (CopiaPedido = 1) e não excluídos.
        public async Task<List<string>> BuscarEmailsCopiaPedido(string codPessoaCliente)
        {
            var emails = await _sqlAsyncConnection.QueryAsync<EmailContato>(
                "SELECT Email AS Email FROM TBT_CONTATO " +
                "WHERE CodPessoaCliente = ? AND CopiaPedido = 1 AND IFNULL(IndExcluido, 0) = 0",
                codPessoaCliente);

            return emails
                .Select(e => e.Email?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
        }

        private class EmailContato
        {
            public string Email { get; set; }
        }
    }
}
