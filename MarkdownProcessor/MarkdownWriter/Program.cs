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
        string text = @"_еж__ж n__aд";
        string[] tokens = text.Split(' ');

        foreach (var item in tokens)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("\n");

        var allTokens = tokensParser.ParseTokens(text);
        foreach (var token in allTokens)
        {
            Console.WriteLine($"СЛОВО - {token.Content}");
            foreach (var item in token.TagPositions)
            {
                Console.WriteLine($"Type: {item.TagType.ToString()}, " +
                                  $"Index: {item.TagIndex}, " +
                                  $"State: {item.TagState}," );
            }
        }
    }
}