using Microsoft.Extensions.DependencyInjection;
using PremierLeague.Simulator;
using PremierLeague.Simulator.Features.Fpl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimulatorServices(this IServiceCollection services)
    {
        services.AddTransient<FplService>();
        services.AddTransient<PremierLeagueSimulator>();
       
        return services;
    }
    
    public static IServiceCollection AddExternalHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<FplService>(client =>
        {
            client.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");
        });        
        return services;
    }
}