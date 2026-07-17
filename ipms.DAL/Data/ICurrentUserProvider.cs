namespace IPMS.DAL.Data;


// Lets the DbContext find out who is making the current request without the
// data layer having to know about HTTP / ASP.NET. The API supplies the real
// implementation; at design time / in tests it simply returns null.
public interface ICurrentUserProvider
{
    Guid? GetUserId();
}
