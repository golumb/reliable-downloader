using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ReliableDownloader
{
    public class WebSystemCalls : IWebSystemCalls
    {
        private static readonly HttpClient _client = new HttpClient();

        public async Task<HttpResponseMessage> GetHeadersAsync(string url, CancellationToken token)
        {
            return await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url), token).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<HttpResponseMessage> DownloadContent(string url, CancellationToken token)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return await _client.SendAsync(httpRequestMessage, token).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<HttpResponseMessage> DownloadPartialContent(string url, long from, long to, CancellationToken token)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                httpRequestMessage.Headers.Range = new RangeHeaderValue(from, to);
                return await _client.SendAsync(httpRequestMessage, token).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<byte[]> DownloadPartialContentWebClient(string url, long from, long to, CancellationToken token)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add($"Range: bytes={from}-{to}");
                try
                {
                    return await client.DownloadDataTaskAsync(url);
                }
                catch (WebException wex)
                {
                    throw wex;
                }
                catch (Exception ex)
                {
                    // log this
                    return new byte[] { };
                }
            }
        }
    }
}