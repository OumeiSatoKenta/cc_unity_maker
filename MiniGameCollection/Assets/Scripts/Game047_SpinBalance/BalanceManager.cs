using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game047_SpinBalance
{
    public class BalanceManager : MonoBehaviour
    {
        [SerializeField] private SpinBalanceGameManager _gameManager;

        private const float BoardRadius = 3f;
        private const float PieceRadius = 0.25f;
        private const float SpawnInterval = 3f;
        private const float Gravity = 3f;
        private const float RotateSpeed = 120f;

        private GameObject _board;
        private List<GameObject> _pieces = new List<GameObject>();
        private List<Vector2> _pieceVelocities = new List<Vector2>();
        private Sprite _boardSprite;
        private Sprite _pieceSprite;
        private Camera _mainCamera;
        private float _boardAngle;
        private float _spawnTimer;

        public void Init()
        {
            _mainCamera = Camera.main;
            _boardSprite = Resources.Load<Sprite>("Sprites/Game047_SpinBalance/board");
            _pieceSprite = Resources.Load<Sprite>("Sprites/Game047_SpinBalance/piece");

            CleanUp();

            _board = new GameObject("Board");
            _board.transform.position = Vector3.zero;
            _board.transform.localScale = Vector3.one * (BoardRadius * 2f / 0.64f);
            var sr = _board.AddComponent<SpriteRenderer>();
            sr.sprite = _boardSprite;
            sr.sortingOrder = 0;

            _boardAngle = 0f;
            _spawnTimer = 0f;

            SpawnPiece();
        }

        private void CleanUp()
        {
            if (_board != null) Destroy(_board);
            foreach (var p in _pieces) if (p != null) Destroy(p);
            _pieces.Clear();
            _pieceVelocities.Clear();
        }

        private void SpawnPiece()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0f, BoardRadius * 0.5f);
            Vector2 pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

            var go = new GameObject("Piece");
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = Vector3.one * 1.2f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _pieceSprite;
            sr.sortingOrder = 5;
            float hue = Random.Range(0f, 1f);
            sr.color = Color.HSVToRGB(hue, 0.7f, 1f);

            _pieces.Add(go);
            _pieceVelocities.Add(Vector2.zero);

            if (_gameManager != null) _gameManager.OnPieceAdded(_pieces.Count);
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            HandleInput();
            UpdatePhysics();
            CheckFallen();

            _spawnTimer += Time.deltaTime;
            float interval = Mathf.Max(1f, SpawnInterval - _pieces.Count * 0.2f);
            if (_spawnTimer >= interval)
            {
                _spawnTimer = 0f;
                SpawnPiece();
            }
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.isPressed)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                if (wp.x < 0)
                    _boardAngle += RotateSpeed * Time.deltaTime;
                else
                    _boardAngle -= RotateSpeed * Time.deltaTime;
            }

            if (_board != null)
                _board.transform.rotation = Quaternion.Euler(0f, 0f, _boardAngle);
        }

        private void UpdatePhysics()
        {
            float angleRad = _boardAngle * Mathf.Deg2Rad;
            Vector2 gravDir = new Vector2(-Mathf.Sin(angleRad), -Mathf.Cos(angleRad));

            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] == null) continue;
                _pieceVelocities[i] += gravDir * Gravity * Time.deltaTime;
                _pieceVelocities[i] *= 0.98f;

                Vector2 pos = _pieces[i].transform.position;
                pos += _pieceVelocities[i] * Time.deltaTime;

                // Collision between pieces
                for (int j = 0; j < _pieces.Count; j++)
                {
                    if (i == j || _pieces[j] == null) continue;
                    Vector2 other = _pieces[j].transform.position;
                    Vector2 diff = pos - other;
                    float dist = diff.magnitude;
                    float minDist = PieceRadius * 2f;
                    if (dist < minDist && dist > 0.001f)
                    {
                        Vector2 push = diff.normalized * (minDist - dist) * 0.5f;
                        pos += push;
                        _pieceVelocities[i] += push * 2f;
                    }
                }

                _pieces[i].transform.position = new Vector3(pos.x, pos.y, 0f);
            }
        }

        private void CheckFallen()
        {
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                if (_pieces[i] == null) continue;
                Vector2 pos = _pieces[i].transform.position;
                if (pos.magnitude > BoardRadius + 0.5f)
                {
                    Destroy(_pieces[i]);
                    _pieces.RemoveAt(i);
                    _pieceVelocities.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnGameOver();
                    return;
                }
            }
        }
    }
}
