MqPmxConvert　.20160504
====

## 概要
.mqo&.mqx形式と.pmx形式のモデルデータを相互変換するためのプログラムです。  
プラグイン形式ではなくプログラム単体で動作し、Metasequoia4標準のボーン情報にも対応しています。  
(This program provides the interconversion between .mqo&.mqx and .pmx, and supports bone information of Metasequoia Ver4.  
 You can run it as not a plugin software but a single program.)  

## 必須要件
Windows向けソフトウェアです。動作には.NET Framework 4.0のインストールが必要です。(http://www.microsoft.com/ja-jp/download/details.aspx?id=17718)  
(For Windows. .NET Framework 4.0 is needed.)    

## 使い方
・mqoからpmxへの変換  
① プログラムを開きます  
② 変換したい.mqoファイルを選択します  
③ 変換してできる.pmxファイルの名前を指定します。標準では元の名前の後ろに「.cnv.pmx」を付与した名前になっています   
④ 変換が実施されコンソールウィンドウに「正常終了」と表示されます  
※曲面やIK、一部材質等の情報は変換されません  
  
・mqoからpmxへの変換  
① プログラムを開きます  
② 変換したい.pmxファイルを選択します  
③ 変換してできる.mqo・.mqxファイルの名前を指定します。標準では元の名前の後ろに「.cnv.mqo」を付与した名前になっています  
④ 変換が実施されコンソールウィンドウに「正常終了」と表示されます  
※物理やモーフ、一部材質等の情報は変換されません  
  
※本プログラムに変換したいファイルをドラッグアンドドロップすることによっても変換することができます  
※また、コマンドライン引数を入力ファイルパス、出力ファイルパスの順で与えることによっても変換することができます  
  
(  
[Mqo to pmx]
 1. Launch this program.  
 2. In a file browser dialog, select source mqo file.  
 3. In a file browser dialog, decide the name of a destination file.  
 4. After the conversion finishes, "正常終了" will be displayed.  
  * Information of curved surfaces and inverse-kinematicsk, materials has been lost maybe.  

[Pmx to Mqo]
 1. Launch this program.  
 2. In a file browser dialog, select source pmx file.  
 3. In a file browser dialog, decide the name of a destination file.  
 4. After the conversion finishes, "正常終了" will be displayed.
  * Information of rigids and joints, morph, materials has been lost maybe.

* You can convert a file with drag-and-drop or command-line arguments.)

## ライセンス
本プログラムはフリーウェアです。完全に無保証で提供されるものでありこれを使用したことにより発生した、または発生させた、あるいは発生させられたなどしたいかなる問題に関して製作者は一切の責任を負いません。  
別途ライセンスが明記されている場所またはファイルを除き、個人・商用利用にかかわらず使用者は自らの責任において本プログラムを自由に複製、改変することが可能です。  

## 謝辞
本プログラム作成にあたっては以下の方のコードを利用させていただいております。この場を借りてお礼申し上げます。  
b2ox様作成 MQOplugin (https://github.com/b2ox/MQOplugin)  
ぱるた様作成 tso2mqo (https://osdn.jp/projects/tdcgexplorer/)  
Alexandre Mutel様 SharpDX (http://sharpdx.org/)  

## Author
saso
