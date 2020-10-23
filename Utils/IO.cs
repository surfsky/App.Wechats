using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace App.Wechats.Utils
{
    /// <summary>
    /// IO 辅助方法（文件、路径、程序集）
    /// </summary>
    internal static partial class IO
    {
        
        //------------------------------------------------
        // 程序集
        //------------------------------------------------
        /// <summary>获取主入口数据集版本号</summary>
        public static Version AssemblyVersion
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        /// <summary>获取主入口数据集路径</summary>
        public static string AssemblyPath
        {
            get { return Assembly.GetEntryAssembly().Location; }
        }

        /// <summary>获取调用者数据集目录</summary>
        public static string AssemblyDirectory
        {
            get { return new FileInfo(AssemblyPath).DirectoryName; }
        }

        /// <summary>获取某个类型归属的程序集版本号</summary>
        public static Version GetVersion(Type type)
        {
            return type.Assembly.GetName().Version;
        }

        //------------------------------------------------
        // 输出
        //------------------------------------------------
        /// <summary>打印到调试窗口</summary>
        public static void Trace(string format, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(Util.GetText(format, args));
        }


        /// <summary>打印到控制台窗口</summary>
        public static void Console(string format, params object[] args)
        {
            System.Console.WriteLine(Util.GetText(format, args));
        }

        /// <summary>打印到调试窗口</summary>
        public static void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(Util.GetText(format, args));
        }

        /// <summary>打印到所有输出窗口</summary>
        public static void Write(string format, params object[] args)
        {
            Trace(format, args);
            Console(format, args);
            //Debug(format, args);
        }

        

        //------------------------------------------------------------
        // 配置相关 *.config>AppSetting
        //------------------------------------------------------------
        /// <summary>从 .config 文件中获取配置信息</summary>
        public static T GetAppSetting<T>(string key)
        {
            var txt = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            return txt.Parse<T>();
        }

        //------------------------------------------------------------
        // 缓存相关
        //------------------------------------------------------------
        /// <summary>清除缓存对象</summary>
        public static void RemoveCache(string key)
        {
            var cache = HttpRuntime.Cache;
            if (cache[key] != null)
            {
                cache.Remove(key);
                System.Diagnostics.Debug.WriteLine("Clear cache : " + key);
            }
        }

        /// <summary>设置缓存对象</summary>
        public static void SetCache<T>(string key, T value, DateTime? expiredTime=null) where T : class
        {
            if (value != null)
            {
                expiredTime = expiredTime ?? Cache.NoAbsoluteExpiration;
                var cache = HttpRuntime.Cache;
                cache.Insert(key, value, null, expiredTime.Value, Cache.NoSlidingExpiration);
                System.Diagnostics.Debug.WriteLine("Create cache : " + key);
            }
        }

        /// <summary>获取缓存对象（缓存到期后会清空，再次请求时会自动获取）</summary>
        /// <param name="creator">创建方法。若该方法返回值为null，不会加入缓存。</param>
        public static T GetCache<T>(string key, Func<T> creator=null, DateTime? expiredTime=null) where T : class
        {
            expiredTime = expiredTime ?? Cache.NoAbsoluteExpiration;
            var cache = HttpRuntime.Cache;    // 可在非Web环境使用
            if (creator == null)
                return cache[key] as T;
            else
            {
                //if (!cache.ContainsKey(key))
                if (cache[key] == null)
                {
                    T o = creator();
                    SetCache(key, o, expiredTime.Value);
                }
                return cache[key] as T;
            }
        }

        /// <summary>判断缓存是否具有某个键值（遍历方式，性能会差一些）</summary>
        /// <remarks>Cache 类并未提供 ContainsKey 方法</remarks>
        public static bool ContainsKey(this Cache cache, object key)
        {
            foreach (DictionaryEntry item in cache)
            {
                if (item.Key == key)
                    return true;
            }
            return false;
        }

        //------------------------------------------------------------
        // 缓存
        //------------------------------------------------------------
        static FreeDictionary<string, object> _dict = new FreeDictionary<string, object>();
        /// <summary>获取缓存字典对象（数据在运行期间不会被清理，且可以容纳空值）</summary>
        public static T GetDict<T>(string key, Func<T> creator = null) where T : class
        {
            if (creator == null)
                return _dict[key] as T;
            else
            {
                if (!_dict.ContainsKey(key))
                {
                    T o = creator();
                    _dict[key] = o;
                }
                return _dict[key] as T;
            }
        }

    }
}
