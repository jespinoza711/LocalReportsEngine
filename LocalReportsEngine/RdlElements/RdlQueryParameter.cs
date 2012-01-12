using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlQueryParameter
    {
        [XmlAttribute]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
