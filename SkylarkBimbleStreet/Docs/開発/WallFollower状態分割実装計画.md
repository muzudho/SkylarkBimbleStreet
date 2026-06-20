# WallFollower 状態分割実装計画

`WallFollower` を、コメントで整理した4状態に合わせて段階的に State パターンへ移行する。

対象状態:

- ［無反応］
- ［壁直前］
- ［壁正面］
- ［壁沿い］

目的は、いきなり大規模に分割せず、現在の挙動を壊さずに、状態判定を明示化してから状態クラス化すること。

## Phase 1: 状態 enum を追加する

1. `WallFollowerStateKind` を追加する。
2. 値は以下にする。
   - `NoReaction`
   - `BeforeWall`
   - `FacingWall`
   - `AlongWall`
3. まずは状態クラスを作らず、状態名だけをコード上で表せるようにする。

## Phase 2: 現在状態の判定メソッドを作る

1. `WallFollower` に `GetStateKind()` を追加する。
2. 現在の内部値から状態を推定する。
3. 既存処理はまだ変更しない。
4. デバッグ表示やログに使える形にする。

状態判定の基本:

```plaintext
進行方向壁なし + 親壁なし => 無反応
進行方向壁あり + 親壁なし => 壁直前
進行方向壁あり + 親壁あり => 壁正面
進行方向壁なし + 親壁あり => 壁沿い
```

## Phase 3: 状態判定用の小さなコンテキストを作る

1. `WallFollowerContext` または内部 private record を作る。
2. 以下をまとめる。
   - 入力方向
   - 進行方向
   - 親壁方向
   - 進行方向壁の有無
   - 親壁の有無
   - 外角判定ゴーストの結果
3. `WallFollower` 内の暗黙状態を、まずこのコンテキストへ集約する。

## Phase 4: 状態インターフェースを作る

1. `IWallFollowerState` を追加する。
2. まずは最小構成にする。

```csharp
internal interface IWallFollowerState
{
	WallFollowerStateKind Kind { get; }

	void Move(WallFollowerContext context);
}
```

3. この時点では既存ロジックを完全移動せず、薄いラッパーとして使う。

## Phase 5: 4状態クラスを追加する

追加するクラス:

```plaintext
NoReactionWallFollowerState
BeforeWallFollowerState
FacingWallFollowerState
AlongWallFollowerState
```

役割:

- `NoReactionWallFollowerState`
  - 通常移動
  - 壁追従解除
- `BeforeWallFollowerState`
  - 次の壁候補を検出
  - 壁正面へ入る準備
- `FacingWallFollowerState`
  - 壁に当たった直後
  - 壁沿い方向を決める
- `AlongWallFollowerState`
  - 壁沿い移動
  - 内角、外角、ローラー補正、ゴースト判定を扱う

## Phase 6: 既存処理を少しずつ状態クラスへ移す

移動順は安全性優先。

1. ［無反応］の処理を移す。
2. ［壁正面］の初回スライド開始処理を移す。
3. ［壁沿い］の継続処理を移す。
4. 最後に［壁直前］の予測処理を整理する。

## Phase 7: ローラー処理を分離する

現在は通常壁追従とローラー壁追従が似ているため、状態分割後に整理する。

候補:

```plaintext
WallFollowMode.Basic
WallFollowMode.Roller
```

または:

```plaintext
WallFollowSettings
```

に以下をまとめる。

- スライド倍率
- 外角ターン倍率
- 接触距離
- ゴースト予測フレーム数

## Phase 8: 重複ユーティリティを整理する

候補:

- `Direction.cs`
- `WallContact.cs`
- `WallFollowerContext.cs`

切り出し候補メソッド:

- 方向の4方向化
- 右回転
- 壁接触辺変換
- `WallContactSide` から方向への変換
- 垂直判定

## Phase 9: 動作確認

各 Phase ごとに確認する。

1. ビルド成功。
2. 通常移動が壊れていない。
3. 壁に押し付けたとき壁沿いに進む。
4. 外角で接触が切れにくい。
5. 内角で詰まりにくい。
6. ローラー取得時の挙動が以前と同等。
7. 先行ゴーストと壁ハイライトが表示される。

## 方針

最初から完全な状態クラス化はしない。
まず `WallFollowerStateKind` と状態判定を入れ、既存挙動を保ったまま、あとから処理を4状態へ移していく。

## 実施状況

- Phase 1: `WallFollowerStateKind` を追加済み。
- Phase 2: `StateKind` と `GetStateKind()` を追加済み。
- Phase 3: `WallFollowerContext` を追加済み。
- Phase 4: `IWallFollowerState` を追加済み。
- Phase 5: 4状態の入れ子クラスを追加済み。
- Phase 6: `Move`、通常壁追従継続、ローラー壁追従継続を状態クラス経由に変更済み。
- 追加整理: 状態クラスが `WallFollower` の private 実装へ直接触る箇所を減らすため、`WallFollowerContext` に委譲メソッドを追加済み。
- 追加整理: `WallFollowerStateKind`、`IWallFollowerState`、`WallFollowerContext` を `WallFollowerState.cs` へ切り出し済み。

次は、状態クラスを別ファイルへ切り出せるように、`WallFollowerContext` へ既存 private 処理の境界を少しずつ移す。
