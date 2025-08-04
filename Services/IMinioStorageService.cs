using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace api.minionStorage.Services
{
    public class MinioStorage : IMinioStorage
    {
        private readonly MinioClient _minioClient;

        public MinioStorage(string endpoint, string accessKey, string secretKey)
        {
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build() as MinioClient;
        }

        public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream data, string type)
        {
            try
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(data)
                    .WithObjectSize(data.Length)
                    .WithContentType(type));

                string url = await GetUrl(bucketName, objectName);
                return url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao fazer o upload: {ex.Message}", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)));
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (MinioException ex)
            {
                throw new Exception($"Erro ao fazer o downloading: {ex.Message}", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string bucketName, string objectName)
        {
            try
            {
                await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName));
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao verificar o arquivo: {ex.Message}", ex);
            }
        }

        public Task<string> GetUrl(string bucketName, string objectName)
        {
            string url = $"https://apis-minio.w4dxlp.easypanel.host/{bucketName}/{objectName}";
            return Task.FromResult(url);
        }
    }
}
