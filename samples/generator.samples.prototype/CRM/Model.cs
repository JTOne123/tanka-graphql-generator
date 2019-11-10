using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

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

    public class SchemaResolvers : ObjectTypeMap
    {
        public SchemaResolvers()
        {
            this["Query"] = new QueryResolverMap();
        }
    }

    public class QueryResolverMap : FieldResolversMap
    {
        public QueryResolverMap()
        {
            Add("field1", context => context.Use<IQueryController>().Contact(context));
            Add("field2", context => context.Use<IQueryController>().Contact(context));
        }
    }


    public class QueryController : QueryControllerBase<Query>
    {
        public override ValueTask<object> Contact(Query parent, ResolverContext context)
        {
            return default;
        }
    }

    public class Query
    {
    }
}