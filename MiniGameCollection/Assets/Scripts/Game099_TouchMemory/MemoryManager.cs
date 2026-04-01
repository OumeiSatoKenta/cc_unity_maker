using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game099_TouchMemory
{
    public class MemoryManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private TouchMemoryGameManager _gameManager;
        [SerializeField, Tooltip("パネルスプライト")] private Sprite _panelSprite;

        private static readonly Vector3[] PanelPositions =
        {
            new(-1.5f, 1.5f, 0), new(1.5f, 1.5f, 0),
            new(-1.5f, -1.5f, 0), new(1.5f, -1.5f, 0)
        };

        private static readonly Color[] PanelColors =
        {
            new(0.9f, 0.2f, 0.2f, 1f), // 赤
            new(0.2f, 0.4f, 0.9f, 1f), // 青
            new(0.2f, 0.8f, 0.3f, 1f), // 緑
            new(0.9f, 0.8f, 0.2f, 1f)  // 黄
        };

        private static readonly Color[] PanelLitColors =
        {
            new(1f, 0.6f, 0.6f, 1f),
            new(0.6f, 0.7f, 1f, 1f),
            new(0.6f, 1f, 0.7f, 1f),
            new(1f, 1f, 0.6f, 1f)
        };

        private GameObject[] _panels;
        private SpriteRenderer[] _renderers;
        private Camera _mainCamera;
        private Coroutine _showCoroutine;

        private void Awake()
        {
            _mainCamera = Camera.main;
            CreatePanels();
        }

        private void CreatePanels()
        {
            _panels = new GameObject[4];
            _renderers = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                var obj = new GameObject($"Panel_{i}");
                obj.transform.SetParent(transform);
                obj.transform.position = PanelPositions[i];
                obj.transform.localScale = Vector3.one * 2.5f;
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _panelSprite;
                sr.color = PanelColors[i];
                sr.sortingOrder = 2;
                obj.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f);
                _panels[i] = obj;
                _renderers[i] = sr;
            }
        }

        public void ShowPattern(List<int> pattern)
        {
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowPatternCoroutine(pattern));
        }

        private IEnumerator ShowPatternCoroutine(List<int> pattern)
        {
            yield return new WaitForSeconds(0.5f);

            foreach (int idx in pattern)
            {
                _renderers[idx].color = PanelLitColors[idx];
                _panels[idx].transform.localScale = Vector3.one * 2.7f;
                yield return new WaitForSeconds(0.4f);
                _renderers[idx].color = PanelColors[idx];
                _panels[idx].transform.localScale = Vector3.one * 2.5f;
                yield return new WaitForSeconds(0.2f);
            }

            _gameManager.OnPatternShowComplete();
        }

        public void FlashPanel(int index)
        {
            StartCoroutine(FlashCoroutine(index, PanelLitColors[index]));
        }

        public void FlashPanelError(int index)
        {
            StartCoroutine(FlashCoroutine(index, new Color(0.5f, 0.5f, 0.5f)));
        }

        private IEnumerator FlashCoroutine(int index, Color color)
        {
            _renderers[index].color = color;
            _panels[index].transform.localScale = Vector3.one * 2.7f;
            yield return new WaitForSeconds(0.2f);
            _renderers[index].color = PanelColors[index];
            _panels[index].transform.localScale = Vector3.one * 2.5f;
        }

        private void Update()
        {
            if (_gameManager == null || _gameManager.State != GameState.Input) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                ProcessInput(Mouse.current.position.ReadValue());

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

            for (int i = 0; i < 4; i++)
            {
                if (_panels[i] != null && hit.gameObject == _panels[i])
                {
                    _gameManager.OnPanelTapped(i);
                    return;
                }
            }
        }
    }
}
