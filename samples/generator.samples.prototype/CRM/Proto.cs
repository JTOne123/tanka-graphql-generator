using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype.CRM
{
   public class QueryController: QueryControllerBase<Query>
   {
       public override ValueTask<IContactSearchResult> Search(Query? objectValue, string q, IResolverContext context)
       {
           throw new System.NotImplementedException();
       }
   }
}