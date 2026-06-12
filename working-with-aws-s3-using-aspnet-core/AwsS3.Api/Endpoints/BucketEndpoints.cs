using Amazon.S3;
using Amazon.S3.Util;

namespace AwsS3.Api.Endpoints;

public static class BucketEndpoints
{
    public static IEndpointRouteBuilder MapBucketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/buckets").WithTags("Buckets");

        group.MapPost("/{bucketName}", async (string bucketName, IAmazonS3 s3, CancellationToken ct) =>
        {
            if (await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName))
            {
                return Results.Conflict($"Bucket {bucketName} already exists.");
            }

            await s3.PutBucketAsync(bucketName, ct);
            return Results.Created($"/buckets/{bucketName}", bucketName);
        });

        group.MapGet("/", async (IAmazonS3 s3, CancellationToken ct) =>
        {
            var response = await s3.ListBucketsAsync(ct);

            // V4: collections are null when empty, not an empty list
            var buckets = response.Buckets?.Select(b => b.BucketName) ?? [];
            return Results.Ok(buckets);
        });

        group.MapDelete("/{bucketName}", async (string bucketName, IAmazonS3 s3, CancellationToken ct) =>
        {
            await s3.DeleteBucketAsync(bucketName, ct);
            return Results.NoContent();
        });

        return app;
    }
}
