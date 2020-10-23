using System;
using System.Collections.Generic;
using System.Linq;
using App.Wechats.Utils;


/*
公众号测试工具：https://mp.weixin.qq.com/debug/
测试账号申请：https://mp.weixin.qq.com/debug/cgi-bin/sandbox?t=sandbox/login
OpenID：是加密后的微信号，每个用户对每个公众号的OpenID是唯一的。对于不同公众号，同一用户的openid不同
UnionID: 同一个微信开放平台帐号下的移动应用、网站应用和公众帐号，用户的unionid是唯一的
IP白名单
    https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140183
    公众号调用接口时，请登录“微信公众平台-开发-基本配置”提前将服务器IP地址添加到IP白名单中，否则将无法调用成功。
    小程序无需配置IP白名单。
*/
/// <summary>
/// 微信公众号
/// </summary>
namespace App.Wechats.OP
{
    /// <summary>访问“OAuthGetToken”接口的反馈</summary>
    public class OAuthGetTokenReply : WechatReply
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string openid { get; set; }
        public string scope { get; set; }
    }

    /// <summary>JSSdk签名</summary>
    public class JsSdkSignature
    {
        public string AppId { get; set; }
        public string Timestamp { get; set; }
        public string NonceStr { get; set; }
        public string Signature { get; set; }
        public string Ticket { get; set; }
    }

    /// <summary>微信用户集合（批量获取用户信息接口返回）</summary>
    public class WechatUsers : WechatReply
    {
        public List<WechatUser> user_info_list { get; set; }
    }

    /// <summary>访问“获取用户列表”接口的反馈</summary>
    public class GetUsersReply : WechatReply
    {
        public int total { get; set; }
        public int count { get; set; }
        public UserIds data { get; set; }
        public string next_openid { get; set; }

        public class UserIds
        {
            public List<string> openid { get; set; }
        }
    }

    /// <summary>创建公众号二维码接口的反馈</summary>
    public class CreateQrCodeReply : WechatReply
    {
        public string ticket { get; set; }
        public int expire_seconds { get; set; }
        public string url { get; set; }
    }


    /// <summary>
    /// 微信公众号辅助类库（需配置IP白名单才可正常运行）
    /// </summary>
    public partial class WechatOP : Wechat
    {
        //------------------------------------------------------------------------------
        // 微信公众号网页授权登录开发
        // 1、引导用户进入授权页面同意授权，获取code
        // 2、通过code换取网页授权access_token（与基础支持中的access_token不同）
        // 3、如果需要，开发者可以刷新网页授权access_token，避免过期
        // 4、通过网页授权access_token和openid获取用户基本信息（支持UnionID机制）
        //------------------------------------------------------------------------------
        /*
        /// <summary>获取公众号签名信息(TODO: 用 GetJsSdkSignature 替代）</summary>
        public static JsSdkUiPackage GetJsSdkUiPackage(string url)
        {
            url = url.Split(',')[0];
            try
            {
                return JSSDKHelper.GetJsSdkUiPackage(WechatConfig.WebAppId, WechatConfig.WebAppSecret, url);
            }
            catch
            {
                return new JsSdkUiPackage("", "", "", "");
            }
        }
        */

        /// <summary>获取JSSDK 签名，供网页客户端进行微信 JS 初始化用（未测试）</summary>
        /// <remarks>
        /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421141115
        /// wx.config({
        ///     debug: false, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
        ///     appId: '', // 必填，公众号的唯一标识
        ///     timestamp: '', // 必填，生成签名的时间戳
        ///     nonceStr: '', // 必填，生成签名的随机串
        ///     signature: '',// 必填，签名
        ///     jsApiList: ['checkJsApi', 'openLocation', 'getLocation','chooseWXPay'] // 必填，需要使用的JS接口列表
        /// });
        /// </remarks>
        public static JsSdkSignature GetJsSdkSignature(string url)
        {
            var appId = WechatConfig.OPAppId;
            var accessToken = GetAccessTokenFromServer();
            var ticket = GetJsSdkTicket(accessToken);
            var timestamp = DateTime.Now.ToTimeStamp();
            var nonceStr = Guid.NewGuid().ToString().MD5();
            var signature = CalcJsSdkSignature(ticket, nonceStr, timestamp, url);
            return new JsSdkSignature() { AppId = appId, Timestamp = timestamp, NonceStr = nonceStr, Signature = signature, Ticket = ticket };
        }

        /// <summary>获取公众号调用微信JS接口的临时票据(有效期7200秒，开发者必须在自己的服务全局缓存jsapi_ticket)（未测试)</summary>
        static string GetJsSdkTicket(string accessToken)
        {
            var dt = DateTime.Now.AddMinutes(60);
            return IO.GetCache<string>("WechatJSAPITicket", () =>
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", accessToken);
                var txt = HttpHelper.Get(url);
                return txt.ParseJObject()["ticket"].ToString();
            }, dt);
        }

        /// <smmary>获取JS-SDK权限验证的签名Signature</summary>
        static string CalcJsSdkSignature(string ticket, string noncestr, string timestamp, string url)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("jsapi_ticket", ticket);
            dict.Add("noncestr", noncestr);
            dict.Add("timestamp", timestamp);
            dict.Add("url", url);
            var txt = BuildSortQueryString(dict);
            return txt.SHA1();
        }

        /// <summary>获取微信网页授权登录信息</summary>
        public static WechatUser OAuthGetUserInfo(string code)
        {
            var reply = OAuthGetAccessToken(code);
            return OAuthGetUserInfo(reply.access_token, reply.openid);
        }


        /// <summary>获取微信网页授权用户信息</summary>
        /// <remarks>
        /// 网页授权作用域为snsapi_userinfo，则此时开发者可以通过access_token和openid拉取用户信息了
        /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140842
        /// </remarks>
        public static WechatUser OAuthGetUserInfo(string accessToken, string openId)
        {
            var url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", accessToken, openId);
            var reply = HttpHelper.Get(url);
            var user = reply.ParseJson<WechatUser>();
            user.opId = user.openid;
            return user;
        }


        /// <summary>获取微信网页授权登录AccessToken</summary>
        /// <remarks>
        /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140842
        /// 网页授权流程分为四步：
        /// 1、引导用户进入授权页面同意授权，获取code
        /// 2、通过code换取网页授权access_token（与基础支持中的access_token不同）
        /// 3、如果需要，开发者可以刷新网页授权access_token，避免过期
        /// 4、通过网页授权access_token和openid获取用户基本信息（支持UnionID机制）
        /// </remarks>
        public static OAuthGetTokenReply OAuthGetAccessToken(string code)
        {
            var key = "WechatWebAccessToken";
            var dt = DateTime.Now.AddMinutes(60);
            return IO.GetCache<OAuthGetTokenReply>(key, () =>
                {
                   var url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
                       WechatConfig.OPAppId, WechatConfig.OPAppSecret, code
                       );
                   var reply = HttpHelper.Get(url);
                   return reply.ParseJson<OAuthGetTokenReply>();
               }, dt);
        }



        /// <summary>批量获取用户信息</summary>
        /// <remarks>
        /// https://api.weixin.qq.com/cgi-bin/user/info/batchget?access_token=ACCESS_TOKEN
        /// POST数据示例
        /// {
        ///     "user_list": [
        ///         {"openid": "otvxTs4dckWG7imySrJd6jSi0CWE", "lang": "zh-CN"}, 
        ///         {"openid": "otvxTs_JZ6SEiP0imdhpi50fuSZg", "lang": "zh-CN"}
        ///     ]
        /// }
        /// </remarks>
        public static WechatUsers OAuthGetUserInfos(List<string> openIds)
        {
            var accessToken = GetAccessTokenFromServer();
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info/batchget?access_token={0}", accessToken);
            var json = new { user_list = openIds.Cast(t => new { openid = t, lang = "zh-CN" }) }.ToJson();
            var reply = HttpHelper.Post(url, json);
            var users = reply.ParseJson<WechatUsers>();
            users.user_info_list.ForEach(t => t.opId = t.openid);
            return users;
        }


        //------------------------------------------------------------------------------
        // 微信公众号后台开发
        //------------------------------------------------------------------------------
        /// <summary>获取访问Token（从Token服务器获取）</summary>
        public static string GetAccessTokenFromServer(bool refresh = false)
        {
            var url = string.Format(WechatConfig.OPTokenServer, refresh);
            return HttpHelper.Get(url);
        }


        /// <summary>获取已关注公众号的用户详细信息</summary>
        /// <remarks>
        /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140839
        /// </remarks>
        public static WechatUser GetUserInfo(string openId)
        {
            var accessToken = GetAccessTokenFromServer();
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}", accessToken, openId);
            var reply = HttpHelper.Get(url);
            var user = reply.ParseJson<WechatUser>();
            user.opId = user.openid;
            return user;
        }

        /// <summary>获取关注用户列表</summary>
        public static GetUsersReply GetUserInfos(string startOpenId)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/user/get?access_token={0}&next_openid=", GetAccessTokenFromServer(), startOpenId);
            var reply = HttpHelper.Get(url);
            return reply.ParseJson<GetUsersReply>();
        }





        //------------------------------------------------------------------------------
        // QrCode （推广支持）
        // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1443433542
        // 公众平台提供了生成带参数二维码的接口。使用该接口可以获得多个带不同场景值的二维码，用户扫描后，公众号可以接收到事件推送。
        // 目前有2种类型的二维码：
        // - 临时二维码，是有过期时间的，最长可以设置为在二维码生成后的30天（即2592000秒）后过期，但能够生成较多数量。临时二维码主要用于帐号绑定等不要求二维码永久保存的业务场景
        // - 永久二维码，是无过期时间的，但数量较少（目前为最多10万个）。永久二维码主要用于适用于帐号绑定、用户来源统计等场景。
        // 用户扫描带场景值二维码时，可能推送以下两种事件：
        // - 如果用户还未关注公众号，则用户可以关注公众号，关注后微信会将带场景值关注事件推送给开发者。
        // - 如果用户已经关注公众号，在用户扫描后会自动进入会话，微信也会将带场景值扫描事件推送给开发者。
        //------------------------------------------------------------------------------
        /// <summary>创建微信公众号二维码（永久）</summary>
        /// <param name="text">场景文本</param>
        public static string CreateQrCode(string text)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", GetAccessTokenFromServer());
            var data = new {
                action_name ="QR_LIMIT_STR_SCENE",
                action_info = new { scene = new {scene_str = text}}
            };
            var reply = HttpHelper.Post(url, data.ToJson());
            var o = reply.ParseJson<CreateQrCodeReply>();
            return string.Format("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}", o.ticket.UrlEncode());
        }


    }
}