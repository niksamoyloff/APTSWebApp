﻿using System;
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
        public string OicConnectionStringMainGroup { get; private set; }
        public string OicConnectionStringMainGroupReserve { get; private set; }
        public string OicConnectionStringReserveGroup { get; private set; }

        private readonly IConfiguration _configuration;

        public Api_OIC(IConfiguration configuration)
        {
            _configuration = configuration;
            OicConnectionStringMainGroup = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringMainGroup").Value;
            OicConnectionStringMainGroupReserve = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringMainGroupReserve").Value;
            OicConnectionStringReserveGroup = _configuration.GetSection("ConnectionStrings").GetSection("oicConnectionStringReserveGroup").Value;
        }
        public DataRowCollection GetTSFromOIC()
        {
            List<string> typeParams = _configuration.GetSection("userSettings").GetSection("sqlInDefTSType").Value.Split(',', ';').Select(p => p.Trim())
                .Distinct().Where(p => int.TryParse(p, out int n) == true).ToList();
            List<string> nameParams = _configuration.GetSection("userSettings").GetSection("sqlLikeDefTSName").Value.Split(',', ';').Select(p => p.Trim())
                .Distinct().ToList();

            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand sqlCommand = new SqlCommand(SqlQueryBuild(typeParams, nameParams), sqlConnection))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                        DataSet ds = new DataSet();
                        sqlDataAdapter.Fill(ds);

                        return ds.Tables[0].Rows;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
        private string GetConnectionString()
        {
            string host;

            if ((host = GetActiveHost(OicConnectionStringMainGroup)) != null)
            {
                return GetConnectionStringByHost(host);
            }
            else if ((host = GetActiveHost(OicConnectionStringMainGroupReserve)) != null)
            {
                return GetConnectionStringByHost(host);
            }
            else if ((host = GetActiveHost(OicConnectionStringReserveGroup)) != null)
            {
                return GetConnectionStringByHost(host);
            }
            else
            {
                return null;
            }
        }
        private string GetActiveHost(string connectionString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("select oik.dbo.fn_getmainoikservername()", sqlConnection))
                {
                    try
                    {
                        sqlConnection.Open();
                        return sqlCommand.ExecuteScalar().ToString();
                    }
                    catch (Exception e)
                    {
                        string msg = e.ToString();
                        return null;
                    }
                }
            }
        }
        private string GetConnectionStringByHost(string host)
        {
            string uHost = host.ToUpper();
            if (uHost == new OicConnectionStringParser(OicConnectionStringMainGroup).Server.ToUpper())
            {
                return OicConnectionStringMainGroup;
            }
            else if (uHost == new OicConnectionStringParser(OicConnectionStringMainGroupReserve).Server.ToUpper())
            {
                return OicConnectionStringMainGroupReserve;
            }
            else if (uHost == new OicConnectionStringParser(OicConnectionStringReserveGroup).Server.ToUpper())
            {
                return OicConnectionStringReserveGroup;
            }
            else
            {
                return null;
            }
        }
        private string SqlQueryBuild(List<string> types, List<string> names)
        {
            StringBuilder query = new StringBuilder("SELECT DefTS.ID, DefTS.Name, EnObj.Abbr " +
                                                    "FROM [OIK].[dbo].[DefTS] " +
                                                    "INNER JOIN EnObj ON DefTS.EObject = EnObj.ID ");
            if (types.Count > 0)
            {
                query.Append("WHERE DefTS.TSType IN(");
                foreach (var type in types)
                {
                    query.Append(type);
                    if (type != types.Last())
                        query.Append(",");
                    else
                        query.Append(") ");
                }
                foreach (var name in names)
                {
                    query.Append("OR DefTS.Name LIKE '%" + name + "%' ");
                }
                return query.ToString();
            }
            else if (names.Count > 0)
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
            else
                return "";
        }
    }
}
