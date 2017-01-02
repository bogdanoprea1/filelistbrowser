using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilelistBrowser
{
    public static class XPaths
    {
        public static string Categories { get { return "//*[@id=\"maincolumn\"]/div/div[3]/div/table/tbody/tr/td/form/p/select[1]"; } }
        public static string SearchIn { get { return "//*[@id=\"maincolumn\"]/div/div[3]/div/table/tbody/tr/td/form/p/select[2]"; } }
        public static string Sort { get { return "//*[@id=\"maincolumn\"]/div/div[3]/div/table/tbody/tr/td/form/p/select[3]";} }
        public static string Name { get { return "//*[@id=\"wrapper\"]/div[1]/div/div[1]/div[2]/div[1]/a/span"; } }

                                                  
        public static string Id { get { return "//*[@id=\"wrapper\"]/div[1]/div/div[1]/div[2]/div[3]/span[4]/a"; } }

        public static string SearchForm { get { return "//*[@id='maincolumn']/div/div[3]/div/table";  } }

        public static string DetailsPosterImage { get { return "//*[@id=\"maincolumn\"]/div[1]/div[5]/div/div[1]/div[2]/img"; } }
        public static string DetailsIMDBImage { get { return "//*[@id=\"maincolumn\"]/div[1]/div[5]/div/div[1]/span[2]/text()"; } }
        public static string DetailsVideoImage { get { return "//*[@id=\"maincolumn\"]/div[1]/div[5]/div/div[2]/iframe"; } }

        public static string DetailsPosterImage2 { get { return "//*[@id=\"maincolumn\"]/div[2]/div[5]/div/div[1]/div[2]/img"; } }
        public static string DetailsIMDBImage2 { get { return "//*[@id=\"maincolumn\"]/div[2]/div[5]/div/div[1]/span[2]/text()"; } }
        public static string DetailsVideoImage2 { get { return "//*[@id=\"maincolumn\"]/div[2]/div[5]/div/div[2]/iframe"; } }
    }
}
