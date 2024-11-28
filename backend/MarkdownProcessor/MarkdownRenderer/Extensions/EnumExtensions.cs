using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Extensions;

public static class EnumExtensions
{
    public static bool IsEscapedTag(this TagType tagType)
    {
        return tagType is TagType.EscapedTag or TagType.EscapedItalicTag or TagType.EscapedBoldTag;
    }

    public static bool IsTemporarilyOpen(this TagState tagState)
    {
        return tagState is TagState.TemporarilyOpen or TagState.TemporarilyOpenInWord;
    }
}