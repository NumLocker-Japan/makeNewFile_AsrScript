const { app, BrowserWindow, globalShortcut, ipcMain, Menu } = require('electron')

app.on('ready', function() {
    let appWindow = new BrowserWindow({ width: 680, height: 340, webPreferences: {
        nodeIntegration: true
    }})
    Menu.setApplicationMenu(null);

    globalShortcut.register('Control+Shift+I', () => {
        appWindow.webContents.openDevTools()
    })
    globalShortcut.register('Esc', () => {
        appWindow.close()
    })

    appWindow.loadURL('file://' + __dirname + '/index.html');

    ipcMain.on('message-from-renderer', (event, arg) => {
        // Rendererからのmessage受け取り
        if (arg === 'closeWindow') {
            appWindow.close();
        }

        if (arg === 'getArgs') {
            event.sender.send('reply-to-renderer', process.argv);
        }

        if(arg[0] === 'run') {
            run2(arg[1], arg[2], arg[3], arg[4]);
        }
    })


    const run2 = (targetDirectory, closeOnFinish, common_extension, filePathList) => {

        const fs = require('fs');

        const promise_run = new Promise((resolve, reject) => {

            ipcMain.on('message-from-renderer', (event, arg) => {
                if (arg === 'finished') {
                    resolve();
                }
            })

            for (let i = 0; i < filePathList.length; i++) {
                if (filePathList[i].trim() !== ''){
                    fs.writeFile(targetDirectory + filePathList[i] + common_extension, '', {encoding : 'utf8', flag : "wx"}, (err) => {
                        if (err !== null){
                            if (err.code === 'EEXIST'){
                                appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "はすでに存在します。", i])
                            } else if (err.code === 'ENOENT'){
                                // パスが存在しない場合と、不正文字が使用されている場合がある
                                if (/^.*(\/|\\).*$/g.test(filePathList[i]) === true) {
                                    let addPathList = filePathList[i].split(/\/|\\/);
                                    addPathList.pop();
                                    // パスの補完を行う。
                                    fs.mkdir((targetDirectory + addPathList.join('\/')).toString().replace(/\\/g, '\/'), { recursive: true }, (err) => {
                                        if (err !== null) {
                                            appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\nフォルダが作成できませんでした。", i]);
                                        } else {
                                            fs.writeFile((targetDirectory + filePathList[i] + common_extension).toString().replace(/\\/g, '\/'), '', {encoding : 'utf8', flag : "wx"}, (err) => {
                                                if (err !== null){
                                                    if (err.code === 'EEXIST'){
                                                        appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "はすでに存在します。", i])
                                                    } else if (err.code === 'ENOENT') {
                                                        // パス/ファイル名のエラーはすべてここでcatch。
                                                        appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "は不正なパス/ファイル名です。", i])
                                                    } else {
                                                        appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "でエラーが発生しました。", i])
                                                    }
                                                } else {
                                                    appWindow.webContents.send('message-from-main', ['promise_fill', "", i])
                                                }
                                            })
                                        }
                                    })
                                } else {
                                    // ファイル名に不正文字が含まれる。
                                    appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "は不正なパス/ファイル名です。", i])
                                }
                            } else {
                                appWindow.webContents.send('message-from-main', ['alert', "ファイルの作成に失敗しました\n" + err.path + "でエラーが発生しました。", i])
                            }
                        } else {
                            appWindow.webContents.send('message-from-main', ['promise_fill', "", i])
                        }
                    });
                } else {
                    appWindow.webContents.send('message-from-main', ['promise_fill', "", i])
                }
            }

        })

        promise_run.then(() => {
            if (closeOnFinish === 'true') {
                appWindow.close();
            }
        })
    }


    appWindow.on('closed', () => {
        appWindow = null
    })
})

app.on('window-all-closed', function() {
    app.quit();
});
