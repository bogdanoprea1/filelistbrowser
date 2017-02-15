using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilelistBrowser.DataBase.Model
{
    public class TorrentTable
    {
        public int TorrentId
        {
            get;
            set;
        }
        public string TorrentName { get; set; }
        public string Type { get; set; }
        public long DownloadNumber { get; set; }
        public int Seeders { get; set; }
        public string Uri { get; set; }
        public Uri Details { get; set; }
        public string Gender { get; set; }
        public decimal Size { get; set; }
        public Uri Image { get; set; }

        public bool IsFavorite { get; set; }
    }
}
