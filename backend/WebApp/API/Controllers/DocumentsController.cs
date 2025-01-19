using API.Contracts.Requests;
using API.Filters;
using Application.Interfaces.Services;
using Application.Services;
using Application.Utils;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Authorize]
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

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpPut("{documentId:guid}/rename")]
    public async Task<IActionResult> RenameDocument(Guid documentId, [FromQuery] string newName)
    {
        var renameResult = await _documentsService.Rename(documentId, newName);

        return !renameResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(renameResult.Error)
            : Ok(Envelope.Ok());
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> GetDocument(Guid documentId)
    {
        var documentResult = await _documentsService.Get(documentId);

        return !documentResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(documentResult.Error)
            : Ok(Envelope.Ok(documentResult.Value));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var deleteResult = await _documentsService.Delete(documentId);

        return !deleteResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(deleteResult.Error)
            : Ok(Envelope.Ok());
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [HttpGet("user-documents")]
    public async Task<IActionResult> GetUserDocuments()
    {
        var userIdClaim = User.FindFirst(CustomClaims.UserId)?.Value;

        var getUserDocumentsResult = await _documentsService.GetUserDocuments(Guid.Parse(userIdClaim!));

        return !getUserDocumentsResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(getUserDocumentsResult.Error)
            : Ok(Envelope.Ok(getUserDocumentsResult.Value));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/editors")]
    public async Task<IActionResult> GetAllEditors(Guid documentId)
    {
        var result = await _documentsService.GetAllEditors(documentId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok(result.Value));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/readers")]
    public async Task<IActionResult> GetAllReaders(Guid documentId)
    {
        var result = await _documentsService.GetAllReaders(documentId);

        return !result.IsSuccess
            ? _errorResponseFactory.CreateResponse(result.Error)
            : Ok(Envelope.Ok(result.Value));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpPost("{documentId:guid}/set-permissions")]
    public async Task<IActionResult> SetDocumentPermissions(Guid documentId,
        [FromForm] SetDocumentPermissionsRequest request)
    {
        var setRoleResult =
            await _documentsService.SetUserPermission(request.DocumentRole, documentId, request.UserId);

        return !setRoleResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(setRoleResult.Error)
            : Ok(Envelope.Ok());
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpPost("{documentId:guid}/get-user-role")]
    public async Task<IActionResult> GetUserRole(Guid documentId, [FromBody] Guid userId)
    {
        var userPermissionsResult = await _documentsService.GetUserRole(documentId, userId);

        return !userPermissionsResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(userPermissionsResult.Error)
            : Ok(Envelope.Ok(userPermissionsResult.Value.ToString()));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentAuthorFilter))]
    [HttpGet("{documentId:guid}/users")]
    public async Task<IActionResult> GetDocumentUsers(Guid documentId)
    {
        var userPermissionsResult = await _documentsService.GetAllUsers(documentId);

        return !userPermissionsResult.IsSuccess
            ? _errorResponseFactory.CreateResponse(userPermissionsResult.Error)
            : Ok(Envelope.Ok(userPermissionsResult.Value));
    }

    [ServiceFilter(typeof(UserExistsFilter))]
    [ServiceFilter(typeof(DocumentExistsFilter))]
    [ServiceFilter(typeof(ValidateDocumentReadersFilter))]
    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> GenerateDownloadLink(Guid documentId)
    {
        var downloadUrl = await _documentsService.GetDownloadUrl(documentId);

        return Ok(new
        {
            Success = true,
            Link = downloadUrl
        });
    }
}