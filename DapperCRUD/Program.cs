using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DapperCRUD.Common;
using DapperCRUD.Data;

namespace DapperCRUD
{
    class Program
    {
        private static StringBuilder NewStringBuilder(string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {Config.DataNamespace}.Sql");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            return sb;
        }

        private static StringBuilder NewStringBuilderDa()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Dapper;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine($"using {Config.EntityNamespace};");
            sb.AppendLine($"using SqlDelete = {Config.DataNamespace}.Sql.SqlDelete;");
            sb.AppendLine($"using SqlInsert = {Config.DataNamespace}.Sql.SqlInsert;");
            sb.AppendLine($"using SqlSelect = {Config.DataNamespace}.Sql.SqlSelect;");
            sb.AppendLine($"using SqlUpdate = {Config.DataNamespace}.Sql.SqlUpdate;");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"namespace {Config.DataNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {Config.DataClassName} : DbBase");
            sb.AppendLine("    {");
            //
            sb.AppendLine("        private static readonly Hashtable Cache = new Hashtable();");
            sb.AppendLine("");
            sb.AppendLine("        private static string GetWhereString(object param)");
            sb.AppendLine("        {");
            sb.AppendLine("            var type = param.GetType();");
            sb.AppendLine("            var hashCode = type.GetHashCode();");
            sb.AppendLine("            var v = Cache[hashCode];");
            sb.AppendLine("            if (v != null)");
            sb.AppendLine("                return v.ToString();");
            sb.AppendLine("");
            sb.AppendLine("            var sb = new StringBuilder(\" WHERE \");");
            sb.AppendLine("            var properties = type.GetProperties();");
            sb.AppendLine("            var hasAnd = false;");
            sb.AppendLine("            foreach (var propertie in properties)");
            sb.AppendLine("            {");
            sb.AppendLine("                if (hasAnd)");
            sb.AppendLine("                {");
            sb.AppendLine("                    sb.Append(\" AND \");");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                {");
            sb.AppendLine("                    hasAnd = true;");
            sb.AppendLine("                }");
            sb.AppendLine("                var propertieName = propertie.Name;");
            sb.AppendLine("                var value = propertie.GetValue(param, null);");
            sb.AppendLine("                var isIlist = value as IList;");
            sb.AppendLine("                if (isIlist != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    sb.Append($\"{ propertieName} IN @{ propertieName}\");");
            sb.AppendLine("                    continue;");
            sb.AppendLine("                }");
            sb.AppendLine("               sb.Append($\"{ propertieName}=@{ propertieName}\");");
            sb.AppendLine("            }");
            sb.AppendLine("            sb.Append(\";\");");
            sb.AppendLine("            var whereStr = sb.Length > 1 ? sb.ToString() : \"\";");
            sb.AppendLine("            Cache.Add(hashCode, whereStr);");
            sb.AppendLine("            return whereStr;");
            sb.AppendLine("        }");

            return sb;
        }

