using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public class QueryController : QueryControllerBase<Query>
    {
        public override ValueTask<Contact> Contact(Query objectValue, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<IEnumerable<Contact>> Contacts(Query objectValue, ResolverContext context)
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