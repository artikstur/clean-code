using MarkdownRenderer.Enums;

namespace MarkdownRenderer;

public class TagPosition(TagType tag, TagState tagState, int tagIndex, string content)
{
    public string Content { get; set; } = content;
    public TagType TagType { get; set; } = tag;
    public TagState TagState { get; set; } = tagState;
    public int TagIndex { get; set; } = tagIndex;
    public TagPosition? TagPair { get; set; }
}