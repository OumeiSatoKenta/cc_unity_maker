using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Game061v2_CookieFactory
{
    public class CookieManager : MonoBehaviour
    {
        [SerializeField] CookieFactoryGameManager _gameManager;
        [SerializeField] CookieFactoryUI _ui;
        [SerializeField] RectTransform _cookieButtonTransform;

        // Cookie state
        public long TotalCookies { get; private set; }
        long _goalCookies;
        float _autoRate; // cookies per second
        int _tapPower = 1;

        // Combo
        int _combo;
        float _comboTimer;
        const float ComboTimeout = 1.5f;

        // Upgrades
        int _ovenLevel;      // max 3
        int _conveyorLevel;  // max 3, unlocked stage2+
        int _packagingLevel; // max 3, unlocked stage3+

        static readonly long[] OvenCosts     = { 100, 250, 600 };
        static readonly long[] ConveyorCosts  = { 500, 1500, 5000 };
        static readonly long[] PackagingCosts = { 3000, 10000, 30000 };

        // Special order (stage3+)
        bool _specialOrderUnlocked;
        bool _specialOrderActive;
        float _specialOrderTimer;
        const float SpecialOrderDuration = 15f;
        const long SpecialOrderReward = 500;

        // Breakdown event (stage4+)
        bool _breakdownUnlocked;
        bool _isBroken;
        int _repairTapsNeeded;
        float _breakdownTimer;
        float _nextBreakdownTime;

        // VIP order (stage5+)
        bool _vipUnlocked;
        bool _vipActive;
        float _vipTimer;
        long _vipGoal;
        long _vipStartCookies;
        const float VIPDuration = 30f;
        const long VIPGoalAmount = 500;
        float _nextVIPTime;

        bool _isActive;
        float _speedMultiplier = 1f;
        int _currentStage;
        float _autoAccumulator;

        void Update()
        {
            if (!_isActive || !_gameManager.IsPlaying) return;

            // Auto production (accumulator to handle low rates)
            if (!_isBroken && _autoRate > 0f)
            {
                float effective = _autoRate * _speedMultiplier;
                _autoAccumulator += effective * Time.deltaTime;
                long earned = (long)_autoAccumulator;
                if (earned > 0)
                {
                    _autoAccumulator -= earned;
                    AddCookies(earned);
                }
            }

            // Combo timer
            if (_combo > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _combo = 0;
                    _ui.UpdateCombo(0);
                }
            }

            // Special order countdown
            if (_specialOrderActive)
            {
                _specialOrderTimer -= Time.deltaTime;
                if (_specialOrderTimer <= 0f)
                {
                    _specialOrderActive = false;
                    // restore auto rate
                    RecalcAutoRate();
                    _ui.SetSpecialOrderActive(false);
                }
                _ui.UpdateSpecialOrderTimer(_specialOrderTimer / SpecialOrderDuration);
            }

            // Breakdown event
            if (_breakdownUnlocked && !_isBroken)
            {
                _breakdownTimer += Time.deltaTime;
                if (_breakdownTimer >= _nextBreakdownTime)
                {
                    TriggerBreakdown();
                }
            }

            // VIP order
            if (_vipUnlocked && !_vipActive)
            {
                _nextVIPTime -= Time.deltaTime;
                if (_nextVIPTime <= 0f)
                {
                    TriggerVIPOrder();
                }
            }
            if (_vipActive)
            {
                _vipTimer -= Time.deltaTime;
                long progress = TotalCookies - _vipStartCookies;
                _ui.UpdateVIPOrder(_vipTimer / VIPDuration, progress, _vipGoal, _vipTimer);
                if (progress >= _vipGoal)
                {
                    CompleteVIPOrder();
                }
                else if (_vipTimer <= 0f)
                {
                    _vipActive = false;
                    _ui.SetVIPOrderActive(false);
                    _nextVIPTime = Random.Range(20f, 35f);
                }
            }

            // Goal check
            if (TotalCookies >= _goalCookies)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }

            _ui.UpdateCookies(TotalCookies, _goalCookies);
        }

        public void SetupStage(int stageIndex, StageManager.StageConfig config)
        {
            StopAllCoroutines();
            _currentStage = stageIndex;
            _speedMultiplier = config.speedMultiplier > 0f ? config.speedMultiplier : 1f;

            // goal
            long[] goals = { 100, 1000, 10000, 100000, 1000000 };
            _goalCookies = goals[Mathf.Clamp(stageIndex, 0, goals.Length - 1)];

            TotalCookies = 0;
            _combo = 0;
            _comboTimer = 0f;
            _autoRate = 0f;
            _autoAccumulator = 0f;
            _tapPower = 1;
            _ovenLevel = 0;
            _conveyorLevel = 0;
            _packagingLevel = 0;

            _specialOrderUnlocked = stageIndex >= 2;
            _specialOrderActive = false;
            RecalcAutoRate();

            _breakdownUnlocked = stageIndex >= 3;
            _isBroken = false;
            _repairTapsNeeded = 0;
            _breakdownTimer = 0f;
            _nextBreakdownTime = Random.Range(30f, 60f);

            _vipUnlocked = stageIndex >= 4;
            _vipActive = false;
            _nextVIPTime = Random.Range(25f, 40f);

            _isActive = true;

            bool conveyorVisible = stageIndex >= 1;
            bool packagingVisible = stageIndex >= 2;
            _ui.SetupShop(
                stageIndex,
                OvenCosts, ConveyorCosts, PackagingCosts,
                conveyorVisible, packagingVisible
            );
            _ui.UpdateCookies(0, _goalCookies);
            _ui.UpdateAutoRate(_autoRate);
            _ui.SetSpecialOrderVisible(_specialOrderUnlocked);
            _ui.SetSpecialOrderActive(false);
            _ui.SetVIPOrderActive(false);
            _ui.UpdateCombo(0);
        }

        void AddCookies(long amount)
        {
            TotalCookies += amount;
        }

        public void Tap()
        {
            if (!_isActive || !_gameManager.IsPlaying) return;

            // Combo
            _combo++;
            _comboTimer = ComboTimeout;
            _ui.UpdateCombo(_combo);

            float comboMult = _combo >= 10 ? 2f : _combo >= 5 ? 1.5f : 1f;
            long earned = (long)(_tapPower * comboMult);
            AddCookies(earned);

            // Visual feedback: scale pulse on cookie button
            if (_cookieButtonTransform != null)
                StartCoroutine(ScalePulse(_cookieButtonTransform, 1.15f, 0.12f));

            // Floating text
            _ui.ShowFloatingText($"+{earned}");
        }

        public void BuyUpgrade(int index)
        {
            if (!_isActive) return;

            switch (index)
            {
                case 0: // Oven
                    if (_ovenLevel >= 3) return;
                    long ovenCost = OvenCosts[_ovenLevel];
                    if (TotalCookies < ovenCost) return;
                    TotalCookies -= ovenCost;
                    _ovenLevel++;
                    _tapPower = 1 + _ovenLevel;
                    break;
                case 1: // ConveyorBelt
                    if (_conveyorLevel >= 3 || _currentStage < 1) return;
                    long convCost = ConveyorCosts[_conveyorLevel];
                    if (TotalCookies < convCost) return;
                    TotalCookies -= convCost;
                    _conveyorLevel++;
                    RecalcAutoRate();
                    break;
                case 2: // PackagingMachine
                    if (_packagingLevel >= 3 || _currentStage < 2) return;
                    long packCost = PackagingCosts[_packagingLevel];
                    if (TotalCookies < packCost) return;
                    TotalCookies -= packCost;
                    _packagingLevel++;
                    RecalcAutoRate();
                    break;
            }

            _ui.UpdateShopButtons(
                _ovenLevel, _conveyorLevel, _packagingLevel,
                TotalCookies,
                OvenCosts, ConveyorCosts, PackagingCosts,
                _currentStage
            );
            _ui.UpdateAutoRate(_autoRate);
            _ui.ShowFloatingText("設備購入！");
        }

        void RecalcAutoRate()
        {
            float base_ = _conveyorLevel * 0.5f + _packagingLevel * 3f;
            if (_specialOrderActive) base_ *= 0.3f; // line occupied
            if (_isBroken) base_ *= 0.3f;
            _autoRate = base_;
            _ui.UpdateAutoRate(_autoRate);
        }

        public void StartSpecialOrder()
        {
            if (!_specialOrderUnlocked || _specialOrderActive || !_isActive) return;
            _specialOrderActive = true;
            _specialOrderTimer = SpecialOrderDuration;
            RecalcAutoRate();
            _ui.SetSpecialOrderActive(true);
            // reward comes at end or we give it immediately as incentive
            StartCoroutine(DelayedSpecialReward());
        }

        IEnumerator DelayedSpecialReward()
        {
            yield return new WaitForSeconds(SpecialOrderDuration * 0.8f);
            if (_specialOrderActive)
            {
                AddCookies(SpecialOrderReward);
                _ui.ShowFloatingText($"特注完了！+{SpecialOrderReward}");
            }
        }

        void TriggerBreakdown()
        {
            _isBroken = true;
            _repairTapsNeeded = 5;
            _breakdownTimer = 0f;
            _nextBreakdownTime = Random.Range(40f, 80f);
            RecalcAutoRate();
            _ui.SetBreakdownActive(true, _repairTapsNeeded);
        }

        public void RepairTap()
        {
            if (!_isBroken) return;
            _repairTapsNeeded--;
            _ui.UpdateRepairProgress(_repairTapsNeeded);
            if (_repairTapsNeeded <= 0)
            {
                _isBroken = false;
                RecalcAutoRate();
                _ui.SetBreakdownActive(false, 0);
            }
        }

        void TriggerVIPOrder()
        {
            _vipActive = true;
            _vipTimer = VIPDuration;
            _vipGoal = VIPGoalAmount;
            _vipStartCookies = TotalCookies;
            _nextVIPTime = Random.Range(30f, 50f);
            _ui.SetVIPOrderActive(true);
        }

        void CompleteVIPOrder()
        {
            if (!_isActive) return;
            _vipActive = false;
            long bonus = TotalCookies / 10;
            AddCookies(bonus);
            _ui.SetVIPOrderActive(false);
            _ui.ShowFloatingText($"VIP達成！+{bonus}ボーナス");
        }

        public void BuyOven() => BuyUpgrade(0);
        public void BuyConveyor() => BuyUpgrade(1);
        public void BuyPackaging() => BuyUpgrade(2);

        public void ResetAll()
        {
            StopAllCoroutines();
            _isActive = false;
            TotalCookies = 0;
            _combo = 0;
            _autoRate = 0f;
            _tapPower = 1;
            _ovenLevel = 0;
            _conveyorLevel = 0;
            _packagingLevel = 0;
            _specialOrderActive = false;
            _isBroken = false;
            _vipActive = false;
        }

        IEnumerator ScalePulse(RectTransform rt, float targetScale, float duration)
        {
            Vector3 orig = rt.localScale;
            float half = duration * 0.5f;
            for (float t = 0; t < half; t += Time.deltaTime)
            {
                rt.localScale = Vector3.Lerp(orig, orig * targetScale, t / half);
                yield return null;
            }
            for (float t = 0; t < half; t += Time.deltaTime)
            {
                rt.localScale = Vector3.Lerp(orig * targetScale, orig, t / half);
                yield return null;
            }
            rt.localScale = orig;
        }
    }
}
