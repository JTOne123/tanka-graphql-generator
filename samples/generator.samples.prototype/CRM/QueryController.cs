using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public class QueryController : IQueryController
    {
        public ValueTask<IResolveResult> Contact(ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<IResolveResult> Contacts(ResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContactController: ContactControllerBase<Contact>
    {
        public override ValueTask<string> FirstName(Contact parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<string> LastName(Contact parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<Address> HomeAddress(Contact parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<Address> OfficeAddress(Contact parent, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}