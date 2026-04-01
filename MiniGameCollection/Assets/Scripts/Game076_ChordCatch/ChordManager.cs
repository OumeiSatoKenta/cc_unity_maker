using UnityEngine;
using UnityEngine.InputSystem;

namespace Game076_ChordCatch
{
    public class ChordManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private ChordCatchGameManager _gameManager;
        [SerializeField, Tooltip("ボタンスプライト")] private Sprite _buttonSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _currentAnswer;
        private GameObject[] _buttons;
        private bool _waitingForAnswer;

        private static readonly string[] ChordNames = { "C Major", "A Minor", "G7", "D Minor", "F Major", "E Minor" };
        private static readonly Color[] ChordColors = {
            new Color(1f, 0.4f, 0.3f), new Color(0.3f, 0.6f, 1f),
            new Color(1f, 0.8f, 0.2f), new Color(0.5f, 0.3f, 0.8f),
            new Color(0.3f, 0.9f, 0.5f), new Color(1f, 0.5f, 0.7f)
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            CreateButtons();
            NextQuestion();
        }

        public void StopGame() { _isActive = false; }

        private void CreateButtons()
        {
            _buttons = new GameObject[6];
            for (int i = 0; i < 6; i++)
            {
                int row = i / 3, col = i % 3;
                float x = -2f + col * 2f;
                float y = -1.5f - row * 1.8f;

                var obj = new GameObject($"Chord_{i}");
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _buttonSprite; sr.sortingOrder = 3;
                sr.color = ChordColors[i];
                var col2 = obj.AddComponent<BoxCollider2D>();
                col2.size = new Vector2(0.75f, 0.55f);

                // Label
                var tObj = new GameObject("Label");
                tObj.transform.SetParent(obj.transform);
                tObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                var tm = tObj.AddComponent<TextMesh>();
                tm.text = ChordNames[i];
                tm.fontSize = 32; tm.characterSize = 0.1f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = Color.white;

                _buttons[i] = obj;
            }
        }

        public void NextQuestion()
        {
            _currentAnswer = Random.Range(0, 6);
            _waitingForAnswer = true;

            // Visual hint: highlight the "playing" indicator
            for (int i = 0; i < _buttons.Length; i++)
            {
                var sr = _buttons[i].GetComponent<SpriteRenderer>();
                sr.color = ChordColors[i];
            }
        }

        private void Update()
        {
            if (!_isActive || !_waitingForAnswer) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    for (int i = 0; i < _buttons.Length; i++)
                    {
                        if (_buttons[i] == hit.gameObject)
                        {
                            HandleAnswer(i);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleAnswer(int index)
        {
            _waitingForAnswer = false;

            // Flash feedback
            var sr = _buttons[index].GetComponent<SpriteRenderer>();
            sr.color = (index == _currentAnswer) ? Color.green : Color.red;

            if (index == _currentAnswer)
                _gameManager.OnCorrectAnswer();
            else
                _gameManager.OnWrongAnswer();
        }

        public string CurrentChordName => ChordNames[_currentAnswer];
    }
}
