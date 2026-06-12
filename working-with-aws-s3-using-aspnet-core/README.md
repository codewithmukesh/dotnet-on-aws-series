# Working with AWS S3 in ASP.NET Core (.NET 10)

Upload, download, list, and delete files in Amazon S3 from an ASP.NET Core Minimal API, using AWS SDK for .NET V4.

- **Article:** https://codewithmukesh.com/blog/working-with-aws-s3-using-aspnet-core/
- **Stack:** .NET 10, AWSSDK.S3 4.0.24.4, AWSSDK.Extensions.NETCore.Setup 4.0.4.7, Scalar

## Run

```
cd AwsS3.Api
dotnet run
```

Open the Scalar API reference at `/scalar/v1`. Configure your AWS credentials with `aws configure` and set the `AWS:Profile` and `AWS:Region` values in `appsettings.json`.
