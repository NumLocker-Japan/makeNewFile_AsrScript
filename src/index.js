window.addEventListener('load', () => {

	const ipcRenderer = window.ipcRenderer;

	ipcRenderer.send('message-from-renderer', 'getArgs');
	ipcRenderer.on('reply-to-renderer', (event, arg) => {
		
		let closeOnFinish = '';
		if (arg.find(x => /^--closeOnFinish=.+$/.test(x))) {
			closeOnFinish = arg.find(x => /^--closeOnFinish=.+$/.test(x)).slice(16);
			if (closeOnFinish !== 'true' && closeOnFinish !== 'false') {
				closeOnFinish = 'true'
			}
		} else {
			closeOnFinish = 'true'
		}

		let darkTheme = '';
		if (arg.find(x => /^--darkTheme=.+$/.test(x))) {
			darkTheme = arg.find(x => /^--darkTheme=.+$/.test(x)).slice(12);
			if (darkTheme !== 'true' && darkTheme !== 'false') {
				darkTheme = 'false'
			}
		} else {
			darkTheme = 'false'
		}
		document.querySelector('body').setAttribute('darkTheme', darkTheme);

		let targetDirectory = '';
		if (arg.find(x => /^--targetDirectory=.+$/.test(x))) {
			targetDirectory = arg.find(x => /^--targetDirectory=.+$/.test(x)).slice(18);
		} else {
			targetDirectory = './';
		}

		let scriptPath;
		if (arg.find(x => /^--scriptPath=.+$/.test(x))) {
			scriptPath = arg.find(x => /^--scriptPath=.+$/.test(x)).slice(13);
		} else {
			scriptPath = null;
		}
		
		console.log("スクリプト格納ディレクトリ(--scriptPath) : " + scriptPath);
		console.log("ターゲットディレクトリ(--targetDirectory) : " + targetDirectory);
		console.log("終了時にウィンドウを閉じる(--closeOnFinish) : " + closeOnFinish);
		console.log("ダークテーマ(--darkTheme) : " + darkTheme);
		if (!scriptPath) {
			alert('スクリプト呼び出しが不正です。\nコマンドラインオプション --scriptPath は必須です。');
			ipcRenderer.send('message-from-renderer', 'closeWindow');
		} else {
			ipcRenderer.send('updateCheck-from-renderer', ['checkUpdate', scriptPath]);
			ipcRenderer.on('updateCheck-reply', (event, arg) => {
				if (arg) {
					console.log("現在のバージョン : " + arg.split('\n')[1]);
					console.log("最終アップデート確認時刻(UnixTime) : " + arg.split('\n')[2]);
					if (parseInt(arg.split('\n')[2]) + 600000000 < Date.now()) {
						// 約1週間毎にアップデートを確認
						const updateURL = 'https://api.github.com/repos/NumLocker-Japan/makeNewFile_AsrScript/releases/latest';
						const request = new XMLHttpRequest();
						request.open('GET', updateURL);
						request.setRequestHeader('Accept', 'application/vnd.github.v3+json')
						request.onreadystatechange = () => {
							if (request.readyState != 4) {
								// 取得未完了
							} else if (request.status == 200 || request.status == 304) {
								if (JSON.parse(request.responseText).name !== arg.split('\n')[1]) {
									ipcRenderer.send('update-result', ['success', scriptPath, arg.split('\n')]);
									let whichToOpenReleases = confirm('新しいバージョンがあります。\nダウンロードページを開きますか？\n------------------------------\n現在のバージョン : ' + arg.split('\n')[1] + '\n最新のバージョン' + JSON.parse(request.responseText).name);
									if (whichToOpenReleases) {
										ipcRenderer.send('message-from-renderer', 'openReleases');
									}
									console.log('アップデートの確認 : 最新版あり');
								} else {
									console.log('アップデートの確認 : 最新版なし');
									ipcRenderer.send('update-result', ['success', scriptPath, arg.split('\n')]);
								}
							} else {
								if (parseInt(arg.split('\n')[3]) >= 10) {
									ipcRenderer.send('update-result', ['success', scriptPath, arg.split('\n')]);
									let whichToOpenReleases = confirm('10回以上連続でアップデートの確認に失敗しました。\n新しいバージョンを手動で確認しますか？インターネット接続が必要です。\n------------------------------\n現在のバージョン : ' + arg.split('\n')[1]);
									if (whichToOpenReleases) {
										ipcRenderer.send('message-from-renderer', 'openReleases');
									}
								} else {
									ipcRenderer.send('update-result', ['failure', scriptPath, arg.split('\n')]);
									console.warn('アップデートの確認 : 更新の取得に失敗しました。');
								}
							}
						}
						request.send(null);
					} else {
						console.log('アップデートの確認 : 必要なし');
					}

					main(targetDirectory, closeOnFinish);
				} else {
					alert('設定を読み込めませんでした。\nコマンドラインオプション --scriptPath の値が間違っています。');
					ipcRenderer.send('message-from-renderer', 'closeWindow');
				}
			})
		}
	})

	const main = (targetDirectory, closeOnFinish) => {
		document.getElementById('usr_input').focus();
		document.getElementById('usr_input').setSelectionRange(0, 0);

		document.getElementById('usr_input').addEventListener('keypress', (event) => {
			if (event.code === 'Enter' && event.ctrlKey === true) {
				document.getElementById('usr_input').value = document.getElementById('usr_input').value + '\n'
			}

			if (event.code === 'Enter' && event.ctrlKey === false) {
				run(targetDirectory, closeOnFinish);
			}
		})

		document.getElementById('common_extension').addEventListener('keypress', (event) => {
			if (event.code === 'Enter') {
				run(targetDirectory, closeOnFinish);
			}
		})

		document.getElementById('make').addEventListener('click', () => {
			run(targetDirectory, closeOnFinish);
		})
	}

	const run = (targetDirectory, closeOnFinish) => {
		let common_extension = '';
		if(document.getElementById('common_extension').value.trim() !== '') {
			common_extension = '.' + document.getElementById('common_extension').value.trim();
		}
		const filePathList = document.getElementById('usr_input').value.split('\n');
		document.getElementById('usr_input').value = '';
		document.getElementById('common_extension').value = '';
		document.getElementById('usr_input').focus();
		document.getElementById('usr_input').setSelectionRange(0, 0);
		ipcRenderer.send('message-from-renderer', ['run', targetDirectory, closeOnFinish, common_extension, filePathList]);

		ipcRenderer.on('message-from-main', (event, arg) => {
			if (arg[0] === 'alert') {
				while (arg[1].length > 0) {
					if (arg[1][0]) {
						alert(arg[1][0]);
					}
					arg[1].shift();
				}
				ipcRenderer.send('reply-to-main', 'finished');
			}
		})
	}

})