using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;

namespace App.Wechats.OP
{
    public class SendTMessageReply : WechatReply
    {
        public string msgid { get; set; }
    }


    public partial class WechatOP
    {

        //------------------------------------------------------------------------------
        // 微信公众号模板消息
        //------------------------------------------------------------------------------
        /// <summary>发送微信模板消息</summary>
        /// <remarks>
        /// 在公众号》功能》添加功能插件中添加并维护消息模板
        /// 接口文档
        ///     https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1433751277
        ///     当前每个账号的模板消息的日调用上限为10万次，单个模板没有特殊限制。【2014年11月18日将接口调用频率从默认的日1万次提升为日10万次，可在MP登录后的开发者中心查看】。
        ///     当账号粉丝数超过10W/100W/1000W时，模板消息的日调用上限会相应提升，以公众号MP后台开发者中心页面中标明的数字为准。
        /// 介绍
        ///     模版消息接口让公众号可以向用户发送预设的模版消息。
        ///     模版消息仅用于公众号向用户发送业务通知。如信用卡刷卡通知，商品购买成功通知等。
        ///     模版消息只对认证的服务号开放。
        ///     模版消息在微信客户端的展示如右图所示
        /// 使用规则
        ///     请根据运营规范使用模版消息，否则可能会被停止内测资格甚至封号惩罚
        ///     请勿使用模版发送垃圾广告或造成骚扰
        ///     请勿使用模版发送营销类消息
        ///     请在符合模版要求的场景时发送模版
        /// </remarks>
        public static SendTMessageReply SendTMessage(string openId, TMessage msg)
        {
            var token = GetAccessTokenFromServer();
            var u = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", GetAccessTokenFromServer());
            var json = new
            {
                touser = openId,
                template_id = msg.TemplateId,
                url = msg.Url,
                data = new
                {
                    first = new { value = msg.first.value, color = msg.first.color },
                    remark = new { value = msg.remark.value, color = msg.remark.color },
                    keyword1 = new { value = msg.keyword1.value, color = msg.keyword1.color },
                    keyword2 = new { value = msg.keyword2.value, color = msg.keyword2.color },
                    keyword3 = new { value = msg.keyword3.value, color = msg.keyword3.color },
                    keyword4 = new { value = msg.keyword4.value, color = msg.keyword4.color },
                    keyword5 = new { value = msg.keyword5.value, color = msg.keyword5.color },
                    keyword6 = new { value = msg.keyword6.value, color = msg.keyword6.color }
                }
            }.ToJson();
            var reply = HttpHelper.Post(u, json);
            return reply.ParseJson<SendTMessageReply>();
        }
    }
}
