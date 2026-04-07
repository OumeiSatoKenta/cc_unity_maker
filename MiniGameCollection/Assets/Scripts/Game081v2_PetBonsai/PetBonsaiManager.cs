using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game081v2_PetBonsai
{
    public enum BranchState { Normal, Overgrown, Pest }

    public class BranchObject : MonoBehaviour
    {
        public BranchState State = BranchState.Normal;
        public int BranchIndex;
        public float OvergrownTimer;
        public float PestTimer;

        SpriteRenderer _sr;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        public void SetState(BranchState state, Sprite normalSp, Sprite overgrownSp, Sprite pestSp)
        {
            State = state;
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            switch (state)
            {
                case BranchState.Normal:    _sr.sprite = normalSp; _sr.color = Color.white; break;
                case BranchState.Overgrown: _sr.sprite = overgrownSp; _sr.color = Color.white; break;
                case BranchState.Pest:      _sr.sprite = pestSp; _sr.color = Color.white; break;
            }
        }

        public IEnumerator PlayPruneEffect()
        {
            float t = 0f;
            Vector3 orig = transform.localScale;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float s = 1f + (t / 0.15f) * 0.4f;
                transform.localScale = orig * s;
                yield return null;
            }
            t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float s = 1.4f - (t / 0.15f) * 1.4f;
                transform.localScale = orig * s;
                yield return null;
            }
            transform.localScale = Vector3.zero;
        }

        public IEnumerator PlayPestEffect()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            Color orig = _sr.color;
            _sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = 1f - t / 0.2f;
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.zero;
        }
    }

    public class PetBonsaiManager : MonoBehaviour
    {
        [SerializeField] PetBonsaiGameManager _gm;
        [SerializeField] Sprite _branchNormal;
        [SerializeField] Sprite _branchOvergrown;
        [SerializeField] Sprite _branchPest;
        // Stage params
        int _branchCount = 3;
        int _waterNeeded = 3;
        bool _hasPest;
        bool _hasRival;
        float _overgrownInterval = 10f;
        float _pestInterval = 12f;

        // Game state
        int _beautyScore;
        float _growthGauge;
        int _waterCount;
        int _comboCount;
        bool _fertilized;
        float _fertilizeTimer;
        float _overgrownCheckTimer;
        float _pestSpawnTimer;
        bool _isActive;
        int _missCount;
        const int MAX_MISS = 3;
        int _totalScore;
        int _rivalScore;
        float _rivalScoreAccum;

        string[] _seasons = { "春", "夏", "秋", "冬" };
        int _seasonIndex;
        float _seasonTimer;
        bool _hasSeasonsEnabled;

        List<BranchObject> _branches = new List<BranchObject>();
        Transform _branchRoot;

        public int TotalScore => _totalScore;

        public void ResetScore()
        {
            _totalScore = 0;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearBranches();

            _branchCount = stageIndex switch
            {
                0 => 3,
                1 => 5,
                2 => 5,
                3 => 7,
                _ => 9
            };
            _waterNeeded = stageIndex < 3 ? 3 + stageIndex : 5;
            _hasPest = stageIndex >= 3;
            _hasRival = stageIndex >= 4;
            _hasSeasonsEnabled = stageIndex >= 1;
            _overgrownInterval = Mathf.Max(5f, 10f / config.speedMultiplier);
            _pestInterval = Mathf.Max(8f, 15f / config.speedMultiplier);

            _beautyScore = 0;
            _growthGauge = 0f;
            _waterCount = 0;
            _comboCount = 0;
            _fertilized = false;
            _fertilizeTimer = 0f;
            _overgrownCheckTimer = _overgrownInterval;
            _pestSpawnTimer = _pestInterval;
            _missCount = 0;
            _seasonIndex = 0;
            _seasonTimer = 15f;
            _rivalScore = stageIndex >= 4 ? 75 : 0;

            SpawnBranches();
            _isActive = true;

            _gm.UpdateBeautyDisplay(_beautyScore);
            _gm.UpdateGrowthDisplay(0f);
            _gm.UpdateWaterDisplay(_waterCount, _waterNeeded);
            _gm.UpdateComboDisplay(0);
            if (_hasSeasonsEnabled) _gm.UpdateSeasonDisplay(_seasons[_seasonIndex]);
            if (_hasRival) _gm.UpdateRivalScore(_rivalScore);
        }

        void SpawnBranches()
        {
            if (_branchRoot == null)
            {
                var rootGO = new GameObject("BranchRoot");
                rootGO.transform.SetParent(transform);
                _branchRoot = rootGO.transform;
            }

            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[PetBonsai] MainCamera not found"); return; }
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float treeBaseY = -0.5f;
            float radius = Mathf.Min(camWidth * 0.5f, camSize * 0.45f);

            for (int i = 0; i < _branchCount; i++)
            {
                float angle = Mathf.PI * (0.1f + 0.8f * i / Mathf.Max(1, _branchCount - 1));
                float bx = Mathf.Cos(angle) * radius;
                float by = treeBaseY + Mathf.Sin(angle) * radius * 0.8f;

                var go = new GameObject($"Branch_{i}");
                go.transform.SetParent(_branchRoot);
                go.transform.position = new Vector3(bx, by, 0f);
                go.transform.localScale = Vector3.one;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _branchNormal;
                sr.sortingOrder = 2;

                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(1.2f, 0.5f);

                var bo = go.AddComponent<BranchObject>();
                bo.BranchIndex = i;
                bo.State = BranchState.Normal;

                _branches.Add(bo);
            }
        }

        void ClearBranches()
        {
            foreach (var b in _branches)
            {
                if (b != null) Destroy(b.gameObject);
            }
            _branches.Clear();
        }

        public void SetActive(bool active) => _isActive = active;

        void Update()
        {
            if (!_isActive) return;

            HandleInput();
            UpdateTimers();
        }

        void HandleInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var cam = Camera.main;
            if (cam == null) return;
            var worldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPos.z = 0f;
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var branch = hit.GetComponent<BranchObject>();
            if (branch != null)
            {
                PruneBranch(branch);
            }
        }

        void PruneBranch(BranchObject branch)
        {
            int gained;
            if (branch.State == BranchState.Pest)
            {
                gained = 10;
                _gm.ShowFeedback("害虫駆除！ +" + gained, new Color(1f, 0.5f, 0f));
                StartCoroutine(branch.PlayPestEffect());
            }
            else if (branch.State == BranchState.Overgrown)
            {
                gained = 20;
                _comboCount++;
                float mult = _comboCount >= 3 ? 1.2f : 1f;
                gained = Mathf.RoundToInt(gained * mult);
                _gm.ShowFeedback("剪定！ +" + gained + (_comboCount >= 3 ? " COMBO×1.2" : ""), Color.green);
                StartCoroutine(branch.PlayPruneEffect());
            }
            else
            {
                // Normal branch
                int balance = ComputeBalance();
                gained = Mathf.Clamp(5 + balance, 5, 15);
                _comboCount++;
                float mult = _comboCount >= 3 ? 1.2f : 1f;
                gained = Mathf.RoundToInt(gained * mult);
                _gm.ShowFeedback("剪定 +" + gained, Color.cyan);
                StartCoroutine(branch.PlayPruneEffect());
            }

            _beautyScore += gained;
            _beautyScore = Mathf.Clamp(_beautyScore, 0, 100);
            _totalScore += gained;
            _gm.UpdateBeautyDisplay(_beautyScore);
            _gm.UpdateComboDisplay(_comboCount);

            _branches.Remove(branch);
            // Remove goes via coroutine (branch.PlayPruneEffect destroys by scaling to 0)
            // Respawn a new branch after delay
            StartCoroutine(RespawnBranch(branch));
        }

        IEnumerator RespawnBranch(BranchObject old)
        {
            yield return new WaitForSeconds(0.5f);
            if (old != null) Destroy(old.gameObject);

            if (!_isActive || _branches.Count >= _branchCount) yield break;

            var cam2 = Camera.main;
            if (cam2 == null) yield break;
            float camSize = cam2.orthographicSize;
            float camWidth = camSize * cam2.aspect;
            float treeBaseY = -0.5f;
            float radius = Mathf.Min(camWidth * 0.5f, camSize * 0.45f);

            int i = _branches.Count;
            float angle = Mathf.PI * (0.1f + 0.8f * i / Mathf.Max(1, _branchCount - 1));
            float bx = Mathf.Cos(angle) * radius;
            float by = treeBaseY + Mathf.Sin(angle) * radius * 0.8f;

            var go = new GameObject($"Branch_{i}");
            go.transform.SetParent(_branchRoot);
            go.transform.position = new Vector3(bx, by, 0f);
            go.transform.localScale = Vector3.one;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _branchNormal;
            sr.sortingOrder = 2;

            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1.2f, 0.5f);

            var bo = go.AddComponent<BranchObject>();
            bo.BranchIndex = i;
            bo.State = BranchState.Normal;
            _branches.Add(bo);
        }

        int ComputeBalance()
        {
            // Score bonus based on roughly equal branches on both sides
            int left = 0, right = 0;
            foreach (var b in _branches)
            {
                if (b == null) continue;
                if (b.transform.position.x < 0) left++;
                else right++;
            }
            int diff = Mathf.Abs(left - right);
            return Mathf.Max(0, 10 - diff * 2);
        }

        void UpdateTimers()
        {
            float dt = Time.deltaTime;

            // Overgrown timer
            _overgrownCheckTimer -= dt;
            if (_overgrownCheckTimer <= 0f)
            {
                _overgrownCheckTimer = _overgrownInterval;
                TryMakeOvergrown();
            }

            // Pest timer
            if (_hasPest)
            {
                _pestSpawnTimer -= dt;
                if (_pestSpawnTimer <= 0f)
                {
                    _pestSpawnTimer = _pestInterval;
                    TrySpawnPest();
                }
                // Pest damage
                for (int idx = 0; idx < _branches.Count; idx++)
                {
                    var b = _branches[idx];
                    if (b == null || b.State != BranchState.Pest) continue;
                    b.PestTimer += dt;
                    if (b.PestTimer >= 3f)
                    {
                        b.PestTimer = 0f;
                        _beautyScore = Mathf.Max(0, _beautyScore - 5);
                        _gm.UpdateBeautyDisplay(_beautyScore);
                    }
                }
            }

            // Fertilize timer
            if (_fertilized)
            {
                _fertilizeTimer -= dt;
                if (_fertilizeTimer <= 0f) _fertilized = false;
            }

            // Season timer
            if (_hasSeasonsEnabled)
            {
                _seasonTimer -= dt;
                if (_seasonTimer <= 0f)
                {
                    _seasonTimer = 15f;
                    _seasonIndex = (_seasonIndex + 1) % 4;
                    _gm.UpdateSeasonDisplay(_seasons[_seasonIndex]);
                }
            }

            // Rival score update (Stage5)
            if (_hasRival)
            {
                _rivalScoreAccum += dt * 0.5f;
                int gained = (int)_rivalScoreAccum;
                if (gained > 0)
                {
                    _rivalScoreAccum -= gained;
                    _rivalScore += gained;
                    _gm.UpdateRivalScore(_rivalScore);
                }
            }
        }

        void TryMakeOvergrown()
        {
            foreach (var b in _branches)
            {
                if (b == null || b.State != BranchState.Normal) continue;
                if (Random.value < 0.4f)
                {
                    b.SetState(BranchState.Overgrown, _branchNormal, _branchOvergrown, _branchPest);
                    break;
                }
            }
        }

        void TrySpawnPest()
        {
            foreach (var b in _branches)
            {
                if (b == null || b.State == BranchState.Pest) continue;
                if (Random.value < 0.35f)
                {
                    b.SetState(BranchState.Pest, _branchNormal, _branchOvergrown, _branchPest);
                    break;
                }
            }
        }

        public void DoWater()
        {
            if (!_isActive) return;

            float gain = 15f * (_fertilized ? 1.5f : 1f);
            _growthGauge = Mathf.Min(100f, _growthGauge + gain);
            _waterCount++;
            _comboCount = 0; // Watering resets prune combo
            _gm.UpdateGrowthDisplay(_growthGauge / 100f);
            _gm.UpdateWaterDisplay(_waterCount, _waterNeeded);
            _gm.ShowFeedback("水やり！ +" + (int)gain + "%", new Color(0.3f, 0.7f, 1f));

            // Check for overgrown
            if (_growthGauge >= 50f)
            {
                TryMakeOvergrown();
            }

            // Check stage clear
            if (_waterCount >= _waterNeeded && _beautyScore >= 85)
            {
                _isActive = false;
                _totalScore += _beautyScore * 10;
                _gm.OnStageClear();
            }
            else if (_waterCount >= _waterNeeded * 2)
            {
                // Too many waters without clearing: check miss
                _missCount++;
                if (_missCount >= MAX_MISS)
                {
                    _isActive = false;
                    _gm.OnGameOver();
                }
            }
        }

        public void DoFertilize()
        {
            if (!_isActive) return;
            _fertilized = true;
            _fertilizeTimer = 15f;
            _gm.ShowFeedback("肥料！ 成長×1.5", new Color(1f, 0.7f, 0.2f));
        }

        public void DoShowcase()
        {
            if (!_isActive) return;
            if (_waterCount >= _waterNeeded && _beautyScore >= 85)
            {
                _isActive = false;
                _totalScore += _beautyScore * 10;
                _gm.OnStageClear();
            }
            else
            {
                string msg = _beautyScore < 85
                    ? $"美しさ不足 ({_beautyScore}/85)"
                    : $"水やり不足 ({_waterCount}/{_waterNeeded})";
                _gm.ShowFeedback(msg, Color.red);
            }
        }
    }
}
