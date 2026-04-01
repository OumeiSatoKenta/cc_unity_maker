using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game082_AquaPet
{
    public class TankManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private AquaPetGameManager _gameManager;
        [SerializeField, Tooltip("魚スプライト")] private Sprite _fishSprite;
        [SerializeField, Tooltip("餌スプライト")] private Sprite _foodSprite;
        [SerializeField, Tooltip("魚追加コスト")] private int _fishCost = 10;

        private bool _isActive;
        private List<FishData> _fishes = new List<FishData>();
        private HashSet<int> _species = new HashSet<int>();
        private float _incomeTimer;

        private static readonly Color[] FishColors = {
            new Color(1f, 0.6f, 0.2f), new Color(0.3f, 0.7f, 1f),
            new Color(1f, 0.3f, 0.5f), new Color(0.4f, 1f, 0.6f),
            new Color(1f, 1f, 0.3f),
        };

        private class FishData
        {
            public GameObject Obj;
            public SpriteRenderer Sr;
            public int SpeciesId;
            public float Speed;
            public float Dir;
        }

        public void StartGame()
        {
            _isActive = true;
            _incomeTimer = 0f;
            // Start with 2 fish
            AddFishInternal(); AddFishInternal();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Animate fish
            foreach (var f in _fishes)
            {
                if (f.Obj == null) continue;
                var pos = f.Obj.transform.position;
                pos.x += f.Dir * f.Speed * Time.deltaTime;
                pos.y += Mathf.Sin(Time.time * 2f + f.Speed * 10f) * 0.003f;

                // Bounce off walls
                if (pos.x > 4f) { f.Dir = -1f; f.Obj.transform.localScale = new Vector3(-0.5f, 0.5f, 1f); }
                if (pos.x < -4f) { f.Dir = 1f; f.Obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); }
                f.Obj.transform.position = pos;
            }

            // Income timer
            if (_fishes.Count > 0) _incomeTimer += Time.deltaTime;

            // Tap to feed (visual only)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Feed visual at tap pos - no gameplay effect beyond visual
            }
        }

        public void AddFish()
        {
            if (_gameManager.TrySpend(_fishCost))
            {
                AddFishInternal();
            }
        }

        private void AddFishInternal()
        {
            int speciesId = Random.Range(0, FishColors.Length);
            _species.Add(speciesId);

            var obj = new GameObject($"Fish_{_fishes.Count}");
            obj.transform.position = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 2f), 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _fishSprite; sr.sortingOrder = 3;
            sr.color = FishColors[speciesId];
            float dir = Random.value > 0.5f ? 1f : -1f;
            obj.transform.localScale = new Vector3(dir * 0.5f, 0.5f, 1f);

            _fishes.Add(new FishData
            {
                Obj = obj, Sr = sr, SpeciesId = speciesId,
                Speed = Random.Range(0.5f, 1.5f), Dir = dir
            });
        }

        public int AutoIncome
        {
            get
            {
                if (_fishes.Count <= 0) return 0;
                if (_incomeTimer >= 3f)
                {
                    _incomeTimer -= 3f;
                    return _fishes.Count;
                }
                return 0;
            }
        }

        public int FishCount => _fishes.Count;
        public int SpeciesCount => _species.Count;
        public int NextFishCost => _fishCost + _fishes.Count * 3;
    }
}
