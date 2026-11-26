using MobiliVendas.Core;
using MobiliVendas.Core.Infra.DataContext;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Pegada.Core
{
    public class InicializaBancoExtends : InicializacaoBanco
    {
        private readonly SQLiteAsyncConnection _connection;
        public InicializaBancoExtends(ISqliteConnection context) : base(context)
        {
            _connection = context.DbConnectionAsync();
        }

        public override async Task Init()
        {
            await base.Init();
            await DataInit();
        }

        public override async Task Atualizacoes()
        {
            try
            {
                await base.AtualizacoesCore();

                // Tabelas/colunas/índices do Pegada que eram criados aqui (TBT_PESSOA_DEPOSITO_FATURAMENTO,
                // TBT_HISTORICO_CARRINHO, campos de TBT_MUNICIPIO/TBT_CARRINHO/TBT_CLIENTE e CREATE_INDEXES_COPIA_PEDIDO)
                // foram incorporados aos scripts de instalação (DDL/INIT e DDL/INDEX do MobiliVendas.Core.DataBase).
                // Alterações novas seguem o exemplo comentado em InicializacaoBanco.AtualizacoesCore.

                await ExecutarAlteracoes("ATUALIZA_VERSAO");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro no script de atualizações do banco.\n\nMessage: {ex.Message}");
            }
        }

        public override async Task DataInit()
        {
            int rowError = 0;
            string sqlError = string.Empty;
            try
            {
                var script = MakeSql("DATA_INIT", "DDL", null);
                var statements = script.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var statement in statements)
                {
                    sqlError = statement;
                    await _connection.ExecuteAsync(statement);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro no script de inicialização do banco.\nLinha: {rowError} \nScript: {sqlError} \nMessage: {ex.Message}");
            }
        }

        public override async Task ExecutarAlteracoes(string scriptName)
        {
            int rowError = 0;
            string sqlError = string.Empty;
            string script = string.Empty;
            string[] statements;

            try
            {

                script = MakeSql(scriptName, "ALTERACOES", null);
                statements = script.Trim().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var statement in statements)
                {
                    try
                    {
                        rowError++;
                        sqlError = statement;
                        await _connection.ExecuteAsync(statement);
                    }
                    catch (SQLiteException sqliteException)
                    {
                        if (sqliteException.Result == SQLite3.Result.Constraint)
                        {
                            Debug.WriteLine("SQLITE EXCEPTION EX: " + sqliteException.Message + " QUERY: " + sqlError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro no script de inicialização do banco");
                Debug.WriteLine($"{rowError}");
                Debug.WriteLine($"{sqlError}");
                Debug.WriteLine($"{ex.Message}");
                throw ex;
            }

        }

        public static string MakeSql(string fileName, string folderName, object parameters)
        {
            string fileFullName = $"Pegada.Core.DataBase.{folderName}.{fileName}";
            var assembly = typeof(InicializaBancoExtends).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(fileFullName);
            return MobiliVendas.Core.DataBase.ManagerQuery.FormatScriptFromStream(fileName, folderName, stream, parameters);
        }
    }
}
