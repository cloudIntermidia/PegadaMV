using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pegada.Core
{
    public static class ManagerQuery
    {
        public static string MakeUpdate(List<string> campos, string tabela, List<string> camposWhere, object parameters)
        {
            var properties = parameters.GetType().GetRuntimeProperties().Select(x => x.Name);
            campos = campos.Where(x => properties.Contains(x)).ToList();
            string columns = string.Empty;
            string where = string.Empty;
            foreach (var campo in campos)
                columns += $"{campo} = '@{campo}',";

            foreach (var campo in camposWhere)
                where += $"{campo} = '@{campo}' AND ";

            columns = columns.Substring(0, columns.Length - 1);
            where = where.Substring(0, where.Length - 4);
            string sql = $"UPDATE {tabela} SET {columns}  WHERE {where}";
            return  MobiliVendas.Core.DataBase.ManagerQuery.FormatScriptFromString(sql, "CRUD", parameters, tabela);
        }

        public static string MakeInsertOrReplace(List<string> campos, string tabela, object parameters)
        {
            string columns = string.Empty;
            string values = string.Empty;
            var properties = parameters.GetType().GetRuntimeProperties().Select(x => x.Name);
            campos = campos.Where(x => properties.Contains(x)).ToList();
            foreach (var campo in campos)
            {
                columns += $"{campo},";
                values += $"'@{campo}',";
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            string sql = $"INSERT OR REPLACE INTO {tabela} ({columns}) VALUES ({values})";
            return MobiliVendas.Core.DataBase.ManagerQuery.FormatScriptFromString(sql, "CRUD", parameters, tabela);
        }

        public static string MakeSql(string fileName, string folderName, object parameters)
        {
            string fileFullName = $"Pegada.Core.DataBase.{folderName}.{fileName}";
            var assembly = typeof(Pegada.Core.ManagerQuery).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(fileFullName);
            return MobiliVendas.Core.DataBase.ManagerQuery.FormatScriptFromStream(fileName, folderName, stream, parameters);
        }
    }
}
