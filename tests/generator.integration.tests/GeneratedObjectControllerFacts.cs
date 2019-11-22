﻿using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace generator.integration.tests
{
    public class ObjectController : TestObjectControllerBase<TestObject>
    {
    }

    public class GeneratedObjectControllerFacts
    {
        public GeneratedObjectControllerFacts()
        {
            _sut = new ObjectController();
        }

        private readonly ObjectController _sut;

        private IResolverContext CreateContext(object? objectValue)
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);
            return context;
        }

        [Fact]
        public async Task NonNull_Property_field_with_null_objectValue()
        {
            /* Given */
            var context = CreateContext(null);

            /* When */
            var result = await _sut.NonNullInt(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task NonNull_Property_field()
        {
            /* Given */
            var context = CreateContext(new TestObject
            {
                NonNullInt = 1
            });

            /* When */
            var result = await _sut.NonNullInt(context);

            /* Then */
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_null_objectValue()
        {
            /* Given */
            var context = CreateContext(null);

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_null()
        {
            /* Given */
            var context = CreateContext(new TestObject
            {
                Int = null
            });

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_value()
        {
            /* Given */
            var context = CreateContext(new TestObject
            {
                Int = 1
            });

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Equal(1, result.Value);
        }
    }
}