using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game012_BridgeBuilder
{
    public class BridgeManager : MonoBehaviour
    {
        [SerializeField] private BridgeBuilderGameManager _gameManager;
        [SerializeField] private BridgeBuilderUI _ui;
        [SerializeField] private Transform _bridgeParent;
        [SerializeField] private Transform _carTransform;
        [SerializeField] private SpriteRenderer _carRenderer;

        private LevelData _levelData;
        private List<BridgePart> _placedParts = new();
        private int _remainingBudget;
        private int _selectedPartType; // 0=plank, 1=support
        private bool _isTestRunning;

        // Part types
        private const int PartPlank = 0;
        private const int PartSupport = 1;

        public void LoadLevel(LevelData data)
        {
            _levelData = data;
            _remainingBudget = data.Budget;
            _selectedPartType = PartPlank;
            _isTestRunning = false;

            // Clear existing parts
            foreach (var part in _placedParts)
            {
                if (part != null && part.gameObject != null)
                    Destroy(part.gameObject);
            }
            _placedParts.Clear();

            // Reset car
            if (_carTransform != null)
            {
                _carTransform.position = new Vector3(_levelData.LeftEdge.x - 1.5f, _levelData.LeftEdge.y + 0.5f, 0f);
                _carTransform.gameObject.SetActive(false);
            }
        }

        public void SelectPartType(int type)
        {
            _selectedPartType = type;
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying || _gameManager.IsTesting) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // Check if click is in the buildable area (between edges, slightly above bottom)
            float minX = _levelData.LeftEdge.x;
            float maxX = _levelData.RightEdge.x;
            float minY = Mathf.Min(_levelData.LeftEdge.y, _levelData.RightEdge.y) - 2f;
            float maxY = Mathf.Max(_levelData.LeftEdge.y, _levelData.RightEdge.y) + 1f;

            if (worldPos.x < minX || worldPos.x > maxX || worldPos.y < minY || worldPos.y > maxY)
                return;

            if (_remainingBudget <= 0) return;

            PlacePart(worldPos);
        }

        private void PlacePart(Vector2 position)
        {
            // Snap to grid (0.5 unit increments)
            float snapX = Mathf.Round(position.x * 2f) / 2f;
            float snapY = Mathf.Round(position.y * 2f) / 2f;
            Vector2 snapped = new Vector2(snapX, snapY);

            // Check for overlap with existing parts
            foreach (var existing in _placedParts)
            {
                if (existing != null && Vector2.Distance(existing.Position, snapped) < 0.3f)
                    return;
            }

            var partObj = new GameObject(_selectedPartType == PartPlank ? "Plank" : "Support");
            partObj.transform.SetParent(_bridgeParent);
            partObj.transform.position = new Vector3(snapped.x, snapped.y, 0f);

            var sr = partObj.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>(
                _selectedPartType == PartPlank
                    ? "Sprites/Game012_BridgeBuilder/plank"
                    : "Sprites/Game012_BridgeBuilder/support");
            sr.sortingOrder = 5;

            if (_selectedPartType == PartPlank)
            {
                partObj.transform.localScale = new Vector3(1.5f, 0.25f, 1f);
                sr.color = new Color(0.65f, 0.45f, 0.2f);
            }
            else
            {
                partObj.transform.localScale = new Vector3(0.2f, 1.2f, 1f);
                sr.color = new Color(0.5f, 0.5f, 0.55f);
            }

            var collider = partObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var part = partObj.AddComponent<BridgePart>();
            part.Init(_selectedPartType, snapped);
            _placedParts.Add(part);

            _remainingBudget--;
            _ui?.SetBudgetText(_remainingBudget);
        }

        public void UndoLastPart()
        {
            if (_placedParts.Count == 0 || _isTestRunning) return;
            var last = _placedParts[_placedParts.Count - 1];
            _placedParts.RemoveAt(_placedParts.Count - 1);
            if (last != null && last.gameObject != null)
                Destroy(last.gameObject);
            _remainingBudget++;
            _ui?.SetBudgetText(_remainingBudget);
        }

        public void StartTest()
        {
            _isTestRunning = true;
            StartCoroutine(RunTest());
        }

        private IEnumerator RunTest()
        {
            // Evaluate bridge structure
            bool hasPath = EvaluateBridge();

            if (!hasPath)
            {
                yield return StartCoroutine(ShowFailure());
                _isTestRunning = false;
                _gameManager.OnTestResult(false);
                yield break;
            }

            // Animate car crossing
            yield return StartCoroutine(AnimateCarCrossing());

            _isTestRunning = false;
            _gameManager.OnTestResult(true);
        }

        private bool EvaluateBridge()
        {
            if (_placedParts.Count == 0) return false;

            // Check that planks form a continuous path from left to right
            int plankCount = 0;
            int supportCount = 0;
            float leftMost = float.MaxValue;
            float rightMost = float.MinValue;

            foreach (var part in _placedParts)
            {
                if (part.PartType == PartPlank)
                {
                    plankCount++;
                    float partLeft = part.Position.x - 0.75f;
                    float partRight = part.Position.x + 0.75f;
                    if (partLeft < leftMost) leftMost = partLeft;
                    if (partRight > rightMost) rightMost = partRight;
                }
                else
                {
                    supportCount++;
                }
            }

            // Need enough planks to span the gap
            bool spansGap = leftMost <= _levelData.LeftEdge.x + 0.5f &&
                            rightMost >= _levelData.RightEdge.x - 0.5f;

            // Need required supports
            bool hasSupports = supportCount >= _levelData.RequiredSupports;

            // Planks must be roughly at the right height
            bool heightOk = true;
            foreach (var part in _placedParts)
            {
                if (part.PartType == PartPlank)
                {
                    float expectedY = Mathf.Lerp(_levelData.LeftEdge.y, _levelData.RightEdge.y,
                        Mathf.InverseLerp(_levelData.LeftEdge.x, _levelData.RightEdge.x, part.Position.x));
                    if (Mathf.Abs(part.Position.y - expectedY) > 1.5f)
                    {
                        heightOk = false;
                        break;
                    }
                }
            }

            return spansGap && hasSupports && heightOk;
        }

        private IEnumerator ShowFailure()
        {
            // Flash parts red to indicate failure
            foreach (var part in _placedParts)
            {
                if (part != null)
                {
                    var sr = part.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.9f, 0.2f, 0.2f);
                }
            }

            yield return new WaitForSeconds(1f);

            // Restore colors
            foreach (var part in _placedParts)
            {
                if (part != null)
                {
                    var sr = part.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = part.PartType == PartPlank
                            ? new Color(0.65f, 0.45f, 0.2f)
                            : new Color(0.5f, 0.5f, 0.55f);
                    }
                }
            }
        }

        private IEnumerator AnimateCarCrossing()
        {
            if (_carTransform == null) yield break;

            _carTransform.gameObject.SetActive(true);
            Vector3 start = new Vector3(_levelData.LeftEdge.x - 1.5f, _levelData.LeftEdge.y + 0.5f, 0f);
            Vector3 end = new Vector3(_levelData.RightEdge.x + 1.5f, _levelData.RightEdge.y + 0.5f, 0f);
            _carTransform.position = start;

            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _carTransform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            _carTransform.position = end;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
