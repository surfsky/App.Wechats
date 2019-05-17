using System;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;
using App.Core;

/// <summary>
/// 微信小程序
/// </summary>
namespace App.Wechats.MP
{
    /// <summary>由JSCode2Session解析出来的小程序Session、openid、unionid数据</summary>
    public class MPSession : WechatReply
    {
        public string openid { get; set; }
        public string unionid { get; set; }
        public string session_key { get; set; }
    }

    /// <summary>wx.getUserInfo() 返回的数据结构</summary>
    public class GetUserInfoReply : WechatReply
    {
        public string rawData { get; set; }
        public string iv { get; set; }
        public string signature { get; set; }
        public string encryptedData { get; set; }
    }

    /// <summary>从用户加密数据解析出来的用户数据结构</summary>
    public class MPUser
    {
        public string nickName { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string avatarUrl { get; set; }  // 头像地址
        public string openId { get; set; }
        public string unionId { get; set; }
    }



    /// <summary>
    /// 微信小程序辅助类库
    /// </summary>
    /// <remarks>
    /// 微信小程序后端 API
    ///     https://developers.weixin.qq.com/miniprogram/dev/framework/server-ability/backend-api.html
    ///     小程序还提供了一系列在后端服务器使用 HTTPS 请求调用的 API，帮助开发者在后台完成各类数据分析、管理和查询等操作。
    ///     如 getAccessToken，code2Session 等。
    ///     access_token 是小程序全局唯一后台接口调用凭据，调用绝大多数后台接口时都需使用。
    ///     开发者应在后端服务器使用getAccessToken获取 access_token，并调用相关 API
    ///     小程序无需配置IP白名单。公众号要配置IP白名单。
    /// 请求参数说明
    ///     对于 GET 请求，请求参数应以 QueryString 的形式写在 URL 中。
    ///     对于 POST 请求，部分参数需以 QueryString 的形式写在 URL 中（一般只有 access_token，如有额外参数会在文档里的 URL 中体现），其他参数如无特殊说明均以 JSON 字符串格式写在 POST 请求的 body 中。
    /// 返回参数说明
    ///     注意：当API调用成功时，部分接口不会返回 errcode 和 errmsg，只有调用失败时才会返回。  
    /// </remarks>
    public class WechatMP : Wechat
    {
        //------------------------------------------------
        // 微信小程序登录相关接口(Session, UnionId 等）
        //------------------------------------------------
        /// <summary>获取访问Token（从Token服务器获取）</summary>
        public static string GetAccessTokenFromServer(bool refresh = false)
        {
            var url = string.Format(WechatConfig.MPTokenServer, refresh);
            return HttpHelper.Get(url);
        }

        /// <summary>登录凭证校验。获取小程序登录账户的OpenId、UnionId、SessionKey。</summary>
        /// <param name="code">小程序调用 wx.login() 后可获取</param>
        /// <remarks>
        /// JSCode2Session（后端API）
        /// 登录凭证校验。
        /// 调试：小程序开发工具通过 wx.login() 接口获得临时登录凭证 code 后传到开发者服务器调用此接口完成登录流程。
        /// 参考：https://developers.weixin.qq.com/miniprogram/dev/api-backend/auth.code2Session.html
        /// 返回值
        ///     openid      string 用户唯一标识
        ///     unionid string 用户在开放平台的唯一标识符，在满足 UnionID 下发条件的情况下会返回，详见 UnionID 机制说明。
        ///     session_key string 会话密钥
        ///     errcode number 错误码
        /// errmsg      string 错误信息
        /// unionId 获得规则
        ///     https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/union-id.html
        ///    如果开发者帐号下存在同主体的公众号，并且该用户已经关注了该公众号。开发者可以直接通过 wx.login + code2Session 获取到该用户 UnionID，无须用户再次授权
        ///    用户在小程序中支付完成后5分钟内，开发者可以直接通过getPaidUnionId接口获取该用户的 UnionID，无需用户授权。
        /// </remarks>
        public static MPSession JSCode2Session(string code)
        {
            var url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, WechatConfig.MPAppId, WechatConfig.MPAppSecret, code);
            var reply = HttpHelper.Get(url);
            return reply.ParseJson<MPSession>();
        }

