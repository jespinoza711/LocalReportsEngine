using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LocalReportsEngine.RdlElements;
using System.Data.Common;
using ExtensionMethods;
using System.Data.SqlClient;

namespace LocalReportsEngine
{
    internal class AdoNetDataSource : IResolvedDataSource
    {
        public bool IsDisposed { get; private set; }

        protected static Dictionary<string, string> DbFactoryNames { get; set; }

        protected DbConnection Connection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        static AdoNetDataSource()
        {
            DbFactoryNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                                 {
                                     {"SQL", "System.Data.SqlClient"},
                                     {"OLEDB", "System.Data.OleDb"},
                                     {"ODBC", "System.Data.Odbc"}
                                 };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="providerFactory"></param>
        /// <returns></returns>
        private static string CreateConnectionString(RdlDataSource dataSource, DbProviderFactory providerFactory)
        {
            var genericConnectionStringBuilder = providerFactory.CreateConnectionStringBuilder();
            Debug.Assert(genericConnectionStringBuilder != null, "genericConnectionStringBuilder != null");
            genericConnectionStringBuilder.ConnectionString = dataSource.ConnectionProperties.ConnectString;

            var sqlConnectionStringBuilder = genericConnectionStringBuilder as SqlConnectionStringBuilder;
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
                Debug.Assert(connection != null, "connection != null");
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

        public void Initialize(RdlDataSource dataSourceElement, ReportMeta reportMeta)
        {
            DataSourceElement = dataSourceElement;
            ReportMeta = reportMeta;

            string factoryName;

            lock (DbFactoryNames)
                if (!DbFactoryNames.TryGetValue(dataSourceElement.ConnectionProperties.DataProvider, out factoryName))
                    throw new ArgumentOutOfRangeException("dataSourceElement", "ConnectionProperties.DataProvider is unknown");

            var providerFactory = DbProviderFactories.GetFactory(factoryName);
            var connectionString = CreateConnectionString(dataSourceElement, providerFactory);
            Connection = CreateConnection(providerFactory, connectionString);
        }

        ~AdoNetDataSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                if (Connection != null)
                    Connection.Dispose();
            }

            IsDisposed = true;
        }

        public object ResolveDataSet(RdlDataSet dataSetElement, bool adhoc)
        {
            // adhoc controls xact behaviour or not
            var queryElement = dataSetElement.Query;

            if (queryElement == null)
                return null;

            var command = Connection.CreateCommand();
            command.CommandType = GetCommandType(queryElement.CommandType);
            command.CommandText = queryElement.CommandText;
            command.CommandTimeout = GetCommandTimeout(queryElement.Timeout);

            foreach(var parameterElement in queryElement.QueryParameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterElement.Name;
                var value = LocalReportsEngineCommon.PhraseToValue(ReportMeta, RdlDataTypeEnum.String, parameterElement.Value);
                parameter.Value = value;

                command.Parameters.Add(parameter);
            }

            using (var reader = command.ExecuteReader())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                return dataTable;
            }
        }

        private int GetCommandTimeout(string timeout)
        {
            int result;
            if (Int32.TryParse(timeout, out result))
                return result;

            return 30;
        }

        private CommandType GetCommandType(string commandType)
        {
            CommandType result;
            if (Enum.TryParse(commandType, true, out result))
                return result;

            return CommandType.Text;
        }

        public RdlDataSource DataSourceElement { get; private set; }

        public ReportMeta ReportMeta { get; private set; }
    }
}
