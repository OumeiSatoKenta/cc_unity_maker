using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game074_NoteRain
{
    public class RainManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private NoteRainGameManager _gameManager;
        [SerializeField, Tooltip("音符スプライト")] private Sprite _noteSprite;
        [SerializeField, Tooltip("キャッチャースプライト")] private Sprite _catcherSprite;
        [SerializeField, Tooltip("落下速度")] private float _fallSpeed = 3f;
        [SerializeField, Tooltip("スポーン間隔")] private float _spawnInterval = 0.8f;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject _catcher;
        private float _catcherY = -4f;
        private float _spawnTimer;
        private int _totalNotes;
        private int _spawnedCount;
        private List<GameObject> _notes = new List<GameObject>();

        private static readonly Color[] NoteColors = {
            new Color(1f, 0.3f, 0.3f), new Color(0.3f, 0.8f, 1f),
            new Color(1f, 0.8f, 0.2f), new Color(0.5f, 1f, 0.4f),
            new Color(1f, 0.5f, 0.8f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame(int totalNotes)
        {
            _isActive = true;
            _totalNotes = totalNotes;
            _spawnedCount = 0;
            _spawnTimer = 0.5f;

            _catcher = new GameObject("Catcher");
            _catcher.transform.position = new Vector3(0f, _catcherY, 0f);
            var sr = _catcher.AddComponent<SpriteRenderer>();
            sr.sprite = _catcherSprite; sr.sortingOrder = 5;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Move catcher with mouse
            if (Mouse.current != null)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);
                if (_catcher != null)
                {
                    var pos = _catcher.transform.position;
                    pos.x = Mathf.Clamp(wp.x, -4f, 4f);
                    _catcher.transform.position = pos;
                }
            }

            // Spawn notes
            if (_spawnedCount < _totalNotes)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    SpawnNote();
                    _spawnTimer = _spawnInterval;
                }
            }

            // Move and check notes
            for (int i = _notes.Count - 1; i >= 0; i--)
            {
                if (_notes[i] == null) { _notes.RemoveAt(i); continue; }
                _notes[i].transform.position += Vector3.down * _fallSpeed * Time.deltaTime;

                // Check catch
                if (_catcher != null && _notes[i].transform.position.y <= _catcherY + 0.5f
                    && _notes[i].transform.position.y >= _catcherY - 0.3f)
                {
                    float dist = Mathf.Abs(_notes[i].transform.position.x - _catcher.transform.position.x);
                    if (dist < 1f)
                    {
                        Destroy(_notes[i]);
                        _notes.RemoveAt(i);
                        _gameManager.OnNoteCaught();
                        continue;
                    }
                }

                // Check miss
                if (_notes[i].transform.position.y < _catcherY - 1.5f)
                {
                    Destroy(_notes[i]);
                    _notes.RemoveAt(i);
                    _gameManager.OnNoteMissed();
                }
            }
        }

        private void SpawnNote()
        {
            float x = Random.Range(-3.5f, 3.5f);
            var obj = new GameObject($"Note_{_spawnedCount}");
            obj.transform.position = new Vector3(x, 6f, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _noteSprite; sr.sortingOrder = 3;
            sr.color = NoteColors[_spawnedCount % NoteColors.Length];
            _notes.Add(obj);
            _spawnedCount++;
        }
    }
}
