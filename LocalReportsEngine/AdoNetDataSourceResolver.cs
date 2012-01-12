using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LocalReportsEngine.RdlElements;
using System.Data.Common;
using ExtensionMethods;
using System.Data.SqlClient;

namespace LocalReportsEngine
{
    internal class AdoNetDataSourceResolver : IDataSourceResolver
    {
        protected static Dictionary<string, string> DbFactoryNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        static AdoNetDataSourceResolver()
        {
            DbFactoryNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            DbFactoryNames.Add("SQL", "System.Data.SqlClient");
            DbFactoryNames.Add("OLEDB", "System.Data.OleDb");
            DbFactoryNames.Add("ODBC", "System.Data.Odbc");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public IDataSetResolver ResolveDataSource(RdlDataSource dataSource)
        {
            string factoryName;
            if (!DbFactoryNames.TryGetValue(dataSource.ConnectionProperties.DataProvider, out factoryName))
                return null;

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(factoryName);
            string connectionString = CreateConnectionString(dataSource, providerFactory);
            DbConnection connection = CreateConnection(providerFactory, connectionString);
            return new AdoNetDataSetResolver(connection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="providerFactory"></param>
        /// <returns></returns>
        private static string CreateConnectionString(RdlDataSource dataSource, DbProviderFactory providerFactory)
        {
            DbConnectionStringBuilder genericConnectionStringBuilder = providerFactory.CreateConnectionStringBuilder();
            genericConnectionStringBuilder.ConnectionString = dataSource.ConnectionProperties.ConnectString;

            SqlConnectionStringBuilder sqlConnectionStringBuilder = genericConnectionStringBuilder as SqlConnectionStringBuilder;
            if (sqlConnectionStringBuilder != null)
            {
                bool integratedSecurity;
                if (Boolean.TryParse(dataSource.ConnectionProperties.IntegratedSecurity, out integratedSecurity) && integratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = true;
                    sqlConnectionStringBuilder.UserID = String.Empty;
                    sqlConnectionStringBuilder.Password = String.Empty;
                }
            }

            return genericConnectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerFactory"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static DbConnection CreateConnection(DbProviderFactory providerFactory, string connectionString)
        {
            DbConnection connection = null;

            try
            {
                connection = providerFactory.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                return connection;
            }
            catch (Exception)
            {
                if (connection != null)
                    connection.Dispose();

                return null;
            }
        }
    }
}
