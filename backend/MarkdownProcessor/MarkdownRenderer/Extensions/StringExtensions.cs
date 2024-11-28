using System.Text;

namespace MarkdownRenderer.Extensions;

public static class StringExtensions
{
    public static IEnumerable<string> SplitBySpacesIgnoringQuotes(this string line)
    {
        List<string> result = new List<string>();
        StringBuilder currentWord = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                currentWord.Append("\"");
                insideQuotes = !insideQuotes; 
            }
            else if (c == ' ' && !insideQuotes)
            {
                if (currentWord.Length > 0)
                {
                    result.Add(currentWord.ToString().Trim());
                    currentWord.Clear(); 
                }
            }
            else
            {
                currentWord.Append(c); 
            }
        }

        if (currentWord.Length > 0)
        {
            result.Add(currentWord.ToString().Trim());
        }

        return result;
    }

    public static bool IsWordContainsReferenceToLink(this string word)
    {
        int firstOpenBracketIndex = word.IndexOf('[');
        if (firstOpenBracketIndex == -1)
            return false;

        int firstCloseBracketIndex = word.IndexOf(']', firstOpenBracketIndex);
        if (firstCloseBracketIndex == -1)
            return false;

        int secondOpenBracketIndex = word.IndexOf('[', firstCloseBracketIndex);
        if (secondOpenBracketIndex == -1)
            return false;

        int secondCloseBracketIndex = word.IndexOf(']', secondOpenBracketIndex);
        if (secondCloseBracketIndex == -1)
            return false;

        return firstCloseBracketIndex + 1 == secondOpenBracketIndex;
    }
    public static bool IsWordContainsLink(this string word)
    {
        int firstOpenBracketIndex = word.IndexOf('[');
        if (firstOpenBracketIndex == -1)
            return false;

        int firstCloseBracketIndex = word.IndexOf(']', firstOpenBracketIndex);
        if (firstCloseBracketIndex == -1)
            return false;

        int secondOpenBracketIndex = word.IndexOf('(', firstCloseBracketIndex);
        if (secondOpenBracketIndex == -1)
            return false;

        int secondCloseBracketIndex = word.IndexOf(')', secondOpenBracketIndex);
        if (secondCloseBracketIndex == -1)
            return false;

        return firstCloseBracketIndex + 1 == secondOpenBracketIndex;
    }
}
