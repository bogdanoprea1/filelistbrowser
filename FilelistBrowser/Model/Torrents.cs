using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        private TorrentDetails _torrentDetails;

        public TorrentDetails TorrentDetails
        {
            get { return _torrentDetails; }
            set { _torrentDetails = value; OnPropertyChanged("TorrentDetails"); }
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
