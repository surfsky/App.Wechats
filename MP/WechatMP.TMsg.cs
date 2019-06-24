using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;

namespace App.Wechats.MP
{
    public partial class WechatMP
    {
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
            var opAppId = WechatConfig.OPAppId;
            var token = GetAccessTokenFromServer();

            // 准备数据
            var msg1 = new
            {
                appid = opAppId,
                template_id = webMsg.TemplateId,
                url = webMsg.Url,
                miniprogram = new { appid = mpAppId, pagepath = mpMsg.Url },
                data = new
                {
                    first = new { value = webMsg.first.value, color = webMsg.first.color },
                    remark = new { value = webMsg.remark.value, color = webMsg.remark.color },
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


    }
}
