using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Xunit;
using NSubstitute;

namespace generator.integration.tests
{
    public class QueryController : QueryControllerBase<Query>
    {
    }

    public class GeneratedControllerFacts
    {
        private QueryController _sut;

        public GeneratedControllerFacts()
        {
            _sut = new QueryController();
        }

        [Fact]
        public async Task NonNull_Property_field()
        {
            /* Given */
            var context = CreateContext(new Query()
            {
                NonNullInt = 1
            });

            /* When */
            var result = await _sut.NonNullInt(context);

            /* Then */
            Assert.Equal(1, result.Value);
        }

        private IResolverContext CreateContext(object objectValue)
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);
            return context;
        }
    }
}