namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LocalReportsEngine.RdlElements;

    public interface IResolvedDataSource : IDisposable
    {
        void Initialize(RdlDataSource dataSourceElement, ReportMeta reportMeta);

        object ResolveDataSet(RdlDataSet dataSetElement, bool adhoc);

        // + OnReportRefreshing
        // + OnReportRefreshed
    }
}
