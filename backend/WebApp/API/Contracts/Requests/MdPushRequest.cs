namespace API.Contracts.Requests;

public record MdPushRequest(Guid UserId, string NewContent, Guid DocumentId);