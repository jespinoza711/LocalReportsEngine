using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalReportsEngine.RdlElements
{
    public class RdlConnectionProperties
    {
        public string ConnectString { get; set; }

        public string DataProvider { get; set; }

        public string IntegratedSecurity { get; set; }

        public string Prompt { get; set; }
    }
}
