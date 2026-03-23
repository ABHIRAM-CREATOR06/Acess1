using System.IO;
using System.Text;
using AXIS_CORE.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using System;

namespace AXIS_CORE.Utils
{
    public class ExportHelper
    {
        public static string ExportToText(Report report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Web Accessibility Report");
            sb.AppendLine($"Website: {report.WebsiteUrl}");
            sb.AppendLine($"Total Issues: {report.TotalIssues}");
            sb.AppendLine($"Errors: {report.ErrorCount}, Warnings: {report.WarningCount}, Info: {report.InfoCount}");
            sb.AppendLine($"Accessibility Score: {report.AccessibilityScore}/100");
            sb.AppendLine($"SEO Score: {report.SEOScore}/100");
            sb.AppendLine($"Performance Score: {report.PerformanceScore}/100");
            sb.AppendLine($"Best Practices Score: {report.BestPracticesScore}/100");
            sb.AppendLine($"Environment Score: {report.EnvironmentScore}/100");
            sb.AppendLine($"Safety Score: {report.SafetyScore}/100");
            sb.AppendLine($"Compliance Status: {report.ComplianceStatus}");

            // Add category breakdown
            sb.AppendLine();
            sb.AppendLine("Category Breakdown:");
            var categoryCounts = report.Issues
                .GroupBy(i => i.Category)
                .Select(g => $"{g.Key}: {g.Count()} issues")
                .ToList();
            foreach (var category in categoryCounts)
            {
                sb.AppendLine($"- {category}");
            }
            if (report.PageLoadTime > 0)
            {
                sb.AppendLine($"Page Load Time: {report.PageLoadTime:F2}s");
                sb.AppendLine($"Request Count: {report.RequestCount}");
                sb.AppendLine($"Page Size: {FormatBytes(report.PageSize)}");
                sb.AppendLine($"Uses CDN: {(report.UsesCDN ? "Yes" : "No")}");
                sb.AppendLine($"Energy Consumption: {report.EnergyConsumptionKWh:F4} kWh per page load");
                sb.AppendLine($"CO₂ Emissions: {report.CO2EmissionsGrams:F2} grams per page load");
                sb.AppendLine($"Environmental Rating: {report.EnvironmentalRating}");
            }
            sb.AppendLine();

            foreach (var issue in report.Issues)
            {
                sb.AppendLine($"Category: {issue.Category}");
                sb.AppendLine($"Type: {issue.Type}");
                sb.AppendLine($"Severity: {issue.SeverityLevel}");
                sb.AppendLine($"Total Occurrences: {issue.Count}");
                
                // Show all unique instances instead of hiding them
                var instances = issue.ElementInstances.Any() ? issue.ElementInstances : new List<string> { issue.ElementSnippet ?? "" };
                for (int i = 0; i < instances.Count; i++)
                {
                    sb.AppendLine();
                    sb.AppendLine($"  Instance {i + 1}:");
                    sb.AppendLine($"  Element: {instances[i]}");
                    sb.AppendLine($"  Fix: {issue.SuggestedFix}");
                    if (!string.IsNullOrEmpty(issue.FixExample))
                        sb.AppendLine($"  Example: {issue.FixExample}");
                }
                sb.AppendLine("---");
            }
            return sb.ToString();
        }

