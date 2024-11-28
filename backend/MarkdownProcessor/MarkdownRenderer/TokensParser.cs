using MarkdownRenderer.Enums;
using MarkdownRenderer.Extensions;
using MarkdownRenderer.Interfaces;

namespace MarkdownRenderer;

public class TokensParser : ITokensParser
{
    private readonly Stack<TagPosition> _tagPositionsStack = new();
    private readonly List<Token> _tokens = new();
    private readonly LinkReferenceHandler _linkReferenceHandler = new();

    public IEnumerable<Token> ParseTokens(string unprocessedText)
    {
        string[] lines = unprocessedText.Split('\n');

        foreach (var linkLine in lines.Where(_linkReferenceHandler.IsReferenceStyleLink))
        {
            _linkReferenceHandler.HandleReferenceStyleLink(linkLine); 
        }

        foreach (var line in lines.Where(line => !_linkReferenceHandler.IsReferenceStyleLink(line)))
        {
            string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            OpenParagraph();

            for (int i = 0; i < words.Length; i++)
            {
                var token = ProcessWord(words[i], i);

                _tokens.Add(token);
            }

            HandleHeaderTag();

            CloseParagraph();

            _tagPositionsStack.Clear();
        }

        return _tokens;
    }

    private void HandleHeaderTag()
    {
        if (_tagPositionsStack.Any(t => t.TagType == TagType.HeaderTag))
        {
            var token = new Token("#");
            token.TagPositions.Add(new TagPosition(TagType.HeaderTag, TagState.Close, 0, "#"));

            _tokens.Add(token);
        }
    }

    private void OpenParagraph()
    {
        var tokenForParagraph = new Token("");
        tokenForParagraph.TagPositions.Add(new TagPosition(TagType.SpanTag, TagState.Open, 0, string.Empty));

        _tokens.Add(tokenForParagraph);
    }

    private void CloseParagraph()
    {
        var tokenForParagraph = new Token("");
        tokenForParagraph.TagPositions.Add(new TagPosition(TagType.SpanTag, TagState.Close, 0, string.Empty));

        _tokens.Add(tokenForParagraph);
    }

    private Token ProcessWord(string word, int wordIndex)
    {
        var currentToken = new Token(word);

        if (wordIndex == 0 && word.Length == 1
                           && HandleMarkdownSymbol(word, 0) == TagType.HeaderTag)
        {
            var tagPosition = new TagPosition(TagType.HeaderTag, TagState.Open, 0, "#");
            currentToken.TagPositions.Add(tagPosition);
            _tagPositionsStack.Push(tagPosition);

            return currentToken;
        }

        if (word is "" or " ")
        {
            return currentToken;
        }

        if (word is "__" or "____")
        {
            return currentToken;
        }

        if (word.IsWordContainsReferenceToLink())
        {
            currentToken.Content = _linkReferenceHandler.ConvertLinkFromRefToHtml(word);
            return currentToken;
        }

        if (word.IsWordContainsLink())
        {
            currentToken.Content = _linkReferenceHandler.ConvertLinkFromWordToHtml(word);
            return currentToken;
        }

        if (word.Any(char.IsDigit))
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
            else if (tagType is TagType.EscapedBoldTag)
                i += 3;
            else if (tagType is TagType.EscapedItalicTag)
                i += 2;
            else if (tagType is TagType.EscapedTag)
                i++;
        }

