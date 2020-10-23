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
        // 特性
        //------------------------------------------------
        /// <summary>获取指定特性列表（支持Type、Property、Method等）</summary>
        public static List<T> GetAttributes<T>(this MemberInfo m) where T : Attribute
        {
            // 这种方法会获取包含基类的Attribute，不合适
            // return (T[])m.GetCustomAttributes(typeof(T), true).ToList();  
            // 准确获取完全一致的  Attribute，不包含基类
            return m.GetCustomAttributes()
                .Where(t => t.GetType() == typeof(T))
                .Select(t => t as T)
                .ToList()
                ;
        }

        /// <summary>获取指定特性（不抛出异常）</summary>
        public static T GetAttribute<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetAttributes<T>().FirstOrDefault();
        }

        // <summary>动态设置对象属性的标题（可用于 PropertyGrid 展示）</summary>
        //public static void SetDisplayName(this object o, string propertyName, string title)
        //{
        //    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(o)[propertyName];
        //    Type t = typeof(MemberDescriptor);
        //    var field = t.GetField("displayName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance);  // netcore fail, return null
        //    field.SetValue(descriptor, title);
        //}
        //
        //<summary>获取对象属性的展示名</summary>
        //public static string GetDisplayName(this object o, string propertyName)
        //{
        //    return TypeDescriptor.GetProperties(o)[propertyName].DisplayName;
        //}



        //------------------------------------------------
        // 事件
        //------------------------------------------------
        /// <summary>获取事件调用者列表</summary>
        public static List<Delegate> GetEventSubscribers(object o, string eventName)
        {
            var type = o.GetType();
            var field = type.GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            if (field != null)
            {
                var d = (Delegate)field.GetValue(o);
                var delegates = d.GetInvocationList();
                return delegates.ToList();
            }
            return new List<Delegate>();
        }




        //------------------------------------------------
        // 辅助
        //------------------------------------------------
        /// <summary>组合各个对象的属性，输出为字典</summary>
        public static Dictionary<string, object> CombineObject(params object[] objs)
        {
            var dict = new Dictionary<string, object>();
            foreach (object o in objs)
            {
                if (o == null)
                    continue;
                foreach (PropertyInfo pi in o.GetType().GetProperties())
                    dict[pi.Name] = pi.GetValue(o);
            }
            return dict;
        }
    }
}