using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Infrastructure;
using Dapper;
using Microsoft.Data.SqlClient;
using AddressCrossReference = AddressesAPI.V1.Domain.AddressCrossReference;

namespace AddressesAPI.V1.Gateways
{
    public class AddressesGatewayTSQL : IAddressesGatewayTSQL
    {
        private readonly string _connectionString;
        public AddressesGatewayTSQL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Return an address for a given LPI_Key
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Address> GetSingleAddressAsync(string addressId)
        {
            var query = QueryBuilder.GetSingleAddress(GlobalConstants.Format.Detailed);
            await using var conn = new SqlConnection(_connectionString);
            //open connection explicitly
            conn.Open();
            var all = await conn.QueryAsync<Address>(query,
                new { key = addressId }
            ).ConfigureAwait(false);

            var result = all.FirstOrDefault();

            conn.Close();

            return result;
        }

        /// <summary>
        /// Return Detailed addresses for matching search
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<(List<Address>, int)> SearchAddressesAsync(SearchParameters request)
        {
            List<Address> result;
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var query = QueryBuilder.GetSearchAddressQuery(request, true, true, false, ref dbArgs);
            var countQuery = QueryBuilder.GetSearchAddressQuery(request, false, false, true, ref dbArgs);
            var format = request.Format;

            // ReSharper disable once ConvertToUsingDeclaration
            int totalCount;
            await using (var conn = new SqlConnection(_connectionString))
            {
                //open connection explicitly
                conn.Open();
                var sql = query + " " + countQuery;

                using (var multi = conn.QueryMultipleAsync(sql, dbArgs).Result)
                {

                    if (format == GlobalConstants.Format.Detailed)
                    {
                        var all = multi.Read<Address>()?.ToList();
                        totalCount = multi.Read<int>().Single();
                        result = all?.ToList();
                    }
                    else
                    {
                        var all = multi.Read<SimpleAddress>()?.Select(x => (Address) x).ToList();
                        totalCount = multi.Read<int>().Single();
                        result = all?.ToList();
                    }
                }

                conn.Close();
            }
            return (result, totalCount);
        }


        public async Task<List<AddressCrossReference>> GetAddressCrossReferenceAsync(long uprn)
        {
            var query = QueryBuilder.GetCrossReferences();
            await using var conn = new SqlConnection(_connectionString);
            //open connection explicitly
            conn.Open();
            var all = await conn.QueryAsync<AddressCrossReference>(query,
                new { UPRN = uprn }
            ).ConfigureAwait(false);

            var result = all.ToList();

            conn.Close();

            return result;
        }

        /// <summary>
        /// Return Simple addresses for matching search
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<(List<SimpleAddress>, int)> SearchSimpleAddressesAsync(SearchParameters request)
        {
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var query = QueryBuilder.GetSearchAddressQuery(request, true, true, false, ref dbArgs);
            var countQuery = QueryBuilder.GetSearchAddressQuery(request, false, false, true, ref dbArgs);

            await using var conn = new SqlConnection(_connectionString);
            //open connection explicitly
            conn.Open();
            var sql = query + " " + countQuery;
            List<SimpleAddress> result;
            int totalCount;
            using (var multi = conn.QueryMultipleAsync(sql, dbArgs).Result)
            {
                var all = multi.Read<SimpleAddress>()?.ToList();
                totalCount = multi.Read<int>().Single();
                result = all?.ToList();
            }

            conn.Close();
            return (result, totalCount);
        }





    }
}
