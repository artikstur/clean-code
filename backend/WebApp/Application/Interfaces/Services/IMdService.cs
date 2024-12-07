using Application.Utils;

namespace Application.Interfaces.Services;

public interface IMdService
{
    Task<Result> Push(string newContent, Guid documentId);
    Task<Result<string>> Pull(Guid documentId);
    Task<Result<string>> GetHtml(string rawMarkdown);
}