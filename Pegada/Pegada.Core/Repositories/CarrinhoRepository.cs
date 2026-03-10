using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Pegada.Core.Entities;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Entities;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;
using System.Runtime.ConstrainedExecution;
using Syncfusion.SfDataGrid.XForms;
using static Dropbox.Api.Files.WriteMode;

namespace Pegada.Core.Repositories
{
    public class CarrinhoRepository : MobiliVendas.Core.Infra.Repositories.CarrinhoRepository
    {
        public CarrinhoRepository(ISqliteConnection context, INivelRepository nivelRepository, ITabelaPrecoRepository tabelaPrecoRepository) : base(context, nivelRepository, tabelaPrecoRepository)
        {
        }

        public override async Task<List<CarrinhoCommandResult>> GetCarrinhos(BuscarCarrinhoCommand command)
        {
            //command.CodPessoaCliente = command.CodPessoaCliente.Replace(".", "").Replace("/", "").Replace("-", "");
            string sql = ManagerQuery.MakeSql("PRO_CARRINHO_GET", "Query", command);
            var carrinhos = await _sqlAsyncConnection.QueryAsync<CarrinhoCommandResult>(sql);

            if (carrinhos?.Count > 0)
            {
                foreach (var car in carrinhos)
                {
                    //tratamento para ajustar erro onde o carrinho fica sem o tipo de deposito.
                    if (string.IsNullOrEmpty(car.CodDeposito)) {
                        string codDeposito = await BuscarDepositoCarrinho(car.CodCarrinho);
                        await AtualizaDepositoCarrinho(car.CodCarrinho, codDeposito);
                    }
                }
            }

            string sqlAtualizado = ManagerQuery.MakeSql("PRO_CARRINHO_GET", "Query", command);
            var carrinhosAtualizado = await _sqlAsyncConnection.QueryAsync<CarrinhoCommandResult>(sqlAtualizado);
            if (carrinhosAtualizado?.Count > 0)
            {
                foreach (var car in carrinhosAtualizado)
                {
                    car.Itens = await BuscarItensCarrinho(new BuscarItensCarrinhoCommand(car.CodCarrinho, car.CodTabelaPreco));

                    foreach (var item in car.Itens)
                    {
                        var grades = await BuscarGradesDoItem(new BuscarGradesItemCommand(car.CodCarrinho, item.CodItemCarrinho));
                        if (car.IndEmAlocacao == 1) {

                            grades = await BuscarGradesDoItemAlocado(new BuscarGradesItemCommand(car.CodCarrinho, item.CodItemCarrinho));

                            foreach (var grade in grades)
                            {
                                item.Grades.Add(grade);
                            }
                        }
                        else {
                            foreach (var grade in grades)
                            {
                                item.Grades.Add(grade);
                            }
                        }
                    }
                }
            }

            return carrinhosAtualizado;
        }

        public virtual async Task<List<CarrinhoPreVenda>> GetCarrinhosPreVenda(BuscarCarrinhoCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_CARRINHO_PRE_VENDA", "Query", command);
            var carrinhos = await _sqlAsyncConnection.QueryAsync<CarrinhoPreVenda>(sql);

            return carrinhos;
        }

        public async Task AtualizaDesconto(decimal desconto, string codCarrinho, decimal codItem)
        {
            string update = ManagerQuery.MakeSql("CARRINHO_UPDATE_DESCONTO_ITEM", "Function", new { Desconto = desconto, CodCarrinho = codCarrinho, CodItemCarrinho = codItem });
            await _sqlAsyncConnection.ExecuteAsync(update);
        }

        public override async Task<bool> AtualizaQtdCarrinho(string codCarrinho)
        {
            int rowsAffected = 0;
            try
            {
                string sql = ManagerQuery.MakeSql("CARRINHO_UPDATE_QTD", "Function", new { CodCarrinho = codCarrinho });
                var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var statement in statements)
                {
                    rowsAffected = await _sqlAsyncConnection.ExecuteAsync(statement);
                }
            }
            catch (SQLiteException sqliteException)
            {
                if (sqliteException.Result == SQLite3.Result.Busy)
                {
                    await AtualizaQtdCarrinho(codCarrinho);
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.Write("CarrinhoRepository => Atualizar QTD Carrinho");
                Debug.Write(ex.Message);
            }


            return rowsAffected > 0;
        }

