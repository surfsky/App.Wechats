using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Xml.Serialization;

namespace App.Wechats.Utils
{
    /// <summary>
    /// UI Attribute 辅助扩展方法
    /// </summary>
    internal static class UIExtension
    {
        //--------------------------------------------
        // 获取 UIAttribute
        //--------------------------------------------
        /// <summary>获取类拥有的 UIAttribute 列表</summary>
        public static List<UIAttribute> GetUIAttributes(this Type type)
        {
            var attrs = new List<UIAttribute>();
            foreach (var prop in type.GetProperties())
                attrs.Add(GetPropertyUI(prop));
            return attrs;
        }

        /// <summary>获取属性的 UI 配置信息</summary>
        public static UIAttribute GetPropertyUI(this PropertyInfo prop)
        {
            UIAttribute attr = Reflector.GetAttribute<UIAttribute>(prop);
            if (attr == null)
                attr = new UIAttribute("", prop.Name);
            attr.Name = prop.Name;
            attr.Field = prop;
            attr.Type = attr.Type ?? prop.PropertyType;
            return attr;
        }

        /// <summary>获取类型说明</summary>
        public static UIAttribute GetUIAttribute(this Type type)
        {
            return type.GetCustomAttribute<UIAttribute>();
        }

        /// <summary>获取  UIAttribute </summary>
        public static UIAttribute GetUIAttribute(this Type type, string propertyName)
        {
            var info = type.GetProperty(propertyName);
            return GetUIAttribute(info);
        }

        /// <summary>获取  UIAttribute </summary>
        public static UIAttribute GetUIAttribute(this PropertyInfo info)
        {
            if (info != null)
                return info.GetCustomAttribute<UIAttribute>();
            return null;
        }

        /// <summary>获取枚举值的文本说明（来自UIAttribute或DescriptionAttribute）</summary>
        public static UIAttribute GetUIAttribute(this object enumValue)
        {
            var info = GetEnumField(enumValue);
            if (info != null)
                return info.GetCustomAttribute<UIAttribute>();
            return null;
        }



        //--------------------------------------------
        // Get UIAttribute property
        //--------------------------------------------
        /// <summary>获取标题（来自TAttribute, UIAttribute, DescriptionAttribute, DisplayNameAttribute）</summary>
        /// <param name="info">类型或成员</param>
        public static string GetTitle(this MemberInfo info)
        {
            if (info != null)
            {
                var attr1 = info.GetCustomAttribute<UIAttribute>();
                if (attr1 != null) return attr1.Title.GetResText();

                var attr2 = info.GetCustomAttributes().FirstOrDefault(t => t.GetType() == typeof(TAttribute)) as TAttribute;
                if (attr2 != null) return attr2.Title.GetResText();

                var attr3 = info.GetCustomAttribute<DescriptionAttribute>();
                if (attr3 != null) return attr3.Description.GetResText();

                var attr4 = info.GetCustomAttribute<DisplayNameAttribute>();
                if (attr4 != null) return attr4.DisplayName.GetResText();

                return info.Name.GetResText();
            }
            return "";
        }

        /// <summary>获取枚举值标题。RoleType.Admin.GetTitle()</summary>
        public static string GetTitle(this object enumValue)
        {
            if (enumValue == null || !enumValue.IsEnum())
                return "";
            MemberInfo info = GetEnumField(enumValue);
            return GetTitle(info);
        }

        /// <summary>获取属性标题。product.GetTitle(t =&lt; t.Name)</summary>
        public static string GetTitle<T>(this T t, Expression<Func<T, object>> expression)
        {
            return expression.GetTitle();  //t.GetProperty(expression.GetName())?.GetTitle();
        }

        /// <summary>获取表达式标题。t.Dept.Name => 部门名称</summary>
        public static string GetTitle<T>(this Expression<Func<T, object>> field)
        {
            return field == null ? "" : GetTitle(field.Body);
        }

        /// <summary>获取表达式标题。t.Dept.Name => 部门名称</summary>
        public static string GetTitle(this Expression expr)
        {
            if (expr == null)
                return "";
            // Lambda 表达式
            if (expr is LambdaExpression le)
                return GetTitle(le.Body);
            // 一元操作符: array.Length, Convert(t.CreatDt)
            if (expr is UnaryExpression ue)
                return GetTitle((MemberExpression)ue.Operand);
            // 成员操作符： t.Dept.Name => body=t.Dept, member=Name
            if (expr is MemberExpression me)
            {
                var name = me.Member.GetTitle();
                if (me.Expression is MemberExpression)
                    return GetTitle(me.Expression) + name;
                else
                    return name;
            }
            // 参数本身：t 返回类型名
            if (expr is ParameterExpression pe)
                return pe.Type.Name;
            return "";
        }



        //--------------------------------------------
        // 辅助方法
        //--------------------------------------------
        /// <summary>获取属性对应的 UI 类型（尝试取属性的 UI.EditorType 标注值，没有的话取属性的自身类型）</summary>
        public static Type GetUIType(this PropertyInfo info)
        {
            if (info != null)
            {
                var dataType = info.GetCustomAttribute<UIAttribute>()?.Type;
                return dataType ?? info.PropertyType;
            }
            return null;
        }

        /// <summary>获取枚举值对应的 UI 数据类型（尝试取枚举值的 UI.Type 标注值，没有的话取枚举类型））</summary>
        public static Type GetUIType(this object enumValue)
        {
            var info = GetEnumField(enumValue);
            if (info != null)
            {
                var dataType = info.GetCustomAttribute<UIAttribute>()?.Type;
                return dataType ?? info.FieldType;
            }
            return null;
        }

        /// <summary>获取类型的分组信息。RoleType.GetUIGroup()</summary>
        public static string GetUIGroup(this Type type)
        {
            var ui = GetUIAttribute(type);
            if (ui != null)
                return ui.Group;
            return "";
        }

        /// <summary>获取枚举值的分组信息。RoleType.Admin.GetUIGroup()</summary>
        public static string GetUIGroup(this object enumValue)
        {
            var ui = GetUIAttribute(enumValue);
            if (ui != null)
                return ui.Group;
            return "";
        }

        /// <summary>获取枚举值对应的字段</summary>
        static FieldInfo GetEnumField(this object enumValue)
        {
            if (enumValue == null) return null;
            var enumType = enumValue.GetType();
            return enumType.GetField(enumValue.ToString());
        }

    }

}