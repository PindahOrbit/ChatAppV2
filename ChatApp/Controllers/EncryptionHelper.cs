using System.Text;

namespace ChatApp.Controllers
{
    using ChatApp.Models;
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class EncryptionHelper
    {

       public static byte[] Encrypt(string plainText, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                return rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), false);
            }
        }

        public static string Decrypt(byte[] encryptedText, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                return Encoding.UTF8.GetString(rsa.Decrypt(encryptedText, false));
            }
        }

        public static byte[] SignData(byte[] data, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                return rsa.SignData(data, new SHA256CryptoServiceProvider());
            }
        }
        public static bool VerifySignature(string messageText, byte[] signature, string publicKey)
        {
            byte[] messageBytes = Convert.FromBase64String(messageText);

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                return rsa.VerifyData(messageBytes, new SHA256CryptoServiceProvider(), signature);
            }
        }



    }

}
