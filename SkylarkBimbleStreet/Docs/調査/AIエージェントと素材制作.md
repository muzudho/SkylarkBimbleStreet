# AIエージェントと素材制作

調査日: 2026-06-10

## 目的

音楽や大量アートを作れるメンバーがいないため、Codex 以外の AI エージェントや生成 AI が素材制作に使えるかを確認したメモ。

## 結論

- AI エージェント名だけで判断するより、その AI にどの生成ツールが接続されているかが重要。
- Codex はプログラム、仕様整理、UI、台詞、素材管理に向く。
- Claude は文章、設計、コード相談、長い文脈整理、画像理解に向く。素材ファイルの直接生成担当として見るより、企画やレビュー寄り。
- Gemini / Google 系は画像生成、音楽生成、音声、動画などの生成モデルが用意されている。素材制作候補としては幅が広い。
- ただし、AI 生成素材でもライセンス確認と利用規約確認は必要。
- 当面は、ライセンスの明確なフリー素材を探して使う方針が安全。

## Claude の見立て

Anthropic の公式ドキュメントでは、Claude はテキストと画像入力、テキスト出力、視覚理解、コーディングなどに対応するモデルとして説明されている。

このプロジェクトでの使いどころ:

- 仕様レビュー
- シナリオや台詞の別案
- コード相談の別視点
- 長いドキュメントの整理
- 画像素材の内容確認や説明

注意:

- Claude 単体を、画像素材や音楽ファイルを直接量産する担当として見るのは違いそう。
- 画像や音楽を作る場合は、別の生成ツールとの組み合わせを考える。

## Gemini / Google 系の見立て

Google AI for Developers の公式ドキュメントでは、Gemini API 周辺に以下の系統がある。

- 画像生成: Gemini image generation、Nano Banana、Imagen
- 音楽生成: Lyria 3 系
- 音声: Text-to-speech、audio understanding
- 動画: Veo
- 調査: Deep Research
- UI 操作系: Computer Use Preview

このプロジェクトでの使いどころ:

- キャラクターや背景のラフ案
- アイコン案
- UI の雰囲気案
- 短い BGM ループや音楽方向性の検討
- 動画や宣伝素材の案出し

注意:

- 実際にゲームへ組み込む素材は、生成サービスの利用規約を確認してから採用する。
- モデルやサービスの仕様は変わるため、使う直前に再確認する。
- 生成物の雰囲気統一、大量差分、アニメーション一式は別途管理が必要。

## 当面の素材方針

- まずはフリー素材を探す。
- 素材ごとに、出所、作者、URL、ライセンス、クレジット表記、加工可否、商用利用可否を記録する。
- ライセンスが曖昧な素材は使わない。
- AI 生成素材は、ラフ案や方向性検討には使いやすい。
- AI 生成素材を最終素材にする場合は、生成元、モデル、生成日、利用規約、加工有無を記録する。
- 有料素材や有料ツールが必要になった場合は、導入前にむずでょさんに相談する。

## 参考リンク

- Anthropic Claude models overview: https://platform.claude.com/docs/en/about-claude/models/overview
- Gemini models: https://ai.google.dev/gemini-api/docs/models
- Gemini image generation: https://ai.google.dev/gemini-api/docs/image-generation
- Gemini audio: https://ai.google.dev/gemini-api/docs/audio
