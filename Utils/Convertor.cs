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
    /// ParseXXXX(string)      负责将字符串解析为对应的类型
    /// ToXXX()                负责将各种数据类型相互转换
    /// CastXXX()              负责列表元素的遍历、转换、筛选
    /// XXXEncode() XXXDecode  负责编解码
    /// </summary>
    internal static partial class Convertor
    {
        //--------------------------------------------------
        // 参数转换
        //--------------------------------------------------
        /// <summary>数组转化为列表</summary>
        public static List<T> ToList<T>(params T[] array)
        {
            List<T> items = new List<T>();
            foreach (var item in array)
                items.Add(item);
            return items;
        }


        //--------------------------------------------------
        // 安全转化为文本
        //--------------------------------------------------
        /// <summary>将可空对象转化为字符串（注意bool字符串是首字母大写的True|False)</summary>
        public static string ToText(this object o, string format="")
        {
            if (o == null)
                return "";

            // 其它的格式自行处理
            if (format.IsEmpty())
                format = "{0}";
            else if(!format.Contains("{"))
                format = "{0:" + format + "}";
            return string.Format(format, o);
        }

        /// <summary>将可空bool对象转化为字符串</summary>
        public static string ToText(this bool? o, string trueText = "True", string falseText = "False")
        {
            return (o == null) ? "" : (o.Value ? trueText : falseText);
        }


        //--------------------------------------------------
        // 安全转化为枚举（可以用To<T>替代
        //--------------------------------------------------
        // C# 泛型不支持 where T : int, long, short, ....
        /// <summary>数字转化为枚举</summary>
        public static T? ToEnum<T>(this int? n) where T : struct
        {
            if (n == null) return null;
            return (T)Enum.ToObject(typeof(T), n);
        }
        /// <summary>数字转化为枚举</summary>
        public static T? ToEnum<T>(this long? n) where T : struct
        {
            if (n == null) return null;
            return (T)Enum.ToObject(typeof(T), n);
        }


        //--------------------------------------------------
        // 自由转化
        //--------------------------------------------------
        /// <summary>数据类型转换（支持简单类型、可空类型、字典、json）</summary>
        public static T To<T>(this object o)
        {
            return (T)To(o, typeof(T));
        }

        /// <summary>数据类型转换（支持简单类型、可空类型、字典、json）</summary>
        public static object To(this object o, Type type)
        {
            // 为空直接输出
            if (o == null)
                return null;

            // 类型一致直接输出
            if (o.GetType() == type)
                return o;
            if (o.GetType().IsSubclassOf(type))
                return o;

            // 可空类型：转化为非空类型后再处理
            if (type.IsNullable())
            {
                if ((o as string) == "")
                    return null;
                return To(o, type.GetRealType());
            }

            // 简单类型处理（字符串、枚举、日期、值类型）
            if (type == typeof(string))
                return o.ToString();
            if (o is string s)
                return s.Parse(type);
            if (type.IsEnum)
                return o.ToString().ParseEnum(type);
            if (type.IsValueType)
                return Convert.ChangeType(o, type);  // 该方法无法处理数字到枚举的转化

            // 字典解析
            if (o is Dictionary<string, object> d)
                return To(d, type);

            // 复杂类型解析（先转化为json再解析）
            return o.ToJson().ParseJson(type);
        }


        /// <summary>将字典转化为指定类型对象</summary>
        public static object To(this Dictionary<string, object> dic, Type type)
        {
            object o = type.Assembly.CreateInstance(type.FullName);
            foreach (PropertyInfo p in type.GetProperties())
            {
                string name = p.Name;
                if (dic.ContainsKey(name))
                {
                    Type propertyType = p.PropertyType;
                    object propertyValue = dic[name];
                    if (propertyValue is Dictionary<string, object>)
                        propertyValue = To(propertyValue as Dictionary<string, object>, propertyType);
                    else
                        propertyValue = ToBasicObject(propertyValue, propertyType);
                    p.SetValue(o, propertyValue, null);
                }
            }
            return o;
        }

        /// <summary>转化为基础数据类型（数字、枚举、日期）</summary>
        public static object ToBasicObject(this object o, Type type)
        {
            if (type.IsSubclassOf(typeof(Enum)))
                return Enum.Parse(type, o.ToString());

            switch (type.FullName)
            {
                case "System.DateTime":
                    return Convert.ToDateTime(o);
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return Convert.ToInt64(o);
                case "System.Boolean":
                    return Convert.ToBoolean(o);
                case "System.Char":
                    return Convert.ToChar(o);
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                    return Convert.ToDouble(o);
                default:
                    return o;
            }
        }

        /*
        /// <summary>将可空对象转化为整型字符串</summary>
        public static string ToIntText(this object o)
        {
            if (o == null) return "";
            return Convert.ToInt32(o).ToString();
        }

        // 以下代码由于侵入性太强及未处理异常而废除，请改用ParseXXX() 方法
        /// <summary>将可空对象转化为整型</summary>
        public static int? ToInt(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToInt32(o);
        }


        /// <summary>将可空对象转化为长整型</summary>
        public static long? ToInt64(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToInt64(o);
        }

        /// <summary>将可空对象转化为Float</summary>
        public static float? ToFloat(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToSingle(o);
        }

        /// <summary>将可空对象转化为Double</summary>
        public static double? ToDouble(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToDouble(o);
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static bool? ToBool(this object o)
        {
            if (o.IsEmpty()) return null;
            return Boolean.Parse(o.ToString());
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static DateTime? ToDateTime(this object o)
        {
            if (o.IsEmpty()) return null;
            return DateTime.Parse(o.ToString());
        }
        */
    }
}