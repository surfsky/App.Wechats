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
        /// <summary>转化为泛型类型。如 typeof(EntityBase).GetGenericType(typeof(Role)) </summary>
        public static Type AsGeneric(this Type type, params Type[] parameterTypes)
        {
            return type.MakeGenericType(parameterTypes);
        }

        /// <summary>转化为泛型方法。如 Get<T>(long id) </summary>
        public static MethodInfo AsGeneric(this MethodInfo mi, params Type[] parameterTypes)
        {
            return mi.MakeGenericMethod(parameterTypes);
        }

        /// <summary>获取列表元素的数据类型（尝试返回第一个元素的数据类型）</summary>
        public static Type GetItemType<T>(this IList<T> list)
        {
            Type type = list.GetType();
            Type itemType = type.GetGenericDataType();
            if (list.Count > 0)
                itemType = list[0].GetType();
            return itemType;
        }

        //------------------------------------------------
        // 类型相关
        //------------------------------------------------
        /// <summary>尝试遍历获取类型（根据类型名、数据集名称）</summary>
        /// <param name="type">要查找的类型或接口</param>
        /// <param name="assemblyName">若不为空，则在指定的程序集中寻找。</param>
        /// <param name="ignoreSystemType">是否忽略系统类型</param>
        /// <param name="onlyClass">只查找类（忽略接口和值类型）</param>
        public static List<Type> GetTypes(Type type, string assemblyName = "", bool ignoreSystemType = true, bool onlyClass=true)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                string name = assembly.FullName;

                // 过滤掉系统自带的程序集
                if (ignoreSystemType)
                    if (name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib"))
                        continue;

                // 获取类型或子类型，并过滤掉接口
                var assemTypes = assembly.GetTypes();
                var matchTypes = type.IsInterface ? assemTypes.Search(t => t.IsInterface(type)) : assemTypes.Search(t => t.IsType(type));
                if (matchTypes.Count > 0)
                {
                    if (onlyClass)
                        matchTypes = matchTypes.Search(t => t.IsClass);
                    if (name == assemblyName)
                        return matchTypes;
                    types.AddRange(matchTypes);
                }
            }
            return types;
        }

        /// <summary>尝试遍历获取类型（根据类型名、数据集名称）</summary>
        public static Type GetType(string typeName, string assemblyName = "", bool ignoreSystemType = true)
        {
            if (typeName.IsEmpty())
                return null;
            var type = Assembly.GetExecutingAssembly().GetType(typeName);
            if (type != null)
                return type;

            // 遍历程序集去找这个类
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                string name = assembly.FullName;
                if (name == assemblyName)
                    return assembly.GetType(typeName);

                // 过滤掉系统自带的程序集
                if (ignoreSystemType)
                    if (name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib"))
                        continue;

                // 尝试获取类别
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>创建对象（根据类型）</summary>
        public static object Create(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        /// <summary>尝试创建对象（根据类型名、数据集名称）</summary>
        public static object Create(string typeName, string assemblyName, params object[] args)
        {
            var type = GetType(typeName, assemblyName);
            if (type != null)
                return Create(type, args);
                //return type.Assembly.CreateInstance(type.Name, true);
            return null;
        }

        /// <summary>获取（可空类型的）真实类型</summary>
        public static Type GetRealType(this Type type)
        {
            if (type.IsNullable())
                return GetRealType(type.GetNullableDataType());
            return type;
        }

        /// <summary>获取类型简短名称（不输出版本号及签名，如 System.Nullable`1[[System.Int32]]）</summary>
        /// <examples>
        /// Type t1 = typeof(int);
        /// Type t2 = typeof(int?);
        /// Type t3 = typeof(List<int>);
        /// Type t4 = typeof(List<SexType>);
        /// Type t5 = typeof(Dictionary<string, int>);
        /// Assert.AreEqual(t1.GetShortName(), "System.Int32");
        /// Assert.AreEqual(t2.GetShortName(), "System.Nullable`1[[System.Int32]]");
        /// Assert.AreEqual(t3.GetShortName(), "System.Collections.Generic.List`1[[System.Int32]]");
        /// Assert.AreEqual(t4.GetShortName(), "System.Collections.Generic.List`1[[App.Core.Tests.SexType, App.CoreTest]]");
        /// Assert.AreEqual(t5.GetShortName(), "System.Collections.Generic.Dictionary`2[[System.String],[System.Int32]]");
        /// </examples>
        public static string GetShortName(this Type type)
        {
            if (type.IsGenericType)
            {
                var gType = type.GetGenericTypeDefinition();
                var types = type.GetGenericArguments();
                var name = string.Format("{0}.{1}", gType.Namespace, gType.Name);
                return string.Format("{0}[{1}]", name, types.Select(t => "[" + t.GetShortName() + "]").ToSeparatedString(","));
            }
            var assemblyName = type.Assembly.GetName().Name;
            if (assemblyName == "mscorlib")
                return type.FullName;
            else
                return string.Format("{0}, {1}", type.FullName, assemblyName);
        }


        /// <summary>获取类型字符串（如 Int32? List&lt;T&gt;）</summary>
        public static string GetTypeString(this Type type, bool shortName = true)
        {
            if (type.IsNullable())
            {
                type = type.GetNullableDataType();
                return GetTypeString(type, shortName) + "?";
            }
            if (type.IsGenericType)
            {
                var gType = type.GetGenericTypeDefinition();
                var types = type.GetGenericArguments();
                var name = shortName ? gType.Name : gType.FullName;
                name = name.TrimEnd("`");
                return string.Format("{0}<{1}>", name, types.Select(t => t.GetTypeString()).ToSeparatedString(", "));
            }
            if (type.IsValueType)
                return type.Name;
            return shortName ? type.Name : type.FullName;
        }

        /// <summary>获取方法描述字符串</summary>
        public static string GetMethodString(this MethodInfo m)
        {
            var ps = m.GetParameters();
            var staticString = m.IsStatic ? "static " : "";
            return string.Format("{0}{1} {2}({3})",
                staticString,
                m.ReturnType.GetTypeString(),
                m.Name,
                ps.Select(p => $"{p.ParameterType.GetTypeString()} {p.Name}").ToSeparatedString(", ")
                );
        }


        /// <summary>获取枚举类型的数据信息</summary>
        public static string GetEnumString(this Type type)
        {
            if (type == null)
                return "";
            type = type.GetRealType();
            var sb = new StringBuilder();
            if (type.IsEnum)
            {
                foreach (var item in Enum.GetValues(type))
                    sb.AppendFormat("{0}-{1}({2}); ", (int)item, item.ToString(), item.GetTitle());
            }
            return sb.ToString();
        }

        //------------------------------------------------
        // 类型相关
        //------------------------------------------------
        /// <summary>是否是某个类型（或子类型）</summary>
        public static bool IsType(this Type raw, Type match)
        {
            return (raw == match) ? true : raw.IsSubclassOf(match);
        }

        /// <summary>是否实现接口</summary>
        public static bool IsInterface(this Type raw, Type match)
        {
            return (raw == match) ? true : match.IsAssignableFrom(raw);
        }


        /// <summary>是否属于某个类型</summary>
        public static bool IsType(this Type type, string typeName)
        {
            if (type.ToString() == typeName)
                return true;
            if (type.ToString() == "System.Object")
                return false;
            return IsType(type.BaseType, typeName);
        }


        /// <summary>是否是列表</summary>
        public static bool IsList(this Type type)
        {
            return type.GetInterface("IList") != null;
        }

        /// <summary>是否是字典</summary>
        public static bool IsDict(this Type type)
        {
            return type.GetInterface("IDictionary") != null;
        }

        /// <summary>是否是集合（包括列表和字典）</summary>
        public static bool IsCollection(this Type type)
        {
            // ICollection<T> : IEnumab.....
            if (type.Name.Contains("ICollection"))
                return true;
            // 其它继承了 ICollection 的子类
            return type.GetInterface("ICollection") != null;
        }

        /// <summary>是否是泛型列表</summary>
        public static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.IsList();
        }

        /// <summary>是否是泛型字典</summary>
        public static bool IsGenericDict(this Type type)
        {
            return type.IsGenericType && type.IsDict();
        }

        /// <summary>是否是匿名类</summary>
        public static bool IsAnonymous(this Type type)
        {
            return type.Name.Contains("AnonymousType");
        }

        /// <summary>是否是泛型类型</summary>
        public static bool IsGeneric(this Type type)
        {
            return type.IsGenericType;
        }

        /// <summary>是否是简单值类型: String + DateTime + 枚举 + 基元类型(Boolean， Byte， SByte， Int16， UInt16， Int32， UInt32， Int64， UInt64， IntPtr， UIntPtr， Char，Double，和Single)</summary>
        public static bool IsSimpleType(this Type type)
        {
            return (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type.IsEnum);
        }

        /// <summary>是否是简单值类型: String + DateTime + 枚举 + 基元类型(Boolean， Byte， SByte， Int16， UInt16， Int32， UInt32， Int64， UInt64， IntPtr， UIntPtr， Char，Double，和Single)</summary>
        public static bool IsNumber(this Type type)
        {
            if (type == typeof(Int16))   return true;
            if (type == typeof(Int32))   return true;
            if (type == typeof(Int64))   return true;
            if (type == typeof(UInt16))  return true;
            if (type == typeof(UInt32))  return true;
            if (type == typeof(UInt64))  return true;
            if (type == typeof(Double))  return true;
            if (type == typeof(Single))  return true;
            if (type == typeof(Decimal)) return true;
            return false;
        }


        /// <summary>是否是可空类型</summary>
        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>获取可空类型中的值类型</summary>
        public static Type GetNullableDataType(this Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                return type.GetGenericArguments()[0];
            return type;
        }

        /// <summary>获取泛型中的数据类型</summary>
        public static Type GetGenericDataType(this Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericArguments()[0];
            return type;
        }


    }
}