using System.Windows;
using System.Windows.Input;

namespace makeNewFile
{
    /// <summary>
    /// TemplateSub.xaml の相互作用ロジック
    /// </summary>
    public partial class TemplateSub : Window
    {
        public TemplateSub()
        {
            InitializeComponent();

            NewTemplateInfo newTemplateInfo = new NewTemplateInfo();
            newTemplateInfo.SetStatus(false);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (TemplateTitle.Text == "")
            {
                MessageBox.Show("テンプレートのタイトルを入力してください。", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NewTemplateInfo newTemplateInfo = new NewTemplateInfo();
            newTemplateInfo.SetStatus(true);
            newTemplateInfo.Set(TemplateTitle.Text, TemplateType.SelectedIndex);
            this.Close();
        }
    }
}
