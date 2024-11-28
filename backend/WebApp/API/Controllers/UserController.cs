using API.Contracts.Requests;
using Application.Interfaces.Services;
using Application.Services;
using Application.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ErrorResponseFactory _errorResponseFactory;

    public UsersController(IUsersService userService, ErrorResponseFactory errorResponseFactory)
    {
        _usersService = userService;
        _errorResponseFactory = errorResponseFactory;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] CreateUserRequest request,
        [FromServices] IValidator<CreateUserRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(error => new ResponseError("VALIDATION_ERROR", error.ErrorMessage, error.PropertyName))
                .ToList();

            return BadRequest(Envelope.Error(errors));
        }

        var registerResult = await _usersService.Register(request.UserName, request.Email, request.Password);

        return !registerResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(registerResult.Error)
            : Ok(Envelope.Ok());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var loginResult = await _usersService.Login(request.Email, request.Password);

        if (!loginResult.IsSuccess)
        {
            return _errorResponseFactory.CreateResponse(loginResult.Error);
        }

        var token = loginResult.Value;
        Response.Cookies.Append("tasty-cookies", token);

        return Ok(Envelope.Ok(token));
    }

    [HttpGet("{userName}")]
    public async Task<IActionResult> GetUserByUserName([FromRoute] string userName)
    {
        var userResult = await _usersService.GetUserByUserName(userName);

        return !userResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(userResult.Error)
            : Ok(Envelope.Ok(userResult.Value));
    }

    [Authorize]
    [HttpGet("/test-authentication")]
    public Task<IActionResult> TestAuthentication()
    {
        return Task.FromResult<IActionResult>(Ok(Envelope.Ok()));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("/test-admin")]
    public Task<IActionResult> TestAdmin()
    {
        return Task.FromResult<IActionResult>(Ok(Envelope.Ok()));
    }
}