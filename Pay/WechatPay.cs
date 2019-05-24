using System;
using System.Collections.Generic;
using System.Xml;
using App.Core;

namespace App.Wechats.Pay
{
    /// <summary>微信统一订单接口回复</summary>
    public class UnifiedOrderReply
    {
        public string appid { get; set; }
        public string mch_id { get; set; }
        public string sub_appid { get; set; }
        public string sub_mch_id { get; set; }
        public string nonce_str { get; set; }
        public string sign { get; set; }
        public string result_code { get; set; }
        public string device_info { get; set; }
        public string trade_type { get; set; }
        public string prepay_id { get; set; }
        public string code_url { get; set; }
        public string mweb_url { get; set; }

        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string err_code { get; set; }
        public string err_code_des { get; set; }
    }

    /// <summary>微信支付回应基类</summary>
    public class PayReply
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string err_code { get; set; }
        public string err_code_des { get; set; }

        /// <summary>该信息是否返回成功标志</summary>
        public bool IsSuccess { get { return return_code == "SUCCESS"; } }
    }

    /// <summary>
    /// 微信支付回调信息
    /// 微信小程序支付回调：https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=9_7
    /// 微信 HTML 支付回调：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_4
    /// </summary>
    public class PayCallback : PayReply
    {
        // 商户及订单信息
        public string appid { get; set; }
        public string mch_id { get; set; }
        public string openid { get; set; }
        public string device_info { get; set; }
        public string is_subscribe { get; set; }
        public string trade_type { get; set; }
        public string transaction_id { get; set; }
        public string out_trade_no { get; set; }

        // 加密信息
        public string nonce_str { get; set; }
        public string sign { get; set; }
        public string sign_type { get; set; }

        // 总金额（订单金额、应支付金额、货币种类）
        public int total_fee { get; set; }
        public int settlement_total_fee { get; set; }
        public string fee_type { get; set; }
        public string bank_type { get; set; }

        // 现金
        public int cash_fee { get; set; }
        public string cash_fee_type { get; set; }


        // 代金券（未完成）
        public int coupon_fee { get; set; }
        public int coupon_count { get; set; }

        // 其它
        public string attach { get; set; }
        public string time_end { get; set; }
    }


    /// <summary>
    /// 微信公众号和小程序公共代码-支付相关
    /// </summary>
    public class WechatPay
    {
        /// <summary>
        /// 微信支付-预支付订单（可本地调试）
        /// 商户在小程序或网页中先调用该接口在微信支付服务后台生成预支付交易单，返回正确的预支付交易后调起支付。</summary>
        /// <remarks>
        /// 支付流程
        /// 小程序：https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=7_4&index=3
        /// 微信内网页 https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_4
        /// 
        /// 统一下单接口
        /// 小程序：https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=9_1
        /// 微信内网页：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_1
        /// 内容是一样的
        /// </remarks>
        public static UnifiedOrderReply UnifiedOrder(
            string appId, string appSecret, string payUrl, 
            string body, double fee, string openId, string orderNo, string ip, string deviceInfo)
        {
            var mchId     = WechatConfig.MchId;
            var mchKey    = WechatConfig.MchKey;

            // 构建参数
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            var nonceStr = BuildNonceStr();
            var tradeType = "JSAPI";
            var dict = new Dictionary<string, string>();
            dict.Add("appid", appId);
            dict.Add("body", body);
            dict.Add("mch_id", mchId);
            dict.Add("nonce_str", nonceStr);
            dict.Add("notify_url", payUrl);
            dict.Add("openid", openId);
            dict.Add("out_trade_no", orderNo);
            dict.Add("spbill_create_ip", ip);
            dict.Add("total_fee", Convert.ToInt32(fee * 100).ToString());
            dict.Add("trade_type", tradeType);
            var sign = BuildPaySign(dict, mchKey);
            dict.Add("sign", sign);
            //Logger.LogDb("WechatUnifiedOrder-dict", dict.ToJson());

            // 发送
            var xml = dict.ToXml("xml");
            var back = HttpHelper.Post(url, xml);
            var info = string.Format("微信统一支付：req=\"{0}\"; \r\nresp=\"{1}\"", xml, back);
            //Logger.LogDb("WechatUnifiedOrder", info, openId, LogLevel.Debug);
            return back.ParseXml<UnifiedOrderReply>();
        }


        /// <summary>构建一次性随机字符串</summary>
        public static string BuildNonceStr()
        {
            return Guid.NewGuid().ToString("N").SubText(32);
        }


        /// <summary>小程序构建支付签名（供客户端参考，可考虑将客户端签名迁移到服务器端）</summary>
        /// <remarks>
        /// https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=7_7&index=3
        /// wx.requestPayment(
        /// {
        ///     'timeStamp': '',
        ///     'nonceStr': '',
        ///     'package': '',
        ///     'signType': 'MD5',
        ///     'paySign': '',
        ///     'success':function(res) { },
        ///     'fail':function(res) { },
        ///     'complete':function(res) { }
        /// })
        /// </remarks>
        static string BuildPaySign(string appId, string prepayId, string nonceStr, DateTime dt)
        {
            string timeStamp = dt.ToTimeStamp();
            string package = string.Format("prepay_id={0}", prepayId);
            var dict = new Dictionary<string, string>();
            dict.Add("appId", appId);
            dict.Add("timeStamp", timeStamp);
            dict.Add("nonceStr", nonceStr);
            dict.Add("package", package);
            dict.Add("signType", "MD5");
            return BuildPaySign(dict, WechatConfig.MchKey);
        }


        /// <summary>构造微信支付签名</summary>
        /// <remarks>https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=4_3</remarks>
        public static string BuildPaySign(Dictionary<string, string> dict, string mchKey)
        {
            var txt = Wechat.BuildSortQueryString(dict) + "&key=" + mchKey;
            return txt.ToMD5().ToUpper();
        }

        /// <summary>校验支付 XML 是否正确</summary>
        public static bool CheckPaySign(string xml, string mchKey)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            var sign = "";
            var dict = new Dictionary<string, string>();
            foreach (XmlNode node in root.ChildNodes)
            {
                var name = node.Name;
                var value = node.InnerText;
                if (name == "sign")
                {
                    sign = value;
                    continue;
                }
                dict.Add(name, value);
            }
            var sign2 = BuildPaySign(dict, mchKey);
            return (sign == sign2);
        }
    }
}
