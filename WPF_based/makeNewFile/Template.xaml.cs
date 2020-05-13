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

namespace makeNewFile
{
    /// <summary>
    /// Template.xaml の相互作用ロジック
    /// </summary>
    public partial class Template : Window
    {
        public Template()
        {
            InitializeComponent();
        }

        

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            //Config config = new Config(ttt.Text);
            //config.Save();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            var senderElement = (Button)sender;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Media.Color selectedColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                BrushConverter brushConverter = new BrushConverter();
                senderElement.Background = (Brush)brushConverter.ConvertFromString(selectedColor.ToString());
            }
        }
    }

    public class Config{

        private MainWindow tt;

        public void Save()
        {
            Console.WriteLine(tt);
        }

        public Config(MainWindow mainWindow)
        {
            tt = mainWindow;
        }
    }

    /// <summary>
    /// コンバータークラス
    /// </summary>
    //[ValueConversion(typeof(string), typeof(string))]
    //public class ColorFormatConverter : IValueConverter
    //{
    //    public object Convert(object value, System.Type targetType,
    //      object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return ((string)value).Substring(1, 2);
    //    }

    //    public object ConvertBack(object value, System.Type targetType,
    //      object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return "#FF" + ((string)value).Substring(1);
    //    }
    //}
}
