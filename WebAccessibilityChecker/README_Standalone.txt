Web Accessibility Checker - Standalone Windows Application
=======================================================

This is a standalone, self-contained Windows application that does not require .NET to be installed on the target machine.

Installation & Usage:
1. Download and extract the WebAccessibilityChecker_Standalone.zip file
2. Run WebAccessibilityChecker.exe
3. The application will start immediately - no installation required

Features:
- Analyze websites for WCAG, Section 508, and RPwD Act compliance
- Check dynamic websites using headless browser rendering
- Export reports in TXT, PDF, and W3C-compliant HTML formats
- Visual charts showing accessibility scores
- Color-coded results by category and severity
- Improved scoring algorithm aligned with industry standards (Lighthouse-style)

Scoring Algorithm Improvements (v2.0):
- More nuanced scoring: Critical issues (-3 points), Warnings (-1 point), Volume penalties
- Modern accessibility patterns: aria-label, aria-labelledby support
- Context-aware checks: Decorative images, wrapped labels, CSS limitations noted
- Realistic severity levels: Many checks changed from Error to Warning/Info
- Better alignment with Google Lighthouse accessibility scoring

System Requirements:
- Windows 10 or later (64-bit)
- No additional software installation required

File Size: ~128MB (includes all necessary runtime components)

Created with .NET 9.0 and published as a self-contained application.
Version 2.0 - Improved scoring algorithm for better real-world accuracy.