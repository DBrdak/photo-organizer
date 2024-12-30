using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Functions.Workflows;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Functions;

public class Functions
{
    private readonly WorkflowsService _workflowsService;
    private const string basePath = "api/v1/photo";

    public Functions(WorkflowsService workflowsService)
    {
        _workflowsService = workflowsService;
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/new")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> UploadPhotoAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var photo = JsonConvert.SerializeObject(request);

        context.Logger.LogLine($"Uploading photo: {photo}");

        //await _workflowsService.UploadPhotoAsync();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Photo uploaded successfully",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/album/new")]
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
    [HttpApi(LambdaHttpMethod.Post, "/album/reset")]
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