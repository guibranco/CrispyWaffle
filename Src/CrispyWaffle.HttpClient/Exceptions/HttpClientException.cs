using System;
using System.Net;

namespace CrispyWaffle.HttpClient.Exceptions
{
    public class HttpClientException : Exception
    {
        public HttpStatusCode? StatusCode { get; }
        public string? ResponseContent { get; }

        public HttpClientException(
            string message,
            HttpStatusCode? statusCode = null,
            string? responseContent = null,
            Exception? inner = null
        )
            : base(message, inner)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}
