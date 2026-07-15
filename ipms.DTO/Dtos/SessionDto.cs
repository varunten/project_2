namespace IPMS.DTO.Dtos;



public class SessionDto
{
    public required Guid FamilyId {get; set;}
    public required DateTimeOffset CreatedAt {get; set;}
    public required bool Current {get; set;}
}


public class SessionsDto
{
    public List<SessionDto> Sessions {get; set;} = new();
}