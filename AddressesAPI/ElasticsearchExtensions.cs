using System;
using AddressesAPI.V1.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace AddressesAPI
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services)
        {
            var url = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
            var defaultIndex = "address-search";

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton(client);

            CreateIndex(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.
                DefaultMappingFor<Address>(m => m
                    .IndexName("address-search")
                    .IdProperty(m => m.AddressKey)
                );
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            client.Indices.Create(indexName,
                index => index.Map<Address>(x => x.AutoMap())
            );
        }
    }
}
