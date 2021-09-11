using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.IO;

namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader, IDisposable
    {
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

            using var fileStream = new FileStream(localFilePath, FileMode.OpenOrCreate);
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
                    var nChunks = 100;  // configurable ?
                    var chunkSize = (long)contentLength / nChunks;
                    var remainder = (long)contentLength % nChunks;
                    var dtStart = DateTime.Now;
                    var start = 0L;
                    var end = (long)contentLength;

                    onProgressChanged(new FileProgress(contentLength, 0, null, new TimeSpan(0, 1, 0)));
                    for (int i = 0; i <= nChunks; ++i)
                    {
                        start = chunkSize * i;
                        if (i<nChunks)
                            end = chunkSize * (i + 1) - 1;
                        else
                            end = start + remainder;
                        using var getResult = await web.DownloadPartialContent(contentFileUrl, start, end, cancellationToken);
                        fileStream.Position = start;
                        await getResult.Content.CopyToAsync(fileStream);
                        var dtNow = DateTime.Now;
                        var elapsed = dtNow.Ticks - dtStart.Ticks;
                        var estimateRemaining = elapsed * ((long)contentLength - end) / (long)contentLength;
                        onProgressChanged(new FileProgress(contentLength, end, ( (double)100 * end / contentLength), new TimeSpan(estimateRemaining)));
                    }

                }
                else
                {
                    Console.WriteLine("Doing regular download");
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