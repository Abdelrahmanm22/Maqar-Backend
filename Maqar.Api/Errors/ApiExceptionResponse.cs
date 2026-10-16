namespace Maqar.Api.Errors
{
    /// <summary>Written by <c>ExceptionMiddleware</c>. <see cref="Details"/> (the stack trace) is set in Development only.</summary>
    public class ApiExceptionResponse : ApiResponse
    {
        public string? Details { get; set; }

        public ApiExceptionResponse(int statusCode, string? message = null, string? details = null)
            : base(statusCode, message)
        {
            Details = details;
        }
    }
}
