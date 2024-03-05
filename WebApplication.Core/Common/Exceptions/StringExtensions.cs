using System.Text.RegularExpressions;

namespace WebApplication.Core.Common.Exceptions
{
    public static class StringExtensions
    {
        private const string EMAIL_PATTERN = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public static bool IsValidEmailAddress(this string s)
        {
            return Regex.IsMatch(s, EMAIL_PATTERN);
        }
         
        public static bool IsValidMobileNumber(this string s)
        {
            return (s.StartsWith("0") && long.TryParse(s, out _));
        }
    }
}
