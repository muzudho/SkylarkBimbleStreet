# MonoGame テンプレート確認

確認日: 2026-06-10

## 概要

`SkylarkBimbleStreet` は MonoGame DesktopGL の C# プロジェクト。

## 確認したファイル

- `SkylarkBimbleStreet/SkylarkBimbleStreet.csproj`
- `SkylarkBimbleStreet/Game1.cs`
- `SkylarkBimbleStreet/Program.cs`
- `SkylarkBimbleStreet/Content/Content.mgcb`
- `SkylarkBimbleStreet/Docs/運用/リポジトリー分離.md`

## 分かったこと

- ターゲットフレームワークは `net8.0`。
- 出力タイプは `WinExe`。
- MonoGame パッケージは `MonoGame.Framework.DesktopGL` と `MonoGame.Content.Builder.Task` の `3.8.*`。
- `Content.mgcb` は `/platform:DesktopGL`、`/profile:Reach`。
- `Content.mgcb` にはまだ画像、音声、フォントなどのアセット登録がない。
- `Game1.cs` はテンプレート状態で、`Initialize`、`LoadContent`、`Update`、`Draw` に TODO が残っている。
- `Draw` は `GraphicsDevice.Clear(Color.CornflowerBlue)` のみ。
- `Update` は GamePad Back または Escape キーで終了する。
- `Program.cs` は `Game1` を生成して `Run()` するだけの最小構成。

## 補足

`Docs/運用/リポジトリー分離.md` には、`ShogiTournamentSystemAnalyzer` と `SkylarkBimbleStreet` を別リポジトリ、別プロジェクトとして扱う運用ルールが書かれている。
