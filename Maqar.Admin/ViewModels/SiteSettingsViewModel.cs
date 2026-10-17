using System.ComponentModel.DataAnnotations;

namespace Maqar.Admin.ViewModels
{
    public class SiteSettingsViewModel
    {
        [Required, StringLength(30)]
        [RegularExpression(@"^\+?[0-9]{8,20}$", ErrorMessage = "Digits with an optional leading +, e.g. +201000000000.")]
        [Display(Name = "Phone (for tel: links)")]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(40)]
        [Display(Name = "Phone (as displayed)")]
        public string PhoneDisplay { get; set; } = string.Empty;

        [Required, StringLength(30)]
        [RegularExpression(@"^[0-9]{8,20}$", ErrorMessage = "Digits only, with the country code, e.g. 201000000000.")]
        [Display(Name = "WhatsApp number")]
        public string WhatsApp { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(300), Display(Name = "Address (Arabic)")]
        public string? AddressAr { get; set; }

        [StringLength(300), Display(Name = "Address (English)")]
        public string? AddressEn { get; set; }

        [Url, StringLength(300), Display(Name = "Facebook page URL")]
        public string? FacebookUrl { get; set; }

        [Url, StringLength(300), Display(Name = "Instagram URL")]
        public string? InstagramUrl { get; set; }

        /// <summary>Display only.</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
