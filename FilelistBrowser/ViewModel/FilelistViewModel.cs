using FilelistBrowser.DataBase;
using FilelistBrowser.Model;
using FilelistBrowser.Service;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private Visibility _listLoadingVisibility;

        public Visibility ListLoadingVisibility
        {
            get { return _listLoadingVisibility; }
            set { _listLoadingVisibility = value; OnPropertyChanged("ListLoadingVisibility"); }
        }

        private string _searchText;
        public string SearchText
        {
          get { return _searchText; }
            set { _searchText = value; OnPropertyChanged("SearchText"); }
        }

        private bool isScollable;
        public bool IsScollable
        {
            get { return isScollable; }
            set { isScollable = value; OnPropertyChanged("IsScollable"); }
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
        public Requests Requests
        {
            get { return _requests; }
            set { _requests = value; OnPropertyChanged("Requests"); }
        }

        private UserProfile _userP;

        public UserProfile UserP
        {
            get { return _userP; }
            set { _userP = value; OnPropertyChanged("UserP"); }
        }

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

        private SqliteConnector _sqlConnector;

        private List<Cookie> _cookies;

        public List<Cookie> Cookies
        {
            get { return _cookies; }
            set { _cookies = value; OnPropertyChanged("Cookies"); }
        }

        public FilelistViewModel()
        {
            ListLoadingVisibility = Visibility.Visible;
            TorrentList = new SortableObservableCollection<Torrents>();
            _requests = new Requests();
            _userP = new UserProfile();
            CategoryDictionar = new Dictionary<int, string>();
            SearchInDictionar = new Dictionary<int, string>();
            SortDictionar = new Dictionary<int, string>();
            Pages = new Dictionary<int, string>();
            _sqlConnector = new SqliteConnector();
            ListLoadingVisibility = Visibility.Collapsed;
            IsScollable = true;
        }



        private List<Torrents> GetTorrentsPage()
        {
            return new List<Torrents>();
        }

        public async Task Login(string username, string password)
        {
            string html = await _requests.Login(username, password);
            this.IsLoggedin = _requests.IsLoggedIn;
            this.Cookies = _requests.Cookies;
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
            this.Cookies = null;
        }

        public async Task Browse(string search, int searchIn, int category, int pageCount)
        {
            ListLoadingVisibility = Visibility.Visible;
            _torrentList.Clear();
            await Browse(search, searchIn, category, pageCount, false);
            ListLoadingVisibility = Visibility.Collapsed;
            IsScollable = true;
        }

        public async Task BrowseOnePage(string search, int searchIn, int category, int pageNr)
        {
            await BrowseOnePage(search, searchIn, category, pageNr, false);
        }

        public async Task GetDetails(Torrents torrent)
        { 
            string detailsHtml = await _requests.GetDetails(torrent.Details);
            _torrentList.Where(t => t.TorrentName == torrent.TorrentName && t.Size == torrent.Size).FirstOrDefault().TorrentDetails = ParseDetails(detailsHtml);

        }

        private async Task Browse(string search, int searchIn, int category, int pageCount, bool withFilters)
        {
            IsScollable = true;
            for (int i = 0; i <= pageCount; i++)
            {             
                string torrentHTML = await _requests.Browse(search, searchIn, category, i);
                
                FillTorrentList(torrentHTML);

                if (i == 0 && withFilters)
                {
                    FillUserDetails(torrentHTML);
                    FillSearchFilter(torrentHTML);
                }
            }
        }

        private async Task BrowseOnePage(string search, int searchIn, int category, int pageNr, bool withFilters)
        {
                string torrentHTML = await _requests.Browse(search, searchIn, category, pageNr);
                FillTorrentList(torrentHTML);
        }

        public bool InsertTorrentOk(int torrentId)
        {
            return this.InsertTorrent(torrentId);
        }
        public async Task<byte[]> Download(string url)
        {
             return await _requests.Download(url);
        }
        
        
        public void SortDownloadsUp()
        {
            _torrentList.Sort(x => x.DownloadNumber, ListSortDirection.Ascending);
        }

        public void SortDownloadsDown()
        {
            _torrentList.Sort(x => x.DownloadNumber, ListSortDirection.Descending);
        }


        public void SortSeedersUp()
        {
            _torrentList.Sort(x => x.Seeders, ListSortDirection.Ascending);
        }

        public void SortSeedersDown()
        {
            _torrentList.Sort(x => x.Seeders, ListSortDirection.Descending);
        }

        public void SortDateUp()
        {
            _torrentList.Sort(x => x.Date, ListSortDirection.Ascending);
        }

        public void SortDateDown()
        {
            _torrentList.Sort(x => x.Date, ListSortDirection.Descending);
        }

        public void GetRecomended()
        {
            IsScollable = false;
            _torrentList.Sort(x => x.RecomendedRatioDecimal, ListSortDirection.Descending);
        }

        public async void LoadTop10Ratio()
        {
            IsScollable = false;
            ListLoadingVisibility = Visibility.Visible;
            _torrentList.Clear();
            List<Torrents> torrentList = new List<Torrents>();
            int page = 0;
            while (torrentList.Count < 10 && page < 50)
            {
                List<Torrents> tempList = new List<Torrents>();
                string torrentHTML = await _requests.Browse("", 0, 0, page);
                tempList.AddRange(GetTorrents(torrentHTML));
                foreach (var item in tempList)
                {
                    if (((item.Leechers >= (item.Seeders / 2)) && item.Leechers > 200) || item.Leechers > 400)
                    {
                        torrentList.Add(item);
                    }
                }
                page++;
            }
            foreach (var item in torrentList)
            {
                _torrentList.Add(item);
            }
            _torrentList.Sort(l => l.Leechers, ListSortDirection.Descending);
            ListLoadingVisibility = Visibility.Collapsed;
        }

        public void SortDownoadLimits(int min, int max)
        {
            if (min >= 0 && max >= 0 && min <= max)
            {
                foreach (var item in _torrentList)
                {
                    if (max > 0)
                    {
                        if (!(item.DownloadNumber > min && item.DownloadNumber < max))
                            item.SortVisibility = Visibility.Collapsed;
                        else item.SortVisibility = Visibility.Visible;
                    }
                    else
                    {
                        if (!(item.DownloadNumber > min))
                            item.SortVisibility = Visibility.Collapsed;
                        else item.SortVisibility = Visibility.Visible;
                    }
                }
            }
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

                    int valueLeechers = 0;
                    var divLeechers = listDivs[9].Descendants("span").ToList().FirstOrDefault();//.Descendants("b").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;

                        if(divLeechers.Descendants("b").FirstOrDefault() != null)
                        {
                            valueLeechers = Convert.ToInt32(divLeechers.Descendants("b").ToList().FirstOrDefault().InnerText);
                        }
                        else
                            valueLeechers = 0;

                    string type = listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").ToList().FirstOrDefault().Attributes["alt"].Value;
                    string uri = listDivs[3].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string details = "http://filelist.ro/" + listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string gender = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;

                    var free = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("img").ToList();
                    bool freeleech = false;
                    foreach (var imgFree in free)
                    {
                        if (imgFree.Attributes["alt"].Value == "FreeLeech")
                        {
                            freeleech = true;
                        }
                            
                    }

                    string sizeValue = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().FirstChild.InnerText;
                    var sizeType = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().LastChild.InnerText;
                    string imageUri = "http://filelist.ro/" + listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").
                        FirstOrDefault().Attributes["src"].Value;


                    string commentNumeber = "";
                    var commentDiv = listDivs[4].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault();
                    if (commentDiv.Descendants("b").FirstOrDefault() == null)
                    {
                        commentNumeber = "0";
                    }
                    else {
                        commentNumeber = commentDiv.Descendants("b").ToList().FirstOrDefault().Descendants("a").FirstOrDefault().InnerText;
                    }
                   
                    
                    
                    string timeDate = listDivs[5].Descendants("span").ToList().FirstOrDefault().Descendants("nobr").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;

                    string time = timeDate.Substring(0, 8);
                    string date = timeDate.Substring(8, timeDate.Length - 8);           

                    int torrentId = 0;
                    int commentNr = commentNumeber != "" ? Convert.ToInt32(commentNumeber) : 0;

                    long downloadNr = Convert.ToInt64(divDownloads.Replace(",", ""));


                    if (id != null && id != "")
                    {
                        torrentId = Convert.ToInt32(id.Substring(id.LastIndexOf('=') + 1));
                    }
                    decimal recMark = GetRecomendedMark(Convert.ToDecimal(commentNr), Convert.ToDecimal(downloadNr), Convert.ToDecimal(divSeeders), Convert.ToDecimal(valueLeechers));
                    _torrentList.Add(new Torrents()
                    {
                        TorrentId = torrentId,
                        TorrentName = divName,
                        DownloadNumber = Convert.ToInt64(divDownloads.Replace(",", "")),
                        Seeders = Convert.ToInt32(divSeeders),
                        Leechers = valueLeechers,
                        Type = type,
                        Uri = uri,
                        Details = new Uri(details),
                        Gender = gender,
                        Size = sizeType == "GB" ? Convert.ToDecimal(sizeValue) : Math.Round(Convert.ToDecimal(sizeValue) / 1024, 2),
                        Image = new Uri(imageUri),
                        SortVisibility = Visibility.Visible,
                        Date = ConvertToDate(date, time),
                        CommentNumber = commentNr,
                        RecomendedRatio = recMark.ToString("0.00"),
                        RecomendedRatioDecimal = recMark,
                        Background = freeleech == true ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.White)
                    });
                }
                catch
                {
                }

            }

        }

        private List<Torrents> GetTorrents(string html)
        {
            List<Torrents> tempTorrentList = new List<Torrents>();
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

                    int valueLeechers = 0;
                    var divLeechers = listDivs[9].Descendants("span").ToList().FirstOrDefault();//.Descendants("b").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;

                    if (divLeechers.Descendants("b").FirstOrDefault() != null)
                    {
                        valueLeechers = Convert.ToInt32(divLeechers.Descendants("b").ToList().FirstOrDefault().InnerText);
                    }
                    else
                        valueLeechers = 0;

                    string type = listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").ToList().FirstOrDefault().Attributes["alt"].Value;
                    string uri = listDivs[3].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string details = "http://filelist.ro/" + listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Attributes["href"].Value;
                    string gender = listDivs[1].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;
                    string sizeValue = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().FirstChild.InnerText;
                    var sizeType = listDivs[6].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().LastChild.InnerText;
                    string imageUri = "http://filelist.ro/" + listDivs[0].Descendants("span").ToList().FirstOrDefault().Descendants("a").ToList().FirstOrDefault().Descendants("img").
                        FirstOrDefault().Attributes["src"].Value;


                    string commentNumeber = "";
                    var commentDiv = listDivs[4].Descendants("span").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault();
                    if (commentDiv.Descendants("b").FirstOrDefault() == null)
                    {
                        commentNumeber = "0";
                    }
                    else
                    {
                        commentNumeber = commentDiv.Descendants("b").ToList().FirstOrDefault().Descendants("a").FirstOrDefault().InnerText;
                    }



                    string timeDate = listDivs[5].Descendants("span").ToList().FirstOrDefault().Descendants("nobr").ToList().FirstOrDefault().Descendants("font").ToList().FirstOrDefault().InnerText;

                    string time = timeDate.Substring(0, 8);
                    string date = timeDate.Substring(8, timeDate.Length - 8);

                    int torrentId = 0;
                    int commentNr = commentNumeber != "" ? Convert.ToInt32(commentNumeber) : 0;

                    long downloadNr = Convert.ToInt64(divDownloads.Replace(",", ""));


                    if (id != null && id != "")
                    {
                        torrentId = Convert.ToInt32(id.Substring(id.LastIndexOf('=') + 1));
                    }

                    tempTorrentList.Add(new Torrents()
                    {
                        TorrentId = torrentId,
                        TorrentName = divName,
                        DownloadNumber = Convert.ToInt64(divDownloads.Replace(",", "")),
                        Seeders = Convert.ToInt32(divSeeders),
                        Leechers = valueLeechers,
                        Type = type,
                        Uri = uri,
                        Details = new Uri(details),
                        Gender = gender,
                        Size = sizeType == "GB" ? Convert.ToDecimal(sizeValue) : Math.Round(Convert.ToDecimal(sizeValue) / 1024, 2),
                        Image = new Uri(imageUri),
                        SortVisibility = Visibility.Visible,
                        Date = ConvertToDate(date, time),
                        CommentNumber = commentNr,
                        RecomendedRatio = GetRecomendedMark(Convert.ToDecimal(commentNr), Convert.ToDecimal(downloadNr), Convert.ToDecimal(divSeeders), Convert.ToDecimal(valueLeechers)).ToString("0.00")
                    });
                }
                catch
                {
                }

            }
            return tempTorrentList;

        }

        private decimal GetRecomendedMark(decimal com, decimal down, decimal seeds, decimal leech)
        {
           // decimal lowValue = (com + seeds + leech) / 100.00m;

            decimal c = com;
            decimal d = down/1000;
            decimal s = seeds / 10;
            decimal l = leech / 10;

            decimal rez = (c + d + s + l) / 4;
            return rez;
            //if (sl == 0)
            //    return 0.00m;
            //else
            //{
            //    decimal result = Math.Round((dw / sl) * 100, 2);
            //    return result;
            //}
        }

        private void FillUserDetails(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var profile = doc.DocumentNode.SelectNodes(XPaths.UserProfileDiv).Descendants("div").ToList()[1].Descendants("div").ToList();
            string userStatus = profile[0].Descendants("font").FirstOrDefault().InnerText;
            string ratio = profile[3].Descendants("span").FirstOrDefault().InnerText.Replace("&nbsp;", " ");
            string downloaded = profile[4].Descendants("span").ToList()[0].InnerText.Replace("&nbsp;", " ");
            string uploaded = profile[4].Descendants("span").ToList()[1].InnerText.Replace("&nbsp;", " ");


            UserP = new UserProfile() { 
                UserName = this.UserName,
                Status = userStatus,
                Ratio = ratio,
                Downloaded = downloaded,
                Uploaded = uploaded
            };

        }

        private DateTime ConvertToDate(string date, string time)
        {
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0;

            //dd/mm/yyyy
            //hh:mm:ss
            try
            {
                year = Convert.ToInt32(date.Substring(6, 4));
                month = Convert.ToInt32(date.Substring(3, 2));
                day = Convert.ToInt32(date.Substring(0, 2));

                hour = Convert.ToInt32(time.Substring(0, 2));
                minute = Convert.ToInt32(time.Substring(3, 2));
                second = Convert.ToInt32(time.Substring(6, 2));
            }
            catch
            {
            }



            return new DateTime(year, month, day, hour, minute, second);

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
            dets.HtmlStringDetails = html;
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
