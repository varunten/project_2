namespace IPMS.DTO.Entities;



public class Role: BaseEntity
{
    public required string Name { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public DateTimeOffset? DeletedAt {get; set;}
}
