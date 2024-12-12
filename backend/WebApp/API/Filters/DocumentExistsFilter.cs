using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using API.Contracts.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class DocumentExistsFilter : IAsyncResourceFilter
{
    private readonly IDocumentsAccessService _documentsAccessService;

    public DocumentExistsFilter(IDocumentsAccessService documentsAccessService)
    {
        _documentsAccessService = documentsAccessService;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        try
        {
            var documentId = Guid.Empty;
            
            if (context.HttpContext.Request.RouteValues.TryGetValue("documentId", out var documentIdValue))
            {
                if (Guid.TryParse(documentIdValue?.ToString(), out var tempDocumentId))
                {
                    if (await _documentsAccessService.ExistById(tempDocumentId))
                    {
                        documentId = tempDocumentId;
                    }
                }
            }
            
            // убрать эти элсы
            else if (context.HttpContext.Request.Query.TryGetValue("documentId", out var documentIdQueryValue))
            {
                if (Guid.TryParse(documentIdQueryValue.ToString(), out var tempDocumentId))
                {
                    if (await _documentsAccessService.ExistById(tempDocumentId))
                    {
                        documentId = tempDocumentId;
                    }
                }
                
                await next();
                return;
            }
            else
            {
                using var reader = new StreamReader(context.HttpContext.Request.Body);
                var body = await reader.ReadToEndAsync();

                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("documentId", out JsonElement documentIdElement))
                {
                    if (Guid.TryParse(documentIdElement.ToString(), out var tempDocumentId))
                    {
                        if (await _documentsAccessService.ExistById(tempDocumentId))
                        {
                            documentId = tempDocumentId;
                        }
                    }
                }
            }
            
            // добавить в общий блок или атрибут на валидацию параметра 
            if (documentId == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult(new { Error = "Invalid documentId" });
                return;
            }
            
            context.HttpContext.Items["DocumentId"] = documentId;
            await next();
        }
        catch
        {
            context.Result = new UnauthorizedResult();
        }
    }
}