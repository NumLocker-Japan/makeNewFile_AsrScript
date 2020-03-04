window.addEventListener('load', () => {

    const {ipcRenderer} = require('electron');

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

        console.log("ターゲットディレクトリ(--targetDirectory) : " + targetDirectory);
        console.log("終了時にウィンドウを閉じる(--closeOnFinish) : " + closeOnFinish);
        console.log("ダークテーマ(--darkTheme) : " + darkTheme);
        main(targetDirectory, closeOnFinish)
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

        let judge_reject_list = Array(filePathList.length);
        judge_reject_list.fill(0);
        const judge_reject = (index) => {
            judge_reject_list[index] = 1;
            if (judge_reject_list.includes(0) === false) {
                ipcRenderer.send('message-from-renderer', 'finished');
            }
        }
        ipcRenderer.on('message-from-main', (event, arg) => {
            if (arg[0] === 'alert') {
                alert(arg[1]);
                judge_reject(arg[2]);
            }

            if (arg[0] === 'promise_fill') {
                judge_reject(arg[2]);
            }
        })
    }

})