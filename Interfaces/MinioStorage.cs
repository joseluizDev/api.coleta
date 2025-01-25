public interface IMinioStorage
{
    Task<string> UploadFileAsync(string bucketName, string objectName, Stream data, string type);
    Task<Stream> DownloadFileAsync(string bucketName, string objectName);
    Task<bool> FileExistsAsync(string bucketName, string objectName);
    Task<string> GetUrl(string bucketName, string objectName);
}

