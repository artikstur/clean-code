using System.IdentityModel.Tokens.Jwt;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using API.Contracts.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class DocumentExistsFilter : IAsyncAuthorizationFilter
{
    private readonly IDocumentsAccessService _documentsAccessService;

    public DocumentExistsFilter(IDocumentsAccessService documentsAccessService)
    {
        _documentsAccessService = documentsAccessService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.RouteValues.TryGetValue("documentId", out var documentIdValue) &&
            await IsDocumentValid(context, documentIdValue?.ToString())) return;

        if (context.HttpContext.Request.Query.TryGetValue("documentId", out var documentIdQueryValue) &&
            await IsDocumentValid(context, documentIdQueryValue.ToString())) return;
        
        context.HttpContext.Request.EnableBuffering();
        var pipeReader = PipeReader.Create(context.HttpContext.Request.Body);
        var body = await ReadBodyUsingPipeReader(pipeReader);
        context.HttpContext.Request.Body.Position = 0;

        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("documentId", out var documentIdElement) &&
            await IsDocumentValid(context, documentIdElement.ToString()))
        {
            return;
        }
        
        context.Result = new ForbidResult();
    }

    private async Task<bool> IsDocumentValid(AuthorizationFilterContext context, string? documentId)
    {
        if (!Guid.TryParse(documentId, out var tempDocumentId)) return false;
        if (!await _documentsAccessService.ExistById(tempDocumentId)) return false;

        context.HttpContext.Items["documentId"] = tempDocumentId;
        return true;
    }
    
    private async Task<string> ReadBodyUsingPipeReader(PipeReader pipeReader)
    {
        var body = new StringBuilder();
        while (true)
        {
            var result = await pipeReader.ReadAsync();
            var buffer = result.Buffer;

            foreach (var segment in buffer)
            {
                body.Append(Encoding.UTF8.GetString(segment.Span));
            }

            pipeReader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        return body.ToString();
    }
}