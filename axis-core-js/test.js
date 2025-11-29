const AxisCore = require('./index');

async function test() {
    const checker = new AxisCore();

    console.log('AXIS-CORE JavaScript SDK Test');
    console.log('==============================\n');

    // Test HTML checking
    console.log('Testing HTML content check...');
    const html = `
        <!DOCTYPE html>
        <html>
        <head><title>Test Page</title></head>
        <body>
            <h1>Main Heading</h1>
            <img src="test.jpg" alt="Test image">
            <img src="test2.jpg">
            <input type="text" id="name">
            <label for="name">Name:</label>
        </body>
        </html>
    `;

    const report = checker.checkHtml(html, 'https://example.com');

    console.log(`Accessibility Score: ${report.accessibilityScore}/100`);
    console.log(`Total Issues: ${report.totalIssues}`);
    console.log(`Compliance: ${report.complianceStatus}\n`);

    if (report.issues.length > 0) {
        console.log('Issues found:');
        report.issues.forEach((issue, i) => {
            console.log(`${i + 1}. ${issue.type} (${issue.severity})`);
            console.log(`   ${issue.suggestedFix}\n`);
        });
    }

    console.log('Test completed successfully!');
}

test().catch(console.error);