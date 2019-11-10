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

    public class QueryController : IQueryController
    {
        public ValueTask<IResolveResult> Contact(ResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AddressController : AddressControllerBase<Address>
    {
        public override ValueTask<object> City(Address parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<object> Country(Address parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<object> Street(Address parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Address
    {

    }
}