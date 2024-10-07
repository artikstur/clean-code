﻿using MarkdownRenderer.Enums;

namespace MarkdownRenderer.Abstractions
{
    public abstract class Tag
    {
        public abstract string MarkdownSymbol { get; }
        public abstract string HtmlTag { get; }
        public abstract TagType TagType { get; }
        public virtual string ConvertToHtmlTag(string content)
        {
            return $"<{HtmlTag}>{content}</{HtmlTag}>";
        }
    }
}
