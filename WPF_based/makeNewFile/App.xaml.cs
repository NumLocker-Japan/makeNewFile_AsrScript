using System.Collections.Generic;
using System.Windows;

namespace makeNewFile
{
    public class AccessArgs   // Mainからもアクセス可能
    {
        private static Dictionary<string, string> valueOfArgs;

        public Dictionary<string, string> ArgsList
        {
            set { valueOfArgs = value; }
            get { return valueOfArgs; }
        }
    }

    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var argsList = new Dictionary<string, string>()
            {
                // 対応する引数一覧 : 初期値込み
                {"currentDirectory", ""},
                {"showDetailsOfErrors", "false"},
                {"alertManyItems", "1000"},
                {"fontSize", ""},
            };

            AccessArgs aa = new AccessArgs();
            if (e.Args.Length > 0)
            {
                // 引数の処理
                string currentDirectory = e.Args[0];
                currentDirectory = currentDirectory.Replace("\"", "");
                argsList["currentDirectory"] = currentDirectory;

                int i = 1;
                while (i < e.Args.Length)
                {
                    // ここで動的に引数を取得
                    argsList[e.Args[i].Split('=')[0].Substring(2)] = e.Args[i].Split('=')[1];
                    i++;
                }
            }

            aa.ArgsList = argsList;
        }
    }
}
