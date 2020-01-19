using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public static class ConfigureServices
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddCrm()
                .AddQueryController<QueryController>();

            services.AddCrm2(options =>
            {
                options.AddQueryController<QueryController>();
            });
        }
    }

    public static class ServiceCollectionExtension
    {
        public static CrmServicesBuilder AddCrm(this IServiceCollection services)
        {
            return new CrmServicesBuilder(services);
        }

        public static IServiceCollection AddCrm2(this IServiceCollection services, Action<CrmServicesBuilder> configure)
        {
            var builder = new CrmServicesBuilder(services);
            configure.Invoke(builder);
            return services;
        }
    }
    
    public class CrmServicesBuilder
    {
        private readonly IServiceCollection _services;

        public CrmServicesBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public CrmServicesBuilder AddQueryController<T>() where T : class, IQueryController
        {
            _services.TryAddScoped<IQueryController, T>();
            return this;
        }

        /*public CrmServicesBuilder AddControllers(Assembly assembly)
        {
            var controllers = assembly.ExportedTypes
                .Where(type => GeneratedControllers.Contains(type.Inter))
            return this;
        }*/
    }
}