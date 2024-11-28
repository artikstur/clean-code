using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Tags;

public class DivTag: Tag
{
    public override string MarkdownSymbol => string.Empty;
    public override string HtmlTag => "div";
    public override TagType TagType => TagType.SpanTag;
}