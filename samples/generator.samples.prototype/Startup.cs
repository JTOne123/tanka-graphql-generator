using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tanka.GraphQL.Generator.Samples.Prototype.CRM;
using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Generator.Samples.Prototype
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            // add generated controllers
            services.AddCRMControllers()
                .AddQueryController<QueryController>()
                .AddContactController<ContactController>()
                .AddContactResultsController<ContactResultsController>()
                .AddContactSearchResultController<ContactSearchResultController>()
                .AddSearchSuggestionsController<SearchSuggestionsController>();

            // add and configure tanka graphql server
            services.AddTankaGraphQL()
                .ConfigureSchema<IMemoryCache>(SchemaFactory.Create)
                // accepts all connections
                .ConfigureWebSockets();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseWebSockets();

            app.UseTankaGraphQLWebSockets("/graphql/ws");

            app.UseEndpoints(endpoints => { });
        }
    }
}