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

        protected DbConnection AdHocConnection { get; set; }

        protected DbConnection TransactionConnection { get; set; }

        protected DbTransaction Transaction { get; set; }

        protected bool IsTransactional { get; set; }

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
                if (Boolean.TryParse(dataSource.ConnectionProperties.IntegratedSecurity, out integratedSecurity) &&
                    integratedSecurity)
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
            IsTransactional = LocalReportsEngineCommon.StringToBool(dataSourceElement.Transaction);

            string factoryName;

            lock (DbFactoryNames)
                if (!DbFactoryNames.TryGetValue(dataSourceElement.ConnectionProperties.DataProvider, out factoryName))
                    throw new ArgumentOutOfRangeException("dataSourceElement",
                                                          "ConnectionProperties.DataProvider is unknown");

            var providerFactory = DbProviderFactories.GetFactory(factoryName);
            var connectionString = CreateConnectionString(dataSourceElement, providerFactory);
            AdHocConnection = CreateConnection(providerFactory, connectionString);
            TransactionConnection = IsTransactional ? CreateConnection(providerFactory, connectionString) : null;
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
                // Ad-hoc connection
                DisposeConnectionNoThrow(AdHocConnection);
                AdHocConnection = null;

                // Our second, transaction connection
                DisposeTransaction(Transaction);
                Transaction = null;
                DisposeConnectionNoThrow(TransactionConnection);
                TransactionConnection = null;
            }

            IsDisposed = true;
        }

        protected void DisposeConnection(DbConnection connection)
        {
            if (connection != null)
                connection.Dispose();
        }

        protected void DisposeConnectionNoThrow(DbConnection connection)
        {
            try
            {
                DisposeConnection(connection);
            }
            catch (Exception)
            {
            }
        }

        protected void DisposeTransaction(DbTransaction transaction)
        {
            if (transaction != null)
                transaction.Dispose();
        }

        protected void DisposeTransactionNoThrow(DbTransaction transaction)
        {
            try
            {
                DisposeTransaction(transaction);
            }
            catch (Exception)
            {
            }
        }

        public object ResolveDataSet(RdlDataSet dataSetElement, bool adhoc)
        {
            var queryElement = dataSetElement.Query;

            if (queryElement == null)
                return null;

            var transactional = !adhoc && IsTransactional;
            var command = (transactional ? TransactionConnection : AdHocConnection).CreateCommand();
            command.CommandType = GetCommandType(queryElement.CommandType);
            command.CommandText = queryElement.CommandText;
            command.CommandTimeout = GetCommandTimeout(queryElement.Timeout);
            command.Transaction = transactional ? Transaction : null;

            foreach (var parameterElement in queryElement.QueryParameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterElement.Name;
                var value = LocalReportsEngineCommon.PhraseToValue(ReportMeta, RdlDataTypeEnum.String,
                                                                   parameterElement.Value);
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

        public void OnReportRefreshing()
        {
            if (IsTransactional == false)
                return;

            // TODO: We can get here where Transaction != null by messing with IsTransactional but that's about it...
            // Make IsT private set and get rid of this
            DisposeTransaction(Transaction);
            Transaction = TransactionConnection.BeginTransaction();
        }

        public void OnReportRefreshed()
        {
            if (IsTransactional == false)
                return;

            try
            {
                Transaction.Commit();
            }
            finally
            {
                DisposeTransactionNoThrow(Transaction);
                Transaction = null;
            }
        }
    }
}