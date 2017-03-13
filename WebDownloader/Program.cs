using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            Console.WriteLine("Введите имя файла со списком доменов \n(Для загрузки из файла по умолчанию (d:\\domains.txt) оставьте строку пустой и нажмите Enter): ");
            string inputPath = Console.ReadLine();
            if (inputPath == string.Empty)
                inputPath = "d:\\domains.txt";
            Console.WriteLine("Укажите путь для выходных данных \n(Чтобы использовать путь по умолчанию (d:\\downloaded_sites\\) оставьте строку пустой и нажмите Enter: ");
            string outputPath= Console.ReadLine();
            if (outputPath == string.Empty)
                outputPath = "d:\\downloaded_sites\\";

            string[] sites = WorkWithFile.GetDomains(inputPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Domain[] domains = new Domain[sites.Count()];
            Console.WriteLine("Выполняется загрузка...");
            for (int i = 0; i < sites.Count(); i++)
                domains[i] = new Domain(sites[i]);

            TaskAsyncManager downloadManager = new TaskAsyncManager(outputPath);
            downloadManager.SetCrawlDelaysAsync(domains).Wait();
            downloadManager.LoadDomains(domains).Wait();
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("-------");
            Console.WriteLine("Загрузка успешно завершена:");
            Console.WriteLine("-входной файл: " + inputPath);
            Console.WriteLine("-выходная директория: " + outputPath);
            Console.WriteLine("-загруженно доменов: " + domains.Count());
            foreach (Domain domain in domains)
                Console.WriteLine($"\t{domain.name}\t{domain.url}\tCrawl-delay: {domain.crawlDelay}");
            Console.WriteLine("-загруженно страниц: " + downloadManager.GetLog().Count);
            Console.WriteLine("-время выполнения: " + (sw.ElapsedMilliseconds / 1000.0).ToString() + "с");

            //Console.WriteLine("Запущенных потоков: " + System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString());
            Console.WriteLine();
            Console.Write("Для завершения работы программы нажмите любую клавишу...");

            Console.ReadKey();
        }
    }
}
