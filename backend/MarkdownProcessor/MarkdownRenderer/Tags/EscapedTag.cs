using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Tags;

public class EscapedTag: Tag
{
    public override string MarkdownSymbol => @"\";
    public override string HtmlTag => " ";
    public override TagType TagType => TagType.EscapedTag;
}