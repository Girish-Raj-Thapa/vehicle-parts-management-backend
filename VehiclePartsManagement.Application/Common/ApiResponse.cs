namespace VehiclePartsManagement.Application.Common;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = [];
    public object? Data { get; set; }

    public static ApiResponse SuccessResponse(string message = "Request completed successfully") =>
        new() { Success = true, Message = message, Data = null };

    public static ApiResponse FailureResponse(
        string message = "Request failed",
        Dictionary<string, List<string>>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [], Data = null };
}

public class ApiResponse<T> : ApiResponse
{
    public static ApiResponse<T> SuccessResponse(
        T data,
        string message = "Request completed successfully") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResponse(
        string message = "Request failed",
        Dictionary<string, List<string>>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [], Data = null };
}
