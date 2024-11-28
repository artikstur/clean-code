using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Tags;

public class ItalicTag: Tag
{
    public override string MarkdownSymbol => "_";
    public override string HtmlTag => "em";
    public override TagType TagType => TagType.ItalicTag;
}