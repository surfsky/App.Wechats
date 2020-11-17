using App.Wechats.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats
{
    /// <summary>
    /// 微信开发账号类型（公众号、小程序等）
    /// </summary>
    public enum WechatAppType : int
    {
        [UI("公众号")] OP,
        [UI("小程序")] MP
    }

    /// <summary>
    /// 微信开发账户配置资源
    /// </summary>
    public class WechatConfig
    {
        //---------------------------------------------
        // 配置信息
        //---------------------------------------------
        //
        // 公众号配置信息
        //
        /// <summary>微信公众号Token服务器地址，如： /HttpApi/Wechat/GetAccessToken?type=OP&refresh={0}&token={1}</summary>
        public static string OPTokenServer;
        /// <summary>微信公众号 AppId</summary>
        public static  string OPAppId;
        /// <summary>微信公众号 AppSecret</summary>
        public static  string OPAppSecret;
        /// <summary>微信公众号推送消息 Token</summary>
        public static  string OPPushToken;
        /// <summary>微信公众号推送消息 Key</summary>
        public static  string OPPushKey;
        /// <summary>微信公众号支付成功回调地址, 如：/Pages/Wechats/pay.ashx。需要在微信支付平台上配置支付目录包含该支付路径。</summary>
        public static  string OPPayUrl;

        //
        // 小程序配置信息
        //
        /// <summary>微信小程序Token服务器地址，如：/HttpApi/Wechat/GetAccessToken?type=MP&refresh={0}&token={1}</summary>
        public static  string MPTokenServer; 
        /// <summary>微信小程序AppID</summary>
        public static  string MPAppId;
        /// <summary>微信小程序AppSecret</summary>
        public static  string MPAppSecret;
        /// <summary>微信小程序消息推送Token</summary>
        public static  string MPPushToken;
        /// <summary>微信小程序消息推送Key</summary>
        public static  string MPPushKey;
        /// <summary>微信小程序支付成功回调地址, 如：/Pages/Wechats/pay.ashx</summary>
        public static  string MPPayUrl;

        //
        // 商户信息
        //
        /// <summary>微信商户号。如：</summary>
        public static  string MchId;
        /// <summary>微信商户平台 API 密钥</summary>
        public static  string MchApiKey;


        //---------------------------------------------
        // 辅助方法
        //---------------------------------------------
        public static string GetAppId(WechatAppType type)
        {
            return (type == WechatAppType.OP) ? WechatConfig.OPAppId : WechatConfig.MPAppId;
        }
        public static string GetAppSecret(WechatAppType type)
        {
            return (type == WechatAppType.OP) ? WechatConfig.OPAppSecret : WechatConfig.MPAppSecret;
        }
        public static string GetPayUrl(WechatAppType type)
        {
            return (type == WechatAppType.OP) ? WechatConfig.OPPayUrl : WechatConfig.MPPayUrl;
        }


        //---------------------------------------------
        // 单例
        //---------------------------------------------
        private static WechatConfig _instance = null;
        public static WechatConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WechatConfig();
                return _instance;
            }
        }


        //---------------------------------------------
        // 事件
        //---------------------------------------------
        public delegate void LogHandler(string name, string user, string request, string reply);

        /// <summary>日志事件</summary>
        /// <example>WechatConfig.Instance.OnLog += ....;</example>
        public event LogHandler OnLog;

        /// <summary>记录日志</summary>
        public static void Log(string name, string user="", string request="", string reply="")
        {
            try
            {
                if (Instance.OnLog != null)
                    Instance.OnLog(name, user, request, reply);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
