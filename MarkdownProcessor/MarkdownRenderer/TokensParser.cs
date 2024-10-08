﻿using MarkdownRenderer.Enums;
using MarkdownRenderer.Interfaces;

namespace MarkdownRenderer;

public class TokensParser : ITokensParser
{
    private Stack<TagPosition> _tagPositionsStack = new();
    private List<Token> _tokens = new();

    public IEnumerable<Token> ParseTokens(string unprocessedText)
    {
        string[] lines = unprocessedText.Split('\n');

        foreach (var line in lines)
        {
            string[] words = line.Split(' ');

            OpenParagraph();

            for (int i = 0; i < words.Length; i++)
            {
                var token = ProcessWord(words[i], i);

                _tokens.Add(token);
            }

            CloseParagraph();

            _tagPositionsStack.Clear();

            _tokens.Add(new Token("\n"));
        }

        return _tokens;
    }

    private void OpenParagraph()
    {
        var tokenForParagraph = new Token(" ");
        tokenForParagraph.TagPositions.Add(new TagPosition(TagType.SpanTag, TagState.Open, 0, string.Empty));

        _tokens.Add(tokenForParagraph);
    }

    private void CloseParagraph()
    {
        var tokenForParagraph = new Token(" ");
        tokenForParagraph.TagPositions.Add(new TagPosition(TagType.SpanTag, TagState.Close, 0, string.Empty));

        _tokens.Add(tokenForParagraph);
    }

    private Token ProcessWord(string word, int wordIndex)
    {
        var currentToken = new Token(word);
        bool isContainsDigits = word.Any(char.IsDigit);
        
        if (wordIndex == 0 && word == "#")
        {
            currentToken.TagPositions.Add(new TagPosition(TagType.HeaderTag, TagState.Open, 0, word));
        }

        if (word == string.Empty || word == " ")
        {
            return currentToken;
        }

        if (word == "__" || word == "____")
        {
            return currentToken;
        }

        if (isContainsDigits)
        {
            ProcessSymbolsInWord(word, 0, currentToken);
            if (ProcessSymbolsInWord(word, word.Length - 2, currentToken) == TagType.NotTag)
                ProcessSymbolsInWord(word, word.Length - 1, currentToken);

            return currentToken;
        }

        for (int i = 0; i < word.Length; i++)
        {
            var tagType = ProcessSymbolsInWord(word, i, currentToken);

            if (tagType is TagType.BoldTag)
                i++;
        }

        return currentToken;
    }

    private TagType ProcessSymbolsInWord(string word, int index, Token currentToken)
    {
        var tagType = HandleMarkdownSymbol(word, index);

        if (tagType is TagType.NotTag)
            return TagType.NotTag;

        var tagState = DetermineTagState(word, tagType, index, currentToken);

        if (tagState is TagState.NotTag)
            return TagType.NotTag;

        bool isPossibleToAddTag = HandleTagStack(tagType, tagState, index, word);

        if (isPossibleToAddTag)
        {
            currentToken.TagPositions.Add(_tagPositionsStack.Peek());
        }

        return tagType;
    }

    private TagType HandleMarkdownSymbol(string word, int index)
    {
        if (word[index] == '_')
        {
            if (index < word.Length - 1 && word[index + 1] == '_')
            {
                return TagType.BoldTag;
            }

            return TagType.ItalicTag;
        }

        return TagType.NotTag;
    }

    private bool HandleTagStack(TagType tagType, TagState tagState, int tagIndex, string word)
    {
        if (tagState == TagState.TemporarilyOpen || tagState == TagState.TemporarilyOpenInWord)
        {
            var tagPosition = new TagPosition(tagType, tagState, tagIndex, word);
            _tagPositionsStack.Push(tagPosition);

            return true;
        }

        if (tagState == TagState.TemporarilyClose)
        {
            TagPosition matchingOpenTag = null;
            Stack<TagPosition> tempStack = new Stack<TagPosition>();

            // Достаем все из стека пока не найдем открывающий
            while (_tagPositionsStack.Count > 0)
            {
                var previousTagPosition = _tagPositionsStack.Pop();

                if (previousTagPosition.TagType == tagType
                    && previousTagPosition.TagState == TagState.TemporarilyOpen)
                {
                    matchingOpenTag = previousTagPosition;
                    tempStack.Push(previousTagPosition);
                    break;
                }

                if (previousTagPosition.TagType == tagType
                    && previousTagPosition.TagState == TagState.TemporarilyOpenInWord
                    && previousTagPosition.Content == word)
                {
                    matchingOpenTag = previousTagPosition;
                    tempStack.Push(previousTagPosition);
                    break;
                }

                tempStack.Push(previousTagPosition);
            }

            // Хешсет для хранения всех тегов между открытым и закрытым
            var tagsInRange = new HashSet<TagPosition>();

            // Промежуточный тег для проверки условия пересечения двойных и одинарных подчерков
            TagPosition? intersecTagPos = null;

            // Кладем все обратно + учитваем, что между одинарными двойное не работает
            while (tempStack.Count > 0)
            {
                var tempTagPosition = tempStack.Pop();
                tagsInRange.Add(tempTagPosition);

                if (tagType == TagType.ItalicTag
                    && tempTagPosition.TagType == TagType.BoldTag
                    && tempTagPosition.TagState == TagState.Close
                    && matchingOpenTag != null)
                {
                    intersecTagPos = tempTagPosition;

                    tempTagPosition.TagPair.TagState = TagState.NotTag;
                    tempTagPosition.TagState = TagState.NotTag;
                }

                _tagPositionsStack.Push(tempTagPosition);
            }

            // Если не нашли открывающий, выходим
            if (matchingOpenTag is null)
            {
                return false;
            }

            var tagPosition = new TagPosition(tagType, TagState.Close, tagIndex, word)
            {
                TagPair = matchingOpenTag
            };

            matchingOpenTag.TagPair = tagPosition;
            matchingOpenTag.TagState = TagState.Open;

            if (intersecTagPos != null && !(tagsInRange.Contains(intersecTagPos.TagPair)))
            {
                matchingOpenTag.TagState = TagState.NotTag;
                tagPosition.TagState = TagState.NotTag;
            }

            _tagPositionsStack.Push(tagPosition);

            return true;
        }

        return false;
    }

    private TagState DetermineTagState(string word, TagType tagType, int symbolIndex, Token currentToken)
    {
        if (symbolIndex == 0 && (tagType is TagType.ItalicTag or TagType.BoldTag))
        {
            return TagState.TemporarilyOpen;
        }

        if (tagType == TagType.ItalicTag && symbolIndex == word.Length - 1)
        {
            return TagState.TemporarilyClose;
        }

        if (tagType == TagType.BoldTag && symbolIndex == word.Length - 2)
        {
            return TagState.TemporarilyClose;
        }

        if (tagType is TagType.ItalicTag or TagType.BoldTag
            && currentToken.TagPositions.Count(t => t.TagType == tagType) % 2 == 0)
        {
            return TagState.TemporarilyOpenInWord;
        }

        if (tagType is TagType.ItalicTag or TagType.BoldTag
            && currentToken.TagPositions.Count(t => t.TagType == tagType) % 2 == 1)
        {
            return TagState.TemporarilyClose;
        }

        return TagState.NotTag;
    }
}