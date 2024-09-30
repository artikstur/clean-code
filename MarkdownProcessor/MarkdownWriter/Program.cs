﻿using MarkdownRenderer;
using MarkdownRenderer.Abstractions;
using MarkdownRenderer.Tags;

class Program
{
    public static void Main()
    {
        var boldTag = new BoldTag();
        var italicTag = new ItalicTag();

        Dictionary<string, Tag> tagsDictionary = new Dictionary<string, Tag>()
        {
            {
                boldTag.MarkdownSymbol,
                boldTag
            },
            {
                italicTag.MarkdownSymbol,
                italicTag
            }
        };

        var tokensParser = new TokensParser(tagsDictionary);
        var allTokens = tokensParser.ParseTokens("\\\\_вот это будет выделено тегом_");

        foreach (var token in allTokens)
        {
            Console.WriteLine(token.Tag.MarkdownSymbol);
        }
    }
}