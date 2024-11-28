using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Tags;

public class BoldTag : Tag
{
    public override string MarkdownSymbol => "__";

    public override string HtmlTag => "strong";
    public override TagType TagType => TagType.BoldTag;
}