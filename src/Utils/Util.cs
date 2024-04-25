using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Src.Utils
{
    public static class Util
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
                
                // Salt create
                byte[] salt = Encoding.UTF8.GetBytes(configuration["Security:PasswordSalt"]);

                // Password + salt 
                byte[] combinedBytes = new byte[salt.Length + Encoding.UTF8.GetBytes(password).Length];
                Array.Copy(salt, combinedBytes, salt.Length);
                Array.Copy(Encoding.UTF8.GetBytes(password), 0, combinedBytes, salt.Length, Encoding.UTF8.GetBytes(password).Length);

                // hash pass
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);

                // Hash convert string
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}