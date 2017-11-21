﻿/* |----------------------------------------------------------------|
 * | Copyright (c) 2017, Grid Entertainment                         |
 * | All Rights Reserved                                            |
 * |                                                                |
 * | This source code is to only be used for educational            |
 * | purposes. Distribution of SoundByte source code in             |
 * | any form outside this repository is forbidden. If you          |
 * | would like to contribute to the SoundByte source code, you     |
 * | are welcome.                                                   |
 * |----------------------------------------------------------------|
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SoundByte.Core.Services
{
    /// <summary>
    /// Simple HTTP service that uses <see cref="HttpClient"/> for handling requests
    /// </summary>
    public class HttpService : IHttpService, IDisposable
    {
        private readonly HttpClient _httpClient;

        private static HttpService _instance;

        /// <summary>
        /// Creates an instance of <see cref="HttpService"/> with a custom <see cref="HttpClient"/>
        /// </summary>
        public HttpService(HttpClient client)
        {
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpService"/>
        /// </summary>
        public HttpService()
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpClientHandler.UseCookies = false;

            _httpClient = new HttpClient(httpClientHandler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeExplode (github.com/Tyrrrz/YoutubeExplode)");
        }

        /// <inheritdoc />
        ~HttpService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request)
        {
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a reusable instance of HttpService
        /// </summary>
        public static HttpService Instance => _instance ?? (_instance = new HttpService());
    }
}
