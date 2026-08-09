using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using wpf_projcts.ViewModels;

namespace wpf_projcts.Views;

public partial class DocumentView : UserControl
{
    private ScrollViewer? _previewScroller;

    public DocumentView()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateCaretStatus();
    }

    public void FocusEditor()
    {
        Editor.Focus();
        Editor.CaretIndex = Editor.Text.Length;
        UpdateCaretStatus();
    }

    public void WrapSelection(string prefix, string suffix, string? placeholder = null)
    {
        if (Editor.SelectedText.Length > 0)
        {
            Editor.SelectedText = prefix + Editor.SelectedText + suffix;
        }
        else
        {
            var start = Editor.CaretIndex;
            Editor.SelectedText = prefix + (placeholder ?? string.Empty) + suffix;
            Editor.CaretIndex = start + prefix.Length;
        }
        Editor.Focus();
    }

    public void InsertAtLineStart(string marker)
    {
        var lineStart = Editor.Text.LastIndexOf('\n', Math.Max(0, Editor.CaretIndex - 1)) + 1;
        Editor.Select(lineStart, 0);
        Editor.SelectedText = marker;
        Editor.CaretIndex = lineStart + marker.Length;
        Editor.Focus();
    }

    private void Editor_SelectionChanged(object sender, RoutedEventArgs e) => UpdateCaretStatus();

    private void UpdateCaretStatus()
    {
        if (DataContext is not DocumentViewModel vm) return;

        var caret = Math.Min(Editor.CaretIndex, Editor.Text.Length);
        var lineIndex = Editor.GetLineIndexFromCharacterIndex(caret);
        var lineStart = Editor.GetCharacterIndexFromLineIndex(lineIndex);
        vm.StatusLine = lineIndex + 1;
        vm.StatusColumn = caret - lineStart + 1;
    }

    private void Editor_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var editorScroller = FindScrollViewer((DependencyObject)sender);
        if (editorScroller is null) return;

        _previewScroller ??= FindScrollViewer(Preview);
        if (_previewScroller is null) return;

        var max = editorScroller.ExtentHeight - editorScroller.ViewportHeight;
        var ratio = max > 0 ? editorScroller.VerticalOffset / max : 0;
        var previewMax = _previewScroller.ExtentHeight - _previewScroller.ViewportHeight;
        _previewScroller.ScrollToVerticalOffset(previewMax * ratio);
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        if (root is ScrollViewer sv) return sv;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var found = FindScrollViewer(VisualTreeHelper.GetChild(root, i));
            if (found is not null) return found;
        }
        return null;
    }
}
