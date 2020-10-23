using System;
using System.Data;
using System.Configuration;
//using System.Web;
//using System.Web.Security;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 字符串操作辅助类
    /// </summary>
    internal static class StringHelper
    {
        /// <summary>为 Url 增加合并查询字符串（若存在则覆盖）</summary>
        /// <param name="queryString">要添加的查询字符串，如a=x&b=x</param>
        public static string AddQueryString(this string url, string queryString)
        {
            if (queryString.IsEmpty())
                return url;
            var u = new Url(url);
            var dict = queryString.ParseQueryDict();
            foreach (var key in dict.Keys)
                u[key] = dict[key];
            return u.ToString();
        }

        /// <summary>设置 url 参数（存在则更改，不存在则增加）</summary>
        public static string SetQueryString(this string url, string key, string value)
        {
            var u = new Url(url);
            u[key] = value;
            return u.ToString();
        }

        /// <summary>清理每行的前后空白字符</summary>
        public static string TrimLines(this string txt)
        {
            if (txt.IsEmpty()) return "";
            var lines = txt.Split(new string[] {"\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line.Trim());
            }
            return sb.ToString();
        }

        /// <summary>裁掉尾部的匹配字符串（及后面的字符串）。如"a.asp".TrimEndFrom(".") => "a"</summary>
        /// <param name="keepKey">是否保留键。如"/Pages/test.aspx".TiemEndFrom("/",true) => "/Pages/"</param>
        public static string TrimEnd(this string name, string key, bool keepKey=false)
        {
            if (name.IsEmpty())
                return "";
            var n = name.LastIndexOf(key);
            if (n != -1)
            {
                if (keepKey)
                    return name.Substring(0, n + key.Length);
                else
                    return name.Substring(0, n);
            }
            return name;
        }



        /// <summary>删除前面的匹配字符串字（及前面的字符串）。如"a.asp".TrimStartTo(".") => "asp"</summary>
        /// <param name="keepKey">是否保留键。如"/Pages/test.aspx".TiemStartTo("/",true) => ".asp"</param>
        public static string TrimStart(this string name, string key, bool keepKey = false)
        {
            if (name.IsEmpty())
                return "";
            var n = name.IndexOf(key);
            if (n != -1)
            {
                if (keepKey)
                    return name.SubText(n);
                else
                    return name.SubText(n + key.Length);
            }
            return name;
        }

        /// <summary>获取最后出现的字符串及后面的部分。如"a.asp".SubstringFrom(".") => "asp"</summary>
        /// <param name="keepKey">是否保留键。如"/Pages/test.aspx".TiemEndFrom("/",true) => "/test.aspx"</param>
        public static string GetEnd(this string name, string key, bool keepKey = false)
        {
            if (name.IsEmpty())
                return "";
            var n = name.LastIndexOf(key);
            if (n != -1)
            {
                if (keepKey)
                    return name.SubText(n);
                else
                    return name.SubText(n + key.Length);
            }
            return name;
        }


        /// <summary>获取前面出现的字符串（直到指定的键值）。如"a.asp".GetStart(".") => "a"</summary>
        /// <param name="keepKey">是否保留键。如"/Pages/test.aspx".GetStart("/",true) => "/"</param>
        public static string GetStart(this string name, string key, bool keepKey = false)
        {
            if (name.IsEmpty())
                return "";
            var n = name.IndexOf(key);
            if (n != -1)
            {
                if (keepKey)
                    return name.SubText(0, n + key.Length);
                else
                    return name.SubText(0, n);
            }
            return name;
        }


        /// <summary>是否包含</summary>
        public static bool Contains(this string source, string value, bool ignoreCase)
        {
            if (source.IsEmpty() || value.IsEmpty())
                return false;

            var sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return (source.IndexOf(value, sc) >= 0);
        }

        /// <summary>转化为逗号分隔的字符串</summary>
        public static string ToSeparatedString(this IEnumerable source, string seperator = ",")
        {
            if (source == null)
                return "";
            string txt = "";
            foreach (var item in source)
                txt += item.ToText() + seperator;
            return txt.TrimEnd(seperator);
        }

        /// <summary>拆分字符串并转化为对象列表（可处理 , ; tab space）</summary>
        public static List<T> Split<T>(this string text)
        {
            List<T> items = new List<T>();
            if (text != null)
            {
                var parts = text.Split(new char[] { ',', ';', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                items = parts.Cast(t => t.Parse<T>());
            }
            return items;
        }

        /// <summary>拆分字符串并转化为字符串列表（可处理 , ; tab space）</summary>
        public static List<string> SplitString(this string text)
        {
            return Split<string>(text);
        }
        /// <summary>拆分字符串并转化为长整型列表（可处理 , ; tab space）</summary>
        public static List<long> SplitLong(this string text)
        {
            return Split<long>(text);
        }


        //--------------------------------------------------
        // 引号处理
        //--------------------------------------------------
        /// <summary>给字符串加上双引号。Qoutes string and escapes fishy('\',"') chars.</summary>
        public static string Quote(this string text)
        {
            // String is already quoted-string.
            if (text != null && text.StartsWith("\"") && text.EndsWith("\""))
                return text;

            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\\')
                    retVal.Append("\\\\");
                else if (c == '\"')
                    retVal.Append("\\\"");
                else
                    retVal.Append(c);
            }
            return "\"" + retVal.ToString() + "\"";
        }

        /// <summary>去除外层的双引号。Unquotes and unescapes escaped chars specified text. For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.</summary>
        public static string Unquote(this string text)
        {
            int startPosInText = 0;
            int endPosInText = text.Length;

            //--- Trim. We can't use standard string.Trim(), it's slow. ----//
            for (int i = 0; i < endPosInText; i++)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                    startPosInText++;
                else
                    break;
            }
            for (int i = endPosInText - 1; i > 0; i--)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                    endPosInText--;
                else
                    break;
            }

            // All text trimmed
            if ((endPosInText - startPosInText) <= 0)
                return "";

            // Remove starting and ending quotes.         
            if (text[startPosInText] == '\"')
                startPosInText++;
            if (text[endPosInText - 1] == '\"')
                endPosInText--;

            // Just '"'
            if (endPosInText == startPosInText - 1)
                return "";

            char[] chars = new char[endPosInText - startPosInText];
            int posInChars = 0;
            bool charIsEscaped = false;
            for (int i = startPosInText; i < endPosInText; i++)
            {
                char c = text[i];
                // Escaping char
                if (!charIsEscaped && c == '\\')
                    charIsEscaped = true;

                // Escaped char
                else if (charIsEscaped)
                {
                    // TODO: replace \n,\r,\t,\v ???
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
                // Normal char
                else
                {
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
            }
            return new string(chars, 0, posInChars);
        }

        /// <summary>给指定字符加斜杠（Escapes specified chars in the specified string.）</summary>
		public static string Escape(this string text, params char[] charsToEscape)
        {
            // Create worst scenario buffer, assume all chars must be escaped
            char[] buffer = new char[text.Length * 2];
            int nChars = 0;
            foreach (char c in text)
            {
                foreach (char escapeChar in charsToEscape)
                {
                    if (c == escapeChar)
                    {
                        buffer[nChars] = '\\';
                        nChars++;
                        break;
                    }
                }

                buffer[nChars] = c;
                nChars++;
            }
            return new string(buffer, 0, nChars);
        }


        /// <summary>去除所有斜杠转义字符（Unescapes all escaped chars.）</summary>
        public static string Unescape(this string text)
        {
            // Create worst scenarion buffer, non of the chars escaped.
            char[] buffer = new char[text.Length];
            int nChars = 0;
            bool escapedCahr = false;
            foreach (char c in text)
            {
                if (!escapedCahr && c == '\\')
                    escapedCahr = true;
                else
                {
                    buffer[nChars] = c;
                    nChars++;
                    escapedCahr = false;
                }
            }

            return new string(buffer, 0, nChars);
        }


        //--------------------------------------------------
        // 
        //--------------------------------------------------
        /// <summary>生成随机文本</summary>
        /// <param name="chars">字符集合</param>
        /// <param name="length">要生成的文本长度</param>
        public static string BuildRandomText(string chars = "0123456789", int length = 6)
        {
            var sb = new StringBuilder();
            var rnd = new Random(BuildRandomSeed());
            for (int i = 0; i < length; i++)
            {
                var index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
        }
        static int BuildRandomSeed()
        {
            var bytes = new byte[4];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>解析逗号表达式</summary>
        public static int[] ToIntArray(this string commaText)
        {
            if (String.IsNullOrEmpty(commaText))
                return new int[0];
            else
                return commaText.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
        }

        /// <summary>重复字符串</summary>
        public static string Repeat(this string c, int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
                sb.Append(c);
            return sb.ToString();
        }

        
        /// <summary>安全裁剪字符串（可替代SubString()方法）</summary>
        /// <param name="startIndex">开始字符位置。base-0</param>
        public static string SubText(this string text, int startIndex, int length=-1)
        {
            if (text.IsEmpty()) return "";
            var n = text.Length;
            if (startIndex >= n)
                return "";
            if (length == -1)
                return text.Substring(startIndex);
            if (startIndex + length > n)
                return text.Substring(startIndex);
            return text.Substring(startIndex, length);
        }


        /// <summary>获取遮罩文本（XXXXXXXXXX****XXXX）</summary>
        /// <param name="n">文本最终长度</param>
        /// <param name="mask">遮罩字符（默认为.）</param>
        public static string Mask(this string text, int n, string mask="*")
        {
            if (text.IsEmpty() || text.Length < n)
                return text;
            else
            {
                int len = text.Length;
                string masks = mask.Repeat(4);
                return text.Substring(0, len - 8) + masks + text.Substring(n - 4, 4);
            }
        }

        /// <summary>获取摘要。格式如 xxxxxx... </summary>
        public static string Summary(this string text, int n)
        {
            if (text.IsEmpty() || text.Length < n)
                return text;
            else
                return text.Substring(0, n) + "....";
        }

        /// <summary>转化为首字母小写字符串</summary>
        public static string ToLowCamel(this string text)
        {
            if (text.IsEmpty())
                return "";
            return text.Substring(0, 1).ToLower() + text.Substring(1).ToLower();
        }

        /// <summary>转化为首字母大写字符串</summary>
        public static string ToHighCamel(this string text)
        {
            if (text.IsEmpty())
                return "";
            return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
        }

        /// <summary>转化为文件大小文本（如 1.3M）</summary>
        public static string ToSizeText(this long bytes, string format="{0:#.##}")
        {
            if (bytes < 0)
                return "0";
            else if (bytes >= 1024L * 1024 * 1024 * 1024)
                return string.Format(format, (double)bytes / (1024L * 1024 * 1024 * 1024)) + " TB";
            else if (bytes >= 1024 * 1024 * 1024)
                return string.Format(format, (double)bytes / (1024 * 1024 * 1024)) + " GB";
            else if (bytes >= 1024 * 1024)
                return string.Format(format, (double)bytes / (1024 * 1024)) + " MB";
            else if (bytes >= 1024)
                return string.Format(format, (double)bytes / 1024) + " KB";
            else
                return string.Format("{0} bytes", bytes);
        }

        //--------------------------------------------------
        // 正则表达式处理字符串
        //--------------------------------------------------
        /// <summary>去除 XML 标签（包含注释）</summary>
        public static string RemoveTag(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, "<[^>]*>", "");                                               // 标签
            text = Regex.Replace(text, @"<!-.*", "", RegexOptions.IgnoreCase);                      // 注释头
            text = Regex.Replace(text, @"->", "", RegexOptions.IgnoreCase);                         // 注释尾
            return text;
        }

        /// <summary>去除脚本标签块</summary>
        public static string RemoveScriptBlock(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"<script[^>]*>[\s\S]*</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
        }

        /// <summary>去除样式标签块</summary>
        public static string RemoveStyleBlock(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"<style[^>]*>[\s\S]*</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
        }

        /// <summary>去除不可见的空白字符（[\t\n\r\f\v]）</summary>
        public static string RemoveBlank(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"\s+", "", RegexOptions.IgnoreCase);
        }

        /// <summary>去除空白字符转义符（[\t\n\r\f\v]）</summary>
        public static string RemoveBlankTranslator(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, @"\\t+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\n+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\r+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\f+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\v+", "", RegexOptions.IgnoreCase);
            return text;
        }

        /// <summary>瘦身：合并多个空白符为一个空格；去除头尾的空格</summary>
        public static string Slim(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.IgnoreCase);
            return text.Trim();
        }

        /// <summary>去除所有HTML痕迹（包括脚本、标签、注释、转义符等）</summary>
        public static string RemoveHtml(this string text)
        {
            if (text.IsEmpty()) return "";

            // 删除标签
            text = Regex.Replace(text, @"<script[^>]*>[\s\S]*</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
            text = Regex.Replace(text, @"<style[^>]*>[\s\S]*</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
            text = Regex.Replace(text, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);                  // 标签 <form> <div> </div> </form>
            text = Regex.Replace(text, @"<!-.*", "", RegexOptions.IgnoreCase);                      // 注释头
            text = Regex.Replace(text, @"->", "", RegexOptions.IgnoreCase);                         // 注释尾

            // 处理转义符
            text = Regex.Replace(text, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);              // 空格
            text = Regex.Replace(text, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);              // "
            text = Regex.Replace(text, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);                // &
            text = Regex.Replace(text, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);                 // <
            text = Regex.Replace(text, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);                 // >
            text = Regex.Replace(text, @"&(copy|#169);", "©", RegexOptions.IgnoreCase);              // 
            text = Regex.Replace(text, @"&(reg|#174);", "®", RegexOptions.IgnoreCase);               // 
            text = Regex.Replace(text, @"&(deg|#176);", "°", RegexOptions.IgnoreCase);              // 
            //text = Regex.Replace(text, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);           // ￠
            //text = Regex.Replace(text, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);          // ￡
            //text = Regex.Replace(text, @"&(yen|#165);", "￥", RegexOptions.IgnoreCase);              // ￥
            //text = Regex.Replace(text, @"&(middot|#183);", "·", RegexOptions.IgnoreCase);           // 
            //text = Regex.Replace(text, @"&(sect|#167);", "§", RegexOptions.IgnoreCase);             // 
            //text = Regex.Replace(text, @"&(para|#182);", "¶", RegexOptions.IgnoreCase);              // 
            text = Regex.Replace(text, @"&#(\d+);", "", RegexOptions.IgnoreCase);                    // 未知转义符
            //html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);               // 换行和空白符

            //
            text.Replace("<", "＜");
            text.Replace(">", "＞");
            text.Replace(" ", " ");
            text.Replace("　", " ");
            text.Replace("/'", "'");
            //text.Replace("/"", """);
            //text = HttpUtility.HtmlDecode(text);
            return text.Trim();
        }


    }
}
