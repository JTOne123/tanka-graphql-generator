using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public static class ConfigureServices
    {
        public static void AddQueryController<T>(this IServiceCollection services) where T : class, IQueryController
        {
            services.AddScoped<IQueryController, T>();
        }
    }
}