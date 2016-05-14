using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeocachingToolbox.GeocachingCom
{
    public interface IGCConnector
    {
        Task<string> Login(string login, string password);
        Task<string> GetPage(string url);
        Task<HttpContent> GetContent(string url, IDictionary<string, string> getData);
        Task<string> PostToPage(string url, IDictionary<string, string> parameters);
    }
}
