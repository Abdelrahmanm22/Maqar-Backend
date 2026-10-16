namespace Maqar.Api.Errors
{
    /// <summary>
    /// The one error shape every Maqar.Api failure returns: <c>{ statusCode, message }</c>.
    /// Controllers return it directly — <c>NotFound(new ApiResponse(404, "Property not found"))</c>.
    /// </summary>
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private static string? GetDefaultMessageForStatusCode(int statusCode) => statusCode switch
        {
            400 => "Bad Request",
            401 => "You are not Authorized",
            403 => "You are not allowed to do this",
            404 => "Resource Not Found",
            405 => "Method Not Allowed",
            500 => "Internal Server Error",
            _ => null,
        };
    }
}
