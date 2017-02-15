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
using System.Text.RegularExpressions;
using System.Net;

namespace FilelistBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string userUriId { get; set; }
        private Grid lastOpenGrid { get; set; }
        public List<Torrents> _torrents { get; set; }
        private int _currentPage = 0;
        private FilelistViewModel _flvm;


        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _flvm = new FilelistViewModel();
                this.DataContext = _flvm;
                CheckLogin();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        private async void CheckLogin()
        {
            string user = FilelistBrowser.Properties.Settings.Default["User"].ToString();
            string password = FilelistBrowser.Properties.Settings.Default["Password"].ToString();

            if (user != "")
            {
                await _flvm.Login(user, MD5Cryptor.Decrypt(password, true));

                if (_flvm.IsLoggedin)
                {
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
            else
            {
                gridLoggIn.Visibility = Visibility.Visible;
                grdTop.Visibility = Visibility.Collapsed;
                loadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void txtCauta_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await _flvm.Browse(txtCauta.Text, Convert.ToInt32(cmbCategory.SelectedValue), Convert.ToInt32(cmbWhere.SelectedValue), txtPages.Text == "" ? 0 : Convert.ToInt32(txtPages.Text));
                scrollListTorrents.ScrollToTop();
            }
        }

        private void OpenDetails(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
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

        private void btn_LogOff(object sender, RoutedEventArgs e)
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
            await _flvm.Browse(txtCauta.Text, Convert.ToInt32(cmbWhere.SelectedValue == null ? 0 : cmbWhere.SelectedValue), Convert.ToInt32(cmbCategory.SelectedValue == null ? 0 : cmbCategory.SelectedValue), txtPages.Text == "" ? 0 : Convert.ToInt32(txtPages.Text));
            scrollListTorrents.ScrollToTop();
            _currentPage = 0;
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
            Grid parent = (((Grid)((Grid)sender).Parent).Parent as Border).Parent as Grid;
            Grid child = ((Grid)(parent).Children[1]);

            Torrents t = parent.DataContext as Torrents;
            if (t.Type.Contains("Film"))
            {
                await _flvm.GetDetails(t);

                child.Visibility = child.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

                if (child.Visibility == Visibility.Visible)
                {
                    if (lastOpenGrid == null)
                    {
                        lastOpenGrid = child;
                    }
                    else
                    {
                        if (lastOpenGrid != child)
                        {
                            lastOpenGrid.Visibility = Visibility.Collapsed;
                            lastOpenGrid = child;
                        }
                    }
                }
            }
        }

        private void youtube_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img.DataContext != null)
                System.Diagnostics.Process.Start(img.DataContext.ToString());
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

        private void btnDownloadUp_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortDownloadsUp();
        }

        private void btnDownloadDown_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortDownloadsDown();
        }

        private void btnBetweenDwonloads_Click(object sender, RoutedEventArgs e)
        {
            int min = txtMin.Text == "" ? 0 : Convert.ToInt32(txtMin.Text);
            int max = txtMax.Text == "" ? 0 : Convert.ToInt32(txtMax.Text);

            _flvm.SortDownoadLimits(min, max);
        }

        private void btnSeedersDown_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortSeedersDown();
        }

        private void btnSeedersUp_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortSeedersUp();
        }

        private async void downloadTorrent_Click(object sender, RoutedEventArgs e)
        {
            Button tb = (Button)sender;
            Torrents ts = (Torrents)tb.DataContext;
            byte[] content = await _flvm.Download(ts.Uri.ToString());
            SaveFileDialog sf = new SaveFileDialog();
            sf.FileName = ts.TorrentName; // Default file name
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

        private async void btn_GoHome_Click(object sender, RoutedEventArgs e)
        {
            
            await _flvm.Browse("", 0, 0, 0);
            _currentPage = 0;
            scrollListTorrents.ScrollToTop();
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(_flvm.IsScollable)
            { 
                var currentScroll = e.VerticalOffset;
                var totalHeigth = ((ScrollViewer)sender).ScrollableHeight;
                if (totalHeigth != 0)
                {
                    if (currentScroll == totalHeigth)
                    {
                        int pages = txtPages.Text == "" ? 0 : Convert.ToInt32(txtPages.Text);
                        _currentPage++;
                        await _flvm.BrowseOnePage(txtCauta.Text, Convert.ToInt32(cmbWhere.SelectedValue == null ? 0 : cmbWhere.SelectedValue),
                            Convert.ToInt32(cmbCategory.SelectedValue == null ? 0 : cmbCategory.SelectedValue),
                             pages + _currentPage);
                    }
                }
            }
        }

        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            Button tb = (Button)sender;
            OpenDetails(tb.DataContext.ToString());
        }

        private void btnDateUp_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortDateUp();
        }

        private void btnDateDown_Click(object sender, RoutedEventArgs e)
        {
            _flvm.SortDateDown();
        }

        private async void openNewTabDetails_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            Torrents t = menu.DataContext as Torrents;
            if (t != null)
            {
                await _flvm.GetDetails(t);
                foreach (var item in _flvm.Cookies)
                {
                    InternetSetCookie(t.Details.ToString(), item.Name, item.Value);
                }
                WebBrowser wb = new WebBrowser();
                wb.Navigated += wb_Navigated;
                wb.Navigate(t.Details);

                ClosableTab theTabItem = new ClosableTab();
                theTabItem.Title = t.TorrentName;
                theTabItem.Content = wb;
                tabControl1.Items.Add(theTabItem);
                theTabItem.Focus();
            }
            else
            {
                MessageBox.Show("I can't belive it's not working.. wtf!?");
            }

        }

        void wb_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent((WebBrowser)sender, true);
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        private async void searchbyCategory_Click(object sender, MouseButtonEventArgs e)
        {
            Image i = (Image)sender;
            Torrents t = i.DataContext as Torrents;
            string type = t.Type;

            int index = _flvm.CategoryDictionar.FirstOrDefault(x => x.Value == type).Key;
            if (index > 0)
            {
                await _flvm.Browse("", 0, index, 0);
                scrollListTorrents.ScrollToTop();
            }
            
        }

        private void btnRecomended_Click(object sender, RoutedEventArgs e)
        {
            _flvm.GetRecomended();
        }

        private void btnRatio_Click(object sender, RoutedEventArgs e)
        {
            _flvm.LoadTop10Ratio();
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

      

    }

    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(BrowserBehavior),
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser webBrowser = dependencyObject as WebBrowser;
            if (webBrowser != null)
                webBrowser.NavigateToString(e.NewValue as string ?? "&nbsp;");
        }
    }
}
