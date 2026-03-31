using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game004_WordCrystal
{
    public class CrystalManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 5;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _crystalPrefab;

        private CrystalController[,] _crystals;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private WordCrystalGameManager _gameManager;
        private Camera _mainCamera;

        private string _targetWord;
        private int _currentLetterIndex;
        private Sprite _crystalSprite;
        private Sprite _brokenSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<WordCrystalGameManager>();
            _mainCamera = Camera.main;
            _crystalSprite = Resources.Load<Sprite>("Sprites/Game004_WordCrystal/crystal");
            _brokenSprite = Resources.Load<Sprite>("Sprites/Game004_WordCrystal/crystal_broken");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 screenPos = mouse.position.ReadValue();
                screenPos.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var crystal = hit.GetComponent<CrystalController>();
                    if (crystal != null && !crystal.IsBroken)
                    {
                        crystal.Break(_brokenSprite);
                        CheckLetter(crystal.Letter);
                    }
                }
            }
        }

        private void CheckLetter(char letter)
        {
            if (_currentLetterIndex >= _targetWord.Length) return;

            if (char.ToUpper(letter) == char.ToUpper(_targetWord[_currentLetterIndex]))
            {
                _currentLetterIndex++;
                if (_gameManager != null)
                    _gameManager.OnCorrectLetter(_currentLetterIndex, _targetWord.Length);

                if (_currentLetterIndex >= _targetWord.Length)
                {
                    if (_gameManager != null)
                        _gameManager.OnWordCompleted();
                }
            }
            else
            {
                if (_gameManager != null)
                    _gameManager.OnMiss();
            }
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _targetWord = GetTargetWord(stageIndex);
            _currentLetterIndex = 0;
            _crystals = new CrystalController[_gridWidth, _gridHeight];
            BuildGrid();
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _stageObjects.Clear();
        }

        private void BuildGrid()
        {
            var letters = GenerateLetters();
            int idx = 0;

            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    if (_crystalPrefab == null) continue;

                    var obj = Instantiate(_crystalPrefab, transform);
                    var gridPos = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gridPos);
                    obj.name = $"Crystal_{x}_{y}";

                    var ctrl = obj.GetComponent<CrystalController>();
                    if (ctrl != null)
                        ctrl.Initialize(gridPos, letters[idx], _crystalSprite);

                    _crystals[x, y] = ctrl;
                    _stageObjects.Add(obj);
                    idx++;
                }
            }
        }

        private char[] GenerateLetters()
        {
            int total = _gridWidth * _gridHeight;
            var letters = new char[total];
            var rand = new System.Random();

            // Place target word letters at random positions
            var positions = new List<int>();
            for (int i = 0; i < total; i++) positions.Add(i);
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            for (int i = 0; i < _targetWord.Length && i < positions.Count; i++)
            {
                letters[positions[i]] = char.ToUpper(_targetWord[i]);
            }

            // Fill remaining with random letters
            for (int i = 0; i < total; i++)
            {
                if (letters[i] == '\0')
                    letters[i] = (char)('A' + rand.Next(26));
            }

            return letters;
        }

        private string GetTargetWord(int stageIndex)
        {
            switch (stageIndex % StageCount)
            {
                case 0: return "CAT";
                case 1: return "LIGHT";
                case 2: return "PUZZLE";
                default: return "CAT";
            }
        }

        public string GetCurrentTargetWord()
        {
            return _targetWord;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_gridHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY - 1f, 0f);
        }
    }
}
