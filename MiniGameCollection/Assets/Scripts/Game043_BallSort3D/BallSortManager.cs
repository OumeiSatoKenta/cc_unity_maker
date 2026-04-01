using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game043_BallSort3D
{
    public class BallSortManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private BallSortGameManager _gameManager;

        [SerializeField, Tooltip("チューブスプライト")]
        private Sprite _tubeSprite;

        [SerializeField, Tooltip("ボールスプライト配列 [0]=赤,[1]=青,[2]=緑,[3]=黄,[4]=紫")]
        private Sprite[] _ballSprites;

        private Camera _mainCamera;
        private int _selectedTube = -1;

        private const int NumColors = 4;
        private const int BallsPerTube = 4;
        private const int NumTubes = 6; // 4色 + 2空チューブ
        private const float TubeSpacing = 1.4f;
        private const float TubeStartX = -3.5f;
        private const float TubeY = -1f;
        private const float BallSize = 0.3f;
        private const float BallSpacing = 0.35f;

        // 各チューブのボール（下から上の順）
        private List<int>[] _tubes;
        private GameObject[,] _ballObjects; // [tube][slot]
        private GameObject[] _tubeObjects;
        private SpriteRenderer[] _tubeRenderers;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            InitializePuzzle();
            RenderAll();
        }

        private void InitializePuzzle()
        {
            _tubes = new List<int>[NumTubes];
            for (int i = 0; i < NumTubes; i++)
                _tubes[i] = new List<int>();

            // ボールをシャッフルして配置
            var allBalls = new List<int>();
            for (int c = 0; c < NumColors; c++)
                for (int j = 0; j < BallsPerTube; j++)
                    allBalls.Add(c);

            // Fisher-Yates シャッフル
            for (int i = allBalls.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allBalls[i], allBalls[j]) = (allBalls[j], allBalls[i]);
            }

            int idx = 0;
            for (int t = 0; t < NumColors; t++)
                for (int b = 0; b < BallsPerTube; b++)
                    _tubes[t].Add(allBalls[idx++]);
            // 最後の2チューブは空
        }

        private void RenderAll()
        {
            // チューブ描画
            _tubeObjects = new GameObject[NumTubes];
            _tubeRenderers = new SpriteRenderer[NumTubes];
            _ballObjects = new GameObject[NumTubes, BallsPerTube];

            for (int t = 0; t < NumTubes; t++)
            {
                float x = TubeStartX + t * TubeSpacing;
                var tubeObj = new GameObject($"Tube_{t}");
                tubeObj.transform.position = new Vector3(x, TubeY, 0f);
                tubeObj.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
                var sr = tubeObj.AddComponent<SpriteRenderer>();
                sr.sprite = _tubeSprite;
                sr.sortingOrder = 1;
                sr.color = Color.white;
                var col = tubeObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
                tubeObj.transform.SetParent(transform);
                _tubeObjects[t] = tubeObj;
                _tubeRenderers[t] = sr;

                // ボール描画
                for (int b = 0; b < _tubes[t].Count; b++)
                {
                    int colorIdx = _tubes[t][b];
                    var ballObj = new GameObject($"Ball_{t}_{b}");
                    float ballY = TubeY - 0.35f + b * BallSpacing;
                    ballObj.transform.position = new Vector3(x, ballY, 0f);
                    ballObj.transform.localScale = new Vector3(BallSize, BallSize, 1f);
                    var bsr = ballObj.AddComponent<SpriteRenderer>();
                    bsr.sprite = (_ballSprites != null && colorIdx < _ballSprites.Length) ? _ballSprites[colorIdx] : null;
                    bsr.sortingOrder = 3;
                    ballObj.transform.SetParent(transform);
                    _ballObjects[t, b] = ballObj;
                }
            }
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);

            var hit = Physics2D.OverlapPoint(wp);
            if (hit == null) return;

            int tappedTube = -1;
            for (int t = 0; t < NumTubes; t++)
                if (_tubeObjects[t] == hit.gameObject) { tappedTube = t; break; }
            if (tappedTube < 0) return;

            if (_selectedTube < 0)
            {
                // 選択
                if (_tubes[tappedTube].Count > 0)
                {
                    _selectedTube = tappedTube;
                    _tubeRenderers[tappedTube].color = new Color(1f, 1f, 0.5f);
                }
            }
            else
            {
                // 移動試行
                if (tappedTube == _selectedTube)
                {
                    // 選択解除
                    _tubeRenderers[_selectedTube].color = Color.white;
                    _selectedTube = -1;
                    return;
                }

                if (CanMove(_selectedTube, tappedTube))
                {
                    MoveBall(_selectedTube, tappedTube);
                    _gameManager.OnMove();
                    if (CheckSolved()) _gameManager.OnSolved();
                }

                _tubeRenderers[_selectedTube].color = Color.white;
                _selectedTube = -1;
            }
        }

        private bool CanMove(int from, int to)
        {
            if (_tubes[from].Count == 0) return false;
            if (_tubes[to].Count >= BallsPerTube) return false;
            if (_tubes[to].Count > 0 && _tubes[to][_tubes[to].Count - 1] != _tubes[from][_tubes[from].Count - 1])
                return false;
            return true;
        }

        private void MoveBall(int from, int to)
        {
            int ballColor = _tubes[from][_tubes[from].Count - 1];
            _tubes[from].RemoveAt(_tubes[from].Count - 1);
            _tubes[to].Add(ballColor);
            RefreshTubeVisuals(from);
            RefreshTubeVisuals(to);
        }

        private void RefreshTubeVisuals(int tube)
        {
            // 既存ボールを削除
            for (int b = 0; b < BallsPerTube; b++)
            {
                if (_ballObjects[tube, b] != null) Destroy(_ballObjects[tube, b]);
                _ballObjects[tube, b] = null;
            }

            float x = TubeStartX + tube * TubeSpacing;
            for (int b = 0; b < _tubes[tube].Count; b++)
            {
                int colorIdx = _tubes[tube][b];
                var ballObj = new GameObject($"Ball_{tube}_{b}");
                float ballY = TubeY - 0.35f + b * BallSpacing;
                ballObj.transform.position = new Vector3(x, ballY, 0f);
                ballObj.transform.localScale = new Vector3(BallSize, BallSize, 1f);
                var bsr = ballObj.AddComponent<SpriteRenderer>();
                bsr.sprite = (_ballSprites != null && colorIdx < _ballSprites.Length) ? _ballSprites[colorIdx] : null;
                bsr.sortingOrder = 3;
                ballObj.transform.SetParent(transform);
                _ballObjects[tube, b] = ballObj;
            }
        }

        private bool CheckSolved()
        {
            for (int t = 0; t < NumTubes; t++)
            {
                if (_tubes[t].Count == 0) continue;
                if (_tubes[t].Count != BallsPerTube) return false;
                int c = _tubes[t][0];
                for (int b = 1; b < BallsPerTube; b++)
                    if (_tubes[t][b] != c) return false;
            }
            return true;
        }
    }
}
