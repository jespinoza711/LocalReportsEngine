namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using RdlElements;

    public static class LocalReportsEngineCommon
    {
        static LocalReportsEngineCommon()
        {
            var defaultResolverType = typeof(AdoNetDataSource);
            DataSourceResolvers = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
                                      {
                                          {"SQL", defaultResolverType},
                                          {"OLEDB", defaultResolverType},
                                          {"ODBC", defaultResolverType}
                                      };
        }

        public static Dictionary<string, Type> DataSourceResolvers { get; set; }

        public static RdlReport DeserializeReport(Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                // Seek to the first element
                while (reader.NodeType != XmlNodeType.Element)
                    if (!reader.Read())
                        break;

                if (reader.EOF) throw new InvalidOperationException("Unexpected end to RDL");

                // Treat the first element as the root
                var rootAttribute = new XmlRootAttribute();
                rootAttribute.ElementName = reader.LocalName;
                rootAttribute.Namespace = reader.NamespaceURI;

                // Perform the deserialization
                var serializer = new XmlSerializer(typeof(RdlReport), rootAttribute);
                return (RdlReport)serializer.Deserialize(reader);
            }
        }

        internal static ReportParameter ElementToObject(RdlReportParameter reportParameterElement, ReportMeta reportMeta)
        {
            if (reportParameterElement == null) throw new ArgumentNullException("reportParameterElement");
            if (reportMeta == null) throw new ArgumentNullException("reportMeta");

            var reportParameter = new ReportParameter();
            reportParameter.DataType = StringToDataTypeEnum(reportParameterElement.DataType);
            reportParameter.MultiValue = StringToBool(reportParameterElement.MultiValue);

            // Load available values
            var validValues = reportParameterElement.ValidValues;
            if (validValues != null)
            {
                var parameterValues = validValues.ParameterValues;
                var dataSetReference = validValues.DataSetReference;
                
                // Explicit list
                if (parameterValues != null)
                {
                    var values = from parameterValue in parameterValues
                                 select new Tuple<string, object>(
                                     parameterValue.Label,
                                     PhraseToValue(reportMeta, reportParameter.DataType, parameterValue.Value));

                    reportParameter.AvailableValues = values.ToArray();
                }
                else if(dataSetReference != null)
                {
                    // TODO: Datasets etc
                    throw new NotImplementedException();
                }
            }

            // Load default values
            var defaultValues = reportParameterElement.DefaultValue;
            if (defaultValues != null)
            {
                var values = defaultValues.Values;
                var dataSetReference = defaultValues.DataSetReference;

                // Explicit list
                if (values != null)
                {
                    reportParameter.DefaultValues = (from value in values
                                                     select PhraseToValue(reportMeta, reportParameter.DataType, value)).ToArray();
                }
                else if (dataSetReference != null)
                {
                    // TODO: Datasets etc
                    throw new NotImplementedException();
                }
            }

            return reportParameter;
        }

        public static bool StringToBool(string value)
        {
            bool result;
            Boolean.TryParse(value, out result);
            return result;
        }

        public static object PhraseToValue(ReportMeta reportMeta, RdlDataTypeEnum dataType, string phrase)
        {
            string expression;
            if (IsExpression(phrase, out expression))
                return reportMeta.ReportExpressionEvaluator.Evaluate(expression);

            switch(dataType)
            {
                case RdlDataTypeEnum.String:
                    return phrase;

                case RdlDataTypeEnum.Boolean:
                    return Boolean.Parse(phrase);

                case RdlDataTypeEnum.Integer:
                    return Int32.Parse(phrase);

                case RdlDataTypeEnum.DateTime:
                    return DateTime.Parse(phrase, CultureInfo.GetCultureInfo("en-US"));

                case RdlDataTypeEnum.Float:
                    return Single.Parse(phrase);

                case RdlDataTypeEnum.Binary:
                    throw new NotImplementedException();
                
                case RdlDataTypeEnum.Variant:
                    return phrase;
                
                case RdlDataTypeEnum.VariantArray:
                    throw new NotImplementedException();
                
                default:
                    throw new ArgumentOutOfRangeException("dataType");
            }
        }

        public static bool IsExpression(string phrase)
        {
            if (String.IsNullOrWhiteSpace(phrase))
                return false;

            return phrase.TrimStart().First() == '=';
        }

        public static bool IsExpression(string phrase, out string expression)
        {
            if (!String.IsNullOrWhiteSpace(phrase))
            {
                phrase = phrase.Trim();
                if (phrase.First() == '=')
                {
                    expression = phrase.Substring(1);
                    return true;
                }
            }

            expression = null;
            return false;
        }

        private static RdlDataTypeEnum StringToDataTypeEnum(string value)
        {
            RdlDataTypeEnum result;
            if (Enum.TryParse(value, true, out result))
                return result;
            
            // Default
            return RdlDataTypeEnum.String;
        }

        internal static IResolvedDataSource ElementToObject(RdlDataSource dataSourceElement, ReportMeta reportMeta)
        {
            if (dataSourceElement.ConnectionProperties == null)
                // Probably a data source reference. The user should resolve these themselves.
                // But if not, give them a null object. If it's never referenced, there's no problem.
                return null;

            if (dataSourceElement.ConnectionProperties == null)
                // Again, if there's no connection string the user should have resolved
                // this data source themselves. But if not, give them a null object.
                return null;

            Type resolverType;
            lock (DataSourceResolvers)
                if (!DataSourceResolvers.TryGetValue(dataSourceElement.ConnectionProperties.DataProvider, out resolverType))
                    return null;

            IResolvedDataSource resolved = null;

            try
            {
                resolved = (IResolvedDataSource) Activator.CreateInstance(resolverType);
                resolved.Initialize(dataSourceElement, reportMeta);
            }
            catch (Exception)
            {
                if (resolved != null)
                    resolved.Dispose();

                throw;
            }

            return resolved;
        }

        internal static string ValueToString(object value, RdlDataTypeEnum dataType)
        {
            switch(dataType)
            {
                case RdlDataTypeEnum.String:
                    return value.ToString();

                case RdlDataTypeEnum.Boolean:
                    try
                    {
                        var converted = Convert.ToBoolean(value);
                        return converted.ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return String.Empty;
                    }

                case RdlDataTypeEnum.Integer:
                    try
                    {
                        var converted = Convert.ToInt32(value);
                        return converted.ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return String.Empty;
                    }

                case RdlDataTypeEnum.DateTime:
                    try
                    {
                        var converted = Convert.ToDateTime(value);
                        return converted.ToString("o"); // ISO 8601
                    }
                    catch (Exception)
                    {
                        return String.Empty;
                    }

                case RdlDataTypeEnum.Float:
                    try
                    {
                        var converted = Convert.ToSingle(value);
                        return converted.ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return String.Empty;
                    }

                case RdlDataTypeEnum.Binary:
                    throw new NotImplementedException();

                case RdlDataTypeEnum.Variant:
                    throw new NotImplementedException();

                case RdlDataTypeEnum.VariantArray:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException("dataType");
            }
        }
    }
}
