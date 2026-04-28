using System.Security.Cryptography;
using System.Text;

namespace BlockChain
{
    public class StringUtil
    {
        public static string ApplySha256(string input) =>
            BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
