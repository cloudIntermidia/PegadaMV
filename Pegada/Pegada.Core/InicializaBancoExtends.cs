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
                bool tabelaExiste, campoExiste, parametroExiste;

                await base.AtualizacoesCore();

                tabelaExiste = await TabelaExiste("TBT_PESSOA_DEPOSITO_FATURAMENTO");
                if (!tabelaExiste)
                    await ExecutarAlteracoes("CREATE_TBT_PESSOA_DEPOSITO_FATURAMENTO");

                campoExiste = await CampoExiste("MinimoFobClienteNovo", "TBT_MUNICIPIO");
                if (!campoExiste)
                    await ExecutarAlteracoes("ALTER_TBT_MUNICIPIO_CAMPOS_MINIMO");

                campoExiste = await CampoExiste("DataLimite", "TBT_CARRINHO");
                if (!campoExiste)
                    await ExecutarAlteracoes("ALTER_TBT_CARRINHO_CAMPOS_INTEGRACAO");

                campoExiste = await CampoExiste("IndBloqueioPedido", "TBT_CLIENTE");
                if (!campoExiste)
                    await ExecutarAlteracoes("ALTER_TBT_CLIENTE_CAMPOS_BLOQUEIO");

                tabelaExiste = await TabelaExiste("TBT_HISTORICO_CARRINHO");
                if (!tabelaExiste)
                    await ExecutarAlteracoes("CREATE_TBT_HISTORICO_CARRINHO");

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
