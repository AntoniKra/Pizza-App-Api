using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace PizzaApp.Services
{
    public class GoogleFileService : IFileService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GoogleFileService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _bucketName = configuration["GcpSettings:BucketName"]
                          ?? throw new ArgumentNullException("Brak nazwy bucketa w appsettings!");

            var keyFileName = configuration["GcpSettings:AuthFileName"]
                              ?? throw new ArgumentNullException("Brak nazwy pliku klucza w appsettings!");

            var keyFilePath = Path.Combine(env.ContentRootPath, keyFileName);

#pragma warning disable CS0618 
            using var stream = File.OpenRead(keyFilePath);
            _storageClient = StorageClient.Create(GoogleCredential.FromStream(stream));
#pragma warning restore CS0618
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Plik jest pusty.");

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var objectName = $"{folderName}/{uniqueFileName}";

            using var stream = file.OpenReadStream();
            await _storageClient.UploadObjectAsync(
                _bucketName,
                objectName,
                file.ContentType,
                stream
            );

            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var uri = new Uri(fileUrl);
            var pathSegments = uri.Segments;
            var objectName = string.Join("", pathSegments.Skip(2)).TrimStart('/');

            try
            {
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            }
            catch (Google.GoogleApiException ex) when (ex.Error.Code == 404)
            {
                // Jak pliku nie ma, to uznajemy, że sukces (i tak chcieliśmy go usunąć)
            }
        }
    }
}