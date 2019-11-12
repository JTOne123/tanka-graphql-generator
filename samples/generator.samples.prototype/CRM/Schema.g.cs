using System.Threading.Tasks;
using Tanka.GraphQL;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public interface IContactController
    {
        ValueTask<IResolveResult> FirstName(ResolverContext context);
        ValueTask<IResolveResult> LastName(ResolverContext context);
    }

    public abstract class ContactControllerBase<T> : IContactController
    {
        public virtual async ValueTask<IResolveResult> FirstName(ResolverContext context)
        {
            var parent = (T)context.ObjectValue;
            var result = await FirstName(parent, context);
            return Resolve.As(result);
        }

        public abstract ValueTask<object> FirstName(T parent, ResolverContext context);
        public virtual async ValueTask<IResolveResult> LastName(ResolverContext context)
        {
            var parent = (T)context.ObjectValue;
            var result = await LastName(parent, context);
            return Resolve.As(result);
        }

        public abstract ValueTask<object> LastName(T parent, ResolverContext context);
    }

    public class ContactResolvers : FieldResolversMap
    {
        public ContactResolvers()
        {
            Add("firstName", context => context.Use<IContactController>().FirstName(context));
            Add("lastName", context => context.Use<IContactController>().LastName(context));
        }
    }

    public interface IQueryController
    {
        ValueTask<IResolveResult> Contact(ResolverContext context);
        ValueTask<IResolveResult> Contacts(ResolverContext context);
    }

    public abstract class QueryControllerBase<T> : IQueryController
    {
        public virtual async ValueTask<IResolveResult> Contact(ResolverContext context)
        {
            var parent = (T)context.ObjectValue;
            var result = await Contact(parent, context);
            return Resolve.As(result);
        }

        public abstract ValueTask<object> Contact(T parent, ResolverContext context);
        public virtual async ValueTask<IResolveResult> Contacts(ResolverContext context)
        {
            var parent = (T)context.ObjectValue;
            var result = await Contacts(parent, context);
            return Resolve.As(result);
        }

        public abstract ValueTask<object> Contacts(T parent, ResolverContext context);
    }

    public class QueryResolvers : FieldResolversMap
    {
        public QueryResolvers()
        {
            Add("contact", context => context.Use<IQueryController>().Contact(context));
            Add("contacts", context => context.Use<IQueryController>().Contacts(context));
        }
    }

    public class SchemaResolvers : ObjectTypeMap
    {
        public SchemaResolvers()
        {
            Add("Contact", new ContactResolvers());
            Add("Query", new QueryResolvers());
        }
    }
}