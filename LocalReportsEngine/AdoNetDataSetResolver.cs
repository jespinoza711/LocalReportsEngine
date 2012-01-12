// -----------------------------------------------------------------------
// <copyright file="AdoNetDataSetResolver.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.Common;
    using LocalReportsEngine.RdlElements;
    using System.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal class AdoNetDataSetResolver : IDataSetResolver
    {
        public AdoNetDataSetResolver(DbConnection connection)
        {
            this.Connection = connection;
        }

        public DbConnection Connection { get; protected set; }

        public object ResolveDataSet(RdlDataSet dataSet)
        {
            DbCommand command = this.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = dataSet.Query.CommandText;

            using (DbDataReader dataReader = command.ExecuteReader())
            {
                DataTable dataTable = new DataTable(dataSet.Name);
                dataTable.Load(dataReader);
                return dataTable;
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                Connection.Dispose();

            disposed = true;
        }
    }
}
