using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.TypeSystem;
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
        public async Task Single_Scalar_Int_argument()
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

        [Fact]
        public async Task Single_Scalar_String_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.GetArgument<string?>("arg").Returns("hello");
            
            /* When */
            await _sut.String(context);

            /* Then */
            await _sut.Received().String(objectValue, "hello", context);
        }

        [Fact]
        public async Task Single_Scalar_Float_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.GetArgument<float?>("arg").Returns(1.123f);
            
            /* When */
            await _sut.Float(context);

            /* Then */
            await _sut.Received().Float(objectValue, 1.123f, context);
        }

        [Fact]
        public async Task Single_Scalar_Boolean_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.GetArgument<bool?>("arg").Returns(true);
            
            /* When */
            await _sut.Boolean(context);

            /* Then */
            await _sut.Received().Boolean(objectValue, true, context);
        }
    }
}