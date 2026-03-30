using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game011_FoldPaper
{
    /// <summary>
    /// 紙の折り畳み処理と入力管理を一元管理する。
    /// グリッドベースの紙をセルで表現し、折り線をタップして折る方向を選択する。
    /// </summary>
    public class PaperManager : MonoBehaviour
    {
        [SerializeField, Tooltip("セルのプレハブ")]
        private GameObject _cellPrefab;

        [SerializeField, Tooltip("折り線のプレハブ")]
        private GameObject _foldLinePrefab;

        [SerializeField, Tooltip("ターゲット表示用プレハブ")]
        private GameObject _targetCellPrefab;

        [SerializeField, Tooltip("セルのサイズ")]
        private float _cellSize = 0.8f;

        public const int TotalStages = 5;

        private int _gridWidth;
        private int _gridHeight;
        private bool[,] _paperGrid; // true = 紙がある
        private int[,] _layerCount; // セル毎の重なり枚数
        private bool[,] _targetGrid; // 目標形状
        private int[,] _targetLayers; // 目標の重なり枚数

        private readonly List<GameObject> _cellObjects = new List<GameObject>();
        private readonly List<GameObject> _foldLineObjects = new List<GameObject>();
        private readonly List<GameObject> _targetObjects = new List<GameObject>();
        private readonly Stack<FoldAction> _undoStack = new Stack<FoldAction>();

        private FoldPaperGameManager _gameManager;

        // 折り線の情報
        private struct FoldLineInfo
        {
            public int position;     // グリッド上の位置
            public bool isHorizontal; // true=横線(上下に折る), false=縦線(左右に折る)
            public GameObject lineObj;
        }

        private readonly List<FoldLineInfo> _activeFoldLines = new List<FoldLineInfo>();

        // 折り操作の記録（Undo用）
        private struct FoldAction
        {
            public bool[,] previousGrid;
            public int[,] previousLayers;
        }

        private void Start()
        {
            _gameManager = GetComponentInParent<FoldPaperGameManager>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

            // 折り線との当たり判定
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            for (int i = 0; i < _activeFoldLines.Count; i++)
            {
                if (_activeFoldLines[i].lineObj == hit.gameObject)
                {
                    ExecuteFold(_activeFoldLines[i]);
                    break;
                }
            }
        }

        public void InitializeStage(int stageIndex)
        {
            ClearAll();
            _undoStack.Clear();

            var stageData = GetStageData(stageIndex);
            _gridWidth = stageData.width;
            _gridHeight = stageData.height;
            _paperGrid = new bool[_gridWidth, _gridHeight];
            _layerCount = new int[_gridWidth, _gridHeight];
            _targetGrid = stageData.target;
            _targetLayers = stageData.targetLayers;

            // 初期状態: 全セルに紙がある
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _paperGrid[x, y] = true;
                    _layerCount[x, y] = 1;
                }
            }

            RebuildVisuals();
            CreateFoldLines();
            ShowTarget();
        }

        private void ExecuteFold(FoldLineInfo foldLine)
        {
            // Undo用に現在の状態を保存
            var action = new FoldAction
            {
                previousGrid = (bool[,])_paperGrid.Clone(),
                previousLayers = (int[,])_layerCount.Clone()
            };
            _undoStack.Push(action);

            if (foldLine.isHorizontal)
            {
                // 横線: 上半分を下に折り畳む（foldLine.position より上を下へ）
                int foldY = foldLine.position;
                for (int x = 0; x < _gridWidth; x++)
                {
                    for (int y = foldY; y < _gridHeight; y++)
                    {
                        if (!_paperGrid[x, y]) continue;
                        int mirrorY = 2 * foldY - 1 - y;
                        if (mirrorY >= 0 && mirrorY < _gridHeight)
                        {
                            _layerCount[x, mirrorY] += _layerCount[x, y];
                            _paperGrid[x, mirrorY] = true;
                        }
                        _paperGrid[x, y] = false;
                        _layerCount[x, y] = 0;
                    }
                }
            }
            else
            {
                // 縦線: 右半分を左に折り畳む（foldLine.position より右を左へ）
                int foldX = foldLine.position;
                for (int x = foldX; x < _gridWidth; x++)
                {
                    for (int y = 0; y < _gridHeight; y++)
                    {
                        if (!_paperGrid[x, y]) continue;
                        int mirrorX = 2 * foldX - 1 - x;
                        if (mirrorX >= 0 && mirrorX < _gridWidth)
                        {
                            _layerCount[mirrorX, y] += _layerCount[x, y];
                            _paperGrid[mirrorX, y] = true;
                        }
                        _paperGrid[x, y] = false;
                        _layerCount[x, y] = 0;
                    }
                }
            }

            RebuildVisuals();
            CreateFoldLines();

            if (_gameManager != null) _gameManager.OnFolded();
        }

        public bool UndoLastFold()
        {
            if (_undoStack.Count == 0) return false;

            var action = _undoStack.Pop();
            _paperGrid = action.previousGrid;
            _layerCount = action.previousLayers;

            RebuildVisuals();
            CreateFoldLines();
            return true;
        }

        public bool CheckMatchesTarget()
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_targetGrid[x, y] != _paperGrid[x, y]) return false;
                    if (_targetGrid[x, y] && _targetLayers[x, y] != _layerCount[x, y]) return false;
                }
            }
            return true;
        }

        private void RebuildVisuals()
        {
            // セルオブジェクトを破棄して再構築
            foreach (var obj in _cellObjects) if (obj != null) Destroy(obj);
            _cellObjects.Clear();

            float offsetX = -(_gridWidth - 1) * _cellSize / 2f;
            float offsetY = -(_gridHeight - 1) * _cellSize / 2f - 1.0f; // 少し下にずらす

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (!_paperGrid[x, y]) continue;

                    Vector3 pos = new Vector3(
                        offsetX + x * _cellSize,
                        offsetY + y * _cellSize,
                        0);

                    GameObject cell;
                    if (_cellPrefab != null)
                    {
                        cell = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
                    }
                    else
                    {
                        cell = new GameObject($"Cell_{x}_{y}");
                        cell.transform.position = pos;
                        cell.transform.SetParent(transform);
                        var sr = cell.AddComponent<SpriteRenderer>();
                        sr.sprite = Resources.Load<Sprite>("Sprites/Game011_FoldPaper/paper_cell");
                    }

                    // 重なり枚数に応じて色の濃さを変える
                    var renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        float intensity = Mathf.Clamp01(1f - (_layerCount[x, y] - 1) * 0.15f);
                        renderer.color = new Color(intensity, intensity * 0.95f, intensity * 0.85f, 1f);
                        renderer.sortingOrder = _layerCount[x, y];
                    }

                    // レイヤー数テキスト表示
                    if (_layerCount[x, y] > 1)
                    {
                        var textObj = new GameObject("LayerText");
                        textObj.transform.SetParent(cell.transform);
                        textObj.transform.localPosition = Vector3.zero;
                        var tm = textObj.AddComponent<TextMesh>();
                        tm.text = _layerCount[x, y].ToString();
                        tm.characterSize = 0.15f;
                        tm.fontSize = 48;
                        tm.anchor = TextAnchor.MiddleCenter;
                        tm.alignment = TextAlignment.Center;
                        tm.color = new Color(0.3f, 0.2f, 0.1f);
                        textObj.GetComponent<MeshRenderer>().sortingOrder = 100;
                    }

                    _cellObjects.Add(cell);
                }
            }
        }

        private void CreateFoldLines()
        {
            // 既存の折り線を破棄
            foreach (var fl in _foldLineObjects) if (fl != null) Destroy(fl);
            _foldLineObjects.Clear();
            _activeFoldLines.Clear();

            float offsetX = -(_gridWidth - 1) * _cellSize / 2f;
            float offsetY = -(_gridHeight - 1) * _cellSize / 2f - 1.0f;

            // 横の折り線（行と行の間）
            for (int y = 1; y < _gridHeight; y++)
            {
                bool hasAbove = false, hasBelow = false;
                for (int x = 0; x < _gridWidth; x++)
                {
                    if (_paperGrid[x, y]) hasAbove = true;
                    if (_paperGrid[x, y - 1]) hasBelow = true;
                }
                if (!hasAbove || !hasBelow) continue;

                float lineY = offsetY + (y - 0.5f) * _cellSize;
                var lineObj = CreateFoldLineObject(
                    new Vector3(offsetX + (_gridWidth - 1) * _cellSize / 2f, lineY, -0.1f),
                    new Vector3(_gridWidth * _cellSize * 0.9f, 0.15f, 1f),
                    true);

                _foldLineObjects.Add(lineObj);
                _activeFoldLines.Add(new FoldLineInfo
                {
                    position = y,
                    isHorizontal = true,
                    lineObj = lineObj
                });
            }

            // 縦の折り線（列と列の間）
            for (int x = 1; x < _gridWidth; x++)
            {
                bool hasLeft = false, hasRight = false;
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_paperGrid[x, y]) hasRight = true;
                    if (_paperGrid[x - 1, y]) hasLeft = true;
                }
                if (!hasLeft || !hasRight) continue;

                float lineX = offsetX + (x - 0.5f) * _cellSize;
                var lineObj = CreateFoldLineObject(
                    new Vector3(lineX, offsetY + (_gridHeight - 1) * _cellSize / 2f, -0.1f),
                    new Vector3(0.15f, _gridHeight * _cellSize * 0.9f, 1f),
                    false);

                _foldLineObjects.Add(lineObj);
                _activeFoldLines.Add(new FoldLineInfo
                {
                    position = x,
                    isHorizontal = false,
                    lineObj = lineObj
                });
            }
        }

        private GameObject CreateFoldLineObject(Vector3 position, Vector3 scale, bool isHorizontal)
        {
            GameObject lineObj;
            if (_foldLinePrefab != null)
            {
                lineObj = Instantiate(_foldLinePrefab, position, Quaternion.identity, transform);
            }
            else
            {
                lineObj = new GameObject("FoldLine");
                lineObj.transform.position = position;
                lineObj.transform.SetParent(transform);
                var sr = lineObj.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.Load<Sprite>("Sprites/Game011_FoldPaper/fold_line");
                sr.color = new Color(0.9f, 0.3f, 0.2f, 0.8f);
                sr.sortingOrder = 50;
            }

            lineObj.transform.localScale = scale;

            // クリック判定用コライダー
            var col = lineObj.AddComponent<BoxCollider2D>();
            // コライダーサイズはスケール前の基準で設定（タップしやすいよう少し大きめ）
            col.size = new Vector2(1f, 1f);

            return lineObj;
        }

        private void ShowTarget()
        {
            foreach (var obj in _targetObjects) if (obj != null) Destroy(obj);
            _targetObjects.Clear();

            float targetOffsetX = 3.5f; // 右側にターゲットを表示
            float offsetY = -(_gridHeight - 1) * _cellSize / 2f - 1.0f;
            float scale = 0.5f;

            // ターゲットラベル
            var labelObj = new GameObject("TargetLabel");
            labelObj.transform.SetParent(transform);
            labelObj.transform.position = new Vector3(targetOffsetX, offsetY + _gridHeight * _cellSize * scale / 2f + 0.5f, 0);
            var labelTm = labelObj.AddComponent<TextMesh>();
            labelTm.text = "目標";
            labelTm.characterSize = 0.2f;
            labelTm.fontSize = 36;
            labelTm.anchor = TextAnchor.MiddleCenter;
            labelTm.alignment = TextAlignment.Center;
            labelTm.color = Color.white;
            labelObj.GetComponent<MeshRenderer>().sortingOrder = 100;
            _targetObjects.Add(labelObj);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (!_targetGrid[x, y]) continue;

                    Vector3 pos = new Vector3(
                        targetOffsetX + (x - (_gridWidth - 1) / 2f) * _cellSize * scale,
                        offsetY + y * _cellSize * scale,
                        0);

                    var cell = new GameObject($"Target_{x}_{y}");
                    cell.transform.position = pos;
                    cell.transform.localScale = Vector3.one * scale;
                    cell.transform.SetParent(transform);
                    var sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = Resources.Load<Sprite>("Sprites/Game011_FoldPaper/paper_cell");

                    float intensity = Mathf.Clamp01(1f - (_targetLayers[x, y] - 1) * 0.15f);
                    sr.color = new Color(intensity * 0.8f, intensity * 0.9f, intensity, 0.7f);
                    sr.sortingOrder = 5;

                    // レイヤー数テキスト
                    if (_targetLayers[x, y] > 1)
                    {
                        var textObj = new GameObject("TargetLayerText");
                        textObj.transform.SetParent(cell.transform);
                        textObj.transform.localPosition = Vector3.zero;
                        var tm = textObj.AddComponent<TextMesh>();
                        tm.text = _targetLayers[x, y].ToString();
                        tm.characterSize = 0.15f;
                        tm.fontSize = 48;
                        tm.anchor = TextAnchor.MiddleCenter;
                        tm.alignment = TextAlignment.Center;
                        tm.color = new Color(0.1f, 0.2f, 0.3f);
                        textObj.GetComponent<MeshRenderer>().sortingOrder = 100;
                    }

                    _targetObjects.Add(cell);
                }
            }
        }

        private void ClearAll()
        {
            foreach (var obj in _cellObjects) if (obj != null) Destroy(obj);
            _cellObjects.Clear();
            foreach (var obj in _foldLineObjects) if (obj != null) Destroy(obj);
            _foldLineObjects.Clear();
            foreach (var obj in _targetObjects) if (obj != null) Destroy(obj);
            _targetObjects.Clear();
            _activeFoldLines.Clear();
        }

        // ========================================
        // ステージデータ定義
        // ========================================

        private struct StageData
        {
            public int width;
            public int height;
            public bool[,] target;
            public int[,] targetLayers;
        }

        private StageData GetStageData(int index)
        {
            switch (index)
            {
                case 0: return Stage0_SimpleHalf();
                case 1: return Stage1_Quarter();
                case 2: return Stage2_LShape();
                case 3: return Stage3_Triangle();
                case 4: return Stage4_Complex();
                default: return Stage0_SimpleHalf();
            }
        }

        // ステージ0: 4x2 → 2x2に折る（縦半分）
        private StageData Stage0_SimpleHalf()
        {
            var data = new StageData { width = 4, height = 2 };
            data.target = new bool[4, 2];
            data.targetLayers = new int[4, 2];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    data.target[x, y] = true;
                    data.targetLayers[x, y] = 2;
                }
            }
            return data;
        }

        // ステージ1: 4x4 → 2x2に折る（縦横半分ずつ）
        private StageData Stage1_Quarter()
        {
            var data = new StageData { width = 4, height = 4 };
            data.target = new bool[4, 4];
            data.targetLayers = new int[4, 4];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    data.target[x, y] = true;
                    data.targetLayers[x, y] = 4;
                }
            }
            return data;
        }

        // ステージ2: 4x3 → L字型に折る
        private StageData Stage2_LShape()
        {
            var data = new StageData { width = 4, height = 3 };
            data.target = new bool[4, 3];
            data.targetLayers = new int[4, 3];
            // 下2行の左半分 (2x2, layers=2)
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    data.target[x, y] = true;
                    data.targetLayers[x, y] = 2;
                }
            }
            // 上1行の左半分 (2x1, layers=1 → 折らないまま残る)
            for (int x = 0; x < 2; x++)
            {
                data.target[x, 2] = true;
                data.targetLayers[x, 2] = 1;
            }
            return data;
        }

        // ステージ3: 4x4 → 上を折って下半分を厚く
        private StageData Stage3_Triangle()
        {
            var data = new StageData { width = 4, height = 4 };
            data.target = new bool[4, 4];
            data.targetLayers = new int[4, 4];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    data.target[x, y] = true;
                    data.targetLayers[x, y] = 2;
                }
            }
            return data;
        }

        // ステージ4: 6x4 → 3回折りで小さくする
        private StageData Stage4_Complex()
        {
            var data = new StageData { width = 6, height = 4 };
            data.target = new bool[6, 4];
            data.targetLayers = new int[6, 4];
            // 左下3x2 に 4層
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    data.target[x, y] = true;
                    data.targetLayers[x, y] = 4;
                }
            }
            return data;
        }
    }
}
