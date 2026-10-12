using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using WebAccessibilityChecker.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebAccessibilityChecker.Utils
{
    public class ExportHelper
    {
        public void ExportToTxt(Report report, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                 AXIS WEB ACCESSIBILITY & QUALITY AUDIT REPORT                  ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Website / Target : {report.WebsiteUrl}");
            sb.AppendLine($"Audit Timestamp  : {report.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Overall Grade    : {report.OverallGrade} ({report.OverallScore}/100)");
            sb.AppendLine($"Compliance Status: {report.ComplianceStatus}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"Total Issues Found: {report.TotalIssues}");
            sb.AppendLine($"  🔴 Critical / Errors  : {report.ErrorCount}");
            sb.AppendLine($"  🟡 Warnings / Serious : {report.WarningCount}");
            sb.AppendLine($"  🔵 Info / Minor       : {report.InfoCount}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("Category Scores:");
            sb.AppendLine($"  ♿ Accessibility: {report.AccessibilityScore}/100");
            sb.AppendLine($"  🔍 SEO          : {report.SEOScore}/100");
            sb.AppendLine($"  ⚡ Performance  : {report.PerformanceScore}/100");
            sb.AppendLine($"  ✨ BestPractices: {report.BestPracticesScore}/100");
            sb.AppendLine($"  🌱 Environment  : {report.EnvironmentScore}/100 (Rating: {report.EnvironmentalRating ?? "Eco"})");
            sb.AppendLine($"  🔒 Safety       : {report.SafetyScore}/100");

            if (report.PageLoadTime > 0)
            {
                sb.AppendLine("--------------------------------------------------------------------------------");
                sb.AppendLine("Page Diagnostics:");
                sb.AppendLine($"  Load Time        : {report.PageLoadTime:F2}s");
                sb.AppendLine($"  Requests         : {report.RequestCount}");
                sb.AppendLine($"  Page Size        : {FormatBytes(report.PageSize)}");
                sb.AppendLine($"  Uses CDN         : {(report.UsesCDN ? "Yes" : "No")}");
                sb.AppendLine($"  Energy / Load    : {report.EnergyConsumptionKWh:F4} kWh");
                sb.AppendLine($"  CO₂ Footprint    : {report.CO2EmissionsGrams:F2} grams");
            }
            sb.AppendLine("================================================================================");
            sb.AppendLine("DETAILED AUDIT FINDINGS");
            sb.AppendLine("================================================================================");

            int idx = 1;
            foreach (var issue in report.Issues)
            {
                sb.AppendLine();
                sb.AppendLine($"[{idx++}] {issue.SeverityBadge} | {issue.CategoryDisplay} | {issue.Type}");
                if (!string.IsNullOrEmpty(issue.WcagGuideline))
                    sb.AppendLine($"    Standard   : {issue.WcagGuideline}");
                if (!string.IsNullOrEmpty(issue.Impact))
                    sb.AppendLine($"    Impact     : {issue.Impact}");
                sb.AppendLine($"    Occurrences: {issue.Count}");
                sb.AppendLine($"    Fix Advice : {issue.SuggestedFix}");
                
                var instances = issue.ElementInstances.Any() ? issue.ElementInstances : new List<string> { issue.ElementSnippet ?? "" };
                for (int i = 0; i < instances.Count; i++)
                {
                    sb.AppendLine($"    Instance {i + 1}: {instances[i]}");
                }

                if (!string.IsNullOrEmpty(issue.FixExample))
                    sb.AppendLine($"    Example    : {issue.FixExample}");
                sb.AppendLine("--------------------------------------------------------------------------------");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public void ExportToJson(Report report, string filePath)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            string json = System.Text.Json.JsonSerializer.Serialize(report, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public void ExportToMarkdown(Report report, string filePath)
        {
            string md = GenerateMarkdownReport(report);
            File.WriteAllText(filePath, md, Encoding.UTF8);
        }

        public string GenerateMarkdownReport(Report report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 🚀 Web Accessibility & Quality Audit Report");
            sb.AppendLine();
            sb.AppendLine($"- **Target URL / File**: `{report.WebsiteUrl ?? "Local"}`");
            sb.AppendLine($"- **Audit Timestamp**: {report.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- **Overall Grade**: **{report.OverallGrade}** ({report.OverallScore}/100)");
            sb.AppendLine($"- **Compliance Status**: `{report.ComplianceStatus ?? "N/A"}`");
            sb.AppendLine();
            sb.AppendLine("## 📊 Category Scores");
            sb.AppendLine();
            sb.AppendLine("| Category | Score | Status |");
            sb.AppendLine("| :--- | :---: | :---: |");
            sb.AppendLine($"| ♿ Accessibility | **{report.AccessibilityScore}/100** | {(report.AccessibilityScore >= 80 ? "✅ Pass" : "⚠️ Remediation Needed")} |");
            sb.AppendLine($"| 🔍 SEO | **{report.SEOScore}/100** | {(report.SEOScore >= 80 ? "✅ Pass" : "⚠️ Review")} |");
            sb.AppendLine($"| ⚡ Performance | **{report.PerformanceScore}/100** | {(report.PerformanceScore >= 80 ? "✅ Pass" : "⚠️ Optimize")} |");
            sb.AppendLine($"| ✨ Best Practices | **{report.BestPracticesScore}/100** | {(report.BestPracticesScore >= 80 ? "✅ Pass" : "⚠️ Review")} |");
            sb.AppendLine($"| 🌱 Eco / Environment | **{report.EnvironmentScore}/100** | {report.EnvironmentalRating ?? "Eco"} ({report.CO2EmissionsGrams:F2}g CO₂) |");
            sb.AppendLine($"| 🔒 Safety & Security | **{report.SafetyScore}/100** | {(report.SafetyScore >= 80 ? "✅ Secure" : "⚠️ Review")} |");
            sb.AppendLine();
            sb.AppendLine($"## 🔍 Issues Overview ({report.TotalIssues} Total)");
            sb.AppendLine($"- 🔴 **Errors / Critical**: {report.ErrorCount}");
            sb.AppendLine($"- 🟡 **Warnings / Serious**: {report.WarningCount}");
            sb.AppendLine($"- 🔵 **Info / Minor**: {report.InfoCount}");
            sb.AppendLine();
            sb.AppendLine("### Detailed Issue Findings");
            sb.AppendLine();
            
            int idx = 1;
            foreach (var issue in report.Issues)
            {
                sb.AppendLine($"#### {idx++}. {issue.SeverityBadge} - {issue.Type} ({issue.CategoryDisplay})");
                if (!string.IsNullOrEmpty(issue.WcagGuideline))
                    sb.AppendLine($"- **Standard**: `{issue.WcagGuideline}`");
                if (!string.IsNullOrEmpty(issue.Impact))
                    sb.AppendLine($"- **Impact**: **{issue.Impact}**");
                sb.AppendLine($"- **Occurrences**: {issue.Count}");
                sb.AppendLine($"- **Recommended Fix**: {issue.SuggestedFix}");
                if (!string.IsNullOrEmpty(issue.ElementSnippet))
                {
                    sb.AppendLine();
                    sb.AppendLine("```html");
                    sb.AppendLine(SafeMarkdownCode(issue.ElementSnippet));
                    sb.AppendLine("```");
                }
                if (!string.IsNullOrEmpty(issue.FixExample))
                {
                    sb.AppendLine();
                    sb.AppendLine("**Remediation Example:**");
                    sb.AppendLine("```html");
                    sb.AppendLine(SafeMarkdownCode(issue.FixExample));
                    sb.AppendLine("```");
                }
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string SafeMarkdownCode(string? code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            return code.Replace("```", "`` `");
        }

        public void ExportToPdf(Report report, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Web Accessibility & Quality Audit").FontSize(20).Bold().FontColor("#007ACC");
                            col.Item().Text($"Target: {report.WebsiteUrl} | Generated: {report.AnalyzedAt:yyyy-MM-dd HH:mm}").FontSize(10).FontColor("#666666");
                        });
                        row.ConstantItem(80).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Grade: {report.OverallGrade}").FontSize(18).Bold().FontColor(report.GradeColor);
                            col.Item().Text($"{report.OverallScore}/100").FontSize(12).FontColor("#666666");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#E0E0E0");
                        
                        col.Item().Text("Executive Summary").FontSize(14).Bold();
                        col.Item().Text($"Overall Compliance: {report.ComplianceStatus} | Total Issues: {report.TotalIssues} (Errors: {report.ErrorCount}, Warnings: {report.WarningCount}, Info: {report.InfoCount})").FontSize(11);
                        
                        col.Item().PaddingTop(10).Text("Category Scores:").FontSize(12).Bold();
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Accessibility: {report.AccessibilityScore}/100");
                            r.RelativeItem().Text($"SEO: {report.SEOScore}/100");
                            r.RelativeItem().Text($"Performance: {report.PerformanceScore}/100");
                        });
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Best Practices: {report.BestPracticesScore}/100");
                            r.RelativeItem().Text($"Environment: {report.EnvironmentScore}/100");
                            r.RelativeItem().Text($"Safety: {report.SafetyScore}/100");
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#E0E0E0");
                        col.Item().Text("Detailed Findings:").FontSize(14).Bold();

                        foreach (var issue in report.Issues)
                        {
                            col.Item().PaddingVertical(4).BorderBottom(1).BorderColor("#EEEEEE").Column(ic =>
                            {
                                ic.Item().Text($"{issue.SeverityBadge} [{issue.Category}] {issue.Type} ({issue.Count} instance(s))").Bold().FontSize(11);
                                if (!string.IsNullOrEmpty(issue.WcagGuideline))
                                    ic.Item().Text($"Standard: {issue.WcagGuideline}").FontSize(9).FontColor("#555555");
                                ic.Item().Text($"Fix: {issue.SuggestedFix}").FontSize(10);
                                if (!string.IsNullOrEmpty(issue.ElementSnippet))
                                    ic.Item().Text($"Element: {issue.ElementSnippet}").FontSize(9).FontFamily("Consolas");
                                if (!string.IsNullOrEmpty(issue.FixExample))
                                    ic.Item().Text($"Example: {issue.FixExample}").FontSize(9).FontFamily("Consolas").FontColor("#007ACC");
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated by AXIS Web Accessibility Checker | Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);
        }

        public void ExportToHtml(Report report, string filePath)
        {
            var html = GenerateAccessibleHtmlReport(report);
            File.WriteAllText(filePath, html, Encoding.UTF8);
        }

        private string GenerateAccessibleHtmlReport(Report report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\" />");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
            sb.AppendLine("    <meta name=\"description\" content=\"Web Accessibility and Quality Audit Report\" />");
            sb.AppendLine("    <title>Web Accessibility Audit - " + System.Web.HttpUtility.HtmlEncode(report.WebsiteUrl ?? "Audit") + "</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        :root { --bg: #0f172a; --card: #1e293b; --border: #334155; --text: #f1f5f9; --sub: #94a3b8; --accent: #38bdf8; --err: #ef4444; --warn: #f59e0b; --succ: #10b981; }");
            sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #0b0f19; color: var(--text); margin: 0; padding: 24px; line-height: 1.6; }");
            sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; }");
            sb.AppendLine("        .header { background: var(--card); border: 1px solid var(--border); border-radius: 16px; padding: 28px; margin-bottom: 24px; display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 20px; }");
            sb.AppendLine("        .badge-grade { font-size: 2.5rem; font-weight: 800; padding: 8px 24px; border-radius: 12px; background: rgba(56,189,248,0.15); border: 2px solid " + report.GradeColor + "; color: " + report.GradeColor + "; }");
            sb.AppendLine("        .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; margin-bottom: 24px; }");
            sb.AppendLine("        .card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 20px; }");
            sb.AppendLine("        .card-num { font-size: 2rem; font-weight: 700; margin-top: 4px; color: var(--accent); }");
            sb.AppendLine("        .card-sub { font-size: 0.85rem; color: var(--sub); }");
            sb.AppendLine("        .issues { display: flex; flex-direction: column; gap: 14px; }");
            sb.AppendLine("        .issue-card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 18px; border-left: 5px solid var(--accent); }");
            sb.AppendLine("        .issue-card.error { border-left-color: var(--err); }");
            sb.AppendLine("        .issue-card.warning { border-left-color: var(--warn); }");
            sb.AppendLine("        .issue-card.info { border-left-color: var(--succ); }");
            sb.AppendLine("        .pill { display: inline-block; padding: 3px 10px; border-radius: 20px; font-size: 0.78rem; font-weight: 600; margin-right: 8px; }");
            sb.AppendLine("        .pill-err { background: rgba(239,68,68,0.2); color: #fca5a5; }");
            sb.AppendLine("        .pill-warn { background: rgba(245,158,11,0.2); color: #fde68a; }");
            sb.AppendLine("        .pill-info { background: rgba(56,189,248,0.2); color: #bae6fd; }");
            sb.AppendLine("        .wcag-tag { background: #334155; color: #cbd5e1; padding: 3px 10px; border-radius: 6px; font-size: 0.78rem; }");
            sb.AppendLine("        code, pre { font-family: ui-monospace, Menlo, Monaco, Consolas, monospace; background: #0a0f1d; padding: 10px 14px; border-radius: 8px; overflow-x: auto; display: block; border: 1px solid #1e293b; color: #38bdf8; font-size: 0.88rem; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <div>");
            sb.AppendLine("                <h1 style=\"margin:0; font-size:1.8rem;\">🚀 Web Accessibility & Quality Audit</h1>");
            sb.AppendLine($"                <p style=\"margin:6px 0 0 0; color:var(--sub);\">Target: <strong>{System.Web.HttpUtility.HtmlEncode(report.WebsiteUrl ?? "Local")}</strong> &bull; Date: {report.AnalyzedAt:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine($"                <p style=\"margin:4px 0 0 0; color:var(--accent);\">Automated Status: <strong>{System.Web.HttpUtility.HtmlEncode(report.ComplianceStatus ?? "N/A")}</strong></p>");
            sb.AppendLine("                <p style=\"margin:4px 0 0 0; color:#f59e0b; font-size:0.85rem;\">⚠️ Automated scan only. Comprehensive WCAG conformance requires manual keyboard, screen-reader, and cognitive testing.</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine($"            <div class=\"badge-grade\">Grade {report.OverallGrade} <span style=\"font-size:1.2rem; display:block; text-align:center;\">{report.OverallScore}/100</span></div>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"grid\">");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">♿ ACCESSIBILITY</div><div class=\"card-num\">{report.AccessibilityScore}</div></div>");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">🔍 SEO SCORE</div><div class=\"card-num\">{report.SEOScore}</div></div>");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">⚡ PERFORMANCE</div><div class=\"card-num\">{report.PerformanceScore}</div></div>");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">✨ BEST PRACTICES</div><div class=\"card-num\">{report.BestPracticesScore}</div></div>");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">🌱 ECO / CARBON</div><div class=\"card-num\">{report.EnvironmentScore}</div><div class=\"card-sub\">{report.CO2EmissionsGrams:F1}g CO₂</div></div>");
            sb.AppendLine($"            <div class=\"card\"><div class=\"card-sub\">🔒 SAFETY & SEC</div><div class=\"card-num\">{report.SafetyScore}</div></div>");
            sb.AppendLine("        </div>");

            sb.AppendLine($"        <h2 style=\"margin:24px 0 16px 0;\">🔍 Audit Findings ({report.TotalIssues} Issues)</h2>");
            sb.AppendLine("        <div class=\"issues\">");

            foreach (var issue in report.Issues)
            {
                string sevClass = issue.SeverityLevel switch { Severity.Error => "error", Severity.Warning => "warning", _ => "info" };
                string pillClass = issue.SeverityLevel switch { Severity.Error => "pill-err", Severity.Warning => "pill-warn", _ => "pill-info" };

                sb.AppendLine($"            <div class=\"issue-card {sevClass}\">");
                sb.AppendLine("                <div style=\"display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:8px; margin-bottom:8px;\">");
                sb.AppendLine($"                    <div><span class=\"pill {pillClass}\">{issue.SeverityBadge}</span><strong>{System.Web.HttpUtility.HtmlEncode(issue.Type ?? "")}</strong> <span style=\"color:var(--sub); font-size:0.85rem;\">({issue.CategoryDisplay})</span></div>");
                if (!string.IsNullOrEmpty(issue.WcagGuideline))
                    sb.AppendLine($"                    <span class=\"wcag-tag\">{System.Web.HttpUtility.HtmlEncode(issue.WcagGuideline)}</span>");
                sb.AppendLine("                </div>");
                sb.AppendLine($"                <p style=\"margin:6px 0;\"><strong>Fix:</strong> {System.Web.HttpUtility.HtmlEncode(issue.SuggestedFix ?? "")}</p>");
                
                if (!string.IsNullOrEmpty(issue.ElementSnippet))
                {
                    sb.AppendLine($"                <pre><code>{System.Web.HttpUtility.HtmlEncode(issue.ElementSnippet)}</code></pre>");
                }
                if (!string.IsNullOrEmpty(issue.FixExample))
                {
                    sb.AppendLine($"                <p style=\"margin:6px 0 2px 0; color:var(--accent); font-size:0.88rem;\">💡 Suggested Remediation:</p>");
                    sb.AppendLine($"                <pre><code>{System.Web.HttpUtility.HtmlEncode(issue.FixExample)}</code></pre>");
                }
                sb.AppendLine("            </div>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string FormatBytes(long bytes)
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