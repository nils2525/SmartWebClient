using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SmartWebClient
{
    public class WebClient
    {
        private HttpClient _client;
        private HttpClientHandler _clientHandler;
        private const string BaseUrl = "https://market.csgo.com/";
        private DateTime _lastRequestTime;

        public TimeSpan TimespanBetweenRequests { get; set; }

        public Dictionary<string, string> DefaultQueryParameters { get; }

        public WebClient(bool useCookies = true)
        {
            _clientHandler = new HttpClientHandler()
            {
                UseCookies = useCookies
            };

            _client = new HttpClient(_clientHandler);

            DefaultQueryParameters = new Dictionary<string, string>();
        }

        public async Task<T> GetObjectAsync<T>(string path, string queryKey, string queryValue)
        {
            return await GetObjectAsync<T>(path, new List<(string, string)>() { (queryKey, queryValue) });
        }

        public async Task<T> GetObjectAsync<T>(string path, List<(string, string)> queryParameters = null)
        {
            var uri = GetRequestUri(path, queryParameters);

            if (_lastRequestTime.Add(TimespanBetweenRequests) > DateTime.Now)
            {
                //Console.WriteLine("Wait until time between requests is reached");
            }
            while (_lastRequestTime.Add(TimespanBetweenRequests) > DateTime.Now)
            {
                //Console.Write(".");
                Thread.Sleep(250);
            }

            _lastRequestTime = DateTime.Now;
            var getResult = await _client.GetAsync(uri);



            if (getResult.IsSuccessStatusCode)
            {
                var getResultString = await getResult.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(getResultString);
                    if(result == null)
                    {
                        Console.WriteLine(getResultString);
                    }

                    return result;
                }
                catch (Exception)
                {
                    Console.WriteLine(getResultString);
                }
            }

            return default(T);
        }

        private Uri GetRequestUri(string path, List<(string, string)> queryParameters)
        {
            return new Uri(BaseUrl + path + GenerateQueryString(queryParameters));
        }
        private string GenerateQueryString(List<(string, string)> queryParameters)
        {
            var resultString = "?";

            if (queryParameters != null)
            {
                foreach (var parameter in queryParameters)
                {
                    resultString += parameter.Item1 + "=" + parameter.Item2 + "&";
                }
            }

            foreach (var parameter in DefaultQueryParameters)
            {
                resultString += parameter.Key + "=" + parameter.Value + "&";
            }

            return resultString;
        }
    }
}