        public void ExportToTxt(Report report, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Web Accessibility Report");
            sb.AppendLine($"Website: {report.WebsiteUrl}");
            sb.AppendLine($"Total Issues: {report.TotalIssues}");
            sb.AppendLine($"Errors: {report.ErrorCount}, Warnings: {report.WarningCount}, Info: {report.InfoCount}");
            sb.AppendLine($"Accessibility Score: {report.AccessibilityScore}/100");
            sb.AppendLine($"SEO Score: {report.SEOScore}/100");
            sb.AppendLine($"Performance Score: {report.PerformanceScore}/100");
            sb.AppendLine($"Best Practices Score: {report.BestPracticesScore}/100");
            sb.AppendLine($"Environment Score: {report.EnvironmentScore}/100");
            sb.AppendLine($"Safety Score: {report.SafetyScore}/100");
            sb.AppendLine($"Compliance Status: {report.ComplianceStatus}");

            // Add category breakdown
            sb.AppendLine();
            sb.AppendLine("Category Breakdown:");
            var categoryCounts = report.Issues
                .GroupBy(i => i.Category)
                .Select(g => $"{g.Key}: {g.Count()} issues")
                .ToList();
            foreach (var category in categoryCounts)
            {
                sb.AppendLine($"- {category}");
            }
            if (report.PageLoadTime > 0)
            {
                sb.AppendLine($"Page Load Time: {report.PageLoadTime:F2}s");
                sb.AppendLine($"Request Count: {report.RequestCount}");
                sb.AppendLine($"Page Size: {FormatBytes(report.PageSize)}");
                sb.AppendLine($"Uses CDN: {(report.UsesCDN ? "Yes" : "No")}");
                sb.AppendLine($"Energy Consumption: {report.EnergyConsumptionKWh:F4} kWh per page load");
                sb.AppendLine($"CO₂ Emissions: {report.CO2EmissionsGrams:F2} grams per page load");
                sb.AppendLine($"Environmental Rating: {report.EnvironmentalRating}");
            }
            sb.AppendLine();

            foreach (var issue in report.Issues)
            {
                sb.AppendLine($"Category: {issue.Category}");
                sb.AppendLine($"Type: {issue.Type}");
                sb.AppendLine($"Severity: {issue.SeverityLevel}");
                sb.AppendLine($"Total Occurrences: {issue.Count}");
                
                // Show all unique instances instead of hiding them
                var instances = issue.ElementInstances.Any() ? issue.ElementInstances : new List<string> { issue.ElementSnippet ?? "" };
                for (int i = 0; i < instances.Count; i++)
                {
                    sb.AppendLine();
                    sb.AppendLine($"  Instance {i + 1}:");
                    sb.AppendLine($"  Element: {instances[i]}");
                    sb.AppendLine($"  Fix: {issue.SuggestedFix}");
                    if (!string.IsNullOrEmpty(issue.FixExample))
                        sb.AppendLine($"  Example: {issue.FixExample}");
                }
                sb.AppendLine("---");
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        public static byte[] ExportToPdf(Report report)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Web Accessibility Report").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Website: {report.WebsiteUrl}").FontSize(14);
                        col.Item().Text($"Total Issues: {report.TotalIssues}").FontSize(14);
                        col.Item().Text($"Accessibility Score: {report.AccessibilityScore}/100").FontSize(14);
                        col.Item().Text($"SEO Score: {report.SEOScore}/100").FontSize(14);
                        col.Item().Text($"Performance Score: {report.PerformanceScore}/100").FontSize(14);
                        col.Item().Text($"Best Practices Score: {report.BestPracticesScore}/100").FontSize(14);
                        col.Item().Text($"Environment Score: {report.EnvironmentScore}/100").FontSize(14);
                        col.Item().Text($"Safety Score: {report.SafetyScore}/100").FontSize(14);
                        col.Item().Text($"Compliance Status: {report.ComplianceStatus}").FontSize(14);

                        // Add category breakdown
                        col.Item().Text("").FontSize(12);
                        col.Item().Text("Category Breakdown:").Bold().FontSize(14);
                        var categoryCounts = report.Issues
                            .GroupBy(i => i.Category)
                            .Select(g => $"{g.Key}: {g.Count()} issues")
                            .ToList();
                        foreach (var category in categoryCounts)
                        {
                            col.Item().Text($"- {category}").FontSize(12);
                        }
                        if (report.PageLoadTime > 0)
                        {
                            col.Item().Text($"Page Load Time: {report.PageLoadTime:F2}s").FontSize(12);
                            col.Item().Text($"Request Count: {report.RequestCount}").FontSize(12);
                            col.Item().Text($"Page Size: {FormatBytes(report.PageSize)}").FontSize(12);
                            col.Item().Text($"Uses CDN: {(report.UsesCDN ? "Yes" : "No")}").FontSize(12);
                            col.Item().Text($"Energy Consumption: {report.EnergyConsumptionKWh:F4} kWh").FontSize(12);
                            col.Item().Text($"CO₂ Emissions: {report.CO2EmissionsGrams:F2} grams").FontSize(12);
                            col.Item().Text($"Environmental Rating: {report.EnvironmentalRating}").FontSize(12);
                        }
                        col.Item().Text("").FontSize(12);
                        col.Item().Text("Issues:").Bold().FontSize(16);
                        foreach (var issue in report.Issues)
                        {
                            col.Item().Text($"{issue.Category} - {issue.Type} - {issue.SeverityLevel}").FontSize(12).Bold();
                            col.Item().Text($"Element: {issue.ElementSnippet}").FontSize(10);
                            col.Item().Text($"Fix: {issue.SuggestedFix}").FontSize(10);
                            if (!string.IsNullOrEmpty(issue.FixExample))
                                col.Item().Text($"Example: {issue.FixExample}").FontSize(10);
                            col.Item().Text("---").FontSize(10);
                        }
                    });
                });
            }).GeneratePdf(stream);
            return stream.ToArray();
        }

        public void ExportToPdf(Report report, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Web Accessibility Report").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Website: {report.WebsiteUrl}").FontSize(14);
                        col.Item().Text($"Total Issues: {report.TotalIssues}").FontSize(14);
                        col.Item().Text($"Accessibility Score: {report.AccessibilityScore}/100").FontSize(14);
                        col.Item().Text($"SEO Score: {report.SEOScore}/100").FontSize(14);
                        col.Item().Text($"Performance Score: {report.PerformanceScore}/100").FontSize(14);
                        col.Item().Text($"Best Practices Score: {report.BestPracticesScore}/100").FontSize(14);
                        col.Item().Text($"Environment Score: {report.EnvironmentScore}/100").FontSize(14);
                        col.Item().Text($"Safety Score: {report.SafetyScore}/100").FontSize(14);
                        col.Item().Text($"Compliance Status: {report.ComplianceStatus}").FontSize(14);

                        // Add category breakdown
                        col.Item().Text("").FontSize(12);
                        col.Item().Text("Category Breakdown:").Bold().FontSize(14);
                        var categoryCounts = report.Issues
                            .GroupBy(i => i.Category)
                            .Select(g => $"{g.Key}: {g.Count()} issues")
                            .ToList();
                        foreach (var category in categoryCounts)
                        {
                            col.Item().Text($"- {category}").FontSize(12);
                        }
                        if (report.PageLoadTime > 0)
                        {
                            col.Item().Text($"Page Load Time: {report.PageLoadTime:F2}s").FontSize(12);
                            col.Item().Text($"Request Count: {report.RequestCount}").FontSize(12);
                            col.Item().Text($"Page Size: {FormatBytes(report.PageSize)}").FontSize(12);
                            col.Item().Text($"Uses CDN: {(report.UsesCDN ? "Yes" : "No")}").FontSize(12);
                            col.Item().Text($"Energy Consumption: {report.EnergyConsumptionKWh:F4} kWh").FontSize(12);
                            col.Item().Text($"CO₂ Emissions: {report.CO2EmissionsGrams:F2} grams").FontSize(12);
                            col.Item().Text($"Environmental Rating: {report.EnvironmentalRating}").FontSize(12);
                        }
                        col.Item().Text("").FontSize(12);
                        col.Item().Text("Issues:").Bold().FontSize(16);
                        foreach (var issue in report.Issues)
                        {
                            col.Item().Text($"{issue.Category} - {issue.Type} - {issue.SeverityLevel}").FontSize(12).Bold();
                            col.Item().Text($"Element: {issue.ElementSnippet}").FontSize(10);
                            col.Item().Text($"Fix: {issue.SuggestedFix}").FontSize(10);
                            if (!string.IsNullOrEmpty(issue.FixExample))
                                col.Item().Text($"Example: {issue.FixExample}").FontSize(10);
                            col.Item().Text("---").FontSize(10);
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }

        public void ExportToHtml(Report report, string filePath)
        {
            var html = GenerateAccessibleHtmlReport(report);
            File.WriteAllText(filePath, html);
        }

        private string GenerateAccessibleHtmlReport(Report report)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\" />");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
            sb.AppendLine("    <meta name=\"description\" content=\"Web Accessibility Analysis Report\" />");
            sb.AppendLine("    <meta name=\"author\" content=\"WebAccessibilityChecker\" />");
            sb.AppendLine("    <meta name=\"generator\" content=\"WebAccessibilityChecker v1.0\" />");
            sb.AppendLine("    <title>Web Accessibility Report - " + System.Web.HttpUtility.HtmlEncode(report.WebsiteUrl ?? "Analysis") + "</title>");
            sb.AppendLine("    <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>♿</text></svg>\" />");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; background-color: #f8f9fa; margin: 0; padding: 20px; }");
            sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
            sb.AppendLine("        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; margin-bottom: 30px; }");
            sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .metric-card { background: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db; }");
            sb.AppendLine("        .metric-card h3 { margin: 0 0 10px 0; color: #2c3e50; font-size: 1.1em; }");
            sb.AppendLine("        .score { font-size: 2em; font-weight: bold; color: #27ae60; }");
            sb.AppendLine("        .score.low { color: #e74c3c; }");
            sb.AppendLine("        .score.medium { color: #f39c12; }");
            sb.AppendLine("        .issues { margin-top: 30px; }");
            sb.AppendLine("        .issue { border: 1px solid #ddd; border-radius: 8px; margin-bottom: 15px; padding: 15px; background: #fafafa; }");
            sb.AppendLine("        .issue-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }");
            sb.AppendLine("        .issue-type { font-weight: bold; color: #2c3e50; }");
            sb.AppendLine("        .severity { padding: 4px 8px; border-radius: 4px; font-size: 0.9em; font-weight: bold; }");
            sb.AppendLine("        .severity.Error { background: #fee; color: #c0392b; }");
            sb.AppendLine("        .severity.Warning { background: #fff3cd; color: #856404; }");
            sb.AppendLine("        .severity.Info { background: #d1ecf1; color: #0c5460; }");
            sb.AppendLine("        .category { display: inline-block; background: #e9ecef; color: #495057; padding: 2px 6px; border-radius: 3px; font-size: 0.8em; margin-right: 10px; }");
            sb.AppendLine("        .element { background: #f8f9fa; padding: 10px; border-radius: 4px; font-family: 'Courier New', monospace; font-size: 0.9em; margin: 10px 0; border-left: 3px solid #6c757d; }");
            sb.AppendLine("        .fix { background: #e8f5e8; padding: 10px; border-radius: 4px; margin: 10px 0; border-left: 3px solid #27ae60; }");
            sb.AppendLine("        .example { background: #fff3cd; padding: 10px; border-radius: 4px; margin: 10px 0; border-left: 3px solid #f39c12; font-family: 'Courier New', monospace; font-size: 0.9em; }");
            sb.AppendLine("        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }");
            sb.AppendLine("        .stat-item { text-align: center; padding: 15px; background: #f8f9fa; border-radius: 6px; }");
            sb.AppendLine("        .stat-value { font-size: 1.5em; font-weight: bold; color: #3498db; }");
            sb.AppendLine("        .stat-label { color: #7f8c8d; font-size: 0.9em; }");
            sb.AppendLine("        @media (max-width: 768px) { .summary { grid-template-columns: 1fr; } .stats { grid-template-columns: repeat(2, 1fr); } }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine($"        <h1>🌐 Web Accessibility Report</h1>");
            sb.AppendLine($"        <p><strong>Website:</strong> {System.Web.HttpUtility.HtmlEncode(report.WebsiteUrl ?? "N/A")}</p>");
            sb.AppendLine($"        <p><strong>Analysis Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            // Summary Section
            sb.AppendLine("        <section aria-labelledby=\"summary-heading\">");
            sb.AppendLine("            <h2 id=\"summary-heading\">📊 Summary</h2>");
            sb.AppendLine("            <div class=\"summary\">");

            // Accessibility Score
            var accClass = report.AccessibilityScore >= 90 ? "score" : report.AccessibilityScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>♿ Accessibility</h3>");
            sb.AppendLine($"                    <div class=\"{accClass}\">{report.AccessibilityScore}/100</div>");
            sb.AppendLine("                </div>");

            // SEO Score
            var seoClass = report.SEOScore >= 90 ? "score" : report.SEOScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>🔍 SEO</h3>");
            sb.AppendLine($"                    <div class=\"{seoClass}\">{report.SEOScore}/100</div>");
            sb.AppendLine("                </div>");

            // Performance Score
            var perfClass = report.PerformanceScore >= 90 ? "score" : report.PerformanceScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>⚡ Performance</h3>");
            sb.AppendLine($"                    <div class=\"{perfClass}\">{report.PerformanceScore}/100</div>");
            sb.AppendLine("                </div>");

            // Best Practices Score
            var bpClass = report.BestPracticesScore >= 90 ? "score" : report.BestPracticesScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>✨ Best Practices</h3>");
            sb.AppendLine($"                    <div class=\"{bpClass}\">{report.BestPracticesScore}/100</div>");
            sb.AppendLine("                </div>");

            // Environment Score
            var envClass = report.EnvironmentScore >= 90 ? "score" : report.EnvironmentScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>🌱 Environment</h3>");
            sb.AppendLine($"                    <div class=\"{envClass}\">{report.EnvironmentScore}/100</div>");
            sb.AppendLine("                </div>");

            // Safety Score
            var safetyClass = report.SafetyScore >= 90 ? "score" : report.SafetyScore >= 70 ? "score medium" : "score low";
            sb.AppendLine("                <div class=\"metric-card\">");
            sb.AppendLine("                    <h3>🔒 Safety</h3>");
            sb.AppendLine($"                    <div class=\"{safetyClass}\">{report.SafetyScore}/100</div>");
            sb.AppendLine("                </div>");

            sb.AppendLine("            </div>");
            sb.AppendLine("        </section>");

            // Charts Section
            sb.AppendLine("        <section aria-labelledby=\"charts-heading\">");
            sb.AppendLine("            <h2 id=\"charts-heading\">📊 Score Visualization</h2>");
            sb.AppendLine("            <div style=\"display: flex; flex-wrap: wrap; gap: 20px; justify-content: center; margin: 20px 0;\">");
            sb.AppendLine(GenerateScoreChart(report));
            sb.AppendLine("            </div>");
            sb.AppendLine("        </section>");

            // Statistics
            sb.AppendLine("        <section aria-labelledby=\"stats-heading\">");
            sb.AppendLine("            <h2 id=\"stats-heading\">📈 Statistics</h2>");
            sb.AppendLine("            <div class=\"stats\">");
            sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.TotalIssues}</div><div class=\"stat-label\">Total Issues</div></div>");
            sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.ErrorCount}</div><div class=\"stat-label\">Errors</div></div>");
            sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.WarningCount}</div><div class=\"stat-label\">Warnings</div></div>");
            sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.InfoCount}</div><div class=\"stat-label\">Info</div></div>");

            if (report.PageLoadTime > 0)
            {
                sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.PageLoadTime:F1}s</div><div class=\"stat-label\">Load Time</div></div>");
                sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.RequestCount}</div><div class=\"stat-label\">Requests</div></div>");
                sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{(report.PageSize / 1024.0):F1}KB</div><div class=\"stat-label\">Page Size</div></div>");
                sb.AppendLine($"                <div class=\"stat-item\"><div class=\"stat-value\">{report.SustainabilityScore:F1}g</div><div class=\"stat-label\">CO₂ Footprint</div></div>");
            }

            sb.AppendLine("            </div>");
            sb.AppendLine("        </section>");

            // Compliance Status
            sb.AppendLine("        <section aria-labelledby=\"compliance-heading\">");
            sb.AppendLine("            <h2 id=\"compliance-heading\">✅ Compliance Status</h2>");
            sb.AppendLine($"            <p><strong>Overall Status:</strong> {System.Web.HttpUtility.HtmlEncode(report.ComplianceStatus ?? "Unknown")}</p>");
            sb.AppendLine("        </section>");

            // Issues Section
            sb.AppendLine("        <section class=\"issues\" aria-labelledby=\"issues-heading\">");
            sb.AppendLine("            <h2 id=\"issues-heading\">🔧 Detailed Issues</h2>");

            var groupedIssues = report.Issues.GroupBy(i => i.Category);
            foreach (var group in groupedIssues)
            {
                sb.AppendLine($"            <h3>{group.Key} Issues ({group.Count()})</h3>");
                foreach (var issue in group)
                {
                    sb.AppendLine("            <div class=\"issue\">");
                    sb.AppendLine("                <div class=\"issue-header\">");
                    sb.AppendLine($"                    <span class=\"category\">{issue.Category}</span>");
                    sb.AppendLine($"                    <span class=\"issue-type\">{System.Web.HttpUtility.HtmlEncode(issue.Type ?? "Unknown")}</span>");
                    sb.AppendLine($"                    <span class=\"severity {issue.SeverityLevel}\">{issue.SeverityLevel}</span>");
                    sb.AppendLine("                </div>");

                    if (!string.IsNullOrEmpty(issue.ElementSnippet))
                    {
                        sb.AppendLine("                <div class=\"element\">");
                        sb.AppendLine($"                    <strong>Element:</strong> <code>{System.Web.HttpUtility.HtmlEncode(issue.ElementSnippet)}</code>");
                        sb.AppendLine("                </div>");
                    }

                    if (!string.IsNullOrEmpty(issue.SuggestedFix))
                    {
                        sb.AppendLine("                <div class=\"fix\">");
                        sb.AppendLine($"                    <strong>Fix:</strong> {System.Web.HttpUtility.HtmlEncode(issue.SuggestedFix)}");
                        sb.AppendLine("                </div>");
                    }

                    if (!string.IsNullOrEmpty(issue.FixExample))
                    {
                        sb.AppendLine("                <div class=\"example\">");
                        sb.AppendLine($"                    <strong>Example:</strong><br><code>{System.Web.HttpUtility.HtmlEncode(issue.FixExample)}</code>");
                        sb.AppendLine("                </div>");
                    }

                    sb.AppendLine("            </div>");
                }
            }

            sb.AppendLine("        </section>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GenerateScoreChart(Report report)
        {
            var scores = new[]
            {
                ("Accessibility", report.AccessibilityScore),
                ("SEO", report.SEOScore),
                ("Performance", report.PerformanceScore),
                ("Best Practices", report.BestPracticesScore),
                ("Environment", report.EnvironmentScore),
                ("Safety", report.SafetyScore)
            };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<svg width=\"400\" height=\"300\" viewBox=\"0 0 400 300\" xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-labelledby=\"chart-title\">");
            sb.AppendLine("    <title id=\"chart-title\">Accessibility Scores Chart</title>");
            sb.AppendLine("    <g transform=\"translate(50,50)\">");

            // Draw axes
            sb.AppendLine("        <line x1=\"0\" y1=\"200\" x2=\"300\" y2=\"200\" stroke=\"#333\" stroke-width=\"2\" />");
            sb.AppendLine("        <line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"200\" stroke=\"#333\" stroke-width=\"2\" />");

            // Draw grid lines
            for (int i = 0; i <= 100; i += 20)
            {
                int y = 200 - (i * 2);
                sb.AppendLine($"        <line x1=\"0\" y1=\"{y}\" x2=\"300\" y2=\"{y}\" stroke=\"#ddd\" stroke-width=\"1\" />");
                sb.AppendLine($"        <text x=\"-10\" y=\"{y + 5}\" text-anchor=\"end\" font-size=\"12\" fill=\"#666\">{i}</text>");
            }

            // Draw bars
            string[] colors = { "#3498db", "#e74c3c", "#f39c12", "#27ae60", "#17a2b8", "#6f42c1" };
            for (int i = 0; i < scores.Length; i++)
            {
                var (label, score) = scores[i];
                int x = i * 75 + 20;
                int height = score * 2;
                int y = 200 - height;

                sb.AppendLine($"        <rect x=\"{x}\" y=\"{y}\" width=\"40\" height=\"{height}\" fill=\"{colors[i]}\" stroke=\"#333\" stroke-width=\"1\">");
                sb.AppendLine($"            <title>{label}: {score}/100</title>");
                sb.AppendLine("        </rect>");
                sb.AppendLine($"        <text x=\"{x + 20}\" y=\"{y - 10}\" text-anchor=\"middle\" font-size=\"12\" fill=\"#333\">{score}</text>");
                sb.AppendLine($"        <text x=\"{x + 20}\" y=\"220\" text-anchor=\"middle\" font-size=\"10\" fill=\"#666\" transform=\"rotate(45,{x + 20},220)\">{label}</text>");
            }

            sb.AppendLine("    </g>");
            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:F2} {sizes[order]}";
        }
    }
}