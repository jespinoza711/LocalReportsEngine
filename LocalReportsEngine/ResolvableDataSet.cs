using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionEvaluator;
using LocalReportsEngine.RdlElements;

namespace LocalReportsEngine
{
    public class ResolvableDataSet : ResolvableResource<RdlDataSet, object>
    {
        public ResolvableReportParameterCollection QueryParameters { get; set; }

        public Evaluator QueryExpressionsEvaluator { get; set; }

        public readonly ResolvableDataSource DataSource;

        public ResolvableDataSet(RdlDataSet element, ResolvableDataSource dataSource)
            : base(element)
        {
            DataSource = dataSource;
        }
    }
}
