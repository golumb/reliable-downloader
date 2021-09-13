using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader, IDisposable
    {
        WebSystemCalls web;
        CancellationTokenSource cts;

        public FileDownloader()
        {
            web = new WebSystemCalls();
            cts = new CancellationTokenSource();
        }

        public async Task<bool> DownloadFileAsync(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            var bUsePartialDownloader = false;
            Int64? contentLength = -1;

            var cancellationToken = cts.Token;
            cancellationToken.Register(() => Console.WriteLine("\n\nDownload cancelled!\n"));

            using var fileStream = new FileStream(localFilePath, FileMode.OpenOrCreate);
            using var headersResult = await web.GetHeadersAsync(contentFileUrl, cancellationToken);

            if (headersResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Error accessing URL {0}\nHttp status code:{1}\nTime:{2}\nPlease copy this message and report it to the technical staff", contentFileUrl, headersResult.StatusCode, DateTime.UtcNow);
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
                    var _contentLength = (long)contentLength;
                    Console.WriteLine("Doing partial download. Content-Length:{0}", _contentLength);
                    var nChunks = 100;  // configurable ?
                    var chunkSize = _contentLength / nChunks;
                    var remainder = _contentLength % nChunks;
                    var dtStart = DateTime.Now;
                    var start = 0L;
                    var end = (long)contentLength;
                    var bWebException = false;
                    byte[] chunkBytes;

                    onProgressChanged(new FileProgress(_contentLength, 0, null, new TimeSpan(0, 1, 0)));

                    for (int i = 0; i <= nChunks; ++i)
                    {
                        chunkBytes = new byte[] { };
                        start = chunkSize * i;
                        if (i < nChunks)
                            end = chunkSize * (i + 1) - 1;
                        else
                            end = _contentLength;
                        var successForThisChunk = false;
                        while (!successForThisChunk)
                        {
                                //using var getResult = await web.DownloadPartialContent(contentFileUrl, start, end, cancellationToken);
                            try
                            {
                                bWebException = false;
                                chunkBytes = await web.DownloadPartialContentWebClient(contentFileUrl, start, end, cancellationToken);
                            }
                            catch(System.Net.WebException wex)
                            {
                                bWebException = true;
                                Console.WriteLine($"DownloadPartialContentWebClient: {wex}");
                            }
                            fileStream.Position = start;
                        
                            //await getResult.Content.CopyToAsync(fileStream);
                            
                            //successForThisChunk = getResult.StatusCode == System.Net.HttpStatusCode.PartialContent;
                            successForThisChunk = !bWebException && chunkBytes.Length>0;

                            if (successForThisChunk)
                            {
                                fileStream.Position = start;
                                await fileStream.WriteAsync(chunkBytes, cancellationToken);
                                successForThisChunk = true;
                            }
                            else 
                            {
                                await Task.Delay(1000); // configurable ?
                                Console.WriteLine("Retrying at {0}", start);
                            }

                        }
                        var dtNow = DateTime.Now;
                        var elapsed = dtNow.Ticks - dtStart.Ticks;
                        var estimateRemaining = elapsed * ((long)contentLength - end) / (long)contentLength;
                        onProgressChanged(new FileProgress(contentLength, end, ((double)100 * end / contentLength), new TimeSpan(estimateRemaining)));
                    }

                }
                else
                {
                    Console.WriteLine("Doing regular download");
                    var success = false;
                    while (!success)
                    {
                        using var getResult = await web.DownloadContent(contentFileUrl, cancellationToken);
                        success = getResult.StatusCode == System.Net.HttpStatusCode.OK;
                        if (success)
                        {
                            fileStream.Position = 0;
                            await getResult.Content.CopyToAsync(fileStream);
                        }
                    }
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
