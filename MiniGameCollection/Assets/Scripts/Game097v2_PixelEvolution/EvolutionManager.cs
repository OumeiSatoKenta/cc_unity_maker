using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game097v2_PixelEvolution
{
    public enum EnvLevel { Low = 0, Mid = 1, High = 2 }

    public class EvolutionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PixelEvolutionGameManager _gameManager;
        [SerializeField] PixelEvolutionUI _ui;

        [Header("Sprites - Evolution Levels")]
        [SerializeField] Sprite _spriteLv0;
        [SerializeField] Sprite _spriteLv1;
        [SerializeField] Sprite _spriteLv2;
        [SerializeField] Sprite _spriteLv3;
        [SerializeField] Sprite _spriteLv4;
        [SerializeField] Sprite _spriteLv5;

        [Header("Display")]
        [SerializeField] SpriteRenderer _evolutionDisplay;

        // State
        int _evolutionLevel;
        int _generation;
        int _generationLimit;
        EnvLevel _temperature = EnvLevel.Mid;
        EnvLevel _humidity = EnvLevel.Mid;
        EnvLevel _light = EnvLevel.Mid;
        bool _isActive;
        int _stageIndex;
        float _complexityFactor;
        int _branchCount;
        bool _hasMutation;
        bool _pendingBranchChoice;

        // Combo
        int _consecutiveOptimalCount;

        public void SetActive(bool active) => _isActive = active;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _complexityFactor = config.complexityFactor;
            _branchCount = config.countMultiplier;
            _hasMutation = stageIndex >= 4;
            _generationLimit = stageIndex switch
            {
                0 => 20,
                1 => 15,
                2 => 12,
                3 => 10,
                _ => 8
            };

            _evolutionLevel = 0;
            _generation = 0;
            _temperature = EnvLevel.Mid;
            _humidity = EnvLevel.Mid;
            _light = EnvLevel.Mid;
            _consecutiveOptimalCount = 0;
            _pendingBranchChoice = false;
            _isActive = true;

            UpdateDisplaySprite();
            _ui?.UpdateEvolutionLevel(_evolutionLevel, 5);
            _ui?.UpdateGeneration(_generation, _generationLimit);
            _ui?.UpdateEnvironment(_temperature, _humidity, _light);
        }

        // --- Environment setters (called by UI buttons) ---
        public void SetTemperature(int level) { if (!_isActive || _pendingBranchChoice) return; _temperature = (EnvLevel)level; _ui?.UpdateEnvironment(_temperature, _humidity, _light); }
        public void SetHumidity(int level) { if (!_isActive || _pendingBranchChoice) return; _humidity = (EnvLevel)level; _ui?.UpdateEnvironment(_temperature, _humidity, _light); }
        public void SetLight(int level) { if (!_isActive || _pendingBranchChoice) return; _light = (EnvLevel)level; _ui?.UpdateEnvironment(_temperature, _humidity, _light); }

        // --- Advance Generation (called by UI button) ---
        public void AdvanceGeneration()
        {
            if (!_isActive || _pendingBranchChoice) return;

            _generation++;
            _ui?.UpdateGeneration(_generation, _generationLimit);

            // Mutation (Stage 5)
            bool mutationOccurred = false;
            if (_hasMutation && Random.value < _complexityFactor * 0.1f)
            {
                mutationOccurred = true;
                int mutDir = Random.Range(0, 2) == 0 ? 1 : -1;
                _evolutionLevel = Mathf.Clamp(_evolutionLevel + mutDir, 0, 5);
                UpdateDisplaySprite();
                _ui?.UpdateEvolutionLevel(_evolutionLevel, 5);
                _ui?.ShowMutationEffect();

                if (_evolutionLevel >= 5)
                {
                    _isActive = false;
                    _gameManager?.OnEvolutionComplete(_generation, _generationLimit);
                    return;
                }
            }

            if (!mutationOccurred)
            {
                int evDir = CalculateEvolutionDirection();
                bool isDevolve = evDir < 0;

                if (evDir > 0 || isDevolve)
                {
                    bool isOptimal = IsOptimalEnvironment();
                    _evolutionLevel = Mathf.Clamp(_evolutionLevel + evDir, 0, 5);
                    UpdateDisplaySprite();
                    _ui?.UpdateEvolutionLevel(_evolutionLevel, 5);
                    StartCoroutine(PlayEvolutionAnimation(isDevolve));
                    _gameManager?.OnEvolutionAdvanced(isOptimal, false);
                }
            }

            // Check for branch point (odd evolution level changes)
            if (_evolutionLevel > 0 && _evolutionLevel < 5 && _evolutionLevel % 2 == 1 && !_pendingBranchChoice)
            {
                bool hiddenAvailable = _stageIndex >= 3 && IsHiddenBranchCondition();
                int branchCount = hiddenAvailable ? Mathf.Max(_branchCount, 3) : _branchCount;
                if (branchCount >= 2)
                {
                    _pendingBranchChoice = true;
                    _ui?.ShowBranchChoice(branchCount, hiddenAvailable);
                    return;
                }
            }

            // Check clear
            if (_evolutionLevel >= 5)
            {
                _isActive = false;
                _gameManager?.OnEvolutionComplete(_generation, _generationLimit);
                return;
            }

            // Check generation limit
            if (_generation >= _generationLimit)
            {
                _isActive = false;
                _gameManager?.OnGenerationLimitReached();
            }
        }

        // --- Branch selection ---
        public void SelectBranch(int index)
        {
            if (!_pendingBranchChoice) return;
            _pendingBranchChoice = false;
            _ui?.HideBranchChoice();

            bool isHidden = index == 2 && _stageIndex >= 3;
            int bonus = isHidden ? 1 : 0;
            _evolutionLevel = Mathf.Clamp(_evolutionLevel + 1 + bonus, 0, 5);
            UpdateDisplaySprite();
            _ui?.UpdateEvolutionLevel(_evolutionLevel, 5);
            StartCoroutine(PlayEvolutionAnimation(false));
            _gameManager?.OnEvolutionAdvanced(false, isHidden);

            if (_evolutionLevel >= 5)
            {
                _isActive = false;
                _gameManager?.OnEvolutionComplete(_generation, _generationLimit);
                return;
            }

            if (_generation >= _generationLimit)
            {
                _isActive = false;
                _gameManager?.OnGenerationLimitReached();
            }
        }

        // --- Internal ---

        int CalculateEvolutionDirection()
        {
            bool optimal = IsOptimalEnvironment();
            bool dangerous = IsDangerousEnvironment();

            if (optimal)
            {
                _consecutiveOptimalCount++;
                return 1;
            }

            // Stage 3+: devolution risk
            if (dangerous && _stageIndex >= 2)
            {
                float devolveChance = _complexityFactor * 0.2f;
                if (Random.value < devolveChance)
                {
                    _consecutiveOptimalCount = 0;
                    return -1;
                }
            }

            _consecutiveOptimalCount = 0;
            return 0;
        }

        bool IsOptimalEnvironment()
        {
            // Stage 1: Temperature High is main factor
            if (_stageIndex == 0)
                return _temperature == EnvLevel.High;

            // Stage 2+: combination matters
            if (_stageIndex >= 1)
                return _temperature == EnvLevel.High && _humidity == EnvLevel.Mid;

            return false;
        }

        bool IsDangerousEnvironment()
        {
            return _temperature == EnvLevel.Low && _humidity == EnvLevel.Low && _light == EnvLevel.Low;
        }

        bool IsHiddenBranchCondition()
        {
            return _temperature == EnvLevel.High && _humidity == EnvLevel.High && _light == EnvLevel.High;
        }

        void UpdateDisplaySprite()
        {
            if (_evolutionDisplay == null) return;
            _evolutionDisplay.sprite = _evolutionLevel switch
            {
                0 => _spriteLv0,
                1 => _spriteLv1,
                2 => _spriteLv2,
                3 => _spriteLv3,
                4 => _spriteLv4,
                _ => _spriteLv5
            };
        }

        IEnumerator PlayEvolutionAnimation(bool isDevolve)
        {
            if (_evolutionDisplay == null) yield break;
            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;

            if (isDevolve)
            {
                // Red flash + shake
                _evolutionDisplay.color = new Color(1f, 0.3f, 0.3f, 1f);
                float shakeAmt = 0.15f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float sx = Random.Range(-shakeAmt, shakeAmt);
                    float sy = Random.Range(-shakeAmt, shakeAmt);
                    _evolutionDisplay.transform.localPosition = new Vector3(sx, sy, 0);
                    yield return null;
                }
                _evolutionDisplay.transform.localPosition = Vector3.zero;
                _evolutionDisplay.color = Color.white;
            }
            else
            {
                // Green flash + scale pulse
                _evolutionDisplay.color = new Color(0.7f, 1f, 0.5f, 1f);
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float scale = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                    _evolutionDisplay.transform.localScale = baseScale * scale;
                    yield return null;
                }
                _evolutionDisplay.transform.localScale = baseScale;
                _evolutionDisplay.color = Color.white;
            }
        }
    }
}
