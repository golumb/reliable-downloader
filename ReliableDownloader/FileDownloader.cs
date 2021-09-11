using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader, IDisposable
    {
        FileProgress fileProgress;
        CancellationTokenSource cts;
        WebSystemCalls web;

        public FileDownloader()
        {
            cts = new CancellationTokenSource();
            web = new WebSystemCalls();
        }

        public async Task<bool> DownloadFileAsync(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            var cancellationToken = cts.Token;
            var bUsePartialDownloader = false;
            Int64? contentLength = -1;

            using var headersResult = await web.GetHeadersAsync(contentFileUrl, cancellationToken);

            if (headersResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Error accessing URL {0}\nHttp status code:{1}\nTime:{2}\nPlease copy this message and report it to the technical staff", contentFileUrl,headersResult.StatusCode,DateTime.UtcNow);
                return false;
            }
            else
            {
                var acceptRanges = headersResult.Headers.AcceptRanges;
                if (acceptRanges != null)
                {
                    acceptRanges.ToList<string>().ForEach(v =>
                    {
                        if (v.Equals("bytes", StringComparison.InvariantCultureIgnoreCase)) bUsePartialDownloader = true;
                    });
                }

                contentLength = headersResult.Content.Headers.ContentLength;

                // Maybe, if content-lenths is unknown, it should be bUsePartialDownloader = false; 
                // TODO: check if we can do partial download without knowing content length

                if (bUsePartialDownloader)
                {
                    Console.WriteLine("Doing partial download. Content-Length:{0}", contentLength);
                }
                else
                {
                    Console.WriteLine("regular...");
                }
            }

            return true;
        }

        public void CancelDownloads()
        {
            cts.Cancel();
        }

        public void Dispose()
        {
            cts?.Dispose();
        }
    }
}