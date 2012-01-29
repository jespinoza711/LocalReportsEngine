// -----------------------------------------------------------------------
// <copyright file="ParametersCollection.cs" company="">
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
    public class ParametersCollection
    {
        internal readonly Dictionary<string, Parameter> Items = new Dictionary<string, Parameter>();
            
        [System.Runtime.CompilerServices.IndexerName("Parameters")]
        public Parameter this [string key]
        {
            get
            {
                Parameter parameter;
                if (Items.TryGetValue(key, out parameter))
                    return parameter;

                return new Parameter(null, null); // Empty parameter
            }
        }
    }
}
