using System.IO;
using Microsoft.Win32;

namespace wpf_projcts.Services;

public static class FileService
{
    private const string MdFilter = "Markdown 文件 (*.md;*.markdown)|*.md;*.markdown|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";

    public static string? PickOpenFile()
    {
        var dialog = new OpenFileDialog { Filter = MdFilter, Multiselect = false };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public static string? PickSaveFile(string? currentPath)
    {
        var dialog = new SaveFileDialog
        {
            Filter = MdFilter,
            DefaultExt = ".md",
            FileName = currentPath is null ? "未命名.md" : Path.GetFileName(currentPath)
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public static Task<string> ReadAsync(string path) => File.ReadAllTextAsync(path);

    public static Task WriteAsync(string path, string content) => File.WriteAllTextAsync(path, content);
}
