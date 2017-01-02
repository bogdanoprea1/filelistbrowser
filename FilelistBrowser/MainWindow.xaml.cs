using FilelistBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using HtmlAgilityPack;
using System.Windows.Controls.Primitives;
using FilelistBrowser.Model;
using FilelistBrowser.Service;
using FilelistBrowser.ViewModel;
using Microsoft.Win32;
using System.IO;
using FilelistBrowser.Helpers;

namespace FilelistBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string userUriId { get; set; }
        public List<Torrents> _torrents { get; set; }
        private int _currentPage = 0;
        private FilelistViewModel _flvm;


        public MainWindow()
        {
            InitializeComponent();
            _flvm = new FilelistViewModel();
            this.DataContext = _flvm;
            CheckLogin();

        }

        private async void CheckLogin()
        {
            string user = FilelistBrowser.Properties.Settings.Default["User"].ToString();
            string password = FilelistBrowser.Properties.Settings.Default["Password"].ToString();

            if (user != "")
            {
                await _flvm.Login(user, MD5Cryptor.Decrypt(password, true));
                loadingBar.Visibility = Visibility.Collapsed;
                txtWhoIsLoggedIn.Text = "Welcome, " + _flvm.UserName;
                gridLoggIn.Visibility = Visibility.Collapsed;
                gridLoggedIn.Visibility = Visibility.Visible;
                grdTop.Visibility = Visibility.Visible;

            }
            else
            {
                gridLoggIn.Visibility = Visibility.Visible;
                grdTop.Visibility = Visibility.Collapsed;
                loadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
               await _flvm.Browse(txtCauta.Text, Convert.ToInt32(cmbCategory.SelectedValue), Convert.ToInt32(cmbWhere.SelectedValue), cmbPages.SelectedIndex);
        }

        private async void click_btnLogin(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pwPassword.Password;


            grdTop.Visibility = Visibility.Collapsed;
            gridLoggIn.Visibility = Visibility.Collapsed;
            loadingBar.Visibility = Visibility.Visible;




            await _flvm.Login(username, password);
            if (_flvm.IsLoggedin)
            {
                if ((bool)chkRememberMe.IsChecked)
                {
                    FilelistBrowser.Properties.Settings.Default["Password"] = MD5Cryptor.Encrypt(password);
                    FilelistBrowser.Properties.Settings.Default["User"] = username;
                    FilelistBrowser.Properties.Settings.Default.Save();
                }
                loadingBar.Visibility = Visibility.Collapsed;
                txtWhoIsLoggedIn.Text = "Welcome, " + _flvm.UserName;
                gridLoggIn.Visibility = Visibility.Collapsed;
                gridLoggedIn.Visibility = Visibility.Visible;
                grdTop.Visibility = Visibility.Visible;
            }
            else
            {
                gridLoggIn.Visibility = Visibility.Visible;
                gridLoggedIn.Visibility = Visibility.Collapsed;
                grdTop.Visibility = Visibility.Collapsed;
                txtWhoIsLoggedIn.Text = "";
                loadingBar.Visibility = Visibility.Collapsed;
                MessageBox.Show("Incorect username or password");
            }

        }

        private void btnLogOff_Click(object sender, RoutedEventArgs e)
        {
            loadingBar.Visibility = Visibility.Visible;
            _flvm.Logout();
            gridLoggIn.Visibility = Visibility.Visible;
            gridLoggedIn.Visibility = Visibility.Collapsed;
            grdTop.Visibility = Visibility.Collapsed;
            txtWhoIsLoggedIn.Text = "";
            FilelistBrowser.Properties.Settings.Default["Password"] = "";
            FilelistBrowser.Properties.Settings.Default["User"] = "";
            FilelistBrowser.Properties.Settings.Default.Save();
           
            loadingBar.Visibility = Visibility.Collapsed;
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _torrents = new List<Torrents>();
            await _flvm.Browse(txtCauta.Text, Convert.ToInt32(cmbWhere.SelectedValue == null ? 0 : cmbWhere.SelectedValue), Convert.ToInt32(cmbCategory.SelectedValue == null ? 0 : cmbCategory.SelectedValue), cmbPages.SelectedIndex == -1 ? 0 : cmbPages.SelectedIndex);

        }

        private void OpenDetails(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        private void details_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image tb = (Image)sender;
            OpenDetails(tb.DataContext.ToString());
        }

        private async void download_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image tb = (Image)sender;
           byte[] content = await _flvm.Download(tb.DataContext.ToString());
           SaveFileDialog sf = new SaveFileDialog();
           sf.FileName = "torrent"; // Default file name
           sf.DefaultExt = ".torrent"; // Default file extension
           sf.Filter = "Torrents (.torrent)|*.torrent";
            Nullable<bool> result = sf.ShowDialog();
           if (result == true)
           {
               string filename = sf.FileName;
               File.WriteAllBytes(filename, content);
               System.Diagnostics.Process.Start(filename);
           }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("merge");
        }

        private async void gridT_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid parent = ((Grid)((Grid)sender).Parent);
            Grid child = ((Grid)(parent).Children[1]);
           
            Torrents t = parent.DataContext as Torrents;//detailsElement.DataContext as Torrents;
            if (t.Type.Contains("Film"))
            {
                await _flvm.GetDetails(t);
                //child.DataContext = t.TorrentDetails;
           
            }
            child.Visibility = child.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }


        private void youtube_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img.DataContext!=null)
            System.Diagnostics.Process.Start(img.DataContext.ToString());
        }
        private void dwButton_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortDownloads();
         
        }

        private void seen_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int torrentId = Convert.ToInt32(btn.DataContext);
            bool inserted = _flvm.InsertTorrentOk(torrentId);

            if (!inserted)
            {
                MessageBox.Show("Success!");
            }
            else
            {
                MessageBox.Show("Torrent allready added");
            }
        }

    }
}
