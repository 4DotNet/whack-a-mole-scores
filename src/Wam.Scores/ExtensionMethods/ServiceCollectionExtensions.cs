using Microsoft.Extensions.DependencyInjection;
using Wam.Scores.Repositories;
using Wam.Scores.Services;

namespace Wam.Scores.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWamScoresModel(this IServiceCollection services)
    {
        services.AddTransient<IScoresService, ScoresService>();
        services.AddTransient<IScoresRepository, ScoresRepository>();
        return services;
    }
}