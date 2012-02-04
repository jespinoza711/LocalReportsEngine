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

        private Resolvable<string, ReportParameter> ReportParameters;

        private void ReportParameters_Resolve(object sender, ResolvableEventArgs<string, ReportParameter> args)
        {
            // Prompt the user first

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
                ReportExpressionEvaluator = CreateExpressionEvaluator(); // Depends on resolvable collections

                Console.WriteLine("StartDate: {0}", ReportParameters["StartDate"].Value);
                Console.WriteLine("EndDate: {0}", ReportParameters["EndDate"].Value);
            }
            catch (Exception)
            {
                if (ReportExpressionEvaluator != null)
                    ReportExpressionEvaluator.Dispose();

                throw;
            } 
        }

        private Evaluator CreateExpressionEvaluator()
        {
            var meta = new ExpressionMeta("VisualBasic");

            // Expressions
            foreach (var expression in GetExpressions())
                meta.AddExpression(expression);

            // TODO: Extensions
            var extension = new Extension();
            extension.Name = "Parameters";
            extension.Instance = new ReadOnlyParameterCollection(ReportParameters);
            meta.AddExtension(extension);

            // TODO: Async compile in Evaluator

            return meta.Compile();
        }

        private IEnumerable<string> GetExpressions()
        {
            foreach (var expression in GetReportParameterExpressions())
                yield return expression;

            // TODO: Get all expressions and compile them

            // TODO: Data set reference etc
        }

        private IEnumerable<string> GetReportParameterExpressions()
        {
            if (ReportElement.ReportParameters != null)
            {
                foreach (var reportParameter in ReportElement.ReportParameters)
                {
                    string expression;

                    // Valid Values
                    if (reportParameter.ValidValues != null && reportParameter.ValidValues.ParameterValues != null)
                        foreach (var parameterValue in reportParameter.ValidValues.ParameterValues)
                        {
                            if (LocalReportsEngineCommon.IsExpression(parameterValue.Label, out expression))
                                yield return expression;

                            if (LocalReportsEngineCommon.IsExpression(parameterValue.Value, out expression))
                                yield return expression;
                        }

                    // Default Values
                    if (reportParameter.DefaultValue != null && reportParameter.DefaultValue.Values != null)
                        foreach (var value in reportParameter.DefaultValue.Values)
                            if (LocalReportsEngineCommon.IsExpression(value, out expression))
                                yield return expression;
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
    }
}
