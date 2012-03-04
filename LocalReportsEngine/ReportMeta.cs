// -----------------------------------------------------------------------
// <copyright file="ReportMeta.cs" company="Todd Aspeotis">
//  Copyright 2012 Todd Aspeotis
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;

namespace LocalReportsEngine
{
    using System;
    using System.IO;
    using System.Reflection;
    using ExpressionEvaluator;
    using RdlElements;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ReportMeta : IDisposable
    {

        public readonly RdlReport ReportElement;

        public readonly Evaluator ReportExpressionEvaluator;

        public readonly Resolvable<string, ReportParameter> ReportParameters;

        public readonly Resolvable<string, IResolvedDataSource> DataSources;

        private void ReportParameters_Resolve(object sender, ResolvableEventArgs<string, ReportParameter> args)
        {
            // Prompt the consumer first

            var reportParameterElement =
                ReportElement.ReportParameters.First(rp => rp.Name.SafeEqualsIgnoreCase(args.ResolvingKey));

            args.ResolvedItem = LocalReportsEngineCommon.ElementToObject(reportParameterElement, this);
            args.IsResolved = true;
        }

        public ReportMeta(RdlReport reportElement)
        {
            try
            {
                ReportElement = reportElement;
                ReportParameters = new Resolvable<string, ReportParameter>(ReportParameters_Resolve, StringComparer.InvariantCultureIgnoreCase);
                DataSources = new Resolvable<string, IResolvedDataSource>(DataSources_Resolve, StringComparer.InvariantCultureIgnoreCase);
                ReportExpressionEvaluator = CreateExpressionEvaluator(); // Depends on resolvable collections
            }
            catch (Exception)
            {
                if (ReportExpressionEvaluator != null)
                    ReportExpressionEvaluator.Dispose();

                throw;
            } 
        }

        private object ResolveDataSet(string dataSetName, bool adhoc)
        {
            var dataSetElement = ReportElement.DataSets.First(ds => ds.Name.SafeEqualsIgnoreCase(dataSetName));
            return ResolveDataSet(dataSetElement, adhoc);
        }

        private void DataSources_Resolve(object sender, ResolvableEventArgs<string, IResolvedDataSource> args)
        {
            // Prompt the user first

            var dataSourceElement =
                ReportElement.DataSources.First(ds => ds.Name.SafeEqualsIgnoreCase(args.ResolvingKey));

            args.ResolvedItem = LocalReportsEngineCommon.ElementToObject(dataSourceElement, this);
            args.IsResolved = true;

            // We need to kick off this event, if the ds was resolved while the report was being refreshed
            if (IsReportRefreshing)
                args.ResolvedItem.OnReportRefreshing();
        }

        public object ResolveDataSet(RdlDataSet dataSetElement, bool adhoc)
        {
            // Prompt the user first

            if (dataSetElement.Query == null)
                return null;

            return DataSources[dataSetElement.Query.DataSourceName].ResolveDataSet(dataSetElement, adhoc);
        }

        private Evaluator CreateExpressionEvaluator()
        {
            var meta = new ExpressionMeta("VisualBasic");

            // Expressions
            string expression;
            foreach (var candidate in GetPossibleExpressions())
                if (LocalReportsEngineCommon.IsExpression(candidate, out expression))
                    meta.AddExpression(expression);

            // TODO: Extensions
            var extension = new Extension();
            extension.Name = "Parameters";
            extension.Instance = new ReadOnlyParameterCollection(ReportParameters);
            meta.AddExtension(extension);

            // TODO: Async compile in Evaluator

            return meta.Compile();
        }

        private IEnumerable<string> GetPossibleExpressions()
        {
            foreach (var expression in GetPossibleReportParameterExpressions())
                yield return expression;

            foreach (var expression in GetPossibleDataSourceExpressions())
                yield return expression;

            foreach (var expression in GetPossibleDataSetExpressions())
                yield return expression;
        }

        private IEnumerable<string> GetPossibleDataSetExpressions()
        {
            foreach (var dataSetElement in ReportElement.DataSets)
            {
                if (dataSetElement.Query == null)
                    continue;

                if (dataSetElement.Query.CommandText != null)
                    yield return dataSetElement.Query.CommandText;

                foreach (var queryParameter in dataSetElement.Query.QueryParameters)
                    yield return queryParameter.Value;
            }
        }

        private IEnumerable<string> GetPossibleDataSourceExpressions()
        {
            foreach (RdlDataSource dataSourceElement in ReportElement.DataSources)
                if (dataSourceElement.ConnectionProperties != null)
                    if (dataSourceElement.ConnectionProperties.ConnectString != null)
                        yield return dataSourceElement.ConnectionProperties.ConnectString;
        }

        private IEnumerable<string> GetPossibleReportParameterExpressions()
        {
            if (ReportElement.ReportParameters != null)
            {
                foreach (var reportParameter in ReportElement.ReportParameters)
                {
                    // Valid Values
                    if (reportParameter.ValidValues != null && reportParameter.ValidValues.ParameterValues != null)
                        foreach (var parameterValue in reportParameter.ValidValues.ParameterValues)
                        {
                            yield return parameterValue.Label;
                            yield return parameterValue.Value;
                        }

                    // Default Values
                    if (reportParameter.DefaultValue != null && reportParameter.DefaultValue.Values != null)
                        foreach (var value in reportParameter.DefaultValue.Values)
                            yield return value;
                }
            }
        }

        ~ReportMeta()
        {
            Dispose(false);
        }

        public static IEnumerable<string> ExpressionsForReportParameter(RdlReportParameter parameterElement)
        {
            if (parameterElement.DefaultValue == null) yield break;
            if (parameterElement.DefaultValue.Values == null) yield break;

            foreach (var value in parameterElement.DefaultValue.Values)
                yield return value;
        }

        public bool IsDisposed { get; private set; }

        public static ReportMeta Load(Stream stream)
        {
            var reportElement = LocalReportsEngineCommon.DeserializeReport(stream);
            return new ReportMeta(reportElement);
        }

        public static ReportMeta LoadFromFile(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentOutOfRangeException("path");

            using (var stream = File.OpenRead(path))
                return Load(stream);
        }

        public static ReportMeta LoadFromResource(string name, Assembly assembly = null)
        {
            if (assembly == null) assembly = Assembly.GetCallingAssembly();

            using (var stream = assembly.GetManifestResourceStream(name))
                return Load(stream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                if (ReportExpressionEvaluator != null)
                    ReportExpressionEvaluator.Dispose();
            }

            IsDisposed = true;
        }

        public bool IsReportRefreshing { get; private set; }

        internal void OnReportRefreshing()
        {
            IsReportRefreshing = true;
            DataSources.ForEachResolved(kvp => kvp.Value.OnReportRefreshing());
        }

        internal void OnReportRefreshed()
        {
            IsReportRefreshing = false;
            DataSources.ForEachResolved(kvp => kvp.Value.OnReportRefreshed());
        }
    }
}
