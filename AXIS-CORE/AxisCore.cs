using System;
using System.Threading.Tasks;
using AXIS_CORE.Models;
using AXIS_CORE.Services;
using AXIS_CORE.Utils;

namespace AXIS_CORE
{
    /// <summary>
    /// AXIS-CORE SDK for programmatic web accessibility checking
    /// </summary>
    public class AxisCore
    {
        private readonly AccessibilityChecker _checker;
        private readonly HtmlParser _parser;

        /// <summary>
        /// Initializes a new instance of the AxisCore SDK
        /// </summary>
        public AxisCore()
        {
            _checker = new AccessibilityChecker();
            _parser = new HtmlParser();
        }

        /// <summary>
        /// Checks accessibility of a web page by URL
        /// </summary>
        /// <param name="url">The URL of the web page to check</param>
        /// <returns>Accessibility report</returns>
        public async Task<Report> CheckUrlAsync(string url)
        {
            var loadResult = await _parser.LoadPageAsync(url);
            return _checker.CheckAccessibility(loadResult);
        }

        /// <summary>
        /// Checks accessibility of HTML content
        /// </summary>
        /// <param name="htmlContent">The HTML content to check</param>
        /// <param name="baseUrl">Optional base URL for resolving relative links</param>
        /// <returns>Accessibility report</returns>
        public Report CheckHtml(string htmlContent, string? baseUrl = null)
        {
            var loadResult = _parser.ParseHtml(htmlContent, baseUrl);
            return _checker.CheckAccessibility(loadResult);
        }

        /// <summary>
        /// Exports a report to text format
        /// </summary>
        /// <param name="report">The accessibility report</param>
        /// <returns>Formatted text report</returns>
        public string ExportToText(Report report)
        {
            return ExportHelper.ExportToText(report);
        }

        /// <summary>
        /// Exports a report to PDF format
        /// </summary>
        /// <param name="report">The accessibility report</param>
        /// <returns>PDF document bytes</returns>
        public byte[] ExportToPdf(Report report)
        {
            return ExportHelper.ExportToPdf(report);
        }

        /// <summary>
        /// Gets the SDK version
        /// </summary>
        public static string Version => "1.0.0";

        /// <summary>
        /// Gets the embedded logo as byte array
        /// </summary>
        public static byte[] GetLogo()
        {
            var assembly = typeof(AxisCore).Assembly;
            using var stream = assembly.GetManifestResourceStream("AXIS_CORE.Resources.logo.png");
            if (stream == null) return Array.Empty<byte>();

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}