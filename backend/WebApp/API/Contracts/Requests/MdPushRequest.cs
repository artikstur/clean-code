namespace API.Contracts.Requests;

public record MdPushRequest(string NewContent, Guid DocumentId);