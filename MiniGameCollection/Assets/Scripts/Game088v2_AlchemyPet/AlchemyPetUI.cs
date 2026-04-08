using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Game088v2_AlchemyPet
{
    public class AlchemyPetUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _missText;
        [SerializeField] TextMeshProUGUI _goalText;
        [SerializeField] TextMeshProUGUI _messageText;

        [Header("Alchemy Area")]
        [SerializeField] Image[] _slotImages;
        [SerializeField] GameObject[] _slotObjects;
        [SerializeField] Button _combineButton;

        [Header("Pet Area")]
        [SerializeField] Image _petImage;
        [SerializeField] TextMeshProUGUI _petNameText;
        [SerializeField] TextMeshProUGUI _petLevelText;
        [SerializeField] Button _feedButton;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [Header("Discovery")]
        [SerializeField] GameObject _discoveryPanel;
        [SerializeField] Image _discoveryPetImage;
        [SerializeField] TextMeshProUGUI _discoveryScoreText;

        [Header("Inventory")]
        [SerializeField] Button[] _materialButtons;
        [SerializeField] Image[] _materialButtonImages;
        [SerializeField] TextMeshProUGUI[] _materialCountTexts;

        // Runtime state
        Coroutine _messageCoroutine;
        Coroutine _discoveryCoroutine;
        int _messageColorState;

        void Awake()
        {
            HideAllPanels();
        }

        void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_discoveryPanel != null) _discoveryPanel.SetActive(false);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo > 1)
            {
                _comboText.text = $"Combo x{combo} (×{multiplier:F1})";
                _comboText.color = combo >= 4 ? new Color(1f, 0.8f, 0f)
                                 : combo >= 3 ? new Color(1f, 0.6f, 0.2f)
                                 : new Color(1f, 0.9f, 0.4f);
            }
            else
            {
                _comboText.text = "";
            }
        }

        public void UpdateMiss(int miss, int maxMiss)
        {
            if (_missText != null)
            {
                _missText.text = $"失敗: {miss}/{maxMiss}";
                _missText.color = miss >= maxMiss - 1 ? Color.red : Color.white;
            }
        }

        public void UpdateStageGoal(int discovered, int goal)
        {
            if (_goalText != null)
                _goalText.text = $"図鑑: {discovered}/{goal}";
        }

        public void SetSlotCount(int count)
        {
            if (_slotObjects == null) return;
            for (int i = 0; i < _slotObjects.Length; i++)
            {
                if (_slotObjects[i] != null)
                    _slotObjects[i].SetActive(i < count);
            }
        }

        public void UpdateSlot(int index, int materialId, Sprite sprite)
        {
            if (_slotImages == null || index >= _slotImages.Length) return;
            var img = _slotImages[index];
            if (img == null) return;
            if (materialId < 0 || sprite == null)
            {
                img.sprite = null;
                img.color = new Color(0.5f, 0.3f, 0.1f, 0.5f);
            }
            else
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
        }

        public void RefreshInventory(Dictionary<int, int> inventory, Sprite[] matSprites, int maxId)
        {
            if (_materialButtons == null) return;
            for (int i = 0; i < _materialButtons.Length; i++)
            {
                if (_materialButtons[i] == null) continue;
                bool available = i <= maxId;
                _materialButtons[i].gameObject.SetActive(available);
                if (!available) continue;

                if (_materialButtonImages != null && i < _materialButtonImages.Length && _materialButtonImages[i] != null)
                {
                    if (matSprites != null && i < matSprites.Length && matSprites[i] != null)
                        _materialButtonImages[i].sprite = matSprites[i];
                }

                int count = inventory.TryGetValue(i, out int c) ? c : 0;
                if (_materialCountTexts != null && i < _materialCountTexts.Length && _materialCountTexts[i] != null)
                {
                    _materialCountTexts[i].text = count.ToString();
                    _materialCountTexts[i].color = count <= 0 ? Color.red : Color.white;
                }
                _materialButtons[i].interactable = count > 0;
            }
        }

        public void UpdatePetDisplay(int petId, Sprite sprite)
        {
            if (_petImage != null)
            {
                _petImage.sprite = sprite;
                _petImage.color = sprite != null ? Color.white : new Color(0f, 0f, 0f, 0f);
            }
            if (_petNameText != null)
                _petNameText.text = petId >= 0 ? $"Pet #{petId:D2}" : "";
            if (_petLevelText != null)
                _petLevelText.text = petId >= 0 ? "Lv.1" : "";
            if (_feedButton != null)
                _feedButton.interactable = petId >= 0;
        }

        public void UpdatePetLevel(int level, int maxLevel)
        {
            if (_petLevelText != null)
                _petLevelText.text = level >= maxLevel ? $"Lv.MAX" : $"Lv.{level}";
        }

        public void ShowDiscovery(int petId, int score)
        {
            if (_discoveryCoroutine != null) StopCoroutine(_discoveryCoroutine);
            _discoveryCoroutine = StartCoroutine(DiscoveryRoutine(petId, score));
        }

        IEnumerator DiscoveryRoutine(int petId, int score)
        {
            if (_discoveryPanel != null) _discoveryPanel.SetActive(true);
            if (_discoveryScoreText != null) _discoveryScoreText.text = $"New Pet! +{score}pt";

            // Scale pulse effect
            if (_discoveryPanel != null)
            {
                var rt = _discoveryPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float t = 0f;
                    rt.localScale = Vector3.zero;
                    while (t < 0.2f)
                    {
                        t += Time.deltaTime;
                        float s = Mathf.Lerp(0f, 1.3f, t / 0.2f);
                        rt.localScale = Vector3.one * s;
                        yield return null;
                    }
                    t = 0f;
                    while (t < 0.15f)
                    {
                        t += Time.deltaTime;
                        float s = Mathf.Lerp(1.3f, 1.0f, t / 0.15f);
                        rt.localScale = Vector3.one * s;
                        yield return null;
                    }
                    rt.localScale = Vector3.one;
                }
            }
            yield return new WaitForSeconds(1.5f);
            if (_discoveryPanel != null) _discoveryPanel.SetActive(false);
        }

        public void PlayDiscoveryEffect()
        {
            // Called from AlchemyManager coroutine — handled via ShowDiscovery
        }

        public void PlayExplosionEffect()
        {
            if (_discoveryCoroutine != null) StopCoroutine(_discoveryCoroutine);
            _discoveryCoroutine = StartCoroutine(ExplosionRoutine());
        }

        IEnumerator ExplosionRoutine()
        {
            // Red flash on cauldron area (combineButton)
            if (_combineButton != null)
            {
                var img = _combineButton.GetComponent<Image>();
                if (img != null)
                {
                    Color orig = img.color;
                    float t = 0f;
                    while (t < 0.3f)
                    {
                        t += Time.deltaTime;
                        float flash = Mathf.PingPong(t * 10f, 1f);
                        img.color = Color.Lerp(orig, Color.red, flash);
                        yield return null;
                    }
                    img.color = orig;
                }
            }
            ShowMessage("💥 爆発！失敗カウント +1");
        }

        public void ShowMessage(string msg)
        {
            if (_messageText == null) return;
            if (_messageCoroutine != null) StopCoroutine(_messageCoroutine);
            _messageCoroutine = StartCoroutine(MessageRoutine(msg));
        }

        IEnumerator MessageRoutine(string msg)
        {
            _messageText.text = msg;
            _messageText.color = new Color(1f, 0.9f, 0.4f, 1f);
            yield return new WaitForSeconds(2f);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _messageText.color = new Color(1f, 0.9f, 0.4f, 1f - t / 0.5f);
                yield return null;
            }
            _messageText.text = "";
        }

        public void ShowStageClear(int stageNumber)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"Stage {stageNumber} Clear!";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void ShowAllClear(int totalScore)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Total Score: {totalScore}";
        }
    }
}
