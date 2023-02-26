namespace FridaySharp
{
    public static class FridayTypes
    {
        public class LoginData
        {
            public string clientCode { get; set; } = "";
            public string password { get; set; }
            public string schoolCode { get; set; } = "sxz";
            public string smsVerificationCode { get; set; } = "";
            public string userName { get; set; }
        }

        public class CommonResponseData
        {
            public int code { get; set; }
            public string msg { get; set; }
            public string data { get; set; }
        }

        public class UserInfo
        {
            public string token { get; set; }
            public int userId { get; set; }
            public string realName { get; set; }
            public int roleType { get; set; }
            public string ezyServer { get; set; }
            public object mobile { get; set; }
            public object clientCode { get; set; }
            public string accessKeyId { get; set; }
            public string accessKeySecret { get; set; }
            public string securityToken { get; set; }
        }

        public class OssAccessResponseData
        {
            public string accessKeyId { get; set; }
            public string accessKeySecret { get; set; }
            public string securityToken { get; set; }
            public string requestId { get; set; }

        }

        public class AddFileData
        {
            public string fileId { get; set; }
            public string fileName { get; set; }
            public string fileUrl { get; set; }
            public string parentId { get; set; }
            public int type { get; set; }
        }

        public class GetAllNotesResponseData
        {
            public int userId { get; set; }
            public string schoolCode { get; set; }
            public int totalCount { get; set; }
            public NoteInfo[] noteList { get; set; }
        }

        public class NoteInfo
        {
            public string fileId { get; set; }
            public string fileName { get; set; }
            public string fileUrl { get; set; }
            public string parentId { get; set; }
            public int type { get; set; }
            public string createTime { get; set; }
            public string updateTime { get; set; }
            public int version { get; set; }
            public bool shared { get; set; }
        }
        public class GetNoteFileListResponse
        {
            public string code { get; set; }
            public int userId { get; set; }
            public int totalCount { get; set; }
            public Resourcelist[] resourceList { get; set; }
        }

        public class Resourcelist
        {
            public string id { get; set; }
            public string fileId { get; set; }
            public string pageName { get; set; }
            public int pageIndex { get; set; }
            public string md5 { get; set; }
            public int resourceType { get; set; }
            public string ossImageUrl { get; set; }
            public string createTimeStamp { get; set; }
            public string updateTimeStamp { get; set; }
        }
    }
}
