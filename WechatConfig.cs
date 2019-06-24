using App.Core;
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
        [UI("公众号")] Open,
        [UI("小程序")] MP
    }

    /// <summary>
    /// 微信开发账户配置资源（请在Web.Config 或 app.config 中进行设置）
    /// </summary>
    public class WechatConfig
    {
        //---------------------------------------------
        // 配置信息
        //---------------------------------------------
        // 公众号配置信息
        public static  string OPTokenServer   = ConfigurationManager.AppSettings["WechatOPTokenServer"]; // 微信公众号Token服务器地址，如： /HttpApi/Wechat/GetAccessToken?type=Web&refresh={0}&securityCode={1}
        public static  string OPAppId         = ConfigurationManager.AppSettings["WechatOPAppID"];             // 微信公众号AppId   
        public static  string OPAppSecret     = ConfigurationManager.AppSettings["WechatOPAppSecret"];     // 微信公众号AppSecret 
        public static  string OPPushToken     = ConfigurationManager.AppSettings["WechatOPPushToken"];     // 微信公众号推送消息Token
        public static  string OPPushKey       = ConfigurationManager.AppSettings["WechatOPPushKey"];         // 微信公众号推送消息Key
        public static  string OPPayUrl        = ConfigurationManager.AppSettings["WechatOPPayUrl"];           // 微信公众号支付成功回调地址

        // 小程序配置信息
        public static  string MPTokenServer   = ConfigurationManager.AppSettings["WechatMPTokenServer"];     // 微信小程序Token服务器地址，如：/HttpApi/Wechat/GetAccessToken?type=MP&refresh={0}&securityCode={1}
        public static  string MPAppId         = ConfigurationManager.AppSettings["WechatMPAppID"];                 // 微信小程序AppID
        public static  string MPAppSecret     = ConfigurationManager.AppSettings["WechatMPAppSecret"];         // 微信小程序AppSecret
        public static  string MPPushToken     = ConfigurationManager.AppSettings["WechatMPPushToken"];         // 微信小程序消息推送Token
        public static  string MPPushKey       = ConfigurationManager.AppSettings["WechatMPPushKey"];             // 微信小程序消息推送Key
        public static  string MPPayUrl        = ConfigurationManager.AppSettings["WechatMPPayUrl"];               // 微信小程序支付成功回调地址

        // 商户信息
        public static  string MchId = ConfigurationManager.AppSettings["WechatMchId"];                     // 微信商户号
        public static  string MchKey = ConfigurationManager.AppSettings["WechatMchKey"];                   // 商户平台设置的密钥key


        //---------------------------------------------
        // 辅助方法
        //---------------------------------------------
        public static string GetAppId(WechatAppType type)
        {
            return (type == WechatAppType.Open) ? WechatConfig.OPAppId : WechatConfig.MPAppId;
        }
        public static string GetAppSecret(WechatAppType type)
        {
            return (type == WechatAppType.Open) ? WechatConfig.OPAppSecret : WechatConfig.MPAppSecret;
        }
        public static string GetPayUrl(WechatAppType type)
        {
            return (type == WechatAppType.Open) ? WechatConfig.OPPayUrl : WechatConfig.MPPayUrl;
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
