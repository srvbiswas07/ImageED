using System.Security.Cryptography;

namespace ImageED.Helpers
{
    public class EncryptionHelper
    {
        private const int KeySize = 256;
        private const int SaltSize = 32;
        private const int Iterations = 50000;

        public static (byte[] encryptedData, byte[] iv, byte[] salt) EncryptImage(byte[] imageData, string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(KeySize / 8); 

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor())
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(imageData, 0, imageData.Length);
                            csEncrypt.FlushFinalBlock();
                        }

                        return (msEncrypt.ToArray(), aes.IV, salt);
                    }
                }
            }
        }

        public static byte[] DecryptImage(byte[] encryptedData, byte[] iv, byte[] salt, string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(KeySize / 8);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (MemoryStream msDecrypt = new MemoryStream())
                    {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                        {
                            csDecrypt.Write(encryptedData, 0, encryptedData.Length);
                            csDecrypt.FlushFinalBlock();
                        }

                        return msDecrypt.ToArray();
                    }
                }
            }
        }
    }
}
