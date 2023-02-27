using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FridaySharp
{
    public static class AesUtilStringExtensions
    {
        public static AesUtil aesUtil = new AesUtil();
        public static string AesEncrypt(this string DecryptedString)
        {
            return aesUtil.AesEncrypt(DecryptedString);
        }
        public static string AesDecrypt(this string EncryptedString)
        {
            return aesUtil.AesDecrypt(EncryptedString);
        }
    }

    public static class JsonUtilStringExtensions
    {
        public static string JsonSerialize(this object? value)
        {
            return JsonSerializer.Serialize(value);
        }
        public static T? JsonDeserialize<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
