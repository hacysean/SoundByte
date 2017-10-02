﻿using System.Net.Http;
using System.Threading.Tasks;

namespace SoundByte.YouTubeParser.Services
{
    /// <summary>
    /// Performs HTTP requests
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Performs a generic HTTP request
        /// </summary>
        Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request);
    }
}
