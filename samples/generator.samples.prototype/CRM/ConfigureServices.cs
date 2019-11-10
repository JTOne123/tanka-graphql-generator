using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public static class ConfigureServices
    {
        public static void AddQueryResolvers<T>(this IServiceCollection services) where T : class, IQueryController
        {
            services.AddTransient<IQueryController, T>();
            services.AddTankaServerExecutionContextExtension<IQueryController>();
        }
    }
}