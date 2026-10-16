using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Maqar.Admin.ViewModels
{
    /// <summary>Shared fields of the Area / Feature lookups. Key lives on each subclass — its [Remote] differs.</summary>
    public abstract class LookupViewModel
    {
        public const string KeyPattern = "^[a-z][a-zA-Z0-9]*$";
        public const string KeyPatternMessage = "Use a camelCase key: letters and digits, starting lowercase (e.g. newCairo).";

        public int Id { get; set; }
        public bool IsCreate => Id == 0;

        public abstract string Key { get; set; }

        [Required, StringLength(100), Display(Name = "Name (Arabic)")]
        public string NameAr { get; set; } = string.Empty;

        [Required, StringLength(100), Display(Name = "Name (English)")]
        public string NameEn { get; set; } = string.Empty;

        [Range(0, 1000), Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// The key is the API contract — the public site filters and translates by it — so it is a camelCase
    /// identifier, unique (checked by [Remote] and again on save), and shouldn't change once listings use it.
    /// </summary>
    public class AreaViewModel : LookupViewModel
    {
        [Required, StringLength(50), RegularExpression(KeyPattern, ErrorMessage = KeyPatternMessage)]
        [Remote("IsKeyAvailable", "Area", AdditionalFields = nameof(Id), ErrorMessage = "Another area already uses this key.")]
        [Display(Name = "Key")]
        public override string Key { get; set; } = string.Empty;

        /// <summary>Display only — listings (deleted ones included) that hold the area.</summary>
        public int PropertyCount { get; set; }
    }

    public class FeatureViewModel : LookupViewModel
    {
        [Required, StringLength(50), RegularExpression(KeyPattern, ErrorMessage = KeyPatternMessage)]
        [Remote("IsKeyAvailable", "Feature", AdditionalFields = nameof(Id), ErrorMessage = "Another feature already uses this key.")]
        [Display(Name = "Key")]
        public override string Key { get; set; } = string.Empty;

        [StringLength(50)]
        [RegularExpression("^bxs?-[a-z0-9-]+$", ErrorMessage = "Use a Boxicons class such as bx-swim.")]
        [Display(Name = "Icon (Boxicons class)")]
        public string? Icon { get; set; }

        /// <summary>Display only — how many listings offer it.</summary>
        public int PropertyCount { get; set; }
    }
}
