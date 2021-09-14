using System;
using System.Threading.Tasks;

namespace ReliableDownloader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // If this url 404's, you can get a live one from https://installerstaging.accurx.com/chain/latest.json.

            var exampleUrl = "https://installerstaging.accurx.com/chain/3.94.56148.0/accuRx.Installer.Local.msi";
            var exampleFilePath = "C:/temp/myfirstdownload.msi";

            //var exampleUrl = "https://sabnzbd.org/tests/internetspeed/20MB.bin";
            //var exampleFilePath = "C:/temp/20MB.bin";

            using var fileDownloader = new FileDownloader();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Task download = Task.Run(() => fileDownloader.DownloadFileAsync(
                                            exampleUrl,
                                            exampleFilePath,
                                            progress => {
                                                Console.Write("\rPercent progress is {0}. Time remaining: {1}",
                                                    string.Format("{0:N0}%", progress.ProgressPercent),
                                                    string.Format("{0:N1} seconds", progress.EstimatedRemaining?.TotalSeconds));
                                            }));
            // monitor task:
            await Task.Run(async () =>
                        {
                            Console.WriteLine("\n\nDownload started. Press 'C' to cancel.\n\n");
                              var done = false;
                              while (!done)
                              {
                                  await Task.Delay(1000);
                                  done = download.Status.Equals(TaskStatus.RanToCompletion);
                                  if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.C)
                                      break;
                              }
                            if (done)
                            {
                                stopwatch.Stop();
                                var _seconds = (decimal)stopwatch.ElapsedMilliseconds / 1000;
                                Console.WriteLine($"\n\nDownload completed in {_seconds:0.#} seconds\n");
                            }
                            else
                                fileDownloader.CancelDownloads();
                      });
        }
    }
}