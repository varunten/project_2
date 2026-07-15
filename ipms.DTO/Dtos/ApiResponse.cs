namespace IPMS.DTO.Dtos;


// Standard success envelope returned by every endpoint.
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}


// Small helper so controllers can write ApiResponse.Ok(dto, "message").
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
}


// Standard error envelope returned by the exception middleware and the
// validation (422) handler.
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;

    // Field -> messages. Only populated for validation (422) errors.
    public Dictionary<string, string[]>? Errors { get; set; }
}
