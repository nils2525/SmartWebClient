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
        private string _baseUrl;
        private DateTime _lastRequestTime;

        public TimeSpan TimespanBetweenRequests { get; set; }

        public Dictionary<string, string> DefaultQueryParameters { get; }

        public WebClient(string baseURL, bool useCookies = true)
        {
            _baseUrl = baseURL;

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

            while (_lastRequestTime.Add(TimespanBetweenRequests) > DateTime.Now)
            {
                Thread.Sleep(10);
            }

            _lastRequestTime = DateTime.Now;

            HttpResponseMessage getResult;
            try
            {
                getResult = await _client.GetAsync(uri);
            }
            catch (Exception ex)
            {
                Logger.LogToConsole(Logger.LogType.Error, ex.Message);
                return default;
            }

            if (getResult.IsSuccessStatusCode)
            {
                var getResultString = await getResult.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(getResultString);
                    if (result == null)
                    {
                        Logger.LogToConsole(Logger.LogType.Warning, "Can't deserialize the following object ' " + getResultString + " '.");
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    Logger.LogToConsole(Logger.LogType.Warning, "Can't deserialize the following object ' " + getResultString + " '.");
                }
            }

            return default(T);
        }

        private Uri GetRequestUri(string path, List<(string, string)> queryParameters)
        {
            return new Uri(_baseUrl + path + GenerateQueryString(queryParameters));
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
