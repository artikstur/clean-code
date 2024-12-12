using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces.Services;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class UserExistsFilter : IAsyncAuthorizationFilter
{
    private readonly IUsersService _usersService;

    public UserExistsFilter(IUsersService usersService)
    {
        _usersService = usersService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Cookies["tasty-cookies"];
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaims.UserId);

        var userId = Guid.Empty;
        bool isValid = userIdClaim is not null &&
                       Guid.TryParse(userIdClaim.Value, out userId) &&
                       (await _usersService.ExistById(userId)).IsSuccess;

        if (!isValid)
        {
            context.Result = new ForbidResult();
            return;
        }
        
        context.HttpContext.Items["userId"] = userId;
    }
}