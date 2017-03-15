using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Запущенных потоков: " + System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString());

            Console.WriteLine("WebDownloader\n");
            Console.WriteLine("Введите имя файла со списком доменов \n(Для загрузки из файла по умолчанию (d:\\domains.txt) \nоставьте строку пустой и нажмите Enter): ");
            string inputPath = Console.ReadLine();
            if (inputPath == string.Empty)
                inputPath = "d:\\domains.txt";
            Console.WriteLine("Укажите путь для выходных данных \n(Чтобы использовать путь по умолчанию (d:\\downloaded_sites\\) \nоставьте строку пустой и нажмите Enter: ");
            string outputPath = Console.ReadLine();
            if (outputPath == string.Empty)
                outputPath = "d:\\downloaded_sites\\";

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Domain[] domains;
            try
            {
                domains = CreateDomains(WorkWithFile.GetDomains(inputPath));
                Console.WriteLine("Выполняется загрузка...");
                TaskAsyncManager downloadManager = new TaskAsyncManager(outputPath);
                downloadManager.SetCrawlDelaysAsync(domains).Wait();
                downloadManager.LoadDomains(domains).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Write("Для завершения работы программы нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("-------");
            Console.WriteLine("Загрузка успешно завершена:");
            Console.WriteLine($"-входной файл: {inputPath}");
            Console.WriteLine($"-выходная директория: {outputPath}");
            Console.WriteLine($"-лог загрузки: {outputPath}log.txt");
            Console.WriteLine($"-обнаружено доменов: {domains.Count()}");
            int totalDownLoadedPages = 0;
            int totalPages = 0;
            foreach (Domain domain in domains)
            {
                Console.WriteLine($"\t{domain.name}\tCrawl-delay: { domain.crawlDelay}" +
                                  $"\tЗагружено страниц: {domain.pages.Select(u => u.Value).Where(u => u.isDownloaded).ToList().Count()}/{domain.pages.Count}");
                totalDownLoadedPages += domain.pages.Select(u => u.Value).Where(u => u.isDownloaded).ToList().Count;
                totalPages += domain.pages.Count;
            }
            Console.WriteLine($"-всего загружено страниц: {totalDownLoadedPages}/{totalPages}");
            Console.WriteLine($"-время выполнения: {(sw.ElapsedMilliseconds / 1000.0).ToString()}с");

            //Console.WriteLine("Запущенных потоков: " + System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString());
            Console.WriteLine();
            Console.Write("Для завершения работы программы нажмите любую клавишу...");

            Console.ReadKey();
        }

        static public Domain[] CreateDomains(string[] sites)
        {
            Domain[] domains = new Domain[sites.Count()];
            for (int i = 0; i < sites.Count(); i++)
                domains[i] = new Domain(sites[i]);
            return domains;
        }
    }
}
