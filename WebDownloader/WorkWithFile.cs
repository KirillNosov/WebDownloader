using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebDownloader
{
    static class WorkWithFile
    {
        public static string[] GetDomains(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Неверно указано имя файла: " + path);
            }

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
                if (!line.StartsWith("http://") && !line.StartsWith("https://"))
                    throw (new Exception("Неверный формат файла!"));
            return lines;
        }

        public static void SavePage(Domain domain, Page page, string path)
        {
            path += domain.name;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filename = Regex.Replace(domain.GetUrlWithoutHttp(page.url), @"[/,? ;><@:]", "_") + ".txt";
            try
            {
                File.WriteAllText(path + "\\" + filename, page.content);
                page.content = null;
            }
            catch (Exception)
            {
               throw new Exception("Неверно указано имя файла: " + path + "\\" + filename);
            }
        }
    }
}
