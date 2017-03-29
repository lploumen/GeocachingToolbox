using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;

namespace GeocachingToolbox.GeocachingCom
{
    public class GCConnector : IGCConnector
    {
        private const string UrlPrefix = "https://www.geocaching.com/";
        private WebBrowserSimulator webBrowser;

        public Task<string> GetPage(string url)
        {
            var usedUrl = prefixUrlIfNeeded(url);
            var response = webBrowser.GetRequestAsString(usedUrl);
            return response;
        }

        private static string prefixUrlIfNeeded(string url)
        {
            string usedUrl = url;
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                usedUrl = UrlPrefix + url;
            }
            return usedUrl;
        }

        public Task<HttpContent> GetContent(string fullUrl, IDictionary<string, string> getData)
        {
            var usedUrl = prefixUrlIfNeeded(fullUrl);
            return webBrowser.GetRequest(usedUrl, getData);
        }

        public Task<string> PostToPage(string url, IDictionary<string, string> parameters)
        {
            var usedUrl = prefixUrlIfNeeded(url);
            var response = webBrowser.PostRequest(usedUrl, parameters);
            return response;
        }

        private string extractRequestVerificationToken(string page)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(page);
            var inputElement = document.QuerySelector($"input[name=\"{GCConstants.REQUEST_VERIFICATION_TOKEN}\"]");
            return inputElement.GetAttribute("value");
        }

        public async Task<string> Login(string login, string password)
        {
            webBrowser = new WebBrowserSimulator();

            var loginPage = await webBrowser.GetRequestAsString(UrlPrefix + "/account/login?ReturnUrl=/play");
            var verificationToken = extractRequestVerificationToken(loginPage);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Username", login },
                { "Password", password },
                { GCConstants.REQUEST_VERIFICATION_TOKEN, verificationToken },
            };

            await webBrowser.PostRequest(UrlPrefix + "account/login/?returnUrl=https%3a%2f%2fwww.geocaching.com/my/default.aspx", parameters);

            var response = await webBrowser.GetRequestAsString(UrlPrefix + "my/default.aspx");
            return response;
        }
    }
}
