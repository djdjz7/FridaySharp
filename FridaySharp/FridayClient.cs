using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Aliyun.OSS.Model.LiveChannelStat;
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
        }

        public FridayClient(string School, string Account, string Password, string CustomAesKey)
        {
            aesUtil = new AesUtil(CustomAesKey);
            school = School;
            account = Account;
            password = Password;
        }

        public async Task LoginAsync()
        {
            LoginData loginData = new LoginData()
            {
                schoolCode = school,
                userName = account,
                password = password,
            };
            string requestData = loginData.JsonSerialize().AesEncrypt();
            HttpContent content = new StringContent(requestData);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var responseString = await httpClient.PostAsync(LoginUrl, content).Result.Content.ReadAsStringAsync();
            CommonResponseData responseData = responseString.JsonDeserialize<CommonResponseData>() ?? new CommonResponseData();
            if (responseData.msg == "操作成功")
            {
                userInfo = responseData.data.AesDecrypt().JsonDeserialize<UserInfo>() ?? new UserInfo();
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
            string ossResponseString = await (await httpClient.GetAsync(GetOssTokenUrl)).Content.ReadAsStringAsync();
            CommonResponseData ossResponseData = ossResponseString.JsonDeserialize<CommonResponseData>() ?? new CommonResponseData();

            if (ossResponseData.msg == "操作成功")
            {
                string ossAccessDataJson = aesUtil.AesDecrypt(ossResponseData.data);
                OssAccessResponseData? ossAccessData = ossAccessDataJson.JsonDeserialize<OssAccessResponseData>();
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
                throw new Exception($"Error occurred while attempting to refresh oss token.\nResponse data: {ossResponseString}");
            }
        }

        public async Task CreateFolderAsync(string FolderName)
        {
            await CreateFolderAsync(FolderName, "0");
        }
        public async Task CreateFolderAsync(NoteInfo FolderInfo)
        {
            await CreateFolderAsync(FolderInfo.fileName, string.IsNullOrEmpty(FolderInfo.parentId) ? "0" : FolderInfo.parentId);
        }
        public async Task CreateFolderAsync(string FolderName, string ParentID)
        {
            if (string.IsNullOrEmpty(FolderName))
            {
                throw InvalidFileNameException;
            }

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {userInfo.token}");
            string parentId = string.IsNullOrEmpty(ParentID) ? "0" : ParentID;

            AddFileData addFileData = new AddFileData()
            {
                fileName = FolderName,
                fileId = Guid.NewGuid().ToString("N"),
                parentId = parentId,
                fileUrl = "",
                type = 0
            };
            string requestData = JsonSerializer.Serialize(addFileData);
            HttpContent requestContent = new StringContent(aesUtil.AesEncrypt(requestData));
            requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            string response = await (await httpClient.PostAsync(AddNoteUrl, requestContent)).Content.ReadAsStringAsync();
            MinimumResponseData createFolderResponseData = JsonSerializer.Deserialize<MinimumResponseData>(response) ?? new MinimumResponseData();
            if (createFolderResponseData.msg != "操作成功")
                throw new Exception($"Error occurred while attempting to create a folder.\nResponse data: {response}");
        }
        public async Task<NoteInfo[]?> GetAllNotesAsync()
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {userInfo.token}");
            string getAllNotesResponseString = await (await httpClient.GetAsync(GetAllNotesUrl)).Content.ReadAsStringAsync();
            CommonResponseData getAllNotesResponse = getAllNotesResponseString.JsonDeserialize<CommonResponseData>() ?? new CommonResponseData();
            if (getAllNotesResponse.msg == "操作成功")
            {
                string getAllNotesData = getAllNotesResponse.data.AesDecrypt();
                return getAllNotesData.JsonDeserialize<GetAllNotesResponseData>()?.noteList;
            }
            else
                throw new Exception($"Error occurred while attempting to retrieve note list.\nResponse data:{getAllNotesResponseString}");
        }
    }
}
