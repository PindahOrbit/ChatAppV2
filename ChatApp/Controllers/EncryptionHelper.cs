using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper
{
    private const string Key = "MySuperSecretKey1234"; // Replace with your secret key

    public static string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = GenerateKeyFromPassword(Key, aesAlg.KeySize / 8);
            aesAlg.GenerateIV(); // Generate a random IV
            byte[] iv = aesAlg.IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            byte[] encryptedBytes;
            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }
            }

            // Combine IV and encrypted data into a single string
            byte[] ivAndEncryptedBytes = new byte[iv.Length + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, ivAndEncryptedBytes, 0, iv.Length);
            Buffer.BlockCopy(encryptedBytes, 0, ivAndEncryptedBytes, iv.Length, encryptedBytes.Length);

            return Convert.ToBase64String(ivAndEncryptedBytes);
        }
    }

    public static string Decrypt(string encryptedText)
    {
        try
        {
            byte[] ivAndEncryptedBytes = Convert.FromBase64String(encryptedText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GenerateKeyFromPassword(Key, aesAlg.KeySize / 8);

                byte[] iv = new byte[aesAlg.BlockSize / 8];
                byte[] encryptedBytes = new byte[ivAndEncryptedBytes.Length - iv.Length];

                Buffer.BlockCopy(ivAndEncryptedBytes, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(ivAndEncryptedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                string decryptedText;
                using (var msDecrypt = new System.IO.MemoryStream(encryptedBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            decryptedText = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return decryptedText;
            }
        }
        catch (Exception)
        {

            return encryptedText;
        }
    
    }

    private static byte[] GenerateKeyFromPassword(string password, int keySize)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            byte[] key = new byte[keySize];
            Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, keySize));
            return key;
        }
    }
}
