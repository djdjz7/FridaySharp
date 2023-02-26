using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridaySharp
{
    public static class StringConstants
    {
        public static readonly string LoginUrl = "http://note.func.zykj.org/api/Account/GuestLogin";
        public static readonly string GetOssTokenUrl = "http://note.func.zykj.org/api/Account/GetOssToken";
        public static readonly string AddNoteUrl = "http://note.func.zykj.org/api/Notes/AddOrUpdate";
        public static readonly string GetAllNotesUrl = "http://note.func.zykj.org/api/Notes/GetAll";
        public static readonly string DeleteNoteUrl = "http://note.func.zykj.org/api/Notes/Delete";
    }
}
