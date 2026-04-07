using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game082v2_AquaPet
{
    public class AquariumManager : MonoBehaviour
    {
        [SerializeField] AquaPetGameManager _gameManager;
        [SerializeField] AquaPetUI _ui;
        [SerializeField] Sprite[] _fishSprites;
        [SerializeField] Sprite _foodSprite;
        [SerializeField] Sprite _bubbleSprite;

        // Stage config
        float _waterQualityDecayRate = 2f;
        int _maxFeedCount = 5;
        bool _breedingEnabled = false;
        bool _diseaseEnabled = false;
        bool _strictBreedingEnabled = false;
        bool _optimalEnvEnabled = false;
        int _targetCollectionCount = 3;
        int _stageIndex = 0;

        // Game state
        float _waterQuality = 100f;
        float _waterTemperature = 22f;
        float _pH = 7.0f;
        int _feedCount = 5;
        int _combo = 0;
        float _comboMultiplier = 1.0f;
        int _totalScore = 0;
        bool _isActive = false;
        bool _isStageClear = false;

        // Disease timer
        float _diseaseTimer = 0f;
        float _diseaseInterval = 20f;

        // Health bonus timer
        float _healthBonusTimer = 0f;

        readonly List<FishData> _fishList = new();
        readonly List<Texture2D> _createdTextures = new();
        readonly List<GameObject> _feedParticles = new();

        public int TotalScore => _totalScore;

        class FishData
        {
            public GameObject go;
            public SpriteRenderer sr;
            public string species;
            public float health = 100f;
            public float hunger = 100f;
            public bool isSick = false;
            public bool isDiscovered = false;
            public bool isRare = false;
            public float optimalTemp = 22f;
            public float optimalPH = 7.0f;
            public Vector3 velocity;
            public float swimTimer;
            public Color baseColor;
            public int spriteIndex;
        }

        static readonly (string name, float optTemp, float optPH, bool isRare)[] FishSpecies =
        {
            ("金魚",      20f, 7.0f, false),   // 0
            ("ネオンテトラ", 25f, 6.5f, false),  // 1
            ("メダカ",    22f, 7.0f, false),   // 2
            ("エンゼルフィッシュ", 27f, 6.5f, false), // 3
            ("クマノミ",  26f, 8.0f, false),   // 4
            ("ベタ",      28f, 7.0f, false),   // 5
            ("グッピー",  25f, 7.5f, false),   // 6
            ("ハナダイ",  24f, 8.2f, false),   // 7
            ("ミノカサゴ", 22f, 8.0f, false),  // 8
            ("ワニトカゲギス", 5f, 7.5f, true), // 9 深海魚（レア）
        };

        // Stage unlocks (which species are available at each stage)
        static readonly int[][] StageSpeciesUnlocks =
        {
            new[]{ 0, 1, 2 },          // Stage 1: 淡水魚3種
            new[]{ 0, 1, 2, 3, 4 },    // Stage 2: +熱帯魚2種
            new[]{ 0, 1, 2, 3, 4, 5, 6 },  // Stage 3: 繁殖で+2種
            new[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8 }, // Stage 4: +海水魚
            new[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, // Stage 5: +深海魚
        };

        public void ResetScore()
        {
            _totalScore = 0;
            _combo = 0;
            _comboMultiplier = 1.0f;
        }

        public void SetActive(bool active)
        {
            if (!active) StopAllCoroutines();
            _isActive = active;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _stageIndex = stageIndex;
            _isStageClear = false;

            // Clear existing fish
            foreach (var f in _fishList)
            {
                if (f.go != null) Destroy(f.go);
            }
            _fishList.Clear();

            // Stage config
            _waterQuality = 100f;
            _waterTemperature = 22f;
            _pH = 7.0f;
            _feedCount = _maxFeedCount;
            _breedingEnabled = stageIndex >= 2;
            _diseaseEnabled = stageIndex >= 3;
            _strictBreedingEnabled = stageIndex >= 4;
            _optimalEnvEnabled = stageIndex >= 1;
            _diseaseTimer = _diseaseInterval;
            _healthBonusTimer = 0f;

            float complexity = config.complexityFactor;
            _waterQualityDecayRate = 2f + complexity * 4f;
            _maxFeedCount = 5;
            _feedCount = _maxFeedCount;

            int[] speciesPool = StageSpeciesUnlocks[Mathf.Clamp(stageIndex, 0, 4)];
            _targetCollectionCount = stageIndex switch
            {
                0 => 3, 1 => 5, 2 => 7, 3 => 9, _ => 10
            };

            // Spawn initial fish (3 per stage base + stageIndex)
            int fishCount = 3 + stageIndex;
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float yMin = -camSize + 2.8f;
            float yMax = camSize - 1.5f;
            float xMin = -camWidth + 0.5f;
            float xMax = camWidth - 0.5f;

            for (int i = 0; i < fishCount && i < speciesPool.Length; i++)
            {
                int specIdx = speciesPool[i % speciesPool.Length];
                SpawnFish(specIdx, new Vector3(
                    Random.Range(xMin, xMax),
                    Random.Range(yMin, yMax),
                    0f
                ));
            }

            _isActive = true;
            RefreshUI();
        }

        void SpawnFish(int speciesIndex, Vector3 pos)
        {
            if (speciesIndex >= FishSpecies.Length) return;
            if (_fishSprites == null || speciesIndex >= _fishSprites.Length) return;

            var spec = FishSpecies[speciesIndex];
            var go = new GameObject($"Fish_{spec.name}");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.8f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _fishSprites[speciesIndex];
            sr.sortingOrder = 1;

            var fd = new FishData
            {
                go = go,
                sr = sr,
                species = spec.name,
                health = 100f,
                hunger = 100f,
                isSick = false,
                isDiscovered = true,
                isRare = spec.isRare,
                optimalTemp = spec.optTemp,
                optimalPH = spec.optPH,
                velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.3f, 0.3f), 0),
                swimTimer = Random.Range(0f, 3f),
                baseColor = Color.white,
                spriteIndex = speciesIndex
            };
            _fishList.Add(fd);
        }

        void Update()
        {
            if (!_isActive) return;

            float dt = Time.deltaTime;

            // Water quality decay
            _waterQuality = Mathf.Max(0f, _waterQuality - _waterQualityDecayRate * dt);

            // Hunger decay
            foreach (var f in _fishList)
            {
                if (f.go == null) continue;
                f.hunger = Mathf.Max(0f, f.hunger - 5f * dt);
            }

            // Health calculation
            foreach (var f in _fishList)
            {
                if (f.go == null) continue;

                float healthLoss = 0f;

                // Poor water quality
                if (_waterQuality < 50f)
                    healthLoss += (50f - _waterQuality) * 0.05f * dt;

                // Hunger
                if (f.hunger < 30f)
                    healthLoss += (30f - f.hunger) * 0.1f * dt;

                // Non-optimal environment (stage 2+)
                if (_optimalEnvEnabled)
                {
                    float tempDiff = Mathf.Abs(_waterTemperature - f.optimalTemp);
                    if (tempDiff > 5f) healthLoss += (tempDiff - 5f) * 0.2f * dt;

                    float phDiff = Mathf.Abs(_pH - f.optimalPH);
                    if (phDiff > 1f) healthLoss += (phDiff - 1f) * 0.3f * dt;
                }

                // Sick
                if (f.isSick)
                    healthLoss += 5f * dt;

                f.health = Mathf.Max(0f, f.health - healthLoss);

                // Update visual color
                if (f.isSick)
                    f.sr.color = Color.Lerp(f.sr.color, new Color(1f, 0.9f, 0.3f), dt * 3f);
                else if (f.health < 30f)
                    f.sr.color = Color.Lerp(f.sr.color, new Color(1f, 0.4f, 0.4f), dt * 2f);
                else
                    f.sr.color = Color.Lerp(f.sr.color, Color.white, dt * 2f);

                // Fish swimming movement
                f.swimTimer -= dt;
                if (f.swimTimer <= 0f)
                {
                    f.velocity = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.4f, 0.4f), 0);
                    f.swimTimer = Random.Range(2f, 5f);
                }

                Vector3 newPos = f.go.transform.position + f.velocity * dt;
                float camSize = Camera.main.orthographicSize;
                float camWidth = camSize * Camera.main.aspect;
                newPos.x = Mathf.Clamp(newPos.x, -camWidth + 0.5f, camWidth - 0.5f);
                newPos.y = Mathf.Clamp(newPos.y, -camSize + 2.8f, camSize - 1.5f);
                f.go.transform.position = newPos;

                // Flip sprite to face movement direction
                if (f.velocity.x < -0.1f)
                    f.go.transform.localScale = new Vector3(-0.8f, 0.8f, 1f);
                else if (f.velocity.x > 0.1f)
                    f.go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }

            // Disease event (stage 4+)
            if (_diseaseEnabled)
            {
                _diseaseTimer -= dt;
                if (_diseaseTimer <= 0f)
                {
                    TriggerDiseaseEvent();
                    _diseaseTimer = _diseaseInterval;
                }
            }

            // Health bonus (all fish healthy)
            _healthBonusTimer += dt;
            if (_healthBonusTimer >= 1f)
            {
                _healthBonusTimer = 0f;
                float avgHealth = GetAverageHealth();
                if (avgHealth >= 80f)
                {
                    _totalScore += 1;
                    _gameManager.UpdateScoreDisplay(_totalScore);
                }
            }

            // Check game over
            bool anyAlive = false;
            foreach (var f in _fishList)
                if (f.go != null && f.health > 0f) anyAlive = true;
            if (!anyAlive && _fishList.Count > 0)
            {
                _isActive = false;
                _gameManager.OnGameOver();
                return;
            }

            // Check stage clear
            if (!_isStageClear)
            {
                int discovered = CountDiscoveredSpecies();
                if (discovered >= _targetCollectionCount)
                {
                    _isStageClear = true;
                    _isActive = false;
                    _gameManager.OnStageClear();
                }
            }

            RefreshUI();
        }

        void TriggerDiseaseEvent()
        {
            if (_fishList.Count == 0) return;
            int idx = Random.Range(0, _fishList.Count);
            if (!_fishList[idx].isSick)
                _fishList[idx].isSick = true;
        }

        int CountDiscoveredSpecies()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var f in _fishList)
                if (f.isDiscovered) seen.Add(f.species);
            return seen.Count;
        }

        float GetAverageHealth()
        {
            int aliveCount = 0;
            float sum = 0f;
            foreach (var f in _fishList)
            {
                if (f.go != null && f.health > 0f) { sum += f.health; aliveCount++; }
            }
            return aliveCount > 0 ? sum / aliveCount : 0f;
        }

        void RefreshUI()
        {
            _gameManager.UpdateWaterQualityDisplay(_waterQuality);
            _gameManager.UpdateAverageHealthDisplay(GetAverageHealth());
            _gameManager.UpdateFeedCountDisplay(_feedCount);
            int discovered = CountDiscoveredSpecies();
            _gameManager.UpdateCollectionDisplay(discovered, _targetCollectionCount);
        }

        // ===== Public button handlers =====

        public void OnFeedPressed()
        {
            if (!_isActive) return;
            if (_feedCount <= 0) return;

            _feedCount--;

            int lowHungerCount = 0;
            foreach (var f in _fishList)
            {
                if (f.go == null || f.health <= 0f) continue;
                if (f.hunger <= 30f) lowHungerCount++;
                f.hunger = Mathf.Min(100f, f.hunger + 20f);
                StartCoroutine(PopAnimation(f.go.transform));
            }

            int baseScore = 10 * Mathf.Max(1, _fishList.Count);
            if (lowHungerCount >= 2)
            {
                _combo++;
                _comboMultiplier = Mathf.Min(3.0f, 1.0f + _combo * 0.3f);
                baseScore = Mathf.RoundToInt(baseScore * _comboMultiplier);
                _gameManager.UpdateComboDisplay(_combo);
                StartCoroutine(ComboTextPop());
            }
            else
            {
                _combo = 0;
                _comboMultiplier = 1.0f;
                _gameManager.UpdateComboDisplay(0);
            }

            _totalScore += baseScore;
            _gameManager.UpdateScoreDisplay(_totalScore);

            // Replenish feed count after interval (simulate rounds)
            StartCoroutine(ReplenishFeed());
        }

        public void OnCleanPressed()
        {
            if (!_isActive) return;

            _waterQuality = Mathf.Min(100f, _waterQuality + 30f);
            _totalScore += 20;
            _gameManager.UpdateScoreDisplay(_totalScore);

            StartCoroutine(WaterCleanFlash());
        }

        public void OnBreedPressed()
        {
            if (!_isActive) return;
            if (!_breedingEnabled) return;

            // Find two healthy fish
            List<FishData> candidates = new();
            foreach (var f in _fishList)
                if (f.go != null && f.health >= 80f) candidates.Add(f);

            bool canBreed = candidates.Count >= 2;

            // Strict breeding: also check environment (stage 5+)
            if (_strictBreedingEnabled)
            {
                // Check if at least one fish's optimal env is close to current
                bool tempOk = false;
                bool phOk = false;
                foreach (var f in candidates)
                {
                    if (Mathf.Abs(_waterTemperature - f.optimalTemp) <= 3f) tempOk = true;
                    if (Mathf.Abs(_pH - f.optimalPH) <= 0.5f) phOk = true;
                }
                if (!tempOk || !phOk) canBreed = false;
            }

            if (!canBreed)
            {
                _combo = 0;
                _comboMultiplier = 1.0f;
                _gameManager.UpdateComboDisplay(0);
                return;
            }

            // Spawn new fish (try to discover a new species)
            int[] stageSpecies = StageSpeciesUnlocks[Mathf.Clamp(_stageIndex, 0, 4)];
            var discoveredSpecies = new System.Collections.Generic.HashSet<string>();
            foreach (var f in _fishList) discoveredSpecies.Add(f.species);

            int newSpecIdx = -1;
            foreach (int si in stageSpecies)
            {
                if (si < FishSpecies.Length && !discoveredSpecies.Contains(FishSpecies[si].name))
                {
                    newSpecIdx = si;
                    break;
                }
            }

            if (newSpecIdx < 0) newSpecIdx = stageSpecies[Random.Range(0, stageSpecies.Length)];

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            Vector3 spawnPos = new(
                Random.Range(-camWidth + 1f, camWidth - 1f),
                Random.Range(-camSize + 3f, camSize - 2f),
                0f
            );

            SpawnFish(newSpecIdx, spawnPos);
            var newFish = _fishList[_fishList.Count - 1];
            StartCoroutine(AppearAnimation(newFish.go.transform));

            _combo++;
            _comboMultiplier = Mathf.Min(3.0f, 1.0f + _combo * 0.3f);
            int score = Mathf.RoundToInt(50 * _comboMultiplier);
            _totalScore += score;
            _gameManager.UpdateScoreDisplay(_totalScore);
            _gameManager.UpdateComboDisplay(_combo);
            StartCoroutine(ComboTextPop());
        }

        IEnumerator PopAnimation(Transform t)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.2f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1.0f, 1.3f, ratio * 2f)
                    : Mathf.Lerp(1.3f, 1.0f, (ratio - 0.5f) * 2f);
                float sign = orig.x < 0 ? -1f : 1f;
                t.localScale = new Vector3(sign * 0.8f * scale, 0.8f * scale, 1f);
                yield return null;
            }
            if (t != null) t.localScale = orig;
        }

        IEnumerator AppearAnimation(Transform t)
        {
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.4f;
                float scale = ratio < 0.6f
                    ? Mathf.Lerp(0f, 1.2f, ratio / 0.6f)
                    : Mathf.Lerp(1.2f, 0.8f, (ratio - 0.6f) / 0.4f);
                t.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            t.localScale = new Vector3(0.8f, 0.8f, 1f);
        }

        IEnumerator WaterCleanFlash()
        {
            var cam = Camera.main;
            Color orig = cam.backgroundColor;
            cam.backgroundColor = new Color(0.1f, 0.4f, 0.8f);
            yield return new WaitForSeconds(0.15f);
            cam.backgroundColor = new Color(0.06f, 0.2f, 0.5f);
            yield return new WaitForSeconds(0.15f);
            cam.backgroundColor = orig;
        }

        IEnumerator ComboTextPop()
        {
            // Signal UI to do combo animation
            _ui.TriggerComboPop();
            yield return null;
        }

        IEnumerator ReplenishFeed()
        {
            yield return new WaitForSeconds(10f);
            if (_isActive)
            {
                _feedCount = Mathf.Min(_maxFeedCount, _feedCount + 1);
            }
        }

        void OnDestroy()
        {
            foreach (var f in _fishList)
            {
                if (f.go != null) Destroy(f.go);
            }
            _fishList.Clear();

            foreach (var t in _createdTextures)
            {
                if (t != null) Destroy(t);
            }
            _createdTextures.Clear();
        }
    }
}
