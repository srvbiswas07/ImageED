using SQLite;
namespace ImageED.Models
{
    public class ImageEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public byte[] EncryptedData { get; set; }

        [NotNull]
        public byte[] IV { get; set; }  // Initialization Vector for encryption

        [NotNull]
        public byte[] Salt { get; set; }  // Salt for password-based key derivation

        [NotNull]
        public DateTime CreatedAt { get; set; }

        // Optional: You might want to add these properties
        public string Description { get; set; }
        public long FileSize { get; set; }
        public string OriginalFileName { get; set; }
    }
}
