namespace DapperCRUD.Entitys
{
    public class Table
    {
        public string Schemname { get; set; }
        public string Tablename { get; set; }
        public int Rows { get; set; }
        public bool HasPrimaryKey { get; set; }
        public string Descriptions { get; set; }
    }
}
