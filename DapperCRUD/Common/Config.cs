using System.Configuration;

namespace DapperCRUD.Common
{
    public class Config
    {
        public static readonly string EntityNamespace = ConfigurationManager.AppSettings["EntityNamespace"];
        public static readonly string EntityClassExt = ConfigurationManager.AppSettings["EntityClassExt"];

        public static readonly string DataNamespace = ConfigurationManager.AppSettings["DataNamespace"];
        public static readonly string DataClassName = ConfigurationManager.AppSettings["DataClassName"];

        public static readonly string BizNamespace = ConfigurationManager.AppSettings["BizNamespace"];
        public static readonly string BizClassName = ConfigurationManager.AppSettings["BizClassName"];

        public static readonly string ConnectStringAppSettingKey = ConfigurationManager.AppSettings["ConnectStringAppSettingKey"];
        public static readonly bool IsSetDefaultValue = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["IsSetDefaultValue"]) && int.Parse(ConfigurationManager.AppSettings["IsSetDefaultValue"]) == 1;
    }
}
