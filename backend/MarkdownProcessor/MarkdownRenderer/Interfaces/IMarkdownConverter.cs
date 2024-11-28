using static System.Net.Mime.MediaTypeNames;

namespace MarkdownRenderer.Interfaces;

/// <summary>
/// Интерфейс основного класса MarkdownConverter
/// </summary>
public interface IMarkdownConverter
{
    /// <summary>
    /// Преобразует markdown текст в html
    /// </summary>
    /// <param name="unprocessedText">Markdown текст.</param>
    /// <returns>Строка с html страничкой.</returns>
    string ConvertToHtml(string unprocessedText);
}