using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlDataSource
    {
        [XmlAttribute]
        public string Name { get; set; }

        public string Transaction { get; set; }

        public string DataSourceReference { get; set; }

        public RdlConnectionProperties ConnectionProperties { get; set; }
    }
}
