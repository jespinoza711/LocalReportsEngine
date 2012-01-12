namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LocalReportsEngine.RdlElements;

    public interface IDataSourceResolver
    {
        IDataSetResolver ResolveDataSource(RdlDataSource dataSource);
    }
}
