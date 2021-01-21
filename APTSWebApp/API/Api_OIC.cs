using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using APTSWebApp;

namespace APTSWebApp.API
{
    public class Api_OIC
    {
        public string OicConnectionStringMainGroup { get; }
        public string OicConnectionStringMainGroupReserve { get; }
        public string OicConnectionStringReserveGroup { get; }

        private readonly IConfiguration _configuration;

        public Api_OIC(IConfiguration configuration)
        {
            _configuration = configuration;
            OicConnectionStringMainGroup = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringMainGroup").Value;
            OicConnectionStringMainGroupReserve = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringMainGroupReserve").Value;
            OicConnectionStringReserveGroup = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringReserveGroup").Value;
        }
        public async Task<DataRowCollection> GetTsFromOicAsync()
        {
            var typeParams = _configuration
                .GetSection("userSettings")
                .GetSection("sqlInDefTSType").Value
                .Split(',', ';')
                .Select(p => p.Trim())
                .Distinct()
                .Where(p => int.TryParse(p, out _))
                .ToList();
            var nameParams = _configuration
                .GetSection("userSettings")
                .GetSection("sqlLikeDefTSName").Value
                .Split(',', ';')
                .Select(p => p.Trim())
                .Distinct()
                .ToList();

            await using var sqlConnection = new SqlConnection(await GetConnectionStringAsync());
            await using var sqlCommand = new SqlCommand(SqlQueryBuild(typeParams, nameParams), sqlConnection);
            try
            {
                await sqlConnection.OpenAsync();
                var sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                var ds = new DataSet();
                sqlDataAdapter.Fill(ds);

                return ds.Tables[0].Rows;
            }
            catch
            {
                return null;
            }
        }
        private async Task<string> GetConnectionStringAsync()
        {
            string host;

            if ((host = await GetActiveHostAsync(OicConnectionStringMainGroup)) != null)
            {
                return GetConnectionStringByHost(host);
            }

            if ((host = await GetActiveHostAsync(OicConnectionStringMainGroupReserve)) != null)
            {
                return GetConnectionStringByHost(host);
            }

            return (host = await GetActiveHostAsync(OicConnectionStringReserveGroup)) != null ? GetConnectionStringByHost(host) : null;
        }
        private static async Task<string> GetActiveHostAsync(string connectionString)
        {
            await using var sqlConnection = new SqlConnection(connectionString);
            await using var sqlCommand = new SqlCommand("select oik.dbo.fn_getmainoikservername()", sqlConnection);
            try
            {
                await sqlConnection.OpenAsync();
                return sqlCommand.ExecuteScalar().ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        private string GetConnectionStringByHost(string host)
        {
            var uHost = host.ToUpper();
            if (uHost == new OicConnectionStringParser(OicConnectionStringMainGroup).Server.ToUpper())
            {
                return OicConnectionStringMainGroup;
            }

            if (uHost == new OicConnectionStringParser(OicConnectionStringMainGroupReserve).Server.ToUpper())
            {
                return OicConnectionStringMainGroupReserve;
            }

            return uHost == new OicConnectionStringParser(OicConnectionStringReserveGroup).Server.ToUpper() ? OicConnectionStringReserveGroup : null;
        }
        private static string SqlQueryBuild(List<string> types, List<string> names)
        {
            var query = new StringBuilder("SELECT DefTS.ID, DefTS.Name, EnObj.Abbr " +
                                          "FROM [OIK].[dbo].[DefTS] " +
                                          "INNER JOIN EnObj ON DefTS.EObject = EnObj.ID ");
            if (types.Count > 0)
            {
                query.Append("WHERE DefTS.TSType IN(");
                foreach (var type in types)
                {
                    query.Append(type);
                    query.Append(type != types.Last() ? "," : ") ");
                }
                foreach (var name in names)
                {
                    query.Append("OR DefTS.Name LIKE '%" + name + "%' ");
                }
                return query.ToString();
            }

            if (names.Count > 0)
            {
                query.Append("WHERE ");
                foreach (var name in names)
                {
                    if (name == names.First())
                        query.Append("DefTS.Name LIKE '%" + name + "%' ");
                    else
                        query.Append("OR DefTS.Name LIKE '%" + name + "%' ");
                }
                return query.ToString();
            }

            return string.Empty;
        }
    }
}
