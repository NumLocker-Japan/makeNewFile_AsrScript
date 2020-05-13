using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace makeNewFile
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // クリックイベント処理
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            StartProcess();
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Configs cfg = new Configs(this);
            cfg.Close(false);
            this.Close();
        }

        private void Body_KeyUp(object sender, KeyEventArgs e)
        {
            // Escキーで終了
            if (e.Key == Key.Escape)
            {
                Configs cfg = new Configs(this);
                cfg.Close(false);
                this.Close();
            }

            if (e.Key == Key.F1)
            {
                Process.Start("https://github.com/NumLocker-Japan/makeNewFile_AsrScript/wiki/Document_v3");  // GitHub Wikiのオンラインドキュメントに飛ばす
            }
        }

        // ×ボタンでの終了
        private void Body_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configs cfg = new Configs(this);
            cfg.Close(false);
        }

        private void Txtbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Return))
            {
                // Ctrl+Enterで改行
                if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
                {
                    int caretIndex = Txtbox.CaretIndex;
                    Txtbox.Text = Txtbox.Text.Insert(caretIndex, Environment.NewLine);
                    Txtbox.CaretIndex = caretIndex + Environment.NewLine.Length;
                }
                else
                {
                    // Enterキーの検知
                    if (e.ImeProcessedKey.ToString() == "None")  // IME確定の場合は、e.ImeProcessedKey.ToString() == "Return"となる
                    {
                        if (UseReturnToMoveFocus.IsChecked == true)
                        {
                            FocusNavigationDirection Direction = 0;
                            TraversalRequest MoveFocusRequest = new TraversalRequest(Direction);
                            UIElement ElementHavingFocus = Keyboard.FocusedElement as UIElement;  // フォーカス要素の取得
                            if (ElementHavingFocus != null)
                            {
                                ElementHavingFocus.MoveFocus(MoveFocusRequest);
                            }
                        }
                        else
                        {
                            e.Handled = true;  // キー処理が終了したことを明示し、処理終了後に改行を挿入させない
                            StartProcess();
                        }
                    }
                }
            }
        }

        private void CmnExt_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Return))
            {
                // 改行はない
                if (UseReturnToMoveFocus.IsChecked == true)
                {
                    FocusNavigationDirection Direction = 0;
                    TraversalRequest MoveFocusRequest = new TraversalRequest(Direction);
                    UIElement ElementHavingFocus = Keyboard.FocusedElement as UIElement;
                    if (ElementHavingFocus != null)
                    {
                        ElementHavingFocus.MoveFocus(MoveFocusRequest);
                    }
                }
                else
                {
                    StartProcess();
                }
            }
        }


        private void DefaultText_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Return))
            {
                if (UseReturnToMoveFocus.IsChecked == true)
                {
                    FocusNavigationDirection Direction = 0;
                    TraversalRequest MoveFocusRequest = new TraversalRequest(Direction);
                    UIElement ElementHavingFocus = Keyboard.FocusedElement as UIElement;
                    if (ElementHavingFocus != null)
                    {
                        ElementHavingFocus.MoveFocus(MoveFocusRequest);
                    }
                }
                else
                {
                    StartProcess();
                }
            }
        }

        private void Sub1_Button_Click(object sender, RoutedEventArgs e)
        {
            Sub1 sub1 = new Sub1();
            sub1.Show();
        }

        private void EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            Template template = new Template();
            template.Show();
        }

        private void Body_Activated(object sender, EventArgs e)
        {
            // 保存された設定の読み込み
            Configs cfg = new Configs(this);
            cfg.Launch();

            this.Title = "新規ファイル作成";

            AccessArgs accessArgs = new AccessArgs();

            // フォントサイズ設定
            if (accessArgs.ArgsList["fontSize"] != "")  // XAML側で13をデフォルトに設定している
            {
                this.FontSize = int.Parse(accessArgs.ArgsList["fontSize"]);
                this.TextEncoding.Width = int.Parse(accessArgs.ArgsList["fontSize"]) * 12;
                this.TextEncoding_utf8.Width = int.Parse(accessArgs.ArgsList["fontSize"]) * 12;
                this.TextEncoding_utf16.Width = int.Parse(accessArgs.ArgsList["fontSize"]) * 12;
                this.TextEncoding_sjis.Width = int.Parse(accessArgs.ArgsList["fontSize"]) * 12;
            }

            // アップデート確認 (終了を待たない。終わらずに終了した場合は次回持ち越し)
            _ = Task.Run(() => CheckForUpdate());
        }

        /// <summary>
        /// 処理開始
        /// </summary>
        private void StartProcess()
        {
            // Shiftキーが押されていた場合は、設定類を保存しない。
            bool SaveSettings = true;   // 設定をレジストリに残すかどうか
            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)  // Shiftキー判別
            {
                SaveSettings = false;
            }

            // メイン処理を投げる
            Exec exec = new Exec(this);
            string allErrors = exec.Start();

            if (allErrors != "")
            {
                MessageBox.Show("以下の場所でエラーが発生しました。\n" + allErrors);
            }

            Configs cfg = new Configs(this);
            cfg.Close(SaveSettings);
            if (CloseOnFinish.IsChecked == true)
            {
                this.Close();
            }
            else
            {
                // ウインドウを閉じない場合は、次の入力に備えて変数を初期化。
                // テキストボックスを空にし、終了の合図とする
                Txtbox.Text = "";
                CmnExt.Text = "";
                DefaultText.Text = "";
            }
        }

        private static HttpClient client = new HttpClient();
        /// <summary>
        /// アップデートの確認
        /// </summary>
        private async Task CheckForUpdate()
        {
            // 以下2項目はリリース用ビルド毎に設定
            string GitHubAPI_token = "fake";  // ビルド時のみ設定
            string version = "beta-3.0.6";  // バージョン
            RegistryKey config_reg_version = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\version", true);
            if (config_reg_version == null)
            {
                config_reg_version = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\version", true);
                config_reg_version.SetValue("version", version, RegistryValueKind.String);
                config_reg_version.SetValue("lastCheck", 0, RegistryValueKind.QWord);
                config_reg_version.SetValue("failCount", 0, RegistryValueKind.DWord);
            }
            else if ((string)config_reg_version.GetValue("version") != version)
            {
                config_reg_version.SetValue("version", version, RegistryValueKind.String);
            }

            DateTime time = DateTime.Now;
            var time_offset = new DateTimeOffset(time.Ticks, new TimeSpan(+09, 00, 00));
            // 約1週間毎にアップデートを確認
            AccessArgs accessArgs = new AccessArgs();
            if (accessArgs.ArgsList["disableCheckForUpdate"] == "true")
            {
                // アップデート確認を行わない事を明示的に示されている場合はパス
                return;
            }

            if (time_offset.ToUnixTimeSeconds() > (long)((long)config_reg_version.GetValue(@"lastCheck") + 600000))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, @"https://api.github.com/repos/NumLocker-Japan/makeNewFile_AsrScript/releases/latest");
                request.Headers.Add("User-Agent", "makeNewFile_AsrScript");
                request.Headers.Add("Authorization", "token " + GitHubAPI_token);
                var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    config_reg_version.SetValue("lastCheck", time_offset.ToUnixTimeSeconds(), RegistryValueKind.QWord);
                    config_reg_version.SetValue("failCount", 0, RegistryValueKind.DWord);
                    string responseText = await response.Content.ReadAsStringAsync();
                    var responseResult_obj = JObject.Parse(responseText);

                    // バージョン情報をキャッチ
                    if (version != (string)responseResult_obj["name"])
                    {
                        if (MessageBox.Show("更新があります。ダウンロードページを開きますか？\n使用中のバージョン : " + version + "\n最新のバージョン : " + (string)responseResult_obj["name"],
                            "アップデートの通知", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Process.Start(@"https://github.com/NumLocker-Japan/makeNewFile_AsrScript/releases");
                        }
                    }
                }
                else
                {
                    int failCount = (int)config_reg_version.GetValue("failCount");
                    if (failCount >= 9)
                    {
                        config_reg_version.SetValue("lastCheck", time_offset.ToUnixTimeSeconds(), RegistryValueKind.QWord);
                        config_reg_version.SetValue("failCount", 0, RegistryValueKind.DWord);
                        if (MessageBox.Show("10回以上連続でアップデートの確認に失敗しました。\n手動でアップデートの確認を行うことができます。\n確認を行いますか？\n使用中のバージョン : " + version,
                            "アップデート手動確認の通知", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Process.Start(@"https://github.com/NumLocker-Japan/makeNewFile_AsrScript/releases");
                        }
                    }
                    else
                    {
                        config_reg_version.SetValue("failCount", failCount + 1, RegistryValueKind.DWord);
                    }
                }
            }
            config_reg_version.Close();
        }
    }

    /// <summary>
    /// メイン処理を行うクラス
    /// </summary>
    public class Exec{
        private MainWindow mw;
        private string currentDirectory;
        private bool showDetailsOfErrors;
        private string commonExtension;
        private DateTime StartTime;
        private string[] splittedPathList;

        public Exec(MainWindow mainWindow){
            mw = mainWindow;
        }

        /// <summary>
        /// 日付・時刻を指定フォーマットで置換
        /// </summary>
        public static string ReplaceDate(string str, DateTime StartTime)
        {
            string[] deb_sam = str.Split('%');
            for (int i = 0; i < deb_sam.Length; i++)
            {
                if (i % 2 == 1)
                {
                    string TimeFormat = deb_sam[i];
                    deb_sam[i] = StartTime.ToString(TimeFormat);
                }
            }
            return String.Join("", deb_sam);
        }

        /// <summary>
        /// タイプを指定し、パスが存在するかを確認
        /// </summary>
        public static bool PathIsExsist(string path, string type)
        {
            if (type == "directory")
            {
                return (Directory.Exists(path));
            }

            if (type == "file")
            {
                return (File.Exists(path));
            }

            return false;
        }

        public string Start(){
            // コマンドライン引数を取得
            AccessArgs accessArgs = new AccessArgs();
            currentDirectory = accessArgs.ArgsList["currentDirectory"];
            // エラー処理の用意
            showDetailsOfErrors = false;
            if (accessArgs.ArgsList["showDetailsOfErrors"] == "true")
            {
                showDetailsOfErrors = true;
            }
            // 共通拡張子の設定
            commonExtension = "";
            if (!Regex.IsMatch(mw.CmnExt.Text, @"^ *$"))
            {
                commonExtension = '.' + mw.CmnExt.Text;
            }
            // 日付・時刻の取得
            StartTime = DateTime.Now;

            string[] pathListSeparator = new string[] { "\r\n" };
            splittedPathList = mw.Txtbox.Text.Split(pathListSeparator, StringSplitOptions.RemoveEmptyEntries);



            // RunMakeFile()に処理を投げる。処理完了まで返らない。
            List<string> CatchedErrors = RunMakeFile();
            // 取得したエラーをまとめて処理
            if (CatchedErrors.Count() > 0)
            {
                string allErrors = "";
                for (int i = 0; i < CatchedErrors.Count(); i++)
                {
                    if (showDetailsOfErrors)
                    {
                        allErrors = allErrors + "\n> ( " + (i + 1).ToString() + " ) --- --- --- --- --- --- --- --- ---\n" + CatchedErrors[i];
                    }
                    else
                    {
                        allErrors = allErrors + "\n" + CatchedErrors[i];
                    }
                }

                // 整形済みのエラー一覧を返す。
                return allErrors;
            }

            return "";
        }

        private List<string> RunMakeFile()
        {
            AccessArgs accessArgs = new AccessArgs();
            var AllErrors = new List<string>();
            for (int i = 0; i < splittedPathList.Length; i++)
            {
                string FormattedPathList = ReplaceDate(splittedPathList[i], StartTime);
                if (Regex.IsMatch(FormattedPathList, @"^([^$*~]*\$+)+[^$*~]*\*[1-9]\d*(|~[1-9]\d*)$"))
                {
                    string name_part = FormattedPathList.Split('*')[0];
                    string number_part = FormattedPathList.Split('*')[1];
                    int number_part__number = int.Parse(number_part.Split('~')[0]);
                    int number_part__offset;
                    if (number_part.Split('~').Length == 1)
                    {
                        if (mw.StartFromZero.IsChecked == true)
                        {
                            number_part__offset = 0;
                        }
                        else
                        {
                            number_part__offset = 1;
                        }
                    }
                    else
                    {
                        number_part__offset = int.Parse(number_part.Split('~')[1]);
                    }

                    // $記号で区切る
                    List<string> name_part__name_List = new List<string>(Regex.Split(name_part, @"\$+"));
                    List<string> name_part__number_List = new List<string>(Regex.Split(name_part, @"[^$]+"));
                    // 区切りの端には空文字が入るので、number_Listにあった場合のみ除去
                    if (name_part__number_List[0] == "") {
                        name_part__number_List.RemoveAt(0);
                    }
                    if (name_part__number_List.Last() == "")
                    {
                        name_part__number_List.RemoveAt(name_part__number_List.Count() - 1);
                    }

                    // 最小桁の確認
                    List<string> name_part__number_List__CheckDigit = new List<string>(name_part__number_List);
                    name_part__number_List__CheckDigit.RemoveAll(x => x == "");
                    name_part__number_List__CheckDigit.Sort();
                    int min_digit = name_part__number_List__CheckDigit[0].Length;

                    if (mw.ZeroPadding.IsChecked == true && (number_part__offset + number_part__number - 1).ToString().Length > min_digit)
                    {
                        AllErrors.Add("連番 - 桁の不足 - 数字が大きすぎます。 : " + FormattedPathList);
                        continue;
                    }

                    if (int.Parse(accessArgs.ArgsList["alertManyItems"]) != 0 && int.Parse(accessArgs.ArgsList["alertManyItems"]) <= number_part__number)
                    {
                        if (MessageBox.Show("大量のファイルまたはフォルダを作成しようとしています。\n処理に時間がかかります。続けますか？\n\n対象 : " + FormattedPathList,
                            "続行の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            continue;
                        }
                    }

                    // 1つめの項目でエラーが出た場合、残る要素もエラーが出ることが必至なため、エラーを検知した場合はbreak
                    string FirstTest = CallMakeFile(number_part__offset, name_part__name_List, name_part__number_List);
                    if (FirstTest != "")
                    {
                        AllErrors.Add("エラーを検知したため、次の連番処理は中止されました。" + splittedPathList[i]);  //整形済みのパスではなく、元の表現を表示
                        continue;
                    }

                    int Counter = 1;  //作成済み項目数をカウント。テストとして1つ作成済みなので、カウンターは1スタート
                    int StartNumber = number_part__offset + 1;
                    while (true)
                    {
                        string result = "";

                        if (Counter >= number_part__number)
                        {
                            break;
                        }

                        result = CallMakeFile(StartNumber, name_part__name_List, name_part__number_List); ;

                        Counter += 1;
                        StartNumber += 1;
                    }
                }
                else
                {
                    string err = MakeFile(FormattedPathList);
                    if (err != "")
                    {
                        AllErrors.Add(err);
                    }
                }
            }
            return AllErrors;
        }

        private string CallMakeFile(int number, List<string> name_part__name_List, List<string> name_part__number_List)
        {
            string formatted = "";

            for (int k = 0; k < name_part__name_List.Count(); k++)
            {
                if (k + 1 == name_part__name_List.Count())
                {
                    formatted += name_part__name_List[k];
                    break;
                }

                // name_part__number_Listは、両端に必ず""を含むため、k+1にアクセスする事でこれを回避する。
                if (mw.ZeroPadding.IsChecked == true)
                {
                    formatted = formatted + name_part__name_List[k] + number.ToString().PadLeft(name_part__number_List[k].Length, '0');
                }
                else
                {
                    formatted = formatted + name_part__name_List[k] + number.ToString();
                }
            }
            string err = MakeFile(formatted);
            if (err != "")
            {
                return (err);
            }
            return "";
        }

        private string MakeFile(string path)
        {
            string targetPath = currentDirectory + "\\" + path;
            string[] splittedPath = Regex.Split(targetPath, @"(\\|\/)");
            int len = splittedPath.Length;
            string fileName = splittedPath[len - 1];
            string fullPath = String.Join("\\", splittedPath);
            string directoryPath;
            if (fullPath != fileName)
            {
                directoryPath = fullPath.Substring(0, fullPath.Length - (1 + fileName.Length));
            }
            else
            {
                directoryPath = "";
            }

            // ディレクトリ部分とファイル部分を分割
            if (directoryPath != "" && !PathIsExsist(directoryPath, "directory"))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }

                catch (Exception ex)
                {
                    if (showDetailsOfErrors)
                    {
                        return ("ディレクトリ : " + directoryPath + "\n詳細 : \n" + ex.ToString());
                    }
                    else
                    {
                        return ("ディレクトリ : " + directoryPath);
                    }
                }
            }

            // ディレクトリのみの作成も可能にする。(\で終わるパスを許可)
            if (fileName != "")
            {
                if (!PathIsExsist(fullPath + commonExtension, "file"))
                {
                    try
                    {
                        using (FileStream fs = File.Create(fullPath + commonExtension))
                        {
                            byte[] contents;
                            if (mw.TextEncoding.SelectedIndex == 0)
                            {
                                contents = new UTF8Encoding().GetBytes(mw.DefaultText.Text);
                            }
                            else if (mw.TextEncoding.SelectedIndex == 1)
                            {
                                contents = new UnicodeEncoding().GetBytes(mw.DefaultText.Text);
                            }
                            else
                            {
                                Encoding s_jis = Encoding.GetEncoding(932);
                                contents = s_jis.GetBytes(mw.DefaultText.Text);
                            }

                            fs.Write(contents, 0, contents.Length);
                        }
                    }

                    catch (Exception ex)
                    {
                        if (showDetailsOfErrors)
                        {
                            return ("ファイル : " + fullPath + commonExtension + "\n詳細 : \n" + ex.ToString());
                        }
                        else
                        {
                            return ("ファイル : " + fullPath + commonExtension);
                        }
                    }
                }
            }

            return "";
        }
    }


    /// <summary>
    /// 設定に関わる
    /// </summary>
    public class Configs
    {
        private MainWindow mw;
        public Configs(MainWindow mainWindow){
            mw = mainWindow;
        }

        ///  <summary>
        /// レジストリ関連の設定を行う
        /// </summary>
        public void Launch()
        {
            RegistryKey config_reg_window = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\config", true);
            if (config_reg_window == null)
            {
                // 初期値の設定
                config_reg_window = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\config", true);
                config_reg_window.SetValue("WindowHeight", 660, RegistryValueKind.DWord);
                config_reg_window.SetValue("WindowWidth", 960, RegistryValueKind.DWord);
                config_reg_window.SetValue("StartFromZero", "False", RegistryValueKind.String);
                config_reg_window.SetValue("ZeroPadding", "True", RegistryValueKind.String);
                config_reg_window.SetValue("UseReturnToMoveFocus", "False", RegistryValueKind.String);
                config_reg_window.SetValue("CloseOnFinish", "True", RegistryValueKind.String);
                config_reg_window.SetValue("TextEncodingIndex", 0, RegistryValueKind.DWord);
            }
            int Win_height = (int)config_reg_window.GetValue("WindowHeight");
            int Win_width = (int)config_reg_window.GetValue("WindowWidth");
            bool StartFromZero, ZeroPadding, UseReturnToMoveFocus, CloseOnFinish;
            if ((string)config_reg_window.GetValue("StartFromZero") == "True")
            {
                StartFromZero = true;
            }
            else
            {
                StartFromZero = false;
            }
            if ((string)config_reg_window.GetValue("ZeroPadding") == "True")
            {
                ZeroPadding = true;
            }
            else
            {
                ZeroPadding = false;
            }
            if ((string)config_reg_window.GetValue("UseReturnToMoveFocus") == "True")
            {
                UseReturnToMoveFocus = true;
            }
            else
            {
                UseReturnToMoveFocus = false;
            }
            if ((string)config_reg_window.GetValue("CloseOnFinish") == "True")
            {
                CloseOnFinish = true;
            }
            else
            {
                CloseOnFinish = false;
            }
            int TextEncodingIndex = (int)config_reg_window.GetValue("TextEncodingIndex");

            config_reg_window.Close();
            mw.Height = Win_height;
            mw.Width = Win_width;
            mw.StartFromZero.IsChecked = StartFromZero;
            mw.ZeroPadding.IsChecked = ZeroPadding;
            mw.UseReturnToMoveFocus.IsChecked = UseReturnToMoveFocus;
            mw.CloseOnFinish.IsChecked = CloseOnFinish;
            mw.TextEncoding.SelectedIndex = TextEncodingIndex;
            mw.Top = (SystemParameters.PrimaryScreenHeight - Win_height) / 2;
            mw.Left = (SystemParameters.PrimaryScreenWidth - Win_width) / 2;
        }

        public void Close(bool saveSettings)
        {
            RegistryKey config_reg_window = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\config", true);

            config_reg_window.SetValue("WindowHeight", (int)mw.Height, RegistryValueKind.DWord);
            config_reg_window.SetValue("WindowWidth", (int)mw.Width, RegistryValueKind.DWord);
            if (saveSettings)
            {
                config_reg_window.SetValue("StartFromZero", mw.StartFromZero.IsChecked.ToString(), RegistryValueKind.String);
                config_reg_window.SetValue("ZeroPadding", mw.ZeroPadding.IsChecked.ToString(), RegistryValueKind.String);
                config_reg_window.SetValue("UseReturnToMoveFocus", mw.UseReturnToMoveFocus.IsChecked.ToString(), RegistryValueKind.String);
                config_reg_window.SetValue("CloseOnFinish", mw.CloseOnFinish.IsChecked.ToString(), RegistryValueKind.String);
                config_reg_window.SetValue("TextEncodingIndex", mw.TextEncoding.SelectedIndex, RegistryValueKind.DWord);
            }
        }
    }
}