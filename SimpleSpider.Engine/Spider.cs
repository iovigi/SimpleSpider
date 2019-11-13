﻿namespace SimpleSpider.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;

    using HtmlAgilityPack;

    using Configuration;
    using Infrastructure;
    using Models;

    public class Spider
    {
        private const int startTimeInterval = 150;
        private const int endTimeInterval = 3000;
        private readonly Random random = new Random();

        private volatile int countOfUrlsToCrawled;
        private string rootUrl;
        private ConcurrentDictionary<string,string> crawledLinks { get; set; }

        public readonly Settings settings;

        public Spider(Settings settings)
        {
            this.settings = settings;
            this.TaskManager = new TaskManager(this.settings.MaxConcurrency);
        }

        public Spider()
            : this(new Settings(1))
        {
        }

        public Action<KeyValuePair<string, List<Link>>> OnPageComplete { get; set; }

        public Action OnCrawlCompleteSuccessfully { get; set; }

        public ITaskManager TaskManager { get; set; }

        public void StartCrawl(string url)
        {
            this.countOfUrlsToCrawled = 0;
            this.rootUrl = url;
            this.crawledLinks = new ConcurrentDictionary<string, string>();
            this.crawledLinks.TryAdd(this.rootUrl, this.rootUrl);

            this.TaskManager.RunTask(this.Crawl, this.rootUrl);
        }

        public void StopCrawl()
        {
            this.rootUrl = null;
            this.crawledLinks = null;
            this.TaskManager.StopAll();
        }

        private void Crawl(object state)
        {
            List<Link> urls = new List<Link>();
            var url = (string)state;
            this.countOfUrlsToCrawled++;

            var web = new HtmlWeb();
            var doc = web.Load(url);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@href]");

            foreach (HtmlNode linkNode in nodes)
            {
                // Get the value of the HREF attribute
                string hrefValue = linkNode.GetAttributeValue("href", string.Empty);

                Uri uri;
                if (hrefValue.StartsWith("/") && Uri.TryCreate(url + hrefValue, UriKind.Absolute, out uri) && this.crawledLinks.TryAdd(uri.AbsoluteUri, uri.AbsoluteUri))
                {
                    urls.Add(new Link() { Url = uri.AbsoluteUri, Title = linkNode.InnerText });
                    Wait();
                    this.TaskManager.RunTask(Crawl, uri.AbsoluteUri);

                    continue;
                }

                if(hrefValue.Contains(rootUrl) && this.crawledLinks.TryAdd(hrefValue, hrefValue))
                {
                    Wait();
                    this.TaskManager.RunTask(Crawl, hrefValue);
                }

                urls.Add(new Link() { Url = hrefValue, Title = linkNode.InnerText });
            }

            this.OnPageComplete?.Invoke(new KeyValuePair<string, List<Link>>(url, urls));
            this.countOfUrlsToCrawled--;

            if(this.countOfUrlsToCrawled == 0)
            {
                this.OnCrawlCompleteSuccessfully?.Invoke();
            }
        }

        private void Wait()
        {
            Thread.Sleep(random.Next(startTimeInterval, endTimeInterval));
        }
    }
}
