# SkylarkBimbleStreet

MonoGame DesktopGL で作る C# ゲームプロジェクト。

## 現在のゲーム

1画面アクションパズルの試作版。

- 青いプレイヤーを操作する。
- 黄色い宝石をすべて集める。
- 緑の出口に到達するとクリア。
- 赤い障害物に当たるとスタート位置へ戻る。
- ゲーム状態はウィンドウタイトルにも表示される。

## 操作

- 移動: `WASD` / 矢印キー / Xbox コントローラー左スティック
- リトライ: `R` / Xbox コントローラー Start
- 終了: `Esc` / Xbox コントローラー Back

## 必要なもの

- .NET 8 SDK
- MonoGame のビルド用ツール

初回ビルド時は `SkylarkBimbleStreet.csproj` の `RestoreDotnetTools` ターゲットにより、`dotnet tool restore` が実行される。

## 実行

```powershell
dotnet run --project .\SkylarkBimbleStreet\SkylarkBimbleStreet.csproj
```

## 構成

- `SkylarkBimbleStreet/Game1.cs`: MonoGame のメインループと試作ゲーム本体。
- `SkylarkBimbleStreet/Program.cs`: `Game1` を起動するエントリーポイント。
- `SkylarkBimbleStreet/Content/Content.mgcb`: 画像、音声、フォントなどのコンテンツ定義。
- `SkylarkBimbleStreet/Docs`: 開発メモと運用メモ。

## 現在の状態

外部アセットなしで動く、図形描画だけのアクションパズル試作版。内部解像度は `1920x1080` で、実ウィンドウへレターボックス付きで拡大縮小する。
