using System;

namespace WebAccessibilityChecker.Models
{
    public enum Severity { Error, Warning, Info }

    public class Issue
    {
        public string? Type { get; set; }
        public string? ElementSnippet { get; set; }
        public string? SuggestedFix { get; set; }
        public Severity SeverityLevel { get; set; }
        public string? FixExample { get; set; } // For bonus
    }
}