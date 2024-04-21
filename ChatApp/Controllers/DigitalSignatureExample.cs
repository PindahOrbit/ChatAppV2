using System;
using System.Security.Cryptography;
using System.Text;

public class DigitalSignatureExample
{
    // Generate key pairs (sender's private key and public key)
    private static RSAParameters senderPrivateKey;
    private static RSAParameters senderPublicKey;

    static DigitalSignatureExample()
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            senderPrivateKey = rsa.ExportParameters(true); // Export private key
            senderPublicKey = rsa.ExportParameters(false); // Export public key
        }
    }

    // Sender signs the message with the private key
    public static byte[] SignMessage(string message)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(senderPrivateKey); // Import private key

            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return signature;
        }
    }

    // Receiver verifies the signature with the public key
    public static bool VerifySignature(string message, byte[] signature)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(senderPublicKey); // Import public key

            byte[] data = Encoding.UTF8.GetBytes(message);
            bool isValid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return isValid;
        }
    }

}
