using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebDownloader
{
    class TaskAsyncManager
    {
        public static string outPath;

        public TaskAsyncManager(string path)
        {
            outPath = path;
        }

        public static List<string> log = new List<string>();

        public async Task LoadDomains(Domain[] domains)
        {
            List<Task> tasks = new List<Task>();
            int pages;
            do
            {
                pages = 0;
                foreach (var domain in domains)
                {
                    if (!domain.IsAvailable())
                        continue;
                    tasks.Add(LoadDomain(domain));
                    pages += domain.GetPages(1).Count();
                }
                await Task.WhenAll(tasks);
            }
            while (pages > 0);
            WorkWithFile.saveLog(log, outPath);
        }

        public async Task LoadDomain(Domain domain)
        {
            List<Task> tasks = new List<Task>();
            List<Page> pages;
            pages = domain.GetPages(20);
            while (pages.Count > 0)
            {
                foreach (var page in pages)
                {
                    tasks.Add(LoadPageAsync(domain, page));
                }
                await Task.WhenAll(tasks);
                pages = domain.GetPages(20);
            }
        }

        async Task LoadPageAsync(Domain domain, Page page)
        {
            page.isStartedDownload = true;
            var httpClient = new HttpClient();
            HttpResponseMessage response;
            if (domain.crawlDelay > 0)
            {
                while (domain.isLock)
                {
                    Thread.Sleep(100);
                }
                Lock(domain);
            }

            response = await httpClient.GetAsync(page.url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(page.url);
                log.Add($"{DateTime.Now.ToString()}\t OK\t {page.url}");
                page.content = await response.Content.ReadAsStringAsync();
                page.isDownloaded = true;
                domain.AddPages(page.GetLinks());
                WorkWithFile.SavePage(domain, page, outPath);
            }
            else
                log.Add($"{DateTime.Now.ToString()}\tERR\t {page.url}  !Ошибка: {response.StatusCode.ToString()}");
        }

        public async Task SetCrawlDelaysAsync(Domain[] domains)
        {
            List<Task> tasks = new List<Task>();
            foreach (var domain in domains)
            {
                if (domain.IsAvailable())
                    tasks.Add(LoadCrawlDelayFromRobotsTXTAsync(domain));
                else
                    log.Add($"{DateTime.Now.ToString()}\tERR\t {domain.url} !Сервер недоступен");
            }
            await Task.WhenAll(tasks);
        }

        async Task LoadCrawlDelayFromRobotsTXTAsync(Domain domain)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response;
            string url = $"{domain.url}/robots.txt";
            response = await httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();

            //if (domain.url == "http://stampard.ru/")
            //    content = "skfhskhf khs kshf ksh 9 khskf hksh khsfk hksf hkshf k Crawl-delay sfshkh 1\nltjeote";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string pattern = @"\bcrawl-delay\w*\b";
                Match m = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    content = content.Substring(m.Index, content.Length - m.Index);
                    pattern = @"(-|)\d+(((\.|,)\d+|)+e(\+|-)\d+|(\.|,)\d+|)";
                    m = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        domain.crawlDelay = (int)(1000 * double.Parse(m.Value.Replace(',', '.'), CultureInfo.InvariantCulture));
                        if (domain.crawlDelay > 0)
                            Lock(domain);
                    }
                }
            }
            else
                log.Add($"{DateTime.Now.ToString()}\tERR\t {url}  Ошибка: {response.StatusCode.ToString()}");
        }

        async Task Lock(Domain domain)
        {
            //Console.BackgroundColor = ConsoleColor.Red;
            //Console.WriteLine("Lock");
            //Console.ResetColor();
            domain.isLock = true;
            await Task.Run(() => Thread.Sleep(domain.crawlDelay));
            //Thread.Sleep(domain.crawlDelay);
            domain.isLock = false;
            //Console.BackgroundColor = ConsoleColor.Green;
            //Console.WriteLine("UnLock");
            //Console.ResetColor();
        }
    }
}