        private static StringBuilder NewStringBuilderBiz()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"using {Config.DataNamespace};");
            sb.AppendLine($"using {Config.EntityNamespace};");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"namespace {Config.BizNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {Config.BizClassName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private static readonly {Config.DataClassName} Da = new {Config.DataClassName}();");
            sb.AppendLine();
            return sb;
        }

        private static void EndStringBuilder(StringBuilder sb, string fileName)
        {
            sb.AppendLine("    }");
            sb.AppendLine("}");
            FileUtility.WriteFile($"{Config.DataNamespace}/Sql", $"{fileName}.cs", sb.ToString());
            Console.WriteLine($"生成:{fileName}");
        }

        private static void EndStringBuilderDa(StringBuilder sb)
        {
            sb.AppendLine("    }");
            sb.AppendLine("}");
            FileUtility.WriteFile(Config.DataNamespace, $"{Config.DataClassName}.cs", sb.ToString());
            Console.WriteLine($"生成:{Config.DataClassName}");
        }

        private static void EndStringBuilderBiz(StringBuilder sb)
        {
            sb.AppendLine("    }");
            sb.AppendLine("}");
            FileUtility.WriteFile(Config.BizNamespace, $"{Config.BizClassName}.cs", sb.ToString());
            Console.WriteLine($"生成:{Config.BizClassName}");
        }

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("开始执行");
            var tableLists = Db.GetTable();
            var i = 0;

            //sql
            var sbSelect = NewStringBuilder("SqlSelect");
            var sbUpdate = NewStringBuilder("SqlUpdate");
            var sbInsert = NewStringBuilder("SqlInsert");
            var sbDelete = NewStringBuilder("SqlDelete");
            //db
            var sbDa = NewStringBuilderDa();
            //biz
            var sbBiz = NewStringBuilderBiz();

            foreach (var table in tableLists)
            {
                i++;

                var schemname = tableLists.Count(p => string.Equals(p.Tablename, table.Tablename, StringComparison.OrdinalIgnoreCase)) > 1 ? table.Schemname : "";
                var tableSchement = $"{table.Tablename}{schemname}";
                var schementTabel = $"{table.Schemname}.{table.Tablename}";
                var columns = Db.GetColumns($"{schementTabel}");
                var primaryKeys = columns.Where(p => p.IsPrimaryKey).ToList();
                var noPrimaryKeys = columns.Where(p => !p.IsPrimaryKey).ToList();
                var noIdentityKeys = columns.Where(p => !p.IsIdentity).ToList();
                var className = $"{tableSchement}{Config.EntityClassExt}";


                #region 表
                Console.WriteLine($"{i}生成表:{schementTabel}");
                var sb = new StringBuilder("using System;");

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"namespace {Config.EntityNamespace}");
                sb.AppendLine("{");

                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// {table.Descriptions}");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public class {className}");
                sb.AppendLine("    {");


                foreach (var column in columns)
                {
                    var defaultVal = DbTypeMap.MapCsharpDefaultValue(column);

                    if (!string.IsNullOrWhiteSpace(column.Remark))
                    {
                        sb.AppendLine("        /// <summary>");
                        var array = column.Remark.Split('\n', '\r').ToList();
                        foreach (var remark in array)
                        {
                            sb.AppendLine($"        /// {remark}");
                        }
                        sb.AppendLine("        /// </summary>");
                    }

                    if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                    {
                        sb.AppendLine($"        /// 默认值:{column.DefaultValue}");
                    }

                    sb.AppendLine("        public " + DbTypeMap.MapCsharpType(column.ColumnType, column.IsNullable) + " " + column.ColumnName + " { get; set; }" + defaultVal);
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                FileUtility.WriteFile(Config.EntityNamespace, $"{className}.cs", sb.ToString());
                #endregion

                #region SQL
                if (primaryKeys.Count > 0)
                {
                    var paras = $"        //object paras = new {{ {string.Join(", ", primaryKeys.Select(p => $"{p.ColumnName}="))} }};";
                    var where = string.Join(" AND ", primaryKeys.Select(p => $"{p.ColumnName}=@{p.ColumnName}").ToList());
                    var whereList = "";
                    //var whereListUpdate = "";
                    if (primaryKeys.Count == 1)
                    {
                        var p = primaryKeys[0];
                        whereList = $"{p.ColumnName} IN @{p.ColumnName}s";
                        //whereListUpdate = $"x.{p.ColumnName}=y.{p.ColumnName}";
                    }
                    else
                    {
                        //TODO whereList
                        //whereList =
                        //whereListUpdate = string.Join(" AND ", primaryKeys.Select(p => $"x.{p.ColumnName}=y.{p.ColumnName}").ToList());
                    }
                    //select
                    sbSelect.AppendLine(paras);
                    sbSelect.AppendLine($"        public const string {tableSchement}ById = @\"SELECT * FROM {schementTabel} t WHERE {where};\";");
                    //selectList
                    sbSelect.AppendLine($"        public const string {tableSchement}ByIds = @\"SELECT * FROM {schementTabel} t WHERE {whereList};\";");


                    //delete
                    sbDelete.AppendLine(paras);
                    sbDelete.AppendLine($"        public const string {tableSchement}ById = @\"DELETE FROM {schementTabel} WHERE {where};\";");
                    //deleteList
                    sbDelete.AppendLine($"        public const string {tableSchement}ByIds = @\"DELETE FROM {schementTabel} WHERE {whereList};\";");


                    //update
                    sbUpdate.AppendLine($"        //object paras = new {{ {string.Join(", ", columns.Select(p => $"{p.ColumnName}="))} }};");
                    sbUpdate.AppendLine($"        public const string {tableSchement}ById = @\"UPDATE {schementTabel} SET {string.Join(",", noPrimaryKeys.Select(p => $"{p.ColumnName}=@{p.ColumnName}"))} WHERE {where};\";");
                    //updateList
                    //sbUpdate.AppendLine($"        public static string {tableSchement}ByIds(List<{tableSchement}Model> lists)");
                    //sbUpdate.AppendLine($"        {{");
                    //sbUpdate.AppendLine($"            var sb = new StringBuilder(\"UPDATE {schementTabel} SET {string.Join(",", noPrimaryKeys.Select(p => $"{p.ColumnName}=y.{p.ColumnName}"))} FROM {schementTabel} x, (\");");
                    //sbUpdate.AppendLine($"            for (var i = 0; i < lists.Count; i++)");
                    //sbUpdate.AppendLine($"            {{");
                    //sbUpdate.AppendLine($"                sb.AppendLine(i == 0");
                    //sbUpdate.AppendLine($"                    ? $\"SELECT {string.Join(",", columns.Select(p=>$"'{{lists[i].{p.ColumnName}}}' {p.ColumnName}"))}\"");
                    //sbUpdate.AppendLine($"                    : $\" UNION SELECT {string.Join(",", columns.Select(p => $"'{{lists[i].{p.ColumnName}}}'"))}\");");
                    //sbUpdate.AppendLine($"            }}");
                    //sbUpdate.AppendLine($"            sb.AppendLine(\") y WHERE {whereListUpdate}; \");");
                    //sbUpdate.AppendLine($"            return sb.ToString();");
                    //sbUpdate.AppendLine($"        }}");
                }
                //insert
                sbInsert.AppendLine($"        //object paras = new {{ {string.Join(", ", noIdentityKeys.Select(p => $"{p.ColumnName}="))} }};");
                sbInsert.AppendLine($"        public const string {tableSchement} = @\"INSERT INTO {schementTabel}({string.Join(",", noIdentityKeys.Select(p => p.ColumnName))}) VALUES({string.Join(",", noIdentityKeys.Select(p => $"@{p.ColumnName}"))});\";");
                sbInsert.AppendLine();


                //select
                sbSelect.AppendLine($"        public const string {tableSchement}All = @\"SELECT * FROM {schementTabel}\";");
                sbSelect.AppendLine();


                //delete
                sbDelete.AppendLine($"        public const string {tableSchement}All = @\"TRUNCATE TABLE {schementTabel};\";");
                sbDelete.AppendLine();
                #endregion

                #region da
                sbDa.AppendLine("        /// <summary>");
                sbDa.AppendLine($"        /// {table.Descriptions}");
                sbDa.AppendLine("        /// </summary>");
                if (primaryKeys.Count > 0)
                {
                    var parms = string.Join(", ", primaryKeys.Select(p => $"{DbTypeMap.MapCsharpType(p.ColumnType, p.IsNullable)} {Method.GetCamelCase(p.ColumnName)}"));
                    var parmsTran = string.Join(", ", primaryKeys.Select(p => $"{DbTypeMap.MapCsharpType(p.ColumnType, p.IsNullable)} {Method.GetCamelCase(p.ColumnName)}")) + ", IDbTransaction tran = null";
                    var obj = $"new {className}(){{ {string.Join(", ", primaryKeys.Select(p => $"{p.ColumnName}={Method.GetCamelCase(p.ColumnName)}"))} }}";
                    var parmsList = "";
                    var parmsListTran = "";
                    var parmsListType = "";
                    var objListParams = "";
                    var objIds = "";
                    if (primaryKeys.Count == 1)
                    {
                        var p = primaryKeys[0];
                        objIds = $"{p.ColumnName}s";
                        parmsListType = $"List<{DbTypeMap.MapCsharpType(p.ColumnType, p.IsNullable)}>";
                        objListParams = $"{Method.GetCamelCase(p.ColumnName)}s";
                        parmsList = $"{parmsListType} {objListParams}";
                        parmsListTran = $"{parmsListType} {objListParams}, IDbTransaction tran = null";
                    }
                    else
                    {
                        //TODO parmsList
                    }
                    //select
                    sbDa.AppendLine($"        public {className} Get{table.Tablename}({parmsTran}) {{ var model = {obj}; return tran?.Connection.QueryFirstOrDefault<{className}>(SqlSelect.{table.Tablename}ById, model, tran)?? Db(p => p.QueryFirstOrDefault<{className}>(SqlSelect.{table.Tablename}ById, model)); }}");
                    //selectList
                    sbDa.AppendLine($"        public List<{className}> Get{table.Tablename}({parmsListTran}) {{ var model = new {{ {objIds} = {objListParams} }}; return tran?.Connection.Query<{className}>(SqlSelect.{table.Tablename}ByIds, model, tran).ToList() ?? Db(p => p.Query<{className}>(SqlSelect.{table.Tablename}ByIds, model).ToList()); }}");


                    //delete
                    sbDa.AppendLine($"        public void Delete{table.Tablename}({parmsTran}) {{ var model = {obj}; if (tran == null) {{ Db(p => {{ p.Execute(SqlDelete.{table.Tablename}ById, model); }}); }} else {{ tran.Connection.Execute(SqlDelete.{table.Tablename}ById, model, tran); }} }}");
                    sbDa.AppendLine($"        public void Delete{table.Tablename}Async({parms}) {{ var model = {obj}; ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ try {{ p.Execute(SqlDelete.{table.Tablename}ById, ({className})m); }} catch (Exception ex) {{ AddLog(ex.Message, ex); }} }}); }}, model); }}");
                    
                    //deleteList
                    sbDa.AppendLine($"        public void Delete{table.Tablename}({parmsListTran}) {{ var model = new {{ {objIds} = {objListParams} }}; if (tran == null) {{ Db(p => {{ tran = p.BeginTransaction(); try {{ p.Execute(SqlDelete.{table.Tablename}ByIds, model, tran); tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }} else {{ tran.Connection.Execute(SqlDelete.{table.Tablename}ByIds, model, tran); }} }}");
                    sbDa.AppendLine($"        public void Delete{table.Tablename}Async({parmsList}) {{ var model = new {{ {objIds} = {objListParams} }}; ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ var tran = p.BeginTransaction(); try {{ p.Execute(SqlDelete.{table.Tablename}ByIds, m, tran); tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }}, model); }}");


                    //update
                    sbDa.AppendLine($"        public void Update({className} model, IDbTransaction tran = null) {{ if (tran == null) {{ Db(p => p.Execute(SqlUpdate.{table.Tablename}ById, model)); }} else {{ tran.Connection.Execute(SqlUpdate.{table.Tablename}ById, model, tran); }} }}");
                    sbDa.AppendLine($"        public void UpdateAsync({className} model) {{ ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ var tran = p.BeginTransaction(); try {{ p.Execute(SqlUpdate.{table.Tablename}ById, ({className})m, tran); tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }}, model); }}");
                    
                    //updateList
                    sbDa.AppendLine($"        public void Update(List<{className}> models, IDbTransaction tran = null) {{ if (tran == null) {{ Db(p => {{ tran = p.BeginTransaction(); try {{ foreach (var item in models) {{ p.Execute(SqlUpdate.{table.Tablename}ById, item, tran); }} tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }} else {{ foreach (var item in models) {{ tran.Connection.Execute(SqlUpdate.{table.Tablename}ById, item, tran); }} }} }}");
                    sbDa.AppendLine($"        public void UpdateAsync(List<{className}> models) {{ ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ var tran = p.BeginTransaction(); try {{ var items = (List<{className}>)m; foreach (var item in items) {{ p.Execute(SqlUpdate.{table.Tablename}ById, item, tran); }} tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }}, models); }}");
                }
                //insert
                sbDa.AppendLine($"        public void Insert({className} model, IDbTransaction tran = null) {{ if (tran == null) {{ Db(p => {{ try {{ p.Execute(SqlInsert.{table.Tablename}, model); }} catch (Exception ex) {{ AddLog(ex.Message, ex); }} }}); }} else {{ tran.Connection.Execute(SqlInsert.{table.Tablename}, model, tran); }} }}");
                sbDa.AppendLine($"        public void InsertAsync({className} model) {{ ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ try {{ p.Execute(SqlInsert.{table.Tablename}, ({className})m); }} catch (Exception ex) {{ AddLog(ex.Message, ex); }} }}); }}, model); }}");
                
                //insertList
                sbDa.AppendLine($"        public void Insert(List<{className}> models, IDbTransaction tran = null) {{ if (tran == null) {{ Db(p => {{ tran = p.BeginTransaction(); try {{ p.Execute(SqlInsert.{table.Tablename}, models.ToArray(), tran); tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }} else {{ tran.Connection.Execute(SqlInsert.{table.Tablename}, models.ToArray(), tran); }} }}");
                sbDa.AppendLine($"        public void InsertAsync(List<{className}> models) {{ ThreadPool.QueueUserWorkItem(m => {{ Db(p => {{ var tran = p.BeginTransaction(); try {{ p.Execute(SqlInsert.{table.Tablename}, ((List<{className}>)m).ToArray(), tran); tran.Commit(); }} catch (Exception ex) {{ tran.Rollback(); AddLog(ex.Message, ex); }} }}); }}, models); }}");
                
                //select
                sbDa.AppendLine($"        public List<{className}> Get{table.Tablename}(object param = null, IDbTransaction tran = null) {{ if (param == null) {{ return tran?.Connection.Query<{className}>(SqlSelect.{table.Tablename}All, transaction: tran).ToList() ?? Db(p => p.Query<{className}>(SqlSelect.{table.Tablename}All).ToList()); }} var sqlStr = $@\"{{SqlSelect.{table.Tablename}All}} {{GetWhereString(param)}}\"; return tran?.Connection.Query<{className}>(sqlStr, param, tran).ToList() ?? Db(p => p.Query<{className}>(sqlStr, param).ToList()); }}");
                
                //delete
                sbDa.AppendLine($"        public void Truncate{table.Tablename}Async(IDbTransaction tran = null) {{ if (tran == null) {{ ThreadPool.QueueUserWorkItem(m => {{ Db(p => p.Execute(SqlDelete.{table.Tablename}All)); }}); }} else {{ tran.Connection.Execute(SqlDelete.{table.Tablename}All, transaction: tran); }} }}");
                sbDa.AppendLine();

                #endregion

                #region biz

                sbBiz.AppendLine("        /// <summary>");
                sbBiz.AppendLine($"        /// {table.Descriptions}");
                sbBiz.AppendLine("        /// </summary>");
                if (primaryKeys.Count > 0)
                {
                    var parms = string.Join(", ", primaryKeys.Select(p => $"{DbTypeMap.MapCsharpType(p.ColumnType, p.IsNullable)} {Method.GetCamelCase(p.ColumnName)}"));
                    var obj = string.Join(", ", primaryKeys.Select(p => Method.GetCamelCase(p.ColumnName)));
                    var parmsList = "";
                    var objListParams = "";
                    if (primaryKeys.Count == 1)
                    {
                        var p = primaryKeys[0];
                        var parmsListType = $"List<{DbTypeMap.MapCsharpType(p.ColumnType, p.IsNullable)}>";
                        objListParams = $"{Method.GetCamelCase(p.ColumnName)}s";
                        parmsList = $"{parmsListType} {objListParams}";

                    }
                    else
                    {
                        //TODO parmsList
                    }
                    //select
                    sbBiz.AppendLine($"        public {className} Get{table.Tablename}({parms}) {{ return Da.Get{table.Tablename}({obj}); }}");
                    //selectList
                    sbBiz.AppendLine($"        public List<{className}> Get{table.Tablename}({parmsList}) {{ return Da.Get{table.Tablename}({objListParams}); }}");


                    //update
                    sbBiz.AppendLine($"        public void Update({className} model) {{ Da.Update(model); }}");
                    sbBiz.AppendLine($"        public void UpdateAsync({className} model) {{ Da.UpdateAsync(model); }}");
                    //updateList
                    sbBiz.AppendLine($"        public void Update(List<{className}> models) {{ Da.Update(models); }}");
                    sbBiz.AppendLine($"        public void UpdateAsync(List<{className}> models) {{ Da.UpdateAsync(models); }}");


                    //delete
                    sbBiz.AppendLine($"        public void Delete{table.Tablename}({parms}) {{ Da.Delete{table.Tablename}({obj}); }}");
                    sbBiz.AppendLine($"        public void Delete{table.Tablename}Async({parms}) {{ Da.Delete{table.Tablename}Async({obj}); }}");
                    //deleteList
                    sbBiz.AppendLine($"        public void Delete{table.Tablename}({parmsList}) {{ Da.Delete{table.Tablename}({objListParams}); }}");
                    sbBiz.AppendLine($"        public void Delete{table.Tablename}Async({parmsList}) {{ Da.Delete{table.Tablename}Async({objListParams}); }}");
                }
                //insert
                sbBiz.AppendLine($"        public void Insert({className} model) {{ Da.Insert(model); }}");
                sbBiz.AppendLine($"        public void InsertAsync({className} model) {{ Da.InsertAsync(model); }}");
                //insertList
                sbBiz.AppendLine($"        public void Insert(List<{className}> models) {{ Da.Insert(models); }}");
                sbBiz.AppendLine($"        public void InsertAsync(List<{className}> models) {{ Da.InsertAsync(models); }}");
                //select
                sbBiz.AppendLine($"        public List<{className}> Get{table.Tablename}(object param = null) {{ return Da.Get{table.Tablename}(param); }}");
                //delete
                sbBiz.AppendLine($"        public void TruncateAsync{table.Tablename}() {{ Da.Truncate{table.Tablename}Async(); }}");
                sbBiz.AppendLine();

                #endregion
            }
            EndStringBuilder(sbSelect, "SqlSelect");
            EndStringBuilder(sbUpdate, "SqlUpdate");
            EndStringBuilder(sbInsert, "SqlInsert");
            EndStringBuilder(sbDelete, "SqlDelete");
            EndStringBuilderDa(sbDa);
            EndStringBuilderBiz(sbBiz);

            CreateDbBase();

            sw.Stop();
            Console.WriteLine($"执行完成,耗时: {sw.ElapsedMilliseconds} ms");

            Console.ReadKey();
        }

        private static void CreateDbBase()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine();
            sb.AppendLine($"namespace {Config.DataNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public abstract class DbBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        private static readonly string ConnectionString = ConfigurationManager.AppSettings[\"{Config.ConnectStringAppSettingKey}\"];");
            sb.AppendLine();

            sb.AppendLine("        public static Action<object, Exception> AddLog { get; set; }");

            sb.AppendLine();
            sb.AppendLine("        protected T Db<T>(Func<IDbConnection, T> func)");
            sb.AppendLine("        {");
            sb.AppendLine("            var db = new SqlConnection(ConnectionString);");
            sb.AppendLine("            db.Open();");
            sb.AppendLine("            var result = func(db);");
            sb.AppendLine("            db.Close();");
            sb.AppendLine("            db.Dispose();");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected void Db(Action<IDbConnection> action)");
            sb.AppendLine("        {");
            sb.AppendLine("            var db = new SqlConnection(ConnectionString);");
            sb.AppendLine("            db.Open();");
            sb.AppendLine("            action(db);");
            sb.AppendLine("            db.Close();");
            sb.AppendLine("            db.Dispose();");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            FileUtility.WriteFile(Config.DataNamespace, "DbBase.cs", sb.ToString());
            Console.WriteLine($"生成:DbBase");
        }
    }
}
