using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace makeNewFile
{
    public partial class MainWindow : Window
    {
        bool StartUp = true;

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

        private void Body_PreviewKeyUp(object sender, KeyEventArgs e)
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

        private void Body_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 置換機能
            if (Keyboard.IsKeyDown(Key.R) && (Keyboard.Modifiers & ModifierKeys.Alt) > 0)
            {
                Sub1 sub1 = new Sub1();
                sub1.Show();
            }

            // テンプレート編集
            if (Keyboard.IsKeyDown(Key.T) && (Keyboard.Modifiers & ModifierKeys.Alt) > 0)
            {
                Template template = new Template();
                template.ShowDialog();
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
                            e.Handled = true;   // キー処理を明示的に終了

                            FocusNavigationDirection Direction = FocusNavigationDirection.Next;
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
                    e.Handled = true;

                    FocusNavigationDirection Direction = FocusNavigationDirection.Next;
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
                    e.Handled = true;

                    FocusNavigationDirection Direction = FocusNavigationDirection.Next;
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
            template.ShowDialog();
        }

        private void Body_Activated(object sender, EventArgs e)
        {
            if (StartUp == true)
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

                StartUp = false;

                // アップデート確認 (終了を待たない。終わらずに終了した場合は次回持ち越し)
                _ = Task.Run(() => CheckForUpdate());
            }
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
            RegistryKey config_reg_version = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Version", true);
            if (config_reg_version == null)
            {
                config_reg_version = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\Version", true);
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
        private string subInfo;     // 画像サイズ・シート名
        private List<string> template;

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
                    // name_part__number_Listの要素数 = name_part__name_Listの要素数の要素数 -1 が常に成立するようにする
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

                    // フォーマット判別
                    // @より後に$を入れると、この辺りの処理が上手くいかない
                    if (name_part__name_List.Last().Split('@').Length == 1)
                    {
                        subInfo = "";
                    }
                    else
                    {
                        subInfo = name_part__name_List.Last().Split('@')[1];
                        name_part__name_List[name_part__name_List.Count() - 1] = name_part__name_List.Last().Split('@')[0];
                    }

                    // 拡張子で呼び出しを場合分け
                    AvailableTemplates availableTemplates = new AvailableTemplates();
                    int index;
                    if (commonExtension == "")
                    {
                        // name_part__name_List.Last()は""である可能性があるので、回避する
                        if (name_part__name_List.Last() == "")      // 拡張子がない場合なので、設定しない
                        {
                            index = -1;
                            template = new List<string>();
                        }
                        else
                        {
                            if (Path.GetExtension(name_part__name_List.Last()) == "")     // 拡張子が存在しない場合
                            {
                                index = -1;
                                template = new List<string>();
                            }
                            else
                            {
                                index = availableTemplates.GetFormats(Path.GetExtension(name_part__name_List.Last()).Substring(1));
                                template = availableTemplates.GetAvailableTemplates(Path.GetExtension(name_part__name_List.Last()).Substring(1));
                            }
                        }
                    }
                    else
                    {
                        index = availableTemplates.GetFormats(commonExtension.Substring(1));
                    }

                    // 1つめの項目でエラーが出た場合、残る要素もエラーが出ることが必至なため、エラーを検知した場合はbreak
                    string FirstTest = CallMakeFile(number_part__offset, name_part__name_List, name_part__number_List, index);
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

                        result = CallMakeFile(StartNumber, name_part__name_List, name_part__number_List, index);
                        if (result != "")
                        {
                            AllErrors.Add(result);
                        }

                        Counter += 1;
                        StartNumber += 1;
                    }
                }
                else
                {
                    if (FormattedPathList.Split('@').Length == 1)
                    {
                        subInfo = "";
                    }
                    else
                    {
                        subInfo = FormattedPathList.Split('@')[1];
                        FormattedPathList = FormattedPathList.Split('@')[0];
                    }

                    string err;
                    // 拡張子で呼び出しを場合分け
                    AvailableTemplates availableTemplates = new AvailableTemplates();
                    int index;
                    if (commonExtension == "")
                    {
                        if (Path.GetExtension(FormattedPathList) == "")   // 拡張子が存在しない場合
                        {
                            index = -1;
                            template = new List<string>();
                        }
                        else
                        {
                            index = availableTemplates.GetFormats(Path.GetExtension(FormattedPathList).Substring(1));
                            template = availableTemplates.GetAvailableTemplates(Path.GetExtension(FormattedPathList).Substring(1));
                        }
                    }
                    else
                    {
                        index = availableTemplates.GetFormats(commonExtension.Substring(1));
                        template = availableTemplates.GetAvailableTemplates(commonExtension.Substring(1));
                    }

                    if (index == -1)
                    {
                        err = MakeFile(FormattedPathList);
                    }
                    else
                    {
                        err = MakeFile(FormattedPathList, false, index);
                    }

                    if (err != "")
                    {
                        AllErrors.Add(err);
                    }
                }
            }
            return AllErrors;
        }

        /// <summary>
        /// 連番形式のパスに対して、数値代入・整形する。
        /// </summary>
        /// <param name="number"></param>
        /// <param name="name_part__name_List"></param>
        /// <param name="name_part__number_List"></param>
        /// <returns></returns>
        private string CallMakeFile(int number, List<string> name_part__name_List, List<string> name_part__number_List, int index)
        {
            string formatted = "";

            for (int k = 0; k < name_part__name_List.Count(); k++)
            {
                if (k + 1 == name_part__name_List.Count())
                {
                    formatted += name_part__name_List[k];
                    break;
                }

                if (mw.ZeroPadding.IsChecked == true)
                {
                    formatted = formatted + name_part__name_List[k] + number.ToString().PadLeft(name_part__number_List[k].Length, '0');
                }
                else
                {
                    formatted = formatted + name_part__name_List[k] + number.ToString();
                }
            }
            string err;

            if (index == -1)
            {
                err = MakeFile(formatted, true);
            }
            else
            {
                err = MakeFile(formatted, false, index);
            }

            if (err != "")
            {
                return (err);
            }
            return "";
        }

        private string MakeFile(string path, bool isTextBased = true, int index = -1)
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
                        // テキストベースかどうかでファイル作成処理を分ける
                        if (isTextBased)
                        {
                            // テンプレートが設定されているかどうかで処理を分ける
                            if (template.Count() == 0)
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
                            else
                            {
                                using (FileStream fs = File.Create(fullPath + commonExtension))
                                {
                                    byte[] contents;
                                    if (int.Parse(template[4]) == 0)
                                    {
                                        contents = new UTF8Encoding().GetBytes(template[3]);
                                    }
                                    else if (int.Parse(template[4]) == 1)
                                    {
                                        contents = new UnicodeEncoding().GetBytes(template[3]);
                                    }
                                    else
                                    {
                                        Encoding s_jis = Encoding.GetEncoding(932);
                                        contents = s_jis.GetBytes(template[3]);
                                    }

                                    fs.Write(contents, 0, contents.Length);
                                }
                            }
                        }
                        else
                        {
                            string err = "";
                            err = MakeFileNotTextBased(fullPath + commonExtension, index);

                            if (err != "")
                            {
                                if (showDetailsOfErrors)
                                {
                                    return ("ファイル : " + fullPath + commonExtension + "\n詳細 : \n" + err);
                                }
                                else
                                {
                                    return ("ファイル : " + fullPath + commonExtension);
                                }
                            }
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

        private string MakeFileNotTextBased(string path, int index)
        {
            string err = "";
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    err = ImageFormatEncoder(path, index);
                    break;

                case 6:
                case 7:
                    err = SpreadsheetFormatEncoder(path, index);
                    break;
            }

            return err;
        }

        private string ImageFormatEncoder(string path, int index)
        {
            string err = "";
            try
            {
                int width;
                int height;
                string colorText;

                // テンプレートの存在をチェック
                if (template.Count() != 0)
                {
                    width = int.Parse(template[3]);
                    height = int.Parse(template[4]);
                    colorText = template[5];
                }
                else
                {
                    // 規定のテンプレート
                    width = 100;
                    height = 100;
                    colorText = "#FFFFFFFF";
                }

                // subInfoの形式をチェック、あればテンプレートを上書き
                if (subInfo != "" && Regex.IsMatch(subInfo, @"[1-9]\d*x[1-9]\d*"))
                {
                    width = int.Parse(subInfo.Split('x')[0]);
                    height = int.Parse(subInfo.Split('x')[1]);
                }

                byte[] imageData = new byte[width * height * 4];

                for (int i = 0; i < width * height; i++)
                {

                    imageData[i * 4] = (byte)Convert.ToInt32(colorText.Substring(7, 2), 16);          // B
                    imageData[i * 4 + 1] = (byte)Convert.ToInt32(colorText.Substring(5, 2), 16);      // G
                    imageData[i * 4 + 2] = (byte)Convert.ToInt32(colorText.Substring(3, 2), 16);      // R
                    imageData[i * 4 + 3] = (byte)Convert.ToInt32(colorText.Substring(1, 2), 16);      // A
                }

                // BitmapSourceを作成
                int stride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;

                BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, imageData, stride);

                FileStream stream = new FileStream(path, FileMode.Create);

                switch (index)
                {
                    case 0:
                        BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
                        bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        bmpBitmapEncoder.Save(stream);
                        break;
                    case 1:
                        GifBitmapEncoder gifBitmapEncoder = new GifBitmapEncoder();
                        gifBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        gifBitmapEncoder.Save(stream);
                        break;
                    case 2:
                        JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
                        jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        jpegBitmapEncoder.Save(stream);
                        break;
                    case 3:
                        PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        pngBitmapEncoder.Save(stream);
                        break;
                    case 4:
                        TiffBitmapEncoder tiffBitmapEncoder = new TiffBitmapEncoder();
                        tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        tiffBitmapEncoder.Save(stream);
                        break;
                    case 5:
                        WmpBitmapEncoder wmpBitmapEncoder = new WmpBitmapEncoder();
                        wmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        wmpBitmapEncoder.Save(stream);
                        break;
                }
                
                stream.Close();
            }
            catch (Exception ex)
            {
                err = ex.ToString();
            }

            // Set image source.
            Console.WriteLine("image, size : " + subInfo);
            return err;
        }

        /// <summary>
        /// スプレッドシートデータを出力
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        private string SpreadsheetFormatEncoder(string path, int index)
        {
            string err = "";
            try
            {
                // 新規ブック作成
                IWorkbook book;
                if (index == 6)
                {
                    book = new HSSFWorkbook();
                }
                else
                {
                    book = new XSSFWorkbook();
                }

                if (subInfo == "")
                {
                    book.CreateSheet("Sheet1");
                }
                else
                {
                    foreach(string sheet in subInfo.Split(','))
                    {
                        book.CreateSheet(sheet);
                    }
                }

                using (var fs = new FileStream(path, FileMode.Create))
                {
                    book.Write(fs);
                }
            }
            catch (Exception ex)
            {
                err = ex.ToString();
            }

            return err;
        }
    }

    /// <summary>
    /// 利用可能なテンプレートを参照
    /// </summary>
    public class AvailableTemplates
    {
        /// <summary>
        /// 指定した拡張子が属するフォーマットのインデックス番号を返す
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public int GetFormats(string ext)
        {
            LoadTemplateConfigs loadTemplateConfigs = new LoadTemplateConfigs();
            List<string> allFormats = loadTemplateConfigs.LoadExtensions();
            
            int index = -1;
            for (int i = 0; i < allFormats.Count(); i++)
            {
                if (allFormats[i].Split(',').Contains(ext))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// 指定した拡張子にヒットした最初のテンプレート情報をを返す。
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableTemplates(string ext)
        {
            LoadTemplateConfigs loadTemplateConfigs = new LoadTemplateConfigs();
            List<List<string>> availableTemplates = loadTemplateConfigs.Load();
            List<string> _return = new List<string>();

            for (int i = 0; i < availableTemplates.Count(); i++)
            {
                if (availableTemplates[i][1] == "True")
                {
                    if (availableTemplates[i][2].Split(',').Contains(ext))
                    {
                        _return = availableTemplates[i];
                        break;
                    }
                }
            }

            return _return;
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
            RegistryKey config_reg_window = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Config", true);
            if (config_reg_window == null)
            {
                // 初期値の設定
                config_reg_window = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\Config", true);
                config_reg_window.SetValue("WindowHeight", 660, RegistryValueKind.DWord);
                config_reg_window.SetValue("WindowWidth", 960, RegistryValueKind.DWord);
                config_reg_window.SetValue("StartFromZero", "False", RegistryValueKind.String);
                config_reg_window.SetValue("ZeroPadding", "True", RegistryValueKind.String);
                config_reg_window.SetValue("UseReturnToMoveFocus", "False", RegistryValueKind.String);
                config_reg_window.SetValue("CloseOnFinish", "True", RegistryValueKind.String);
                config_reg_window.SetValue("TextEncodingIndex", 0, RegistryValueKind.DWord);
            }

            RegistryKey regTemplates = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", true);
            if (regTemplates == null)
            {
                regTemplates = Registry.CurrentUser.CreateSubKey(@"Software\ASR_UserTools\makeNewFile\Templates", true);
                // ここで初期値の代入
                regTemplates.SetValue("count", 1);
                regTemplates.SetValue("tagList", 0);
                regTemplates.SetValue("headerTitle_0", "HTMLテンプレート");
                regTemplates.SetValue("isEnabled_0", "False");
                regTemplates.SetValue("targetExtension_0", "htm,html");
                regTemplates.SetValue("defaultText_0", "<!DOCTYPE html>\n<html lang=\"ja\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Document</title>\n</head>\n<body>\n    \n</body>\n</html>");
                regTemplates.SetValue("charasetIndex_0", 0);

                regTemplates.SetValue("BmpExtensions", "bmp");
                regTemplates.SetValue("GifExtensions", "gif");
                regTemplates.SetValue("JpegExtensions", "jpg,jpeg");
                regTemplates.SetValue("PngExtensions", "png");
                regTemplates.SetValue("TiffExtensions", "tif,tiff");
                regTemplates.SetValue("WmpExtensions", "wmp");
                regTemplates.SetValue("OfficeSpreadsheetBIFFExtensions", "xls");
                regTemplates.SetValue("OfficeSpreadsheetOOXMLExtensions", "xlsx");
            }
            regTemplates.Close();

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
            RegistryKey config_reg_window = Registry.CurrentUser.OpenSubKey(@"Software\ASR_UserTools\makeNewFile\Config", true);

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