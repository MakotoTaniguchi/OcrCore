# サービス名
* OcrCore

## 開発環境
* Visual Studio 2019

## インストール
* .NetCore 3.0
  * https://dotnet.microsoft.com/download/dotnet-core/3.0

## フレームワーク
* ASP.NET Core

## 開発言語
* C#言語

## 実行方法
* 下記バッチを実行
  * https://github.com/MakotoTaniguchi/OcrCore/blob/develop/OcrService/dotnet-run.bat

## 環境変数
* Amazon
  * AWSAccessKeyId：アクセスキー
  * AWSSecretKey：シークレットキー
* Azure
  * COMPUTER_VISION_SUBSCRIPTION_KEY：サブスクライブキー
  * COMPUTER_VISION_ENDPOINT：エンドポイントキー
* GCP
  * GOOGLE_APPLICATION_CREDENTIALS：証明JSONファイルパス
    * 証明JSONファイルをGoogle Cloudからダウンロード

## 備考
* Windows SDKを使用してWindows10のOCRを利用しています。
  * MacではWindows SDKのOCRが利用出来ません。