using System.ComponentModel.DataAnnotations;

namespace Maqar.Api.Dtos
{
    public sealed record SiteSettingsDto(
        string Phone,
        string PhoneDisplay,
        string Whatsapp,
        string Email,
        LocalizedDto? Address,
        string? FacebookUrl,
        string? InstagramUrl);

    /// <summary>The contact form, optionally sent from a listing page (<see cref="PropertyId"/>).</summary>
    public sealed class ContactRequestDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(30, MinimumLength = 6)]
        [RegularExpression(@"^\+?[0-9 ()-]{6,30}$", ErrorMessage = "The Phone field must contain digits, spaces, dashes or a leading +.")]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(2000, MinimumLength = 5)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Set when the message is an inquiry about a listing. Must be a published listing.</summary>
        [Range(1, int.MaxValue)]
        public int? PropertyId { get; set; }
    }

    public sealed record ContactCreatedDto(int Id);

    public sealed class PropertyEventRequestDto
    {
        public const string View = "view";
        public const string Whatsapp = "whatsapp";
        public const string Call = "call";

        /// <summary>What happened on the listing page: "view", "whatsapp" or "call".</summary>
        [Required(ErrorMessage = "type is required: view, whatsapp or call.")]
        [RegularExpression("^(view|whatsapp|call)$", ErrorMessage = "type must be one of: view, whatsapp, call.")]
        public string? Type { get; set; }
    }
}
