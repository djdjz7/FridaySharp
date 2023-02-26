using System.Runtime.CompilerServices;

namespace FridaySharp
{
    public static class FridayExceptions
    {
        public static Exception ClientNotLoggedInException = new Exception("This client is not currently logged in.");
        
    }
}
