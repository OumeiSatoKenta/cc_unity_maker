using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace Game043_BallSort3D
{
    public class TubeManager : MonoBehaviour
    {
        [SerializeField] private BallSort3DGameManager _gameManager;

        private static readonly Color[] BallColors = {
            new Color(1f, 0.3f, 0.3f),
            new Color(0.3f, 0.6f, 1f),
            new Color(0.3f, 1f, 0.4f),
            new Color(1f, 0.8f, 0.2f),
            new Color(0.8f, 0.3f, 1f),
        };

        private const int TubeCapacity = 4;
        private const float TubeSpacing = 1.6f;
        private const float BallSize = 0.3f;
        private const float TubeBaseY = -2f;

        private List<List<int>> _tubes;
        private List<GameObject> _tubeObjects = new List<GameObject>();
        private List<List<GameObject>> _ballObjects = new List<List<GameObject>>();
        private int _selectedTube = -1;
        private GameObject _floatingBall;
        private Sprite _ballSprite;
        private Sprite _tubeSprite;
        private Camera _mainCamera;

        public void GenerateStage(int stage)
        {
            _mainCamera = Camera.main;
            _ballSprite = Resources.Load<Sprite>("Sprites/Game043_BallSort3D/ball");
            _tubeSprite = Resources.Load<Sprite>("Sprites/Game043_BallSort3D/tube");

            CleanUp();

            int colorCount = Mathf.Min(3 + stage / 2, BallColors.Length);
            int tubeCount = colorCount + 2;

            _tubes = new List<List<int>>();
            var allBalls = new List<int>();
            for (int c = 0; c < colorCount; c++)
                for (int i = 0; i < TubeCapacity; i++)
                    allBalls.Add(c);

            Shuffle(allBalls);

            int ballIdx = 0;
            for (int t = 0; t < tubeCount; t++)
            {
                var tube = new List<int>();
                if (t < colorCount)
                {
                    for (int i = 0; i < TubeCapacity; i++)
                        tube.Add(allBalls[ballIdx++]);
                }
                _tubes.Add(tube);
            }

            float totalWidth = (tubeCount - 1) * TubeSpacing;
            float startX = -totalWidth / 2f;

            for (int t = 0; t < tubeCount; t++)
            {
                float x = startX + t * TubeSpacing;
                var tubeObj = new GameObject("Tube_" + t);
                tubeObj.transform.position = new Vector3(x, TubeBaseY, 0f);
                var sr = tubeObj.AddComponent<SpriteRenderer>();
                sr.sprite = _tubeSprite;
                sr.sortingOrder = 1;
                sr.color = new Color(0.7f, 0.8f, 0.9f, 0.6f);
                tubeObj.transform.localScale = new Vector3(1.2f, 1f, 1f);
                var col = tubeObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.6f, 1.5f);
                col.offset = new Vector2(0f, 0.5f);
                _tubeObjects.Add(tubeObj);

                var balls = new List<GameObject>();
                for (int i = 0; i < _tubes[t].Count; i++)
                {
                    var ballObj = CreateBall(_tubes[t][i], x, TubeBaseY + 0.15f + i * BallSize);
                    balls.Add(ballObj);
                }
                _ballObjects.Add(balls);
            }

            _selectedTube = -1;
            if (_floatingBall != null) { Destroy(_floatingBall); _floatingBall = null; }
        }

        private void CleanUp()
        {
            foreach (var to in _tubeObjects) if (to != null) Destroy(to);
            _tubeObjects.Clear();
            foreach (var bl in _ballObjects) foreach (var b in bl) if (b != null) Destroy(b);
            _ballObjects.Clear();
            if (_floatingBall != null) { Destroy(_floatingBall); _floatingBall = null; }
        }

        private GameObject CreateBall(int colorIdx, float x, float y)
        {
            var go = new GameObject("Ball");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * 0.9f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _ballSprite;
            sr.color = BallColors[colorIdx];
            sr.sortingOrder = 5;
            return go;
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                int tappedTube = GetTubeAt(wp);
                if (tappedTube < 0) return;

                if (_selectedTube < 0)
                {
                    if (_tubes[tappedTube].Count > 0)
                    {
                        _selectedTube = tappedTube;
                        int topColor = _tubes[tappedTube][_tubes[tappedTube].Count - 1];
                        _tubes[tappedTube].RemoveAt(_tubes[tappedTube].Count - 1);

                        var topBall = _ballObjects[tappedTube][_ballObjects[tappedTube].Count - 1];
                        _ballObjects[tappedTube].RemoveAt(_ballObjects[tappedTube].Count - 1);
                        _floatingBall = topBall;
                        _floatingBall.transform.position = new Vector3(
                            _tubeObjects[tappedTube].transform.position.x,
                            TubeBaseY + 1.8f, 0f);
                    }
                }
                else
                {
                    if (tappedTube == _selectedTube)
                    {
                        PutBack();
                    }
                    else if (_tubes[tappedTube].Count < TubeCapacity)
                    {
                        int topColor = _floatingBall.GetComponent<SpriteRenderer>().color == BallColors[0] ? 0 :
                                       _floatingBall.GetComponent<SpriteRenderer>().color == BallColors[1] ? 1 :
                                       _floatingBall.GetComponent<SpriteRenderer>().color == BallColors[2] ? 2 :
                                       _floatingBall.GetComponent<SpriteRenderer>().color == BallColors[3] ? 3 : 4;

                        if (_tubes[tappedTube].Count == 0 || _tubes[tappedTube][_tubes[tappedTube].Count - 1] == topColor)
                        {
                            _tubes[tappedTube].Add(topColor);
                            float x = _tubeObjects[tappedTube].transform.position.x;
                            float y = TubeBaseY + 0.15f + (_tubes[tappedTube].Count - 1) * BallSize;
                            _floatingBall.transform.position = new Vector3(x, y, 0f);
                            _ballObjects[tappedTube].Add(_floatingBall);
                            _floatingBall = null;
                            _selectedTube = -1;
                            if (_gameManager != null) _gameManager.OnBallMoved();
                        }
                        else
                        {
                            PutBack();
                        }
                    }
                    else
                    {
                        PutBack();
                    }
                }
            }
        }

        private void PutBack()
        {
            if (_floatingBall == null || _selectedTube < 0) return;
            var sr = _floatingBall.GetComponent<SpriteRenderer>();
            int colorIdx = 0;
            for (int i = 0; i < BallColors.Length; i++)
            {
                if (sr.color == BallColors[i]) { colorIdx = i; break; }
            }
            _tubes[_selectedTube].Add(colorIdx);
            float x = _tubeObjects[_selectedTube].transform.position.x;
            float y = TubeBaseY + 0.15f + (_tubes[_selectedTube].Count - 1) * BallSize;
            _floatingBall.transform.position = new Vector3(x, y, 0f);
            _ballObjects[_selectedTube].Add(_floatingBall);
            _floatingBall = null;
            _selectedTube = -1;
        }

        private int GetTubeAt(Vector3 worldPos)
        {
            for (int i = 0; i < _tubeObjects.Count; i++)
            {
                if (Mathf.Abs(worldPos.x - _tubeObjects[i].transform.position.x) < 0.7f)
                    return i;
            }
            return -1;
        }

        public bool CheckAllSorted()
        {
            foreach (var tube in _tubes)
            {
                if (tube.Count == 0) continue;
                if (tube.Count != TubeCapacity) return false;
                int first = tube[0];
                if (tube.Any(b => b != first)) return false;
            }
            return true;
        }

        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
