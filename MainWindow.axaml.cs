using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using KalkulatorIP.Models;

namespace KalkulatorIP;

public partial class MainWindow : Window
{
    // ── Cached brushes ──────────────────────────────────────────────────────
    private static readonly SolidColorBrush BrushAccent   = new(Color.Parse("#58A6FF"));
    private static readonly SolidColorBrush BrushGreen    = new(Color.Parse("#3FB950"));
    private static readonly SolidColorBrush BrushSecond   = new(Color.Parse("#8B949E"));
    private static readonly SolidColorBrush BrushText     = new(Color.Parse("#E6EDF3"));
    private static readonly SolidColorBrush BrushMuted    = new(Color.Parse("#6E7681"));
    private static readonly SolidColorBrush BrushSurface  = new(Color.Parse("#0D1117"));
    private static readonly SolidColorBrush BrushBorder   = new(Color.Parse("#21262D"));
    private static readonly FontFamily      FontMono      = new("Consolas,'Courier New',monospace");

    public MainWindow() => InitializeComponent();

    // ── Input handlers ──────────────────────────────────────────────────────
    private void Input_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return) RunCalculation();
    }

    private void Calculate_Click(object? sender, RoutedEventArgs e) => RunCalculation();

    // ── Core logic ──────────────────────────────────────────────────────────
    private void RunCalculation()
    {
        HideError();

        string ip   = IpTextBox.Text?.Trim()   ?? "";
        string mask = MaskTextBox.Text?.Trim()  ?? "";

        // Allow combined entry in IP field
        if (ip.Contains('/'))
        {
            var parts = ip.Split('/', 2);
            ip   = parts[0].Trim();
            mask = parts[1].Trim();
            MaskTextBox.Text = mask;
        }

        if (string.IsNullOrWhiteSpace(ip))   { ShowError("Podaj adres IP."); return; }
        if (string.IsNullOrWhiteSpace(mask))  { ShowError("Podaj maskę podsieci."); return; }

        try
        {
            var result = IPCalculator.Calculate(ip, mask);
            DisplayResults(result);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string msg)
    {
        ErrorText.Text      = msg;
        ErrorBorder.IsVisible = true;
        ResultsPanel.IsVisible = false;
    }

    private void HideError() => ErrorBorder.IsVisible = false;

    // ── Display results ─────────────────────────────────────────────────────
    private void DisplayResults(IPCalculatorResult r)
    {
        // Summary
        NetworkSummaryText.Text = $"{r.NetworkAddress}/{r.Prefix}";
        MaskSummaryText.Text    = $"Maska: {r.SubnetMask}   |   Wildcard: {r.WildcardMask}";
        ClassText.Text          = $"Klasa {r.NetworkClass}";
        UsableHostsText.Text    = r.UsableHosts.ToString("N0");
        TotalHostsText.Text     = r.TotalHosts.ToString("N0");
        BinaryMaskText.Text     = r.BinaryMask;

        // Info cards
        NetworkText.Text    = r.NetworkAddress;
        BroadcastText.Text  = r.BroadcastAddress;
        MinHostText.Text    = r.MinHost;
        MaxHostText.Text    = r.MaxHost;
        SubnetMaskText.Text = r.SubnetMask;
        WildcardText.Text   = r.WildcardMask;

        // Subnets
        PopulateSubnets(r);

        ResultsPanel.IsVisible = true;
    }

    // ── Subnet expanders ────────────────────────────────────────────────────
    private void PopulateSubnets(IPCalculatorResult r)
    {
        SubnetsContainer.Children.Clear();

        if (r.PossibleSubnets.Count == 0)
        {
            SubnetCountText.Text = "(sieć /30 lub mniejsza — brak dalszych podziałów)";
            SubnetsContainer.Children.Add(new TextBlock
            {
                Text       = "Wybrana sieć jest już na poziomie /30 lub mniejszym. Dalszy podział na hoście nie jest możliwy.",
                Foreground = BrushSecond,
                TextWrapping = TextWrapping.Wrap,
                FontSize   = 13
            });
            return;
        }

        SubnetCountText.Text = $"({r.PossibleSubnets.Count} możliwych podziałów)";

        foreach (var div in r.PossibleSubnets)
            SubnetsContainer.Children.Add(BuildSubnetExpander(div));
    }

    private Expander BuildSubnetExpander(SubnetDivision div)
    {
        // ── Header ──────────────────────────────────────────────────────────
        var headerGrid = new Grid { Margin = new Avalonia.Thickness(0) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(72,  GridUnitType.Pixel));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(100, GridUnitType.Pixel));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(110, GridUnitType.Pixel));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(1,   GridUnitType.Star));

        headerGrid.Children.Add(ColText(0, $"/{div.Prefix}",
            BrushAccent, 14, FontWeight.SemiBold, FontMono));
        headerGrid.Children.Add(ColText(1, div.NumberOfSubnets.ToString("N0"),
            BrushText, 14));
        headerGrid.Children.Add(ColText(2, div.UsableHostsPerSubnet.ToString("N0"),
            BrushGreen, 14));
        headerGrid.Children.Add(ColText(3, div.SubnetMask,
            BrushSecond, 13, FontWeight.Normal, FontMono));

        // ── Content ─────────────────────────────────────────────────────────
        var listStack = new StackPanel { Spacing = 1, Margin = new Avalonia.Thickness(0, 6, 0, 0) };

        foreach (var s in div.Subnets)
        {
            listStack.Children.Add(new TextBlock
            {
                Text       = s,
                FontFamily = FontMono,
                FontSize   = 12,
                Foreground = BrushText,
                Padding    = new Avalonia.Thickness(10, 3)
            });
        }

        if (div.HasMore)
        {
            listStack.Children.Add(new TextBlock
            {
                Text        = $"  … i jeszcze {div.TotalSubnets - div.Subnets.Count:N0} podsieci (wyświetlono pierwsze {div.Subnets.Count})",
                Foreground  = BrushMuted,
                FontStyle   = FontStyle.Italic,
                FontSize    = 12,
                Margin      = new Avalonia.Thickness(0, 4, 0, 0)
            });
        }

        var contentBorder = new Border
        {
            Background    = BrushSurface,
            CornerRadius  = new Avalonia.CornerRadius(4),
            Padding       = new Avalonia.Thickness(4, 0, 4, 6),
            Child         = listStack
        };

        return new Expander
        {
            Header      = headerGrid,
            Content     = contentBorder,
            IsExpanded  = false,
            Margin      = new Avalonia.Thickness(0, 0, 0, 2)
        };
    }

    // Helper: create a Grid-column TextBlock
    private static TextBlock ColText(int col, string text, ISolidColorBrush brush,
        double size, FontWeight weight = FontWeight.Normal, FontFamily? font = null)
    {
        var tb = new TextBlock
        {
            Text                = text,
            Foreground          = brush,
            FontSize            = size,
            FontWeight          = weight,
            VerticalAlignment   = VerticalAlignment.Center
        };
        if (font is not null) tb.FontFamily = font;
        Grid.SetColumn(tb, col);
        return tb;
    }

    // ── Footer link ─────────────────────────────────────────────────────────
    private void EbtechLink_Click(object? sender, RoutedEventArgs e) =>
        OpenUrl("http://www.ebtech.pl");

    private static void OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
        }
        catch { /* ignore if browser not available */ }
    }
}
