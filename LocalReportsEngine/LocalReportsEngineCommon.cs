using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            DataSourceResolvers = new Dictionary<string, IDataSourceResolver>(StringComparer.InvariantCultureIgnoreCase);
            AdoNetDataSourceResolver defaultResolver = new AdoNetDataSourceResolver();
            DataSourceResolvers.Add("SQL", defaultResolver);
            DataSourceResolvers.Add("OLEDB", defaultResolver);
            DataSourceResolvers.Add("ODBC", defaultResolver);
        }

        public static Dictionary<string, IDataSourceResolver> DataSourceResolvers { get; set; }

        public static RdlReport DeserializeReport(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    // Seek to the first element
                    while (reader.NodeType != XmlNodeType.Element && reader.Read()) ;
                    if (reader.EOF) throw new InvalidOperationException("Unexpected end to RDL");

                    // Treat the first element as the root
                    XmlRootAttribute rootAttribute = new XmlRootAttribute();
                    rootAttribute.ElementName = reader.LocalName;
                    rootAttribute.Namespace = reader.NamespaceURI;

                    // Perform the deserialization
                    XmlSerializer serializer = new XmlSerializer(typeof(RdlReport), rootAttribute);
                    return (RdlReport)serializer.Deserialize(reader);
                }
            }
        }

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

        private static void ResolveDataSource(RdlReport report, string dataSourceName, Dictionary<string, IDataSetResolver> dataSources)
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
        }
    }
}
