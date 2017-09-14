using System;
using System.IO;
using System.Text;

namespace DapperCRUD.Common
{
    public class FileUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespaces"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void WriteFile(string namespaces, string fileName, string content)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + $"/{namespaces}/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fs = new FileStream(path + fileName, FileMode.Create);
            var sw = new StreamWriter(fs, Encoding.UTF8);
            //开始写入
            sw.Write(content);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
    }
}
