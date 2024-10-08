﻿using System.Text;
using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Enums;
using MarkdownRenderer.Interfaces;
using MarkdownRenderer.Tags;

namespace MarkdownRenderer;

public class MarkdownConverter : IMarkdownConverter
{
    private readonly IDictionary<TagType, Tag> _tags = new Dictionary<TagType, Tag>();
    private readonly ITokensParser _parser;
    public MarkdownConverter(ITokensParser parser)
    {
        _parser = parser;

        _tags.Add(TagType.BoldTag, new BoldTag());
        _tags.Add(TagType.ItalicTag, new ItalicTag());
        _tags.Add(TagType.SpanTag, new SpanTag());
    }
    public string ConvertToHtml(string unprocessedText)
    {
        var tokens = _parser.ParseTokens(unprocessedText);
        StringBuilder sb = new StringBuilder();

        foreach (var token in tokens)
        {
            var content = token.Content;
            var tagPositions = token.TagPositions;

            if (tagPositions.Count > 0)
            {
                var sortedTagPositions = tagPositions.OrderBy(tp => tp.TagIndex).ToList();
                int currentIndex = 0;

                foreach (var tagPosition in sortedTagPositions)
                {
                    sb.Append(content.Substring(currentIndex, tagPosition.TagIndex - currentIndex));

                    if (tagPosition.TagState == TagState.Open)
                    {
                        sb.Append(GetHtmlTag(tagPosition.TagType, true));
                    }
                    else if (tagPosition.TagState == TagState.Close)
                    {
                        sb.Append(GetHtmlTag(tagPosition.TagType, false));
                    }

                    currentIndex = tagPosition.TagIndex + (tagPosition.TagType == TagType.BoldTag ? 2 : 1);
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

            if (token.Content != "\n")
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
