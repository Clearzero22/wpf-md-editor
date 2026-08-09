using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using wpf_projcts.Services;

namespace wpf_projcts.ViewModels;

public partial class DocumentViewModel : ObservableObject
{
    private static readonly MarkdownRenderService RenderService = new();
    private readonly DispatcherTimer _debounce;
    private int _renderVersion;
    private CancellationTokenSource? _renderCts;

    public DocumentViewModel(string? filePath, string text, int untitledIndex = 1)
    {
        _debounce = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };
        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            _ = RenderPreviewAsync();
        };

        FilePath = filePath;
        UntitledName = $"未命名{untitledIndex}.md";
        Text = text;
        IsDirty = false;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    [NotifyPropertyChangedFor(nameof(FilePathText))]
    private string? filePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private string untitledName = "未命名.md";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private string text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    [NotifyPropertyChangedFor(nameof(SaveStatus))]
    private bool isDirty;

    [ObservableProperty]
    private int statusLine = 1;

    [ObservableProperty]
    private int statusColumn = 1;

    [ObservableProperty]
    private int wordCount;

    [ObservableProperty]
    private FlowDocument previewDocument = new();

    public string DisplayTitle => (IsDirty ? "● " : "") + (FilePath is null ? UntitledName : Path.GetFileName(FilePath));

    public string FilePathText => FilePath ?? "未保存";

    public string SaveStatus => IsDirty ? "未保存" : "已保存";

    partial void OnTextChanged(string value)
    {
        IsDirty = true;
        _debounce.Stop();
        _debounce.Start();
    }

    private async Task RenderPreviewAsync()
    {
        var version = ++_renderVersion;
        var cts = new CancellationTokenSource();
        _renderCts?.Cancel();
        _renderCts = cts;

        var text = Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            PreviewDocument = new FlowDocument { PagePadding = new Thickness(16) };
            WordCount = 0;
            return;
        }

        try
        {
            var ast = await Task.Run(() => RenderService.Parse(text));
            if (cts.IsCancellationRequested || version != _renderVersion) return;

            PreviewDocument = RenderService.Render(ast);
            WordCount = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        catch
        {
            if (version == _renderVersion)
                PreviewDocument = new FlowDocument { PagePadding = new Thickness(16) };
        }
    }

    public async Task<bool> SaveAsync(string? forcePath = null)
    {
        var path = forcePath ?? FilePath;
        if (path is null)
        {
            path = FileService.PickSaveFile(FilePath);
            if (path is null) return false;
        }

        try
        {
            await FileService.WriteAsync(path, Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败：{ex.Message}", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        FilePath = path;
        IsDirty = false;
        return true;
    }

    public async Task<bool> PromptSaveIfDirtyAsync(string action)
    {
        if (!IsDirty) return true;

        var result = MessageBox.Show(
            $"是否保存对“{DisplayTitle}”的更改？",
            action,
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel) return false;
        return result == MessageBoxResult.No || await SaveAsync();
    }
}