        /// <summary>获取用户详细信息（解析 wx.getUserInfo 返回的加密数据）</summary>
        /// <remarks>
        /// https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/signature.html
        /// wx.getUserInfo()的数据结构如：
        /// {
        ///    "encryptedData" : "W2+0ELqrgVbH98UZPdYFNhv6tfXYUlfDpRTG23oWtyldSAFZIJ0m3GRJfqaSwDRT01KdV9tLtfStN0x1SMFRl0amT2NtCeUbeKI6L0RPcTzCE2MtNULKb3eoUWzd26OH7gy9afTGQ4cWE31J3isbabufm29wb+wJI8f4gIbdFS+jYzr7NeG7mE5Rnh+gRSCvy9/ewFAHuVO3N6aC5hXErqgT1LkET8ZV8NT8tFwfB7/4okHPbH7VhxR7U9TDVUQJdpOXGmor4YZKhnbzZML0qwVDpA99W7IkJb/yfqMB/Oa44lx2j9Z1Npmvf1yW6yRONKgwrfk5JBsx693dqlF9X3llToCclYY4zuBGdPeY7jCxNsDnXs1EVTJyG1COB+eBtcVskzw6hAEx7gDy0glvsK3/aG112noAJMcepHJufhpjQxlyl6Nx9EBecO6OsUFKkdrqaKGRgOPXYGswDohwI96ZMMGGar5z7lJV92TH82/QgtJUzo72nISHViNalVdVjC1MO5CNcUDPVp2Vz0v68FfaM9915Xc2RQ5UOQQzPEw=",
        ///    "errMsg" : "getUserInfo:ok",
        ///    "iv" : "jc/xof3WDnVAOmVW5OjB+A==",
        ///    "rawData" : "{\"nickName\":\"梁益鑫\",\"gender\":1,\"language\":\"zh_CN\",\"city\":\"Wenzhou\",\"province\":\"Zhejiang\",\"country\":\"China\",\"avatarUrl\":\"https://wx.qlogo.cn/mmopen/vi_32/Q0j4TwGTfTJmO5r4Cx9rO2SS3AR6bAnZKdtNU5TDXBk5tibjQBPhibWPMHxasUP9ba2cib7dibgicyP4M9y97pbuXxQ/132\"}",
        ///    "signature" : "940d55d51b6e01c789a60ca2ada6bb197dd12450",
        ///    "userInfo" : {
        ///       "avatarUrl" : "https://wx.qlogo.cn/mmopen/vi_32/Q0j4TwGTfTJmO5r4Cx9rO2SS3AR6bAnZKdtNU5TDXBk5tibjQBPhibWPMHxasUP9ba2cib7dibgicyP4M9y97pbuXxQ/132",
        ///       "city" : "Wenzhou",
        ///       "country" : "China",
        ///       "gender" : 1,
        ///       "language" : "zh_CN",
        ///       "nickName" : "梁益鑫",
        ///       "province" : "Zhejiang"
        ///    }
        /// }
        /// encryptedData 数据解密后结构如：
        /// {
        ///   "nickName": "NICKNAME",
        ///   "gender": GENDER,
        ///   "city": "CITY",
        ///   "province": "PROVINCE",
        ///   "country": "COUNTRY",
        ///   "avatarUrl": "AVATARURL",
        ///   "openId": "OPENID",
        ///   "unionId": "UNIONID",
        ///   "watermark": {
        ///     "appid": "APPID",
        ///     "timestamp": TIMESTAMP
        ///     }
        /// }
        /// </remarks>
        public static MPUser DecryptUserInfo(string getUserInfoReply, string sessionKey)
        {
            try
            {
                //Logger.LogDb("DecrytUserInfo-Start", getUserInfoReply, "", LogLevel.Debug);
                var reply = getUserInfoReply.ParseJson<GetUserInfoReply>();

                // 比对签名
                var sign = CalcSignature(reply.rawData, sessionKey);
                if (sign != reply.signature)
                {
                    //Logger.LogDb("DecrytUserInfo-CheckSignFail", "signature check fail: " + sign, "", LogLevel.Debug);
                    return null;
                }
                //Logger.LogDb("DecrytUserInfo-CheckSignOk", "signature check ok", "", LogLevel.Debug);

                // AES 解密
                var txt = AESDecrypt(reply.encryptedData, sessionKey, reply.iv);
                //Logger.LogDb("DecrytUserInfo-Ok", txt, "", LogLevel.Debug);
                return txt.ParseJson<MPUser>();
            }
            catch (Exception ex)
            {
                //Logger.LogDb("DecrytUserInfo-Fail", ex.Message, "", LogLevel.Debug);
                return null;
            }
        }

