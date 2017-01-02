using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilelistBrowser.Service
{
    public class Links
    {
        public const string BaseUri = "http://filelist.ro/";

        public const string Browse = "/browse.php?search={0}&cat={1}&searchin={2}&page={3}";
        public const string Login = "/takelogin.php";
        public const string HomePage = "/my.php";
        public const string Loggout = "/logout.php?id={0}";

    }
}
