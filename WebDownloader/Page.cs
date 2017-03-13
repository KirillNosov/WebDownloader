using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebDownloader
{
    class Page
    {
        public string url { get; set; }
        public string content { get; set; }
        public bool isDownloaded { get; set; }
        public bool isStartedDownload{ get; set; }

        public Page(string url)
        {
            this.url = url;
            content = null;
            isDownloaded = false;
            isStartedDownload = false;
        }

        public IEnumerable<String> GetLinks()
        {
            List<String> links = new List<string>();
            Match m;
            string HRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

            try
            {
                m = Regex.Match(this.content, HRefPattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
                while (m.Success)
                {

                    links.Add(m.Groups[1].Value);
                    m = m.NextMatch();
                }
            }

            catch (RegexMatchTimeoutException)
            {
                Console.WriteLine("The matching operation timed out.");
            }
            return (links);
        }
    }
}
