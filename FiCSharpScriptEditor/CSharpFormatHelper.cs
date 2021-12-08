using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiCSharpScriptEditor
{
    /// <summary>
    /// 格式化C#代码助手
    /// </summary>
    public class CSharpFormatHelper 
    {
        //remove empty lines from string
        public static string RemoveEmptyLines(string lines)
        {
            return Regex.Replace(lines, @"^\s*$\n|\r", "", RegexOptions.Multiline).TrimEnd();
        }

        //Indent String with Spaces
        public static string Indent(int count)
        {
            if (count <= 0)
            {
                return "";
            }
            else
            {
                return "    ".PadLeft(count * 2);
            }
        }

        /// <summary>
        /// 格式化C#代码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string FormatCSharpCode(string code)
        {
            //去除空白行
            //code = RemoveEmptyLines(code);
            StringBuilder sb = new StringBuilder();
            int count = 2;
            int times = 0;
            string[] lines = code.Split('\n');
            foreach (var line in lines)
            {
                bool condition1 = line.TrimEnd().EndsWith("{"); //必须以{大括号结尾
                bool condition2 = !line.Contains("//"); //不包含注释符

                if (line.TrimStart().StartsWith("{") || (condition1 && condition2))
                {
                    sb.Append(Indent(count * times) + line.TrimStart() + "\n");
                    times++;
                }
                else if (line.TrimStart().StartsWith("}"))
                {
                    times--;
                    if (times <= 0)
                    {
                        times = 0;
                    }
                    sb.Append(Indent(count * times) + line.TrimStart() + "\n");
                }
                else
                {
                    sb.Append(Indent(count * times) + line.TrimStart() + "\n");
                }
            }
            return sb.ToString();
        }
    }
}
