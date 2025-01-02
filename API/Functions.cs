using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using API.Auth;
using API.Files;
using API.Requests;
using API.Workflows;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace API;

public class Functions
{
    private readonly FileService _fileService;
    private readonly WorkflowsService _workflowsService;
    private readonly MicrosoftAuthenticationService _msAuthService;

    public Functions(
        FileService fileService,
        WorkflowsService workflowsService,
        MicrosoftAuthenticationService msAuthService)
    {
        _fileService = fileService;
        _workflowsService = workflowsService;
        _msAuthService = msAuthService;
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/microsoft/login")]
    public APIGatewayHttpApiV2ProxyResponse MicrosoftLogin(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var authUrl = _msAuthService.GenerateAuthorizationUrl();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 302,
            Headers = new Dictionary<string, string>
            {
                { "Location", authUrl }
            }
        };
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/microsoft/callback")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> MicrosoftCallback(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if (!request.QueryStringParameters?.ContainsKey("code") ?? true)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 400,
                Body = "Authorization code is required"
            };
        }

        var authorizationCode = request.QueryStringParameters["code"];
        await _msAuthService.ExchangeAuthorizationCodeForTokensAsync(authorizationCode);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Authentication successful! You can close this window."
        };
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Put, "/microsoft/tokens/refresh")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> MicrosoftRefreshTokens(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var refreshResult = await _msAuthService.GenerateNewRefreshToken();

        return refreshResult.IsSuccess ?
            new APIGatewayHttpApiV2ProxyResponse { StatusCode = 200 } :
            new APIGatewayHttpApiV2ProxyResponse {StatusCode = 400, Body = refreshResult.Error};
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
            //var result = await _workflowsService.UploadPhotoAsync(request);

            switch (result.IsSuccess)
            {
                case true:
                    context.Logger.LogInformation($"Successfully uploaded {request.Name}");
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = $"Successfully uploaded {request.Name}"
                    };
                case false:
                    context.Logger.LogError(result.Error);
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 400,
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