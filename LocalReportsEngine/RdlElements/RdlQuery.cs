using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlQuery
    {
        public string DataSourceName { get; set; }

        public string CommandType { get; set; }

        public string CommandText { get; set; }

        [XmlArrayItem("QueryParameter")]
        public List<RdlQueryParameter> QueryParameters { get; set; }

        public string Timeout { get; set; }
    }
}
