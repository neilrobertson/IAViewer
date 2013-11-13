using IAViewer.WebCrawl.Poco;
using System;

namespace IAViewer.WebCrawl.Crawler
{
    public class PageCrawlStartingArgs : CrawlArgs
    {
        public PageToCrawl PageToCrawl { get; private set; }

        public PageCrawlStartingArgs(CrawlContext crawlContext, PageToCrawl pageToCrawl)
            : base(crawlContext)
        {
            if (pageToCrawl == null)
                throw new ArgumentNullException("pageToCrawl");

            PageToCrawl = pageToCrawl;
        }
    }
}
