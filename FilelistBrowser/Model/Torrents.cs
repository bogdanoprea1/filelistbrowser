using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FilelistBrowser.Model
{
    public class Torrents : INotifyPropertyChanged
    {

        public int TorrentId { get; set; }

        public string TorrentName { get; set; }
        public string Type { get; set; }
        public long DownloadNumber { get; set; }
        public int Seeders { get; set; }
        public string Uri { get; set; }
        public Uri Details { get; set; }
        public string Gender { get; set; }
        public decimal Size { get; set; }
        public Uri Image { get; set; }
        public int Leechers { get; set; }
        public int CommentNumber { get; set; }

        public string RecomendedRatio { get; set; }
        public decimal RecomendedRatioDecimal { get; set; }
        public DateTime Date { get; set; }

        private TorrentDetails _torrentDetails;

        public TorrentDetails TorrentDetails
        {
            get { return _torrentDetails; }
            set { _torrentDetails = value; OnPropertyChanged("TorrentDetails"); }
        }

        private Visibility _sortVisibility;

        public Visibility SortVisibility
        {
            get { return _sortVisibility; }
            set { _sortVisibility = value; OnPropertyChanged("SortVisibility"); }
        }

        private SolidColorBrush _background;

        public SolidColorBrush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged("Background"); }
        } 

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

    public class TorrentDetails : INotifyPropertyChanged
    {

        public string PoserImage { get; set; }
        public string Video { get; set; }
        public decimal ImdbMark { get; set; }

        public string HtmlStringDetails { get; set; }

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
}
