// -----------------------------------------------------------------------
// <copyright file="ReadOnlyParameterCollection.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ReadOnlyParameterCollection : MarshalByRefObject
    {
        private Resolvable<string, ReportParameter> ResolvableReportParameters { get; set; }

        internal ReadOnlyParameterCollection(Resolvable<string, ReportParameter> resolvableReportParameters)
        {
            ResolvableReportParameters = resolvableReportParameters;
        }

        [System.Runtime.CompilerServices.IndexerName("Parameters")]
        public object this[string parameterName]
        {
            get
            {
                var reportParameter = ResolvableReportParameters[parameterName];
                return new ReadOnlyParameter(reportParameter.Label, reportParameter.Value);
            }
        }
    }
}
