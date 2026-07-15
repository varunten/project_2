namespace ipms.MVC.Services;


// Thrown when the API answers with an error status. Carries the API's message
// and, for 422s, the per-field validation errors so we can show them on a form.
public class ApiException : Exception
{
    public int StatusCode { get; }

    public Dictionary<string, string[]>? Errors { get; }

    public ApiException(
        int statusCode,
        string message,
        Dictionary<string, string[]>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
