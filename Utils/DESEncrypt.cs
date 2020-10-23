using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace App.Wechats.Utils
{
    /// <summary>
    /// DES加密/解密类（EncryptHelper已经实现，本类将废除）
    /// </summary>
    [Obsolete]
    internal class DESEncrypt
    {
        /// <summary>默认加密向量</summary>  
        private static byte[] _vector = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };


        /// <summary>DES加密字符串</summary>  
        /// <param name="text">待加密的字符串</param>  
        /// <param name="key">加密密钥,要求为8位</param>  
        /// <returns>加密成功返回加密后的字符串，失败返回空字符串</returns>  
        public static string EncryptDES(string text, string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                var cryptor = new DESCryptoServiceProvider().CreateEncryptor(keyBytes, _vector);
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, cryptor, CryptoStreamMode.Write);
                cStream.Write(textBytes, 0, textBytes.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return "";
            }
        }

        /// <summary>DES解密字符串</summary>  
        /// <param name="text">待解密的字符串</param>  
        /// <param name="key">解密密钥,要求为8位,和加密密钥相同</param>  
        /// <returns>解密成功返回解密后的字符串，失败返空字符串</returns>  
        public static string DecryptDES(string text, string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] textBytes = Convert.FromBase64String(text);
                var cryptor = new DESCryptoServiceProvider().CreateDecryptor(keyBytes, _vector);
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, cryptor, CryptoStreamMode.Write);
                cStream.Write(textBytes, 0, textBytes.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return "";
            }
        }
    }

}
