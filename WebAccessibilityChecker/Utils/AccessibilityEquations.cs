using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAccessibilityChecker.Utils
{
    public static class AccessibilityEquations
    {
        // 1. Accessibility Score (Penalty Model)
        public static double CalculateAccessibilityScore(int errorCount, int warningCount, int infoCount)
        {
            double penalty = 10.0 * errorCount + 5.0 * warningCount + 1.0 * infoCount;
            return Math.Max(0, 100 - penalty);
        }

        // 2. WCAG Color Contrast Ratio
        public static double CalculateContrastRatio(double lum1, double lum2)
        {
            double brighter = Math.Max(lum1, lum2);
            double darker = Math.Min(lum1, lum2);
            return (brighter + 0.05) / (darker + 0.05);
        }

        // 3. Relative Luminance
        public static double CalculateRelativeLuminance(double r, double g, double b)
        {
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        // 4. sRGB to Linear RGB Conversion
        public static double SRGBToLinear(double c)
        {
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        // 5. Hex to RGB Normalization
        public static (double r, double g, double b) HexToNormalizedRGB(string hex)
        {
            if (hex.StartsWith("#") && hex.Length == 7)
            {
                int r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                return (r / 255.0, g / 255.0, b / 255.0);
            }
            throw new ArgumentException("Invalid hex color format");
        }

        // 6. Precision, Recall, F1
        public static double CalculatePrecision(int tp, int fp)
        {
            return tp / (double)(tp + fp);
        }

        public static double CalculateRecall(int tp, int fn)
        {
            return tp / (double)(tp + fn);
        }

        public static double CalculateF1(double precision, double recall)
        {
            return 2 * precision * recall / (precision + recall);
        }

        // 7. False Positive & False Negative Rates
        public static double CalculateFPR(int fp, int tn)
        {
            return fp / (double)(fp + tn);
        }

        public static double CalculateFNR(int fn, int tp)
        {
            return fn / (double)(fn + tp);
        }

        // 8. Jaccard Similarity Between Tools A and B
        public static double CalculateJaccardSimilarity(ISet<string> issuesA, ISet<string> issuesB)
        {
            int intersection = issuesA.Intersect(issuesB).Count();
            int union = issuesA.Union(issuesB).Count();
            return union == 0 ? 0 : intersection / (double)union;
        }

        // 9. Complementarity of Tool B over Tool A
        public static double CalculateComplementarity(ISet<string> issuesA, ISet<string> issuesB)
        {
            int uniqueB = issuesB.Except(issuesA).Count();
            return issuesA.Count == 0 ? 0 : uniqueB / (double)issuesA.Count;
        }

        // 10. Marginal Gain of Adding Tool t to Ensemble E
        public static double CalculateMarginalGain(ISet<string> ensembleIssues, ISet<string> newEnsembleIssues)
        {
            int gain = newEnsembleIssues.Count - ensembleIssues.Count;
            return ensembleIssues.Count == 0 ? 0 : gain / (double)ensembleIssues.Count;
        }

        // 11. Ensemble Coverage
        public static double CalculateEnsembleCoverage(ISet<string> ensembleIssues, ISet<string> allIssues)
        {
            return allIssues.Count == 0 ? 0 : ensembleIssues.Count / (double)allIssues.Count;
        }

        // 12. Ensemble Redundancy
        public static double CalculateEnsembleRedundancy(ISet<string> ensembleIssues, Dictionary<string, int> multiplicity)
        {
            if (ensembleIssues.Count == 0) return 0;
            double sum = 0;
            foreach (var issue in ensembleIssues)
            {
                sum += multiplicity.ContainsKey(issue) ? multiplicity[issue] : 1;
            }
            return sum / ensembleIssues.Count;
        }

        // 13. Cost per New Issue When Adding Tool t
        public static double CalculateCostPerNewIssue(double costT, ISet<string> ensembleIssues, ISet<string> newEnsembleIssues)
        {
            int newIssues = newEnsembleIssues.Count - ensembleIssues.Count;
            return newIssues == 0 ? double.PositiveInfinity : costT / newIssues;
        }

        // 14. Composite Accessibility Index (WCAG + Screen Reader)
        public static double CalculateCompositeAccessibilityIndex(double srci100, double w100, double alpha)
        {
            return alpha * srci100 + (1 - alpha) * w100;
        }

        // 15. Confidence Score for Issue Instance i
        public static double CalculateConfidenceScore(Dictionary<string, (double weight, double x)> toolData)
        {
            double weightedSum = 0;
            double totalWeight = 0;
            foreach (var data in toolData.Values)
            {
                weightedSum += data.weight * data.x;
                totalWeight += data.weight;
            }
            return totalWeight == 0 ? 0 : weightedSum / totalWeight;
        }

        // 16. Improvement Rate Over Time
        public static double CalculateImprovementRate(double score1, double score0, double time)
        {
            return (score1 - score0) / time;
        }

        // 17. Failures Normalized Per 1000 Elements
        public static double CalculateFailuresPerThousand(int failures, int elements)
        {
            return elements == 0 ? 0 : failures / (double)elements * 1000;
        }

        // 18. Weighted Issue Impact
        public static double CalculateWeightedIssueImpact(List<(double severity, double probability)> issues)
        {
            return issues.Sum(issue => issue.severity * issue.probability);
        }
    }
}