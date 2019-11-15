using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
    public partial class Contact
    {
        public string FullName => $"{FirstName} {LastName}";
    }

    public class ContactController : ContactControllerBase<Contact>
    {
        public override ValueTask<string> FullName(Contact objectValue, ResolverContext context)
        {
            return new ValueTask<string>(objectValue.FullName);
        }

        public override ValueTask<Address> OfficeAddress(Contact objectValue, ResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}