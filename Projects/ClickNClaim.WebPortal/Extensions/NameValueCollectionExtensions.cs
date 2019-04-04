using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Extensions
{
    public static  class NameValueCollectionExtensions
    {
        public static string Join(this NameValueCollection collection, Func<string, string> selector, string separator)
        {
            return String.Join(separator, collection.Cast<string>().Select(e => selector(e)));
        }
    }
}