using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public static class HttpUtils
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string key in nvc.Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;

                string[] values = nvc.GetValues(key);
                if (values == null) continue;

                foreach (string value in values)
                {
                    sb.Append(sb.Length == 0 ? "?" : "&");
                    sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));
                }
            }

            return sb.ToString();
        }

        public static async Task<bool> CheckIfServiceIsAvailableAsync(string _url, string _method = "GET", int _counter = 0)
        {
            int chances = _counter;
            bool available = false;
            try
            {                
                HttpWebRequest request = HttpWebRequest.CreateHttp(_url);
                request.Method = _method;
                //request.Credentials = CredentialCache.DefaultCredentials;
                request.AllowAutoRedirect = false;                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        available = true;
                }
                return available;
            }
            catch(WebException)
            {
                if (chances == 0)
                    return available;
                await Task.Delay(TimeSpan.FromSeconds(1));
                chances--;
                return await CheckIfServiceIsAvailableAsync(_url, _method, chances);
            }
        }

        public static bool CheckIfServiceIsAvailable(string _url, string _method = "GET")
        {
            bool available = false;
            try
            {                
                HttpWebRequest request = HttpWebRequest.CreateHttp(_url);
                request.Method = _method;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.AllowAutoRedirect = false;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        available = true;
                }
                return available;
            }
            catch (WebException)
            {
                return available;
            }
        }
    }
}
