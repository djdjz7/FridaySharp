using System.Net.Http.Headers;
using System.Text.Json;
using static FridaySharp.FridayExceptions;
using static FridaySharp.FridayTypes;
using static FridaySharp.StringConstants;

namespace FridaySharp
{
    public class FridayClient
    {
        private AesUtil aesUtil;
        private string school;
        private string account;
        private string password;
        private bool isUserLoggedIn = false;
        private UserInfo userInfo = new UserInfo();
        public UserInfo UserInfo
        {
            get
            {
                if (isUserLoggedIn && userInfo != null)
                    return userInfo;
                else
                    throw ClientNotLoggedInException;
            }
        }

        private HttpClient httpClient = new HttpClient();

        public FridayClient(string School, string Account, string Password)
        {
            aesUtil = new AesUtil();
            school = School;
            account = Account;
            password = Password;
            _ = LoginAsync();
        }

        public FridayClient(string School, string Account, string Password, string CustomAesKey)
        {
            aesUtil = new AesUtil(CustomAesKey);
            school = School;
            account = Account;
            password = Password;
            _ = LoginAsync();
        }

        public async Task LoginAsync()
        {
            LoginData loginData = new LoginData()
            {
                schoolCode = school,
                userName = account,
                password = password,
            };
            string requestData = aesUtil.AesEncrypt(JsonSerializer.Serialize(loginData));
            HttpContent content = new StringContent(requestData);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.PostAsync(LoginUrl, content).Result.Content.ReadAsStringAsync();
            CommonResponseData responseData = JsonSerializer.Deserialize<CommonResponseData>(response) ?? new CommonResponseData();
            if (responseData.msg == "操作成功")
            {
                userInfo = JsonSerializer.Deserialize<UserInfo>(aesUtil.AesDecrypt(responseData.data)) ?? new UserInfo();
                await RefreshOssTokenAsync();
                isUserLoggedIn = true;
            }
            else
            {
                throw new Exception(responseData.msg);
            }
        }

        public async Task RefreshOssTokenAsync()
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {userInfo.token}");
            string ossResult = await httpClient.GetAsync(GetOssTokenUrl).Result.Content.ReadAsStringAsync();
            CommonResponseData ossResponseData = JsonSerializer.Deserialize<CommonResponseData>(ossResult) ?? new CommonResponseData();

            if (ossResponseData.msg == "操作成功")
            {
                string ossAccessDataJson = aesUtil.AesDecrypt(ossResponseData.data);
                OssAccessResponseData? ossAccessData = JsonSerializer.Deserialize<OssAccessResponseData>(ossAccessDataJson);
                if (ossAccessData != null)
                {
                    userInfo.accessKeyId = ossAccessData.accessKeyId;
                    userInfo.accessKeySecret = ossAccessData.accessKeySecret;
                    userInfo.securityToken = ossAccessData.securityToken;
                }
                else
                    throw new Exception("Internal exception.");
            }
            else
            {
                throw new Exception(ossResponseData.msg);
            }
        }

    }
}
