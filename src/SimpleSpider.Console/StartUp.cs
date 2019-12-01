namespace SimpleSpider.Console
{
    using System;

    using Engine;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            Spider spider = new Spider();

            spider.OnNewLink = x =>
             {
                 Console.WriteLine(x.ExactLinkToDomain);
             };

            spider.StartCrawl("http://abv.bg");

            Console.ReadKey();
        }
    }
}
