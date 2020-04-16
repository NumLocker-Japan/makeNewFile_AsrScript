const { app, BrowserWindow, globalShortcut, ipcMain, Menu, shell } = require('electron');
const fs = require('fs');

app.on('ready', function() {
	let appWindow = new BrowserWindow({ width: 680, height: 360, webPreferences: {
		// Rendererプロセスでrequireできるようにする
		nodeIntegration: false,
		contextIsolation: false,
		preload: __dirname + '/preload.js'
	  }})
	Menu.setApplicationMenu(null);

	// キーボードショートカットを指定
	globalShortcut.register('Control+Shift+I', () => {
		appWindow.webContents.openDevTools();
	})
	globalShortcut.register('Esc', () => {
		appWindow.close();
	})
	globalShortcut.register('F1', () => {
		shell.openExternal('https://github.com/NumLocker-Japan/makeNewFile_AsrScript/wiki/オンラインヘルプ');
	})

	appWindow.loadURL('file://' + __dirname + '/index.html');

	// アップデート関連
	ipcMain.on('updateCheck-from-renderer', (event, arg) => {
		if (arg[0] === 'checkUpdate') {
			fs.readFile(arg[1] + 'resources\\version', 'utf8', (err, data) => {
				if (err !== null) {
					event.sender.send('updateCheck-reply', null);
				} else {
					event.sender.send('updateCheck-reply', data);
				}
			});
		}
	})

	ipcMain.on('update-result', (event, arg) => {
		if (arg[0] === 'success') {
			arg[2][2] = Date.now();
			arg[2][3] = 0;
			fs.writeFile(arg[1] + 'resources\\version', arg[2].join('\n'), {encoding : 'utf8'}, (err) => {
				// 
			})
		}
		
		if (arg[0] === 'failure') {
			arg[2][3] = parseInt(arg[2][3]) + 1;
			fs.writeFile(arg[1] + 'resources\\version', arg[2].join('\n'), {encoding : 'utf8'}, (err) => {
				// 
			})
		}
	})

	// Rendererからのmessage受け取り
	ipcMain.on('message-from-renderer', (event, arg) => {
		if (arg === 'closeWindow') {
			appWindow.close();
		}

		if (arg === 'getArgs') {
			event.sender.send('reply-to-renderer', process.argv);
		}

		if(arg[0] === 'run') {
			runInMain(arg[1], arg[2], arg[3], arg[4]);
		}

		if (arg === 'openReleases') {
			shell.openExternal('https://github.com/NumLocker-Japan/makeNewFile_AsrScript/releases');
		}
	})

	// Rendererプロセスからこちらに処理が移る
	const runInMain = (targetDirectory, closeOnFinish, common_extension, filePathList) => {

		const applyNumbers = () => {
			// 連番対応
		}

		const makeFiles = (x) => {
			return new Promise((resolve, reject) => {
				if (x.trim() !== ''){
					fs.writeFile(targetDirectory + x + common_extension, '', {encoding : 'utf8', flag : "wx"}, (err) => {
						if (err !== null){
							if (err.code === 'EEXIST'){
								resolve("ファイルの作成に失敗しました。\n" + err.path + "はすでに存在します。");
							} else if (err.code === 'ENOENT'){
								// パスが存在しない場合と、不正文字が使用されている場合がある
								// 連番作成用のコードかどうか確認。
								if (/^[^$*]*\$+[^$*]*\*\d+$/g.test(x) === true) {

									// 連番作成用コード
									if ((10 ** x.match(/\$/g).length - 1) >= x.split('*')[1]) {
										// x.replace('$'.repeat(x.match(/\$/g).length))
										resolve("ファイルの作成に失敗しました。\n現在、連番作成は利用できません。");
									} else {
										resolve("ファイルの作成に失敗しました。\n数字が大きすぎます。");
									}
	
								} else {
	
									if (/^.*(\/|\\).*$/g.test(x) === true) {
										let addPathList = x.split(/\/|\\/);
										addPathList.pop();
										// パスの補完を行う。
										fs.mkdir((targetDirectory + addPathList.join('\/')).toString().replace(/\\/g, '\/'), { recursive: true }, (err) => {
											if (err !== null) {
												resolve("ファイルの作成に失敗しました。\nフォルダが作成できませんでした。");
											} else {
												fs.writeFile((targetDirectory + x + common_extension).toString().replace(/\\/g, '\/'), '', {encoding : 'utf8', flag : "wx"}, (err) => {
													if (err !== null){
														if (err.code === 'EEXIST'){
															resolve("ファイルの作成に失敗しました。\n" + err.path + "はすでに存在します。");
														} else if (err.code === 'ENOENT') {
															// パス/ファイル名のエラーはすべてここでcatch。
															resolve("ファイルの作成に失敗しました。\n" + err.path + "は不正なパス/ファイル名です。");
														} else {
															resolve("ファイルの作成に失敗しました。\n" + err.path + "でエラーが発生しました。");
														}
													} else {
														resolve(null);
													}
												})
											}
										})
									} else {
										// ファイル名に不正文字が含まれる。
										resolve("ファイルの作成に失敗しました。\n" + err.path + "は不正なパス/ファイル名です。");
									}
								}
							} else {
								resolve("ファイルの作成に失敗しました。\n" + err.path + "でエラーが発生しました。");
							}
						} else {
							resolve(null);
						}
					});
				} else {
					resolve(null);
				}
			})
		}

		Promise.all(filePathList.map(x => {
			return makeFiles(x);
		})).then((result) => {
			appWindow.webContents.send('message-from-main', ['alert', result]);
			ipcMain.on('reply-to-main', (event, arg) => {
				if (arg === 'finished') {
					if (closeOnFinish === 'true') {
						appWindow.close();
					}
				}
			})
		})
	}

	appWindow.on('closed', () => {
		appWindow = null;
	})
})

app.on('window-all-closed', function() {
	app.quit();
});