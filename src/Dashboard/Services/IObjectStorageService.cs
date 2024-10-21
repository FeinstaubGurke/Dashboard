using Dashboard.Dtos;

namespace Dashboard.Services
{
    public interface IObjectStorageService
    {
        Task<bool> UploadFileAsync(string key, Stream stream, CancellationToken cancellationToken = default);
        Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> DeleteFilesAsync(string[] keys, CancellationToken cancellationToken = default);
        Task<byte[]> GetFileAsync(string key, CancellationToken cancellationToken = default);
        Task<FileInfoDto[]> GetFileInfosAsync(string prefix, CancellationToken cancellationToken = default);
    }
}
