using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
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