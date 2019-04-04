using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal
{
    public static class JsonHelper
    {
        public static MvcHtmlString GetJson(object vm)
        {
            string s = JsonConvert.SerializeObject(vm, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Include });
            return new MvcHtmlString(s);
        }

        public static string GetJsonString(object vm)
        {
            return JsonConvert.SerializeObject(vm, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Include });
        }

        public static T ParseJson<T>(string obj)
        {

            return JsonConvert.DeserializeObject<T>(obj, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Include, MissingMemberHandling = MissingMemberHandling.Ignore });
        }

    }
}