using IAViewer.WebCrawl.Poco;
using System;

namespace IAViewer.WebCrawl.Crawler
{
    public class CrawlArgs : EventArgs
    {
        public CrawlContext CrawlContext { get; set; }

        public CrawlArgs(CrawlContext crawlContext)
        {
            if (crawlContext == null)
                throw new ArgumentNullException("crawlContext");

            CrawlContext = crawlContext;
        }
    }
}
