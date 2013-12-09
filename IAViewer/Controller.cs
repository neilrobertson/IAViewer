using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IAViewer.WebCrawl.Core;
using IAViewer.WebCrawl.Crawler;
using IAViewer.WebCrawl.Poco;
using System.Net;
using log4net.Config;
using IAViewer.DB;
using log4net;

namespace IAViewer
{
    public class Controller
    {
        protected static Controller uniqueInstance;
        protected static object syncLocker = new object();

        protected DBConnectionPool _databaseConnectionPool;

        protected string _projectGUID;
        static ILog _logger = LogManager.GetLogger(typeof(Controller).FullName);

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




        public void RunWebCrawler(string rootURI, string[] uriContains)
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

            string userGUID = Guid.NewGuid().ToString();
            CreateProject(rootURI, userGUID);

            if (uriContains != null)
            {
                foreach (string uriContent in uriContains)
                {
                    //webCrawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
                    //{
                    //    CrawlDecision decision = new CrawlDecision();
                    //    Match match = Regex.Match(pageToCrawl.Uri.ToString(), uriContent, RegexOptions.IgnoreCase);
                    //    if (!match.Success)
                    //        return new CrawlDecision { Allow = false, Reason = "Include all uri parts" };
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




        private void CreateProject(string rootURI, string userGUID)
        { 
            _projectGUID = System.Guid.NewGuid().ToString();
            IDatabase database = _databaseConnectionPool.GetObject();
            try
            {
                string command = "INSERT INTO [Projects] (projectGUID, userGUID, rootURL) VALUES (@projectGUID, @userGUID, @rootURL);";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@projectGUID", _projectGUID);
                parameters.Add("@userGUID", userGUID);
                parameters.Add("@rootURL", rootURI);
                database.ExecuteNonQueryWithParameters(command, parameters);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString()); 
                _logger.Error("An unhandled exception was thrown inserting project details to DB");
                _logger.Error(exception);
            }
            finally
            {
                _databaseConnectionPool.PutObject(database);
            }
        }




        private void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            _logger.InfoFormat("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }




        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                _logger.Error(String.Format("Crawl of page failed {0} StatusCode: [{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.StatusCode));
            else
            {
                IDatabase database = _databaseConnectionPool.GetObject();
                try
                {
                    string command = "INSERT INTO [CrawledPage] ([PageGUID], [ProjectGUID], [URL], [StatusCode], [PageSize], [HttpResponse], [CrawlDepth], [ParentURI]) VALUES (@PageGUID, @ProjectGUID, @URL, @StatusCode, @PageSize, @HttpResponse, @CrawlDepth, @ParentURI);";

                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("@PageGUID", crawledPage.PageGUID);
                    parameters.Add("@ProjectGUID", _projectGUID);
                    parameters.Add("@URL", crawledPage.Uri.AbsoluteUri);
                    parameters.Add("@StatusCode", crawledPage.StatusCode);
                    parameters.Add("@PageSize", crawledPage.PageSizeInBytes);
                    parameters.Add("@HttpResponse", crawledPage.HttpWebResponse.Headers.ToString());
                    parameters.Add("@CrawlDepth", crawledPage.CrawlDepth);
                    parameters.Add("@ParentURI", crawledPage.ParentUri.AbsoluteUri);

                    database.ExecuteNonQueryWithParameters(command, parameters);
                    
                    if (string.IsNullOrEmpty(crawledPage.RawContent) == false)
                    {
                        string contentCommand = "INSERT INTO [PageContent] ([PageGUID], [ProjectGUID], [URL], [RawContent]) VALUES (@pageGUID, @projectGUID, @url, @rawContent);";
                        Dictionary<string, object> contentParams = new Dictionary<string, object>();
                        contentParams.Add("@pageGUID", crawledPage.PageGUID);
                        contentParams.Add("@projectGUID", _projectGUID);
                        contentParams.Add("@url", crawledPage.Uri.AbsoluteUri);
                        contentParams.Add("@rawContent", crawledPage.RawContent);
                        
                        database.ExecuteNonQueryWithParameters(contentCommand, contentParams);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                    _logger.Error("An unhandled exception was thrown inserting crawled page details/content to DB");
                    _logger.Error(exception);
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
