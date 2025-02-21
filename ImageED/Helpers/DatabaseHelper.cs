using ImageED.Models;
using SQLite;

namespace ImageED.Helpers
{
    public class DatabaseHelper
    {
        private SQLiteAsyncConnection _db;

        private async Task Init()
        {
            if (_db != null) return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "images.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<ImageEntry>();
        }

        public async Task<List<ImageEntry>> GetAllImagesAsync()
        {
            await Init();
            return await _db.Table<ImageEntry>().ToListAsync();
        }

        public async Task<int> SaveImageAsync(ImageEntry entry)
        {
            await Init();
            return await _db.InsertAsync(entry);
        }

        public async Task<ImageEntry> GetImageAsync(int id)
        {
            await Init();
            return await _db.GetAsync<ImageEntry>(id);
        }


        public async Task<int> DeleteImageAsync(int id)
        {
            await Init();
            return await _db.DeleteAsync<ImageEntry>(id);
        }
    }
}
