using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.BodaccReader
{
    public class BodaccReader
    {
        private string firmApiBodaccTemplate = "https://firmapi.com/api/v1/companies/{0}/notices?api_key=5cad63ea1e284b706bbf02487d500e453e1d1003";

        public List<notice> GetBodacc(string siren)
        {
            WebClient wc = new WebClient();
            var res = wc.DownloadString(String.Format(firmApiBodaccTemplate, siren));
            byte[] bytes = Encoding.Default.GetBytes(res);
            res = Encoding.UTF8.GetString(bytes);


            var rez = JsonConvert.DeserializeObject<result>(res, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            if (rez.status == "success")
                return rez.notices;
            else
                return null;

        }
    }
}
