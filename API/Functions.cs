using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Web;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using API.Files;
using API.Requests;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace API;

public class Functions
{
    private readonly FileService _fileService;

    public Functions(FileService fileService)
    {
        _fileService = fileService;
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/photos/new")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> UploadPhotoAsync(
        [FromQuery] string name,
        [FromQuery] string extension,
        [FromBody] string data,
        APIGatewayHttpApiV2ProxyRequest apiRequest,
        ILambdaContext context)
    {
        var file = apiRequest.Body;

        var request = new UploadPhotoRequest(name, extension, data);

        try
        {
            var result = await _fileService.UploadPhotoAsync(request);

            switch (result.IsSuccess)
            {
                case true:
                    context.Logger.LogLine($"Successfully uploaded {request.Name}");
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = $"Successfully uploaded {request.Name}"
                    };
                case false:
                    context.Logger.LogLine($"Failed to upload {request.Name}, error: {result.Error}");
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 500,
                        Body = $"Failed to upload {request.Name}"
                    };
            }
        }
        catch (Exception e)
        {
            context.Logger.LogLine($"An error occured when tried to upload a photo: {e.Message}");

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to upload {request.Name}"
            };
        }
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, $"photos/album/new")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> SetAlbumAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var photo = JsonConvert.SerializeObject(request);

        context.Logger.LogLine($"Setting album: {photo}");

        //await _workflowsService.SetAlbumAsync();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Album set successfully",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, $"photos/album/reset")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> ResetAlbumAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var photo = JsonConvert.SerializeObject(request);

        context.Logger.LogLine($"Resetting album: {photo}");

        //await _workflowsService.ResetAlbumAsync();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Album reset successfully",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }
}