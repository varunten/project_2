namespace IPMS.DTO.Exceptions;


// Base class for every "expected" error the application throws on purpose.
// The middleware reads StatusCode to build the HTTP response, so services can
// just throw one of these instead of returning IActionResults.
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}


// 400 - the request itself is malformed / not allowed in this state.
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(400, message) { }
}


// 401 - authentication failed (bad credentials, invalid token).
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(401, message) { }
}


// 403 - authenticated, but not allowed to do this action.
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(403, message) { }
}


// 404 - the requested resource does not exist.
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404, message) { }
}


// 409 - the action conflicts with the current state (e.g. duplicate, wrong status).
public class ConflictException : AppException
{
    public ConflictException(string message) : base(409, message) { }
}


// 422 - the request was understood but a business/validation rule failed.
public class ValidationException : AppException
{
    public ValidationException(string message) : base(422, message) { }
}
