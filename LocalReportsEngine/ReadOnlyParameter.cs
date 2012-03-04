// -----------------------------------------------------------------------
// <copyright file="ReadOnlyParameter.cs" company="">
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
    public sealed class ReadOnlyParameter : MarshalByRefObject
    {
        public object Label { get; private set; }

        // readonly fields cause errors in the sandbox??
        public object Value { get; private set; }

        // values etc.

        internal ReadOnlyParameter(object label, object value)
        {
            Label = label;
            Value = value;
        }
    }
}
