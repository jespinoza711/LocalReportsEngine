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

        public readonly ResolvableDataSourceCollection DataSources = new ResolvableDataSourceCollection();

        public readonly ResolvableDataSetCollection DataSets = new ResolvableDataSetCollection();

        public readonly ResolvableReportParameterCollection ReportParameters = new ResolvableReportParameterCollection();

        public readonly Evaluator ReportExpressionEvaluator;

        public ReportMeta(RdlReport reportElement, bool resolveDataSources, bool resolveDataSets)
        {
            try
            {
                ReportElement = reportElement;

                // Wrap data sources
                foreach(var element in ReportElement.DataSources)
                    DataSources.Add(new ResolvableDataSource(element));

                // Wrap data sets
                foreach(var element in ReportElement.DataSets)
                    DataSets.Add(new ResolvableDataSet(element, DataSources.FirstOrDefault(ds => element.Query.DataSourceName.SafeEqualsIgnoreCase(ds.Resource.Name))));

                // Report Parameters
                var meta = LocalReportsEngineCommon.FindParametersAndExpressions(ReportElement.ReportParameters,
                                                                                 ReportParameters,
                                                                                 x => new ResolvableReportParameter(x),
                                                                                 ExpressionsForReportParameter);

                // Add extensions first
                ReportExpressionEvaluator = meta.Compile();

                if (resolveDataSources)
                    ResolveDataSources();

                if (resolveDataSets)
                    ResolveDataSets();
            }
            catch (Exception)
            {
                if (ReportExpressionEvaluator != null)
                    ReportExpressionEvaluator.Dispose();

                throw;
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

        public event EventHandler<ResolvableResourceEventArgs<ResolvableDataSource>> DataSourceResolve;

        public event EventHandler<ResolvableResourceEventArgs<ResolvableDataSet>> DataSetResolve;

        public void ResolveDataSources()
        {
            foreach (var resolving in DataSources)
            {
                if (resolving.IsResolved)
                    continue;

                var subscribers = DataSourceResolve;
                if (subscribers != null)
                {
                    var eventArgs = new ResolvableResourceEventArgs<ResolvableDataSource>(resolving);
                    var invocationList = DataSourceResolve.GetInvocationList();

                    foreach (EventHandler invoke in invocationList)
                    {
                        invoke(this, eventArgs);
                        if (resolving.IsResolved)
                            break;
                    }
                }

                if (resolving.IsResolved)
                    continue;

                // Default resolver
                LocalReportsEngineCommon.ResolveDataSource(resolving);
            }
        }

        public void ResolveDataSets()
        {
            // DRY (ResolveDataSources)! We can combine the common functionality
            foreach(var resolving in DataSets)
            {
                if (resolving.IsResolved)
                    continue;

                if (resolving.DataSource == null || !resolving.DataSource.IsResolved)
                    continue;

                var subscribers = DataSetResolve;
                if (subscribers != null)
                {
                    var eventArgs = new ResolvableResourceEventArgs<ResolvableDataSet>(resolving);
                    var invocationList = DataSourceResolve.GetInvocationList();

                    foreach(EventHandler invoke in invocationList)
                    {
                        invoke(this, eventArgs);
                        if (resolving.IsResolved)
                            break;
                    }
                }

                if (resolving.IsResolved)
                    continue;

                // Default resolver
                LocalReportsEngineCommon.ResolveDataSet(resolving);
            }
        }

        public static ReportMeta Load(Stream stream, bool resolveDataSources = true, bool resolveDataSets = true)
        {
            var reportElement = LocalReportsEngineCommon.DeserializeReport(stream);
            return new ReportMeta(reportElement, resolveDataSources, resolveDataSets);
        }

        public static ReportMeta LoadFromFile(string path, bool resolveDataSources = true, bool resolveDataSets = true)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentOutOfRangeException("path");

            using (var stream = File.OpenRead(path))
                return Load(stream, resolveDataSources, resolveDataSets);
        }

        public static ReportMeta LoadFromResource(string name, Assembly assembly = null, bool resolveDataSources = true, bool resolveDataSets = true)
        {
            if (assembly == null) assembly = Assembly.GetCallingAssembly();

            using (var stream = assembly.GetManifestResourceStream(name))
                return Load(stream, resolveDataSources, resolveDataSets);
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
    }
}
