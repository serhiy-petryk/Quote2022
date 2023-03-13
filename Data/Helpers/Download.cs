using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Data.Helpers
{
    public static class Download
    {
        public static string DownloadPage(string url, string filename, bool isXMLHttpRequest = false)
        {
            string response = null;
            using (var wc = new WebClientEx())
            {
                /*if (ServicePointManager.DefaultConnectionLimit != int.MaxValue)
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    WebRequest.DefaultWebProxy = null;
                }
                wc.Proxy = null;*/
                wc.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    // wc.Headers.Add("Cache-Control", "no-cache");
                    if (isXMLHttpRequest)
                        wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    var bb = wc.DownloadData(url);
                    response = Encoding.UTF8.GetString(bb);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);
                        var folder = Path.GetDirectoryName(filename);
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        File.WriteAllText(filename, response, Encoding.UTF8);
                        //                File.WriteAllText(filename, response);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                    }
                    else
                        throw ex;
                }
            }

            return response;
        }

        public class WebClientEx : WebClient
        {
            public int? TimeoutInMilliseconds;
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
                request.AllowAutoRedirect = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (TimeoutInMilliseconds.HasValue)
                    request.Timeout = TimeoutInMilliseconds.Value;
                return request;
            }
        }

        public static string DownloadPage_POST(string url, string filename, object parameters)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            string response = null;
            using (var wc = new WebClient())
            {
                if (ServicePointManager.DefaultConnectionLimit != int.MaxValue)
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    WebRequest.DefaultWebProxy = null;
                }
                wc.Proxy = null;

                try
                {
                    if (parameters is NameValueCollection nvc)
                        response = Encoding.UTF8.GetString(wc.UploadValues(url, "POST", nvc));
                    else if (parameters is string json)
                        response = wc.UploadString(url, "POST", json);
                    else
                        throw new Exception("DownloadPage_POST. Invalid type of request parameters");
                }
                catch (Exception ex)
                {
                    if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else
                        throw ex;
                }
            }

            if (!string.IsNullOrEmpty(filename))
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                File.WriteAllText(filename, response, Encoding.UTF8);
            }

            return response;
        }


    }
}
