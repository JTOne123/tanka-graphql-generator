using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public class ContactResultsController : ContactResultsControllerBase<ContactResults>
    {
        public override ValueTask<IEnumerable<Contact>> Contact(ContactResults objectValue, IResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}