using System;
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
            await fileDownloader.DownloadFileAsync(exampleUrl, exampleFilePath, progress => { Console.WriteLine($"Percent progress is {progress.ProgressPercent}"); });
        }
    }
}