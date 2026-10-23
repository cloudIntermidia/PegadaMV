using System;
using System.Collections.Generic;
using System.Linq;
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

        // Rodapé com as regras do TituloViewController (consultar) + Titulo.titulosForUsuario do PegadaIOS. A base soma o
        // ValorAReceber (saldo) em todos os campos, que é 0 no título pago: Valor Total e Total liquidado saíam zerados.
        public override Task<TituloTotalModeloCommandResult> BuscarTituloTotal(List<TituloCommandResult> command)
        {
            var hoje = DateTime.Now.Date;
            var retorno = new TituloTotalModeloCommandResult
            {
                ValorTotal = command.Sum(x => x.ValorOriginal),
                ValorEmAberto = command.Sum(x => x.ValorAReceber),
                // ValorAVencer/ValorVencido: CASE WHEN T.ValorAberto > 0 AND T.DataVencimento >= / < DATE('NOW') THEN T.ValorAberto
                ValorAVencer = command.Where(x => x.ValorAReceber > 0 && x.DataVencimento.Date >= hoje).Sum(x => x.ValorAReceber),
                ValorVencido = command.Where(x => x.ValorAReceber > 0 && x.DataVencimento.Date < hoje).Sum(x => x.ValorAReceber),
                ValorLiquidado = command.Sum(x => x.ValorPago),
                QtdTotal = command.Count
            };
            return Task.FromResult(retorno);
        }
    }
}
