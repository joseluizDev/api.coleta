using System.Text;

namespace api.coleta.Tests.Fakes;

public class FakeMinioStorage : IMinioStorage
{
    public List<(string Bucket, string ObjectName, string ContentType)> Uploads { get; } = [];

    public Task<string> UploadFileAsync(string bucketName, string objectName, Stream data, string type)
    {
        Uploads.Add((bucketName, objectName, type));
        return Task.FromResult($"https://fake-minio/{bucketName}/{objectName}");
    }

    public Task<Stream> DownloadFileAsync(string bucketName, string objectName)
    {
        return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
    }

    public Task<bool> FileExistsAsync(string bucketName, string objectName)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetUrl(string bucketName, string objectName)
    {
        return Task.FromResult($"https://fake-minio/{bucketName}/{objectName}");
    }
}
