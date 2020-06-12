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
    /// ProgressBarWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
        }

        public void SetProgressValue(double num)
        {
            this.Title = "実行中... " + Math.Floor(num).ToString() + "%";
            progress_bar.Value = Math.Floor(num);
        }
    }
}
