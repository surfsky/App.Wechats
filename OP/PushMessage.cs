using App.Wechats.Utils;
using System;
using System.Collections.Generic;

namespace App.Wechats.OP
{
    /// <summary>
    /// 微信公众号消息处理
    /// 负责推送签名校验(GET) 及推送消息处理(POST XML)
    /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421135319
    /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140454
    /// 公众平台官网的开发-基本设置页面，勾选协议成为开发者，点击“修改配置”按钮，填写
    /// - URL是开发者用来接收微信消息和事件的接口URL。
    /// - Token可由开发者可以任意填写，用作生成签名（该Token会和接口URL中包含的Token进行比对，从而验证安全性）。
    /// - EncodingAESKey由开发者手动填写或随机生成，将用作消息体加解密密钥。
    /// 该URL负责处理
    /// （1）微信服务器将发送GET请求到填写的服务器地址URL上，做签名校验。此时必须认证通过。
    /// （2）当普通微信用户向公众账号发消息时，微信服务器将POST消息的XML数据包到开发者填写的URL上，服务器可解析这些信息并做处理
    /// 示例
    /// https://www.bearmanager.cn/WeiXin/WEBService.ashx?signature=a7dc992b9be9cf583c325753e86b180029be3192&echostr=7154795684745412365&timestamp=1554724621&nonce=1885473029
    /// </summary>
    /*
    公众号消息示例
	text
		<xml><ToUserName><![CDATA[gh_ae1207a05405]]></ToUserName>
		<FromUserName><![CDATA[oaL9vxImyL4JKm6Xobz-rYx4XVIE]]></FromUserName>
		<CreateTime>1555166378</CreateTime>
		<MsgType><![CDATA[text]]></MsgType>
		<Content><![CDATA[手机]]></Content>
		<MsgId>22264625617127345</MsgId>
		</xml>
	scan
		<xml><ToUserName><![CDATA[gh_ae1207a05405]]></ToUserName>
		<FromUserName><![CDATA[oaL9vxImyL4JKm6Xobz-rYx4XVIE]]></FromUserName>
		<CreateTime>1555168420</CreateTime>
		<MsgType><![CDATA[event]]></MsgType>
		<Event><![CDATA[SCAN]]></Event>
		<EventKey><![CDATA[/pages/index/index?inviteStoreId=35]]></EventKey>
		<Ticket><![CDATA[gQHE8TwAAAAAAAAAAS5odHRwOi8vd2VpeGluLnFxLmNvbS9xLzAyZ1RKWUFRRV9kTDQxMDAwME0wN3UAAgRBhrBcAwQAAAAA]]></Ticket>
		</xml>     
    */

    /// <summary>微信公众号推送消息类型</summary>
    public enum PushMessageType
    {
        Text,
        Image,
        Voice,
        Video,
        ShortVideo,
        Link,
        Location,
        Event
    }

    /// <summary>微信公众号推送消息事件类型</summary>
    public enum PushEventType
    {
        Subscribe,
        Unsubscribe,
        Scan,
        Location,
        Click,
        View,
        view_miniprogram,       // 查看小程序
        TemplateSendJobFinish   // 模板详细发送成功
    }

    /// <summary>微信公众号推送消息</summary>
    /// <remarks>https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140453</remarks>
    public class PushMessage
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string CreateTime { get; set; }
        public string MsgId { get; set; }

        // text
        public string Content { get; set; }

        // image
        public string PicUrl { get; set; }

        // voice（语音识别需要在公众号>接口权限中打开）
        public string MediaId { get; set; }
        public string Format { get; set; }
        public string Recognition { get; set; }

        // video & ShortVideo
        public string ThumbMediaId { get; set; }

        // location
        public string Longitude { get; set; }  // 经度
        public string Latitude { get; set; }   // 纬度
        public string Precision { get; set; }  // 精度
        public string Scale { get; set; }
        public string Label { get; set; }

        // link
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }

        // event
        public string EventKey { get; set; }
        public string Ticket { get; set; }


        public PushMessageType? MsgType { get; set; }
        public PushEventType? Event { get; set; }
        public DateTime? CreateDt { get { return this.CreateTime.ParseTimeStamp(); } }

        //--------------------------------------------
        // 回复消息
        // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140543
        //--------------------------------------------
        /// <summary>回复转接客户服务</summary>
        public string ReplyTransferCustomerService()
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "transfer_customer_service"
            }.ToXml("xml");
        }

        /// <summary>回复跳转消息（微信官方不支持）</summary>
        public string ReplyMiniProgram(string appId, string pagePath)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "event",
                Event = "view_miniprogram",
                EventKey = pagePath
            }.ToXml("xml");
        }

        /// <summary>回复文本消息</summary>
        public string ReplyText(string text)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "text",
                Content = text
            }.ToXml("xml");
        }

        /// <summary>回复图片消息</summary>
        public string ReplyImage(string mediaId)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "image",
                Image = new
                {
                    MediaId = mediaId
                }
            }.ToXml("xml");
        }

        /// <summary>回复语音消息</summary>
        public string ReplyVoice(string mediaId)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "voice",
                Voice = new
                {
                    MediaId = mediaId
                }
            }.ToXml("xml");
        }

        /// <summary>回复视频消息</summary>
        public string ReplyVideo(string mediaId, string thumbMediaId)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "video",
                Video = new
                {
                    MediaId = mediaId,
                    ThumbMediaId = thumbMediaId,
                }
            }.ToXml("xml");
        }

        /// <summary>回复音乐消息</summary>
        /// <param name="musicUrl">音乐链接</param>
        /// <param name="hqMusicUrl">高质量音乐链接，WIFI环境优先使用该链接播放音乐</param>
        /// <param name="thumbMediaId">缩略图的媒体id，通过上传多媒体文件，得到的id</param>
        public string ReplyMusic(string title, string description, string musicUrl, string hqMusicUrl, string thumbMediaId)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "music",
                Music = new
                {
                    Title = title,
                    Description = description,
                    MusicUrl = musicUrl,
                    HQMusicUrl = hqMusicUrl,
                    ThumbMediaId = thumbMediaId,
                }
            }.ToXml("xml");
        }

        /// <summary>回复单条新闻</summary>
        /// <param name="picUrl">图片链接，支持JPG、PNG格式，较好的效果为大图360*200，小图200*200</param>
        public string ReplyNews(string title, string description, string picUrl, string url)
        {
            var items = new List<item>()
            {
                new item(title, description, picUrl, url)
            };
            return ReplyNews(items);
        }

        /// <summary>回复多条新闻</summary>
        public string ReplyNews(List<item> items)
        {
            return new
            {
                ToUserName = this.FromUserName,
                FromUserName = this.ToUserName,
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "news",
                ArticleCount = items.Count,
                Articles = items
            }.ToXml("xml");
        }

        /// <summary>公众号新闻</summary>
        public class item
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string PicUrl { get; set; }
            public string Url { get; set; }
            public item (string title,string description, string picUrl, string url)
            {
                this.Title = title;
                this.Description = description;
                this.PicUrl = picUrl;
                this.Url = url;
            }
        }
    }



}


