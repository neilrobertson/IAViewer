using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IAViewer.WebCrawl.Core;
using IAViewer.WebCrawl.Crawler;
using IAViewer.WebCrawl.Poco;
using System.Net;
using log4net.Config;
using IAViewer.DB;

namespace IAViewer
{
    public class Controller
    {
        protected static Controller uniqueInstance;
        protected static object syncLocker = new object();

        protected DBConnectionPool _databaseConnectionPool;

        protected String projectID;

        private Controller()
        {

        }

        public static Controller GetInstance()
        {
            lock (syncLocker)
            {
                if (uniqueInstance == null)
                    uniqueInstance = new Controller();
                return uniqueInstance;
            }
        }

        public void RunWebCrawler(String rootURI, String[] uriContains)
        {
            if (rootURI == null || rootURI == "")
                throw new ArgumentNullException();

            Uri uri = new Uri(rootURI);
            
            CrawlConfiguration crawlConfig = AbotConfigurationSectionHandler.LoadFromXml().Convert();
            crawlConfig.CrawlTimeoutSeconds = 100;
            crawlConfig.MaxConcurrentThreads = 10;
            crawlConfig.MaxPagesToCrawl = 1000;

            XmlConfigurator.Configure();

            PoliteWebCrawler webCrawler = new PoliteWebCrawler();

            webCrawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            webCrawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            webCrawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            webCrawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;
            
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            dbConfig = DBConfigurationSectionHandler.LoadFromXml().Convert();
            
            IDatabase database = DBFactory.GetDatabase(dbConfig.DatabaseType);
            Func<IDatabase> databaseGenerator = database.GetDatabaseGenerator();
            _databaseConnectionPool = new DBConnectionPool(databaseGenerator);

            CreateProject(rootURI);

            if (uriContains != null)
            {
                foreach (String uriContent in uriContains)
                {
                    //webCrawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
                    //{
                    //    CrawlDecision decision = new CrawlDecision();
                    //    if (pageToCrawl.Uri.Authority == uriContent)
                    //        return new CrawlDecision { Allow = true, Reason = "Include all uri parts" };
                    //
                    //    return decision;
                    //});
                }
            }

            CrawlResult result = webCrawler.Crawl(uri);
            
            if (result.ErrorOccurred)
                Console.WriteLine("Crawl of {0} completed with ERROR!!!", result.RootUri.AbsoluteUri);
            else
                Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

            _databaseConnectionPool.CloseAllConnections();
            _databaseConnectionPool = null;
        }

        private void CreateProject(String rootURI)
        {
            projectID = System.Guid.NewGuid().ToString();
            IDatabase database = _databaseConnectionPool.GetObject();
            try
            {
                String command = "INSERT INTO Project (ProjectID, RootURL) VALUES ('" + projectID + "', '" + rootURI + "');";
                database.ExecuteNonQuery(command);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                _databaseConnectionPool.PutObject(database);
            }
        }

        private void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            //Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed {0} StatusCode: [{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.StatusCode);
            else
            {
                IDatabase database = _databaseConnectionPool.GetObject();
                try
                {
                    String command = "INSERT INTO [CrawledPage] ([PageGUID], [ProjectGUID], [URL], [StatusCode], [PageSize], [HttpResponse], [CrawlDepth], [ParentURI]) VALUES (@PageGUID, @ProjectGUID, @URL, @StatusCode, @PageSize, @HttpResponse, @CrawlDepth, @ParentURI);";

                    Dictionary<String, Object> parameters = new Dictionary<String, Object>();
                    parameters.Add("@PageGUID", crawledPage.PageGUID);
                    parameters.Add("@ProjectGUID", projectID);
                    parameters.Add("@URL", crawledPage.Uri.AbsoluteUri);
                    parameters.Add("@StatusCode", crawledPage.StatusCode);
                    parameters.Add("@PageSize", crawledPage.PageSizeInBytes);
                    parameters.Add("@HttpResponse", crawledPage.HttpWebResponse.Headers.ToString());
                    parameters.Add("@CrawlDepth", crawledPage.CrawlDepth);
                    parameters.Add("@ParentURI", crawledPage.ParentUri.AbsoluteUri);

                    database.ExecuteNonQueryWithParameters(command, parameters);
                    
                    if (string.IsNullOrEmpty(crawledPage.RawContent) == false)
                    {
                        String contentCommand = "INSERT INTO [PageContent] ([PageGUID], [ProjectGUID], [URL], [RawContent]) VALUES (@pageGUID, @projectGUID, @url, @rawContent);";
                        Dictionary<String, Object> contentParams = new Dictionary<String, Object>();
                        contentParams.Add("@pageGUID", crawledPage.PageGUID);
                        contentParams.Add("@projectGUID", projectID);
                        contentParams.Add("@url", crawledPage.Uri.AbsoluteUri);
                        contentParams.Add("@rawContent", crawledPage.RawContent);
                        
                        database.ExecuteNonQueryWithParameters(contentCommand, contentParams);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
                finally
                {
                    _databaseConnectionPool.PutObject(database);
                }
            }
            Console.WriteLine("Crawl of page succeeded {0} StatusCode: [{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.StatusCode);
        }

        private void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

    }
}
