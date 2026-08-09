using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using wpf_projcts.Services;

namespace wpf_projcts.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string NewDocumentTemplate = """
        # 未命名文档

        使用菜单或工具栏快速编辑 Markdown，右侧为实时预览。

        ## 基本语法

        **加粗**、*斜体*、`行内代码`

        - 列表项一
        - 列表项二

        1. 有序项一
        2. 有序项二

        > 这是一段引用。

        [链接文字](https://example.com)

        | 列一 | 列二 |
        | ---- | ---- |
        | A    | B    |

        ```csharp
        Console.WriteLine("Hello Markdown!");
        ```

        ---

        - [x] 已完成任务
        - [ ] 未完成任务
        """;

    private int _untitledCounter = 1;

    public MainViewModel()
    {
        NewDocumentCommand.Execute(null);
    }

    public ObservableCollection<DocumentViewModel> Documents { get; } = new();

    [ObservableProperty]
    private DocumentViewModel? selectedDocument;

    [RelayCommand]
    private void NewDocument()
    {
        var doc = new DocumentViewModel(null, NewDocumentTemplate, _untitledCounter++)
        {
            IsDirty = false
        };
        Documents.Add(doc);
        SelectedDocument = doc;
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var path = FileService.PickOpenFile();
        if (path is null) return;
        await OpenPathAsync(path);
    }

    public async Task OpenPathAsync(string path)
    {
        var existing = Documents.FirstOrDefault(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            SelectedDocument = existing;
            return;
        }

        try
        {
            var text = await FileService.ReadAsync(path);
            var doc = new DocumentViewModel(path, text);
            Documents.Add(doc);
            SelectedDocument = doc;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开文件：{ex.Message}", "打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SaveDocument()
    {
        if (SelectedDocument is null) return;
        await SelectedDocument.SaveAsync();
    }

    [RelayCommand]
    private async Task SaveDocumentAs()
    {
        if (SelectedDocument is null) return;
        var path = FileService.PickSaveFile(SelectedDocument.FilePath);
        if (path is null) return;
        await SelectedDocument.SaveAsync(path);
    }

    [RelayCommand]
    private async Task CloseTab(DocumentViewModel? doc)
    {
        doc ??= SelectedDocument;
        if (doc is null) return;
        if (!await doc.PromptSaveIfDirtyAsync("关闭文档")) return;

        Documents.Remove(doc);
        if (ReferenceEquals(SelectedDocument, doc))
            SelectedDocument = Documents.LastOrDefault();
    }

    public async Task<bool> PromptSaveAllAsync()
    {
        foreach (var doc in Documents.ToList())
        {
            if (!await doc.PromptSaveIfDirtyAsync("关闭应用程序"))
                return false;
        }
        return true;
    }

    [RelayCommand]
    private void Exit() => Application.Current?.MainWindow?.Close();
}
