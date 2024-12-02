using Application.Utils;

namespace Application.Interfaces.Services;

public interface IMdService
{
    Task<Result> Push(Guid userId, string newContent, Guid documentId);
    Task<Result<string>> Pull(Guid userId, Guid documentId);
    Task<Result<string>> GetHtml(string rawMarkdown);
}