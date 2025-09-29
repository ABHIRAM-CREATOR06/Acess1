using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using HtmlAgilityPack;
using WebAccessibilityChecker.Services;
using WebAccessibilityChecker.Utils;
using WebAccessibilityChecker.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace WebAccessibilityChecker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private HtmlParser _htmlParser = new HtmlParser();
    private AccessibilityChecker _checker = new AccessibilityChecker();
    private SEOChecker _seoChecker = new SEOChecker();
    private ExportHelper _exportHelper = new ExportHelper();
    private Report? _currentReport;

    public event PropertyChangedEventHandler? PropertyChanged;

    private ChartValues<double> _scores = new ChartValues<double>();
    public ChartValues<double> Scores
    {
        get => _scores;
        set
        {
            _scores = value;
            OnPropertyChanged(nameof(Scores));
        }
    }

    private string[] _labels = new string[0];
    public string[] Labels
    {
        get => _labels;
        set
        {
            _labels = value;
            OnPropertyChanged(nameof(Labels));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        string input = UrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Please enter a URL or select a file.");
            return;
        }

        // Show loading
        LoadingProgressBar.Visibility = Visibility.Visible;
        LoadingTextBlock.Visibility = Visibility.Visible;
        AnalyzeButton.IsEnabled = false;

        HtmlDocument doc;
        string? url = null;
        PageLoadResult? loadResult = null;
        if (Uri.TryCreate(input, UriKind.Absolute, out _) && input.StartsWith("http"))
        {
            url = input;
            // URL with headless rendering for JS content
            try
            {
                loadResult = await _htmlParser.LoadFromUrlWithHeadlessAsync(input);
                doc = loadResult.Document;
            }
            catch (Exception ex)
            {
                // Fallback to simple HTTP if headless fails
                MessageBox.Show($"Headless browser failed, falling back to basic HTTP download: {ex.Message}", "Browser Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                doc = await _htmlParser.LoadFromUrlAsync(input);
                loadResult = new PageLoadResult
                {
                    Document = doc,
                    LoadTime = 0,
                    RequestCount = 1,
                    PageSize = doc.DocumentNode.OuterHtml.Length,
                    IsCompressed = false,
                    HasCachingHeaders = false
                };
            }
        }
        else if (File.Exists(input))
        {
            // File
            try
            {
                doc = _htmlParser.LoadFromFile(input);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading HTML file: {ex.Message}", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            MessageBox.Show("Invalid URL or file path.");
            return;
        }

        try
        {
            if (loadResult != null)
            {
                // Use headless browser results for full analysis
                _currentReport = _checker.CheckAccessibility(loadResult);
            }
            else
            {
                // Fallback for file analysis - create minimal PageLoadResult
                var minimalLoadResult = new PageLoadResult
                {
                    Document = doc,
                    LoadTime = 0,
                    RequestCount = 1,
                    PageSize = doc.DocumentNode.OuterHtml.Length,
                    IsCompressed = false,
                    HasCachingHeaders = false
                };
                _currentReport = _checker.CheckAccessibility(minimalLoadResult);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing the file: {ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        _currentReport.WebsiteUrl = url ?? "Local File";

        // Add SEO checks if URL
        if (!string.IsNullOrEmpty(url))
        {
            var seoIssuesList = await _seoChecker.CheckSEOAsync(doc, url);
            _currentReport.Issues.AddRange(seoIssuesList);
        }

        // Calculate SEO score
        var seoIssues = _currentReport.Issues.Where(i => i.Category == Category.SEO).ToList();
        int seoPenalty = seoIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                         seoIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                         seoIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
        _currentReport.SEOScore = Math.Max(0, 100 - seoPenalty);

        // Calculate Best Practices score
        var bpIssues = _currentReport.Issues.Where(i => i.Category == Category.BestPractices).ToList();
        int bpPenalty = bpIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                        bpIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                        bpIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
        _currentReport.BestPracticesScore = Math.Max(0, 100 - bpPenalty);

        ResultsDataGrid.ItemsSource = _currentReport.Issues;
        AccessibilityScoreTextBlock.Text = _currentReport.AccessibilityScore.ToString();
        SEOScoreTextBlock.Text = _currentReport.SEOScore.ToString();
        PerformanceScoreTextBlock.Text = _currentReport.PerformanceScore.ToString();
        BestPracticesScoreTextBlock.Text = _currentReport.BestPracticesScore.ToString();
        EnvironmentScoreTextBlock.Text = _currentReport.EnvironmentScore.ToString();
        SafetyScoreTextBlock.Text = _currentReport.SafetyScore.ToString();
        ComplianceTextBlock.Text = _currentReport.ComplianceStatus;

        // Update category summary
        var categoryCounts = _currentReport.Issues
            .GroupBy(i => i.Category)
            .Select(g => $"{g.Key}: {g.Count()} issues")
            .ToList();
        CategorySummaryTextBlock.Text = string.Join(" | ", categoryCounts);

        // Update chart data
        Scores = new ChartValues<double>
        {
            _currentReport.AccessibilityScore,
            _currentReport.SEOScore,
            _currentReport.PerformanceScore,
            _currentReport.BestPracticesScore,
            _currentReport.EnvironmentScore,
            _currentReport.SafetyScore
        };
        Labels = new[] { "Accessibility", "SEO", "Performance", "Best Practices", "Environment", "Safety" };

        // Hide loading
        LoadingProgressBar.Visibility = Visibility.Collapsed;
        LoadingTextBlock.Visibility = Visibility.Collapsed;
        AnalyzeButton.IsEnabled = true;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
            UrlTextBox.Text = openFileDialog.FileName;
        }
    }

    private void ExportTxtButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report to export. Please analyze first.");
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Text files (*.txt)|*.txt";
        if (saveFileDialog.ShowDialog() == true)
        {
            _exportHelper.ExportToTxt(_currentReport, saveFileDialog.FileName);
            MessageBox.Show("TXT Report exported successfully.");
        }
    }

    private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report to export. Please analyze first.");
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
        if (saveFileDialog.ShowDialog() == true)
        {
            _exportHelper.ExportToPdf(_currentReport, saveFileDialog.FileName);
            MessageBox.Show("PDF Report exported successfully.");
        }
    }

    private void ExportHtmlButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report to export. Please analyze first.");
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "HTML files (*.html)|*.html";
        if (saveFileDialog.ShowDialog() == true)
        {
            _exportHelper.ExportToHtml(_currentReport, saveFileDialog.FileName);
            MessageBox.Show("HTML Report exported successfully.");
        }
    }
}