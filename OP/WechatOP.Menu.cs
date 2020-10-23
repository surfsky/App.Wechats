using App.Wechats.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;


namespace App.Wechats.OP
{
    /// <summary>微信菜单项类型</summary>
    public enum WechatMenuType
    {
        click,                  // 用户点击后，微信服务器会通过消息接口(event类型)推送点击事件给开发者，并且带上按钮中开发者填写的key值，开发者可以通过自定义的key值进行消息回复。
        view,                   // 接跳转到开发者指定的url中
        miniprogram,            // 小程序
        scancode_push,          // 
        scancode_waitmsg,       // 
        pic_sysphoto,           // 
        pic_photo_or_album,     // 
        pic_weixin,             // 
        location_select,        // 
        media_id,               // 
        view_limited            // 
    }

    /// <summary>微信菜单</summary>
    /// <remarks>
    /// 微信菜单客户端有缓存，需要24小时微信客户端才会展现出来。建议测试时可以尝试取消关注公众账号后，再次关注，则可以看到创建后的效果
    /// </remarks>
    public class WechatMenu
    {
        public List<WechatMenuItem> button { get; set; } = new List<WechatMenuItem>();
    }

    /// <summary>微信菜单项</summary>
    public class WechatMenuItem
    {
        public WechatMenuType type { get; set; }

        // 超链接
        public string name { get; set; }
        public string url { get; set; }

        // 各种事件
        public string key { get; set; }

        // 小程序
        public string appid { get; set; }
        public string pagepath { get; set; }

        // 查看媒体
        public string media_id { get; set; }

        // 子按钮
        public List<WechatMenuItem> sub_button { get; set; } = new List<WechatMenuItem>();

        //
        public WechatMenuItem() { }
        protected WechatMenuItem(WechatMenuType type, string name, string url = null, string key = null, string appId = null, string pagePath = null, string mediaId=null)
        {
            this.name = name;
            this.type = type;
            this.url = url;
            this.key = key;
            this.appid = appId;
            this.pagepath = pagePath;
            this.media_id = mediaId;
        }
        public static WechatMenuItem Root(string name, params WechatMenuItem[] items)           {return new WechatMenuItem() { name = name, sub_button = items.ToList() };}
        public static WechatMenuItem View(string name, string url)                              {return new WechatMenuItem(WechatMenuType.view, name, url);}
        public static WechatMenuItem Click(string name, string key)                             {return new WechatMenuItem(WechatMenuType.click, name, null, key, null);}
        public static WechatMenuItem ScanWait(string name, string key)                          {return new WechatMenuItem(WechatMenuType.scancode_waitmsg, name, null, key);}
        public static WechatMenuItem ScanPush(string name, string key)                          {return new WechatMenuItem(WechatMenuType.scancode_push, name, null, key);}
        public static WechatMenuItem Pic1(string name, string key)                              {return new WechatMenuItem(WechatMenuType.pic_sysphoto, name, null, key);}
        public static WechatMenuItem Pic2(string name, string key)                              {return new WechatMenuItem(WechatMenuType.pic_photo_or_album, name, null, key);}
        public static WechatMenuItem Pic3(string name, string key)                              {return new WechatMenuItem(WechatMenuType.pic_weixin, name, null, key);}
        public static WechatMenuItem Location(string name, string key)                          {return new WechatMenuItem(WechatMenuType.location_select, name, null, key);}
        public static WechatMenuItem Media1(string name, string mediaId)                        {return new WechatMenuItem(WechatMenuType.media_id, name, null, null, null, null, mediaId);}
        public static WechatMenuItem Media2(string name, string mediaId)                        {return new WechatMenuItem(WechatMenuType.view_limited, name, null, null, null, null, mediaId);}
        public static WechatMenuItem MP(string name, string appId, string pagePath, string url) {return new WechatMenuItem(WechatMenuType.miniprogram, name, url, null, appId, pagePath); }
    }


    /// <summary>
    /// </summary>
    public partial class WechatOP
    {
        /// <summary>设置微信公众号菜单</summary>
        /// <remarks>https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421141013</remarks>
        /// <example>https://www.cnblogs.com/mchina/p/3276878.html</example>
        public static WechatReply SetMenu(WechatMenu menu)
        {
            var json = menu.ToJson();
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}", WechatOP.GetAccessTokenFromServer());
            return HttpHelper.Post(url, json, Encoding.UTF8, "application/json").ParseJson<WechatReply>();
        }

        /// <summary>获取微信公众号菜单</summary>
        /// <remarks>https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421141014</remarks>
        public static WechatMenu GetMenu()
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}", WechatOP.GetAccessTokenFromServer());
            return HttpHelper.Get(url).ParseJson<WechatMenu>();
        }

        /// <summary>删除微信公众号菜单</summary>
        /// <remarks>https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421141015</remarks>
        public static WechatReply DeleteMenu()
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/delete?access_token={0}", WechatOP.GetAccessTokenFromServer());
            return HttpHelper.Get(url).ParseJson<WechatReply>();
        }
    }
}