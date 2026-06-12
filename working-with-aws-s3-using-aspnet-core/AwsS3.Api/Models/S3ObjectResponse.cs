namespace AwsS3.Api.Models;

// V4: Size is long? because the SDK moved value-type properties to nullables
public sealed record S3ObjectResponse(string Key, long? SizeInBytes, string PresignedUrl);
