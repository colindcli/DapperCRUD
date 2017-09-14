using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DapperCRUD.Common
{
    public class Method
    {
        /// <summary>
        /// 骆驼命名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetCamelCase(string s)
        {
            return GetCase(s, false);
        }

        /// <summary>
        /// 帕斯卡命名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetPascal(string s)
        {
            return GetCase(s, true);
        }

        private static string GetCase(string s, bool pascal)
        {
            var arrays = s.ToCharArray();
            var i = 0;
            var upper = false;
            var lists = new List<string>();
            var regex = new Regex("[A-Z]");
            foreach (var array in arrays)
            {
                if (i == 0)
                {
                    lists.Add(pascal ? array.ToString().ToUpper() : array.ToString().ToLower());
                }
                else
                {
                    if (upper)
                    {
                        lists.Add(array.ToString().ToLower());
                        upper = false;
                    }
                    else
                    {
                        lists.Add(array.ToString());
                        upper = regex.IsMatch(array.ToString());
                    }
                }
                i++;
            }
            return string.Join("", lists);
        }
    }
}
