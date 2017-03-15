using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebDownloader
{
    class Domain
    {
        public string name { get; set; }

        public string url { get; set; }

        public int crawlDelay { get; set; }

        public bool isLock { get; set; }

        public bool isAvailable;

        public Dictionary<string, Page> pages = new Dictionary<string, Page>();

        public string protocol
        {
            get
            {
                if (url.StartsWith("https://"))
                    return "https";
                else
                    return "http";
            }
        }

        public Domain(string url)
        {
            this.url = url;
            this.name = GetDomain(url);
            crawlDelay = 0;
            isLock = false;
            isAvailable = IsAvailable();
            pages.Add(url, new Page(url));
        }

        public void AddPages(IEnumerable<string> urls)
        {
            string pattern = ".jpeg|.jpg|.png|.pdf|.css|.js|.ico|.zip|.mailto";
            Match m;
            foreach (var url in urls)
            {
                m = Regex.Match(url, pattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
                if (m.Success)
                {
                    continue;
                }

                string urlForAdd = ClearLink(url);
                if (IsOwnerOfLinks(urlForAdd))
                {
                    if (urlForAdd != String.Empty)
                    {
                        if (!IsFullLink(urlForAdd))
                        {
                            urlForAdd = this.protocol + "://" + this.name + "/" + urlForAdd;
                            urlForAdd = ClearLink(urlForAdd);
                        }
                        if (!pages.ContainsKey(urlForAdd))
                            pages.Add(urlForAdd, new Page(urlForAdd));
                    }
                }

            }
        }

        public List<Page> GetPages(int n)
        {
            return pages.Select(u => u.Value).Where(u => (!u.isDownloaded) && (!u.isStartedDownload)).Take(n).ToList();            
        }

        public bool IsOwnerOfLinks(string url)
        {
            if (IsFullLink(url))
                return (url.StartsWith($"http://{name}") || (url.StartsWith($"https://{name}"))
                        || url.StartsWith($"http://www.{name}") || url.StartsWith($"https://www.{name}"));
            else
                return true;
        }

        public string GetUrlWithoutHttp(string url)
        {
            string result = url.Replace("https://", "");
            result = result.Replace("http://", "");
            return result;
        }
        public bool IsAvailable()
        {
            IPStatus status = IPStatus.Unknown;
            try
            {
                status = new Ping().Send(name).Status;
            }
            catch { }
            if (status == IPStatus.Success)
                return true;
            else
                return false;
        }

        public static string GetDomain(string url)
        {
            url = url.Replace("https://", "");
            url = url.Replace("http://", "");
            if (url.IndexOf("/") > 0)
            {
                url = url.Remove(url.IndexOf("/"), url.Length - url.IndexOf("/"));
            }
            return url;
        }

        public static string ClearLink(string url)
        {
            string resultLink = url.Replace("://", "|||");
            resultLink = resultLink.Replace("//", "/");
            resultLink = resultLink.Replace("|||", "://");
            while ((resultLink.Length > 0) && (resultLink[resultLink.Length - 1] == '/'))
                resultLink = resultLink.Substring(0, resultLink.Length - 1);
            return resultLink;
        }

        public static bool IsFullLink(string url)
        {
            return url.StartsWith("http://") || (url.StartsWith("https://"));
        }
    }
}
