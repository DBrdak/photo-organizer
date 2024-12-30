using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Web;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using API.Requests;
using API.Workflows;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace API;

public class Functions
{
    private readonly WorkflowsService _workflowsService;

    public Functions(WorkflowsService workflowsService)
    {
        _workflowsService = workflowsService;
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
            await _workflowsService.UploadPhotoAsync(request);
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

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200
        };
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