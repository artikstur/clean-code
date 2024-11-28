namespace API.Contracts.Responses;

public record GetAllEditorsResponse(ICollection<GetAllEditorsResponseDto> UserDtos);

public class GetAllEditorsResponseDto
{
    public string UserName { get; set; }
}
