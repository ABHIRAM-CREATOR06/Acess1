# 🚀 Web Accessibility Checker

A comprehensive Windows desktop application for checking web accessibility compliance with WCAG guidelines, featuring modern UI design and advanced JavaScript rendering capabilities.

## ✨ Features

### Core Functionality
- **URL Analysis**: Check any website for accessibility issues
- **File Upload**: Analyze local HTML files
- **WCAG Compliance**: Automated checks against WCAG 2.1 standards
- **Real-time Scoring**: Get instant accessibility scores (0-100)
- **Compliance Status**: Determine if your site is Fully/Mostly/Partially/Not Compliant

### Advanced Checks
- **Alt Text Validation**: Ensure all images have proper alt attributes
- **Form Accessibility**: Check labels and form controls
- **Heading Hierarchy**: Validate proper heading structure
- **Color Contrast**: Analyze text/background color ratios
- **Eye Comfort**: Check font sizes and line heights
- **JavaScript Content**: Render dynamic content with headless browser

### Export Options
- **TXT Reports**: Detailed text-based accessibility reports
- **PDF Reports**: Professional PDF documents with formatting
- **Comprehensive Data**: Includes scores, compliance status, and fix recommendations

## 🎯 Requirements

- **Windows 10/11**
- **.NET 9.0 Runtime** (automatically included in standalone version)
- **Internet connection** for URL analysis

## 🚀 Installation & Usage

### Option 1: Standalone Executable (Recommended)
1. Download the `WebAccessibilityChecker.exe` from the releases
2. Double-click to run (no installation required)
3. The app includes all dependencies

### Option 2: From Source
```bash
# Clone the repository
git clone <repository-url>
cd WebAccessibilityChecker

# Run the application
dotnet run
```

### Option 3: Build Standalone Version
```bash
# Build for distribution
dotnet publish -c Release -r win-x64 --self-contained

# Run from publish directory
cd bin/Release/net9.0-windows/win-x64/publish
./WebAccessibilityChecker.exe
```

## 📖 How to Use

1. **Launch the Application**
   - Run `WebAccessibilityChecker.exe` or use `dotnet run`

2. **Enter URL or Select File**
   - Type a website URL (e.g., `https://example.com`)
   - Or click "📁 Browse File" to select an HTML file

3. **Analyze Content**
   - Click "🔍 Analyze" to start checking
   - The app will render JavaScript content and analyze accessibility

4. **Review Results**
   - View issues in the table with color-coded severity
   - Check your accessibility score and compliance status
   - See detailed recommendations for each issue

5. **Export Reports**
   - Use "📄 Export to TXT" for detailed text reports
   - Use "📕 Export to PDF" for professional PDF documents

## 🎨 User Interface

### Modern Design Features
- **Card-based Layout**: Clean, professional appearance
- **Color-coded Results**: Red (Errors), Orange (Warnings), Green (Info)
- **Responsive Design**: Adapts to different window sizes
- **Emoji Icons**: Visual cues for better user experience
- **Segoe UI Font**: Modern, readable typography

### Accessibility Score System
- **95-100**: Fully Compliant
- **80-94**: Mostly Compliant
- **60-79**: Partially Compliant
- **<60**: Not Compliant

## 🔧 Technical Details

### Architecture
- **Frontend**: WPF (Windows Presentation Foundation)
- **Backend**: .NET 9.0 with C#
- **HTML Parsing**: HtmlAgilityPack
- **JavaScript Rendering**: PuppeteerSharp with headless Chrome
- **PDF Generation**: QuestPDF

### Project Structure
```
WebAccessibilityChecker/
├── Models/           # Data models (Issue, Report)
├── Services/         # Core logic (Parser, Checker)
├── Utils/            # Helper classes (Export)
├── Resources/        # Application resources
├── MainWindow.xaml   # UI layout
└── MainWindow.xaml.cs # UI logic
```

### Dependencies
- **HtmlAgilityPack**: HTML parsing and manipulation
- **PuppeteerSharp**: Headless browser for JavaScript rendering
- **QuestPDF**: PDF document generation
- **Microsoft.Extensions**: Dependency injection and logging

## 🐛 Troubleshooting

### Common Issues

**"Headless browser failed"**
- This is normal on first run - the app falls back to HTTP download
- Chrome will be downloaded automatically for future runs
- The app works perfectly with HTTP fallback

**"Cannot find Chrome"**
- The app will automatically download Chrome on first use
- If download fails, it uses HTTP method which works for most sites

**"Application won't start"**
- Ensure you have .NET 9.0 runtime installed
- Try running as administrator
- Check Windows Firewall settings

### Performance Tips
- **URL Analysis**: Works best with modern websites
- **File Analysis**: Instant results for local HTML files
- **Large Sites**: May take longer due to JavaScript rendering
- **Memory Usage**: ~200MB with Chrome, ~50MB with HTTP fallback

## 📊 Accessibility Standards

The app checks compliance with:
- **WCAG 2.1**: Web Content Accessibility Guidelines
- **Section 508**: US federal accessibility standards
- **India RPwD Act**: Rights of Persons with Disabilities Act

### Check Categories
1. **Perceivable**: Alt text, color contrast, media alternatives
2. **Operable**: Keyboard navigation, timing, seizures
3. **Understandable**: Readable text, predictable behavior
4. **Robust**: Compatible with assistive technologies

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

### Development Setup
```bash
# Install .NET 9.0 SDK
# Clone repository
git clone <repository-url>
cd WebAccessibilityChecker

# Restore packages
dotnet restore

# Run in development mode
dotnet run
```

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **HtmlAgilityPack**: For robust HTML parsing
- **PuppeteerSharp**: For headless browser capabilities
- **QuestPDF**: For beautiful PDF generation
- **.NET Community**: For excellent development tools

## 📞 Support

For issues, questions, or feature requests:
- Create an issue on GitHub
- Check the troubleshooting section
- Review the technical documentation

---

**Made with ❤️ for a more accessible web**