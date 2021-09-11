using System;
using System.Threading;
using System.Threading.Tasks;
using ReliableDownloader;

namespace ReliableDownloader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // If this url 404's, you can get a live one from https://installerstaging.accurx.com/chain/latest.json.
            var exampleUrl = "https://installerstaging.accurx.com/chain/3.94.56148.0/accuRx.Installer.Local.msi";
            var exampleFilePath = "C:/temp/myfirstdownload.msi";
            using var fileDownloader = new FileDownloader();

            

            Task download = Task.Run(() => fileDownloader.DownloadFileAsync(
                                            exampleUrl, 
                                            exampleFilePath, 
                                            progress => { Console.Write($"\rPercent progress is {string.Format("{0:N0}%", progress.ProgressPercent)}"); }
                                            ));
            // monitor thread:
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
                                Console.WriteLine("\n\nDownload completed\n\n");                        
                              else
                                fileDownloader.CancelDownloads();
                      });
        }
    }
}