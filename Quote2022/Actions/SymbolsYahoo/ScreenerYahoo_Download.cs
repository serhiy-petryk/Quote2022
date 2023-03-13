using System;
using System.Net;

namespace Quote2022.Actions.SymbolsYahoo
{
    public static class ScreenerYahoo_Download
    {

        private const string cookie =
            @"F=d=ldj.XLk9vMSCfFykBtUCsDEHnmVP8pGW_1oATGbxvpmP; PH=l=en-US; Y=v=1&n=21udsavh10goo&l=i4h648_f4jh8a/o&p=m1svvua00000000&iz=17500&r=em&intl=us; GUC=AQEACAJj66hkF0IisATW&s=AQAAABrNbqcJ&g=Y-pekg; A1=d=AQABBMq18GACELD3MDowdkkyTRVaqQQ8VqsFEgEACAKo62MXZNwP0iMA_eMBAAcIyrXwYAQ8VqsID1q9S3Bia_-xlDCnFHK0BQkBBwoB5w&S=AQAAAhQWxo_M9Kg0ZaCHt9OtPtM; A3=d=AQABBMq18GACELD3MDowdkkyTRVaqQQ8VqsFEgEACAKo62MXZNwP0iMA_eMBAAcIyrXwYAQ8VqsID1q9S3Bia_-xlDCnFHK0BQkBBwoB5w&S=AQAAAhQWxo_M9Kg0ZaCHt9OtPtM; ucs=tr=1676655154000; OTH=v=1&d=eyJraWQiOiIwMTY0MGY5MDNhMjRlMWMxZjA5N2ViZGEyZDA5YjE5NmM5ZGUzZWQ5IiwiYWxnIjoiUlMyNTYifQ.eyJjdSI6eyJndWlkIjoiWlhMV0VIU0JHVFFWVFZGTURLVDQyVERZSFEiLCJwZXJzaXN0ZW50Ijp0cnVlLCJzaWQiOiJLUm54bE1vb3ViY2YifX0.YOUeF_nkIvUl-7ZTjQuHW4J4oKJ0Xc0eMPFs0Oq0QhXoxClLcMyLQitk8Xiwr40fzu-k5-8CSA13SpKFu2iIZ1sIzK1eP5V-TcxIMUovYByEkZNJfzgo0G9sJgzaDilWJe811fTLHCx_IsX5lAMEKvk1Jjo6J0YxJ_a3CcbUCzo; T=af=JnRzPTE2NzY1Njg3NTQmcHM9VUlWWVVHeFF4MVUzYTVKb2JMNGxlQS0t&d=bnMBeWFob28BZwFaWExXRUhTQkdUUVZUVkZNREtUNDJURFlIUQFhYwFBSGpiVVJEagFhbAFzZXJnZWlfcGV0cmlrAXNjAWRlc2t0b3Bfd2ViAWZzAWlTczhnMnRqa24uRgF6egExdnU0akJEL0gBYQFRQUUBbGF0AUYubmtqQgFudQEw&kt=EAASrz8bER4K73j_fJiuDgk1w--~I&ku=FAArgJCHquStTuR5c2GjeQdioDYlTEzSx6rNm3Oap7lKAoHqaVXLl74kkLQLDe.4NpzYtgrve4Tfk_RwVuFiAzfqlLfs8T9Oepdg2AVM1awV9LESKOaQ2Dw4vcvrSuEKUNU5U.bdyNil9T7VxJjlcrR7DRXjpKIHnsKlhpRC0pt4pE-~E; gpp=DBABBgAA~BVoIgAAQ.QAAA; gpp_sid=8; A1S=d=AQABBMq18GACELD3MDowdkkyTRVaqQQ8VqsFEgEACAKo62MXZNwP0iMA_eMBAAcIyrXwYAQ8VqsID1q9S3Bia_-xlDCnFHK0BQkBBwoB5w&S=AQAAAhQWxo_M9Kg0ZaCHt9OtPtM&j=WORLD; PRF=t%3DSPY%252BMO%252BPSA-PH%252B%255ENYA%252B%255EIXIC%252B%255EDJUSPL%252B%255ERUMIC%252B%255ERUT%252B%255EDJI%252B%255EGSPC%252B%255ESP600%252B%255ERMCC%252BBIT%252BATMP%252BMKC-V%26newChartbetateaser%3D1; cmp=t=1676983287&j=0&u=1YNN";

        private const string postData ="{\"size\":25,\"offset\":50,\"sortField\":\"intradaymarketcap\",\"sortType\":\"DESC\",\"quoteType\":\"EQUITY\",\"topOperator\":\"AND\",\"query\":{\"operator\":\"AND\",\"operands\":[{\"operator\":\"or\",\"operands\":[{\"operator\":\"EQ\",\"operands\":[\"region\",\"us\"]}]}]},\"userId\":\"ZXLWEHSBGTQVTVFMDKT42TDYHQ\",\"userIdType\":\"guid\"}";
        private const string uri = @"https://query1.finance.yahoo.com/v1/finance/screener?crumb=cq3g%2Fl3sJYd&lang=en-US&region=US&formatted=true&corsDomain=finance.yahoo.com";

        public static void Start(Action<string> showStatusAction)
        {
            showStatusAction($"ScreenerYahoo_Download. Started.");

            using (var wc = new Download.WebClientEx())
            {
                wc.Headers.Add(HttpRequestHeader.Cookie, cookie);
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                wc.Headers.Add("Origin", "https://finance.yahoo.com");
                var result = wc.UploadString(uri, postData);
            }

            showStatusAction($"ScreenerYahoo_Download. Finished.");
        }
    }
}
