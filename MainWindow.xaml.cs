using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using wpf_projcts.ViewModels;
using wpf_projcts.Views;

namespace wpf_projcts;

public partial class MainWindow : Window
{
    private bool _closingConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private DocumentView? ActiveView => TabHost.SelectedContent as DocumentView;

    private void TabHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabHost.SelectedContent is DocumentView view)
            view.FocusEditor();
    }

    private async void Window_Closing(object sender, CancelEventArgs e)
    {
        if (_closingConfirmed) return;

        e.Cancel = true;
        if (Vm is not null && await Vm.PromptSaveAllAsync())
        {
            _closingConfirmed = true;
            Close();
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (Vm is null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;

        foreach (var file in files.Where(f =>
                     f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
        {
            _ = Vm.OpenPathAsync(file);
        }
    }

    private void Heading_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string marker })
            ActiveView?.InsertAtLineStart(marker);
    }

    private void Bold_Click(object sender, RoutedEventArgs e) => ActiveView?.WrapSelection("**", "**");

    private void Italic_Click(object sender, RoutedEventArgs e) => ActiveView?.WrapSelection("*", "*");

    private void Code_Click(object sender, RoutedEventArgs e) => ActiveView?.WrapSelection("`", "`");

    private void CodeBlock_Click(object sender, RoutedEventArgs e) =>
        ActiveView?.WrapSelection("```\n", "\n```", "代码");

    private void Link_Click(object sender, RoutedEventArgs e) =>
        ActiveView?.WrapSelection("[", "](链接)", "链接文字");

    private void Image_Click(object sender, RoutedEventArgs e) =>
        ActiveView?.WrapSelection("![", "](图片地址)", "图片描述");

    private void Bullet_Click(object sender, RoutedEventArgs e) => ActiveView?.InsertAtLineStart("- ");

    private void Numbered_Click(object sender, RoutedEventArgs e) => ActiveView?.InsertAtLineStart("1. ");

    private void Quote_Click(object sender, RoutedEventArgs e) => ActiveView?.InsertAtLineStart("> ");

    private void Hr_Click(object sender, RoutedEventArgs e) => ActiveView?.InsertAtLineStart("---\n");

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this,
            "Markdown 编辑器\n\n基于 WPF + Markdig 开发的 Markdown 编辑器，支持实时预览、多标签编辑。",
            "关于",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
