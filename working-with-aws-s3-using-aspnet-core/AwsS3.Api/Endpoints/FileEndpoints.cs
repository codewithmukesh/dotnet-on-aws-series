using Amazon.S3;
using Amazon.S3.Model;
using AwsS3.Api.Models;

namespace AwsS3.Api.Endpoints;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/buckets/{bucketName}/files").WithTags("Files");

        group.MapPost("/", async (string bucketName, IFormFile file, string? prefix, IAmazonS3 s3, CancellationToken ct) =>
        {
            var key = string.IsNullOrWhiteSpace(prefix)
                ? file.FileName
                : $"{prefix.TrimEnd('/')}/{file.FileName}";

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };

            await s3.PutObjectAsync(request, ct);
            return Results.Created($"/buckets/{bucketName}/files/{key}", key);
        }).DisableAntiforgery();

        group.MapGet("/", async (string bucketName, string? prefix, IAmazonS3 s3, CancellationToken ct) =>
        {
            var response = await s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            }, ct);

            // V4: S3Objects is null when the bucket has no matching objects
            var objects = response.S3Objects ?? [];

            var files = new List<S3ObjectResponse>();
            foreach (var s3Object in objects)
            {
                var presignedUrl = await s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = s3Object.Key,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                });

                files.Add(new S3ObjectResponse(s3Object.Key, s3Object.Size, presignedUrl));
            }

            return Results.Ok(files);
        });

        group.MapGet("/{*key}", async (string bucketName, string key, IAmazonS3 s3, CancellationToken ct) =>
        {
            var response = await s3.GetObjectAsync(bucketName, key, ct);

            // Streams straight from S3 to the client. No MemoryStream, no buffering.
            return Results.Stream(response.ResponseStream, response.Headers.ContentType, Path.GetFileName(key));
        });

        group.MapDelete("/{*key}", async (string bucketName, string key, IAmazonS3 s3, CancellationToken ct) =>
        {
            await s3.DeleteObjectAsync(bucketName, key, ct);
            return Results.NoContent();
        });

        return app;
    }
}
