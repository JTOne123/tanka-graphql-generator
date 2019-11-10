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
}