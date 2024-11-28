using MarkdownRenderer.Extensions;

namespace MarkdownRenderer;

public class LinkReferenceHandler
{ 
    public readonly Dictionary<string, (string, string)> LinkReferences = new();
    public bool IsReferenceStyleLink(string line)
    {
        if (line.TrimStart().StartsWith("[") && line.Contains("]:"))
        {
            int urlStartIndex = line.IndexOf("]:", StringComparison.Ordinal) + 2;

            string[] urlData = line.Substring(urlStartIndex)
                .Trim()
                .SplitBySpacesIgnoringQuotes().ToArray();

            if (urlData.Length == 2)
            {
                string title = urlData[1];
                if (title[0] == '"' && title[^1] == '"')
                {
                    return true;
                }
                return false;
            }

            if (urlData.Length == 1)
            {
                return true;
            }
        }

        return false;
    }
    public void HandleReferenceStyleLink(string line)
    {
        string urlKey = string.Join(" ", line
            .Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1)
            .Trim()
            .SplitBySpacesIgnoringQuotes());

        int urlStartIndex = line.IndexOf("]:", StringComparison.Ordinal) + 2;

        string[] urlData = line.Substring(urlStartIndex)
            .TrimStart()
            .SplitBySpacesIgnoringQuotes().ToArray();

        LinkReferences.Add(urlKey, (urlData[0], urlData.Length == 2 ? urlData[1] : string.Empty));
    }

    public string ConvertLinkFromWordToHtml(string word)
    {
        int firstOpenBracketIndex = word.IndexOf('[');
        int firstCloseBracketIndex = word.IndexOf(']', firstOpenBracketIndex);

        var linkTextTemp = word.Substring(firstOpenBracketIndex + 1, firstCloseBracketIndex - firstOpenBracketIndex - 1)
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string linkText = string.Join(" ", linkTextTemp);

        int secondOpenBracketIndex = word.IndexOf('(', firstCloseBracketIndex);
        int secondCloseBracketIndex = word.IndexOf(')', secondOpenBracketIndex);

        var linkTemp = word.Substring(secondOpenBracketIndex + 1, secondCloseBracketIndex - secondOpenBracketIndex - 1)
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string link = string.Join(" ", linkTemp);

        string beforeBracket = firstOpenBracketIndex != 0 ? word.Substring(0, firstOpenBracketIndex) : string.Empty;
        string afterBracket = secondCloseBracketIndex != word.Length - 1 ? word.Substring(secondCloseBracketIndex + 1) : string.Empty;
        string htmlLink = $"{beforeBracket}<a href=\"{link}\">{linkText}</a>{afterBracket}";

        return htmlLink;
    }
    public string ConvertLinkFromRefToHtml(string word)
    {
        int firstOpenBracketIndex = word.IndexOf('[');
        int firstCloseBracketIndex = word.IndexOf(']', firstOpenBracketIndex);

        var linkTextTemp = word.Substring(firstOpenBracketIndex + 1, firstCloseBracketIndex - firstOpenBracketIndex - 1)
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string linkText = string.Join(" ", linkTextTemp);

        int secondOpenBracketIndex = word.IndexOf('[', firstCloseBracketIndex);
        int secondCloseBracketIndex = word.IndexOf(']', secondOpenBracketIndex);

        var linkIdTemp = word.Substring(secondOpenBracketIndex + 1, secondCloseBracketIndex - secondOpenBracketIndex - 1)
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string linkId = string.Join(" ", linkIdTemp);

        if (LinkReferences.ContainsKey(linkId))
        {
            string beforeBracket = firstOpenBracketIndex != 0 ? word.Substring(0, firstOpenBracketIndex) : string.Empty;
            string afterBracket = secondCloseBracketIndex != word.Length - 1 ? word.Substring(secondCloseBracketIndex + 1) : string.Empty;
            return $"{beforeBracket}<a href=\"{LinkReferences[linkId].Item1}\" title={LinkReferences[linkId].Item2}>{linkText}</a>{afterBracket}";
        }

        return word;
    }
}