using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SQLite;
using FilelistBrowser.DataBase.Model;

namespace FilelistBrowser.DataBase
{

    public class SqliteConnector
    {
        private string dbPath
        {
            get
            { return "flDb.db"; }
        }

        private SQLiteConnection _connection;

        public SqliteConnector()
        {
            _connection = new SQLiteConnection("Data Source=" + dbPath);
            _connection.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [TorrentTable] (TorrentId INTEGER PRIMARY KEY, 'TorrentName' TEXT, 'Type' TEXT, " +
                    " 'DownloadNumber' NUM, 'Seeders' INT, 'Uri' TEXT, 'Details' TEXT, 'Gender' TEXT, 'Size' NUM, 'Image' TEXT, 'IsFavorite' INT, 'PoserImage' TEXT, 'Video' TEXT, 'ImdbMark' NUM );", _connection))
            {
                mCmd.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public void InsertTorrent(TorrentTable torrentId)
        {
            _connection.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("INSERT INTO TorrentTable(TorrentId, TorrentName, Type, ) VALUES(" + torrentId + ")", _connection))
            {
                mCmd.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public bool TorrentExists(int torrentId)
        {
            _connection.Open();
            object ret;
            using (SQLiteCommand mCmd = new SQLiteCommand("SELECT TorrentId FROM TorrentTable", _connection))
            {
                ret = mCmd.ExecuteScalar();
            }
            _connection.Close();
            return ret == null ? false : true;
        }

        public List<TorrentTable> GetTorrents()
        {
            _connection.Open();
            List<TorrentTable> torrents = new List<TorrentTable>();
            using (SQLiteCommand mCmd = new SQLiteCommand("SELECT TorrentId FROM TorrentTable", _connection))
            {
                SQLiteDataReader reader = mCmd.ExecuteReader();
                while (reader.Read())
                    torrents.Add(new TorrentTable() { TorrentId = Convert.ToInt32(reader["TorrentId"]) });
            }
            _connection.Close();
            return torrents;
        }

    }
}
 //public string TorrentName { get; set; }
 //       public string Type { get; set; }
 //       public long DownloadNumber { get; set; }
 //       public int Seeders { get; set; }
 //       public string Uri { get; set; }
 //       public Uri Details { get; set; }
 //       public string Gender { get; set; }
 //       public decimal Size { get; set; }
 //       public Uri Image { get; set; }