using System.Security.Cryptography;

namespace ImageED.Helpers
{
    public class EncryptionHelper
    {
        // Key size: 256 bits (32 bytes)
        private const int KeySize = 256;
        // Salt size: 32 bytes
        private const int SaltSize = 32;
        // Number of iterations for key derivation
        private const int Iterations = 50000;

        public static (byte[] encryptedData, byte[] iv, byte[] salt) EncryptImage(byte[] imageData, string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate key from password using PBKDF2
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(KeySize / 8);  // Convert bits to bytes

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    // Generate a new IV for each encryption
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
            // Regenerate the key using the same password and salt
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
