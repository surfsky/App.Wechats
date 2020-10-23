using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 资源
    /// </summary>
    internal static class ResHelp
    {
        /// <summary>获取资源文本</summary>
        /// <remarks>请配置 AppCoreConfig.UseGlobal 和 ResType 属性</remarks>
        public static string GetResText(this string resName)
        {
            bool useGlobal = UtilConfig.Instance.UseGlobal;
            if (useGlobal)
                return GetResText(resName, UtilConfig.Instance.ResType);
            return resName;
        }

        /// <summary>获取资源文本</summary>
        /// <param name="resType">资源类。如 App.Properties.Resouce</param>
        public static string GetResText(this string resName, Type resType)
        {
            if (resType != null)
                return new ResourceManager(resType).GetString(resName);
            return resName;
        }

        /// <summary>获取资源图片</summary>
        public static Image GetResImage(this string resName, Type resType)
        {
            return new ResourceManager(resType).GetObject(resName) as Image;
        }

        /// <summary>获取资源文件</summary>
        public static byte[] GetResFile(this string resName, Type resType)
        {
            return new ResourceManager(resType).GetObject(resName) as byte[];
        }
    }
}