        public override async Task<WcfPedidoModelInput> BuscarCarrinhoParaTransmissao(string codCarrinho)
        {
            var camposCarrinho = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_CARRINHO);")).Select(x => x.name).ToList();
            string sqlCamposCarrinho = string.Join(",", camposCarrinho);
            var carrinho = (await _sqlAsyncConnection.QueryAsync<Integracao_TBT_CARRINHO>($"SELECT {sqlCamposCarrinho} FROM TBT_CARRINHO WHERE CodCarrinho = '{codCarrinho}';")).FirstOrDefault();
            carrinho.CodSituacaoPedido = "3";

            var camposItem = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_ITEM_CARRINHO);")).Select(x => x.name).ToList();
            string sqlCamposItem = string.Join(",", camposItem);
            sqlCamposItem = sqlCamposItem.Substring(0, sqlCamposItem.Length);
            var itens = await _sqlAsyncConnection.QueryAsync<TBT_ITEM_CARRINHO>($"SELECT {sqlCamposItem} FROM TBT_ITEM_CARRINHO WHERE CodCarrinho = '{codCarrinho}';");
            var grades = await _sqlAsyncConnection.Table<TBT_GRADE_ITEM_CARRINHO>().Where(x => x.CodCarrinho == codCarrinho).ToListAsync();

            WcfPedidoModelInput retorno = new WcfPedidoModelInput();
            foreach (var item in itens)
            {
                item.Grades = new List<TBT_GRADE_ITEM_CARRINHO>();
                var lstGrades = grades.Where(x => x.CodCarrinho == item.CodCarrinho && x.CodItemCarrinho == item.CodItemCarrinho).ToList();
                if (lstGrades != null)
                {
                    item.Grades.AddRange(lstGrades);
                }
            }

            PEDIDOVENDA pedido = new PEDIDOVENDA();
            pedido.Carrinho = carrinho;
            pedido.Itens = itens;

