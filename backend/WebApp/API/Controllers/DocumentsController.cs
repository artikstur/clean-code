using API.Contracts.Responses;
using Application.Interfaces.Services;
using Application.Services;
using Application.Utils;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ErrorResponseFactory _errorResponseFactory;
    private readonly IDocumentsService _documentsService;

    public DocumentsController(ErrorResponseFactory errorResponseFactory, IDocumentsService documentsService)
    {
        _errorResponseFactory = errorResponseFactory;
        _documentsService = documentsService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateDocument([FromQuery] string name)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var createResult = await _documentsService.Create(userId, name);

        return !createResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(createResult.Error)
            : Ok(Envelope.Ok());
    }

    [HttpPut("{documentId:guid}/rename")]
    public async Task<IActionResult> RenameDocument(Guid documentId, [FromQuery] string newName)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var renameResult = await _documentsService.Rename(userId, documentId, newName);

        return !renameResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(renameResult.Error)
            : Ok(Envelope.Ok());
    }

    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> GetDocument(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var getResult = await _documentsService.Get(userId, documentId);

        return !getResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(getResult.Error)
            : Ok(Envelope.Ok(getResult.Value));
    }

    [HttpPut("{documentId:guid}/update-content")]
    public async Task<IActionResult> UpdateDocumentContent(Guid documentId, [FromBody] string newContent)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var updateResult = await _documentsService.Update(userId, documentId, newContent);

        return !updateResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(updateResult.Error)
            : Ok(Envelope.Ok());
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var deleteResult = await _documentsService.Delete(userId, documentId);

        return !deleteResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(deleteResult.Error)
            : Ok(Envelope.Ok());
    }

    [HttpPost("add-editor")]
    public async Task<IActionResult> AddEditorToDocument([FromQuery] Guid documentId, [FromQuery] Guid newUserId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _documentsService.AddUserAsEditor(userId, documentId, newUserId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok());
    }

    [HttpPost("remove-editor")]
    public async Task<IActionResult> RemoveEditorFromDocument([FromQuery] Guid documentId, [FromQuery] Guid oldUserId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _documentsService.RemoveUserFromEditors(userId, documentId, oldUserId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok());
    }

    [HttpGet("{documentId}/editors")]
    public async Task<IActionResult> GetAllEditors(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _documentsService.GetAllEditors(userId, documentId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok(new GetAllEditorsResponse(
                result.Value.Select(u => new GetAllEditorsResponseDto()
                {
                    UserName = u.UserName,
                }).ToList())));
    }
}