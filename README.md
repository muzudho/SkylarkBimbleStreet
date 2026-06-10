# SkylarkBimbleStreet

MonoGame DesktopGL で作る C# ゲームプロジェクト。

## 必要なもの

- .NET 8 SDK
- MonoGame のビルド用ツール

初回ビルド時は `SkylarkBimbleStreet.csproj` の `RestoreDotnetTools` ターゲットにより、`dotnet tool restore` が実行される。

## 実行

```powershell
dotnet run --project .\SkylarkBimbleStreet\SkylarkBimbleStreet.csproj
```

## 構成

- `SkylarkBimbleStreet/Game1.cs`: MonoGame のメインループ。
- `SkylarkBimbleStreet/Program.cs`: `Game1` を起動するエントリーポイント。
- `SkylarkBimbleStreet/Content/Content.mgcb`: 画像、音声、フォントなどのコンテンツ定義。
- `SkylarkBimbleStreet/Docs`: 開発メモと運用メモ。

## 現在の状態

MonoGame DesktopGL のテンプレート直後の状態。まだゲーム固有の描画、入力、アセット登録は入っていない。
