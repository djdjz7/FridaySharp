using System.Security.Cryptography;
using System.Text;

namespace FridaySharp
{
    public class AesUtil
    {

        private string key;
        public AesUtil(string AesKey)
        {
            key = string.IsNullOrEmpty(AesKey) ? AesKey : "teEb3gnyru3QCnxv";
        }
        public AesUtil()
        {
            key = "teEb3gnyru3QCnxv";
        }

        public string AesDecrypt(string encryptedStr)
        {
            Aes aes = Aes.Create();
            byte[] bkey = Encoding.UTF8.GetBytes(key);
            byte[] content = Convert.FromBase64String(encryptedStr);
            aes.Mode = CipherMode.ECB;
            aes.Key = bkey;
            aes.Padding = PaddingMode.PKCS7;
            var decryptor = aes.CreateDecryptor();
            byte[] resultArray = decryptor.TransformFinalBlock(content, 0, content.Length);
            decryptor.Dispose();
            return Encoding.UTF8.GetString(resultArray);
        }

        public string AesEncrypt(string decryptedStr)
        {
            Aes aes = Aes.Create();
            byte[] bkey = Encoding.UTF8.GetBytes(key);
            byte[] content = Encoding.UTF8.GetBytes(decryptedStr);
            aes.Mode = CipherMode.ECB;
            aes.Key = bkey;
            aes.Padding = PaddingMode.PKCS7;
            var encryptor = aes.CreateEncryptor();
            byte[] resultArray = encryptor.TransformFinalBlock(content, 0, content.Length);
            encryptor.Dispose();
            return Convert.ToBase64String(resultArray);
        }
    }
}
