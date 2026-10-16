namespace Maqar.Api.Errors
{
    /// <summary>Built by the <c>InvalidModelStateResponseFactory</c> in Program.cs for every invalid request body.</summary>
    public class ApiValidationErrorResponse : ApiResponse
    {
        public IEnumerable<string> Errors { get; set; }

        public ApiValidationErrorResponse() : base(400)
        {
            Errors = new List<string>();
        }
    }
}
