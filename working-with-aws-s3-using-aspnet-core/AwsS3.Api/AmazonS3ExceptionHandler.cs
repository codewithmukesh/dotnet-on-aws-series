using Amazon.S3;
using Microsoft.AspNetCore.Diagnostics;

namespace AwsS3.Api;

internal sealed class AmazonS3ExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AmazonS3Exception s3Exception)
        {
            return false;
        }

        var statusCode = s3Exception.ErrorCode switch
        {
            "NoSuchBucket" or "NoSuchKey" => StatusCodes.Status404NotFound,
            "AccessDenied" or "InvalidAccessKeyId" or "SignatureDoesNotMatch" => StatusCodes.Status403Forbidden,
            "BucketAlreadyExists" or "BucketAlreadyOwnedByYou" or "BucketNotEmpty" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = statusCode,
                Title = s3Exception.ErrorCode,
                Detail = s3Exception.Message
            }
        });
    }
}
