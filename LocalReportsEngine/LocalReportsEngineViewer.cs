using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using LocalReportsEngine.RdlElements;

namespace LocalReportsEngine
{
    public class LocalReportsEngineViewer : Control
    {
        public ReportViewer ReportViewer { get; protected set; }

        public LocalReport LocalReport { get { return this.ReportViewer.LocalReport; } }

        public RdlReport Report { get; protected set; }

        public Dictionary<string, object> DataSets { get; protected set; }

        public string ReportPath { get; set; }

        public LocalReportsEngineViewer()
            : base()
        {
            this.ReportViewer = new ReportViewer();

            this.SuspendLayout();
            this.ReportViewer.Name = "ReportViewer";
            this.ReportViewer.Dock = DockStyle.Fill;
            this.Controls.Add(this.ReportViewer);
            this.ResumeLayout();

            this.Reset();
        }

        public void RefreshReport()
        {
            this.Reset();

            this.Report = LocalReportsEngineCommon.DeserializeReport(this.ReportPath);
            LocalReportsEngineCommon.ResolveDataSets(this.Report, this.DataSets, this.DataSources); // Later we will add hooks for users

            this.LocalReport.ReportPath = this.ReportPath;

            foreach (var kvp in this.DataSets)
                this.LocalReport.DataSources.Add(new ReportDataSource(kvp.Key, kvp.Value));

            this.ReportViewer.RefreshReport();
        }

        public void Reset()
        {
            this.DataSets = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            this.DataSources = new Dictionary<string, IDataSetResolver>(StringComparer.InvariantCultureIgnoreCase);
            this.Report = null;
            this.ReportViewer.Reset();
        }

        public Dictionary<string, IDataSetResolver> DataSources { get; protected set; }
    }
}
