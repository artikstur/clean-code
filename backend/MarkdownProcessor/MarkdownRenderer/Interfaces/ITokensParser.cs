namespace MarkdownRenderer.Interfaces;

public interface ITokensParser
{
    IEnumerable<Token> ParseTokens(string unprocessedText);
}