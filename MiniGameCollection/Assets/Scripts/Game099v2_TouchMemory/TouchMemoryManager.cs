using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game099v2_TouchMemory
{
    public class TouchMemoryManager : MonoBehaviour
    {
        [SerializeField] TouchMemoryGameManager _gameManager;
        [SerializeField] TouchMemoryUI _ui;

        // Panel sprites (assigned by SceneSetup)
        [SerializeField] Sprite[] _panelSprites;      // normal state per panel index
        [SerializeField] Sprite[] _panelLitSprites;   // lit state per panel index

        // Cached camera
        Camera _mainCamera;

        // Runtime state
        bool _isActive;
        bool _inputPhase;

        // Stage params
        int _panelCount;
        int _startPatternLength;
        int _roundCount;
        bool _colorChange;
        bool _reverseEven;
        float _speedMultiplier;

        // Round state
        int _currentRound;      // 0-based
        int _patternLength;
        List<int> _pattern = new List<int>();
        int _inputIndex;
        float _inputStartTime;

        // Coroutine tracking
        Coroutine _startRoundCoroutine;

        // Panel GameObjects
        List<GameObject> _panels = new List<GameObject>();
        List<SpriteRenderer> _panelRenderers = new List<SpriteRenderer>();
        List<BoxCollider2D> _panelColliders = new List<BoxCollider2D>();
        Color[] _panelBaseColors;

        // Panel color sets for color-change mode
        static readonly Color[] s_colorSetA = {
            new Color(0.12f, 0.59f, 0.86f, 1f),
            new Color(0.86f, 0.31f, 0.12f, 1f),
            new Color(0.16f, 0.71f, 0.31f, 1f),
            new Color(0.71f, 0.20f, 0.78f, 1f),
            new Color(0.86f, 0.71f, 0.04f, 1f),
            new Color(0.0f,  0.78f, 0.78f, 1f),
            new Color(0.78f, 0.24f, 0.39f, 1f),
            new Color(0.31f, 0.47f, 0.86f, 1f),
            new Color(0.47f, 0.78f, 0.16f, 1f),
        };

        Coroutine _playbackCoroutine;

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        void Update()
        {
            if (!_isActive || !_inputPhase) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (_mainCamera == null) { _mainCamera = Camera.main; if (_mainCamera == null) return; }

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreen);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            int panelIdx = _panelColliders.IndexOf(hit as BoxCollider2D);
            if (panelIdx < 0) return;

            OnPanelTapped(panelIdx);
        }

        public void SetActive(bool value)
        {
            _isActive = value;
            if (!value)
            {
                _inputPhase = false;
                StopAllCoroutines();
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _speedMultiplier = Mathf.Max(config.speedMultiplier, 0.1f);

            // Parse customData: "panelCount,startPatternLength,roundCount,colorChange,reverseEven"
            ParseStageData(config.customData);

            _isActive = true;
            _inputPhase = false;
            _currentRound = 0;
            _patternLength = _startPatternLength;

            BuildPanels();
            _startRoundCoroutine = StartCoroutine(StartRoundDelayed(0.5f));
        }

        void ParseStageData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _panelCount = 4;
                _startPatternLength = 2;
                _roundCount = 5;
                _colorChange = false;
                _reverseEven = false;
                return;
            }
            var parts = data.Split(',');
            _panelCount = parts.Length > 0 && int.TryParse(parts[0].Trim(), out int pc) ? pc : 4;
            _startPatternLength = parts.Length > 1 && int.TryParse(parts[1].Trim(), out int spl) ? spl : 2;
            _roundCount = parts.Length > 2 && int.TryParse(parts[2].Trim(), out int rc) ? rc : 5;
            _colorChange = parts.Length > 3 && parts[3].Trim().ToLower() == "true";
            _reverseEven = parts.Length > 4 && parts[4].Trim().ToLower() == "true";
            // Clamp to valid ranges
            _panelCount = Mathf.Max(_panelCount, 2);
            _startPatternLength = Mathf.Max(_startPatternLength, 1);
            _roundCount = Mathf.Max(_roundCount, 1);
        }

        void BuildPanels()
        {
            // Destroy existing panels
            foreach (var p in _panels)
                if (p != null) Destroy(p);
            _panels.Clear();
            _panelRenderers.Clear();
            _panelColliders.Clear();

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) { Debug.LogError("[TouchMemoryManager] No main camera found."); return; }

            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float areaTop = camSize - topMargin;

            int cols = Mathf.CeilToInt(Mathf.Sqrt(_panelCount));
            int rows = Mathf.CeilToInt((float)_panelCount / cols);
            float maxCell = 2.0f;
            float cellSize = Mathf.Min(availableHeight / rows, camWidth * 2f / cols * 0.9f, maxCell);
            float gap = cellSize * 0.15f;
            float gridW = cols * (cellSize + gap) - gap;
            float gridH = rows * (cellSize + gap) - gap;
            float startX = -gridW * 0.5f + cellSize * 0.5f;
            float startY = areaTop - (availableHeight - gridH) * 0.5f - cellSize * 0.5f;

            _panelBaseColors = new Color[_panelCount];

            for (int i = 0; i < _panelCount; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float px = startX + col * (cellSize + gap);
                float py = startY - row * (cellSize + gap);

                var go = new GameObject($"Panel_{i}");
                go.transform.position = new Vector3(px, py, 0f);
                go.transform.localScale = Vector3.one * cellSize;

                var sr = go.AddComponent<SpriteRenderer>();
                int sprIdx = i % (_panelSprites != null ? _panelSprites.Length : 1);
                sr.sprite = (_panelSprites != null && sprIdx < _panelSprites.Length) ? _panelSprites[sprIdx] : null;
                sr.sortingOrder = 5;
                _panelBaseColors[i] = s_colorSetA[i % s_colorSetA.Length];
                sr.color = _panelBaseColors[i];

                var col2d = go.AddComponent<BoxCollider2D>();
                col2d.size = Vector2.one;

                _panels.Add(go);
                _panelRenderers.Add(sr);
                _panelColliders.Add(col2d);
            }
        }

        void RefreshPanelColors()
        {
            // Shift color assignment by round for color-change mode
            for (int i = 0; i < _panelCount; i++)
            {
                int offset = (_currentRound + i) % s_colorSetA.Length;
                _panelBaseColors[i] = s_colorSetA[offset];
                if (_panelRenderers[i] != null)
                    _panelRenderers[i].color = _panelBaseColors[i];
            }
        }

        IEnumerator StartRoundDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            _startRoundCoroutine = null;
            StartRound();
        }

        void StartRound()
        {
            if (!_isActive) return;

            if (_colorChange) RefreshPanelColors();

            bool isReverse = _reverseEven && (_currentRound % 2 == 1); // 0-based: round 1,3,... are even in display
            _ui?.UpdateRound(_currentRound + 1);
            _ui?.SetReverseIndicator(isReverse);

            // Generate pattern
            _pattern.Clear();
            for (int i = 0; i < _patternLength; i++)
                _pattern.Add(Random.Range(0, _panelCount));

            _inputIndex = 0;
            _inputPhase = false;

            if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = StartCoroutine(PlaybackPattern(isReverse));
        }

        IEnumerator PlaybackPattern(bool isReverse)
        {
            float litDuration = 0.4f * _speedMultiplier;
            float interval = 0.6f * _speedMultiplier;

            // Show the pattern to memorize (in order or reverse)
            var displayOrder = new List<int>(_pattern);
            if (isReverse) displayOrder.Reverse();

            foreach (int idx in displayOrder)
            {
                if (!_isActive) yield break;
                yield return LightPanel(idx, litDuration);
                yield return new WaitForSeconds(interval - litDuration);
            }

            // Begin input phase
            _inputStartTime = Time.time;
            _inputPhase = true;
        }

        IEnumerator LightPanel(int idx, float duration)
        {
            if (idx < 0 || idx >= _panels.Count) yield break;
            var sr = _panelRenderers[idx];
            var go = _panels[idx];
            if (sr == null || go == null) yield break;

            // Swap to lit sprite
            if (_panelLitSprites != null && idx < _panelLitSprites.Length)
                sr.sprite = _panelLitSprites[idx % _panelLitSprites.Length];

            // Scale pulse
            Vector3 orig = go.transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1.0f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1.0f, (t - 0.5f) * 2f);
                go.transform.localScale = orig * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            go.transform.localScale = orig;

            // Swap back to normal sprite
            if (_panelSprites != null && idx < _panelSprites.Length)
                sr.sprite = _panelSprites[idx % _panelSprites.Length];
            sr.color = _panelBaseColors[idx];
        }

        void OnPanelTapped(int panelIdx)
        {
            bool isReverse = _reverseEven && (_currentRound % 2 == 1);
            int expectedIdx = isReverse
                ? _pattern[_pattern.Count - 1 - _inputIndex]
                : _pattern[_inputIndex];

            if (panelIdx == expectedIdx)
            {
                // Correct
                StartCoroutine(FlashPanel(panelIdx, new Color(0.3f, 1f, 0.3f, 1f), 0.15f));
                _inputIndex++;

                if (_inputIndex >= _pattern.Count)
                {
                    // Round cleared
                    _inputPhase = false;
                    float inputTime = Time.time - _inputStartTime;
                    _gameManager?.OnRoundCleared(_currentRound + 1, _patternLength, inputTime);

                    _currentRound++;
                    if (_currentRound >= _roundCount)
                    {
                        // Stage clear
                        _isActive = false;
                        _gameManager?.OnStageClear(_currentRound + 1); // pass display round number for UI
                    }
                    else
                    {
                        _patternLength++;
                        _startRoundCoroutine = StartCoroutine(StartRoundDelayed(0.8f));
                    }
                }
            }
            else
            {
                // Wrong
                _inputPhase = false;
                _isActive = false;
                StartCoroutine(FlashPanel(panelIdx, new Color(1f, 0.2f, 0.2f, 1f), 0.2f));
                StartCoroutine(CameraShake(0.1f, 0.3f));
                StartCoroutine(NotifyMissDelayed(0.4f));
            }
        }

        IEnumerator FlashPanel(int idx, Color flashColor, float duration)
        {
            if (idx < 0 || idx >= _panelRenderers.Count) yield break;
            var sr = _panelRenderers[idx];
            if (sr == null) yield break;
            Color orig = _panelBaseColors[idx];
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (sr != null) sr.color = orig;
        }

        IEnumerator CameraShake(float amplitude, float duration)
        {
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            if (cam == null) yield break;
            Vector3 origPos = cam.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (cam == null) yield break;
                float x = Random.Range(-amplitude, amplitude);
                float y = Random.Range(-amplitude, amplitude);
                cam.transform.localPosition = origPos + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (cam != null) cam.transform.localPosition = origPos;
        }

        IEnumerator NotifyMissDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            _gameManager?.OnMissed();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            foreach (var p in _panels)
                if (p != null) Destroy(p);
            _panels.Clear();
        }
    }
}
