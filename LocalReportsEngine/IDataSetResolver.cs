namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LocalReportsEngine.RdlElements;

    public interface IDataSetResolver : IDisposable
    {
        object ResolveDataSet(RdlDataSet dataSet);
    }
}
