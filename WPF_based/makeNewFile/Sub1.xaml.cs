using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
