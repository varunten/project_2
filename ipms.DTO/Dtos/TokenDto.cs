namespace IPMS.DTO.Dtos;


public class TokenDto
{
    public string AccessToken {get; set;} = string.Empty;
    public string RefreshToken {get; set;} = string.Empty;
    public string Type {get; set;} = "Bearer";
}


public class RefreshTokenDto
{
    public string Token {get; set;} = string.Empty;
}