        return currentToken;
    }

    private TagType ProcessSymbolsInWord(string word, int index, Token currentToken)
    {
        var tagType = HandleMarkdownSymbol(word, index);

        if (tagType is TagType.NotTag)
            return tagType;

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
        if (word[index] == '\\')
        {
            if (index < word.Length - 1 && word[index + 1] == '_')
            {
                if (index < word.Length - 2 && word[index + 2] == '_')
                {
                    return TagType.EscapedBoldTag;
                }

                return TagType.EscapedItalicTag;
            }

            if (index < word.Length - 1 && word[index + 1] == '\\')
            {
                return TagType.EscapedTag;
            }
        }

        if (word[index] == '_')
        {
            if (index < word.Length - 1 && word[index + 1] == '_')
            {
                return TagType.BoldTag;
            }

            return TagType.ItalicTag;
        }

        if (word[index] == '#')
        {
            return TagType.HeaderTag;
        }

        return TagType.NotTag;
    }

    private void PushTagToStack(TagType tagType, TagState tagState, int tagIndex, string word)
    {
        var tagPosition = new TagPosition(tagType, tagState, tagIndex, word);
        _tagPositionsStack.Push(tagPosition);
    }

    private bool IsMatchingOpenTag(TagPosition tagPosition, TagType tagType, string word)
    {
        return (tagPosition.TagType == tagType && tagPosition.TagState == TagState.TemporarilyOpen) ||
               (tagPosition.TagType == tagType && tagPosition.TagState == TagState.TemporarilyOpenInWord && tagPosition.Content == word);
    }

    private TagPosition? FindMatchingOpenTag(TagType tagType, string word, Stack<TagPosition> tempStack)
    {
        TagPosition? matchingOpenTag = null;

        while (_tagPositionsStack.Count > 0)
        {
            var previousTag = _tagPositionsStack.Pop();
            tempStack.Push(previousTag);

            if (IsMatchingOpenTag(previousTag, tagType, word))
            {
                matchingOpenTag = previousTag;
                break;
            }
        }

        return matchingOpenTag;
    }

    private TagPosition? RestoreAndValidateTags(TagPosition? matchingOpenTag, TagType tagType, Stack<TagPosition> tempStack, int tagIndex, string word)
    {
        var tagsInRange = new HashSet<TagPosition>();
        TagPosition? intersectTag = null;

        while (tempStack.Count > 0)
        {
            var tempTagPosition = tempStack.Pop();
            tagsInRange.Add(tempTagPosition);

            intersectTag = GetEnclosedTagInBoldAndItalic(tagType, tempTagPosition, matchingOpenTag);

            _tagPositionsStack.Push(tempTagPosition);
        }

        if (matchingOpenTag is null)
        {
            return null;
        }

        var newTag = CreateAndCloseTag(tagType, tagIndex, word, matchingOpenTag);

        ValidateIntersectionRule(tagsInRange, intersectTag, matchingOpenTag, newTag);

        return newTag;
    }

    private void ValidateIntersectionRule(HashSet<TagPosition> tagsInRange, TagPosition? intersectTag, TagPosition matchingOpenTag, TagPosition newTag)
    {
        if (intersectTag != null && !(tagsInRange.Contains(intersectTag.TagPair!)))
        {
            matchingOpenTag.TagState = TagState.NotTag;
            newTag.TagState = TagState.NotTag;
        }
    }

    private TagPosition CreateAndCloseTag(TagType tagType, int tagIndex, string word, TagPosition matchingOpenTag)
    {
        var tagPosition = new TagPosition(tagType, TagState.Close, tagIndex, word)
        {
            TagPair = matchingOpenTag
        };

        matchingOpenTag.TagPair = tagPosition;
        matchingOpenTag.TagState = TagState.Open;

        return tagPosition;
    }

    private TagPosition? GetEnclosedTagInBoldAndItalic(TagType tagType, TagPosition currentTag, TagPosition? matchingOpenTag)
    {
        if (tagType == TagType.ItalicTag
            && currentTag is { TagType: TagType.BoldTag, TagState: TagState.Close }
            && matchingOpenTag != null)
        {
            currentTag.TagPair!.TagState = TagState.NotTag;
            currentTag.TagState = TagState.NotTag;

            return currentTag;
        }

        return null;
    }

    private bool HandleTemporarilyClose(TagType tagType, int tagIndex, string word)
    {
        Stack<TagPosition> tempStack = new Stack<TagPosition>();
        TagPosition? matchingOpenTag = FindMatchingOpenTag(tagType, word, tempStack);

        var newTag = RestoreAndValidateTags(matchingOpenTag, tagType, tempStack, tagIndex, word);

        if (newTag is null)
        {
            return false;
        }

        _tagPositionsStack.Push(newTag);

        return true;
    }

    private bool HandleTagStack(TagType tagType, TagState tagState, int tagIndex, string word)
    {
        if (tagType.IsEscapedTag())
        {
            PushTagToStack(tagType, tagState, tagIndex, word);
            return true;
        }

        if (tagState.IsTemporarilyOpen())
        {
            PushTagToStack(tagType, tagState, tagIndex, word);
            return true;
        }

        if (tagState == TagState.TemporarilyClose)
        {
            return HandleTemporarilyClose(tagType, tagIndex, word);
        }

        return false;
    }

    private TagState DetermineTagState(string word, TagType tagType, int symbolIndex, Token currentToken)
    {
        if (tagType is TagType.EscapedTag or TagType.EscapedItalicTag or TagType.EscapedBoldTag)
        {
            return TagState.SingleTag;
        }

        int shift = currentToken.TagPositions.Any(t => t.TagType == TagType.EscapedTag) ? 2 : 0;
        if (symbolIndex - shift == 0 && (tagType is TagType.ItalicTag or TagType.BoldTag))
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