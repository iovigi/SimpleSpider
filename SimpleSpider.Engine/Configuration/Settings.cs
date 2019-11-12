namespace SimpleSpider.Engine.Configuration
{
    public class Settings
    {
        public Settings(int maxConcurrency)
        {
            this.MaxConcurrency = maxConcurrency;
        }

        public int MaxConcurrency { get; private set; }
    }
}
