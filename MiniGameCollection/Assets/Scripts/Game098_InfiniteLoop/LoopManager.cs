using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game098_InfiniteLoop
{
    public class LoopManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private InfiniteLoopGameManager _gameManager;
        [SerializeField, Tooltip("オブジェクトスプライト(6個: 時計,花瓶,絵画,本棚,窓,ランプ)")]
        private Sprite[] _objectSprites;
        [SerializeField, Tooltip("変化後スプライト(本棚alt)")]
        private Sprite _bookshelfAltSprite;

        private static readonly Vector3[] Positions =
        {
            new(-2.5f, 1.5f, 0), new(0f, 1.5f, 0), new(2.5f, 1.5f, 0),
            new(-2.5f, -1.5f, 0), new(0f, -1.5f, 0), new(2.5f, -1.5f, 0)
        };

        private static readonly string[] ObjectNames =
            { "時計", "花瓶", "絵画", "本棚", "窓", "ランプ" };

        private GameObject[] _objects;
        private SpriteRenderer[] _renderers;
        private int _currentStage;
        private int _changedIndex = -1;
        private bool _inputEnabled = true;
        private Camera _mainCamera;

        // 各ステージの元の状態を保存
        private Vector3[] _originalPositions;
        private Vector3[] _originalScales;
        private Color[] _originalColors;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetupRoom(int stage)
        {
            _currentStage = stage;
            _inputEnabled = true;

            if (_objects == null)
            {
                _objects = new GameObject[6];
                _renderers = new SpriteRenderer[6];
                _originalPositions = new Vector3[6];
                _originalScales = new Vector3[6];
                _originalColors = new Color[6];

                for (int i = 0; i < 6; i++)
                {
                    var obj = new GameObject(ObjectNames[i]);
                    obj.transform.SetParent(transform);
                    obj.transform.position = Positions[i];
                    obj.transform.localScale = Vector3.one * 1.2f;
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = _objectSprites != null && i < _objectSprites.Length ? _objectSprites[i] : null;
                    sr.color = Color.white;
                    sr.sortingOrder = 2;
                    var col = obj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1f, 1f);
                    _objects[i] = obj;
                    _renderers[i] = sr;
                    _originalPositions[i] = Positions[i];
                    _originalScales[i] = Vector3.one * 1.2f;
                    _originalColors[i] = Color.white;
                }
            }
            else
            {
                // 全オブジェクトを元に戻す
                for (int i = 0; i < 6; i++)
                {
                    if (_objects[i] == null) continue;
                    _objects[i].SetActive(true);
                    _objects[i].transform.position = _originalPositions[i];
                    _objects[i].transform.localScale = _originalScales[i];
                    _renderers[i].color = _originalColors[i];
                    _renderers[i].sprite = _objectSprites != null && i < _objectSprites.Length ? _objectSprites[i] : null;
                }
            }

            // 現ステージの変化を適用
            ApplyChange(stage);
        }

        private void ApplyChange(int stage)
        {
            switch (stage)
            {
                case 0: // 時計の色が変わる
                    _changedIndex = 0;
                    _renderers[0].color = new Color(1f, 0.4f, 0.4f);
                    break;
                case 1: // 花瓶の位置がずれる
                    _changedIndex = 1;
                    _objects[1].transform.position = new Vector3(0.3f, 1.7f, 0f);
                    break;
                case 2: // 絵画が反転
                    _changedIndex = 2;
                    var s = _objects[2].transform.localScale;
                    _objects[2].transform.localScale = new Vector3(-s.x, s.y, s.z);
                    break;
                case 3: // 本棚の本が1冊消える
                    _changedIndex = 3;
                    if (_bookshelfAltSprite != null)
                        _renderers[3].sprite = _bookshelfAltSprite;
                    break;
                case 4: // 窓の色が変わる
                    _changedIndex = 4;
                    _renderers[4].color = new Color(0.4f, 0.5f, 1f);
                    break;
                default:
                    _changedIndex = -1;
                    break;
            }
        }

        private void Update()
        {
            if (!_inputEnabled || _gameManager == null || !_gameManager.IsPlaying) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                ProcessInput(Mouse.current.position.ReadValue());
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        ProcessInput(touch.position.ReadValue());
                        break;
                    }
                }
            }
        }

        private void ProcessInput(Vector2 screenPos)
        {
            if (_mainCamera == null) return;
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            int tappedIndex = -1;
            for (int i = 0; i < 6; i++)
            {
                if (_objects[i] != null && hit.gameObject == _objects[i])
                {
                    tappedIndex = i;
                    break;
                }
            }

            if (tappedIndex < 0) return;

            if (tappedIndex == _changedIndex)
            {
                _inputEnabled = false;
                // 正解エフェクト: スケールパルス
                StartCoroutine(CorrectEffect(tappedIndex));
            }
            else
            {
                _gameManager.OnWrongTap();
            }
        }

        private IEnumerator CorrectEffect(int index)
        {
            if (_objects[index] != null)
            {
                var t = _objects[index].transform;
                Vector3 orig = t.localScale;
                t.localScale = orig * 1.3f;
                yield return new WaitForSeconds(0.15f);
                if (t != null) t.localScale = orig;
            }
            yield return new WaitForSeconds(0.1f);
            _gameManager.OnCorrectTap();
        }

        public void PlayLoopTransition(int nextStage)
        {
            StartCoroutine(LoopTransitionCoroutine(nextStage));
        }

        private IEnumerator LoopTransitionCoroutine(int nextStage)
        {
            _inputEnabled = false;

            // フェードアウト
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = 1f - (elapsed / duration);
                for (int i = 0; i < 6; i++)
                    if (_renderers[i] != null) _renderers[i].color = new Color(_renderers[i].color.r, _renderers[i].color.g, _renderers[i].color.b, a);
                yield return null;
            }

            // 部屋をリセットして次の変化を適用
            SetupRoom(nextStage);

            // フェードイン前にアルファを0にリセット
            for (int i = 0; i < 6; i++)
                if (_renderers[i] != null)
                {
                    Color c = _renderers[i].color;
                    _renderers[i].color = new Color(c.r, c.g, c.b, 0f);
                }

            // フェードイン
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = elapsed / duration;
                for (int i = 0; i < 6; i++)
                    if (_renderers[i] != null)
                    {
                        Color c = _renderers[i].color;
                        _renderers[i].color = new Color(c.r, c.g, c.b, a);
                    }
                yield return null;
            }

            // アルファを確実に1に
            for (int i = 0; i < 6; i++)
                if (_renderers[i] != null)
                {
                    Color c = _renderers[i].color;
                    _renderers[i].color = new Color(c.r, c.g, c.b, 1f);
                }

            _inputEnabled = true;
        }

        public void ShakeCamera()
        {
            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            if (_mainCamera == null) yield break;
            Vector3 orig = _mainCamera.transform.position;
            float elapsed = 0f;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-0.1f, 0.1f);
                float y = Random.Range(-0.1f, 0.1f);
                _mainCamera.transform.position = orig + new Vector3(x, y, 0f);
                yield return null;
            }
            _mainCamera.transform.position = orig;
        }
    }
}
