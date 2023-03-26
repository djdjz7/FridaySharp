using Aliyun.OSS;
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

        private List<NoteInfo> allNotes = new List<NoteInfo>();
        public List<NoteInfo> AllNotes
        {
            get
            {
                if (isUserLoggedIn)
                    return allNotes;
                else
                    throw ClientNotLoggedInException;
            }
        }

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
        public async Task<List<NoteInfo>?> GetAllNotesAsync()
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {userInfo.token}");
            string getAllNotesResponseString = await (await httpClient.GetAsync(GetAllNotesUrl)).Content.ReadAsStringAsync();
            CommonResponseData getAllNotesResponse = getAllNotesResponseString.JsonDeserialize<CommonResponseData>() ?? new CommonResponseData();
            if (getAllNotesResponse.msg == "操作成功")
            {
                string getAllNotesData = getAllNotesResponse.data.AesDecrypt();
                allNotes = new List<NoteInfo>(getAllNotesData.JsonDeserialize<GetAllNotesResponseData>()?.noteList ?? Array.Empty<NoteInfo>());
                return allNotes;
            }
            else
                throw new Exception($"Error occurred while attempting to retrieve note list.\nResponse data:{getAllNotesResponseString}");
        }
        public bool DeleteObjectFromOss(NoteInfo NoteInfo)
        {
            return DeleteObjectFromOss(NoteInfo.fileUrl);
        }
        public bool DeleteObjectFromOss(string NoteFileUrl)
        {
            var bucketName = "friday-note";
            // 创建OSSClient实例。
            var client = new OssClient(
                "https://oss-cn-hangzhou.aliyuncs.com",
                userInfo.accessKeyId,
                userInfo.accessKeySecret,
                userInfo.securityToken);
            try
            {
                var keys = new List<string>();
                keys.Add(NoteFileUrl);
                var request = new DeleteObjectsRequest(bucketName, keys, true);
                var result = client.DeleteObjects(request);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task DeleteNoteAsync(string NoteID)
        {
            string contentString = $"[\"{NoteID}\"]";
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {userInfo.token}");
            HttpContent content = new StringContent(contentString.AesEncrypt());
            content.Headers.ContentType.MediaType = "application/json";
            string responseString = await (await httpClient.PostAsync(DeleteNoteUrl, content)).Content.ReadAsStringAsync();
            var response = responseString.JsonDeserialize<MinimumResponseData>();
            if (response.msg != "操作成功")
                throw new Exception($"Error occurred while attempting to delete a note.\nResponse data: {responseString}");
        }
        public async Task DeleteNoteAsync(NoteInfo NoteInfo)
        {
            await DeleteNoteAsync(NoteInfo.fileId);
        }
    }
}
