using Amazon.SimpleSystemsManagement;
using API.Auth;
using API.Auth.Options;
using API.Data;
using API.Data.Repositories;
using API.Files;
using API.Files.OneDrive;
using API.SystemsManager;
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
                client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            });

        services.AddScoped<FileService>();
        services.AddScoped<DataContext>();
        services.AddScoped<AlbumsRepository>();
        services.ConfigureOptions<MicrosoftSettingsSetup>();
        services.AddAWSService<IAmazonSimpleSystemsManagement>();
        services.AddScoped<MicrosoftAuthenticationService>();
        services.AddScoped<SystemsManagerClient>();
        services.AddScoped<MicrosoftTokenCredential>();
    }
}