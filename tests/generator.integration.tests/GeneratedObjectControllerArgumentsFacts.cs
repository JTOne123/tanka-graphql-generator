using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.Generator.Integration.Tests.Model;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace tanka.graphql.generator.integration.tests
{
    public abstract class ArgumentsTestObjectController : ArgumentsTestObjectControllerBase<ArgumentsTestObject>
    {

    }

    public class GeneratedObjectControllerArgumentsFacts
    {
        private readonly ArgumentsTestObjectController _sut;

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
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg"] = 1
            });

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
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg"] = "hello"
            });

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
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg"] = 1.123
            });
            
            /* When */
            await _sut.Float(context);

            /* Then */
            await _sut.Received().Float(objectValue, 1.123, context);
        }

        [Fact]
        public async Task Single_Scalar_Boolean_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg"] = true
            });
            
            /* When */
            await _sut.Boolean(context);

            /* Then */
            await _sut.Received().Boolean(objectValue, true, context);
        }

        [Fact]
        public async Task Single_InputObject_argument()
        {
            /* Given */
            var objectValue = new ArgumentsTestObject();
            var context = CreateContext(objectValue);
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg"] = new Dictionary<string, object>()
                {
                    ["int"] = 1
                }
            });

            /* When */
            await _sut.Input(context);

            /* Then */
            await _sut.Received().Input(objectValue, Arg.Is<TestInputObject>(ti => ti.Int == 1), context);
        }
    }
}