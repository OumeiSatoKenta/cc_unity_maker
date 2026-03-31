using UnityEngine;
using UnityEngine.InputSystem;

namespace Game052_HammerNail
{
    public class NailManager : MonoBehaviour
    {
        [SerializeField] private HammerNailGameManager _gameManager;

        private const float NailTotalDepth = 2f;
        private const float PerfectWindow = 0.15f;
        private const float GoodWindow = 0.3f;
        private const float IndicatorSpeed = 3f;

        private GameObject _nail;
        private GameObject _hammer;
        private GameObject _board;
        private GameObject _indicator;
        private GameObject _targetZone;
        private Sprite _nailSprite, _hammerSprite, _boardSprite;
        private float _nailDepth;
        private float _indicatorPhase;
        private float _nailStartY;
        private bool _animating;
        private float _animTimer;

        public void Init()
        {
            _nailSprite = Resources.Load<Sprite>("Sprites/Game052_HammerNail/nail");
            _hammerSprite = Resources.Load<Sprite>("Sprites/Game052_HammerNail/hammer");
            _boardSprite = Resources.Load<Sprite>("Sprites/Game052_HammerNail/board");

            CleanUp();

            _board = new GameObject("Board");
            _board.transform.position = new Vector3(0f, -2f, 0f);
            _board.transform.localScale = new Vector3(3f, 1f, 1f);
            var bsr = _board.AddComponent<SpriteRenderer>(); bsr.sprite = _boardSprite; bsr.sortingOrder = 0;

            SpawnNail();

            _hammer = new GameObject("Hammer");
            _hammer.transform.position = new Vector3(1.5f, 1f, 0f);
            _hammer.transform.localScale = Vector3.one * 1.2f;
            var hsr = _hammer.AddComponent<SpriteRenderer>(); hsr.sprite = _hammerSprite; hsr.sortingOrder = 8;

            // Timing indicator bar
            _targetZone = new GameObject("TargetZone");
            _targetZone.transform.position = new Vector3(0f, 3f, 0f);
            _targetZone.transform.localScale = new Vector3(1f, 0.3f, 1f);
            var tzsr = _targetZone.AddComponent<SpriteRenderer>();
            tzsr.sprite = _boardSprite; tzsr.color = new Color(0.3f, 1f, 0.3f, 0.4f); tzsr.sortingOrder = 1;

            _indicator = new GameObject("Indicator");
            _indicator.transform.position = new Vector3(-3f, 3f, 0f);
            _indicator.transform.localScale = new Vector3(0.15f, 0.4f, 1f);
            var isr = _indicator.AddComponent<SpriteRenderer>();
            isr.sprite = _boardSprite; isr.color = Color.yellow; isr.sortingOrder = 3;

            _indicatorPhase = 0f;
            _animating = false;
        }

        private void SpawnNail()
        {
            if (_nail != null) Destroy(_nail);
            _nailDepth = 0f;
            _nailStartY = 0f;
            _nail = new GameObject("Nail");
            _nail.transform.position = new Vector3(0f, _nailStartY, 0f);
            _nail.transform.localScale = Vector3.one * 1.5f;
            var sr = _nail.AddComponent<SpriteRenderer>(); sr.sprite = _nailSprite; sr.sortingOrder = 5;
        }

        private void CleanUp()
        {
            if (_nail != null) Destroy(_nail);
            if (_hammer != null) Destroy(_hammer);
            if (_board != null) Destroy(_board);
            if (_indicator != null) Destroy(_indicator);
            if (_targetZone != null) Destroy(_targetZone);
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            if (_animating)
            {
                _animTimer -= Time.deltaTime;
                if (_animTimer <= 0f)
                {
                    _animating = false;
                    _hammer.transform.position = new Vector3(1.5f, 1f, 0f);
                    _hammer.transform.rotation = Quaternion.identity;
                    if (_nailDepth >= NailTotalDepth)
                    {
                        bool perfect = _nailDepth >= NailTotalDepth + 0.3f;
                        if (_gameManager != null) _gameManager.OnNailComplete(perfect);
                        SpawnNail();
                    }
                }
                return;
            }

            _indicatorPhase += Time.deltaTime * IndicatorSpeed;
            float x = Mathf.Sin(_indicatorPhase) * 3f;
            if (_indicator != null) _indicator.transform.position = new Vector3(x, 3f, 0f);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                float dist = Mathf.Abs(x);
                if (dist < PerfectWindow)
                {
                    HitNail(0.8f);
                }
                else if (dist < GoodWindow)
                {
                    HitNail(0.4f);
                }
                else
                {
                    if (_gameManager != null) _gameManager.OnMiss();
                    _animating = true;
                    _animTimer = 0.2f;
                    _hammer.transform.position = new Vector3(0.5f, 0.3f, 0f);
                    _hammer.transform.rotation = Quaternion.Euler(0, 0, -30f);
                }
            }
        }

        private void HitNail(float depth)
        {
            _nailDepth += depth;
            float y = _nailStartY - _nailDepth * 0.5f;
            if (_nail != null) _nail.transform.position = new Vector3(0f, y, 0f);

            _animating = true;
            _animTimer = 0.15f;
            _hammer.transform.position = new Vector3(0.3f, 0.5f, 0f);
            _hammer.transform.rotation = Quaternion.Euler(0, 0, -45f);
        }
    }
}
