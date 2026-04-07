# Design: Game074v2_NoteRain

## スクリプト構成

### namespace: `Game074v2_NoteRain`

| クラス | 役割 |
|-------|------|
| `NoteRainGameManager` | ゲーム状態管理、StageManager/InstructionPanel統合 |
| `NoteController` | 音符生成・落下・受け皿・判定ロジック、入力処理 |
| `NoteRainUI` | スコア・コンボ・ライフ・判定テキスト表示 |
| `Note` | 個々の音符オブジェクト（落下・カーブ・加速対応） |

## 盤面・ステージデータ設計

```
// NoteController.SetupStage(StageConfig config, int stageIndex)
// stageIndex 0-4 → BPM, noteCount, spawnRange, enableFake, enableDouble, enableCurve

Stage 0: bpm=80, noteCount=15, spawnRangeRatio=0.4f, fake=false, double=false, curve=false
Stage 1: bpm=100, noteCount=25, spawnRangeRatio=0.85f, fake=false, double=false, curve=false
Stage 2: bpm=120, noteCount=35, spawnRangeRatio=0.85f, fake=true, double=true, curve=false
Stage 3: bpm=140, noteCount=45, spawnRangeRatio=0.9f, fake=true, double=true, accelerating=true
Stage 4: bpm=160, noteCount=60, spawnRangeRatio=0.95f, fake=true, double=true, curve=true, accelerating=true
```

## 入力処理フロー

- `NoteController.Update()` で `Mouse.current.leftButton.isPressed` + `Mouse.current.position.ReadValue()` を使用
- `Mouse.current.delta.ReadValue()` で受け皿移動量を計算（画面→ワールド変換）
- ドラッグ開始座標は記録し、受け皿がカメラ範囲外に出ないようクランプ

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 6.0f
float camWidth = camSize * Camera.main.aspect;
// 判定ライン: y = -camSize + 2.5f (下端から2.5u上)
// 受け皿スポーン: 判定ライン同じY
// 音符スポーン: y = camSize - 0.5f (上端から0.5u下)
// 受け皿移動範囲: -camWidth*spawnRangeRatio 〜 +camWidth*spawnRangeRatio
```

## SceneSetup 構成方針

- `[MenuItem("Assets/Setup/074v2 NoteRain")]`
- カメラ背景: 深い紺色 (0.04, 0.04, 0.15)
- スプライト: `Assets/Resources/Sprites/Game074v2_NoteRain/`
- 落下ノーツはランタイムで動的生成（プレハブでなくSpriteRenderer付きGameObject）
- 判定ラインはSpriteRendererの横長スプライトで可視化

## StageManager統合

```csharp
void OnStageChanged(int stageIndex)
{
    var config = _stageManager.GetCurrentStageConfig();
    _noteController.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## InstructionPanel内容

- title: "NoteRain"
- description: "降ってくる音符を受け皿でキャッチしてメロディを完成させよう"
- controls: "受け皿を左右にドラッグして音符を受け止めよう。タイミングよくキャッチするとPerfect！赤いフェイクノーツは避けてね"
- goal: "ライフを使い切らずに全ての音符をキャッチしてステージクリア！"

## ビジュアルフィードバック設計

1. **キャッチ成功時（Perfect/Great）**: 音符のスケールパルス（1.0 → 1.4 → 0.0、0.15秒）+ 色フラッシュ（白→元色）
2. **Miss時**: 音符が赤くフラッシュしながら消える + カメラシェイク（0.1秒、0.1f振幅）
3. **コンボ50以上**: コンボテキストが虹色アニメーション

## スコアシステム

- Perfect: 120pt × max(1.0, 1.0 + combo×0.12) ≤ 3.0
- Great: 70pt × max(1.0, 1.0 + combo×0.06) ≤ 2.0
- Good: 30pt（倍率なし）
- Miss: 0pt、コンボリセット、ライフ-1

## ステージ別新ルール表

| Stage | 新要素 | 詳細 |
|-------|-------|------|
| 1 | 基本ルール | 中央寄り落下（横幅40%）、BPM80 |
| 2 | 全幅展開 | 画面全幅（85%）に落下範囲拡大 |
| 3 | フェイク/同時 | 赤いフェイクノーツ + 同時2ノーツ |
| 4 | 加速ノーツ | 途中で速度が1.5倍になるノーツ |
| 5 | カーブ落下 | 斜め軌道で着地点が変化するノーツ |

## 判断ポイントの実装設計

- **先読み**: 次の音符スポーン予告インジケーター（薄く次の落下位置を示す）
- **フェイク識別**: フェイクノーツは赤色（Color.red）、本物は通常色（カテゴリ色）
- **同時ノーツ**: 0.1秒以内に2つスポーン、どちらをキャッチするか選択が必要

## Note クラス設計

```csharp
public enum NoteType { Normal, Fake, Accelerating, Curve }
public class Note : MonoBehaviour
{
    public NoteType noteType;
    public float fallSpeed;      // 初期落下速度
    public float curveOffset;    // カーブ用X方向オフセット速度
    bool _accelerated;           // 加速済みフラグ

    void Update()
    {
        // 加速ノーツ: Y < 0 になったら速度1.5倍
        if (noteType == NoteType.Accelerating && !_accelerated && transform.position.y < 0)
        {
            fallSpeed *= 1.5f;
            _accelerated = true;
        }
        // カーブノーツ: X方向にも移動
        transform.position += new Vector3(curveOffset, -fallSpeed, 0) * Time.deltaTime;
    }
}
```
