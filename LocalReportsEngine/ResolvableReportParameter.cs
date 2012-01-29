using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LocalReportsEngine.RdlElements;

namespace LocalReportsEngine
{
    public class ResolvableReportParameter : ResolvableResource<RdlReportParameter, object>
    {
        public ResolvableReportParameter(RdlReportParameter resource)
            : base(resource)
        {
        }
    }
}