            retorno.PEDIDOVENDA = pedido;
            return retorno;
        }

        public override async Task<WcfPedidoModelInput> BuscarCarrinhoParaTransmissaoCancelado(string codCarrinho, string user, string motivoCancelamento = null)
        {
            var camposCarrinho = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_CARRINHO);")).Select(x => x.name).ToList();
            string sqlCamposCarrinho = string.Join(",", camposCarrinho);
            var carrinho = (await _sqlAsyncConnection.QueryAsync<Integracao_TBT_CARRINHO>($"SELECT {sqlCamposCarrinho} FROM TBT_CARRINHO WHERE CodCarrinho = '{codCarrinho}';")).FirstOrDefault();
            carrinho.CodSituacaoPedido = "3";
            //Tarefa 30914
            carrinho.UserCancel = user;
            carrinho.MotivoCancelamento = motivoCancelamento;

            var camposItem = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_ITEM_CARRINHO);")).Select(x => x.name).ToList();
            string sqlCamposItem = string.Join(",", camposItem);
            sqlCamposItem = sqlCamposItem.Substring(0, sqlCamposItem.Length);
            var itens = await _sqlAsyncConnection.QueryAsync<TBT_ITEM_CARRINHO>($"SELECT {sqlCamposItem} FROM TBT_ITEM_CARRINHO WHERE CodCarrinho = '{codCarrinho}';");
            var grades = await _sqlAsyncConnection.Table<TBT_GRADE_ITEM_CARRINHO>().Where(x => x.CodCarrinho == codCarrinho).ToListAsync();

            WcfPedidoModelInput retorno = new WcfPedidoModelInput();
            foreach (var item in itens)
            {
                item.Grades = new List<TBT_GRADE_ITEM_CARRINHO>();
                var lstGrades = grades.Where(x => x.CodCarrinho == item.CodCarrinho && x.CodItemCarrinho == item.CodItemCarrinho).ToList();
                if (lstGrades != null)
                {
                    item.Grades.AddRange(lstGrades);
                }
            }

            PEDIDOVENDA pedido = new PEDIDOVENDA();
            pedido.Carrinho = carrinho;
            pedido.Itens = itens;

            retorno.PEDIDOVENDA = pedido;
            return retorno;
        }

        public override async Task<List<string>> ValidacoesDoCarrinho(string codCarrinho)
        {
            List<string> lstErros = new List<string>();
            var Itens = await BuscarItensCarrinho(new BuscarItensCarrinhoCommand(codCarrinho, null));

            decimal valorTotalCarrinho = 0;

            foreach (var item in Itens) {
                if (item.PercDesc2 > 0)
                {
                    string sqlInto = $"SELECT PercentualDesconto FROM TBT_ITEM_CARRINHO_DESC WHERE CodCarrinho = '{codCarrinho}' AND SeqItemPedido = '{item.CodItemCarrinho}' AND Acumulador = '2'";
                    decimal valorTotalLiquido = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sqlInto);
                    if (valorTotalLiquido == 0) {
                        lstErros.Add($"Não foi encontrado desconto para o item, {item.CodItemCarrinho} produto {item.CodProduto} ");
                    }
                }

                if (item.PercDesc3 > 0)
                {
                    string sqlInto = $"SELECT PercentualDesconto FROM TBT_ITEM_CARRINHO_DESC WHERE CodCarrinho = '{codCarrinho}' AND SeqItemPedido = '{item.CodItemCarrinho}' AND Acumulador = '3'";
                    decimal valorTotalLiquido = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sqlInto);
                    if (valorTotalLiquido == 0)
                    {
                        lstErros.Add($"Não foi encontrado desconto para o item, {item.CodItemCarrinho} produto {item.CodProduto} ");
                    }
                }

                if (item.PercDesc4 > 0)
                {
                    string sqlInto = $"SELECT PercentualDesconto FROM TBT_ITEM_CARRINHO_DESC WHERE CodCarrinho = '{codCarrinho}' AND SeqItemPedido = '{item.CodItemCarrinho}' AND Acumulador = '4'";
                    decimal valorTotalLiquido = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sqlInto);
                    if (valorTotalLiquido == 0)
                    {
                        lstErros.Add($"Não foi encontrado desconto para o item, {item.CodItemCarrinho} produto {item.CodProduto} ");
                    }
                }

                if (item.PercDesc5 > 0)
                {
                    string sqlInto = $"SELECT PercentualDesconto FROM TBT_ITEM_CARRINHO_DESC WHERE CodCarrinho = '{codCarrinho}' AND SeqItemPedido = '{item.CodItemCarrinho}' AND Acumulador = '5'";
                    decimal valorTotalLiquido = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sqlInto);
                    if (valorTotalLiquido == 0)
                    {
                        lstErros.Add($"Não foi encontrado desconto para o item, {item.CodItemCarrinho} produto {item.CodProduto} ");
                    }
                }

                valorTotalCarrinho += item.QtdTotal * item.ValorTotalLiquido;
            }


            //################################################################################
            //ESSE BLOCO ESTA COMENTADO, POIS FOI VISTO QUE NÃO HA VALOR NA TABELA DE UF
            //string sql = $"SELECT " +
            //           "   UF.ValorMinimo " +
            //           " FROM TBT_UF " +
            //           " INNER JOIN TBT_ENDERECO_CLIENTE EC ON EC.UF = UF.Descricao " +
            //           " INNER JOIN TBT_CLIENTE CLI ON  CLI.CodPessoaCliente = EC.CodPessoaCliente " +
            //           " INNER JOIN TBT_CARRINHO C ON  C.CodPessoaCliente = CLI.CodPessoaCliente " +
            //           $" WHERE C.CodCarrinho = '{codCarrinho}';"; 
            //var ValorMinimo = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sql);

            //if (ValorMinimo < valorTotalCarrinho)
            //{
            //    await AtualizaFreteCarrinho(codCarrinho, "F");
            //}
            //else {
            //    await AtualizaFreteCarrinho(codCarrinho, "C");
            //}
            //################################################################################





            //if (tipoFrete == null)
            //{
            //    lstErros.Add("Tipo de frete é obrigatório.");
            //    return lstErros;
            //}

            //sql = ManagerQuery.MakeSql("PRO_VALICAO_PEDIDO_GET", "VALIDACOES", new { CodCarrinho = codCarrinho });
            //ValidacoesPedido validacoesPedido = (await _sqlAsyncConnection.QueryAsync<ValidacoesPedido>(sql)).FirstOrDefault();

            //if (validacoesPedido != null)
            //{
            //    sql = $"SELECT ValorTotalLiquido FROM TBT_CARRINHO WHERE CodCarrinho = '{codCarrinho}';";
            //    decimal valorTotalLiquido = await _sqlAsyncConnection.ExecuteScalarAsync<decimal>(sql);


            //    if (tipoFrete == "C" && validacoesPedido.MinimoCIF > valorTotalLiquido)
            //    {
            //        lstErros.Add($"Valor mínimo de pedido é R$ {validacoesPedido.MinimoCIF.ToString("N2")} para tipo frete CIF ");
            //    }
            //    else if (tipoFrete == "F")
            //    {
            //        sql = $"SELECT CodSituacaoCliente FROM TBT_CARRINHO C" +
            //            $" INNER JOIN TBT_CLIENTE CLI ON CLI.CodPessoaCliente = C.CodPessoaCliente " +
            //            $" WHERE CodCarrinho = '{codCarrinho}';";
            //        string codSituacaoCliente = await _sqlAsyncConnection.ExecuteScalarAsync<string>(sql);

            //        if (codSituacaoCliente == "50" && validacoesPedido.MinimoFobClienteNovo > valorTotalLiquido)
            //        {
            //            lstErros.Add($"Valor mínimo de pedido para cliente novo é R$ {validacoesPedido.MinimoFob.ToString("N2")} para tipo frete FOB ");
            //        }

            //        if (validacoesPedido.MinimoFob > valorTotalLiquido)
            //        {
            //            lstErros.Add($"Valor mínimo de pedido é R$ {validacoesPedido.MinimoFob.ToString("N2")} para tipo frete FOB");
            //        }

            //    }
            //}

            return lstErros;
        }

        public virtual async Task<ObservableCollection<ItemCommandResult>> BuscarItensCarrinho(BuscarItensCarrinhoCommand command)
        {
            string sql = ManagerQuery.MakeSql("PRO_ITEM_CARRINHO_GET", "Query", command);
            var itens = await _sqlAsyncConnection.QueryAsync<ItemCommandResult>(sql);
            return new ObservableCollection<ItemCommandResult>(itens);
        }

        public virtual async Task<string> BuscarDepositoCarrinho(string codCarrinho)
        {
            string sql = "SELECT CASE WHEN UPPER(CodDeposito) = 'IMEDIATO' THEN 1 ELSE 2 END AS CodDeposito " +
                          "  FROM TBT_ITEM_CARRINHO " +
                         $"  WHERE CodCarrinho = '{codCarrinho}' " +
                          "  GROUP BY CodCarrinho " +
                          "  Order By CASE WHEN UPPER(CodDeposito) = 'IMEDIATO' THEN 2 ELSE 1 END; ";
            var carrinho = await _sqlAsyncConnection.QueryAsync<CarrinhoCommandResult>(sql);
            return carrinho.FirstOrDefault().CodDeposito;
        }

        public virtual async Task AtualizaDepositoCarrinho(string codCarrinho, string codDeposito)
        {
            string update = " UPDATE TBT_CARRINHO " +
                         $" SET CodDeposito = '{codDeposito}' " +
                         $" WHERE CodCarrinho = '{codCarrinho}' ; ";
            await _sqlAsyncConnection.ExecuteAsync(update);
        }

        public virtual async Task AtualizaFreteCarrinho(string codCarrinho, string tipoFrete)
        {
            string update = " UPDATE TBT_CARRINHO " +
                        $" SET CifFob = '{tipoFrete}' " +
                        $" WHERE CodCarrinho = '{codCarrinho}'; ";
            await _sqlAsyncConnection.ExecuteAsync(update);
        }

    }
}
