using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace makeNewFile
{
    /// <summary>
    /// Sub1.xaml の相互作用ロジック
    /// </summary>
    public partial class Sub1 : Window
    {
        public Sub1()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(@"https://docs.microsoft.com/ja-jp/dotnet/api/system.globalization.datetimeformatinfo?view=netframework-4.7.2");
        }
    }
}
