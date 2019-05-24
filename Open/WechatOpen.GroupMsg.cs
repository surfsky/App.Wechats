using App.Core;
using System.Text;

namespace App.Wechats.Open
{

    /// <summary>发送群组消息的反馈</summary>
    public class SendGroupNewsReply : WechatReply
    {
        public int msg_id { get; set; }
        public int msg_data_id { get; set; }
    }

    /// <summary>
    /// 群发消息相关
    /// 参考：https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1481187827_i0l21
    /// </summary>
    public partial class WechatOpen
    {
        /// <summary>群发新闻消息（图文）</summary>
        public static SendGroupNewsReply SendGroupNews(int tagId, string mediaId)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/sendall?access_token=ACCESS_TOKEN{0}", GetAccessTokenFromServer());
            var data = new
            {
                filter = new { is_to_all = false, tag_id = tagId },
                mpnews = new { media_id = mediaId },
                msgtype = "mpnews",
                send_ignore_reprint = 0
            };
            var reply = HttpHelper.Post(url, data.ToJson(), Encoding.UTF8, "application/json");
            return reply.ParseJson<SendGroupNewsReply>();
        }
    }
}