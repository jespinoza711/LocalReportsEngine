namespace LocalReportsEngine.RdlElements
{
    using System.Xml.Serialization;

    public class RdlReportParameter
    {
        public string AllowBlank { get; set; }

        public string DataType { get; set; }

        public RdlDefaultValue DefaultValue { get; set; }

        public string Nullable { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public string Prompt { get; set; }

        public string Hidden { get; set; }

        public string MultiValue { get; set; }

        public RdlValidValues ValidValues { get; set; }

        public string UsedInQuery { get; set; }
    }
}
