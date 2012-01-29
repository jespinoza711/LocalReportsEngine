using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlReport
    {
        public string Author { get; set; }

        public string Custom { get; set; }

        [XmlArrayItem("DataSet")]
        public List<RdlDataSet> DataSets { get; set;  }

        [XmlArrayItem("DataSource")]
        public List<RdlDataSource> DataSources { get; set; }

        public string Description { get; set; }

        [XmlArrayItem("ReportParameter")]
        public List<RdlReportParameter> ReportParameters { get; set; } 
    }
}
