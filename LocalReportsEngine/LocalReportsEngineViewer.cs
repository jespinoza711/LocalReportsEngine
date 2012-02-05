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

        public LocalReportsEngineViewer()
        {
            ReportViewer = new ReportViewer();

            SuspendLayout();
            ReportViewer.Name = "ReportViewer";
            ReportViewer.Dock = DockStyle.Fill;
            Controls.Add(ReportViewer);
            ResumeLayout();

            Reset();
        }

        public void Reset()
        {
            // report meta = null, dispose if necessary
            ReportViewer.Reset();
        }
    }
}
