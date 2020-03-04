# 拡張ファイル作成 (As/Rスクリプト)
As/Rのスクリプトコマンドとして使用可能なソフトウェア。<br>
空のファイルの作成をサポート。1階層までのディレクトリを含めたファイル作成が可能。

## 仕様
- 既存のファイルの上書きは行わない。エラーを返す。
- 2階層以上の深いディレクトリ構造を作成することはできない。エラーを返す。Node.jsの仕様(?)

## コマンドラインオプション
各オプションの値は、開発者ツールのconsoleタブで確認可能。<br>
これらのオプションを付与する場合は、
```
[commandLineOption]=[value]
```
の形をとり、不要なスペースはすべて削除して使用。

例 : ``--closeOnFinish=false``

### --targetDirectory
`` デフォルト値 : ./ ``<br>
ブートディレクトリを指定する。<br>
開発用のオプション。使用メリットは特になし。
### --closeOnFinish
`` デフォルト値 : true ``<br>
ファイル作成終了時にウィンドウを閉じる。
### --darkTheme
`` デフォルト値 : false ``<br>
ダークテーマを有効にする。

---------------------------------------

## Authors
NumLocker (https://github.com/NumLocker-Japan)

## Credits
This software was made using the following open source project.

### [electron](https://github.com/electron/electron)
Released under the MIT license<br>
https://opensource.org/licenses/mit-license.php

## License
*Copyright (c) 2020 NumLocker*<br>
Released under the MIT license<br>
https://opensource.org/licenses/mit-license.php