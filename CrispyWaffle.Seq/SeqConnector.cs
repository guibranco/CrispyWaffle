using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrispyWaffle.Seq
{
    public class SeqConnector
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly string _apiKey;

        public SeqConnector(string serverUrl, string apiKey)
        {
            _httpClient = new HttpClient();
            _serverUrl = serverUrl;
            _apiKey = apiKey;
        }

        public async Task SendLogAsync(string message)
        {
            var content = new StringContent(message, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _serverUrl)
            {
                Content = content
            };
            request.Headers.Add("X-Seq-ApiKey", _apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        // Implement error handling and retries
    }
}
