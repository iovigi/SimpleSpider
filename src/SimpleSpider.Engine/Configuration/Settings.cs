namespace SimpleSpider.Engine.Configuration
{
    public class Settings
    {
        public Settings(int maxConcurrency, int minTimeIntervalDeplay,int maxTimeIntervalDelay, int maxFailTimesForUrl)
        {
            this.MaxConcurrency = maxConcurrency;
            this.MinTimeIntervalDelay = minTimeIntervalDeplay;
            this.MaxTimeIntervalDelay = maxTimeIntervalDelay;
            this.MaxFailTimesForUrl = maxFailTimesForUrl;
        }

        public int MaxConcurrency { get; private set; }
        public int MinTimeIntervalDelay { get; set; }
        public int MaxTimeIntervalDelay { get; set; }
        public int MaxFailTimesForUrl { get; set; }
    }
}
