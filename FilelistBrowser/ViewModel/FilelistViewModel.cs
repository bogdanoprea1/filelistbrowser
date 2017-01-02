using FilelistBrowser.DataBase;
using FilelistBrowser.Model;
using FilelistBrowser.Service;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FilelistBrowser.ViewModel
{
    public class FilelistViewModel : INotifyPropertyChanged
    {
        #region properties

        private DataGridRowDetailsVisibilityMode _rowDetailsVisible;
        public DataGridRowDetailsVisibilityMode RowDetailsVisible
        {
            get { return _rowDetailsVisible; }
            set
            {
                _rowDetailsVisible = value;
                OnPropertyChanged("RowDetailsVisible");
            }
        }


        private string _searchText;
        public string SearchText
        {
          get { return _searchText; }
            set { _searchText = value; OnPropertyChanged("SearchText"); }
        }

        private int _searchIn;
        public int SearchIn
        {
          get { return _searchIn; }
            set { _searchIn = value; OnPropertyChanged("SearchIn"); }
        }

        private int _category;
        public int Category
        {
          get { return _category; }
            set { _category = value; OnPropertyChanged("Category"); }
        }

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { _pageCount = value; OnPropertyChanged("PageCount"); }
        }

        private bool _isLoggedin;
        public bool IsLoggedin
        {
            get { return _isLoggedin; }
            set { _isLoggedin = value; OnPropertyChanged("IsLoggedin"); }
        }

        private int _userId;
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; OnPropertyChanged("IsLoggedin"); }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged("IsLoggedin"); }
        }

        private Requests _requests;

        private Dictionary<int, string> _categoryDictionar;
        public Dictionary<int, string> CategoryDictionar
        {
            get { return _categoryDictionar; }
            set { _categoryDictionar = value; OnPropertyChanged("CategoryDictionar"); }
        }

        private Dictionary<int, string> _searchInDictionar;
        public Dictionary<int, string> SearchInDictionar
        {
            get { return _searchInDictionar; }
            set { _searchInDictionar = value; OnPropertyChanged("SearchInDictionar"); }
        }


        private Dictionary<int, string> _sortDictionar;
        public Dictionary<int, string> SortDictionar
        {
            get { return _sortDictionar; }
            set { _sortDictionar = value; OnPropertyChanged("SortDictionar"); }
        }

        private Dictionary<int, string> _pages;
        public Dictionary<int, string> Pages
        {
            get { return _pages; }
            set { _pages = value; OnPropertyChanged("Pages"); }
        }

        private SortableObservableCollection<Torrents> _torrentList;
        public SortableObservableCollection<Torrents> TorrentList
        {
            get { return _torrentList; }
            set { _torrentList = value; }
        }
        #endregion

        private bool _downloadSortAscending = false;
        private SqliteConnector _sqlConnector;
        public FilelistViewModel()
        {
            TorrentList = new SortableObservableCollection<Torrents>();
            _requests = new Requests();
            CategoryDictionar = new Dictionary<int, string>();
            SearchInDictionar = new Dictionary<int, string>();
            SortDictionar = new Dictionary<int, string>();
            Pages = new Dictionary<int, string>();
            _sqlConnector = new SqliteConnector();
        }

        private List<Torrents> GetTorrentsPage()
        {
            return new List<Torrents>();
        }


        public async Task Login(string username, string password)
        {
            string html = await _requests.Login(username, password);
            this.IsLoggedin = _requests.IsLoggedIn;
            if (this.IsLoggedin)
            { 
                this.GetLogOutURI(html);
                await Browse("", 0, 0, 0, true);
            }
        }

        public async void Logout()
        {
            await _requests.LogOff(this.UserId);
            this.IsLoggedin = _requests.IsLoggedIn;
        }

        public async Task Browse(string search, int searchIn, int category, int pageCount)
        {
            _torrentList.Clear();
            await Browse(search, searchIn, category, pageCount, false);
        }

        public async Task GetDetails(Torrents torrent)
        { 
            string detailsHtml = await _requests.GetDetails(torrent.Details);
            _torrentList.Where(t => t.TorrentName == torrent.TorrentName && t.Size == torrent.Size).FirstOrDefault().TorrentDetails = ParseDetails(detailsHtml);
        }

        private async Task Browse(string search, int searchIn, int category, int pageCount, bool withFilters)
        {
            for (int i = 0; i <= pageCount; i++)
            {             
                string torrentHTML = await _requests.Browse(search, searchIn, category, i);
                
                FillTorrentList(torrentHTML);

                if (i == 0 && withFilters) FillSearchFilter(torrentHTML);
            }
        }

        public bool InsertTorrentOk(int torrentId)
        {
            return this.InsertTorrent(torrentId);
        }
        public async Task<byte[]> Download(string url)
        {
             return await _requests.Download(url);
        }
        public void SortDownloads()
        {
            
            if (_downloadSortAscending)
            {
                _torrentList.Sort(x => x.DownloadNumber, ListSortDirection.Ascending);
            }
            else
            {
                _torrentList.Sort(x => x.DownloadNumber, ListSortDirection.Descending);
            }
            _downloadSortAscending = !_downloadSortAscending;
        }

        #region SqlDB
        private bool InsertTorrent(int torrentId)
        {
            bool exists = _sqlConnector.TorrentExists(torrentId);
            if (!exists)
            {
                //_sqlConnector.InsertTorrent(torrentId);
                return false;
            }
            else
            {
                return true;
            }
               
        }
        #endregion

        #region HTML Parser
        private void GetLogOutURI(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            //*[@id="wrapper"]/div[1]/div/div[1]/div[2]/div[1]/a/span
            //*[@id=\"wrapper\"]/div[1]/div/div[1]/div[2]/div[1]/a/span";
            var name = doc.DocumentNode.SelectSingleNode(XPaths.Name);
            var id = doc.DocumentNode.SelectSingleNode(XPaths.Id);
            _userId = Convert.ToInt32(id.Attributes["href"].Value.Replace("/logout.php?id=", ""));
            UserName = name.InnerText;
        }
        
        private void FillSearchFilter(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var categories = doc.DocumentNode.SelectNodes(XPaths.SearchForm);
            //var x = categories.SelectNodes("td");
            var cats = categories[1].Descendants("select").ToList()[0];
            var searchin = categories[1].Descendants("select").ToList()[1];
            var sort = categories[1].Descendants("select").ToList()[2];

            var valuesCat = cats.Descendants("option").ToList();
            for (int i = 0; i < valuesCat.Count; i++)
            {
                CategoryDictionar.Add(Convert.ToInt32(valuesCat[i].Attributes["value"].Value), valuesCat[i].NextSibling.InnerText);
            }

            var valuesSearch = searchin.Descendants("option").ToList();
            for (int i = 0; i < valuesSearch.Count; i++)
            {
                SearchInDictionar.Add(Convert.ToInt32(valuesSearch[i].Attributes["value"].Value), valuesSearch[i].NextSibling.InnerText);
            }

            var valuesSort = sort.Descendants("option").ToList();
            for (int i = 0; i < valuesSort.Count; i++)
            {
                SortDictionar.Add(Convert.ToInt32(valuesSort[i].Attributes["value"].Value), valuesSort[i].NextSibling.InnerText);
            }

            for (int i = 0; i < 100; i++)
            {
                Pages.Add(i, i.ToString());
            }
        }

        private void FillTorrentList(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
        
            var torrents = doc.DocumentNode.Descendants("div").ToList().Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "torrentrow");
            foreach (var item in torrents)
            {
                try
                {
                    var listDivs = item.Descendants("div").ToList();
                    string divName = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["title"].Value;
                    string id = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("a").FirstOrDefault().Attributes["href"].Value;
                    string divDownloads = listDivs[7].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().FirstChild.InnerText;
                    string divSeeders = listDivs[8].Descendants("span").ToList().FirstOrDefault().Descendants("b").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;
                    string type = listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").ToList().FirstOrDefault().Attributes["alt"].Value;
                    string uri = listDivs[3].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string details = "http://filelist.ro/" + listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string gender = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;
                    string sizeValue = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().FirstChild.InnerText;
                    var sizeType = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().LastChild.InnerText;
                    string imageUri = "http://filelist.ro/" + listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").
                        FirstOrDefault().Attributes["src"].Value;

                    int torrentId = 0;
                    if (id != null && id != "")
                    {
                        torrentId = Convert.ToInt32(id.Substring(id.LastIndexOf('=') + 1));
                    }

                    _torrentList.Add(new Torrents()
                    {
                        TorrentId = torrentId,
                        TorrentName = divName,
                        DownloadNumber = Convert.ToInt64(divDownloads.Replace(",", "")),
                        Seeders = Convert.ToInt32(divSeeders),
                        Type = type,
                        Uri = uri,
                        Details = new Uri(details),
                        Gender = gender,
                        Size = sizeType == "GB" ? Convert.ToDecimal(sizeValue) : Math.Round(Convert.ToDecimal(sizeValue) / 1024, 2),
                        Image = new Uri(imageUri)
                    });
                }
                catch
                {
                }

            }

        }

        private TorrentDetails ParseDetails(string html)
        {
            TorrentDetails dets = new TorrentDetails();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var imdb = doc.DocumentNode.SelectNodes(XPaths.DetailsIMDBImage) == null ? doc.DocumentNode.SelectNodes(XPaths.DetailsIMDBImage2) : doc.DocumentNode.SelectNodes(XPaths.DetailsIMDBImage);
            var poster = doc.DocumentNode.SelectNodes(XPaths.DetailsPosterImage) == null ? doc.DocumentNode.SelectNodes(XPaths.DetailsPosterImage2) : doc.DocumentNode.SelectNodes(XPaths.DetailsPosterImage);
            var video = doc.DocumentNode.SelectNodes(XPaths.DetailsVideoImage) == null ? doc.DocumentNode.SelectNodes(XPaths.DetailsVideoImage2) : doc.DocumentNode.SelectNodes(XPaths.DetailsVideoImage);

            string imdbmarg =imdb!=null ? imdb.FirstOrDefault().InnerText : "0";
            string posterImg = poster != null ? poster.FirstOrDefault().Attributes["src"].Value.ToString() : "";
            string videoUri = video != null ? video.FirstOrDefault().Attributes["src"].Value : "";

            dets.ImdbMark =Convert.ToDecimal(imdbmarg);
            dets.PoserImage = posterImg;
            dets.Video = videoUri;

            return dets;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public void Sort<TKey>(Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction)
        {
            switch (direction)
            {
                case System.ComponentModel.ListSortDirection.Ascending:
                    {
                        ApplySort(Items.OrderBy(keySelector));
                        break;
                    }
                case System.ComponentModel.ListSortDirection.Descending:
                    {
                        ApplySort(Items.OrderByDescending(keySelector));
                        break;
                    }
            }
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            ApplySort(Items.OrderBy(keySelector, comparer));
        }

        private void ApplySort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }
    }
}
