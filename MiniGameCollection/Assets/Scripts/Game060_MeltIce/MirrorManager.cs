using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game060_MeltIce
{
    public class MirrorManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private MeltIceGameManager _gameManager;
        [SerializeField, Tooltip("氷スプライト")] private Sprite _iceSprite;
        [SerializeField, Tooltip("鏡スプライト")] private Sprite _mirrorSprite;
        [SerializeField, Tooltip("太陽スプライト")] private Sprite _sunSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private List<GameObject> _targetIce = new List<GameObject>();
        private List<GameObject> _wrongIce = new List<GameObject>();
        private int _totalTargetIce;
        private float _meltTimer;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;

            // Sun
            var sun = new GameObject("Sun");
            sun.transform.position = new Vector3(-4f, 4f, 0f);
            var sunSr = sun.AddComponent<SpriteRenderer>();
            sunSr.sprite = _sunSprite; sunSr.sortingOrder = 1;

            // Target ice blocks
            Vector2[] targetPositions = { new(1f, 1f), new(2f, -1f), new(-1f, -2f) };
            foreach (var pos in targetPositions)
            {
                var obj = CreateIce(pos, true);
                _targetIce.Add(obj);
            }
            _totalTargetIce = _targetIce.Count;

            // Wrong ice (should not melt)
            Vector2[] wrongPositions = { new(-2f, 0f), new(3f, 2f) };
            foreach (var pos in wrongPositions)
            {
                var obj = CreateIce(pos, false);
                _wrongIce.Add(obj);
            }
        }

        public void StopGame() { _isActive = false; }

        private GameObject CreateIce(Vector2 pos, bool isTarget)
        {
            var obj = new GameObject(isTarget ? "TargetIce" : "WrongIce");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _iceSprite; sr.sortingOrder = 3;
            sr.color = isTarget ? new Color(0.7f, 0.85f, 1f) : new Color(1f, 0.7f, 0.7f);
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.45f, 0.45f);
            col.isTrigger = true;
            return obj;
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && _gameManager.MirrorsRemaining > 0)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                PlaceMirror(wp);
            }

            // Auto-melt: simple proximity check between mirrors and ice
            _meltTimer -= Time.deltaTime;
            if (_meltTimer <= 0f)
            {
                _meltTimer = 0.5f;
                CheckMelt();
            }
        }

        private void PlaceMirror(Vector2 pos)
        {
            var obj = new GameObject("Mirror");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _mirrorSprite; sr.sortingOrder = 4;
            obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            // Rotate randomly for variety
            obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));

            _gameManager.OnMirrorPlaced();
        }

        private void CheckMelt()
        {
            var mirrors = GameObject.FindGameObjectsWithTag("Untagged");
            // Simple: any ice within 1.5 units of a "Mirror" named object gets melted
            var allMirrors = new List<GameObject>();
            foreach (var obj in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (obj.gameObject.name == "Mirror") allMirrors.Add(obj.gameObject);
            }

            for (int i = _targetIce.Count - 1; i >= 0; i--)
            {
                if (_targetIce[i] == null) { _targetIce.RemoveAt(i); continue; }
                foreach (var m in allMirrors)
                {
                    if (Vector2.Distance(m.transform.position, _targetIce[i].transform.position) < 1.5f)
                    {
                        Destroy(_targetIce[i]);
                        _targetIce.RemoveAt(i);
                        break;
                    }
                }
            }

            for (int i = _wrongIce.Count - 1; i >= 0; i--)
            {
                if (_wrongIce[i] == null) { _wrongIce.RemoveAt(i); continue; }
                foreach (var m in allMirrors)
                {
                    if (Vector2.Distance(m.transform.position, _wrongIce[i].transform.position) < 1.5f)
                    {
                        Destroy(_wrongIce[i]);
                        _wrongIce.RemoveAt(i);
                        _gameManager.OnWrongIceMelted();
                        return;
                    }
                }
            }
        }

        public int RemainingIce => _targetIce.Count;
        public int TotalTargetIce => _totalTargetIce;
    }
}
