using System;
using System.Collections.Generic;
using System.Net;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// Generic response wrapper returned by the wrapper methods.
    /// </summary>
    public class HttpResponse<T>
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public T? Data { get; set; }
        public string? RawContent { get; set; }
        public IList<string>? Errors { get; set; }
        public IDictionary<string, IEnumerable<string>>? Headers { get; set; }
        public TimeSpan Duration { get; set; }

        public static HttpResponse<T> Success(T? data, HttpStatusCode statusCode, string? rawContent, IDictionary<string, IEnumerable<string>>? headers, TimeSpan duration)
        {
            return new HttpResponse<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Data = data,
                RawContent = rawContent,
                Headers = headers,
                Duration = duration
            };
        }

        public static HttpResponse<T> Failure(HttpStatusCode? statusCode, string? rawContent, IList<string>? errors, IDictionary<string, IEnumerable<string>>? headers, TimeSpan duration)
        {
            return new HttpResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                RawContent = rawContent,
                Errors = errors,
                Headers = headers,
                Duration = duration
            };
        }
    }
}