using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlDataSet
    {
        [XmlAttribute]
        public string Name { get; set; }

        public RdlQuery Query { get; set; }
    }
}
