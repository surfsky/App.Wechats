using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Text;
using App.Core;

namespace App.Wechats
{
    /// <summary>微信接口反馈基类（兼容公众号和小程序）</summary>
    public class WechatReply
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
    }

    /// <summary>获取访问Token方法的应答（兼容公众号和小程序）</summary>
    public class GetAccessTokenReply : WechatReply
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public DateTime FetchDt { get; set; }
        public DateTime ExpireDt { get; set; }

        public void CalcExpireDt(DateTime fetchDt)
        {
            this.FetchDt = fetchDt;
            this.ExpireDt = fetchDt.AddSeconds(this.expires_in).AddMinutes(-10);  // 提早10分钟去刷新
        }
    }


    /// <summary>微信用户信息（兼容公众号和小程序）</summary>
    public class WechatUser : WechatReply
    {
        // 附加信息
        public string mpId { get; set; }            // 小程序用户ID
        public string mpSessionKey { get; set; }    // 小程序用户SessionKey
        public string opId { get; set; }            // 公众号用户ID
        public string opSessionKey { get; set; }    // 公众号用户SessionKey

        // 订阅信息
        public int subscribe { get; set; }          // 是否订阅该公众号
        public int subscribe_time { get; set; }     // 订阅时间
        public string subscribe_scene { get; set; } // 订阅渠道
        public int qr_scene { get; set; }           // 二维码扫码场景（开发者自定义）
        public string qr_scene_str { get; set; }    // 二维码扫码场景描述（开发者自定义）

        // 用户信息
        public string openid { get; set; }
        public string unionid { get; set; }
        public string nickname { get; set; }
        public int sex { get; set; }                // 值为1时是男性，值为2时是女性，值为0时是未知
        public string language { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string headimgurl { get; set; }
        public string remark { get; set; }
        public int groupid { get; set; }
        public List<int> tagid_list { get; set; }
        public List<string> privilege { get; set; }

    }

    /// <summary>微信模板消息（兼容公众号和小程序）</summary>
    public class TMessage
    {
        /// <summary>模板消息ID（与模板消息内容紧密相关，就不分离开了）</summary>
        public string TemplateId { get; set; }

        /// <summary>点击消息后的跳转地址</summary>
        public string Url { get; set; }


        public TMessageItem first { get; set; }
        public TMessageItem remark { get; set; }
        public TMessageItem keyword1 { get; set; }
        public TMessageItem keyword2 { get; set; }
        public TMessageItem keyword3 { get; set; }
        public TMessageItem keyword4 { get; set; }
        public TMessageItem keyword5 { get; set; }
        public TMessageItem keyword6 { get; set; }

        public TMessage(
            string templateId, string url, 
            string first="", string remark="", 
            string keyword1="", string keyword2="", string keyword3="", string keyword4="", string keyword5="", string keyword6="")
        {
            this.TemplateId = templateId;
            this.Url = url;

            this.first = new TMessageItem(first);
            this.remark = new TMessageItem(remark);
            this.keyword1 = new TMessageItem(keyword1);
            this.keyword2 = new TMessageItem(keyword2);
            this.keyword3 = new TMessageItem(keyword3);
            this.keyword4 = new TMessageItem(keyword4);
            this.keyword5 = new TMessageItem(keyword5);
            this.keyword6 = new TMessageItem(keyword6);
        }

        /// <summary>微信模板消息子项</summary>
        public class TMessageItem
        {
            public string value { get; set; }
            public string color { get; set; }
            public TMessageItem(string value, string color = "#173177")
            {
                this.value = value;
                this.color = color;
            }
        }
    }



    /// <summary>微信公众号和小程序公共代码</summary>
    public partial class Wechat
    {

        /*************************************************
        GetAccessToken（后端API）
        获取后台接口调用凭证。AccessToken 是小程序全局唯一后台接口调用凭据，调用绝大多数后台接口时都需使用
        参考 
            小程序获取Token文档：https://developers.weixin.qq.com/miniprogram/dev/api-backend/auth.getAccessToken.html
            公众号获取Token文档：https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140183
            访问方法一致，公众号要求配置IP白名单，小程序无需配置。
        返回值：
            access_token   string    获取到的凭证
            expires_in	   number    凭证有效时间，单位：秒。目前是7200秒之内的值。
            errcode	       number    错误码
            errmsg	       string    错误信息
        access_token 的存储与更新
            access_token 的存储至少要保留 512 个字符空间；
            access_token 的有效期目前为 2 个小时，需定时刷新，重复获取将导致上次获取的 access_token 失效；
            access_token 的有效期通过返回的 expire_in 来传达，目前是7200秒之内的值，需要根据这个有效时间提前去刷新。
        建议开发者使用中控服务器统一获取和刷新 access_token
            其他业务逻辑服务器所使用的 access_token 均来自于该中控服务器，不应该各自去刷新，否则容易造成冲突，导致 access_token 覆盖而影响业务；
            在刷新过程中，中控服务器可对外继续输出的老 access_token，此时公众平台后台会保证在5分钟内，新老 access_token 都可用，这保证了第三方业务的平滑过渡；
            access_token 的有效时间可能会在未来有调整，所以中控服务器不仅需要内部定时主动刷新
            还需要提供被动刷新 access_token 的接口，这样便于业务服务器在API调用获知 access_token 已超时的情况下，可以触发 access_token 的刷新流程。
        *************************************************/
        /// <summary>获取访问Token（带缓存机制）</summary>
        /// <param name="refresh">是否强制刷新，从微信官方服务器重新生成Token</param>
        public static string GetAccessToken(WechatAppType type, bool refresh=false)
        {
            GetAccessTokenReply data;
            var key = "WechatAccessToken" + type.ToString();
            var cache = HttpContext.Current.Cache;
            if (refresh || cache[key] == null)
            {
                var appId     = WechatConfig.GetAppId(type);
                var appSecret = WechatConfig.GetAppSecret(type);
                data = GetAccessTokenInternal(appId, appSecret);
                cache.Insert(key, data, null, data.ExpireDt, Cache.NoSlidingExpiration);
            }
            data = cache[key] as GetAccessTokenReply;
            return data.access_token;
        }

        /// <summary>获取访问Token（公众号和小程序通用）</summary>
        private static GetAccessTokenReply GetAccessTokenInternal(string appId, string appSecret)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appId, appSecret);
            var reply = HttpHelper.Get(url);
            var o = reply.ParseJson<GetAccessTokenReply>();
            o.CalcExpireDt(DateTime.Now);
            if (o.errcode != 0)
                throw new Exception(o.errmsg);
            return o;
        }


        /// <summary>计算推送消息签名字符串（公众号和小程序通用）</summary>
        public static string CalcPushMessageSign(string timestamp, string nonce, string token)
        {
            var arr = new[] { token, timestamp, nonce }.OrderBy(t => t).ToArray();
            var arrString = string.Join("", arr);
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString().ToLower();
        }


        /// <summary>构建排序后的查询字符串（公众号和小程序通用）</summary>
        public static string BuildSortQueryString(Dictionary<string, string> dict)
        {
            // (1) 参数排序
            var items = dict.OrderBy(t => t.Key).ToList();

            // (2) 拼装成查询字符串（如果参数的值为空不参与签名）
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (item.Value.IsNotEmpty())
                    sb.AppendFormat("{0}={1}&", item.Key, item.Value);
            }
            return sb.ToString().TrimEnd('&');
        }
    }

}