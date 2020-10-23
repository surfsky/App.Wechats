using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
//using System.Drawing.Imaging;
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
    /// XML 相关的辅助类
    /// </summary>
    internal static class XmlHelper
    {
        //------------------------------------------
        // xml
        //------------------------------------------
        /// <summary>将XML字符串转为Json字符串（慎用，层次和属性都可能有差异）</summary>
        /// <example>
        /// "<Person><Name>Kevin</Name><Age>21</Age></Person>"  -> {"Person":{"Name":"Kevin","Age":"21"}}
        /// </example>
        public static string ParseXmlToJson(this string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, true);
        }

        /// <summary>将 Json 字符串解析为动态对象</summary>
        public static dynamic ParseDynamic(this string json)
        {
            return JsonConvert.DeserializeObject<dynamic>(json);
        }

        /// <summary>解析XML字符串为对象</summary>
        public static T ParseXml<T>(this string xml) where T : class
        {
            return new Xmlizer().Parse<T>(xml);
        }

        /// <summary>解析XML字符串为对象</summary>
        public static object ParseXml(this string xml, Type type)
        {
            return new Xmlizer().Parse(xml, type);
        }

        /// <summary>解析XML字符串为 Xml 文档对象</summary>
        public static XmlDocument ParseXml(this string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>将对象转化为 XML 字符串</summary>
        public static string ToXml(this object o, string rootName = "xml")
        {
            return new Xmlizer().ToXml(o, rootName);
        }



        //-------------------------------------
        // XML 文件读写
        //-------------------------------------
        /// <summary>保存对象为 Xml 文件</summary>
        public static void SaveXmlFile(this object obj, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(obj.ToXml());
                writer.Close();
            }
        }

        /// <summary>加载 XML 文件并解析为对象</summary>
        public static object LoadXmlFile(string filePath, Type type)
        {
            if (!File.Exists(filePath))
                return null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                var obj = reader.ReadToEnd().ParseXml(type);
                reader.Close();
                return obj;
            }
        }

        /// <summary>加载 XML 文件并解析为对象</summary>
        public static T LoadXmlFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
                return null;
            var txt = File.ReadAllText(filePath);
            return txt.ParseXml<T>();
        }
    }
}