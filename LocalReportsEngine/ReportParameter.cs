// -----------------------------------------------------------------------
// <copyright file="ParameterMeta.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LocalReportsEngine
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ReportParameter
    {
        public Tuple<string, object>[] AvailableValues { get; set; }

        public object[] DefaultValues { get; set; }

        public bool MultiValue { get; set; }

        // TODO: Handle this better, null value_ might not actually mean "use defaults"
        private object _value;

        public object Label
        {
            get
            {
                if (AvailableValues == null)
                    return null;

                if (DefaultValues == null)
                    return null;

                // Doesn't handle null values at the moment -- assumes all default values have a value.
                // TODO: Observe that SSRS seems to handle it such that if a value does not have a label, the value is the label.
                var labels = DefaultValues.Select(df => AvailableValues.First(av => df.Equals(av.Item2)).Item1);

                if (MultiValue)
                    return labels.ToArray();

                return labels.First();
            }
        }

        public object Value
        {
            get
            {
                if (_value != null)
                    return _value;

                return MultiValue ? DefaultValues : DefaultValues.FirstOrDefault();
            }

            set { _value = value; }
        }

        public RdlElements.RdlDataTypeEnum DataType { get; set; }
    }
}
