using Amazon.SimpleSystemsManagement;
using API.Auth;
using API.Data;
using API.Data.Repositories;
using API.Files;
using API.Files.OneDrive;
using API.SystemsManager;
using API.Workflows;
using API.Workflows.N8N;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddSystemsManager("/photo-organizer")
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddHttpClient<OneDriveClient>(
            (_, client) =>
            {
                client.BaseAddress = new Uri("https://graph.microsoft.com");
            });

        services.AddHttpClient<N8NClient>(
            (_, client) =>
            {
                client.DefaultRequestHeaders.Add("API_KEY", $"{configuration["n8n:key"]}");

                client.BaseAddress = new Uri("https://n8n.ec2.dbrdak.com/webhook");
            });

        services.AddScoped<WorkflowsService>();
        services.AddScoped<FileService>();
        services.AddScoped<DataContext>();
        services.AddScoped<AlbumsRepository>();
        services.ConfigureOptions<OneDriveSettingsSetup>();
        services.AddAWSService<IAmazonSimpleSystemsManagement>();
        services.AddScoped<MicrosoftAuthenticationService>();
        services.AddScoped<SystemsManagerClient>();
        services.AddScoped<MicrosoftTokenCredential>();
    }
}