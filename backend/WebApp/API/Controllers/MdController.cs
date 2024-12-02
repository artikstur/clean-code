using API.Contracts.Requests;
using Application.Interfaces.Services;
using Application.Services;
using Application.Utils;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MdController : ControllerBase
{
    private readonly ErrorResponseFactory _errorResponseFactory;
    private readonly IMdService _mdService;

    public MdController(ErrorResponseFactory errorResponseFactory, IMdService mdService)
    {
        _errorResponseFactory = errorResponseFactory;
        _mdService = mdService;
    }

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] MdPushRequest request)
    {
        var pushResult = await _mdService.Push(request.UserId, request.NewContent, request.DocumentId);

        return pushResult.IsSuccess
            ? Ok(Envelope.Ok())
            : _errorResponseFactory.CreateResponse(pushResult.Error);
    }

    [HttpGet("pull")]
    public async Task<IActionResult> Pull([FromQuery] MdPullRequest request)
    {
        var pullResult = await _mdService.Pull(request.UserId, request.DocumentId);

        return pullResult.IsSuccess
            ? Ok(Envelope.Ok(pullResult.Value))
            : _errorResponseFactory.CreateResponse(pullResult.Error);
    }
    
    [HttpGet("html")]
    public async Task<IActionResult> GetHtml([FromBody] string rawMarkdown)
    {
        var htmlResult = await _mdService.GetHtml(rawMarkdown);

        return htmlResult.IsSuccess
            ? Ok(Envelope.Ok(htmlResult.Value))
            : _errorResponseFactory.CreateResponse(htmlResult.Error);
    }
}