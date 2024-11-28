namespace MarkdownRenderer.Enums;

public enum TagState
{
    Close = 1,
    Open = 2,
    TemporarilyOpen = 3,
    TemporarilyOpenInWord = 4,
    TemporarilyClose = 5,
    SingleTag = 6,
    NotTag = 8,
}