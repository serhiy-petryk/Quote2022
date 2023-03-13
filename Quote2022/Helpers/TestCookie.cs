using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Quote2022.Actions;

namespace Quote2022.Helpers
{
    public static class TestCookie
    {
        private static string GetChromeCookiePath()
        {
            string s = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            s += @"\Google\Chrome\User Data\Default\NetWork\cookies";

            if (!File.Exists(s))
                return string.Empty;
            return s;
        }


        public static void Test3()
        {
            // https://www.youtube.com/watch?v=vvnGGR2NuSg
            // https://stackoverflow.com/questions/24047326/null-values-when-reading-from-chrome-sqlite-cookie-database
            // https://www.codeproject.com/Articles/330142/Cookie-Quest-A-Quest-to-Read-Cookies-from-Four-Pop
            // c# read cookies from chrome sqlite

            var aa = GetChromeCookiePath();

            /*using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                StringBuilder query = new StringBuilder();
                //chrome
                query.Append("SELECT value FROM cookies WHERE host_key LIKE '%.search.cpan.org%' AND name LIKE '%__utmc%';");
                //mozilla
                //query.Append("SELECT value FROM moz_cookies WHERE host LIKE '%.oasgames.com%' AND name LIKE '%__utmz%';");
                //query.Append("ORDER BY name");
                using (SQLiteCommand cmd = new SQLiteCommand(query.ToString(), conn))
                {
                    conn.Open();
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            textBox1.Text = dr.GetValue(0).ToString();
                            // textBox1.Text = dr.GetValue(0) + " " + dr.GetValue(1) + " " + dr.GetValue(2) + " " + dr.GetValue(3) + " " + dr.GetValue(4) + " " + dr.GetValue(5);
                        }
                    }
                }
            }*/
        }

        public static void Test()
        {
            // GET https://www.eoddata.com/Data/symbollist.aspx?e=NYSE HTTP/1.1

            //var request = (HttpWebRequest)WebRequest.Create(https://eoddata.com/symbols.aspx);

            // request.Method = "POST";
            /*request.ContentType = "application/x-www-form-urlencoded";

            var query = string.Join("&",
                loginData.Cast<string>().Select(key => $"{key}={loginData[key]}"));

            var buffer = Encoding.ASCII.GetBytes(query);
            request.ContentLength = buffer.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();*/

            /*container = request.CookieContainer = new CookieContainer();

            var response = request.GetResponse();
            response.Close();
            CookieContainer = container;*/

            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                    // wc.Headers.Add("Cache-Control", "no-cache");
//                    if (isXMLHttpRequest)
  //                      wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    var uri = new Uri("https://eoddata.com/symbols.aspx");
                    wc.Headers.Add(HttpRequestHeader.Referer, uri.Host);
                    var bb = wc.DownloadData(uri);
                    var response = Encoding.UTF8.GetString(bb);
                // return response;

                uri = new Uri("https://www.eoddata.com/Data/symbollist.aspx?e=NYSE");
                var bb2 = wc.DownloadData(uri);
                var response2 = Encoding.UTF8.GetString(bb);


            }
            //var a1 = GetResponse("https://eoddata.com/symbols.aspx");
            // var a2 = GetResponse("https://www.eoddata.com/Data/symbollist.aspx?e=NYSE");
        }

        public class WebClientEx : WebClient
        {
            public CookieContainer CC;
            public WebResponse Response;
            public int? TimeoutInMilliseconds;

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                Response = base.GetWebResponse(request);
                return Response;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                if (CC == null)
                    CC = new CookieContainer();
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
                request.AllowAutoRedirect = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.CookieContainer = CC;

                if (TimeoutInMilliseconds.HasValue)
                    request.Timeout = TimeoutInMilliseconds.Value;
                return request;
            }
        }

        public static string GetResponse(string url, bool isXMLHttpRequest = false)
        {
            string response = null;
            using (var wc = new Download.WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    // wc.Headers.Add("Cache-Control", "no-cache");
                    if (isXMLHttpRequest)
                        wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    var uri = new Uri(url);
                    wc.Headers.Add(HttpRequestHeader.Referer, uri.Host);
                    var bb = wc.DownloadData(url);
                    response = Encoding.UTF8.GetString(bb);
                    return response;
                }
                catch (Exception ex)
                {
                    /*if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
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
                    else*/
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                    }
                    else
                        throw ex;
                }
            }

            return response;
        }


    }
}
