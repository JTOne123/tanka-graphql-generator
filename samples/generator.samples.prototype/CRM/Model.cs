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
    public static class Loader
    {
        public static SchemaBuilder Load()
        {
            //todo: should the SDL be injected here as string from the source file?
            //todo: should the SDL be loaded from source file here?
            return new SchemaBuilder()
                .Sdl(
                    @"
type Contact {
    id: ID!
    firstName: String!
    lastName: String!
    address: Address
}

type Address {
    city: String!
    country: String!
}

type Query {
    contact(id: ID!): Contact
}

schema {
    query: Query
}
");
        }
    }

    public static class ConfigureServices
    {
        public static void AddQueryResolvers<T>(this IServiceCollection services) where T: class, IQueryResolvers
        {
            services.AddTransient<IQueryResolvers, T>();
            services.AddTankaServerExecutionContextExtension<IQueryResolvers>();
        }
    }

    public class SchemaResolvers : ObjectTypeMap
    {
        public SchemaResolvers()
        {
            this["Query"] = new FieldResolversMap()
            {
                {"contact", context => context.Use<IQueryResolvers>().Contact(context)}
            };
        }
    }


    public interface IQueryResolvers
    {
        ValueTask<IResolveResult> Contact(ResolverContext context);
    }
    
    public abstract class QueryResolvers<TObjectType>: IQueryResolvers
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
