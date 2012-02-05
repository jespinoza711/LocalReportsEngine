using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public ReportMeta CurrentReportMeta { get; private set; }

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

        public void LoadFromFile(string path)
        {
            Reset();

            CurrentReportMeta = ReportMeta.LoadFromFile(path);
            LocalReport.ReportPath = path;

            RefreshReport();
        }

        public void LoadFromResource(string name, Assembly assembly = null)
        {
            Reset();

            CurrentReportMeta = ReportMeta.LoadFromResource(name, assembly);
            LocalReport.ReportEmbeddedResource = name;

            RefreshReport();
        }

        public void RefreshReport()
        {
            // Update the GUI (perhaps for the first time)

            // Specify all parameters
            LocalReport.SetParameters(GetParameters());

            // Specify all data sets
            foreach (var dataSetElement in CurrentReportMeta.ReportElement.DataSets)
            {
                var instance = CurrentReportMeta.ResolveDataSet(dataSetElement, false);
                var item = new ReportDataSource(dataSetElement.Name, instance);
                LocalReport.DataSources.Add(item);
            }

            // Render the report
            ReportViewer.RefreshReport();
        }

        private IEnumerable<Microsoft.Reporting.WinForms.ReportParameter> GetParameters()
        {
            foreach(var parameterElement in CurrentReportMeta.ReportElement.ReportParameters)
            {
                var parameter = new Microsoft.Reporting.WinForms.ReportParameter();
                parameter.Name = parameterElement.Name;

                // TODO: foreach(var value in xxx)
                var resolvedParameter = CurrentReportMeta.ReportParameters[parameter.Name];
                parameter.Values.Add(LocalReportsEngineCommon.ValueToString(resolvedParameter.Value, resolvedParameter.DataType));
                yield return parameter;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed == false && disposing)
            {
                if (CurrentReportMeta != null)
                    CurrentReportMeta.Dispose();
            }

            base.Dispose(disposing);
        }

        public void Reset()
        {
            if (CurrentReportMeta != null)
            {
                CurrentReportMeta.Dispose();
                CurrentReportMeta = null;
            }

            ReportViewer.Reset();
        }
    }
}
