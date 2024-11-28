using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Tags;

public class HeaderTag: Tag
{
    public override string MarkdownSymbol => "#";
    public override string HtmlTag => "h1";
    public override TagType TagType => TagType.HeaderTag;
}