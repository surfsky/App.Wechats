using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;

namespace App.Wechats.Open
{
    /// <summary>用户标签</summary>
    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; }
        /// <summary>粉丝数</summary>
        public int count { get; set; }
    }

    public class CreateTagReply : WechatReply
    {
        public Tag tag { get; set; }
    }
    public class GetTagsReply : WechatReply
    {
        public List<Tag> tags { get; set; }
    }
    public class GetUserTagsReply : WechatReply
    {
        public List<int> tagid_list { get; set; }
    }
    public class GetUsersByTagReply : WechatReply
    {
        public int count { get; set; }
        public string next_openid { get; set; }
        public GetUsersByTagData data { get; set; }

        public class GetUsersByTagData
        {
            public List<string> openid { get; set; }
        }
    }

    //------------------------------------------------------------------------------
    // 用户标签管理
    // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140837
    //------------------------------------------------------------------------------
    public partial class WechatOpen
    {
        /*
        POST: { "tag":{ "name" : "广东"} }
        返回：{ "tag":{ "id":134, "name":"广东"   } }
        */
        /// <summary>创建标签(一个公众号，最多可以创建100个标签。)</summary>
        /// <remarks>https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140837</remarks>
        public static CreateTagReply CreateTag(string name)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/create?access_token={0}", GetAccessTokenFromServer());
            var data = new { tag = new { name = name } };
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<CreateTagReply>();
        }


        /// <summary>获取公众号已创建的标签</summary>
        public static GetTagsReply GetTags()
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/get?access_token={0}", GetAccessTokenFromServer());
            var reply = HttpHelper.Get(url);
            return reply.ParseJson<GetTagsReply>();
        }

        /// <summary>修改标签</summary>
        public static WechatReply UpdateTag(int id, string name)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/update?access_token={0}", GetAccessTokenFromServer());
            var data = new { tag = new { id = id, name = name } };
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<WechatReply>();
        }

        /// <summary>删除标签</summary>
        /// <remarks>当某个标签下的粉丝超过10w时，后台不可直接删除标签。此时，开发者可以对该标签下的openid列表，先进行取消标签的操作，直到粉丝数不超过10w后，才可直接删除该标签。</remarks>
        public static WechatReply RemoveTag(int id)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/delete?access_token={0}", GetAccessTokenFromServer());
            var data = new { tag = new { id = id } };
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<WechatReply>();
        }

        /// <summary>获取用户身上的标签ID列表</summary>
        public static GetUserTagsReply GetUserTags(string openId)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/getidlist?access_token={0}", GetAccessTokenFromServer());
            var data = new { openid = openId };
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<GetUserTagsReply>();
        }


        /// <summary>获取具有某个标签的用户列表</summary>
        public static GetUsersByTagReply GetUsersByTag(int tagId)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/user/tag/get?access_token={0}", GetAccessTokenFromServer());
            var data = new { tagid = tagId,   next_openid = ""};
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<GetUsersByTagReply>();
        }


        /// <summary>为用户打标签</summary>
        public static WechatReply SetUserTag(List<string> openIds, int tagId)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/tags/members/batchtagging?access_token={0}", GetAccessTokenFromServer());
            var data = new {openid_list = openIds, tagid = tagId};
            var reply = HttpHelper.Post(url, data.ToJson());
            return reply.ParseJson<WechatReply>();
        }

        //--------------------------------------------
        // 复合逻辑
        //--------------------------------------------
        /// <summary>设置微信公众号用户标签</summary>
        public static Tag TrySetUserTag(string openId, string tagName)
        {
            var tag = WechatOpen.TryGetTag(tagName);
            if (tag != null)
                WechatOpen.TrySetUserTag(openId, tag);
            return tag;
        }

        /// <summary>尝试获取或创建公众号标签</summary>
        static Tag TryGetTag(string tagName)
        {
            // 获取公众号已有的标签
            var tags = WechatOpen.GetTags();
            var tag = tags.tags.Find(t => t.name == tagName);
            if (tag == null)
            {
                // 如果不存在该标签，则新建
                var reply = WechatOpen.CreateTag(tagName);
                if (reply.errcode == 0)
                {
                    tag = new Tag();
                    tag.id = reply.tag.id;
                    tag.name = reply.tag.name;
                }
            }
            return tag;
        }

        /// <summary>尝试设置用户身上的标签</summary>
        static bool TrySetUserTag(string openId, Tag tag)
        {
            // 获取用户身上的标签
            var userTags = WechatOpen.GetUserTags(openId);
            if (!userTags.tagid_list.Contains(tag.id))
            {
                // 如果不包含该标签，则设置
                var reply = WechatOpen.SetUserTag(new List<string>() { openId }, tag.id);
                if (reply.errcode != 0)
                    return false;
            }
            return true;
        }

    }
}
