using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces.Services;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ValidateAuthorOrReaderFilter : IAsyncResourceFilter
{
    private readonly IDocumentsAccessService _documentsAccessService;

    public ValidateAuthorOrReaderFilter(IDocumentsAccessService documentsAccessService)
    {
        _documentsAccessService = documentsAccessService;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var token = context.HttpContext.Request.Cookies["tasty-cookies"];
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaims.UserId);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var access = context.HttpContext.Items.TryGetValue("DocumentId", out var documentIdObj) &&
                         Guid.TryParse(documentIdObj?.ToString(), out var documentId) &&
                         (await _documentsAccessService.CheckAccessToRead(userId, documentId) ||
                          await _documentsAccessService.IsAuthor(userId, documentId));
            
            if (access)
            {
                await next();
            }

            context.Result = new BadRequestObjectResult(new { Error = "You are not author or reader" });
        }
        catch
        {
            context.Result = new UnauthorizedResult();
        }
    }
}