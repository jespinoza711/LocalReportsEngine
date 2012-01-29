namespace LocalReportsEngine.RdlElements
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class RdlValidValues
    {
        public RdlDataSetReference DataSetReference { get; set; }

        [XmlArrayItem("ParameterValue")]
        public List<RdlParameterValue> ParameterValues { get; set; }
    }
}
