namespace Maqar.Api.Helpers
{
    public static class PhoneNumbers
    {
        /// <summary>
        /// The digits-only international form wa.me expects. Egyptian local numbers ("01012345678") get the
        /// country code ("201012345678"); anything else just loses its spaces, dashes and "+".
        /// Null when nothing usable is left.
        /// </summary>
        public static string? ToWhatsApp(string? number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return null;

            var digits = new string(number.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits.StartsWith('0'))
                digits = "2" + digits;

            return digits.Length >= 8 ? digits : null;
        }
    }
}
