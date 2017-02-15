using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Extensions;
namespace FilelistBrowser.Service
{
    public class Requests
    {

        private List<Cookie> _cookies;
        public List<Cookie> Cookies
        {
            get { return _cookies; }
            set { _cookies = value; }
        }

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { _isLoggedIn = value; }
        }

        private Uri Client
        {
            get { return new Uri(Links.BaseUri); }
        }

        public Requests()
        {
            Cookies = new List<Cookie>();   
        }

        private RestClient _baseClient;

        public RestClient BaseClient
        {
            get
            {
                if (this._baseClient == null)
                {
                    RestClient rc = new RestClient(Client);
                    rc.CookieContainer = new System.Net.CookieContainer();
                    _baseClient = rc;
                    return rc;

                }
                else return this._baseClient;
            }
            set
            {
                _baseClient = value;
            }
        }

        public async Task<string> Login(string userName, string password)
        {

            var request = new RestRequest("/takelogin.php", Method.POST);
            Cookies = new List<Cookie>();
            request.AddParameter("username", userName);
            request.AddParameter("password", password);
            IRestResponse response = await BaseClient.ExecuteTaskAsync(request);
            var cookie = BaseClient.CookieContainer.GetCookieHeader(new Uri("http://filelist.ro/my.php"));
            Cookies = BaseClient.CookieContainer.GetAllCookies().ToList();
            if (_cookies.Count == 4)
            {
                IsLoggedIn = true;
                return response.Content;
            }
            else
            {
                IsLoggedIn = false;
                return "";
            }
        }

        public async Task<string> Browse(string search, int searchIn, int category, int page)
        {
            var request = new RestRequest(string.Format(Links.Browse, search, category,searchIn, page), Method.GET);
            foreach (var item in Cookies)
            {
                request.AddCookie(item.Name, item.Value);
            }
            IRestResponse response = await _baseClient.ExecuteTaskAsync(request);
            return response.Content;        
        }

        public async Task<string> GetDetails(Uri url)
        {
            var request = new RestRequest(url);
            foreach (var item in Cookies)
            {
                request.AddCookie(item.Name, item.Value);
            }
            IRestResponse response = await _baseClient.ExecuteTaskAsync(request);
            return response.Content;        
        }

        public async Task LogOff(int userId)
        {
            var request = new RestRequest(string.Format(Links.Loggout, userId), Method.GET);
            foreach (var item in Cookies)
            {
                request.AddCookie(item.Name, item.Value);
            }
            IRestResponse response = await _baseClient.ExecuteTaskAsync(request);
            IsLoggedIn = false;
            Cookies = new List<Cookie>();
        }

        public async Task<byte[]> Download(string url)
        {
            var request = new RestRequest(url, Method.GET);
            foreach (var item in Cookies)
            {
                request.AddCookie(item.Name, item.Value);
            }
            IRestResponse response = await _baseClient.ExecuteTaskAsync(request);
            return response.RawBytes;
        }
    }
}
