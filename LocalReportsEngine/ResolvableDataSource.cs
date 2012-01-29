using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LocalReportsEngine.RdlElements;

namespace LocalReportsEngine
{
    public class ResolvableDataSource : ResolvableResource<RdlDataSource, object>
    {
        public ResolvableDataSource(RdlDataSource resource)
            : base(resource)
        {
        }
    }
}
