using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface ISessionService
{
    // Lists the user's active sessions, flagging which one the presented
    // refresh token belongs to.
    Task<SessionsDto> GetSessionsAsync(Guid userId, string refreshToken);

    Task RevokeAllSessionsAsync(Guid userId);

    Task RevokeSessionAsync(Guid userId, Guid familyId);
}
