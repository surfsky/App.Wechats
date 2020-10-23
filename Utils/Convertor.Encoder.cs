using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.Wechats.Utils
{

    /// <summary>
    /// 编解码辅助
    /// </summary>
    internal static partial class Convertor
    {
        
        //--------------------------------------------------
        // 字节数组
        //--------------------------------------------------
        /// <summary>字节数组转换为数字</summary>
        public static short  ToInt16(this byte[] bytes)  { return BitConverter.ToInt16(bytes,0); }
        public static int    ToInt32(this byte[] bytes)  { return BitConverter.ToInt32(bytes, 0); }
        public static long   ToInt64(this byte[] bytes)  { return BitConverter.ToInt64(bytes, 0); }
        public static ushort ToUInt16(this byte[] bytes) { return BitConverter.ToUInt16(bytes, 0); }
        public static uint   ToUInt32(this byte[] bytes) { return BitConverter.ToUInt32(bytes, 0); }
        public static ulong  ToUInt64(this byte[] bytes) { return BitConverter.ToUInt64(bytes, 0); }
        public static float  ToFloat(this byte[] bytes)  { return BitConverter.ToSingle(bytes, 0); }
        public static double ToDouble(this byte[] bytes) { return BitConverter.ToDouble(bytes, 0); }

        
        /// <summary>转换为字节数组</summary>
        public static byte[] ToBytes(this short n)  { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this int n)    { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this long n)   { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this ushort n) { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this uint n)   { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this ulong n)  { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this float n)  { return BitConverter.GetBytes(n); }
        public static byte[] ToBytes(this double n) { return BitConverter.GetBytes(n); }

        /// <summary>对象转换为字节数组</summary>
        public static byte[] ToObjectBytes(this object o)
        {
            if (o == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter ser = new BinaryFormatter();
                ser.Serialize(ms, o);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        /// <summary>对象转换为字节数组</summary>
        public static object ToObject(this byte[] bytes)
        {
            if (bytes == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                BinaryFormatter ser = new BinaryFormatter();
                var o = ser.Deserialize(ms);
                ms.Close();
                return o;
            }
        }


        /// <summary>将图像转换为字节数组</summary>
        public static byte[] ToBytes(this Image img)
        {
            if (img == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        /// <summary>字符串转换为字节数组</summary>
        public static byte[] ToBytes(this string txt, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return txt.IsEmpty() ? new byte[0] : encoding.GetBytes(txt);
        }

        /// <summary>字节数组转换为字符串</summary>
        public static string ToString(this byte[] bytes, Encoding encoding = null)
        {
            if (bytes == null || bytes.Length == 0)
                return "";
            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetString(bytes);
        }

        
        
        //--------------------------------------------------
        // 流
        //--------------------------------------------------
        /// <summary>将文本转化为流</summary>
        public static MemoryStream ToStream(this string text, Encoding encoding = null)
        {
            return text.ToBytes(encoding).ToStream();
        }

        /// <summary>将字节数组转化为流</summary>
        public static MemoryStream ToStream(this byte[] bytes)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary> 将 Stream 转成 byte[] </summary> 
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }


        //--------------------------------------------------
        // 常用字符串编码转换
        //--------------------------------------------------
        //--------------------------------------------------
        // Url、Html、QueryString
        //--------------------------------------------------
        /// <summary> url编码</summary> 
        /// <param name="enc">编码。默认为 UTF-8</param>
        public static string UrlEncode(this string text, Encoding enc=null)
        {
            enc = enc ?? Encoding.UTF8;
            return HttpUtility.UrlEncode(text, enc);
        }

        /// <summary> url解码</summary> 
        public static string UrlDecode(this string text, Encoding enc = null)
        {
            enc = enc ?? Encoding.UTF8;
            return HttpUtility.UrlDecode(text, enc);
        }

        /// <summary>进行Html编码。格式如：&amp;quot;Name&amp;quot;</summary>
        public static string HtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        /// <summary>进行Html反编码。格式如：&quot;Name&quot;</summary>
        public static string HtmlDecode(this string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        /// <summary>组装QueryString。如：a=1&amp;b=2&amp;c=3</summary>
        public static string ToQueryString(this Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            var i = 0;
            foreach (var item in data)
            {
                i++;
                sb.AppendFormat("{0}={1}", item.Key, item.Value);
                if (i < data.Count)
                    sb.Append("&");
            }
            return sb.ToString();
        }

        //--------------------------------------------------
        // Base64
        //--------------------------------------------------
        /// <summary>转化为Base64字符串编码，如"hvsmnRkNLIX24EaM7KQqIA=="</summary>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>解析Base64字符串，如"hvsmnRkNLIX24EaM7KQqIA=="</summary>
        public static byte[] ToBase64Bytes(this string text)
        {
            return Convert.FromBase64String(text);
        }

        //--------------------------------------------------
        // ASCII
        //--------------------------------------------------
        /// <summary>按位转为 ASCII 字符串，如：86fb269d190d2c85f6e0468ceca42a20</summary>
        public static string ToASCString(this byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>按位转为 ASCII 字符串，如：86fb269d190d2c85f6e0468ceca42a20</summary>
        public static byte[] ToASCBytes(this string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        //--------------------------------------------------
        // 16 进制
        //--------------------------------------------------
        /// <summary>转化为16进制字符串</summary>
        public static string ToHexString(this string text, Encoding enc = null)
        {
            enc = enc ?? Encoding.UTF8;
            var bytes = enc.GetBytes(text);
            return ToHexString(bytes);
        }

        /// <summary>将byte数组转化为16进制字符串（如 86fb269d190d2c85f6e0468ceca42a20）</summary>
        /// <param name="insertSpace">是否插入空格</param>
        public static string ToHexString(this byte[] bytes, bool insertSpace = false)
        {
            if ((bytes == null) || (bytes.Length == 0))
                return "";
            else
            {
                var sb = new StringBuilder();
                var format = insertSpace ? "{0:X2} " : "{0:X2}";
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(string.Format(format, bytes[i]));
                return sb.ToString();
            }
        }

        /// <summary>解析16进制字符串，转化为字节数组（如：86fb269d190d2c85f6e0468ceca42a20）</summary>
        /// <param name="hexText">16进制文本，如 86fb26 或 86 fb 26</param>
        public static byte[] ToHexBytes(this string hexText)
        {
            if (hexText.Contains(' '))
            {
                string[] hexs = hexText.Trim().Split(' ');
                byte[] bytes = new byte[hexs.Length];
                for (int i = 0; i < hexs.Length; i++)
                    bytes[i] = (byte)(int.Parse(hexs[i]));
                return bytes;
            }
            else
            {
                var n = hexText.Length / 2;
                byte[] bytes = new byte[n];
                for (int i=0; i<n; i++)
                {
                    var b = hexText.SubText(i * 2, 2);
                    bytes[i] = byte.Parse(b, System.Globalization.NumberStyles.HexNumber);
                }
                return bytes;
            }
        }

        //--------------------------------------------------
        // 二进制
        //--------------------------------------------------
        /// <summary>转换为二进制文本</summary>
        public static string ToBitString(this short n)  { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this int n)    { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this long n)   { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this ushort n) { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this uint n)   { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this ulong n)  { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this float n)  { return BitConverter.GetBytes(n).ToBitString(); }
        public static string ToBitString(this double n) { return BitConverter.GetBytes(n).ToBitString(); }


        /// <summary>转化为二进制文本</summary>
        /// <param name="order">正顺或逆序。逆序适合查看（高位-低位），正顺适合CPU存取（低位-高位）。</param>
        public static string ToBitString(this byte[] bytes, bool order=false)
        {
            var sb = new StringBuilder(bytes.Length * 8);
            for (int i = 0; i < bytes.Length; i++)
            {
                var bt = order ? bytes[i] : bytes[bytes.Length-i-1];
                sb.Append(Convert.ToString(bt, 2).PadLeft(8, '0'));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>二进制文本转化为字节数组</summary>
        public static byte[] ToBitBytes(this string str)
        {
            var items = Regex.Matches(str, @"([01]{8})");
            byte[] bytes = new byte[items.Count];
            for (int i = 0; i < items.Count; i++)
                bytes[i] = Convert.ToByte(items[i].Value, 2);
            return bytes;
        }

        /// <summary>反转字节数组顺序</summary>
        public static byte[] ReverseBytes(this byte[] bytes)
        {
            if (bytes == null)
                return null;
            var bs = new byte[bytes.Length];
            var n = bytes.Length;
            for (int i = 0; i < n; i++)
                bs[i] = bytes[n-i-1];
            return bs;
        }

        //--------------------------------------------------
        // Unicode
        //--------------------------------------------------
        /// <summary>Unicode编码（如将“亲爱的”编码为\u4eb2\u7231\u7684）</summary>
        public static string UnicodeEncode(this string str)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    if (c > 256)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x"));
                    }
                    else
                        sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>Unicode解码（如将\u4eb2\u7231\u7684解码为亲爱的）</summary>
        public static string UnicodeDecode(this string str)
        {
            return Regex.Unescape(str);
            // 以下实现逻辑供参考
            //Regex reg = new Regex(@"\\[uU]([0-9a-fA-F]{4})");
            //return reg.Replace(str, delegate (Match m) {
            //    return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString();
            //});
        }

    }
}