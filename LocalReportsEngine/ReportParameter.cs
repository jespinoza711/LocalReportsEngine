// -----------------------------------------------------------------------
// <copyright file="ParameterMeta.cs" company="">
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
    public class ReportParameter
    {
        public readonly Dictionary<string, object> AvailableValues = new Dictionary<string, object>();

        public readonly List<object> DefaultValues = new List<object>();

        public string Label { get; set; }

        public object Value { get; set; }

        public RdlElements.RdlDataTypeEnum DataType { get; set; }
    }
}
