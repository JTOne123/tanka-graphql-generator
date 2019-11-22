using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace generator.integration.tests
{
    public abstract class ArgumentsTestObjectController : ArgumentsTestObjectControllerBase<ArgumentsTestObject>
    {

    }

    public class GeneratedObjectControllerArgumentsFacts
    {
        private ArgumentsTestObjectController _sut;

        public GeneratedObjectControllerArgumentsFacts()
        {
            _sut = Substitute.ForPartsOf<ArgumentsTestObjectController>();
        }

        private IResolverContext CreateContext(
            object? objectValue
        )
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);
            
            return context;
        }

        [Fact]
        public async Task Single_Scalar_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.GetArgument<int?>("arg").Returns(1);

            /* When */
            await _sut.Int(context);

            /* Then */
            await _sut.Received().Int(objectValue, 1, context);
        }
    }
}