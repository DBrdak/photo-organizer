using Functions.Workflows;
using Functions.Workflows.N8n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Functions;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddSystemsManager("/photo-organizer")
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddHttpClient<N8nClient>(
            (_, client) =>
            {
                client.DefaultRequestHeaders.Add("API_KEY", $"{configuration["n8n:key"]}");

                client.BaseAddress = new Uri("https://dbrdak.app.n8n.cloud/webhook/photos");
            });

        services.AddScoped<WorkflowsService>();
    }
}