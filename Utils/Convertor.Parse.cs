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
    /// 负责各种类型转换、列表类型转换
    /// ParseXXXX(string) 负责将字符串解析为对应的类型
    /// ToXXX()           负责将各种数据类型相互转换
    /// CastXXX()         负责列表元素的遍历、转换、筛选
    /// </summary>
    internal static partial class Convertor
    {
        /// <summary>获取类型编码</summary>
        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        //--------------------------------------------------
        // 将文本解析为值或对象
        //--------------------------------------------------
        /// <summary>是否是基础类型（数字、枚举、布尔、日期）</summary>
        public static bool IsBasicType(this Type type)
        {
            if (type.IsNullable())
                type = type.GetRealType();
            if (type == typeof(string))    return true;
            if (type == typeof(int))       return true;
            if (type == typeof(long))      return true;
            if (type == typeof(short))     return true;
            if (type == typeof(uint))      return true;
            if (type == typeof(ulong))     return true;
            if (type == typeof(ushort))    return true;
            if (type == typeof(float))     return true;
            if (type == typeof(double))    return true;
            if (type == typeof(decimal))   return true;
            if (type == typeof(bool))      return true;
            if (type == typeof(DateTime))  return true;
            if (type.IsEnum())             return true;
            return false;
        }

        /// <summary>解析为指定类型对象（数字、枚举、布尔、日期、类对象）</summary>
        public static T Parse<T>(this string text)
        {
            var type = typeof(T);
            if (type.IsBasicType())
                return (T)text.ParseBasicType(typeof(T));
            return (T)text.ParseJson(type);
        }

        /// <summary>解析为指定类型对象（数字、枚举、布尔、日期、类对象）</summary>
        public static object Parse(this string text, Type type, bool ignoreException=false)
        {
            try
            {
                if (type.IsBasicType())
                    return text.ParseBasicType(type);
                return text.ParseJson(type);
            }
            catch( Exception ex)
            {
                IO.Trace(ex.Message);
                if (ignoreException)
                    return null;
                else throw ex;
            }
        }


        /// <summary>将文本解析为基础数据类型（数字、枚举、布尔、日期）</summary>
        /// <remarks>ParseBasicType, ParseSimpleType, ParseValue, ParseNumber</remarks>
        public static object ParseBasicType(this string text, Type type)
        {
            if (type == typeof(string))
                return text;

            // 可空类型
            if (type.IsNullable())
            {
                if (text.IsEmpty())
                    return null;
                type = type.GetRealType();
                if (type == typeof(int))      return text.ParseInt();
                if (type == typeof(long))     return text.ParseLong();
                if (type == typeof(short))    return text.ParseShort();
                if (type == typeof(uint))     return text.ParseUInt();
                if (type == typeof(ulong))    return text.ParseULong();
                if (type == typeof(ushort))   return text.ParseUShort();
                if (type == typeof(float))    return text.ParseFloat();
                if (type == typeof(double))   return text.ParseDouble();
                if (type == typeof(decimal))  return text.ParseDecimal();
                if (type == typeof(bool))     return text.ParseBool();
                if (type == typeof(DateTime)) return text.ParseDate();
                if (type.IsEnum())            return text.ParseEnum(type);
            }

            // 非可空类型，如果文本为空，输出默认值
            if (text.IsEmpty())
            {
                if (type == typeof(int))      return 0;
                if (type == typeof(long))     return 0;
                if (type == typeof(short))    return 0;
                if (type == typeof(uint))     return 0;
                if (type == typeof(ulong))    return 0;
                if (type == typeof(ushort))   return 0;
                if (type == typeof(float))    return 0;
                if (type == typeof(double))   return 0;
                if (type == typeof(decimal))  return 0;
                if (type == typeof(bool))     return true;
                if (type == typeof(DateTime)) return new DateTime();
                if (type.IsEnum())            return "0".ParseEnum(type);
            }

            // 非可空类型，如果文本非空，解析之
            if (type == typeof(int))      return text.ParseInt().Value;
            if (type == typeof(long))     return text.ParseLong().Value;
            if (type == typeof(short))    return text.ParseShort().Value;
            if (type == typeof(uint))     return text.ParseUInt().Value;
            if (type == typeof(ulong))    return text.ParseULong().Value;
            if (type == typeof(ushort))   return text.ParseUShort().Value;
            if (type == typeof(float))    return text.ParseFloat().Value;
            if (type == typeof(double))   return text.ParseDouble().Value;
            if (type == typeof(decimal))  return text.ParseDecimal().Value;
            if (type == typeof(bool))     return text.ParseBool().Value;
            if (type == typeof(DateTime)) return text.ParseDate().Value;
            if (type.IsEnum())            return text.ParseEnum(type);

            // 剩下的不解析了
            return text;
        }


        /// <summary>Parse string to enum object</summary>
        /// <param name="text"></param>
        public static object ParseEnum(this string text, Type enumType)
        {
            try
            {
                return text.IsEmpty() ? null : Enum.Parse(enumType, text, true);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Parse string to enum? </summary>
        /// <param name="text">Enum text(name or value). Eg. "Male" or "0"</param>
        public static T? ParseEnum<T>(this string text) where T : struct
        {
            if (Enum.TryParse<T>(text, true, out T val))
                return val;
            return null;
        }


        /// <summary>解析枚举字符串列表（支持枚举名或值，如Male,Female 或 0,1）</summary>
        /// <param name="text">Enum texts, eg. "Male,Female" or "0,1"</param>
        public static List<T> ParseEnums<T>(this string text, char separator = ',') where T : struct
        {
            var enums = new List<T>();
            if (text.IsNotEmpty())
            {
                var items = text.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    var e = item.ParseEnum<T>();
                    if (e != null)
                        enums.Add(e.Value);
                }
            }
            return enums;
        }

        /// <summary>Parse string to DateTime?</summary>
        public static DateTime? ParseDate(this string text)
        {
            if (DateTime.TryParse(text, out DateTime val))
                return val;
            return null;
        }

        /// <summary>Parse string to decimal?</summary>
        public static decimal? ParseDecimal(this string text)
        {
            if (Decimal.TryParse(text, out Decimal val))
                return val;
            return null;
        }

        /// <summary>Parse string to double?</summary>
        public static double? ParseDouble(this string text)
        {
            if (Double.TryParse(text, out Double val))
                return val;
            return null;
        }

        /// <summary>Parse string to float?</summary>
        public static float? ParseFloat(this string text)
        {
            if (float.TryParse(text, out float val))
                return val;
            return null;
        }

        /// <summary>Parse string to int?</summary>
        public static int? ParseInt(this string text)
        {
            if (Int32.TryParse(text, out Int32 val))
                return val;
            return null;
        }

        /// <summary>Parse string to uint?</summary>
        public static uint? ParseUInt(this string text)
        {
            if (UInt32.TryParse(text, out UInt32 val))
                return val;
            return null;
        }

        /// <summary>Parse string to int64?</summary>
        public static long? ParseLong(this string text)
        {
            if (Int64.TryParse(text, out Int64 val))
                return val;
            return null;
        }

        /// <summary>Parse string to short?</summary>
        public static short? ParseShort(this string text)
        {
            if (Int16.TryParse(text, out Int16 val))
                return val;
            return null;
        }
        /// <summary>Parse string to ushort?</summary>
        public static ushort? ParseUShort(this string text)
        {
            if (UInt16.TryParse(text, out UInt16 val))
                return val;
            return null;
        }

        /// <summary>Parse string to ulong?</summary>
        public static ulong? ParseULong(this string text)
        {
            if (UInt64.TryParse(text, out UInt64 val))
                return val;
            return null;
        }

        /// <summary>Parse string to bool?</summary>
        /// <param name="text">true|false|True|False</param>
        public static bool? ParseBool(this string text)
        {
            if (bool.TryParse(text, out bool val))
                return val;
            return null;
        }

        /// <summary>Parse querystring to dict（eg. id=1&amp;name=Kevin）</summary>
        /// <param name="text">Querystring, eg. id=1&amp;name=Kevin</param>
        public static FreeDictionary<string, string> ParseQueryDict(this string text)
        {
            var dict = new FreeDictionary<string, string>();
            if (text.IsEmpty())
                return dict;
            var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var key = match.Result("$2");
                var value = match.Result("$3");
                dict.Add(key, value);
            }
            return dict;
        }

        /// <summary>Parse json to dict</summary>
        public static Dictionary<string, string> ParseJsonDict(string jsonStr)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
        }

    }
}