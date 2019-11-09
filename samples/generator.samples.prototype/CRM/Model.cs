using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public static class ConfigureServices
    {
        public static void AddQueryResolvers<T>(this IServiceCollection services) where T: class, IQueryController
        {
            services.AddTransient<IQueryController, T>();
            services.AddTankaServerExecutionContextExtension<IQueryController>();
        }
    }

    public class SchemaResolvers : ObjectTypeMap
    {
        public SchemaResolvers()
        {
            this["Query"] = new FieldResolversMap()
            {
                {"contact", context => context.Use<IQueryController>().Contact(context)}
            };
        }
    }


    public abstract class QueryResolvers<TObjectType> : IQueryController
    {
       
        public virtual async ValueTask<IResolveResult> Contact(ResolverContext context)
        {
            var parent = (TObjectType)context.ObjectValue;
            var id = context.GetArgument<string>("id");
            var contact = await Contact(parent, id);

            return Resolve.As(contact);
        }

        protected abstract Task<object> Contact(TObjectType parent, string id);
    }

    public class QueryResolvers : QueryResolvers<Query>
    {
        protected override Task<object> Contact(Query parent, string id)
        {
            return null;
        }
    }

    public class Query
    {

    }

    public abstract class ContactResolver<TObjectType>
    {
        public virtual ValueTask<IResolveResult> FirstName(ResolverContext context)
        {
            return ResolveSync.As(FirstName((TObjectType)context.ObjectValue));
        }

        public abstract string FirstName(TObjectType contact);
    }
}
