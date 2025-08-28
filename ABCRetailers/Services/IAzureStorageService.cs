using Azure.Data.Tables;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;
using Azure.Storage.Files.Shares.Models;

namespace ABCRetailers.Services
{
    // A simple file-based implementation that requires no Azure account.
    // Entities are stored as JSON arrays per type under App_Data/{Type}.json
    // "Blob", "Queue", and "FileShare" operations map to local folders for demo purposes.
    public interface IAzureStorageService
    {
        // Initialization is a no-op for local implementation.
        Task InitializeStorageAsync();

        // Table-like operations
        Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new();
        Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task AddEntityAsync<T>(T entity) where T : class, ITableEntity, new();
        Task UpdateEntityAsync<T>(T entity) where T : class, ITableEntity, new();
        Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();

        // "Blob" operations
        Task<string> UploadImageAsync(Stream stream, string fileName, string contentType);

        // "Queue" operation (writes to a local log)
        Task SendMessageAsync(string message);

        // "File share" operations
        Task<string> UploadToFileShareAsync(Stream stream, string fileName);
        Task<byte[]> DownloadFromFileShareAsync(string fileName);
    }

    public class AzureStorageService : IAzureStorageService
    {
        private readonly string _dataRoot;
        private readonly string _uploadsRoot;
        private readonly string _filesRoot;
        private readonly string _queueLog;

        public AzureStorageService(string dataRoot)
        {
            _dataRoot = Path.Combine(dataRoot, "tables");
            _uploadsRoot = Path.Combine(dataRoot, "uploads");
            _filesRoot = Path.Combine(dataRoot, "fileshare");
            Directory.CreateDirectory(_dataRoot);
            Directory.CreateDirectory(_uploadsRoot);
            Directory.CreateDirectory(_filesRoot);
            _queueLog = Path.Combine(dataRoot, "queue.log");
        }

        public Task InitializeStorageAsync() => Task.CompletedTask;

        private static string FileNameFor<T>() => $"{typeof(T).Name}.json";

        private string PathFor<T>() => Path.Combine(_dataRoot, FileNameFor<T>());

        private static List<T> CloneList<T>(IEnumerable<T> items) =>
            System.Text.Json.JsonSerializer.Deserialize<List<T>>(
                System.Text.Json.JsonSerializer.Serialize(items,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = false })
            ) ?? new List<T>();

        private async Task<List<T>> LoadAsync<T>() where T : class, ITableEntity, new()
        {
            var path = PathFor<T>();
            if (!File.Exists(path)) return new List<T>();
            using var fs = File.OpenRead(path);
            var list = await System.Text.Json.JsonSerializer.DeserializeAsync<List<T>>(fs) ?? new List<T>();
            return list;
        }

        private async Task SaveAsync<T>(IEnumerable<T> entities) where T : class, ITableEntity, new()
        {
            var path = PathFor<T>();
            await using var fs = File.Create(path);
            await System.Text.Json.JsonSerializer.SerializeAsync(fs, entities,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new()
        {
            return await LoadAsync<T>();
        }

        public async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var list = await LoadAsync<T>();
            return list.FirstOrDefault(e => e.PartitionKey == partitionKey && e.RowKey == rowKey);
        }

        public async Task AddEntityAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var list = await LoadAsync<T>();
            // ensure keys
            if (string.IsNullOrWhiteSpace(entity.PartitionKey))
                entity.PartitionKey = typeof(T).Name;
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();
            entity.Timestamp = DateTimeOffset.UtcNow;
            list.Add(entity);
            await SaveAsync(list);
        }

        public async Task UpdateEntityAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var list = await LoadAsync<T>();
            var idx = list.FindIndex(e => e.PartitionKey == entity.PartitionKey && e.RowKey == entity.RowKey);
            if (idx >= 0)
            {
                entity.Timestamp = DateTimeOffset.UtcNow;
                list[idx] = entity;
                await SaveAsync(list);
            }
            else
            {
                await AddEntityAsync(entity);
            }
        }

        public async Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var list = await LoadAsync<T>();
            list = list.Where(e => !(e.PartitionKey == partitionKey && e.RowKey == rowKey)).ToList();
            await SaveAsync(list);
        }

        public async Task<string> UploadImageAsync(Stream stream, string fileName, string contentType)
        {
            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
            var path = Path.Combine(_uploadsRoot, safeName);
            await using (var fs = File.Create(path))
            {
                await stream.CopyToAsync(fs);
            }
            // Return a relative path usable by the app. We'll expose /media route in controller if needed; for now return file:// path relative to app.
            return $"/media/{safeName}";
        }

        public async Task SendMessageAsync(string message)
        {
            var line = $"{DateTime.UtcNow:O}\t{message}{Environment.NewLine}";
            await File.AppendAllTextAsync(_queueLog, line);
        }

        public async Task<string> UploadToFileShareAsync(Stream stream, string fileName)
        {
            var safeName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Path.GetFileName(fileName)}";
            var path = Path.Combine(_filesRoot, safeName);
            await using (var fs = File.Create(path))
            {
                await stream.CopyToAsync(fs);
            }
            return safeName;
        }

        public async Task<byte[]> DownloadFromFileShareAsync(string fileName)
        {
            var path = Path.Combine(_filesRoot, fileName);
            if (!File.Exists(path)) throw new FileNotFoundException(fileName);
            return await File.ReadAllBytesAsync(path);
        }
    }
}