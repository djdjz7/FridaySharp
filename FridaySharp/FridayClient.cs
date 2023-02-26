using static FridaySharp.FridayTypes;
namespace FridaySharp
{
    public class FridayClient
    {
        private AesUtil aesUtil;
        public FridayClient()
        {
            aesUtil = new AesUtil();
        }
        public FridayClient(string CustomAesKey)
        {
            aesUtil = new AesUtil(CustomAesKey);
        }
        
    }
}
