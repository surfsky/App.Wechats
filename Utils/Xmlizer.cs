using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace App.Wechats.Utils
{


    /// <summary>
    /// Xml序列化及反序列化操作类（无需定义Xml相关的特性标签）
    /// </summary>
    /// <remarks>
    /// Author: surfsky.cnblogs.com
    /// LastUpdate: 2019-04-19
    /// 
    /// [功能]
    /// 将对象序列化为 XML 输出
    ///     支持的数据类型
    ///         简单类型（字符串、时间、基数据类型）：直接输出文本
    ///         简单值List：<Favorite><String>Math</String><String>Art</String></Favorite>
    ///         对象List：<Persons><Person><Name>..</Name></Kevin><Person><Name>..</Name></Kevin></Persons>
    ///         Dict：<Friends><GirlFriend><Name>..</Name></GirlFriend>GirlFriend><BoyFriend>...</BoyFriend></Friends>
    ///         Table：<Incomes><Row><Name>Kevin</Name><Age>21</Age>...</Row></Incomes>
    ///         类及结构体：<Person><Name>Kevin</Name><Age>10</Age></Object>
    ///     格式控制
    ///         时间
    ///         枚举
    ///         标签大小写格式
    ///         是否忽略空值
    ///         
    /// 将 XML 文本解析为对象
    ///     支持的数据类型
    ///         简单类型（字符串、时间、基数据类型）
    ///         简单值List
    ///         对象List
    ///         Dict: 
    ///         类及结构体：
    ///     注意：
    ///         现阶段仅支持标签方式（如<Person><Name>X</Name></Person>），不支持Attribute方式（如<Person Name="X"></Person>)
    ///     
    /// [任务]
    /// 输出
    ///     支持Attribute，定义该属性的序列化和解析方式
    ///         [XmlAttribute] : <Person Name="X"></Person>
    ///         [XmlArray]  或 [XmlSerilizer(typeof(XmlArraySerilizer))]
    ///         [XmlString(useCDATA)]
    ///         [XmlEnum(useInt)]
    ///         [XmlDateTime("yyyy-MM-dd")]
    ///         [XmlTable("Row")]
    ///     优化输出格式控制
    ///         重构输出代码，不直接输出文本
    ///         而是构建一个 XmlDocument 对象，最后再根据格式参数再生成 xml 文本
    ///     XML输出
    ///         避免对象无限循环引用：维护一个List<Object>列表，保存复杂类型数据，若已经有引用了，则不输出该属性
    /// 
    /// 解析
    ///     支持Attribute
    ///     
    /// </remarks>
    internal class Xmlizer
    {
        //-------------------------------------------------
        // 属性
        //-------------------------------------------------
        /// <summary>是否采用LowCamel方式输出标签名称</summary>
        public bool FormatLowCamel { get; set; } = false;

        /// <summary>枚举格式化方式</summary>
        public EnumFomatting FormatEnum { get; set; } = EnumFomatting.Text;

        /// <summary>时间格式化方式</summary>
        public string FormatDateTime { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>是否插入渐进符</summary>
        public bool FormatIndent { get; set; } = false;

        /// <summary>是否忽略空值</summary>
        public bool IgnoreNull { get; set; } = true;

        //-------------------------------------------------
        // 构造析构
        //-------------------------------------------------
        /// <summary>Xml序列化</summary>
        /// <param name="xmlHead">XML文件头<?xml ... ?></param>
        /// <param name="useCData">是否需要CDATA包裹数据</param>
        public Xmlizer(
            bool formatLowCamel=false, EnumFomatting formatEnum=EnumFomatting.Text, string formatDateTime="yyyy-MM-dd HH:mm:ss", bool formatIndent=false,
            bool ignoreNull=true)
        {
            this.FormatLowCamel = formatLowCamel;
            this.FormatEnum = formatEnum;
            this.FormatDateTime = formatDateTime;
            this.FormatIndent = formatIndent;
            this.IgnoreNull = ignoreNull;
        }

        /// <summary>获取Camel格式名称</summary>
        string GetCamelName(string name)
        {
            return (this.FormatLowCamel) ? name.ToLowCamel() : name;
        }

        /// <summary>获取标签名（根据类型自动命名）</summary>
        string GetTagName(Type type)
        {
            if (type.IsAnonymous())        return GetCamelName("Item");
            if (type.IsDict())             return GetCamelName("Dictionary");
            if (type.IsList())             return GetTagName(type.GetGenericDataType()) + "s";
            if (type.IsArray)              return GetTagName(type.GetElementType()) + "s";
            return GetCamelName(type.Name);
        }

        /// <summary>获取Xml安全文本（将特殊字符用CDATA解决）</summary>
        static string XmlTextEncode(string txt)
        {
            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，可用转义符或CDATA解决
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
                return string.Format("<![CDATA[ {0} ]]>", txt);
            return txt;
        }

        /// <summary>XML标签名称编码</summary>
        static string XmlTagEncode(string txt)
        {
            if (txt.IndexOfAny(new char[] { '<', '&', '/', '>' }) != -1)
                return txt.UrlEncode();
            return txt;
        }

        /// <summary>XML标签名称反解码</summary>
        static string XmlTagDecode(string txt)
        {
            return txt.UrlDecode();
        }


        //-------------------------------------------------
        // 对象转 XML
        //-------------------------------------------------
        #region 将对象序列化为XML
        /// <summary>将对象序列化为 XML</summary>
        /// <param name="rootName">根节点名称</param>
        /// <param name="ignoreNull">是否跳过空元素</param>
        /// <param name="addXmlHead">是否添加xml头部</param>
        public string ToXml(object o, string rootName="", bool addXmlHead=false)
        {
            var sb = new StringBuilder();
            if (rootName.IsEmpty())
                rootName = GetTagName(o.GetType());
            if (addXmlHead)
                sb.AppendFormat("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            sb.AppendFormat("<{0}>", rootName);
            WriteInner(sb, o, rootName);
            sb.AppendFormat("</{0}>", rootName);
            return sb.ToString();
        }


        /// <summary>输出对象的内部 XML文本（不输出外部标签）</summary>
        private void WriteInner(StringBuilder sb, object o, string tagName="", bool ignoreNull=true)
        {
            if (o == null) return;
            // 根据类型进行输出（简单类型直接输出文本；复杂类型输出复杂Xml；顺序不要轻易调整；）
            var type = o.GetType().GetRealType();
            if (o is string)               WriteString(sb, o);
            else if (o is DateTime)        WriteDateTime(sb, o);
            else if (type.IsPrimitive)     WriteValue(sb, o);
            else if (type.IsEnum)          WriteEnum(sb, o);
            else
            {
                tagName = GetCamelName(tagName);
                if (tagName.IsEmpty())
                    tagName = GetTagName(type);

                if (o is DataTable)        WriteDataTable(sb, o);
                else if (o is IDictionary) WriteDict(sb, o);
                else if (o is IEnumerable) WriteList(sb, o);
                else                       WriteClass(sb, o);
            }
        }

        /// <summary>输出字符串类型数据</summary>
        private static void WriteString(StringBuilder sb, object obj)
        {
            sb.Append(XmlTextEncode(obj.ToText()));
        }

        /// <summary>输出枚举类型数据</summary>
        private void WriteEnum(StringBuilder sb, object obj)
        {
            if (this.FormatEnum == EnumFomatting.Int) sb.AppendFormat("{0:d}", obj);
            else sb.AppendFormat("{0}", obj);
        }

        /// <summary>输出时间类型数据</summary>
        private void WriteDateTime(StringBuilder sb, object obj)
        {
            var dt = Convert.ToDateTime(obj);
            if (dt != new DateTime())
                sb.AppendFormat(dt.ToString(this.FormatDateTime));
        }

        /// <summary>输出值类型数据</summary>
        private static void WriteValue(StringBuilder sb, object obj)
        {
            sb.AppendFormat("{0}", obj);
        }

        /// <summary>输出列表类型数据</summary>
        private void WriteList(StringBuilder sb, object obj)
        {
            foreach (var item in (obj as IEnumerable))
            {
                var node = SerializationNode.FromType(item.GetType());
                // 无论简单类型还是复杂类型，列表元素输出都要加上类型名。格式如：<Person>...</Person> or <String>...</String>
                sb.AppendFormat("<{0}>", node.Name);
                WriteInner(sb, item, "");
                sb.AppendFormat("</{0}>", node.Name);
            }
        }

        /// <summary>输出字典类型数据</summary>
        /// <param name="keyValueMode">健值模式还是Item模式</param>
        private void WriteDict(StringBuilder sb, object obj)
        {
            var dict = (obj as IDictionary);
            foreach (var key in dict.Keys)
            {
                var tag = XmlTagEncode(key.ToText());
                sb.AppendFormat("<{0}>", tag);
                WriteInner(sb, dict[key], "");
                sb.AppendFormat("</{0}>", tag);
            }
        }

        /// <summary>输出数据表类型数据</summary>
        private void WriteDataTable(StringBuilder sb, object obj)
        {
            var table = obj as DataTable;
            var cols = table.Columns;
            foreach (DataRow row in table.Rows)
            {
                sb.AppendFormat("<Row>");
                foreach (DataColumn col in cols)
                {
                    var tag = col.ColumnName;
                    sb.AppendFormat("<{0}>", tag);
                    WriteInner(sb, row[tag], tag);
                    sb.AppendFormat("<{0}>", tag);
                }
                sb.AppendFormat("</Row>");
            }
        }

        /// <summary>输出类类型数据</summary>
        private void WriteClass(StringBuilder sb, object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // 跳过加忽略标签的节点
                if (Reflector.GetAttribute<NonSerializedAttribute>(property) != null
                    || Reflector.GetAttribute<JsonIgnoreAttribute>(property) != null
                    || Reflector.GetAttribute<System.Xml.Serialization.XmlIgnoreAttribute>(property) != null
                    )
                    continue;
                var subObj = property.GetValue(obj);
                if (subObj == null && this.IgnoreNull)
                    continue;

                var tag = property.Name;
                sb.AppendFormat("<{0}>", tag);
                WriteInner(sb, subObj, tag);
                sb.AppendFormat("</{0}>", tag);
            }
        }
        #endregion


        //-------------------------------------------------
        // XML转对象
        //-------------------------------------------------
        #region 将Xml解析为对象
        /// <summary>解析 XML 字符串为对象（请自行捕捉解析异常）</summary>
        public  T Parse<T>(string xml) where T : class
        {
            return Parse(xml, typeof(T)) as T;
        }

        /// <summary>解析 XML 字符串为对象（请自行捕捉解析异常）</summary>
        public object Parse(string xml, Type type)
        {
            // 简单值类型直接解析
            var node = SerializationNode.FromType(type);
            if (node.Type == SerializationType.Basic)
                return xml.ParseBasicType(type);

            // 复杂类型再解析ML
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return ParseNode(doc.DocumentElement, type);
        }

        /// <summary>将XML节点解析为指定类型的对象</summary>
        object ParseNode(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            if (tag.Type == SerializationType.Basic) return ParseNodeToValue(node, type);
            if (tag.Type == SerializationType.List)   return ParseNodeToList(node, type);
            if (tag.Type == SerializationType.Array)  return ParseNodeToArray(node, type);
            if (tag.Type == SerializationType.Dict)   return ParseNodeToDict(node, type);
            return ParseNodeToObject(node, type);
        }

        /// <summary>将XML节点解析为简单值对象</summary>
        object ParseNodeToValue(XmlNode node, Type type)
        {
            var text = node.InnerText;
            return text.ParseBasicType(type);
        }

        /// <summary>将xml解析为对象</summary>
        object ParseNodeToObject(XmlNode node, Type type)
        {
            if (node.IsEmpty()) return null;
            var o = Activator.CreateInstance(type);
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!p.CanWrite) continue;
                XmlNode cnode = node.SelectSingleNode(p.Name);
                if (cnode == null)
                    continue;

                var value = ParseNode(cnode, p.PropertyType);
                p.SetValue(o, value, null);
            }
            return o;
        }

        /// <summary>将xml解析为集合</summary>
        private IList ParseNodeToList(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var list = Activator.CreateInstance(type) as IList;
            var nodes = node.SelectNodes(tag.ItemType.Name);
            foreach (XmlNode subNode in nodes)
            {
                var item = ParseNode(subNode, tag.ItemType);
                list.Add(item);
            }
            return list;
        }

        /// <summary>将xml解析为数组</summary>
        private object ParseNodeToArray(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var nodes = node.SelectNodes(tag.ItemType.Name);
            Array array = Array.CreateInstance(type, nodes.Count);
            var collection = Convert.ChangeType(array, type);
            int index = 0;
            foreach (XmlNode subNode in nodes)
            {
                var item = ParseNode(subNode, tag.ItemType);
                SetItemValue(collection, item, index++);
            }
            return collection;
        }

        /// <summary>将xml解析为字典</summary>
        /// <remarks>
        /// 格式如：
        ///     <Persons>
        ///         <Kevin>...</Kevin>
        ///         <Willion>.....</Willion>
        ///     </Persons>
        /// </remarks>
        private object ParseNodeToDict(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var dict = Activator.CreateInstance(type) as IDictionary;
            var nodes = node.ChildNodes;
            foreach (XmlNode subNode in nodes)
            {
                var key = XmlTagDecode(subNode.Name);
                var item = ParseNode(subNode, tag.ItemType);
                dict.Add(key, item);
            }
            return dict;
        }



        /// <summary>设置集合某个元素的值</summary>
        private void SetItemValue<T>(T collection, object obj, int index)
        {
            var methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("SetValue"));
            if (methodInfo == null)
                throw new Exception($"反序列化集合xml内容失败，目标{typeof(T).FullName}非集合类型");

            var instance = Expression.Constant(collection);
            var param1 = Expression.Constant(obj);
            var param2 = Expression.Constant(index);
            var addExpression = Expression.Call(instance, methodInfo, param1, param2);
            var setValue = Expression.Lambda<Action>(addExpression).Compile();
            setValue.Invoke();
        }
        #endregion

    }

    /* 测试代码
    public enum Sex
    {
        Male,
        Female
    }
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime? Birthday { get; set; }
        public Sex? Sex { get; set; }
        public string About { get; set; }
        public Person Brother { get; set; }
        public List<Person> Parents { get; set; }
        public List<string> Favorites { get; set; }
        public Dictionary<string, Person> Friends { get; set; }
        public Dictionary<string, float> Scores { get; set; }

        public Person() { }
        public Person(string name) { this.Name = name; }

        public static Person Demo()
        {
            var p = new Person();
            p.Name = "Kevin";
            p.Age = 21;
            p.Birthday = DateTime.Now.AddYears(-21);
            p.Sex = Tests.Sex.Male;
            p.About = "<This is me>";
            p.Brother = new Person() { Name = "Kevin's brother" };
            p.Favorites = new List<string>() { "Art", "Computer" };
            p.Parents = new List<Person>() { new Person("Monther"), new Person("Father") };
            p.Scores = new Dictionary<string, float>()
            {
                {"Math", 99},
                {"English", 100 }
            };
            p.Friends = new Dictionary<string, Person>()
            {
                {"GirlFriend", new Person("Cherry")},
                {"BoyFriend", new Person("Bob") }
            };
            return p;
        }
    }
    var p = Person.Demo();
    var x = p.ToXml("Person");
    Trace.Write(x);

    var o1 = x.ParseXml<Person>();
    Trace.Write(o1.ToJson());
    */

}
