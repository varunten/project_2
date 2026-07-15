namespace IPMS.DTO.Entities;

public class TokenFamily: BaseEntity
{
    public DateTimeOffset? RevokedAt {get; set;}
    public required Guid UserId {get; set;}
}
