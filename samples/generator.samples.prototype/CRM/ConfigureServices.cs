using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public static class ConfigureServices
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddProtoControllers();
        }
    }
}