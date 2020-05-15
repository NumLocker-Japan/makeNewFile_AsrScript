using Microsoft.Win32;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateConfigs templateConfigs = new TemplateConfigs(this);
            List<List<string>> setupInfo = new List<List<string>>(templateConfigs.Load());

            foreach(var templateInfo in setupInfo)
            {
                if (templateInfo.Count() == 5)
                {
                    AddTemplateItem(true, "Text", templateInfo[0], templateInfo[1], templateInfo[2], templateInfo[3], templateInfo[4]);
                }
                else
                {
                    AddTemplateItem(true, "Image", templateInfo[0], templateInfo[1], templateInfo[2], templateInfo[3], templateInfo[4], templateInfo[5]);
                }
            }

            TemplateRegConfigs templateRegConfigs = new TemplateRegConfigs();
            List<string> setupExtensionsInfo = templateRegConfigs.LoadExtensions();
            BMP_Extensions.Text = setupExtensionsInfo[0];
            GIF_Extensions.Text = setupExtensionsInfo[1];
            JPEG_Extensions.Text = setupExtensionsInfo[2];
            PNG_Extensions.Text = setupExtensionsInfo[3];
            TIFF_Extensions.Text = setupExtensionsInfo[4];
            WMP_Extensions.Text = setupExtensionsInfo[5];
            OfficeSpreadsheet_BIFF_Extensions.Text = setupExtensionsInfo[6];
            OfficeSpreadsheet_OOXML_Extensions.Text = setupExtensionsInfo[7];
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
            TemplateConfigs templateConfigs = new TemplateConfigs(this);
            if (templateConfigs.Export() == 0)
            {
                this.Close();
            }
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

            NewTemplateInfo newTemplateInfo = new NewTemplateInfo();
            if (newTemplateInfo.GetStatus())
            {
                AddTemplateItem();
            }
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

        private void AddTemplateItem(bool setup = false, string type = "", string p0 = "", string p1 = "",
                                                string p2 = "", string p3 = "", string p4 = "", string p5 = "")
        {
            System.Windows.Controls.GroupBox addingItem;

            if (setup)
            {
                if (type == "Text")
                {
                    // p0 = headerTitle
                    // p1 = isEnabled
                    // p2 = targetExtension
                    // p3 = defaultText
                    // p4 = charasetIndex
                    addingItem = AddTextTemplate(p0, totalCount, p1, p2, p3, int.Parse(p4));
                }
                else
                {
                    // p0 = headerTitle
                    // p1 = isEnabled
                    // p2 = targetExtension
                    // p3 = sizeX
                    // p4 = sizeY
                    // p5 = backgroundColor
                    addingItem = AddImageTemplate(p0, totalCount, p1, p2, int.Parse(p3), int.Parse(p4), p5);
                }
            }
            else
            {
                NewTemplateInfo newTemplateInfo = new NewTemplateInfo();
                List<string> info = newTemplateInfo.Get();
                if (int.Parse(info[1]) == 0)
                {
                    addingItem = AddTextTemplate(info[0], totalCount);

                }
                else
                {
                    addingItem = AddImageTemplate(info[0], totalCount);
                }
            }

            TemplatesField.Children.Add(addingItem);
            allTemplatesList.Add(addingItem);
            visualAllTemplatesListIndex.Add(totalCount);

            totalCount += 1;
        }

        private System.Windows.Controls.GroupBox AddTextTemplate(string title, int count, string isEnabled = "False",
                                                                     string targetExtension = "", string defaultText = "", int charasetIndex = 0)
        {
            ThicknessConverter thicknessConverter = new ThicknessConverter();

            System.Windows.Controls.GroupBox groupBox = new System.Windows.Controls.GroupBox();
            groupBox.Header = title;
            groupBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            groupBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");
            groupBox.Tag = "Text";

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

            if (isEnabled == "True")
            {
                isEnabledCheckbox.IsChecked = true;
            }
            else
            {
                isEnabledCheckbox.IsChecked = false;
            }

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
            extTextBox.Text = targetExtension;

            childStackPanel_02.Children.Add(extLabel);
            childStackPanel_02.Children.Add(extTextBox);


            System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
            textBox.AcceptsReturn = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            textBox.Height = 120;
            textBox.Text = defaultText;


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

            comboBox.SelectedIndex = charasetIndex;

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

        private System.Windows.Controls.GroupBox AddImageTemplate(string title, int count, string isEnabled = "False", string targetExtension = "",
                                                                     int sizeX = 100, int sizeY = 100, string backgroundColor = "#FFFFFFFF")
        {
            ThicknessConverter thicknessConverter = new ThicknessConverter();
            BrushConverter brushConverter = new BrushConverter();

            System.Windows.Controls.GroupBox groupBox = new System.Windows.Controls.GroupBox();
            groupBox.Header = title;
            groupBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            groupBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,10");
            groupBox.Tag = "Image";

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

            if (isEnabled == "True")
            {
                isEnabledCheckbox.IsChecked = true;
            }
            else
            {
                isEnabledCheckbox.IsChecked = false;
            }

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
            extTextBox.Text = targetExtension;

            childStackPanel_02.Children.Add(extLabel);
            childStackPanel_02.Children.Add(extTextBox);


            StackPanel childStackPanel_03 = new StackPanel();
            childStackPanel_03.Orientation = System.Windows.Controls.Orientation.Horizontal;
            childStackPanel_03.Margin = (Thickness)thicknessConverter.ConvertFromString("0");

            System.Windows.Controls.Label sizeLabel = new System.Windows.Controls.Label();
            sizeLabel.Content = "サイズ (x:y)";
            sizeLabel.Width = 90;
            sizeLabel.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox width_1TextBox = new System.Windows.Controls.TextBox();
            width_1TextBox.Width = 60;
            width_1TextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("10,0,0,0");
            width_1TextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            width_1TextBox.VerticalAlignment = VerticalAlignment.Center;
            width_1TextBox.VerticalContentAlignment = VerticalAlignment.Center;
            width_1TextBox.Text = sizeX.ToString();

            System.Windows.Controls.Label width_Label = new System.Windows.Controls.Label();
            width_Label.Content = "x";
            width_Label.VerticalAlignment = VerticalAlignment.Center;

            System.Windows.Controls.TextBox width_2TextBox = new System.Windows.Controls.TextBox();
            width_2TextBox.Width = 60;
            width_2TextBox.Margin = (Thickness)thicknessConverter.ConvertFromString("0,0,0,0");
            width_2TextBox.Padding = (Thickness)thicknessConverter.ConvertFromString("5,2");
            width_2TextBox.VerticalAlignment = VerticalAlignment.Center;
            width_2TextBox.VerticalContentAlignment = VerticalAlignment.Center;
            width_2TextBox.Text = sizeY.ToString();

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
            colorChangeButton.Background = (Brush)brushConverter.ConvertFromString(backgroundColor);
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

    public class TemplateConfigs{

        private Template tp;
        
        public TemplateConfigs(Template template)
        {
            tp = template;
        }

        public int Export()
        {
            List<List<string>> regInfo = new List<List<string>>();
            string tagList = "";    // タイプの種類を2進数で表す
            bool isOK = true;

            foreach (var childGroupBox in LogicalTreeHelper.GetChildren(tp.TemplatesField))
            {
                if (childGroupBox is DependencyObject)
                {
                    if (isOK && (string)((System.Windows.Controls.GroupBox)childGroupBox).Tag == "Text")
                    {
                        List<string> result = new List<string>();
                        result = GetTextGropBoxInfo((System.Windows.Controls.GroupBox)childGroupBox);
                        if (result.Count() != 0)
                        {
                            regInfo.Add(result);
                            tagList += "0";
                        }
                        else
                        {
                            isOK = false;
                        }
                        
                    }
                    else if (isOK && (string)((System.Windows.Controls.GroupBox)childGroupBox).Tag == "Image")
                    {
                        List<string> result = new List<string>();
                        result = GetImageGropBoxInfo((System.Windows.Controls.GroupBox)childGroupBox);
                        if (result.Count() != 0)
                        {
                            regInfo.Add(result);
                            tagList += "1";
                        }
                        else
                        {
                            isOK = false;
                        }
                    }
                }
            }

            if (isOK == false)
            {
                System.Windows.MessageBox.Show("エラーが発生したため、処理を中止しました。\nデータが正しく入力されていることを確認してください。", "エラー通知", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            // 入力の整合性を確認する。
            List<string> formatExtList = new List<string>();
            formatExtList.Add(tp.BMP_Extensions.Text);
            formatExtList.Add(tp.GIF_Extensions.Text);
            formatExtList.Add(tp.JPEG_Extensions.Text);
            formatExtList.Add(tp.PNG_Extensions.Text);
            formatExtList.Add(tp.TIFF_Extensions.Text);
            formatExtList.Add(tp.WMP_Extensions.Text);
            formatExtList.Add(tp.OfficeSpreadsheet_BIFF_Extensions.Text);
            formatExtList.Add(tp.OfficeSpreadsheet_OOXML_Extensions.Text);

            CheckValue checkValue = new CheckValue();
            // フォーマットに対応する拡張子の登録に二重登録されているものがないか確認
            List<string> res_01 = checkValue.CheckFormatRegistration(formatExtList);
            if (res_01.Count() != 0)
            {
                System.Windows.MessageBox.Show("同一の拡張子が複数のフォーマットに登録されています。\nデータを正しく修正してください。\n対象の拡張子 : " + string.Join(",", res_01), "エラー通知", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            // テンプレートの拡張子の整合性確認(有効化されているもののみ)
            // 画像フォーマットに関連付けられた拡張子は、テキストテンプレートとして登録されていてはならない。
            // その逆があってもならない。
            List<string> imageFormatExtList = new List<string>();
            imageFormatExtList.Add(tp.BMP_Extensions.Text);
            imageFormatExtList.Add(tp.GIF_Extensions.Text);
            imageFormatExtList.Add(tp.JPEG_Extensions.Text);
            imageFormatExtList.Add(tp.PNG_Extensions.Text);
            imageFormatExtList.Add(tp.TIFF_Extensions.Text);
            imageFormatExtList.Add(tp.WMP_Extensions.Text);

            List<string> res_02 = checkValue.CheckFormatsOfTemplates(regInfo, imageFormatExtList);
            if (res_02.Count() != 0)
            {
                System.Windows.MessageBox.Show("有効化されているテンプレートに登録されている拡張子と、フォーマットに登録されている拡張子の情報に差異があります。\nデータを正しく修正してください。\n対象のテンプレート : " + string.Join(",", res_02), "エラー通知", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            int tagListNum = Convert.ToInt32(tagList, 2);
            ExportToReg(regInfo, tagListNum);
            return 0;
        }

        private List<string> GetTextGropBoxInfo(System.Windows.Controls.GroupBox groupBoxItem)
        {
            List<string> _return = new List<string>();
            bool isOK = true;

            try
            {
                StackPanel parentStackPanel = (StackPanel)GetChildrenElements(groupBoxItem)[0];
                List<Object> childrenObject = new List<Object>(GetChildrenElements(parentStackPanel));

                string headerTitle = (string)groupBoxItem.Header;
                bool isEnabled = (bool)((System.Windows.Controls.CheckBox)GetChildrenElements(childrenObject[0])[2]).IsChecked;
                string targetExtension = ((System.Windows.Controls.TextBox)GetChildrenElements(childrenObject[1])[1]).Text;
                string defaultText = ((System.Windows.Controls.TextBox)childrenObject[2]).Text;
                int charasetIndex = ((System.Windows.Controls.ComboBox)GetChildrenElements(childrenObject[3])[1]).SelectedIndex;

                _return.Add(headerTitle.ToString());
                _return.Add(isEnabled.ToString());
                _return.Add(targetExtension);
                _return.Add(defaultText);
                _return.Add(charasetIndex.ToString());
            }
            catch (Exception)
            {
                isOK = false;
            }
            
            if (isOK)
            {
                return _return;
            }
            else
            {
                _return.Clear();
                return _return;
            }
        }

        private List<string> GetImageGropBoxInfo(System.Windows.Controls.GroupBox groupBoxItem)
        {
            List<string> _return = new List<string>();
            bool isOK = true;

            try
            {
                StackPanel parentStackPanel = (StackPanel)GetChildrenElements(groupBoxItem)[0];
                List<Object> childrenObject = new List<Object>(GetChildrenElements(parentStackPanel));

                string headerTitle = (string)groupBoxItem.Header;
                bool isEnabled = (bool)((System.Windows.Controls.CheckBox)GetChildrenElements(childrenObject[0])[2]).IsChecked;
                string targetExtension = ((System.Windows.Controls.TextBox)GetChildrenElements(childrenObject[1])[1]).Text;
                int sizeX = int.Parse(((System.Windows.Controls.TextBox)GetChildrenElements(childrenObject[2])[1]).Text);
                int sizeY = int.Parse(((System.Windows.Controls.TextBox)GetChildrenElements(childrenObject[2])[3]).Text);
                string backgroundColor = ((System.Windows.Controls.Button)GetChildrenElements(childrenObject[3])[1]).Background.ToString();

                _return.Add(headerTitle.ToString());
                _return.Add(isEnabled.ToString());
                _return.Add(targetExtension);
                _return.Add(sizeX.ToString());
                _return.Add(sizeY.ToString());
                _return.Add(backgroundColor);
            }
            catch (Exception)
            {
                isOK = false;
            }

            if (isOK)
            {
                return _return;
            }
            else
            {
                _return.Clear();
                return _return;
            }
        }

        private List<object> GetChildrenElements(object element)
        {
            List<object> _return = new List<object>();

            foreach (var childItem in LogicalTreeHelper.GetChildren((DependencyObject)element))
            {
                if (childItem is DependencyObject)
                {
                    _return.Add(childItem);
                }
            }

            return _return;
        }

        private void ExportToReg(List<List<string>> infoList, int tagList)
        {
            // Templatesキーを一度削除
            if (Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", false) != null)
            {
                RegistryKey parentRegTree = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\", true);
                parentRegTree.DeleteSubKeyTree("Templates");
                parentRegTree.Close();
            }

            RegistryKey regTemplates = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", true);

            int count = infoList.Count();
            regTemplates.SetValue("count", count);
            regTemplates.SetValue("tagList", tagList);

            for (int i = 0; i < count; i++)
            {
                string tagList_2 = Convert.ToString(tagList, 2).PadLeft(count, '0');
                if (tagList_2.Substring(i, 1) == "0")
                {
                    regTemplates.SetValue("headerTitle_" + i.ToString(), infoList[i][0]);
                    regTemplates.SetValue("isEnabled_" + i.ToString(), infoList[i][1]);
                    regTemplates.SetValue("targetExtension_" + i.ToString(), infoList[i][2]);
                    regTemplates.SetValue("defaultText_" + i.ToString(), infoList[i][3]);
                    regTemplates.SetValue("charasetIndex_" + i.ToString(), int.Parse(infoList[i][4]));
                }
                else
                {
                    regTemplates.SetValue("headerTitle_" + i.ToString(), infoList[i][0]);
                    regTemplates.SetValue("isEnabled_" + i.ToString(), infoList[i][1]);
                    regTemplates.SetValue("targetExtension_" + i.ToString(), infoList[i][2]);
                    regTemplates.SetValue("sizeX_" + i.ToString(), int.Parse(infoList[i][3]));
                    regTemplates.SetValue("sizeY_" + i.ToString(), int.Parse(infoList[i][4]));
                    regTemplates.SetValue("backgroundColor_" + i.ToString(), infoList[i][5]);
                }
            }

            regTemplates.SetValue("BmpExtensions", tp.BMP_Extensions.Text);
            regTemplates.SetValue("GifExtensions", tp.GIF_Extensions.Text);
            regTemplates.SetValue("JpegExtensions", tp.JPEG_Extensions.Text);
            regTemplates.SetValue("PngExtensions", tp.PNG_Extensions.Text);
            regTemplates.SetValue("TiffExtensions", tp.TIFF_Extensions.Text);
            regTemplates.SetValue("WmpExtensions", tp.WMP_Extensions.Text);
            regTemplates.SetValue("OfficeSpreadsheetBIFFExtensions", tp.OfficeSpreadsheet_BIFF_Extensions.Text);
            regTemplates.SetValue("OfficeSpreadsheetOOXMLExtensions", tp.OfficeSpreadsheet_OOXML_Extensions.Text);

            regTemplates.Close();
        }

        public List<List<string>> Load()
        {
            RegistryKey regTemplates = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", false);

            // レジストリから情報を取得
            int count = (int)regTemplates.GetValue("count");
            int tagList = (int)regTemplates.GetValue("tagList");
            List<List<string>> _return = new List<List<string>>();

            for (int i = 0; i < count; i++)
            {
                List<string> _return_child = new List<string>();

                string tagList_2 = Convert.ToString(tagList, 2).PadLeft(count, '0');
                if (tagList_2.Substring(i, 1) == "0")
                {
                    _return_child.Add((string)regTemplates.GetValue("headerTitle_" + i.ToString()));
                    _return_child.Add((string)regTemplates.GetValue("isEnabled_" + i.ToString()));
                    _return_child.Add((string)regTemplates.GetValue("targetExtension_" + i.ToString()));
                    _return_child.Add((string)regTemplates.GetValue("defaultText_" + i.ToString()));
                    _return_child.Add(regTemplates.GetValue("charasetIndex_" + i.ToString()).ToString());
                }
                else
                {
                    _return_child.Add((string)regTemplates.GetValue("headerTitle_" + i.ToString()));
                    _return_child.Add((string)regTemplates.GetValue("isEnabled_" + i.ToString()));
                    _return_child.Add((string)regTemplates.GetValue("targetExtension_" + i.ToString()));
                    _return_child.Add(regTemplates.GetValue("sizeX_" + i.ToString()).ToString());
                    _return_child.Add(regTemplates.GetValue("sizeY_"+ i.ToString()).ToString());
                    _return_child.Add((string)regTemplates.GetValue("backgroundColor_" + i.ToString()));
                }

                _return.Add(_return_child);
            }

            return _return;
        }
    }

    /// <summary>
    /// 外部からの呼び出しに対応するためにクラスを分ける
    /// </summary>
    public class TemplateRegConfigs {
        public List<string> LoadExtensions()
        {
            List<string> _return = new List<string>();
            RegistryKey regTemplates = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", false);

            _return.Add((string)regTemplates.GetValue("BmpExtensions"));
            _return.Add((string)regTemplates.GetValue("GifExtensions"));
            _return.Add((string)regTemplates.GetValue("JpegExtensions"));
            _return.Add((string)regTemplates.GetValue("PngExtensions"));
            _return.Add((string)regTemplates.GetValue("TiffExtensions"));
            _return.Add((string)regTemplates.GetValue("WmpExtensions"));
            _return.Add((string)regTemplates.GetValue("OfficeSpreadsheetBIFFExtensions"));
            _return.Add((string)regTemplates.GetValue("OfficeSpreadsheetOOXMLExtensions"));

            return _return;
        }
    }

    public class CheckValue
    {
        /// <summary>
        /// フォーマットと拡張子のペアの登録を検証
        /// </summary>
        public List<string> CheckFormatRegistration(List<string> extList)
        {
            List<string> _registeredExtension = new List<string>();
            List<string> unfairExt = new List<string>();

            for (int i = 0; i < extList.Count(); i++)
            {
                List<string> extList__childList = new List<string>(extList[i].Split(','));
                for (int j = 0; j < extList__childList.Count(); j++)
                {
                    if (_registeredExtension.Contains(extList__childList[j]))
                    {
                        unfairExt.Add(extList__childList[j]);
                    }
                    else
                    {
                        _registeredExtension.Add(extList__childList[j]);
                    }
                }
            }

            return unfairExt;
        }

        
        public List<string> CheckFormatsOfTemplates(List<List<string>> templateList, List<string> imageFormatExt)
        {
            List<string> unfair = new List<string>();

            for (int i = 0; i < templateList.Count(); i++)
            {
                // 有効化されているもののみ
                if (templateList[i][1] == "True")
                {
                    List<string> targetExtList = new List<string>(templateList[i][2].Split(','));
                    for (int j = 0; j < targetExtList.Count(); j++)
                    {
                        // テキストベースでかつ画像フォーマットである場合
                        if (templateList[i].Count() == 5 && imageFormatExt.Contains(targetExtList[j]))
                        {
                            unfair.Add(templateList[i][0]);
                        }
                        else if (templateList[i].Count() == 6 && !imageFormatExt.Contains(targetExtList[j]))
                        {
                            unfair.Add(templateList[i][0]);
                        }
                    }
                }
            }

            return unfair;
        }
    }

    /// <summary>
    /// テンプレートの追加
    /// TemplateSubクラスから情報を受け渡しする目的で使用
    /// </summary>
    public class NewTemplateInfo
    {
        private static string templateTitle;
        private static int templateType;
        private static bool status;

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

        public void SetStatus(bool st)
        {
            status = st;
        }

        /// <summary>
        /// TO DO : テンプレート追加が正常に終了したかを確認する
        /// </summary>
        /// <returns></returns>
        public bool GetStatus()
        {
            return status;
        }
    }
}
