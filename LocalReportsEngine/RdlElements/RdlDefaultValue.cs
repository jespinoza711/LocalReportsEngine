using System.Collections.Generic;
using System.Xml.Serialization;

namespace LocalReportsEngine.RdlElements
{
    public class RdlDefaultValue
    {
        public RdlDataSetReference DataSetReference { get; set; }

        [XmlArrayItem("Value")]
        public List<string> Values { get; set; }
    }
}
