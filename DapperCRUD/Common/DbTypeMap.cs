using System;
using DapperCRUD.Entitys;

namespace DapperCRUD.Common
{
    public class DbTypeMap
    {
        public static string MapCsharpDefaultValue(Columns column)
        {
            if (!Config.IsSetDefaultValue)
            {
                return "";
            }
            if (string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                return "";
            }
            var defaultValue = column.DefaultValue.ToLower();
            var dbtype = column.ColumnType.ToLower();
            var csharpDefaultVal = "";
            switch (defaultValue)
            {
                case "(getdate())": csharpDefaultVal = " = DateTime.Now;"; break;
                case "(newid())": csharpDefaultVal = " = Guid.NewGuid();"; break;
                default:
                    {
                        switch (dbtype)
                        {
                            case "bigint":
                            case "int":
                            case "smallint":
                            case "tinyint":
                                {
                                    csharpDefaultVal = $" = {defaultValue.Replace("((", "").Replace("))", "")};";
                                    break;
                                }
                            case "decimal":
                            case "numeric":
                            case "money":
                            case "smallmoney":
                                {
                                    var d = Convert.ToDecimal(defaultValue.Replace("((", "").Replace("))", ""));
                                    var s = d == 0 ? "0" : $"Convert.ToDecimal({d})";
                                    csharpDefaultVal = $" = {s};";
                                    break;
                                }
                            case "text":
                            case "varchar":
                            case "ntext":
                            case "nvarchar":
                                {
                                    csharpDefaultVal = $" = \"{defaultValue.Replace("('", "").Replace("')", "")}\";";
                                    break;
                                }
                            case "bit":
                                {
                                    csharpDefaultVal = " = " + (defaultValue == "((1))" ? "true" : "false") + ";";
                                    break;
                                }
                            case "date":
                            case "datetime":
                            case "datetime2":
                            case "smalldatetime":
                                {
                                    var d = defaultValue.Replace("('", "").Replace("')", "");
                                    csharpDefaultVal = DateTime.TryParse(d, out DateTime dt) ? $" = Convert.ToDateTime(\"{d}\");" : $" = \"{d}\";";
                                    break;
                                }
                        }
                        break;
                    }
            }

            if (!string.IsNullOrWhiteSpace(defaultValue) && string.IsNullOrWhiteSpace(csharpDefaultVal))
            {
                csharpDefaultVal = $" = {defaultValue};";
            }
            return csharpDefaultVal;
        }

        public static string MapCsharpType(string dbtype, bool isNull)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            var csharpType = "object";
            switch (dbtype)
            {
                case "bigint": csharpType = "long" + (isNull ? "?" : ""); break;
                case "binary": csharpType = "byte[]"; break;
                case "bit": csharpType = "bool" + (isNull ? "?" : ""); break;
                case "char": csharpType = "string"; break;
                case "date": csharpType = "DateTime" + (isNull ? "?" : ""); break;
                case "datetime": csharpType = "DateTime" + (isNull ? "?" : ""); break;
                case "datetime2": csharpType = "DateTime" + (isNull ? "?" : ""); break;
                case "datetimeoffset": csharpType = "DateTimeOffset" + (isNull ? "?" : ""); break;
                case "decimal": csharpType = "decimal" + (isNull ? "?" : ""); break;
                case "float": csharpType = "double" + (isNull ? "?" : ""); break;
                case "image": csharpType = "byte[]"; break;
                case "int": csharpType = "int" + (isNull ? "?" : ""); break;
                case "money": csharpType = "decimal" + (isNull ? "?" : ""); break;
                case "nchar": csharpType = "string"; break;
                case "ntext": csharpType = "string"; break;
                case "numeric": csharpType = "decimal" + (isNull ? "?" : ""); break;
                case "nvarchar": csharpType = "string"; break;
                case "real": csharpType = "Single" + (isNull ? "?" : ""); break;
                case "smalldatetime": csharpType = "DateTime" + (isNull ? "?" : ""); break;
                case "smallint": csharpType = "short" + (isNull ? "?" : ""); break;
                case "smallmoney": csharpType = "decimal" + (isNull ? "?" : ""); break;
                case "sql_variant": csharpType = "object"; break;
                case "sysname": csharpType = "object"; break;
                case "text": csharpType = "string"; break;
                case "time": csharpType = "TimeSpan" + (isNull ? "?" : ""); break;
                case "timestamp": csharpType = "byte[]"; break;
                case "tinyint": csharpType = "byte" + (isNull ? "?" : ""); break;
                case "uniqueidentifier": csharpType = "Guid" + (isNull ? "?" : ""); break;
                case "varbinary": csharpType = "byte[]"; break;
                case "varchar": csharpType = "string"; break;
                case "xml": csharpType = "string"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }

        public static Type MapCommonType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return Type.Missing.GetType();
            dbtype = dbtype.ToLower();
            Type commonType;
            switch (dbtype)
            {
                case "bigint": commonType = typeof(long); break;
                case "binary": commonType = typeof(byte[]); break;
                case "bit": commonType = typeof(bool); break;
                case "char": commonType = typeof(string); break;
                case "date": commonType = typeof(DateTime); break;
                case "datetime": commonType = typeof(DateTime); break;
                case "datetime2": commonType = typeof(DateTime); break;
                case "datetimeoffset": commonType = typeof(DateTimeOffset); break;
                case "decimal": commonType = typeof(decimal); break;
                case "float": commonType = typeof(double); break;
                case "image": commonType = typeof(byte[]); break;
                case "int": commonType = typeof(int); break;
                case "money": commonType = typeof(decimal); break;
                case "nchar": commonType = typeof(string); break;
                case "ntext": commonType = typeof(string); break;
                case "numeric": commonType = typeof(decimal); break;
                case "nvarchar": commonType = typeof(string); break;
                case "real": commonType = typeof(Single); break;
                case "smalldatetime": commonType = typeof(DateTime); break;
                case "smallint": commonType = typeof(short); break;
                case "smallmoney": commonType = typeof(decimal); break;
                case "sql_variant": commonType = typeof(object); break;
                case "sysname": commonType = typeof(object); break;
                case "text": commonType = typeof(string); break;
                case "time": commonType = typeof(TimeSpan); break;
                case "timestamp": commonType = typeof(byte[]); break;
                case "tinyint": commonType = typeof(byte); break;
                case "uniqueidentifier": commonType = typeof(Guid); break;
                case "varbinary": commonType = typeof(byte[]); break;
                case "varchar": commonType = typeof(string); break;
                case "xml": commonType = typeof(string); break;
                default: commonType = typeof(object); break;
            }
            return commonType;
        }
    }
}
