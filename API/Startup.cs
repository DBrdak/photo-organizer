using API.Data;
using API.Data.Repositories;
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

        services.AddHttpClient<N8NClient>(
            (_, client) =>
            {
                client.DefaultRequestHeaders.Add("API_KEY", $"{configuration["n8n:key"]}");

                client.BaseAddress = new Uri("https://dbrdak.app.n8n.cloud/webhook/photos");
            });

        services.AddScoped<WorkflowsService>();
        services.AddScoped<DataContext>();
        services.AddScoped<AlbumsRepository>();
    }
}