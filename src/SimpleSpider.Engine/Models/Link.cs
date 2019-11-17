namespace SimpleSpider.Engine.Models
{
    public class Link
    {
        public string ToDomain { get; set; }
        public string TitleToDomain { get; set; }
        public string ExactLinkToDomain { get; set; }
        public bool IsSubDomain { get; set; }
        public string SourceDomainName { get; set; }
        public string SourceDomainOrigin { get; set; }
    }
}