        /// <summary>校验签名是否正确</summary>
        /// <remarks>https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/signature.html</remarks>
        static string CalcSignature(string rawData, string sessionKey)
        {
            return (rawData + sessionKey).ToSHA1();
        }
        

        /// <summary>
        /// 微信小程序加密数据解密算法
        /// https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/signature.html
        /// (1) 对称解密使用的算法为 AES-128-CBC，数据采用PKCS#7填充。
        /// (2) 对称解密的目标密文为 Base64_Decode(encryptedData)。
        /// (3) 对称解密秘钥 aeskey = Base64_Decode(session_key), aeskey 是16字节。
        /// (4) 对称解密算法初始向量 为Base64_Decode(iv)，其中iv由数据接口返回。
        /// </summary>
        static string AESDecrypt(string encryptedText, string key, string iv)
        {
            // 设置 cipher 格式 AES-128-CBC
            var aes = new AesCryptoServiceProvider();
            aes.KeySize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            // 解密
            var decryptor = aes.CreateDecryptor();
            byte[] raw = Convert.FromBase64String(encryptedText);
            byte[] result = decryptor.TransformFinalBlock(raw, 0, raw.Length);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>用户支付完成后五分钟内，获取该用户的 UnionId，无需用户授权。</summary>
        /// <remarks>
        /// GetPaidUnionId （后端API）
        /// 用户支付完成后五分钟内，获取该用户的 UnionId，无需用户授权。
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/auth.getPaidUnionId.html
        ///     GET https://api.weixin.qq.com/wxa/getpaidunionid?access_token=ACCESS_TOKEN&openid=OPENID
        /// 返回的 JSON 数据包
        ///     unionid string 用户唯一标识，调用成功后返回
        ///    errcode number 错误码
        ///     errmsg string 错误信息
        /// </remarks>
        public static string GetPaidUnionId(string openId)
        {
            var token = GetAccessTokenFromServer();
            var url = string.Format("https://api.weixin.qq.com/wxa/getpaidunionid?access_token={0}&openid={1}", token, openId);
            var txt = HttpHelper.Get(url);
            var o = txt.ParseJObject();
            return o.GetValue("openid").ToText();
        }



        //------------------------------------------------
        // 微信小程序模板消息
        //------------------------------------------------
        /// <summary>发送微信小程序模板消息</summary>
        /// <remarks>
        /// 发送模板消息
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/templateMessage.send.html
        ///     https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/template-message.html
        ///     页面的<form/> 组件，属性 report-submit 为 true 时，可以声明为需要发送模板消息，此时点击按钮提交表单可以获取 formId，用于发送模板消息。或者当用户完成 支付行为，可以获取 prepay_id 用于发送模板消息。
        ///     当用户在小程序内完成过支付行为，可允许开发者向用户在7天内推送有限条数的模板消息（1次支付可下发3条，多次支付下发条数独立，互相不影响）
        ///     当用户在小程序内发生过提交表单行为且该表单声明为要发模板消息的，开发者需要向用户提供服务时，可允许开发者向用户在7天内推送有限条数的模板消息（1次提交表单可下发1条，多次提交下发条数独立，相互不影响）
        ///     模板消息统一显示在微信的“服务通知”里面，点击后可跳到对应的小程序页面。
        /// 请求
        ///     POST https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token=ACCESS_TOKEN
        ///     {
        ///       "touser": "OPENID",
        ///       "template_id": "TEMPLATE_ID",
        ///       "page": "index",
        ///       "form_id": "FORMID",
        ///       "data": {
        ///           "keyword1": {"value": "339208499"},
        ///           "keyword2": {"value": "2015年01月05日 12:30"},
        ///           "keyword3": {"value": "粤海喜来登酒店"} ,
        ///           "keyword4": {"value": "广州市天河区天河路208号"}
        ///       },
        ///       "emphasis_keyword": "keyword1.DATA"
        ///     }
        ///  返回的 JSON 数据包
        ///      errcode number 错误码
        ///      errmsg string 错误信息
        /// </remarks>
        public static WechatReply SendTMessage(string openId, TMessage msg, string formId)
        {
            if (formId.IsEmpty())
                return null;

            // 构造json
            var o = new
            {
                touser = openId,
                template_id = msg.TemplateId,
                page = msg.Url,
                form_id = formId,
                data = new
                {
                    keyword1 = new { value = msg.keyword1.value },
                    keyword2 = new { value = msg.keyword2.value },
                    keyword3 = new { value = msg.keyword3.value },
                    keyword4 = new { value = msg.keyword4.value },
                    keyword5 = new { value = msg.keyword5.value },
                    keyword6 = new { value = msg.keyword6.value }
                }
            }.ToJson();

            // 发送
            var token = GetAccessTokenFromServer();
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token={0}", token);
            var val = HttpHelper.Post(url, o.ToBytes());
            return val.ParseJson<WechatReply>();
        }


        /// <summary>同时发送公众号和小程序消息</summary>
        /// <remarks>
        /// 本接口在服务器端调用
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/uniformMessage.send.html
        ///     POST https://api.weixin.qq.com/cgi-bin/message/wxopen/template/uniform_send?access_token=ACCESS_TOKEN
        /// 请求参数
        ///     access_token        string 是   接口调用凭证
        ///     touser              string 是   用户openid，可以是小程序的openid，也可以是mp_template_msg.appid对应的公众号的openid
        ///     weapp_template_msg  Object 否   小程序模板消息相关的信息，可以参考小程序模板消息接口; 有此节点则优先发送小程序模板消息
        ///     mp_template_msg     Object 是   公众号模板消息相关的信息，可以参考公众号模板消息接口；有此节点并且没有weapp_template_msg节点时，发送公众号模板消息
        /// weapp_template_msg 的结构
        ///     template_id         string 是   小程序模板ID
        ///     page                string 是   小程序页面路径
        ///     form_id             string 是   小程序模板消息formid
        ///     data                string 是   小程序模板数据
        ///     emphasis_keyword    string 是   小程序模板放大关键词
        /// mp_template_msg 的结构
        ///     appid               string 是   公众号appid，要求与小程序有绑定且同主体
        ///     template_id         string 是   公众号模板id
        ///     url                 string 是   公众号模板消息所要跳转的url
        ///     miniprogram         string 是   公众号模板消息所要跳转的小程序，小程序的必须与公众号具有绑定关系
        ///     data                string 是   公众号模板消息的数据
        /// 返回的 JSON 数据包
        ///     errcode             number 错误码
        ///     errmsg              string 错误信息
        /// </remarks>
        public static WechatReply SendUniformMessage(string openId, TMessage webMsg, TMessage mpMsg, string mpFormId)
        {
            var mpAppId = WechatConfig.MPAppId;
            var webAppId = WechatConfig.OpenAppId;
            var token = GetAccessTokenFromServer();

            // 准备数据
            var msg1 = new
            {
                appid = webAppId,
                template_id = webMsg.TemplateId,
                url = webMsg.Url,
                miniprogram = new { appid = mpAppId, pagepath = mpMsg.Url },
                data = new
                {
                    first    = new { value = webMsg.first.value,    color = webMsg.first.color },
                    remark   = new { value = webMsg.remark.value,   color = webMsg.remark.color },
                    keyword1 = new { value = webMsg.keyword1.value, color = webMsg.keyword1.color },
                    keyword2 = new { value = webMsg.keyword2.value, color = webMsg.keyword2.color },
                    keyword3 = new { value = webMsg.keyword3.value, color = webMsg.keyword3.color },
                    keyword4 = new { value = webMsg.keyword4.value, color = webMsg.keyword4.color },
                    keyword5 = new { value = webMsg.keyword5.value, color = webMsg.keyword5.color },
                    keyword6 = new { value = webMsg.keyword6.value, color = webMsg.keyword6.color },
                }
            };
            var msg2 = new
            {
                template_id = mpMsg.TemplateId,
                page = mpMsg.Url,
                form_id = mpFormId,
                data = new
                {
                    keyword1 = new { value = mpMsg.keyword1.value },
                    keyword2 = new { value = mpMsg.keyword2.value },
                    keyword3 = new { value = mpMsg.keyword3.value },
                    keyword4 = new { value = mpMsg.keyword4.value },
                    keyword5 = new { value = mpMsg.keyword5.value },
                    keyword6 = new { value = mpMsg.keyword6.value }
                }
            };

            var o = new
            {
                touser = openId,
                weapp_template_msg = msg1,
                mp_template_msg = msg2
            }.ToJson();

            //
            var u = string.Format("https://api.weixin.qq.com/cgi-bin/message/wxopen/template/uniform_send?access_token={0}", token);
            var val = HttpHelper.Post(u, o.ToBytes());
            return val.ParseJson<WechatReply>();
        }


        //------------------------------------------------
        // 微信小程序二维码
        //------------------------------------------------
        /// <summary>获取小程序二维码</summary>
        /// <remarks>
        /// wxacode.createQRCode
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/wxacode.createQRCode.html
        ///     POST https://api.weixin.qq.com/cgi-bin/wxaapp/createwxaqrcode?access_token=ACCESS_TOKEN
        ///     获取小程序二维码，适用于需要的码数量较少的业务场景。
        ///     通过该接口生成的小程序码，永久有效，有数量限制(生成的码数量限制为 100,000)
        ///     参数
        ///         path    string 是   扫码进入的小程序页面路径，最大长度 128 字节，不能为空；对于小游戏，可以只传入 query 部分，来实现传参效果，如：传入 "?foo=bar"，即可在 wx.getLaunchOptionsSync 接口中的 query 参数获取到 { foo: "bar"}。
        ///         width number	430	否 二维码的宽度，单位 px。最小 280px，最大 1280px
        ///     如果调用成功，会直接返回图片二进制内容，如果请求失败，会返回 JSON 格式的数据。
        ///         errcode number  错误码
        ///         errmsg  string 错误信息
        /// wxacode.get
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/wxacode.get.html
        ///     POST https://api.weixin.qq.com/wxa/getwxacode?access_token=ACCESS_TOKEN
        ///     适用于需要的码数量较少的业务场景。通过该接口生成的小程序码，永久有效，有数量限制
        /// wxacode.getUnlimited
        ///     https://developers.weixin.qq.com/miniprogram/dev/api-backend/wxacode.getUnlimited.html
        ///     POST https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=ACCESS_TOKEN
        ///     获取小程序码，适用于需要的码数量极多的业务场景。通过该接口生成的小程序码，永久有效，数量暂无限制
        /// </remarks>
        public static Image GetQrCode(string path, int width)
        {
            var token = GetAccessTokenFromServer();
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/wxaapp/createwxaqrcode?access_token={0}", token);
            var o = new { path = path, width = width };
            var response = HttpHelper.Post(url, o.ToJson().ToStream(), "application/json");
            if (response.ContentType.Contains("image"))
                return response.ToImage();
            else
                throw new Exception(response.ToText());
        }
    }
}