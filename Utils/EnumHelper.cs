using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace App.Wechats.Utils
{
    /// <summary>
    /// 枚举值相关信息
    /// </summary>
    internal class EnumInfo
    {
        /// <summary>数字值</summary>
        public int ID { get; set; }

        /// <summary>枚举名</summary>
        public string Title { get; set; }

        /// <summary>枚举值（枚举对象本身）</summary>
        public object Value { get; set; }

        /// <summary>枚举分组（由UIAttribute设置）</summary>
        public string Group { get; set; }

        /// <summary>概述</summary>
        public string FullName => ToString();

        /// <summary>显示文本：英文（分组/名称）</summary>
        public override string ToString()
        {
            return this.Group.IsEmpty()
                    ? string.Format("{0}({1})", this.Value, this.Title)
                    : string.Format("{0}({1}/{2})", this.Value, this.Group, this.Title)
                    ;
        }
    }

    /// <summary>
    /// 枚举相关辅助方法（扩展方法）
    /// 尝试去获取 DescriptionAttribute, UIAttribute 的值作为枚举名称，都没有的话才用原Enum名。
    /// Historey: 
    ///     2017-10-31 Init
    ///     2017-11-01 尝试改为泛型版本失败，泛型不支持枚举约束，但类型转化时又必须指明是类类型还是值类型
    ///     以后再尝试，可用T : struct 来约束
    /// </summary>
    /// <example>
    /// public enum OrderStatus
    /// {
    ///     [Description("新建")]  New;
    ///     [UI("完成")]           Finished;
    /// }
    /// var items = typeof(OrderStatus).ToList();
    /// </example>
    internal static class EnumHelper
    {
        /// <summary>判断一个对象是否是枚举类型</summary>
        public static bool IsEnum(this object value)
        {
            return value?.GetType().BaseType == typeof(Enum);
        }

        /// <summary>判断一个类型是否是枚举类型</summary>
        public static bool IsEnum(this Type type)
        {
            return type?.BaseType == typeof(Enum);
        }

        /// <summary>获取枚举的值列表</summary>
        public static List<T> GetEnums<T>(this Type enumType) where T : struct
        {
            //return Enum.GetValues(enumType).CastEnum<T>();
            var values = new List<T>();
            foreach (var value in Enum.GetValues(enumType))
                values.Add((T)value);
            return values;
        }

        //-------------------------------------------------
        // EnumInfo
        //-------------------------------------------------
        /// <summary>获取一组枚举值的详细信息</summary>
        public static List<EnumInfo> ToEnumInfos(this IList enumValues)
        {
            var infos = new List<EnumInfo>();
            foreach (var item in enumValues)
                infos.Add(item.GetEnumInfo());
            return infos;
        }

        /// <summary>获取枚举值信息（ID,Name,Value,Group)</summary>
        public static EnumInfo GetEnumInfo(this object enumValue)
        {
            var title = enumValue.GetTitle();
            var group = enumValue.GetUIGroup();
            return new EnumInfo() { Title = title, Value = enumValue, ID = (int)enumValue, Group = group };
        }

        /// <summary>将枚举类型转化为列表{Name=xxx, Value=xxx, ID=x, Group=x}</summary>
        public static List<EnumInfo> GetEnumInfos(this Type enumType, params string[] groups)
        {
            var items = new List<EnumInfo>();
            foreach (var value in Enum.GetValues(enumType))
            {
                var info = GetEnumInfo(value);
                if (groups.Length==0 || groups.Contains(info.Group))
                    items.Add(info);
            }
            return items;
        }

        /// <summary>获取权限分组</summary>
        public static List<string> GetEnumGroups(this Type enumType)
        {
            var groups = new List<string>();
            var items = enumType.GetEnumInfos();
            foreach (var item in items)
                if (!groups.Contains(item.Group))
                    groups.Add(item.Group);
            return groups;
            //return groups.Select(t => new { Group = t }).ToList();
        }
    }

}