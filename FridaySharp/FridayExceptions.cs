using System.Runtime.CompilerServices;

namespace FridaySharp
{
    public static class FridayExceptions
    {
        public static Exception ClientNotLoggedInException = new Exception("This client is not currently logged in.");
        public static Exception InvalidFileNameException = new Exception("The file name or folder name must not be an empty string.");
    }
}
