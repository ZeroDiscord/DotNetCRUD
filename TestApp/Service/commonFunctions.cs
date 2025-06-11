using System;
using System.Security.Cryptography;
using System.Text;

namespace TestApp.Service
{
    public class commonFunctions
    {
        public string CalculateSHA256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                
                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
