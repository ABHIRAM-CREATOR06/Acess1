using System.IO;
using System.Text;
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
            sb.AppendLine("Web Accessibility Report");
            sb.AppendLine($"Total Issues: {report.TotalIssues}");
            sb.AppendLine($"Errors: {report.ErrorCount}, Warnings: {report.WarningCount}, Info: {report.InfoCount}");
            sb.AppendLine($"Accessibility Score: {report.AccessibilityScore}/100");
            sb.AppendLine($"Compliance Status: {report.ComplianceStatus}");
            sb.AppendLine();

            foreach (var issue in report.Issues)
            {
                sb.AppendLine($"Type: {issue.Type}");
                sb.AppendLine($"Severity: {issue.SeverityLevel}");
                sb.AppendLine($"Element: {issue.ElementSnippet}");
                sb.AppendLine($"Fix: {issue.SuggestedFix}");
                if (!string.IsNullOrEmpty(issue.FixExample))
                    sb.AppendLine($"Example: {issue.FixExample}");
                sb.AppendLine("---");
            }
            File.WriteAllText(filePath, sb.ToString());
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
                        col.Item().Text($"Total Issues: {report.TotalIssues}").FontSize(14);
                        col.Item().Text($"Accessibility Score: {report.AccessibilityScore}/100").FontSize(14);
                        col.Item().Text($"Compliance Status: {report.ComplianceStatus}").FontSize(14);
                        col.Item().Text("").FontSize(12);
                        col.Item().Text("Issues:").Bold().FontSize(16);
                        foreach (var issue in report.Issues)
                        {
                            col.Item().Text($"{issue.Type} - {issue.SeverityLevel}").FontSize(12).Bold();
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
    }
}