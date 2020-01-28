using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Tanka.GraphQL.Generator.Samples.Prototype.CRM;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Samples.Prototype
{
    public class SchemaFactory
    {
        public static async ValueTask<ISchema> Create(IMemoryCache cache)
        {
            var crm = await cache.GetOrCreateAsync("CRM", entry =>
            {
                var sdl = LoadSchemaFileFromEmbeddedResource();
                var schema = new SchemaBuilder()
                    .Sdl(sdl)
                    .UseResolversAndSubscribers(new CRMResolvers())
                    .Build();

                return Task.FromResult(schema);
            });

            return crm;
        }

        private static string LoadSchemaFileFromEmbeddedResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream("Tanka.GraphQL.Generator.Samples.Prototype.CRM.CRM.graphql")
                ?? throw new InvalidOperationException("Could not load resource");

            using var reader = new StreamReader(resourceStream , Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}