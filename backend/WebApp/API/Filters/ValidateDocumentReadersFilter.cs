using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ValidateDocumentReadersFilter : IAsyncAuthorizationFilter
{
    private readonly IDocumentsAccessService _documentsAccessService;

    public ValidateDocumentReadersFilter(IDocumentsAccessService documentsAccessService)
    {
        _documentsAccessService = documentsAccessService;
    }
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        bool isValid = context.HttpContext.Items.TryGetValue("documentId", out var documentIdObj) &&
                       context.HttpContext.Items.TryGetValue("userId", out var userIdObj) &&
                       documentIdObj is not null && userIdObj is not null &&
                       Guid.TryParse(documentIdObj.ToString(), out var documentId) &&
                       Guid.TryParse(userIdObj.ToString(), out var userId) &&
                       await _documentsAccessService.CheckAccessToRead(userId, documentId);

        if (!isValid)
        {
            context.Result = new ForbidResult();
        }
    }
}