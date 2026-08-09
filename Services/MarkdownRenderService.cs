using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Wpf;

namespace wpf_projcts.Services;

public class MarkdownRenderService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseSupportedExtensions()
        .Build();

    /// <summary>解析 Markdown 为 AST。纯 CPU 操作，可在后台线程执行。</summary>
    public MarkdownDocument Parse(string markdown)
    {
        return Markdig.Markdown.Parse(markdown, Pipeline);
    }

    /// <summary>将 AST 渲染为 FlowDocument。必须在 UI 线程执行。</summary>
    public FlowDocument Render(MarkdownDocument ast)
    {
        var document = new FlowDocument();
        var renderer = new WpfRenderer(document);
        Pipeline.Setup(renderer);
        renderer.Render(ast);
        return document;
    }
}
