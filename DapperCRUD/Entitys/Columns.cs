namespace DapperCRUD.Entitys
{
    public class Columns
    {
        public int ColumnId { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        /// <summary>
        /// 是否自增长标识
        /// </summary>
        public bool IsIdentity { get; set; }
        public bool IsNullable { get; set; }
        public int ByteLength { get; set; }
        public int CharLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public string Remark { get; set; }

        public string DefaultValue { get; set; }
    }
}
