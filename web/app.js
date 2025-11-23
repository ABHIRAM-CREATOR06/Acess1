function calculateAccessibilityScore() {
    const errorCount = parseInt(document.getElementById('errorCount').value) || 0;
    const warningCount = parseInt(document.getElementById('warningCount').value) || 0;
    const infoCount = parseInt(document.getElementById('infoCount').value) || 0;

    const score = AccessibilityEquations.calculateAccessibilityScore(errorCount, warningCount, infoCount);
    document.getElementById('accessibilityScore').textContent = score.toFixed(2);
}

function calculateContrastRatio() {
    const lum1 = parseFloat(document.getElementById('lum1').value) || 0;
    const lum2 = parseFloat(document.getElementById('lum2').value) || 0;

    const ratio = AccessibilityEquations.calculateContrastRatio(lum1, lum2);
    document.getElementById('contrastRatio').textContent = ratio.toFixed(2);
}

function calculateRelativeLuminance() {
    const r = parseFloat(document.getElementById('r').value) || 0;
    const g = parseFloat(document.getElementById('g').value) || 0;
    const b = parseFloat(document.getElementById('b').value) || 0;

    const luminance = AccessibilityEquations.calculateRelativeLuminance(r, g, b);
    document.getElementById('relativeLuminance').textContent = luminance.toFixed(4);
}

function calculateSRGBToLinear() {
    const srgb = parseFloat(document.getElementById('srgb').value) || 0;

    const linear = AccessibilityEquations.srgbToLinear(srgb);
    document.getElementById('linearRGB').textContent = linear.toFixed(4);
}

function calculateHexToRGB() {
    const hex = document.getElementById('hexColor').value;

    try {
        const rgb = AccessibilityEquations.hexToNormalizedRGB(hex);
        document.getElementById('rgbValues').textContent = `R: ${rgb.r.toFixed(3)}, G: ${rgb.g.toFixed(3)}, B: ${rgb.b.toFixed(3)}`;
    } catch (error) {
        document.getElementById('rgbValues').textContent = 'Invalid hex format';
    }
}

function calculateMLMetrics() {
    const tp = parseInt(document.getElementById('tp').value) || 0;
    const fp = parseInt(document.getElementById('fp').value) || 0;
    const tn = parseInt(document.getElementById('tn').value) || 0;
    const fn = parseInt(document.getElementById('fn').value) || 0;

    const precision = AccessibilityEquations.calculatePrecision(tp, fp);
    const recall = AccessibilityEquations.calculateRecall(tp, fn);
    const f1 = AccessibilityEquations.calculateF1(precision, recall);
    const fpr = AccessibilityEquations.calculateFPR(fp, tn);
    const fnr = AccessibilityEquations.calculateFNR(fn, tp);

    document.getElementById('precision').textContent = precision.toFixed(4);
    document.getElementById('recall').textContent = recall.toFixed(4);
    document.getElementById('f1').textContent = f1.toFixed(4);
    document.getElementById('fpr').textContent = fpr.toFixed(4);
    document.getElementById('fnr').textContent = fnr.toFixed(4);
}

function calculateEnsembleMetrics() {
    const issuesA = document.getElementById('issuesA').value.split(',').map(s => s.trim()).filter(s => s);
    const issuesB = document.getElementById('issuesB').value.split(',').map(s => s.trim()).filter(s => s);
    const ensembleIssues = document.getElementById('ensembleIssues').value.split(',').map(s => s.trim()).filter(s => s);
    const newEnsembleIssues = document.getElementById('newEnsembleIssues').value.split(',').map(s => s.trim()).filter(s => s);
    const allIssues = document.getElementById('allIssues').value.split(',').map(s => s.trim()).filter(s => s);
    const costT = parseFloat(document.getElementById('costT').value) || 0;

    const jaccard = AccessibilityEquations.calculateJaccardSimilarity(issuesA, issuesB);
    const complementarity = AccessibilityEquations.calculateComplementarity(issuesA, issuesB);
    const marginalGain = AccessibilityEquations.calculateMarginalGain(ensembleIssues, newEnsembleIssues);
    const coverage = AccessibilityEquations.calculateEnsembleCoverage(ensembleIssues, allIssues);
    const costPerNew = AccessibilityEquations.calculateCostPerNewIssue(costT, ensembleIssues, newEnsembleIssues);

    document.getElementById('jaccard').textContent = jaccard.toFixed(4);
    document.getElementById('complementarity').textContent = complementarity.toFixed(4);
    document.getElementById('marginalGain').textContent = marginalGain.toFixed(4);
    document.getElementById('coverage').textContent = coverage.toFixed(4);
    document.getElementById('costPerNew').textContent = isFinite(costPerNew) ? costPerNew.toFixed(4) : '∞';
}

function calculateCompositeMetrics() {
    const srci100 = parseFloat(document.getElementById('srci100').value) || 0;
    const w100 = parseFloat(document.getElementById('w100').value) || 0;
    const alpha = parseFloat(document.getElementById('alpha').value) || 0;
    const score1 = parseFloat(document.getElementById('score1').value) || 0;
    const score0 = parseFloat(document.getElementById('score0').value) || 0;
    const time = parseFloat(document.getElementById('time').value) || 1;
    const failures = parseInt(document.getElementById('failures').value) || 0;
    const elements = parseInt(document.getElementById('elements').value) || 1;

    const compositeAI = AccessibilityEquations.calculateCompositeAccessibilityIndex(srci100, w100, alpha);
    const improvementRate = AccessibilityEquations.calculateImprovementRate(score1, score0, time);
    const failuresPerK = AccessibilityEquations.calculateFailuresPerThousand(failures, elements);

    document.getElementById('compositeAI').textContent = compositeAI.toFixed(2);
    document.getElementById('improvementRate').textContent = improvementRate.toFixed(4);
    document.getElementById('failuresPerK').textContent = failuresPerK.toFixed(2);
}

// Initialize with default calculations
document.addEventListener('DOMContentLoaded', function() {
    calculateAccessibilityScore();
    calculateContrastRatio();
    calculateRelativeLuminance();
    calculateSRGBToLinear();
    calculateHexToRGB();
    calculateMLMetrics();
    calculateEnsembleMetrics();
    calculateCompositeMetrics();
});