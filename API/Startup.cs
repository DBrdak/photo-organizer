using API.Data;
using API.Data.Repositories;
using API.Files;
using API.Files.OneDrive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services.AddScoped<FileService>();
        services.AddScoped<DataContext>();
        services.AddScoped<AlbumsRepository>();
        services.ConfigureOptions<OneDriveSettingsSetup>();
    }
}