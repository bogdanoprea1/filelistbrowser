using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Animation;
namespace FilelistBrowser
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class test : Window
    {
        public test()
        {
            InitializeComponent();
            tesst.AppendText(@"<img style='max-width: 700px;overflow: hidden;' border='0' src='http://cdn.flro.eu/full/LJTNmzVKV7L.jpg\'>"); 
        }
       
    }
}
