namespace Smartitecture.Core.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string[]? Details { get; set; }
        public int StatusCode { get; set; }
        public string Timestamp { get; set; }
        public string Path { get; set; }

        public ErrorResponse(string message, string errorCode, string[]? details = null, int statusCode = 500, string path = "")
        {
            Message = message;
            ErrorCode = errorCode;
            Details = details;
            StatusCode = statusCode;
            Timestamp = DateTime.UtcNow.ToString("O");
            Path = path;
        }
    }
}
