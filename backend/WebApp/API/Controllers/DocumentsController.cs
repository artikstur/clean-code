using API.Contracts.Requests;
using API.Filters;
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


    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateAuthorOrEditorFilter))]
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

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> GetDocument(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var documentResult = await _documentsService.Get(userId, documentId);

        return !documentResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(documentResult.Error)
            : Ok(Envelope.Ok(documentResult.Value));
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
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

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/editors")]
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
            : Ok(Envelope.Ok(result.Value));
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/readers")]
    public async Task<IActionResult> GetAllReaders(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _documentsService.GetAllReaders(userId, documentId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok(result.Value));
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpPost("{documentId:guid}/set-permissions")]
    public async Task<IActionResult> SetDocumentPermissions(Guid documentId,
        [FromForm] SetDocumentPermissionsRequest request)
    {
        var ownerIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(ownerIdClaim, out var ownerId))
        {
            return Unauthorized();
        }

        var setRoleResult =
            await _documentsService.SetUserPermission(ownerId, request.DocumentRole, documentId, request.UserId);

        return !setRoleResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(setRoleResult.Error)
            : Ok(Envelope.Ok());
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/get-user-role")]
    public async Task<IActionResult> GetUserRole(Guid documentId, [FromBody] Guid userId)
    {
        var ownerIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(ownerIdClaim, out var ownerId))
        {
            return Unauthorized();
        }

        var userPermissionsResult = await _documentsService.GetUserRole(ownerId, documentId, userId);

        return !userPermissionsResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(userPermissionsResult.Error)
            : Ok(Envelope.Ok(userPermissionsResult.Value.ToString()));
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/users")]
    public async Task<IActionResult> GetDocumentUsers(Guid documentId)
    {
        var ownerIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(ownerIdClaim, out var ownerId))
        {
            return Unauthorized();
        }

        var userPermissionsResult = await _documentsService.GetAllUsers(ownerId, documentId);

        return !userPermissionsResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(userPermissionsResult.Error)
            : Ok(Envelope.Ok(userPermissionsResult.Value));
    }

    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateAuthorOrReaderFilter))]
    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> GenerateDownloadLink(Guid documentId)
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var downloadUrl = await _documentsService.GetDownloadUrl(userId, documentId);

        return Ok(new
        {
            Success = true,
            Link = downloadUrl
        });
    }
}