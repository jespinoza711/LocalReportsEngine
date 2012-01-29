using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ExpressionEvaluator;
using LocalReportsEngine.RdlElements;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ExtensionMethods;

namespace LocalReportsEngine
{
    public static class LocalReportsEngineCommon
    {
        static LocalReportsEngineCommon()
        {
            var defaultResolver = new AdoNetDataSourceResolver();
            DataSourceResolvers = new Dictionary<string, IDataSourceResolver>(StringComparer.InvariantCultureIgnoreCase)
                                      {
                                          {"SQL", defaultResolver},
                                          {"OLEDB", defaultResolver},
                                          {"ODBC", defaultResolver}
                                      };
        }

        public static Dictionary<string, IDataSourceResolver> DataSourceResolvers { get; set; }

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

        /*
        public static void ResolveDataSets(RdlReport report, Dictionary<string, object> dataSets, Dictionary<string, IDataSetResolver> dataSources)
        {
            foreach (RdlDataSet dataSet in report.DataSets)
            {
                if (dataSets.ContainsKey(dataSet.Name))
                    continue;

                // This is a no-op if the data source has already been resolved
                // TODO: Bad, make ResolveDataSource resolve the data source and nothing else
                ResolveDataSource(report, dataSet.Query.DataSourceName, dataSources);

                IDataSetResolver dataSetResolver = dataSources[dataSet.Query.DataSourceName];
                dataSets.Add(dataSet.Name, dataSetResolver.ResolveDataSet(dataSet));
            }
        }
         * */

        /*public static void ResolveDataSources(RdlReport report, string dataSourceName, Dictionary<string, IDataSetResolver> dataSources)
        {
            if (dataSources.ContainsKey(dataSourceName))
                return;

            RdlDataSource dataSource = report.DataSources.FirstOrDefault(ds => ds.Name.SafeEquals(dataSourceName));
            if (dataSource == null)
                return;

            IDataSourceResolver dataSourceResolver;
            if (!DataSourceResolvers.TryGetValue(dataSource.ConnectionProperties.DataProvider, out dataSourceResolver))
                return;

            IDataSetResolver resolvedDataSource = dataSourceResolver.ResolveDataSource(dataSource);
            dataSources.Add(dataSource.Name, resolvedDataSource);
        }*/

        public static bool IsRdlExpression(string expression)
        {
            if (String.IsNullOrWhiteSpace(expression))
                return false;

            return expression.TrimStart().First() == '=';
        }

        public static ExpressionMeta FindParametersAndExpressions<T, U>(IEnumerable<T> input, Collection<U> output, Func<T, U> collectionConverter, Func<T, IEnumerable<string>> expressionConverter)
        {
            var meta = new ExpressionMeta("VisualBasic");

            foreach (var item in input)
            {
                foreach(var expression in expressionConverter(item))
                    if (IsRdlExpression(expression))
                        meta.Expressions.Add(expression);

                output.Add(collectionConverter(item));
            }

            return meta;
        }

        public static void ResolveDataSource(ResolvableDataSource resolving)
        {
            IDataSourceResolver resolver;
            if (!DataSourceResolvers.TryGetValue(resolving.Resource.ConnectionProperties.DataProvider, out resolver))
                return;

            resolving.Result = resolver.ResolveDataSource(resolving.Resource);
            resolving.IsResolved = true;
        }

        public static void ResolveDataSet(ResolvableDataSet resolving)
        {
            var resolver = resolving.DataSource.Result as IDataSetResolver;
            if (resolver == null)
                return;

            resolving.Result = resolver.ResolveDataSet(resolving.Resource);
            resolving.IsResolved = true;
        }
    }
}
