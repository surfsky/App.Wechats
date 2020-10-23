using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 负责安全、加密、解密等内容
    /// </summary>
    internal static class EncryptHelper
    {


        //-------------------------------------------------------------------------
        // 获取字符串的hash值
        //-------------------------------------------------------------------------
        /// <summary>获取字符串 MD5 哈希值（32字符）,如：C6CEBD9247AAB3A6EDAA7629F404CC50</summary>
        public static string MD5(this string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var md5 = new MD5CryptoServiceProvider();
            var bytes = md5.ComputeHash(encoding.GetBytes(text));
            return bytes.ToHexString();
        }

        /// <summary>获取字符串 SHA1 哈希值（40字符）,如：D3486AE9136E7856BC42212385EA797094475802</summary>
        public static string SHA1(this string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var sha1 = new SHA1CryptoServiceProvider();
            var bytes = sha1.ComputeHash(encoding.GetBytes(text));
            return bytes.ToHexString();
        }

        /// <summary>获取字符串 HmacSHA256 哈希值（64字符），如：852D2FEC4BDA6ADD8F12C5C1DFF8420510AC5B85EF432140C7097AAEE3C270CA</summary>
        public static string HmacSHA256(this string text, string secret="", Encoding encoding=null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var hmacsha256 = new HMACSHA256(encoding.GetBytes(secret));
            var bytes = hmacsha256.ComputeHash(encoding.GetBytes(text));
            return bytes.ToHexString();
        }




        //-------------------------------------------------------------------------
        // 异或加密解密
        //-------------------------------------------------------------------------
        /// <summary>循环异或加解密</summary>
        /// <param name="txt">原文本</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">文本编码方式</param>
        /// <returns>异或加密后的文本</returns>
        public static string XOR(this string txt, string key, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] ret = XOR(encoding.GetBytes(txt), encoding.GetBytes(key));
            return encoding.GetString(ret);
        }

        /// <summary>循环异或加密</summary>
        /// <param name="src">源字节数组</param>
        /// <param name="key">密钥字节数组</param>
        /// <returns>加密后的字节数组（长度和源字节数组相同）</returns>
        public static byte[] XOR(this byte[] src, byte[] key)
        {
            byte[] ret = new byte[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                int j = i % key.Length;
                ret[i] = (byte)(src[i] ^ key[j]);
            }
            return ret;
        }



        ///-------------------------------------------------------------------------
        /// 使用 DES 算法加密解密（对称算法）
        ///-------------------------------------------------------------------------
        /// <summary>
        /// 用 DES 算法加密字符串。
        /// DES 是私钥加密又称为对称加密，因为同一密钥既用于加密又用于解密
        /// 速度快，特别适用于对较大的数据流执行加密转换
        /// </summary>
        /// <param name="text">要加密的文本</param>
        /// <param name="key">8或16字节密钥，如"12345678"</param>
        /// <returns>加密后的文本</returns>
        public static string DesEncrypt(this string text, string key, Encoding encoding = null)
        {
            if (text.IsEmpty()) return "";
            encoding = encoding ?? Encoding.UTF8;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = encoding.GetBytes(key);

            byte[] inputBuffer = encoding.GetBytes(text);
            byte[] outputBuffer = des.CreateEncryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return outputBuffer.ToBase64String();
        }

        /// <summary>用 DES 算法解密字符串</summary>
        /// <param name="text">要解密的文本</param>
        /// <param name="key">密钥：8或16字节</param>
        /// <returns>解密后的文本</returns>
        public static string DesDecrypt(this string text, string key, Encoding encoding = null)
        {
            if (text.IsEmpty())  return "";
            encoding = encoding ?? Encoding.UTF8;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = encoding.GetBytes(key);

            byte[] inputBuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = des.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return encoding.GetString(outputBuffer).TrimEnd('\0');
        }

        ///-------------------------------------------------------------------------
        /// RSA 非对称加密
        ///-------------------------------------------------------------------------
        /// <summary>
        /// 用RSA加密字符串，产生加密文本和密钥
        /// RSA算法的理论根据来自于一个大素数所具有的特性。对于给定的两个大素数A与B，很容易计算出它们的乘积。
        /// 但是，仅知道AB的乘积却很难计算原来的A与B各自的值。
        /// 在不深入到RSA算法细节的情况下，可以简单得认为(A，B)这对数定义了非对称算法中的私钥，而AB的乘积则定义了算法中的公钥。
        /// 以Base64String方式保存，公钥324个字符，私钥1220字符。
        /// </summary>
        /// <example>
        ///    string msg = "hello world";
        ///    var pair = EncryptHelper.RSACreateKeyPair();
        ///    string encrytedMsg = EncryptHelper.RSAEncrypt(msg, pair.Key);
        ///    string decrytedMsg = EncryptHelper.RSADecrypt(encrytedMsg, pair.Value);
        /// </example>
        /// <remarks>
        ///    公钥
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPjB3WlB6ODRqY21CNkVSTFlWTXVTNTQ2UzJuY1NHZERWeXZobEwzNXBwQitKWjR5c1Rlbk45eXpwZ1V1azRpNGtueDdCTTl1dUMxZlIzdFpaMnFhMDMvaWhoM1VjLzF0aFJtdjJpRFF3N3NkRkpOcm5JUnhyZnhxSXRPOWFlaGpIbEdad1MwZVhyNjN1dVRyOG9ibWZhaDdscWQ5VTArOGhOTXV1STJycW8zaz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnhBNkFCM2JDOXZVZ09mZ2pkZmE0VWIvcWxMdGxVdkJnTzNMMklrcTVFY3dCYVRTM1pKQjFEWW42K3ZUblEzUk43aUVQbEhRakYwVkl2eTllMk82QmRhd0NoWUNISFlhYUdhVGN5azFhYUFNVFRPV0M3YmMwMVNlVlNSVWp0QnZHRVZad01NSXIwUlUzUEYyZVhaZWN0d0FLZFNWZGQvYkowNVl5cnNZNm1Bcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnAxV2ZwQ2lVS0VSc0RMcFhMQmV0ZElETlFrSDZUNmk5OVEwdHJBRTBKYncvU25aaVBablVkYzU5UGxPSFNsM1VzU1c4V0hYNWZjdEYwMUdQMWhZazhDZlV0TXNWVGZrSUdVd2o4QkxBSkM4ZzdqZG8yMm1GVzZtUnYxRnU5QWxjQXZEbXRRZnVDWWxXd3JWdjd2M1g1WFk3cytsYnpKNHU4SGFKeG55eUgzVT08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPjVXdWE1WWg3RnJOdTNveGJ6T2dBSEFMNHhzenhJUFhxQk51SFE5eENBVUh1R3NXNURmNEMvVUIyVzNFUTVNeEt3UlRjbzVKeUJoQ2hBVmxGTDgwZ3gvT3YwUTB4Rjl2TWRvNkxmOThwQWYvZzBMd0JpcU1xN3o0bml0WXk1RVoyUllPQ29kbDhhZ2Mxdm5VSlNuN2t5QjY5R0NwQnc3RDFDSVR3UDBQcXFpcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnh4SXVUUWZLdTVwNllONFV2RXZXR1pCOUEwRXVpZzVxZGZrTUJ1NUU4UU9aNzROdGNKcHRCYkhuSEh5YmE5bHBORkVKRTVFalluZzltRTlWUmNHZExKVUw1NVM3TlM3cWUvdi9ETkpmTEJOMmlaWUxxY2lkOHZSVWtRVmZ1UnlRR3p5SUtYZG9DTG5FdjMvY0xLaTI3TFhDM1NYV0VDL2JYNlorbFBQcXhtMD08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnZYSmdUQWlBQmYxOHZVcnliVWNGQlNjeG9OUDJyQ1QySjMvZmVvbmxvRWVVbWlBaUo3MDVrNzRORk9aRzFCNGxaMk1kcHVFdXRSdmxUbHZuSzdNL21SUUsvKzJJUHp6dU9vckhoZEJjcElzeUJPWWJsdE0rajNyR2NwTGJjYmcxdWlGS0I3Y0lUWkpPcFlGMFViY0tKWW9UMEFuTHJqNEhXSEcrTFNFVXRZcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+
        ///    私钥
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPjB3WlB6ODRqY21CNkVSTFlWTXVTNTQ2UzJuY1NHZERWeXZobEwzNXBwQitKWjR5c1Rlbk45eXpwZ1V1azRpNGtueDdCTTl1dUMxZlIzdFpaMnFhMDMvaWhoM1VjLzF0aFJtdjJpRFF3N3NkRkpOcm5JUnhyZnhxSXRPOWFlaGpIbEdad1MwZVhyNjN1dVRyOG9ibWZhaDdscWQ5VTArOGhOTXV1STJycW8zaz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjcyeTlROHJDYVJOeFo2R3MrREdXNEd3MmJ4d0tTQWxYS0oyTGM4T04vUzc0WFRjTVdVZy9zZ0NYT2IyRjdoRGlnSG9vQnJZU2dEN043QU0zbk5mMzV3PT08L1A+PFE+NGFJK3R2YmZRbWRPQzFxSUtNbzJwRDI3cEFFWXcxM2d3OTJsKzNsYUo2V2duNk1CWi9FWUNMOExhOUF4M0FCd2NaT1dNRmZKVm45WmQ3cVJJR21kbnc9PTwvUT48RFA+UVQxK0xUVFkyTTBGZjltY1NsMG80YXBiQXRlL0xYWHVIQkVoNkwrR3QxRFBPSWRCaENxZHdLRk1rOTFDMjJZYWNpdlhNRXo0cVoxemV6WTlOeTNhVFE9PTwvRFA+PERRPm1lMGFSYTF6TDVUVURERE50SzRHeXRNR2dHTHpKc3lUZW10cVFYMVBBTDhnTGVlQkhReS9yYS9QTmRUSlB1SFowOUd3WXZod2RSN1p4VEUwc2x6NjF3PT08L0RRPjxJbnZlcnNlUT5qazNybVgxUmhzZDNWRW9tK3ZWanFUKzZjRktxRjIrWkpqTklIMUhPdVcrNlJtaHQrSXlvSXdSelorK01FbXJtaEZ5ZEpDYWdyek5FTTlLVzhteFUzUT09PC9JbnZlcnNlUT48RD5UUjA0V3pyOEx4YmNaSkxiWlJsRFpNVHdHMDk1Mk44ODBVQTJVY1hET3d1ZlBhemxaazl6U2NrcXgybnFKaEV3cHNrcFZ4Y0hJZlFFcUlieUxQblFqSUl2aGpLL1lwd2VCT0xGTDBpTW1NdnJSUUpIYVVXMHA0Skh3KzgwWHhxamg0Wk5OY3g5ZTJ4Tm45WVp3dmtjbHBGM2toRmJaZHdiamxHV1ByekpVMlU9PC9EPjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnhBNkFCM2JDOXZVZ09mZ2pkZmE0VWIvcWxMdGxVdkJnTzNMMklrcTVFY3dCYVRTM1pKQjFEWW42K3ZUblEzUk43aUVQbEhRakYwVkl2eTllMk82QmRhd0NoWUNISFlhYUdhVGN5azFhYUFNVFRPV0M3YmMwMVNlVlNSVWp0QnZHRVZad01NSXIwUlUzUEYyZVhaZWN0d0FLZFNWZGQvYkowNVl5cnNZNm1Bcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjdmZlRHVlBRaXZ4bHplTDcvZ21ISXllYzBxUzVaQjZQUEJ0aWkxR0QxNmYyZTFleG5UZmV6d2E0WHZ0d3FjeDVCM0dhTFNZZ0tpdnBsS1FTdVBoUTF3PT08L1A+PFE+MHVtcVRGeTdQNHZFZnJhZTNOMXhtSk5PVlJYYk1nTXJRU3dJUk1ZT1FWQmg4dTY5eGdXUStwVjRIa0RaaUpVODVDZ29xVjkvUFBmWUhTZFM0RkVuN1E9PTwvUT48RFA+V2hqamZ3cGh3YzJRQ2VTdnZkNERvVWJGTkdlVG5abUlaNXlDc2ZiWjhST3gyYjg1Q3lwMUhITVI2VHhQeXFvVkJNRjVPekRkS3JwWGhLU2VSaFFXSHc9PTwvRFA+PERRPlZXOG5kNlU4aUVJaWh6Mk1YbVVwSmFmTjNETnRSZlg0cUg2Z250TW5aUmVkaFoxbHEvZ0hRU29ZclJDUnpYeStYS0ZUejBBS3QzU2h5elZwb2NuZUJRPT08L0RRPjxJbnZlcnNlUT5uaFVuMTRBUWpHQkh3a0ZZVzhYSk1TUGpCNXl1MWVObWREMlFqWGtad1BNVC9lLzVTMlBwaGhiODg4ekw1c1JyRmh5cXQ0Y1lzL3FNVVFKWjJ5OWl0Zz09PC9JbnZlcnNlUT48RD5CUU9KNnFieTNYWHZXUXdyZS96UFFlZnlpOU12Q0N2MWlnK3FrNzN0ZGJNTjVpN2U4Y2R2OXVTc2NuYUk0NFM1NlhrVytPanZiTXpzeTFiZXloVnZZaE9GU0x2RytQc2lTa2lyank4U0hLcHFiRFY2UmkwK1c5UTNJWlFLZDJReTlxT1pUY1NydS9rL0FkSy9MMWZMWm4rclVpUUtDUlBEWDNuRDJtODN5MkU9PC9EPjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnAxV2ZwQ2lVS0VSc0RMcFhMQmV0ZElETlFrSDZUNmk5OVEwdHJBRTBKYncvU25aaVBablVkYzU5UGxPSFNsM1VzU1c4V0hYNWZjdEYwMUdQMWhZazhDZlV0TXNWVGZrSUdVd2o4QkxBSkM4ZzdqZG8yMm1GVzZtUnYxRnU5QWxjQXZEbXRRZnVDWWxXd3JWdjd2M1g1WFk3cytsYnpKNHU4SGFKeG55eUgzVT08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjBzb1QvekVGdUZsVjkvUCtPQjlEL3JNaEZmNnpXNHVSRkhFWVczSTZRZmpBaVdQbWpXOHB2TFZEQ0JqUUZXdEdETHFrWmV6bHBOZnZiNXlHRE90NUxRPT08L1A+PFE+eXptTGFUUmw1QURCT0wwVkxlVlZUdEp2ZUVVcHVSTzJiUFRGZGxEejlEbFYvaS81M1hIdk5uUzEzaEdrTW5PaEIwUWh1U0dhdEhDZWpsT1MyeDZjYVE9PTwvUT48RFA+RWxMUXRERk52djI5RkdYWVpvYUpRWjNPdFh2RG1hU2pRdlNsMm1VdW5VZURiUzRLLzZaM0tWbFViMkxBeSt2Y1ZnVHZmNUM5VWJ3WHEwc2UrQWFNTFE9PTwvRFA+PERRPmo0dG02SG1sV3FZWjFRemhyOWhrS0ZmRmVxdEhyRDI3Ump5aVdVOFc4Yk9xQlBBNFNtMVdyUjFFOU1WN09GT3FNeXF4czBXRU05MjBjUTJoRm5zSnVRPT08L0RRPjxJbnZlcnNlUT5sRGpFNG5iVGZxa1ZQZmhXVmhKbTZWSi9sYTdPbnI2Myt3ME1kT1Fkb2F3L1o3dkNaQ1ZDa01jUzJNSC9SQXNOb1NxdjhQbU5sRTBKc1diZTdtYnJOdz09PC9JbnZlcnNlUT48RD5BcldES1NBekRkZlZ1L3NFVUVWdDNIWDlYTFkrdkMxcUxkNXh2OWdoSlA5QU13OXBsYis0emROeSthQldqMkNuZ2Rxdml1c1dCL0JUaUJGSTk5bFI3YjZ6Y05veEtNUUJXUzFpbGFvQ08wUmR4STFwZU9pYzlQU1ZpVWdNMnQ0SlRyZUJ6TWxpSFQ4QktKOC9hN1lvZXMzU1dMTXV2eTVhWUh1OVdTOFA3V0U9PC9EPjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPjVXdWE1WWg3RnJOdTNveGJ6T2dBSEFMNHhzenhJUFhxQk51SFE5eENBVUh1R3NXNURmNEMvVUIyVzNFUTVNeEt3UlRjbzVKeUJoQ2hBVmxGTDgwZ3gvT3YwUTB4Rjl2TWRvNkxmOThwQWYvZzBMd0JpcU1xN3o0bml0WXk1RVoyUllPQ29kbDhhZ2Mxdm5VSlNuN2t5QjY5R0NwQnc3RDFDSVR3UDBQcXFpcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPi9EeG01ZEZQQkxCenVCR0ZLb2h1c25aY3FhdnFWUS9MWHJFMDQ0R1ZJVVJkcGlvdFcwVUtYVmc3amdCTkp3czdNeDJvMjJ4Um1JQjRzSk0ya1FyMWZRPT08L1A+PFE+Nk5nSzF6OFEzTDJWNFc1NGJSNjRuM3BEZU12OTAyaytrTE42YnJ0VVhBNWtLTzZBYzFzOXVMaDJQaVd2cWtPUTdwM0x1dC9YbEJPTzRQUG5GQzBPeHc9PTwvUT48RFA+YmtoQ2ZRenN6WklRcGxRU3N5di9xa3VSN0NWY1NXQ2tmSHhpTVc2QXM1RGVtVDRyb3BJbkcvVHQ1UUpBdVdkeHRNR3RDSEx0b2czWnR1cEdtMWY5U1E9PTwvRFA+PERRPjJKWUZWMzBKVVZ0a2JJSldzS0h2K0NCQlhMN2JoMmlSVXdZdjc0cC9DUFkyQXNEL2FNNUpWbWIzVGcyK0hqR2xRZUF6M3N0U2V5SEtPTU9IZ2dQN2h3PT08L0RRPjxJbnZlcnNlUT5qTzRiVkpBa0xzMzkyY0VPYlNzd054WXRxUmhSYXAraW50SXNJa0pGbVRpZjdISklTeSsxOGcrcU1vUEdZUTBBRmJUaml2SHNjS2NRazNBWTF4WFFTdz09PC9JbnZlcnNlUT48RD5vMzQ3dExlK2dENy9FN29TOGNPajBGS04xNTdWZUJmSDNnNVVKeGdnTWFVNmVmbk9GeHFlSnJ4NW5GREhLVVl0UDh3T01NTjhYclliQjBzRzh6bkwwVmszZ0lVcHkvS3d1QlVwVG42SEc4QTVHMmxQbkdxenZDYVUxcnNENE9UV1dsV0g3RktRN2Q5Yjk5bWpvd2ZCaXdLUzJHL281MlRQZkUzNjdlcWFoWUU9PC9EPjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnh4SXVUUWZLdTVwNllONFV2RXZXR1pCOUEwRXVpZzVxZGZrTUJ1NUU4UU9aNzROdGNKcHRCYkhuSEh5YmE5bHBORkVKRTVFalluZzltRTlWUmNHZExKVUw1NVM3TlM3cWUvdi9ETkpmTEJOMmlaWUxxY2lkOHZSVWtRVmZ1UnlRR3p5SUtYZG9DTG5FdjMvY0xLaTI3TFhDM1NYV0VDL2JYNlorbFBQcXhtMD08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjkya1FxandMMkZKeU05ZHd0WEJIS1VVZlRMdklpZ1FpL3NVMW9GS0owQ3Vabm40bGR4QW44WU83TFhPTnJBYkphS2ZWOEc1dk4vR080UzltaTVZM3RRPT08L1A+PFE+emZ0OEcyWVI1ZEZsamwxVGZIWDF2ZHRJY3VTcFFsV3Y1VXVFeWtvcWVaM3l1N0EzRUxNbUY5TVMzTkdzVFZQc2RCUlcvMGJRMnptclo3dVB2UndXMlE9PTwvUT48RFA+UnZFeGZoN0pwclc5V1hBbE9XR0FoaXp6MUtUajArOXJ1WElDOW4yMFZxU3AzL0I3L2EvOFJCeHB4NWFpd1BrUk41a29sMUNUUTQ5WVVPbXVsY2YxSVE9PTwvRFA+PERRPkZ5SzFDTnJKRGRnY0lRWm9keVZFcWNOMGVyc21LN1kySUhuLyt6eWpVcStqOE9MVS9JSXl1Q0JVRVF0WDRBT2FIbHVlZmVPMVY3bzlmMVE0eTlQWTJRPT08L0RRPjxJbnZlcnNlUT5yaDd3WnJsOWhjTDhBOWhIVWIyVWlRYzdsaDZxa1VsRmNpSDB2UUVZcXlMZkI2a2tyeU9sWVFHNWRHQlJhNEh1SmtscFpjWHJnVjdlbHhsREJUVWJTQT09PC9JbnZlcnNlUT48RD5QdHlmNEZWQWtLTkVWOGhwTmRpZ3dDdmZQaUxjSmw1TkFmbmw4VjVXU09GQlA5LzV0Q0Fmb3plOWFrSnk5Y2lPcnlXVHZ5a0t5bko3eU5mc2JuMm9PcmVkK2xBQmRXUjRaYTJXRWY4U2tUd242c0dCMUFrR0hqWGtwaVR5eGxna3VVUUM1WE1iU3BTaDh5aVZ4UWJaRUx6Um50MUkyVTdsMUxiTEFjVEJyU0U9PC9EPjwvUlNBS2V5VmFsdWU+
        ///    PFJTQUtleVZhbHVlPjxNb2R1bHVzPnZYSmdUQWlBQmYxOHZVcnliVWNGQlNjeG9OUDJyQ1QySjMvZmVvbmxvRWVVbWlBaUo3MDVrNzRORk9aRzFCNGxaMk1kcHVFdXRSdmxUbHZuSzdNL21SUUsvKzJJUHp6dU9vckhoZEJjcElzeUJPWWJsdE0rajNyR2NwTGJjYmcxdWlGS0I3Y0lUWkpPcFlGMFViY0tKWW9UMEFuTHJqNEhXSEcrTFNFVXRZcz08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjNhalNDc0hyTlFlQXk5Y0NGeG10SDIrZjNNQ04wNlJhUm1RdG50QUxObUJJSGF0SUVEdEREdWJJbWJGbm4xR01Vd3dtTDNMWmNvYWk5Z0FDUk4vOS9RPT08L1A+PFE+MnN2NkhXVXBvOHRIYk1CUk1ncHE5Y2NKMTdsSFhBdFdSR0taNy9TZmtjRXRROTFsU0Y0TVFjbWErMFQ3Zkd6OUY1SzB1NEdEZzFHblQ4VG9wWFpVSnc9PTwvUT48RFA+ZGFnZHpjMy9Mb2ZNQ3VjVzJmSXNIZXFmWFlyci9YSlk1TkphRU5sM2lpWnpRU1JndlJUbjFHb1dBdGJUSFJNcFBBL1AyUkhLY0dzYzV4MDhGeGthZFE9PTwvRFA+PERRPm5OZ0VIL1pZOXpYTnFjUGpjTC9QRlFqdG9Wc01NSXRmOS8zRzVDQzBFc1FzTXE0TzRPV3FXNjZ1RHRuUWZjMlRVTWEyRUlRelJudk9PSHlyV1pHTm1RPT08L0RRPjxJbnZlcnNlUT5NdHNMNEZTL2xiUUlwcHlaeFl6UkRScmFGNWpPenFuK2pHVm1INzBLZUdBRUNZWk9FY0k3MklMVXdRSTVvQzRJS292aXpoNzVMQ0xWVVhPNmNTQ3NTZz09PC9JbnZlcnNlUT48RD5DR2kwUjR1MUN3OEdZMnlaT0NxSDJZTzAyenV0WGxUQnJGMHJzVWUvcm82ZTFQeUtKNU5wbzlveXdITzhQeHdiY3V4ZVc1THhQTTdCTGVvOFo1OHNPbTZ4U3hqK3lCL3JhL2FqOXdvemcyQ1ZTS1g2VlJzS2MzRFp1MkNTRmtCLzFkRmhKcjY4VklMYklIZjcyd1IvcGExcnhwbVkyeDNHSU8zb3dYeW1sTWs9PC9EPjwvUlNBS2V5VmFsdWU+
        /// </remarks>
        public static KeyValuePair<string, string> RSACreateKeyPair()
        {
            RSACryptoServiceProvider p = new RSACryptoServiceProvider();
            string publicKey = p.ToXmlString(false);
            string privateKey = p.ToXmlString(true);

            Encoding encoding = new System.Text.UTF8Encoding();
            publicKey = Convert.ToBase64String(encoding.GetBytes(publicKey)); // 无需再做压缩处理，经过测试压缩不了多少
            privateKey = Convert.ToBase64String(encoding.GetBytes(privateKey));
            return new KeyValuePair<string, string>(publicKey, privateKey);
        }

        /// <summary>使用公钥加密文本</summary>
        /// <param name="txt"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string txt, string publicKey, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            publicKey = encoding.GetString(Convert.FromBase64String(publicKey));

            RSACryptoServiceProvider p = new RSACryptoServiceProvider();
            p.FromXmlString(publicKey);
            byte[] bMsg = encoding.GetBytes(txt);
            byte[] bEnc = p.Encrypt(bMsg, false);
            return System.Convert.ToBase64String(bEnc);
        }

        /// <summary>使用私钥解密文本</summary>
        /// <param name="txt"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSADecrypt(string txt, string privateKey, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            privateKey = encoding.GetString(Convert.FromBase64String(privateKey));

            RSACryptoServiceProvider p = new RSACryptoServiceProvider();
            p.FromXmlString(privateKey);
            byte[] bEnc = System.Convert.FromBase64String(txt);
            byte[] bMsg = p.Decrypt(bEnc, false);
            return encoding.GetString(bMsg);
        }


        ///-------------------------------------------------------------------------
        /// 文件比较
        ///-------------------------------------------------------------------------
        /// <summary>获取文件的Md5散列值</summary>
        public static string FileHash(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buffer = null;
                BinaryReader reader = new BinaryReader(stream);
                MD5 md5serv = MD5CryptoServiceProvider.Create();
                buffer = md5serv.ComputeHash(stream);
                string result = BitConverter.ToString(buffer);
                reader.Close();
                return result;
            }
        }

        /// <summary>使用md5散列值来比较两个文件是否相同。请自行捕捉异常。</summary>
        /// <param name="srcFilename"></param>
        /// <param name="destFilename"></param>
        /// <returns></returns>
        public static bool FileCompare(string srcFilename, string destFilename)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            FileInfo srcFile = new FileInfo(srcFilename);
            FileInfo destFile = new FileInfo(destFilename);
            byte[] srcHash = md5.ComputeHash(srcFile.OpenRead());
            byte[] destHash = md5.ComputeHash(destFile.OpenRead());
            if (srcHash.Length != destHash.Length)
                return false;
            for (int i = 0; i < srcHash.Length; i++)
            {
                if (srcHash[i] != destHash[i])
                    return false;
            }
            return true;
        }

       

    }
}
