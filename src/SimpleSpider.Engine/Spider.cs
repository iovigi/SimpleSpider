namespace SimpleSpider.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;

    using HtmlAgilityPack;
    using Nager.PublicSuffix;

    using Configuration;
    using Infrastructure;
    using Models;

    public class Spider
    {
        private const int startTimeInterval = 150;
        private const int endTimeInterval = 3000;
        private const int maxFailTime = 5;

        private readonly Random random = new Random();
        private readonly DomainParser domainParser = new DomainParser(new WebTldRuleProvider());

        private volatile int countOfUrlsToCrawled;
        private string rootUrl;
        private ConcurrentDictionary<string, string> crawledLinks { get; set; }

        public readonly Settings settings;

        public Spider(Settings settings)
        {
            this.settings = settings;
            this.TaskManager = new TaskManager(this.settings.MaxConcurrency);
        }

        public Spider()
            : this(new Settings(1, startTimeInterval, endTimeInterval, maxFailTime))
        {
        }

        public Action<Link> OnNewLink { get; set; }

        public Action OnCrawlCompleteSuccessfully { get; set; }

        private Action<Exception> OnException { get; set; }

        public ITaskManager TaskManager { get; set; }

        public void StartCrawl(string url)
        {
            this.countOfUrlsToCrawled = 0;
            this.rootUrl = url;
            this.crawledLinks = new ConcurrentDictionary<string, string>();
            this.crawledLinks.TryAdd(this.rootUrl, this.rootUrl);

            this.TaskManager.RunTask(this.Crawl, new KeyValuePair<string, int>(this.rootUrl, 0));
        }

        public void StopCrawl()
        {
            this.rootUrl = null;
            this.crawledLinks = null;
            this.TaskManager.StopAll();
        }

        private void Crawl(object state)
        {
            var kv = (KeyValuePair<string, int>)state;

            try
            {
                List<Link> urls = new List<Link>();
                var url = kv.Key;

                this.countOfUrlsToCrawled++;

                var web = new HtmlWeb();
                var doc = web.Load(url);

                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@href]");

                foreach (HtmlNode linkNode in nodes)
                {
                    string hrefValue = linkNode.GetAttributeValue("href", string.Empty);

                    Uri uri;
                    if (hrefValue.StartsWith("/") && Uri.TryCreate(url + hrefValue, UriKind.Absolute, out uri) && this.crawledLinks.TryAdd(uri.AbsoluteUri, uri.AbsoluteUri))
                    {
                        this.OnNewLink?.Invoke(this.GetLink(uri.AbsoluteUri, url, linkNode.InnerText));

                        Wait();
                        this.TaskManager.RunTask(Crawl, new KeyValuePair<string, int>(uri.AbsoluteUri, 0));

                        continue;
                    }

                    if (hrefValue.Contains(rootUrl) && this.crawledLinks.TryAdd(hrefValue, hrefValue))
                    {
                        this.Wait();
                        this.TaskManager.RunTask(Crawl, new KeyValuePair<string, int>(hrefValue, 0));
                    }

                    this.OnNewLink?.Invoke(this.GetLink(hrefValue, url, linkNode.InnerText));
                }

                this.countOfUrlsToCrawled--;

                if (this.countOfUrlsToCrawled == 0)
                {
                    this.OnCrawlCompleteSuccessfully?.Invoke();
                }
            }
            catch (Exception exc)
            {
                this.OnException?.Invoke(exc);

                this.Wait();
                int failTime = kv.Value + 1;

                if(failTime > settings.MaxFailTimesForUrl)
                {
                    return;
                }

                this.TaskManager.RunTask(Crawl, new KeyValuePair<string, int>(kv.Key, failTime));
            }
        }

        private void Wait()
        {
            Thread.Sleep(random.Next(settings.MinTimeIntervalDelay, settings.MaxTimeIntervalDelay));
        }

        private Link GetLink(string toLink, string fromLink, string title)
        {
            var toInfo = this.domainParser.Get(toLink);
            var fromInfo = this.domainParser.Get(fromLink);

            return new Link()
            {
                ToDomain = toInfo.RegistrableDomain,
                TitleToDomain = title,
                ExactLinkToDomain = toLink,
                IsSubDomain = string.IsNullOrWhiteSpace(fromInfo.SubDomain),
                SourceDomainName = fromInfo.RegistrableDomain,
                SourceDomainOrigin = fromLink
            };
        }
    }
}
