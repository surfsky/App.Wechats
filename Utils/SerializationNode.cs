using App.Wechats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>序列化节点信息（可供 Xmlizer, Jsonlizer 使用）</summary>
    internal class SerializationNode
    {
        public SerializationType Type { get; set; }
        public Type ItemType { get; set; }
        public string Name { get; set; }

        public SerializationNode(SerializationType type, Type itemType, string name)
        {
            this.Type = type;
            this.ItemType = itemType;
            this.Name = name;
        }

        /// <summary>获取类型相关信息</summary>
        public static SerializationNode FromType(Type type)
        {
            var realType = type.GetRealType();
            if (realType.IsSimpleType())    return new SerializationNode(SerializationType.Basic, type, type.Name);
            if (type.IsAnonymous())         return new SerializationNode(SerializationType.Class, type.GenericTypeArguments[0], "Anonymous");
            if (type.IsGenericDict())       return new SerializationNode(SerializationType.Dict, type.GenericTypeArguments[1], type.GenericTypeArguments[1].Name + "s");
            if (type.IsGenericList())       return new SerializationNode(SerializationType.List, type.GenericTypeArguments[0], type.GenericTypeArguments[0].Name + "s");
            if (type.IsArray)               return new SerializationNode(SerializationType.Array, type.GetTypeInfo().GetElementType(), type.GetTypeInfo().GetElementType().ToString() + "s");
            return new SerializationNode(SerializationType.Class, type, type.Name);
        }
    }


    /// <summary>枚举输出方式</summary>
    public enum EnumFomatting
    {
        Text = 0,
        Int = 1
    }

    /// <summary>序列化类型</summary>
    public enum SerializationType
    {
        /// <summary>简单值类型和字符串（可以直接格式化为文本）</summary>
        Basic,
        /// <summary>类或结构体对象</summary>
        Class,
        /// <summary>列表</summary>
        List,
        /// <summary>数组</summary>
        Array,
        /// <summary>字典</summary>
        Dict
    }


}
