using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
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
        int totalCount = 0;
        List<System.Windows.Controls.GroupBox> allTemplatesList = new List<System.Windows.Controls.GroupBox>();     // テンプレートは、GroupBoxのリストとして状態を管理
        List<int> visualAllTemplatesListIndex = new List<int>();    // 見た目順にテンプレートの通し番号が並びます。

        public Template()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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
            var senderElement = (System.Windows.Controls.Button)sender;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Media.Color selectedColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                BrushConverter brushConverter = new BrushConverter();
                senderElement.Background = (Brush)brushConverter.ConvertFromString(selectedColor.ToString());
            }
        }

        private void AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            TemplateSub templateSub = new TemplateSub();
            _ = templateSub.ShowDialog();
            AddTemplateItem();
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            var senderElement = (System.Windows.Controls.Button)sender;
            List<string> allTemplatesTitle = new List<string>();
            foreach (var child in LogicalTreeHelper.GetChildren(TemplatesField))
            {
                if (child is DependencyObject)
                {
                    allTemplatesTitle.Add(((System.Windows.Controls.GroupBox)child).Header.ToString());
                }
            }

            if (System.Windows.MessageBox.Show("テンプレート「" + allTemplatesTitle[visualAllTemplatesListIndex.FindIndex(x => x == int.Parse(senderElement.Name.Substring(10)))] + "」を削除してもよろしいですか？", "削除の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                visualAllTemplatesListIndex.Remove(int.Parse(senderElement.Name.Substring(10)));
                RefreshTemplates();
            }
        }


        /// <summary>
        /// 要素を1つ上に移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToUp(object sender, RoutedEventArgs e)
        {
            var senderElement = (System.Windows.Controls.Button)sender;
            int itemindex = visualAllTemplatesListIndex.FindIndex(x => x == int.Parse(senderElement.Name.Substring(9)));
            // 既に一番上にある場合は、return
            if (itemindex == 0)
            {
                return;
            }
            visualAllTemplatesListIndex.RemoveAt(itemindex);
            visualAllTemplatesListIndex.Insert(itemindex - 1, int.Parse(senderElement.Name.Substring(9)));

            RefreshTemplates();
        }

        private void MoveToBottom(object sender, RoutedEventArgs e)
        {
            var senderElement = (System.Windows.Controls.Button)sender;
            int itemindex = visualAllTemplatesListIndex.FindIndex(x => x == int.Parse(senderElement.Name.Substring(13)));
            // 既に一番下にある場合は、return
            if (itemindex == visualAllTemplatesListIndex.Count() - 1)
            {
                return;
            }

            // 下から2番目にあるときは、末尾に追加(エラー回避)
            if (itemindex == visualAllTemplatesListIndex.Count() - 2)
            {
                visualAllTemplatesListIndex.RemoveAt(itemindex);
                visualAllTemplatesListIndex.Add(int.Parse(senderElement.Name.Substring(13)));
            }
            else
            {
                visualAllTemplatesListIndex.RemoveAt(itemindex);
                visualAllTemplatesListIndex.Insert(itemindex + 1, int.Parse(senderElement.Name.Substring(13)));
            }

            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            TemplatesField.Children.Clear();
            foreach (int templateIndex in visualAllTemplatesListIndex)
            {
                TemplatesField.Children.Add(allTemplatesList[templateIndex]);
            }
        }



        // テンプレート追加

        private void AddTemplateItem()
        {
            NewTemplateInfo newTemplateInfo = new NewTemplateInfo();
            List<string> info = newTemplateInfo.Get();
            System.Windows.Controls.GroupBox addingItem;
            if (int.Parse(info[1]) == 0)
            {
                addingItem = AddTextTemplate(info[0], totalCount);
                
            }
            else
            {
                addingItem = AddImageTemplate(info[0], totalCount);
            }

            TemplatesField.Children.Add(addingItem);
            allTemplatesList.Add(addingItem);
            visualAllTemplatesListIndex.Add(totalCount);

            totalCount += 1;
        }

        private System.Windows.Controls.GroupBox AddTextTemplate(string title, int count)
        {
            ThicknessConverter thicknessConverter = new ThicknessConverter();

            System.Windows.Controls.GroupBox groupBox = new System.Windows.Controls.GroupBox();
            groupBox.Header = title;
            groupBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            groupBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            StackPanel parentStackPanel = new StackPanel();
            parentStackPanel.Margin = (Thickness)thicknessConverter.ConvertFromString("15");


            StackPanel childStackPanel_01 = new StackPanel();
            childStackPanel_01.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_01.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            System.Windows.Controls.Label typeLabel = new System.Windows.Controls.Label();
            typeLabel.Content = "種類";
            typeLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.Label textLabel = new System.Windows.Controls.Label();
            textLabel.Content = "テキスト";

            System.Windows.Controls.CheckBox isEnabledCheckbox = new System.Windows.Controls.CheckBox();
            isEnabledCheckbox.Content = "有効";
            isEnabledCheckbox.Margin = (Thickness)thicknessConverter.ConvertFromString("50,0,0,0");
            isEnabledCheckbox.VerticalAlignment = VerticalAlignment.Center;

            childStackPanel_01.Children.Add(typeLabel);
            childStackPanel_01.Children.Add(textLabel);
            childStackPanel_01.Children.Add(isEnabledCheckbox);


            StackPanel childStackPanel_02 = new StackPanel();
            childStackPanel_02.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_02.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            System.Windows.Controls.Label extLabel = new System.Windows.Controls.Label();
            extLabel.Content = "対象拡張子";
            extLabel.Width = 90;
            extLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox extTextBox = new System.Windows.Controls.TextBox();
            extTextBox.Width = 200;
            extTextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            extTextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            extTextBox.VerticalAlignment = VerticalAlignment.Center;
            extTextBox.VerticalContentAlignment = VerticalAlignment.Center;

            childStackPanel_02.Children.Add(extLabel);
            childStackPanel_02.Children.Add(extTextBox);


            System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
            textBox.AcceptsReturn = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            textBox.Height = 120;


            StackPanel childStackPanel_03 = new StackPanel();
            childStackPanel_03.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_03.Margin = (Thickness)thicknessConverter.ConvertFromString("0,10,0,0");

            System.Windows.Controls.Label charasetLabel = new System.Windows.Controls.Label();
            charasetLabel.Content = "文字コード";
            charasetLabel.Width = 90;
            charasetLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.ComboBox comboBox = new System.Windows.Controls.ComboBox();
            comboBox.SelectedIndex = 0;
            comboBox.Width = 100;
            comboBox.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            comboBox.VerticalAlignment = VerticalAlignment.Center;

            ComboBoxItem comboBoxItem_01 = new ComboBoxItem();
            comboBoxItem_01.Content = "UTF-8";
            ComboBoxItem comboBoxItem_02 = new ComboBoxItem();
            comboBoxItem_02.Content = "UTF-16";
            ComboBoxItem comboBoxItem_03 = new ComboBoxItem();
            comboBoxItem_03.Content = "Shift_JIS";

            comboBox.Items.Add(comboBoxItem_01);
            comboBox.Items.Add(comboBoxItem_02);
            comboBox.Items.Add(comboBoxItem_03);

            childStackPanel_03.Children.Add(charasetLabel);
            childStackPanel_03.Children.Add(comboBox);


            StackPanel childStackPanel_04 = new StackPanel();
            childStackPanel_04.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_04.Margin = (Thickness)thicknessConverter.ConvertFromString("0,15,0,0");

            System.Windows.Controls.Button delButton = new System.Windows.Controls.Button();
            delButton.Content = "テンプレートを削除";
            delButton.Name = "delButton_" + count.ToString();

            delButton.SetResourceReference(System.Windows.Controls.Control.TemplateProperty, "delButton");

            delButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            delButton.Width = 120;
            delButton.Height = 25;
            delButton.Click += DeleteTemplate_Click;

            System.Windows.Controls.Button moveUpButton = new System.Windows.Controls.Button();
            moveUpButton.Name = "moveToUp_" + count.ToString();
            moveUpButton.Content = "↑";
            moveUpButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            moveUpButton.Margin = (Thickness)thicknessConverter.ConvertFromString("90,0,0,0");
            moveUpButton.Width = 40;
            moveUpButton.Height = 25;
            moveUpButton.Click += MoveToUp;

            System.Windows.Controls.Button moveBottomButton = new System.Windows.Controls.Button();
            moveBottomButton.Name = "moveToBottom_" + count.ToString();
            moveBottomButton.Content = "↓";
            moveBottomButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            moveBottomButton.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            moveBottomButton.Width = 40;
            moveBottomButton.Height = 25;
            moveBottomButton.Click += MoveToBottom;

            childStackPanel_04.Children.Add(delButton);
            childStackPanel_04.Children.Add(moveUpButton);
            childStackPanel_04.Children.Add(moveBottomButton);


            parentStackPanel.Children.Add(childStackPanel_01);
            parentStackPanel.Children.Add(childStackPanel_02);
            parentStackPanel.Children.Add(textBox);
            parentStackPanel.Children.Add(childStackPanel_03);
            parentStackPanel.Children.Add(childStackPanel_04);

            groupBox.Content = parentStackPanel;

            return groupBox;
        }

        private System.Windows.Controls.GroupBox AddImageTemplate(string title, int count)
        {
            ThicknessConverter thicknessConverter = new ThicknessConverter();
            BrushConverter brushConverter = new BrushConverter();

            System.Windows.Controls.GroupBox groupBox = new System.Windows.Controls.GroupBox();
            groupBox.Header = title;
            groupBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            groupBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            StackPanel parentStackPanel = new StackPanel();
            parentStackPanel.Margin = (Thickness)thicknessConverter.ConvertFromString("15");


            StackPanel childStackPanel_01 = new StackPanel();
            childStackPanel_01.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_01.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            System.Windows.Controls.Label typeLabel = new System.Windows.Controls.Label();
            typeLabel.Content = "種類";
            typeLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.Label textLabel = new System.Windows.Controls.Label();
            textLabel.Content = "画像";

            System.Windows.Controls.CheckBox isEnabledCheckbox = new System.Windows.Controls.CheckBox();
            isEnabledCheckbox.Content = "有効";
            isEnabledCheckbox.Margin = (Thickness)thicknessConverter.ConvertFromString("50,0,0,0");
            isEnabledCheckbox.VerticalAlignment = VerticalAlignment.Center;

            childStackPanel_01.Children.Add(typeLabel);
            childStackPanel_01.Children.Add(textLabel);
            childStackPanel_01.Children.Add(isEnabledCheckbox);


            StackPanel childStackPanel_02 = new StackPanel();
            childStackPanel_02.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_02.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");

            System.Windows.Controls.Label extLabel = new System.Windows.Controls.Label();
            extLabel.Content = "対象拡張子";
            extLabel.Width = 90;
            extLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox extTextBox = new System.Windows.Controls.TextBox();
            extTextBox.Width = 200;
            extTextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            extTextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            extTextBox.VerticalAlignment = VerticalAlignment.Center;
            extTextBox.VerticalContentAlignment = VerticalAlignment.Center;

            childStackPanel_02.Children.Add(extLabel);
            childStackPanel_02.Children.Add(extTextBox);


            StackPanel childStackPanel_03 = new StackPanel();
            childStackPanel_03.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_03.Margin = (Thickness)thicknessConverter.ConvertFromString("0");

            System.Windows.Controls.Label sizeLabel = new System.Windows.Controls.Label();
            sizeLabel.Content = "サイズ";
            sizeLabel.Width = 90;
            sizeLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox width_1TextBox = new System.Windows.Controls.TextBox();
            width_1TextBox.Width = 60;
            width_1TextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            width_1TextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            width_1TextBox.VerticalAlignment = VerticalAlignment.Center;
            width_1TextBox.VerticalContentAlignment = VerticalAlignment.Center;

            System.Windows.Controls.Label width_Label = new System.Windows.Controls.Label();
            width_Label.Content = "x";
            width_Label.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox width_2TextBox = new System.Windows.Controls.TextBox();
            width_2TextBox.Width = 60;
            width_2TextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,0");
            width_2TextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            width_2TextBox.VerticalAlignment = VerticalAlignment.Center;
            width_2TextBox.VerticalContentAlignment = VerticalAlignment.Center;

            childStackPanel_03.Children.Add(sizeLabel);
            childStackPanel_03.Children.Add(width_1TextBox);
            childStackPanel_03.Children.Add(width_Label);
            childStackPanel_03.Children.Add(width_2TextBox);


            StackPanel childStackPanel_04 = new StackPanel();
            childStackPanel_04.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_04.Margin = (Thickness)thicknessConverter.ConvertFromString("0,10,0,0");

            System.Windows.Controls.Label bgcLabel = new System.Windows.Controls.Label();
            bgcLabel.Content = "背景色";
            bgcLabel.Width = 90;
            bgcLabel.VerticalAlignment = VerticalAlignment.Center;

            //System.Windows.Controls.Label colorNameLabel = new System.Windows.Controls.Label();

            //System.Windows.Data.Binding colorNameLabelBinding = new System.Windows.Data.Binding();
            //colorNameLabelBinding.Path = new PropertyPath("Background");
            //colorNameLabelBinding.ElementName = "T" + count.ToString() + "_ColorSample";
            //colorNameLabel.SetBinding(ContentProperty, colorNameLabelBinding);

            //colorNameLabel.Margin = (Thickness)thicknessConverter.ConvertFromString("15,0,0,0");
            //colorNameLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.Button colorChangeButton = new System.Windows.Controls.Button();
            colorChangeButton.Name = "T" + count.ToString() + "_ColorSample";
            colorChangeButton.Content = "";
            colorChangeButton.Background = (Brush)brushConverter.ConvertFromString("Blue");
            colorChangeButton.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            colorChangeButton.Width = 40;
            colorChangeButton.SetResourceReference(System.Windows.Controls.Control.TemplateProperty, "colorButton");
            colorChangeButton.VerticalAlignment = VerticalAlignment.Center;
            colorChangeButton.Click += ChangeColor;

            childStackPanel_04.Children.Add(bgcLabel);
            //childStackPanel_04.Children.Add(colorNameLabel);
            childStackPanel_04.Children.Add(colorChangeButton);

            StackPanel childStackPanel_05 = new StackPanel();
            childStackPanel_05.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_05.Margin = (Thickness)thicknessConverter.ConvertFromString("0,15,0,0");

            System.Windows.Controls.Button delButton = new System.Windows.Controls.Button();
            delButton.Content = "テンプレートを削除";
            delButton.Name = "delButton_" + count.ToString();

            delButton.SetResourceReference(System.Windows.Controls.Control.TemplateProperty, "delButton");

            delButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            delButton.Width = 120;
            delButton.Height = 25;
            delButton.Click += DeleteTemplate_Click;

            System.Windows.Controls.Button moveUpButton = new System.Windows.Controls.Button();
            moveUpButton.Name = "moveToUp_" + count.ToString();
            moveUpButton.Content = "↑";
            moveUpButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            moveUpButton.Margin = (Thickness)thicknessConverter.ConvertFromString("90,0,0,0");
            moveUpButton.Width = 40;
            moveUpButton.Height = 25;
            moveUpButton.Click += MoveToUp;

            System.Windows.Controls.Button moveBottomButton = new System.Windows.Controls.Button();
            moveBottomButton.Name = "moveToBottom_" + count.ToString();
            moveBottomButton.Content = "↓";
            moveBottomButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            moveBottomButton.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            moveBottomButton.Width = 40;
            moveBottomButton.Height = 25;
            moveBottomButton.Click += MoveToBottom;

            childStackPanel_05.Children.Add(delButton);
            childStackPanel_05.Children.Add(moveUpButton);
            childStackPanel_05.Children.Add(moveBottomButton);


            parentStackPanel.Children.Add(childStackPanel_01);
            parentStackPanel.Children.Add(childStackPanel_02);
            parentStackPanel.Children.Add(childStackPanel_03);
            parentStackPanel.Children.Add(childStackPanel_04);
            parentStackPanel.Children.Add(childStackPanel_05);

            groupBox.Content = parentStackPanel;

            return groupBox;
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
    /// テンプレートの追加
    /// </summary>
    public class NewTemplateInfo
    {
        private static string templateTitle;
        private static int templateType;

        public void Set(string title, int type)
        {
            templateTitle = title;
            templateType = type;
        }

        public List<string> Get()
        {
            List<string> _return = new List<string>();
            _return.Add(templateTitle);
            _return.Add(templateType.ToString());
            return _return;
        }
    }
}
