using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Src.Utils
{
    public static class Util
    {

        private static IConfiguration _configuration;

        // IConfiguration inject
        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static string HashPassword(string password, string saltStr)
        {
            using (var sha256 = SHA256.Create())
            {
                try
                {
                    // Salt create
                    byte[] salt = Encoding.UTF8.GetBytes(saltStr);

                    // Password + salt 
                    byte[] combinedBytes = new byte[salt.Length + Encoding.UTF8.GetBytes(password).Length];
                    Array.Copy(salt, combinedBytes, salt.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(password), 0, combinedBytes, salt.Length, Encoding.UTF8.GetBytes(password).Length);

                    // hash pass
                    byte[] hashBytes = sha256.ComputeHash(combinedBytes);

                    // Hash convert string
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("HashPassword ERR : "+ e.Message);
                    return "";
                }
                
            }
        }
    }
}