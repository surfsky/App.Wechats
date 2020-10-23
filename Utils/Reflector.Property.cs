using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 反射相关静态方法和属性
    /// </summary>
    internal static partial class Reflector
    {
        //------------------------------------------------
        // 读写属性
        //------------------------------------------------
        /// <summary>获取对象的属性值。也可考虑用dynamic实现。</summary>
        /// <param name="propertyName">属性名。可考虑用nameof()表达式来实现强类型。</param>
        public static object GetValue(this object obj, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                int n = propertyName.IndexOf(".");
                var name = propertyName.Substring(0, n);
                var nextName = propertyName.Substring(n+1);
                obj = GetValue(obj, name);
                return GetValue(obj, nextName);
            }
            else
            {
                PropertyInfo pi = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                return pi.GetValue(obj);
            }
        }

        /// <summary>设置对象的属性值。</summary>
        public static void SetValue(this object obj, string propertyName, object propertyValue)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Type type = pi.PropertyType;
            Type propertyType = type.GetRealType();
            var valueType = propertyValue?.GetType().GetRealType();

            // 比较属性的类型和值的类型, 如果一致就直接赋值, 不一致就转化为文本再处理
            if (propertyType == valueType)
                pi.SetValue(obj, propertyValue);
            else
            {
                var txt = propertyValue.ToText();
                if (propertyValue.IsEnum())
                    txt = ((int)propertyValue).ToString();
                else if (propertyType == typeof(DateTime))
                    txt = string.Format("{0:yyyy-MM-dd HH:mm:ss}", propertyValue);
                SetValue(obj, propertyName, txt);
            }
        }

        /// <summary>设置对象的属性值（用文本，转化为相应的数据类型），需要测试，给非空类型赋予可空数据会出错的</summary>
        public static void SetValue(this object obj, string propertyName, string propertyValue)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            Type type = pi.PropertyType;
            if (type == typeof(string))
            {
                pi.SetValue(obj, propertyValue, null);
                return;
            }

            // 将字符串转化为对应的值类型
            Type realType = type.GetRealType();
            object value = propertyValue;
            if      (type == typeof(bool))                 value = propertyValue.ParseBool() ?? false;
            else if (type == typeof(Int64))                value = propertyValue.ParseLong() ?? 0;
            else if (type == typeof(Int32))                value = propertyValue.ParseInt() ?? 0;
            else if (type == typeof(Int16))                value = propertyValue.ParseShort() ?? 0;
            else if (type == typeof(DateTime))             value = propertyValue.ParseDate() ?? new DateTime();
            else if (type == typeof(float))                value = propertyValue.ParseFloat() ?? 0.0;
            else if (type == typeof(double))               value = propertyValue.ParseDouble() ?? 0.0;
            else if (type == typeof(decimal))              value = propertyValue.ParseDecimal() ?? (decimal)0.0;
            else if (type == typeof(bool?))                value = propertyValue.ParseBool();
            else if (type == typeof(Int64?))               value = propertyValue.ParseLong();
            else if (type == typeof(Int32?))               value = propertyValue.ParseInt();
            else if (type == typeof(Int16?))               value = propertyValue.ParseShort();
            else if (type == typeof(DateTime?))            value = propertyValue.ParseDate();
            else if (type == typeof(float?))               value = propertyValue.ParseFloat();
            else if (type == typeof(double?))              value = propertyValue.ParseDouble();
            else if (type == typeof(decimal?))             value = propertyValue.ParseDecimal();
            else if (type.IsEnum)                          value = propertyValue.ParseEnum(realType);
            else if (type.IsNullable() && realType.IsEnum) value = propertyValue.ParseEnum(realType);

            // 赋值
            pi.SetValue(obj, value, null);
        }



        /// <summary>获取对象的属性值（强类型版本）。var name = user.GetPropertyValue(t=> t.Name);</summary>
        public static TValue GetValue<T, TValue>(this T obj, Expression<Func<T, TValue>> property)
        {
            return property.Compile().Invoke(obj);
        }

        /// <summary>设置对象的属性值（强类型版本）。user.SetPropertyValue(t=> t.Name, "Cherry");</summary>
        public static void SetValue<T, TValue>(this T obj, Expression<Func<T, TValue>> property, TValue value)
        {
            string name = GetName(property);
            typeof(T).GetProperty(name).SetValue(obj, value, null);
        }


        //------------------------------------------------
        // Name
        //------------------------------------------------
        /// <summary>Get expresson from Func. var exp = ExpressOf((Person p)=> p.Name);</summary>
        public static Expression<Func<T, TR>> ExpressOf<T, TR>(this Expression<Func<T, TR>> expression)
        {
            return expression;
        }

        /// <summary>获取表达式名。GetName&lt;User&gt;(t =&gt; t.Dept.Name);</summary>
        public static string GetName<T>(Expression<Func<T, object>> expression)
        {
            return expression.GetName();
        }

        /// <summary>获取表达式名。GetName&lt;User&gt;(t =&gt; t.Dept.Name);</summary>
        public static string GetName(this Expression expr)
        {
            if (expr == null)
                return "";
            // Lambda 表达式
            if (expr is LambdaExpression le)
                return GetName(le.Body);
            // 一元操作符: array.Length, Convert(t.CreatDt)
            if (expr is UnaryExpression ue)
                return GetName((MemberExpression)ue.Operand);
            // 成员操作符： t.Dept.Name => body=t.Dept, member=Name
            if (expr is MemberExpression me)
            {
                var name = me.Member.Name;
                if (me.Expression is MemberExpression)
                    return GetName(me.Expression) + "." + name;
                else
                    return name;
            }
            // 参数本身：t 返回类型名
            if (expr is ParameterExpression pe)
                return pe.Type.Name;
            return "";
        }

        //------------------------------------------------
        // Property
        //------------------------------------------------
        /// <summary>获取对象的属性信息</summary>
        /// <param name="propertyName">属性名。可考虑用nameof()表达式来实现强类型。</param>
        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        /// <summary>获取对象的属性信息。GetProperty&lt;Person&gt;(t =&gt; t.Name)</summary>
        public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression)
        {
            return expression.GetProperty();
        }

        /// <summary>获取表达式属性信息(t.Dept.Name => Name)</summary>
        public static PropertyInfo GetProperty(this Expression expr)
        {
            if (expr == null)
                return null;
            // Lambda 表达式
            if (expr is LambdaExpression le)
                return GetProperty(le.Body);
            // 一元操作符: array.Length, Convert(t.CreatDt)
            if (expr is UnaryExpression ue)
                return GetProperty((MemberExpression)ue.Operand);
            // 成员操作符： t.Dept.Name : expression=t.Dept, member=Name
            if (expr is MemberExpression me)
                return me.Member as PropertyInfo;
            return null;
        }


    }
}