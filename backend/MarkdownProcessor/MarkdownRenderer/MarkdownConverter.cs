using System.Text;
using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;
using MarkdownRenderer.Interfaces;
using MarkdownRenderer.Tags;

namespace MarkdownRenderer;

public class MarkdownConverter : IMarkdownConverter
{
    private readonly ITokensParser _parser;
    private readonly IDictionary<TagType, Tag> _tags = new Dictionary<TagType, Tag>();
    public MarkdownConverter(ITokensParser parser)
    {
        _parser = parser;

        _tags.Add(TagType.BoldTag, new BoldTag());
        _tags.Add(TagType.ItalicTag, new ItalicTag());
        _tags.Add(TagType.SpanTag, new DivTag());
        _tags.Add(TagType.HeaderTag, new HeaderTag());
        _tags.Add(TagType.EscapedTag, new EscapedTag());
    }

    public string ConvertToHtml(string unprocessedText)
    {
        var tokens = _parser.ParseTokens(unprocessedText).ToList();

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var content = token.Content;
            var tagPositions = token.TagPositions;

            if (tagPositions.Count > 0)
            {
                var sortedTagPositions = tagPositions.Where(tp => tp.TagState is TagState.Open or TagState.Close or TagState.SingleTag)
                    .OrderBy(tp => tp.TagIndex).ToList();
                int currentIndex = 0;

                foreach (var tagPosition in sortedTagPositions)
                {
                    sb.Append(content.Substring(currentIndex, tagPosition.TagIndex - currentIndex));

                    bool isNeedToInsertTag = false;
                    if (tagPosition.TagType == TagType.EscapedTag)
                    {
                        sb.Append("");
                        isNeedToInsertTag = true;
                    }
                    else if (tagPosition.TagType is TagType.EscapedItalicTag or TagType.EscapedBoldTag)
                    {
                        sb.Append("");
                        isNeedToInsertTag = true;
                    }
                    else if (tagPosition.TagState == TagState.Open)
                    {
                        sb.Append(GetHtmlTag(tagPosition.TagType, true));
                        isNeedToInsertTag = true;
                    }
                    else if (tagPosition.TagState == TagState.Close)
                    {
                        sb.Append(GetHtmlTag(tagPosition.TagType, false));
                        isNeedToInsertTag = true;
                    }

                    if (isNeedToInsertTag)
                    {
                        if (tagPosition.TagType == TagType.EscapedTag)
                        {
                            currentIndex = tagPosition.TagIndex + 2;
                        }
                        else
                        {
                            currentIndex = tagPosition.TagIndex + (tagPosition.TagType == TagType.BoldTag ? 2 : 1);
                        }
                    }
                }

                if (currentIndex < content.Length)
                {
                    sb.Append(content.Substring(currentIndex));
                }
            }
            else
            {
                sb.Append(content);
            }

            if (token.Content != "\n"
                && !token.TagPositions.Any(t => t.TagType is TagType.SpanTag or TagType.HeaderTag)
                && i < tokens.Count - 1
                && !tokens[i + 1].TagPositions.Any(t => t.TagType is TagType.SpanTag or TagType.HeaderTag))
            {
                sb.Append(" ");
            }
        }

        return sb.ToString().Trim();
    }

    private string GetHtmlTag(TagType tagType, bool isOpening)
    {
        var currentTag = _tags[tagType];

        return isOpening ? $"<{currentTag.HtmlTag}>" : $"</{currentTag.HtmlTag}>";
    }
}
