using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Maqar.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Maqar.Domain.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public Gender Gender { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The broker's WhatsApp number (digits only, as wa.me expects). The public site uses it — then
        /// PhoneNumber, then the site-wide number — for the WhatsApp button on this user's listings.
        /// </summary>
        public string? WhatsAppNumber { get; set; }

        /// <summary>Optional profile image URL.</summary>
        public string? ImageUrl { get; set; }
    }
}
