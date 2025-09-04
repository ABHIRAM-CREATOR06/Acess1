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

namespace WebAccessibilityChecker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private HtmlParser _htmlParser = new HtmlParser();
    private AccessibilityChecker _checker = new AccessibilityChecker();
    private ExportHelper _exportHelper = new ExportHelper();
    private Report? _currentReport;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        string input = UrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Please enter a URL or select a file.");
            return;
        }

        HtmlDocument doc;
        if (Uri.TryCreate(input, UriKind.Absolute, out _))
        {
            // URL with headless rendering for JS content
            doc = await _htmlParser.LoadFromUrlWithHeadlessAsync(input);
        }
        else if (File.Exists(input))
        {
            // File
            doc = _htmlParser.LoadFromFile(input);
        }
        else
        {
            MessageBox.Show("Invalid URL or file path.");
            return;
        }

        _currentReport = _checker.CheckAccessibility(doc);
        ResultsDataGrid.ItemsSource = _currentReport.Issues;
        ScoreTextBlock.Text = _currentReport.AccessibilityScore.ToString();
        ComplianceTextBlock.Text = _currentReport.ComplianceStatus;
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
}