using API.Contracts.Requests;
using API.Filters;
using Application.Interfaces.Services;
using Application.Services;
using Application.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Authorize]
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

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentEditorsFilter))]
    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] MdPushRequest request)
    {
        var pushResult = await _mdService.Push(request.NewContent, request.DocumentId);

        return pushResult.IsSuccess
            ? Ok(Envelope.Ok())
            : _errorResponseFactory.CreateResponse(pushResult.Error);
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentReadersFilter))]
    [HttpGet("pull")]
    public async Task<IActionResult> Pull([FromQuery] MdPullRequest request)
    {
        var pullResult = await _mdService.Pull(request.DocumentId);

        return pullResult.IsSuccess
            ? Ok(Envelope.Ok(pullResult.Value))
            : _errorResponseFactory.CreateResponse(pullResult.Error);
    }

    [HttpPost("html")]
    [Produces("text/plain")]
    public async Task<IActionResult> GetHtml([FromBody] string rawMarkdown)
    {
        var htmlResult = await _mdService.GetHtml(rawMarkdown);

        return htmlResult.IsSuccess
            ? Content(htmlResult.Value)
            : _errorResponseFactory.CreateResponse(htmlResult.Error);
    }
}