using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game090v2_StarshipCrew
{
    public class StarshipCrewUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _requiredText;

        [Header("Crew Area")]
        [SerializeField] Transform _crewCardsContainer;
        [SerializeField] GameObject _crewCardPrefab;

        [Header("Mission Area")]
        [SerializeField] Transform _missionButtonsContainer;
        [SerializeField] GameObject _missionButtonPrefab;
        [SerializeField] Button _dispatchButton;
        [SerializeField] Button _cancelButton;
        [SerializeField] TextMeshProUGUI _successRateText;
        [SerializeField] TextMeshProUGUI _compatText;

        [Header("Equipment")]
        [SerializeField] GameObject _equipmentPanel;
        [SerializeField] Button _equipCombatBtn;
        [SerializeField] Button _equipEngineBtn;
        [SerializeField] Button _equipMedicalBtn;

        [Header("Result Panel")]
        [SerializeField] GameObject _resultPanel;
        [SerializeField] TextMeshProUGUI _resultTitleText;
        [SerializeField] TextMeshProUGUI _resultScoreText;
        [SerializeField] TextMeshProUGUI _resultBonusText;
        [SerializeField] Button _continueButton;
        [SerializeField] Image _resultBgImage;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [Header("All Clear Panel")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [Header("Screen Flash")]
        [SerializeField] Image _screenFlash;

        [SerializeField] StarshipCrewGameManager _gameManager;
        [SerializeField] CrewManager _crewManager;

        List<Button> _crewCardButtons = new List<Button>();
        List<Image>  _crewCardImages  = new List<Image>();
        List<Button> _missionButtons  = new List<Button>();

        // Colors
        static readonly Color ColSelected  = new Color(1f, 0.9f, 0.2f, 1f);
        static readonly Color ColNormal    = Color.white;
        static readonly Color ColCompleted = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        static readonly Color ColGoodCompat= new Color(0.2f, 1f, 0.4f, 1f);
        static readonly Color ColBadCompat = new Color(1f, 0.3f, 0.3f, 1f);
        static readonly Color ColSuccess   = new Color(0.2f, 0.8f, 0.3f, 0.95f);
        static readonly Color ColPerfect   = new Color(1f, 0.8f, 0.1f, 0.95f);
        static readonly Color ColFail      = new Color(0.8f, 0.2f, 0.2f, 0.95f);

        public void SetupCrewCards(List<CrewData> stageCrew, CrewData[] allCrew)
        {
            // Clear existing
            foreach (Transform child in _crewCardsContainer) Destroy(child.gameObject);
            _crewCardButtons.Clear();
            _crewCardImages.Clear();

            for (int i = 0; i < stageCrew.Count; i++)
            {
                int idx = i; // capture
                var card = Instantiate(_crewCardPrefab, _crewCardsContainer);
                var btn = card.GetComponent<Button>();
                var img = card.GetComponent<Image>();
                if (img == null) img = card.GetComponentInChildren<Image>();
                var texts = card.GetComponentsInChildren<TextMeshProUGUI>();

                // Portrait
                var portraitImg = card.transform.Find("Portrait")?.GetComponent<Image>();
                if (portraitImg != null && stageCrew[i].portrait != null)
                    portraitImg.sprite = stageCrew[i].portrait;

                // Name
                if (texts.Length > 0) texts[0].text = stageCrew[i].crewName;
                // Skills
                if (texts.Length > 1)
                    texts[1].text = $"戦:{stageCrew[i].combatSkill} 技:{stageCrew[i].engSkill} 医:{stageCrew[i].medSkill}";

                btn.onClick.AddListener(() => _crewManager.OnCrewCardClicked(idx));
                _crewCardButtons.Add(btn);
                _crewCardImages.Add(img);
            }
        }

        public void SetupMissionButtons(MissionData[] missions, Func<Difficulty, Sprite> getSpriteFunc)
        {
            foreach (Transform child in _missionButtonsContainer) Destroy(child.gameObject);
            _missionButtons.Clear();

            for (int i = 0; i < missions.Length; i++)
            {
                int idx = i;
                var btnObj = Instantiate(_missionButtonPrefab, _missionButtonsContainer);
                var btn = btnObj.GetComponent<Button>();
                var img = btnObj.GetComponent<Image>();
                if (img == null) img = btnObj.GetComponentInChildren<Image>();

                Sprite msprite = getSpriteFunc(missions[i].difficulty);
                if (img != null && msprite != null) img.sprite = msprite;

                var texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = missions[i].missionName;
                if (texts.Length > 1) texts[1].text = DifficultyLabel(missions[i].difficulty);

                if (missions[i].isCompleted)
                {
                    btn.interactable = false;
                    if (img != null) img.color = ColCompleted;
                }
                else
                {
                    btn.onClick.AddListener(() => _crewManager.OnMissionSelected(idx));
                }

                _missionButtons.Add(btn);
            }
        }

        string DifficultyLabel(Difficulty d) => d switch
        {
            Difficulty.Easy     => "★ かんたん",
            Difficulty.Medium   => "★★ ふつう",
            Difficulty.Hard     => "★★★ むずかしい",
            Difficulty.VeryHard => "★★★★ 超難関",
            Difficulty.Boss     => "★★★★★ BOSS",
            _ => ""
        };

        public void SetupEquipmentPanel(bool hasEquipment)
        {
            if (_equipmentPanel == null) return;
            _equipmentPanel.SetActive(hasEquipment);
            if (hasEquipment)
            {
                _equipCombatBtn?.onClick.RemoveAllListeners();
                _equipEngineBtn?.onClick.RemoveAllListeners();
                _equipMedicalBtn?.onClick.RemoveAllListeners();
                _equipCombatBtn?.onClick.AddListener(() => _crewManager.OnEquipSelected(EquipType.Combat));
                _equipEngineBtn?.onClick.AddListener(() => _crewManager.OnEquipSelected(EquipType.Engine));
                _equipMedicalBtn?.onClick.AddListener(() => _crewManager.OnEquipSelected(EquipType.Medical));
            }
        }

        public void UpdateCrewSelection(List<int> selectedIndices)
        {
            for (int i = 0; i < _crewCardImages.Count; i++)
            {
                if (_crewCardImages[i] == null) continue;
                bool sel = selectedIndices.Contains(i);
                _crewCardImages[i].color = sel ? ColSelected : ColNormal;
                // Scale pulse
                _crewCardButtons[i].transform.localScale = sel ? Vector3.one * 1.08f : Vector3.one;
            }
        }

        public void UpdateMissionSelection(int selectedIndex, float successRate)
        {
            for (int i = 0; i < _missionButtons.Count; i++)
            {
                if (_missionButtons[i] == null) continue;
                // visual highlight managed by dispatch button text
            }
            if (_successRateText != null)
                _successRateText.text = selectedIndex >= 0 ? $"成功率: {successRate:F0}%" : "ミッションを選択";
        }

        public void UpdateDispatchButton(bool canDispatch, float successRate)
        {
            if (_dispatchButton != null)
                _dispatchButton.interactable = canDispatch;
        }

        public void ShowCompatibility(List<int> selectedIndices, List<CrewData> stageCrew)
        {
            if (_compatText == null) return;
            if (selectedIndices.Count < 2) { _compatText.text = ""; return; }

            int goodPairs = 0, badPairs = 0;
            for (int a = 0; a < selectedIndices.Count; a++)
            {
                for (int b = a + 1; b < selectedIndices.Count; b++)
                {
                    int idxB = selectedIndices[b];
                    if (System.Array.IndexOf(stageCrew[selectedIndices[a]].goodCompat, idxB) >= 0) goodPairs++;
                    if (System.Array.IndexOf(stageCrew[selectedIndices[a]].badCompat,  idxB) >= 0) badPairs++;
                }
            }

            if (selectedIndices.Count >= 3 && goodPairs >= 2)
                _compatText.text = "<color=#FFD700>★ 3人シナジー発動! ×1.5ボーナス!</color>";
            else if (goodPairs > 0)
                _compatText.text = $"<color=#44FF88>良相性 {goodPairs}ペア (+{goodPairs*20}%)</color>";
            else if (badPairs > 0)
                _compatText.text = $"<color=#FF4444>相性悪化 {badPairs}ペア (-{badPairs*15}%)</color>";
            else
                _compatText.text = "相性: 普通";
        }

        public void ShowMissionResult(bool success, bool isPerfect, int earned, int synergyBonus)
        {
            if (_resultPanel == null) return;
            _resultPanel.SetActive(true);

            if (_resultBgImage != null)
                _resultBgImage.color = isPerfect ? ColPerfect : success ? ColSuccess : ColFail;

            if (_resultTitleText != null)
                _resultTitleText.text = isPerfect ? "完璧クリア!" : success ? "ミッション成功!" : "ミッション失敗...";

            if (_resultScoreText != null)
                _resultScoreText.text = success ? $"+{earned}pt" : "ー";

            if (_resultBonusText != null)
            {
                if (synergyBonus > 0)
                    _resultBonusText.text = $"シナジーボーナス: +{synergyBonus}pt";
                else
                    _resultBonusText.text = success ? "コンボ継続中!" : "コンボリセット";
            }

            // Pop animation
            StartCoroutine(PopAnimation(_resultPanel.transform));

            // Flash screen on failure
            if (!success)
                StartCoroutine(FlashRed());
        }

        IEnumerator PopAnimation(Transform t)
        {
            t.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(0f, 1.2f, elapsed / 0.15f);
                t.localScale = Vector3.one * s;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < 0.08f)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1.2f, 1.0f, elapsed / 0.08f);
                t.localScale = Vector3.one * s;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator FlashRed()
        {
            if (_screenFlash == null) yield break;
            _screenFlash.color = new Color(1f, 0f, 0f, 0.45f);
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(0.45f, 0f, elapsed / 0.4f);
                _screenFlash.color = new Color(1f, 0f, 0f, a);
                yield return null;
            }
            _screenFlash.color = new Color(1f, 0f, 0f, 0f);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
            if (_requiredText != null && _gameManager != null)
                _requiredText.text = $"クリア: {_gameManager.GetMissionsClearedThisStage()} / {_gameManager.GetRequiredClears()}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 2)
                _comboText.text = $"Combo x{combo}  ×{multiplier:F1}";
            else
                _comboText.text = "";
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"Stage {stage} クリア!\nScore: {score}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_resultPanel != null) _resultPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Total Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        // Called by Continue button (UnityEvent)
        public void OnContinueClicked()
        {
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (_gameManager != null)
            {
                int stage = _gameManager.GetCurrentStageIndex() + 1;
                UpdateStage(stage, 5);
            }
        }
    }
}
