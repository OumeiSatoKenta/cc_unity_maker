using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game041v2_StackJump
{
    public class StackJumpMechanic : MonoBehaviour
    {
        [SerializeField] StackJumpGameManager _gameManager;
        [SerializeField] Sprite _spriteBlock;
        [SerializeField] Sprite _spriteBase;
        [SerializeField] Sprite _spriteCutBlock;

        // Stage config
        float _slideSpeed = 2.0f;
        int _targetCount = 10;
        bool _useAlternateAxis = false;
        bool _useAcceleration = false;
        bool _useCameraShake = false;
        bool _useShrinkStart = false;

        // Game state
        bool _isActive = false;
        int _stackCount = 0;
        int _comboCount = 0;
        float _blockWidth = 2.0f;
        float _maxBlockWidth = 2.0f;
        bool _isAxisX = true;
        float _currentSpeed;
        int _accelCounter = 0;

        // Block thickness
        const float BlockHeight = 0.4f;
        const float PerfectThreshold = 0.08f;
        const float MaxWidth = 2.0f;

        // Scene objects
        GameObject _slidingBlock;
        GameObject _topBlock; // the most recent placed block
        List<GameObject> _stackedBlocks = new List<GameObject>();
        Camera _mainCamera;

        // Sliding state
        float _slideDir = 1f;
        float _slidingX = 0f;
        float _slidingY = 0f;
        float _slideRange;

        // Camera follow
        float _cameraBaseY;
        float _stackBaseY;

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        void Update()
        {
            if (!_isActive || _slidingBlock == null) return;

            // Input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBlock();
                return;
            }

            // Slide
            SlideBlock();

            // Camera follow
            FollowCamera();
        }

        void SlideBlock()
        {
            if (_isAxisX)
            {
                _slidingX += _slideDir * _currentSpeed * Time.deltaTime;
                if (_slidingX > _slideRange || _slidingX < -_slideRange)
                {
                    _slideDir *= -1f;
                    _slidingX = Mathf.Clamp(_slidingX, -_slideRange, _slideRange);
                }
                _slidingBlock.transform.position = new Vector3(_slidingX, _slidingBlock.transform.position.y, 0f);
            }
            else
            {
                // Alternate axis: slide in Y direction (2D representation of Z-axis)
                _slidingY += _slideDir * _currentSpeed * Time.deltaTime;
                if (_slidingY > _slideRange || _slidingY < -_slideRange)
                {
                    _slideDir *= -1f;
                    _slidingY = Mathf.Clamp(_slidingY, -_slideRange, _slideRange);
                }
                _slidingBlock.transform.position = new Vector3(_slidingBlock.transform.position.x, _slidingY + GetBlockY(_stackCount), 0f);
            }
        }

        void FollowCamera()
        {
            if (_mainCamera == null) return;
            float targetY = _cameraBaseY + Mathf.Max(0, _stackCount - 4) * BlockHeight;
            Vector3 camPos = _mainCamera.transform.position;
            camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * 3f);
            _mainCamera.transform.position = camPos;
        }

        public void SetupStage(int stageIndex)
        {
            // Clear previous state
            ClearAllBlocks();
            _stackCount = 0;
            _comboCount = 0;
            _accelCounter = 0;
            _isAxisX = true;

            // Stage parameters
            switch (stageIndex)
            {
                case 0: // Stage 1
                    _slideSpeed = 2.0f;
                    _targetCount = 10;
                    _useAlternateAxis = false;
                    _useAcceleration = false;
                    _useCameraShake = false;
                    _useShrinkStart = false;
                    _blockWidth = MaxWidth;
                    break;
                case 1: // Stage 2
                    _slideSpeed = 2.5f;
                    _targetCount = 15;
                    _useAlternateAxis = true;
                    _useAcceleration = false;
                    _useCameraShake = false;
                    _useShrinkStart = false;
                    _blockWidth = MaxWidth;
                    break;
                case 2: // Stage 3
                    _slideSpeed = 3.0f;
                    _targetCount = 20;
                    _useAlternateAxis = true;
                    _useAcceleration = true;
                    _useCameraShake = false;
                    _useShrinkStart = false;
                    _blockWidth = MaxWidth;
                    break;
                case 3: // Stage 4
                    _slideSpeed = 3.5f;
                    _targetCount = 25;
                    _useAlternateAxis = true;
                    _useAcceleration = true;
                    _useCameraShake = true;
                    _useShrinkStart = false;
                    _blockWidth = MaxWidth;
                    break;
                case 4: // Stage 5
                    _slideSpeed = 4.0f;
                    _targetCount = 30;
                    _useAlternateAxis = true;
                    _useAcceleration = true;
                    _useCameraShake = true;
                    _useShrinkStart = true;
                    _blockWidth = MaxWidth * 0.7f;
                    break;
                default:
                    _slideSpeed = 2.0f;
                    _targetCount = 10;
                    _blockWidth = MaxWidth;
                    break;
            }

            _currentSpeed = _slideSpeed;
            _maxBlockWidth = MaxWidth;

            // Camera reset
            if (_mainCamera != null)
            {
                _cameraBaseY = _mainCamera.transform.position.y;
                Vector3 cp = _mainCamera.transform.position;
                cp.y = _cameraBaseY;
                _mainCamera.transform.position = cp;
            }

            // Calc stack base Y
            _stackBaseY = GetStackBaseY();

            // Place base block
            PlaceBaseBlock();

            // Spawn first sliding block
            SpawnSlidingBlock();

            _isActive = true;
        }

        float GetStackBaseY()
        {
            if (_mainCamera == null) return -3.0f;
            float camSize = _mainCamera.orthographicSize;
            return -camSize + 1.5f; // bottom area + UI margin
        }

        float GetBlockY(int index)
        {
            return _stackBaseY + index * BlockHeight;
        }

        void PlaceBaseBlock()
        {
            var obj = CreateBlockObject("Base", _spriteBase != null ? _spriteBase : _spriteBlock, MaxWidth, BlockHeight, 0);
            obj.transform.position = new Vector3(0f, _stackBaseY, 0f);
            _stackedBlocks.Add(obj);
            _topBlock = obj;
        }

        void SpawnSlidingBlock()
        {
            if (_slidingBlock != null) Destroy(_slidingBlock);

            float camSize = _mainCamera != null ? _mainCamera.orthographicSize : 5f;
            float camWidth = _mainCamera != null ? camSize * _mainCamera.aspect : 2.5f;
            _slideRange = camWidth * 0.75f;

            float y = GetBlockY(_stackCount + 1);

            if (_useAlternateAxis && _stackCount > 0 && _stackCount % 2 == 1)
            {
                _isAxisX = false;
                _slidingX = 0f;
                _slidingY = y - BlockHeight * 0.5f;
            }
            else
            {
                _isAxisX = true;
                _slidingX = -_slideRange;
                _slidingY = 0f;
            }

            _slidingBlock = CreateBlockObject("SlidingBlock", _spriteBlock, _blockWidth, BlockHeight, 1);
            _slidingBlock.transform.position = new Vector3(_slidingX, y, 0f);

            _slideDir = 1f;
        }

        void PlaceBlock()
        {
            if (_slidingBlock == null) return;

            float topBlockX = _topBlock != null ? _topBlock.transform.position.x : 0f;
            float topBlockWidth = _topBlock != null ? GetBlockWidthOf(_topBlock) : MaxWidth;
            float slidingX = _slidingBlock.transform.position.x;
            float offset = slidingX - topBlockX;
            float overlapLeft = (topBlockX - topBlockWidth / 2f);
            float overlapRight = (topBlockX + topBlockWidth / 2f);
            float slideLeft = slidingX - _blockWidth / 2f;
            float slideRight = slidingX + _blockWidth / 2f;
            float newLeft = Mathf.Max(overlapLeft, slideLeft);
            float newRight = Mathf.Min(overlapRight, slideRight);
            float overlap = newRight - newLeft;

            if (overlap < 0.05f)
            {
                // Miss - game over (including near-zero floating point cases)
                Destroy(_slidingBlock);
                _slidingBlock = null;
                _gameManager.OnGameOver();
                return;
            }

            bool isPerfect = Mathf.Abs(offset) < PerfectThreshold;
            float placedWidth;
            float placedX;

            if (isPerfect)
            {
                placedWidth = Mathf.Min(_blockWidth + 0.2f, _maxBlockWidth);
                placedX = topBlockX;
                _comboCount++;
                int perfectScore = 300 + _comboCount * 150;
                _gameManager.OnPerfect();
                _gameManager.OnScoreAdded(perfectScore);
                _gameManager.OnComboChanged(_comboCount);
            }
            else
            {
                placedWidth = overlap;
                placedX = (newLeft + newRight) / 2f;
                _comboCount = 0;
                _gameManager.OnComboChanged(0);

                // Spawn cut block (falling piece)
                SpawnCutBlock(slidingX, _slidingBlock.transform.position.y, _blockWidth, placedWidth, placedX);
            }

            // Place the block
            Destroy(_slidingBlock);
            _slidingBlock = null;
            _blockWidth = placedWidth;
            _stackCount++;

            var placedObj = CreateBlockObject($"Block_{_stackCount}", _spriteBlock, placedWidth, BlockHeight, 0);
            placedObj.transform.position = new Vector3(placedX, GetBlockY(_stackCount), 0f);
            _stackedBlocks.Add(placedObj);
            _topBlock = placedObj;

            // Score
            _gameManager.OnScoreAdded(100);
            _gameManager.OnStackCountChanged(_stackCount, _targetCount);

            // Feedback
            StartCoroutine(BlockPulse(placedObj));
            if (_useCameraShake) StartCoroutine(CameraShake(0.05f, 0.2f));

            // Acceleration
            if (_useAcceleration)
            {
                _accelCounter++;
                if (_accelCounter % 5 == 0)
                    _currentSpeed = Mathf.Min(_currentSpeed + 0.5f, _slideSpeed + 2.0f);
            }

            // Alternate axis
            if (_useAlternateAxis) _isAxisX = !_isAxisX;

            // Check clear
            if (_stackCount >= _targetCount)
            {
                float widthRatio = _blockWidth / _maxBlockWidth;
                int bonus = Mathf.RoundToInt(widthRatio * 1000f);
                _gameManager.OnStageClear(bonus);
                return;
            }

            // Next block
            SpawnSlidingBlock();
        }

        void SpawnCutBlock(float slidingX, float slidingY, float totalWidth, float overlapWidth, float overlapX)
        {
            float cutWidth = totalWidth - overlapWidth;
            if (cutWidth <= 0.01f) return;

            // Determine cut piece X
            float overlapLeft = overlapX - overlapWidth / 2f;
            float overlapRight = overlapX + overlapWidth / 2f;
            float slideLeft = slidingX - totalWidth / 2f;
            float slideRight = slidingX + totalWidth / 2f;

            float cutX;
            if (slideLeft < overlapLeft) cutX = slideLeft + cutWidth / 2f;
            else cutX = slideRight - cutWidth / 2f;

            var cut = CreateBlockObject("CutBlock", _spriteCutBlock != null ? _spriteCutBlock : _spriteBlock, cutWidth, BlockHeight * 0.9f, 2);
            cut.transform.position = new Vector3(cutX, slidingY, 0f);
            StartCoroutine(FallAndDestroy(cut));

            // Red flash on top block
            if (_topBlock != null) StartCoroutine(RedFlash(_topBlock));
        }

        IEnumerator FallAndDestroy(GameObject obj)
        {
            float elapsed = 0f;
            Vector3 startPos = obj.transform.position;
            float camSize = _mainCamera != null ? _mainCamera.orthographicSize : 5f;
            while (elapsed < 1.5f && obj != null)
            {
                elapsed += Time.deltaTime;
                obj.transform.position = startPos + Vector3.down * (elapsed * elapsed * 5f);
                if (obj.transform.position.y < -camSize - 2f) break;
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        IEnumerator BlockPulse(GameObject obj)
        {
            if (obj == null) yield break;
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                obj.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            if (obj != null) obj.transform.localScale = Vector3.one;
        }

        IEnumerator RedFlash(GameObject obj)
        {
            if (obj == null) yield break;
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.15f);
            if (sr != null) sr.color = orig;
        }

        IEnumerator CameraShake(float magnitude, float duration)
        {
            if (_mainCamera == null) yield break;
            Vector3 origPos = _mainCamera.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-magnitude, magnitude);
                float y = Random.Range(-magnitude, magnitude);
                _mainCamera.transform.position = origPos + new Vector3(x, y, 0f);
                yield return null;
            }
            _mainCamera.transform.position = origPos;
        }

        GameObject CreateBlockObject(string name, Sprite sprite, float width, float height, int sortOrder)
        {
            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            if (sprite != null)
            {
                float sprW = sprite.bounds.size.x;
                float sprH = sprite.bounds.size.y;
                obj.transform.localScale = new Vector3(
                    sprW > 0 ? width / sprW : width,
                    sprH > 0 ? height / sprH : height,
                    1f
                );
            }
            else
            {
                obj.transform.localScale = new Vector3(width, height, 1f);
            }
            return obj;
        }

        float GetBlockWidthOf(GameObject obj)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                return sr.sprite.bounds.size.x * obj.transform.localScale.x;
            return obj.transform.localScale.x;
        }

        void ClearAllBlocks()
        {
            StopAllCoroutines();
            foreach (var b in _stackedBlocks)
                if (b != null) Destroy(b);
            _stackedBlocks.Clear();
            if (_slidingBlock != null) { Destroy(_slidingBlock); _slidingBlock = null; }
            _topBlock = null;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        void OnDestroy()
        {
            ClearAllBlocks();
        }
    }
}
