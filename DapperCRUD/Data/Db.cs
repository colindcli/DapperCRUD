using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DapperCRUD.Common;
using DapperCRUD.Entitys;

namespace DapperCRUD.Data
{
    public class Db
    {
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["ConnectString"];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddRange(parms);
                var adapter = new SqlDataAdapter(command);

                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public static List<Table> GetTable()
        {
            const string sqlStr = @"SELECT schem.name schemname,
       obj.name tablename,
       idx.rows,
       CAST
       (
       CASE WHEN
       (
           SELECT COUNT(1)
           FROM sys.indexes
           WHERE object_id=obj.OBJECT_ID
               AND is_primary_key=1
       )>=1 THEN 1 ELSE 0 END
       AS Bit) HasPrimaryKey,
	   ep.value Descriptions
FROM sys.objects obj
INNER JOIN dbo.sysindexes idx
    ON obj.object_id=idx.id
    AND idx.indid<=1
INNER JOIN sys.schemas schem
    ON obj.schema_id=schem.schema_id
LEFT JOIN sys.extended_properties ep
	ON ep.major_id=obj.object_id AND ep.minor_id=0
WHERE type='U'
ORDER BY schem.name ASC,obj.name ASC;";

            var dt = GetDataTable(sqlStr);
            var tableLists = new List<Table>();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                tableLists.Add(new Table()
                {
                    HasPrimaryKey = Convert.ToBoolean(dt.Rows[i]["HasPrimaryKey"].ToString()),
                    Rows = int.Parse(dt.Rows[i]["rows"].ToString()),
                    Tablename = dt.Rows[i]["tablename"].ToString(),
                    Schemname = dt.Rows[i]["schemname"].ToString(),
                    Descriptions = dt.Rows[i]["Descriptions"].ToString()
                });
            }
            tableLists.RemoveAll(p => string.Equals(p.Schemname, "dbo", StringComparison.OrdinalIgnoreCase) &&
                                      string.Equals(p.Tablename, "sysdiagrams", StringComparison.OrdinalIgnoreCase));

            foreach (var item in tableLists)
            {
                item.Tablename = Method.GetPascal(item.Tablename);
            }
            return tableLists;
        }

        public static List<Columns> GetColumns(string tableName)
        {
            const string sqlStr = @"WITH indexCTE
AS
(
    SELECT ic.column_id,
           ic.index_column_id,
           ic.object_id
    FROM sys.indexes idx
    INNER JOIN sys.index_columns ic
        ON idx.index_id=ic.index_id
        AND idx.object_id=ic.object_id
    WHERE idx.object_id=OBJECT_ID(@Tablename)
        AND idx.is_primary_key=1
)
SELECT colm.column_id ColumnID,
       CAST(CASE WHEN indexCTE.column_id IS NULL THEN 0 ELSE 1 END AS Bit) IsPrimaryKey,
       colm.name ColumnName,
       systype.name ColumnType,
       colm.is_identity IsIdentity,
       colm.is_nullable IsNullable,
       CAST(colm.max_length AS Int) ByteLength,
       (
       CASE WHEN systype.name='nvarchar' AND colm.max_length>0 THEN colm.max_length/2 WHEN systype.name='nchar' AND colm.max_length>0 THEN colm.max_length/2 WHEN systype.name='ntext' AND colm.max_length>0 THEN colm.max_length/2 ELSE colm.max_length END
       ) CharLength,
       CAST(colm.precision AS Int) Precision,
       CAST(colm.scale AS Int) Scale,
       prop.value Remark,
	   sc.text DefaultValue
FROM sys.columns colm
INNER JOIN sys.types systype
    ON colm.system_type_id=systype.system_type_id
    AND colm.user_type_id=systype.user_type_id
LEFT JOIN syscomments sc ON colm.default_object_id=sc.id
LEFT JOIN sys.extended_properties prop
    ON colm.object_id=prop.major_id
    AND colm.column_id=prop.minor_id
LEFT JOIN indexCTE
    ON colm.column_id=indexCTE.column_id
    AND colm.object_id=indexCTE.object_id
WHERE colm.object_id=OBJECT_ID(@Tablename)
ORDER BY colm.column_id;";

            var dt = GetDataTable(sqlStr, new SqlParameter("tableName", SqlDbType.VarChar) { Value = tableName });
            var columnLists = new List<Columns>();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                columnLists.Add(new Columns()
                {
                    ByteLength = int.Parse(dt.Rows[i]["ByteLength"].ToString()),
                    CharLength = int.Parse(dt.Rows[i]["CharLength"].ToString()),
                    ColumnId = int.Parse(dt.Rows[i]["ColumnId"].ToString()),
                    ColumnName = dt.Rows[i]["ColumnName"].ToString(),
                    ColumnType = dt.Rows[i]["ColumnType"].ToString(),
                    IsIdentity = Convert.ToBoolean(dt.Rows[i]["IsIdentity"].ToString()),
                    IsNullable = Convert.ToBoolean(dt.Rows[i]["IsNullable"].ToString()),
                    IsPrimaryKey = Convert.ToBoolean(dt.Rows[i]["IsPrimaryKey"].ToString()),
                    Precision = int.Parse(dt.Rows[i]["Precision"].ToString()),
                    Remark = dt.Rows[i]["Remark"].ToString(),
                    Scale = int.Parse(dt.Rows[i]["Scale"].ToString()),
                    DefaultValue = dt.Rows[i]["DefaultValue"].ToString()
                });
            }

            foreach (var item in columnLists)
            {
                item.ColumnName = Method.GetPascal(item.ColumnName);
            }
            return columnLists;
        }
    }
}
