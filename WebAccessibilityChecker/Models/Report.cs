using System.Collections.Generic;
using System.Linq;

namespace WebAccessibilityChecker.Models
{
    public class Report
    {
        public List<Issue> Issues { get; set; } = new List<Issue>();
        public int TotalIssues => Issues.Count;
        public int ErrorCount => Issues.Count(i => i.SeverityLevel == Severity.Error);
        public int WarningCount => Issues.Count(i => i.SeverityLevel == Severity.Warning);
        public int InfoCount => Issues.Count(i => i.SeverityLevel == Severity.Info);
        public int AccessibilityScore { get; set; }
        public string? ComplianceStatus { get; set; }
    }
}