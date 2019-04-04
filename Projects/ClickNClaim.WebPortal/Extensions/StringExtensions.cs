using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ClickNClaim.WebPortal.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveAccent(this string s)
        {
            return Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(s));
        }

        public static string Clean(this string s)
        {
            var sb = new StringBuilder(
                Regex.Replace(
                    HttpUtility.HtmlDecode(s.Replace("&", "and"))
                                            .RemoveAccent(), @"(?!W+$)W+(?<!^W+)", "")
                                            .Trim()
                    );
            sb.Replace("  ", " ").Replace(" ", "-");

            return sb.ToString().ToLower();
        }
    }
}