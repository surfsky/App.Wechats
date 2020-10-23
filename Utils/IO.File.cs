using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 文件相关辅助操作
    /// </summary>
    internal static partial class IO
    {
        /// <summary>合并两个路径（善不支持..操作符）</summary>
        public static string CombinePath(this string path1, string path2)
        {
            path1 = path1 ?? "";
            path2 = path2 ?? "";
            path1 = path1.Replace(@"/", @"\").TrimEnd('\\');
            path2 = path2.Replace(@"/", @"\").TrimStart('\\');
            return path1.IsEmpty() ? path2 : $"{path1}\\{path2}";
        }

        /// <summary>合并两个网页路径（善不支持..操作符）</summary>
        public static string CombineWebPath(this string path1, string path2)
        {
            path1 = path1 ?? "";
            path2 = path2 ?? "";
            path1 = path1.Replace(@"\", @"/").TrimEnd('/');
            path2 = path2.Replace(@"\", @"/").TrimStart('/');
            return path1.IsEmpty() ? path2 : $"{path1}/{path2}";
        }

        /// <summary>计算相对路径</summary>
        /// <returns>相对路径，格式如: \subfolder\filename.doc </returns>
        public static string ToRelativePath(this string physicalPath, string root)
        {
            if (physicalPath.IsEmpty() || root.IsEmpty())
                return "";
            root = root.ToLower();
            if (physicalPath.ToLower().SubText(0, root.Length) == root)
            {
                var p = "\\" + physicalPath.Substring(root.Length);
                return p;
            }
            return "";
        }

        //------------------------------------------------
        // 路径
        //------------------------------------------------
        /// <summary>准备文件路径（不存在则创建）</summary>
        /// <param name="fileOrFolderPath">文件或目录的物理路径</param>
        public static void PrepareDirectory(string fileOrFolderPath)
        {
            var folder = fileOrFolderPath;

            // 如果参数是文件，则尝试获取目录
            var ext = fileOrFolderPath.GetFileExtension();
            if (ext.IsNotEmpty())
            {
                var fi = new FileInfo(fileOrFolderPath);
                folder = fi.Directory.FullName;
            }

            //
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        /// <summary>删除目录及子文件</summary>
        public static bool DeleteDirectory(string folder)
        {
            try
            {
                var di = new DirectoryInfo(folder);
                di.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                File.SetAttributes(folder, FileAttributes.Normal);
                if (Directory.Exists(folder))
                {
                    foreach (string f in Directory.GetFileSystemEntries(folder))
                    {
                        if (File.Exists(f))
                            File.Delete(f);
                        else
                            DeleteDirectory(f);
                    }
                    Directory.Delete(folder);
                }
                return true;
            }
            catch { return false; }
        }

        //------------------------------------------------
        // 文件存取
        //------------------------------------------------
        /// <summary>读取文件到字节数组</summary>
        public static byte[] ReadFileBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        /// <summary>读取文件文本</summary>
        public static string ReadFileText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>删除文件</summary>
        public static void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        /// <summary>获取文件的MD5哈希信息</summary>
        /// <param name="filePath"></param>
        /// <returns>十六进制字符串</returns>
        public static string GetFileMD5(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);
                file.Close();

                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.AppendFormat("{0:x2}", bytes[i]);
                return sb.ToString();
            }
        }

        /// <summary>写文件（新建或附加）</summary>
        /// <param name="filePath">文件的物理路径</param>
        /// <param name="append">是否附加再末尾，还是新建文件</param>
        public static void WriteFile(string filePath, string data, bool append = true)
        {
            if (append)
            {
                using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    var sw = new StreamWriter(fs);
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.Write(data);
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    var sw = new StreamWriter(fs);
                    sw.Write(data);
                    sw.Flush();
                    sw.Close();
                }
            }
        }


        /// <summary>合并文件</summary>
        /// <param name="files">源文件路径列表</param>
        /// <param name="mergeFile">合并文件路径</param>
        public static void MergeFiles(List<string> files, string mergeFile, bool deleteRawFiles = true)
        {
            if (File.Exists(mergeFile))
                File.Delete(mergeFile);

            using (FileStream stream = new FileStream(mergeFile, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (string file in files)
                    {
                        // 拷贝合并到新文件（并删除临时文件）
                        using (FileStream fileStream = new FileStream(file, FileMode.Open))
                        {
                            using (BinaryReader fileReader = new BinaryReader(fileStream))
                            {
                                byte[] bytes = fileReader.ReadBytes((int)fileStream.Length);
                                writer.Write(bytes);
                            }
                        }
                        if (deleteRawFiles)
                            File.Delete(file);
                    }
                }
            }
        }

        //------------------------------------------------
        // 文件名处理
        //------------------------------------------------
        /// <summary>获取查询字符串字典</summary>
        public static FreeDictionary<string, string> GetQuery(this string fileName)
        {
            return new Url(fileName).Dict;
        }

        /// <summary>获取查询字符串字典</summary>
        public static string GetQueryString(this string fileName)
        {
            return new Url(fileName).Dict.ToString();
        }

        /// <summary>去除尾部的查询字符串</summary>
        public static string TrimQuery(this string url)
        {
            if (url.IsEmpty())
                return "";
            int n = url.LastIndexOf('?');
            if (n != -1)
                return url.Substring(0, n);
            return url;
        }

        /// <summary>去除文件扩展名</summary>
        public static string TrimExtension(this string url)
        {
            if (url.IsEmpty())
                return "";
            int n = url.LastIndexOf('.');
            if (n != -1)
                return url.Substring(0, n);
            return url;
        }

        /// <summary>去除目录部分</summary>
        public static string TrimFolder(this string url)
        {
            if (url.IsEmpty())
                return "";
            int n = url.LastIndexOf('/');
            if (n != -1)
                url = url.Substring(n + 1);
            n = url.LastIndexOf('\\');
            if (n != -1)
                url = url.Substring(n + 1);
            return url;
        }

        /// <summary>获取文件名（去掉路径和查询字符串）</summary>
        public static string GetFileName(this string url)
        {
            if (url.IsEmpty())
                return "";
            return url.TrimQuery().TrimFolder();
        }

        /// <summary>获取文件目录（返回目录值不带斜杠）</summary>
        public static string GetFileFolder(this string url)
        {
            if (url.IsEmpty())
                return "";

            int n = url.LastIndexOf('/');
            if (n != -1)
                return url.Substring(0, n);
            n = url.LastIndexOf('\\');
            if (n != -1)
                return url.Substring(0, n);
            return "";
        }

        /// <summary>获取文件扩展名（扩展名经过小写处理;）</summary>
        public static string GetFileExtension(this string fileName)
        {
            if (fileName.IsEmpty())
                return "";
            fileName = fileName.TrimQuery();
            int n = fileName.LastIndexOf('.');
            if (n != -1)
            {
                var ext = fileName.Substring(n).ToLower();
                if (!ext.Contains(@"/") && !ext.Contains(@"\"))  // 不包含路径斜杠
                    return ext;
            }
            return "";
        }

        /// <summary>构建后继文件名（附加递增数字），如：rawname_2.eml, rawname_3.eml</summary>
        /// <param name="format">格式字符串。如：_{0}, -{0}, ({0})</param>
        public static string GetNextName(this string url, string format= @"_{0}")
        {
            // 三部分
            var num = 2;      // 数字编号
            var front = url;  // 字符 "." 前面的部分
            var last = "";    // 字符 "." 后面的部分
            int n = url.LastIndexOf('.');
            if (n != -1)
            {
                last = url.Substring(n);
                front = url.Substring(0, n);
            }

            // 将格式化公式转化为正则表达式并匹配，如：{0} -> (\d+)$
            var match = format
                .Replace("(", @"\(").Replace(")", @"\)")    // 替换()直接量
                .Replace("[", @"\[").Replace("]", @"\]")    // 替换[]直接量
                .Replace("{0}", @"(\d+)")                   // 替换数字部分
                ;
            Regex reg = new Regex(match + "$");
            var m = reg.Match(front);
            if (m.Success)
            {
                // 如果匹配成功，计算新编号
                var txt = m.Result("$1");
                try
                {
                    var k = txt.ParseInt();
                    if (k != null) num = k.Value + 1;
                }
                catch { }

                // 去除匹配部分
                front = reg.Replace(front, "");
            }

            // 构造新名称: (\d+) -> {0}
            //var format = match.Replace(@"(\d+)", "{0}").Replace(@"\", "");
            var numText = string.Format(format, num);
            return string.Format("{0}{1}{2}", front, numText, last);
        }

        /// <summary>该文件是否是图片（根据扩展名）</summary>
        public static bool IsImageFile(this string fileName)
        {
            string ext = GetFileExtension(fileName);
            if (ext.IsEmpty())
                return false;

            string[] exts = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".bmp", ".tif", ".tiff" };
            return exts.Contains(ext);
        }

        /// <summary>获取文件 MimeType</summary>
        public static string GetMimeType(this string fileName)
        {
            if (fileName.IsEmpty())
                return "";
            var ext = GetFileExtension(fileName);
            if (ext.IsEmpty())
                return "application/octet-stream";
            var mimeType = Mimes[ext];
            return mimeType ?? "application/octet-stream";
        }

        /// <summary>
        /// MimeType
        /// https://www.w3school.com.cn/media/media_mimeref.asp
        /// </summary>
        public static FreeDictionary<string, string> Mimes { get; set; }
        static IO()
        {
            Mimes = new FreeDictionary<string, string>();
            Mimes[".jpg"]  = "image/jpeg";
            Mimes[".jpeg"] = "image/jpeg";
            Mimes[".png"]  = "image/png";
            Mimes[".bmp"]  = "image/bmp";
            Mimes[".gif"]  = "image/gif";
            Mimes[".html"] = "text/html";
            Mimes[".json"] = "text/json";
            Mimes[".txt"]  = "text/plain";

            // 来自IISExpress application.config
            Mimes[".doc"]  = "application/msword";
            Mimes[".docx"] = "application/application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            Mimes[".xls"]  = "application/vnd.ms-excel";
            Mimes[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Mimes[".ppt"]  = "application/vnd.ms-powerpoint";
            Mimes[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

            Mimes[".exe"]  = "application/octet-stream";
            Mimes[".pdf"]  = "application/pdf";
            Mimes[".js"]   = "application/x-javascript";
            Mimes[".mp3"]  = "audio/mp3";
            Mimes[".mp4"]  = "vedio/mp4";

            //
            Mimes[".cdr"]  = "application/x-cdr";
        }
    }
}
