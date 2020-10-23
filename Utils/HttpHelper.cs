using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
//using System.Web;
//using System.Web.UI;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 要上传的文件信息
    /// </summary>
    internal class PostFile
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public PostFile(byte[] file, string fileName = null, string contentType = null)
        {
            this.File = file;
            this.FileName = fileName;
            this.ContentType = contentType;
        }
    }


    /// <summary>
    /// HTTP 操作相关（GET/POST/...)
    /// </summary>
    internal static class HttpHelper
    {
        ///------------------------------------------------------------
        /// 解析返回对象（文本、图像等）
        ///------------------------------------------------------------
        /// <summary>获取 Http 响应文本</summary>
        public static string ToText(this HttpWebResponse response)
        {
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
                encoding = "UTF-8";
            var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            return reader.ReadToEnd();
        }

        /// <summary>获取 Http 响应图片</summary>
        public static Image ToImage(this HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                return new Bitmap(stream);
            }
        }


        //---------------------------------------------------------
        // Get 方法
        //---------------------------------------------------------
        /// <summary>Get</summary>
        public static string Get(string url, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
        {
            // 请求
            cookieContainer = cookieContainer ?? new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = cookieContainer;
            SetRequestHeaders(request, headers);

            // 返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            return response.ToText();
        }

        /// <summary>设置请求头</summary>
        /// <remarks>
        /// 限制标头要调用方法或属性进行设置，不然会报错：
        /// Host由系统设置为当前主机信息。
        /// Referer由 Referer 属性设置。
        /// User-Agent由 UserAgent 属性设置。
        /// Accept由 Accept 属性设置。
        /// Connection由 Connection 属性和 KeepAlive 属性设置。
        /// Range HTTP标头是通过AddRange来添加手工
        /// If-Modified-Since HTTP标头通过IfModifiedSince 属性设置
        /// Content-Length由 ContentLength 属性设置。
        /// Content-Type由 ContentType 属性设置。
        /// Expect由 Expect 属性设置。
        /// Date由 Date属性设置，默认为系统的当前时间。
        /// Transfer-Encoding由 TransferEncoding 属性设置（SendChunked 属性必须为 true）。
        /// </remarks>
        static void SetRequestHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
            {
                var key = header.Key;
                var value = header.Value;
                if (!WebHeaderCollection.IsRestricted(key))
                    request.Headers.Add(key, value);
                else
                {
                    var k = key.ToLower();
                    if (k == "host")            request.Host = value;
                    else if (k == "referer")    request.Referer = value;
                    else if (k == "user-agent") request.UserAgent = value;
                    else if (k == "accept")     request.Accept = value;
                    else if (k == "connection") request.Connection = value;
                }
            }
        }


        //---------------------------------------------------------
        // Post方法
        //---------------------------------------------------------
        /// <summary>Post (查询）字符串</summary>
        public static string Post(string url, string text, Encoding encoding = null, string contentType = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
        {
            byte[] bytes = text.ToBytes(encoding);
            return Post(url, bytes, contentType, cookieContainer, headers);
        }

        /// <summary>POST 字节数组</summary>
        public static string Post(string url, byte[] bytes, string contentType = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
        {
            MemoryStream stream = bytes.ToStream();
            return Post(url, stream, contentType, cookieContainer, headers).ToText();
        }

        /// <summary>Post 文件</summary>
        public static string PostFile(string url, string filePath, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            return Post(url, fileStream, null, cookieContainer, headers).ToText();
        }

        /// <summary>Post 字节流</summary>
        public static HttpWebResponse Post(string url, Stream stream, string contentType = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers=null)
        {
            // 参数
            cookieContainer = cookieContainer ?? new CookieContainer();
            contentType = contentType ?? "application/x-www-form-urlencoded";

            // 请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = stream != null ? stream.Length : 0;
            request.CookieContainer = cookieContainer;
            //request.SendChunked = false;
            //request.KeepAlive = true;
            //request.Proxy = null;
            //request.Timeout = Timeout.Infinite;
            //request.ReadWriteTimeout = Timeout.Infinite;
            //request.AllowWriteStreamBuffering = false;
            //request.ProtocolVersion = HttpVersion.Version11;

            // 头部
            SetRequestHeaders(request, headers);


            // 上传
            if (stream != null)
            {
                Stream requestStream = request.GetRequestStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);
                stream.Close();
            }

            // 返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            return response;
        }




        ///------------------------------------------------------------
        /// Post MultipartForm Http 协议 (multipart/form-data) 辅助方法。
        /// 普通post是简单的name=value值连接，而multipart/form-data则是添加了分隔符的内容组合。
        /// 一次性可以传多个数据，如：文本、图片、文件。
        /// 和普通POST的区别: http://blog.csdn.net/five3/article/details/7181521
        /// 来自Face++ 代码示例：https://console.faceplusplus.com.cn/documents/6329752
        ///------------------------------------------------------------
        /// <summary>Post MultipartForm</summary>
        public static string PostMultipartForm(string url, Dictionary<string, object> data, Encoding encoding = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
        {
            string boundary = string.Format("----------{0:N}", Guid.NewGuid()); // 分隔字符串
            string contentType = "multipart/form-data; boundary=" + boundary;   // multipart 请求类型
            byte[] bytes = BuildMultipartFormData(data, boundary, encoding);    // 将参数转化为字节数组
            return Post(url, bytes, contentType, cookieContainer, headers);
        }

        /// <summary>组装参数字典</summary>
        /// <param name="data">参数字典</param>
        /// <param name="boundary">分隔字符串</param>
        private static byte[] BuildMultipartFormData(Dictionary<string, object> data, string boundary, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            bool flag = false;
            Stream stream = new MemoryStream();
            foreach (var item in data)
            {
                if (flag)
                    stream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                flag = true;
                if (item.Value is PostFile)
                {
                    PostFile fileParameter = (PostFile)item.Value;
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", new object[]
                    {
                        boundary,
                        item.Key,
                        fileParameter.FileName ?? item.Key,
                        fileParameter.ContentType ?? "application/octet-stream"
                    });
                    stream.Write(encoding.GetBytes(s), 0, encoding.GetByteCount(s));
                    stream.Write(fileParameter.File, 0, fileParameter.File.Length);
                }
                else
                {
                    string s2 = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, item.Key, item.Value);
                    stream.Write(encoding.GetBytes(s2), 0, encoding.GetByteCount(s2));
                }
            }
            string s3 = "\r\n--" + boundary + "--\r\n";
            stream.Write(encoding.GetBytes(s3), 0, encoding.GetByteCount(s3));
            stream.Position = 0L;
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            stream.Close();
            return array;
        }



        /// <summary>获取服务器或网络图片</summary>
        /// <param name="url">可用~/，也可以用完整的http地址</param>
        //public static Image GetServerOrNetworkImage(string url)
        //{
        //    if (url.StartsWith("~/") || url.StartsWith(".") || url.StartsWith("/"))
        //    {
        //        if (Asp.IsWeb)
        //            return Painter.LoadImage(Asp.MapPath(url));
        //    }
        //    else
        //        return HttpHelper.GetNetworkImage(url);
        //    return null;
        //}

        /// <summary>获取网络图片</summary>
        public static Image GetNetworkImage(string url)
        {
            try
            {
                var req = (HttpWebRequest)(WebRequest.Create(new Uri(url)));
                req.Timeout = 180000;
                req.Method = "GET";
                var res = (HttpWebResponse)(req.GetResponse());
                return new Bitmap(res.GetResponseStream());
            }
            catch
            {
                return null;
            }
        }
    }
}