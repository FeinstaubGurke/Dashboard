using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dashboard.Dtos;
using System.Net;

namespace Dashboard.Services
{
    public class S3ObjectStorageService : IObjectStorageService
    {
        private readonly ILogger<S3ObjectStorageService> _logger;
        private readonly BasicAWSCredentials _credentials;
        private readonly AmazonS3Config _amazonS3Config;
        private readonly string _bucketName;

        public S3ObjectStorageService(
            ILogger<S3ObjectStorageService> logger,
            IConfiguration configuration)
        {
            this._logger = logger;

            var configSection = configuration.GetSection("ObjectStorageService");
            var enpointUrl = configSection.GetValue<string>("EndpointUrl");
            var accessKey = configSection.GetValue<string>("AccessKey");
            var secretKey = configSection.GetValue<string>("SecretKey");
            var bucketName = configSection.GetValue<string>("BucketName") ?? throw new ArgumentException("BucketName missing");

            this._credentials = new BasicAWSCredentials(accessKey, secretKey);
            this._bucketName = bucketName;

            this._amazonS3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.EUCentral1,
                ServiceURL = enpointUrl,
                ForcePathStyle = true
            };
        }

        public async Task<bool> DeleteFilesAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            using var client = new AmazonS3Client(this._credentials, this._amazonS3Config);

            var request = new DeleteObjectsRequest
            {
                BucketName = this._bucketName,
                Objects = keys.Select(o => new KeyVersion { Key = o }).ToList()
            };

            var response = await client.DeleteObjectsAsync(request, cancellationToken);
            return !response.DeleteErrors.Any();
        }

        public async Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            using var client = new AmazonS3Client(this._credentials, this._amazonS3Config);

            try
            {
                await client.GetObjectMetadataAsync(this._bucketName, key, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string key, CancellationToken cancellationToken = default)
        {
            using var client = new AmazonS3Client(this._credentials, this._amazonS3Config);

            var request = new GetObjectRequest
            {
                BucketName = this._bucketName,
                Key = key
            };

            using var memoryStream = new MemoryStream();
            var response = await client.GetObjectAsync(request, cancellationToken);
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

            return memoryStream.ToArray();
        }

        public async Task<FileInfoDto[]> GetFileInfosAsync(string prefix, CancellationToken cancellationToken = default)
        {
            using var client = new AmazonS3Client(this._credentials, this._amazonS3Config);

            var request = new ListObjectsV2Request
            {
                BucketName = this._bucketName,
                Delimiter = "/",
                Prefix = prefix,
            };

            var response = await client.ListObjectsV2Async(request, cancellationToken);
            return response.S3Objects.Select(o => new FileInfoDto { Key = o.Key, LastModified = o.LastModified }).ToArray();
        }

        public async Task<bool> UploadFileAsync(string key, Stream stream, CancellationToken cancellationToken = default)
        {
            using var client = new AmazonS3Client(this._credentials, this._amazonS3Config);

            var request = new PutObjectRequest
            {
                BucketName = this._bucketName,
                Key = key,
                InputStream = stream
            };

            var response = await client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
}
