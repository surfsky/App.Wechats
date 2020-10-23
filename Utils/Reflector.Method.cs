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
        // 方法
        //------------------------------------------------
        /// <summary>获取类的所有公共方法（包括祖先的）。注意 Type.GetMethods()只能获取当前类下的方法。</summary>
        /// <param name="searchAncestors">是否检索祖先的同名方法</param>
        public static List<MethodInfo> GetMethods(this Type type, string name, bool searchAncestors = true)
        {
            var methods = new List<MethodInfo>();

            // 遍历到父节点，寻找指定方法
            var t = type;
            var ms = new List<MethodInfo>();
            while (t != null)
            {
                ms = t.GetMethods().Where(m => m.Name == name).ToList();
                methods.AddRange(ms);
                if (!searchAncestors)
                    break;
                t = t.BaseType;
            }
            return methods;
        }

        /// <summary>获取类的公共方法（包括祖先的），若有重名取第一个。</summary>
        public static MethodInfo GetMethod(this Type type, string name, bool searchAncestors)
        {
            return GetMethods(type, name, searchAncestors).FirstOrDefault();
        }


        /// <summary>获取当前方法信息</summary>
        public static MethodInfo GetCurrentMethod()
        {
            return new System.Diagnostics.StackTrace().GetFrame(1).GetMethod() as MethodInfo;
        }

        /// <summary>获取当前类信息</summary>
        public static Type GetCurrentType()
        {
            return new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
        }

        /*
        /// <summary>获取方法名（失败）</summary>
        public static string GetMethodName<T>(Expression<Func<T, Delegate>> expr)
        {
            var name = "";
            if (expr.Body is MethodCallExpression)
                name = ((MethodCallExpression)expr.Body).Method.Name;
            return name;
        }
        */


    }
}