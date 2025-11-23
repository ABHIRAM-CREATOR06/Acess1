class AccessibilityEquations {
    // 1. Accessibility Score (Penalty Model)
    static calculateAccessibilityScore(errorCount, warningCount, infoCount) {
        const penalty = 10 * errorCount + 5 * warningCount + 1 * infoCount;
        return Math.max(0, 100 - penalty);
    }

    // 2. WCAG Color Contrast Ratio
    static calculateContrastRatio(lum1, lum2) {
        const brighter = Math.max(lum1, lum2);
        const darker = Math.min(lum1, lum2);
        return (brighter + 0.05) / (darker + 0.05);
    }

    // 3. Relative Luminance
    static calculateRelativeLuminance(r, g, b) {
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    // 4. sRGB to Linear RGB Conversion
    static srgbToLinear(c) {
        return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    }

    // 5. Hex to RGB Normalization
    static hexToNormalizedRGB(hex) {
        if (!hex.startsWith('#') || hex.length !== 7) {
            throw new Error('Invalid hex color format');
        }
        const r = parseInt(hex.substring(1, 3), 16) / 255;
        const g = parseInt(hex.substring(3, 5), 16) / 255;
        const b = parseInt(hex.substring(5, 7), 16) / 255;
        return { r, g, b };
    }

    // 6. Precision, Recall, F1
    static calculatePrecision(tp, fp) {
        return tp / (tp + fp);
    }

    static calculateRecall(tp, fn) {
        return tp / (tp + fn);
    }

    static calculateF1(precision, recall) {
        return 2 * precision * recall / (precision + recall);
    }

    // 7. False Positive & False Negative Rates
    static calculateFPR(fp, tn) {
        return fp / (fp + tn);
    }

    static calculateFNR(fn, tp) {
        return fn / (fn + tp);
    }

    // 8. Jaccard Similarity Between Tools A and B
    static calculateJaccardSimilarity(issuesA, issuesB) {
        const setA = new Set(issuesA);
        const setB = new Set(issuesB);
        const intersection = new Set([...setA].filter(x => setB.has(x)));
        const union = new Set([...setA, ...setB]);
        return union.size === 0 ? 0 : intersection.size / union.size;
    }

    // 9. Complementarity of Tool B over Tool A
    static calculateComplementarity(issuesA, issuesB) {
        const setA = new Set(issuesA);
        const setB = new Set(issuesB);
        const uniqueB = [...setB].filter(x => !setA.has(x)).length;
        return setA.size === 0 ? 0 : uniqueB / setA.size;
    }

    // 10. Marginal Gain of Adding Tool t to Ensemble E
    static calculateMarginalGain(ensembleIssues, newEnsembleIssues) {
        const gain = newEnsembleIssues.length - ensembleIssues.length;
        return ensembleIssues.length === 0 ? 0 : gain / ensembleIssues.length;
    }

    // 11. Ensemble Coverage
    static calculateEnsembleCoverage(ensembleIssues, allIssues) {
        return allIssues.length === 0 ? 0 : ensembleIssues.length / allIssues.length;
    }

    // 12. Ensemble Redundancy (simplified - assuming multiplicity of 1 for each issue)
    static calculateEnsembleRedundancy(ensembleIssues) {
        if (ensembleIssues.length === 0) return 0;
        // For simplicity, assuming each issue appears once
        return 1.0; // Average multiplicity of 1
    }

    // 13. Cost per New Issue When Adding Tool t
    static calculateCostPerNewIssue(costT, ensembleIssues, newEnsembleIssues) {
        const newIssues = newEnsembleIssues.length - ensembleIssues.length;
        return newIssues === 0 ? Infinity : costT / newIssues;
    }

    // 14. Composite Accessibility Index (WCAG + Screen Reader)
    static calculateCompositeAccessibilityIndex(srci100, w100, alpha) {
        return alpha * srci100 + (1 - alpha) * w100;
    }

    // 15. Confidence Score for Issue Instance i (simplified)
    static calculateConfidenceScore(toolData) {
        let weightedSum = 0;
        let totalWeight = 0;
        for (const data of Object.values(toolData)) {
            weightedSum += data.weight * data.x;
            totalWeight += data.weight;
        }
        return totalWeight === 0 ? 0 : weightedSum / totalWeight;
    }

    // 16. Improvement Rate Over Time
    static calculateImprovementRate(score1, score0, time) {
        return (score1 - score0) / time;
    }

    // 17. Failures Normalized Per 1000 Elements
    static calculateFailuresPerThousand(failures, elements) {
        return elements === 0 ? 0 : failures / elements * 1000;
    }

    // 18. Weighted Issue Impact
    static calculateWeightedIssueImpact(issues) {
        return issues.reduce((sum, issue) => sum + issue.severity * issue.probability, 0);
    }
}