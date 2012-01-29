// -----------------------------------------------------------------------
// <copyright file="Parameter.cs" company="">
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
    public class Parameter
    {
        public readonly object Value;

        public readonly object Label;

        public Parameter(object label, object value)
        {
            Label = label;
            Value = value;
        }
    }
}
