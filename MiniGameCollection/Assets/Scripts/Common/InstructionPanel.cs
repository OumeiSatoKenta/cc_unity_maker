using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// ゲーム開始時にチュートリアルオーバーレイを表示する共通コンポーネント。
/// タイトル・説明・操作方法・ゴールを表示し、「はじめる」で開始。
/// 2回目以降はPlayerPrefsで自動スキップ（?ボタンで再表示可能）。
/// </summary>
public class InstructionPanel : MonoBehaviour
{
    [SerializeField, Tooltip("パネルルート")] private GameObject _panelRoot;
    [SerializeField, Tooltip("タイトルテキスト")] private TextMeshProUGUI _titleText;
    [SerializeField, Tooltip("説明テキスト")] private TextMeshProUGUI _descriptionText;
    [SerializeField, Tooltip("操作方法テキスト")] private TextMeshProUGUI _controlsText;
    [SerializeField, Tooltip("ゴールテキスト")] private TextMeshProUGUI _goalText;
    [SerializeField, Tooltip("はじめるボタン")] private Button _startButton;
    [SerializeField, Tooltip("?ボタン（再表示用）")] private Button _helpButton;

    private string _gameId;
    private string _title;
    private string _description;
    private string _controls;
    private string _goal;

    /// <summary>パネルが閉じられたときに発火するイベント</summary>
    public event Action OnDismissed;

    private string PrefsKey => $"instruction_seen_{_gameId}";

    /// <summary>
    /// チュートリアルを表示する。初回のみ自動表示、2回目以降はスキップ。
    /// </summary>
    public void Show(string gameId, string title, string description, string controls, string goal)
    {
        _gameId = gameId;
        _title = title;
        _description = description;
        _controls = controls;
        _goal = goal;

        if (_startButton != null)
            _startButton.onClick.AddListener(OnStartButtonClicked);
        if (_helpButton != null)
            _helpButton.onClick.AddListener(ShowPanel);

        if (PlayerPrefs.GetInt(PrefsKey, 0) == 1)
        {
            // 2回目以降: スキップして即開始
            if (_panelRoot != null) _panelRoot.SetActive(false);
            if (_helpButton != null) _helpButton.gameObject.SetActive(true);
            OnDismissed?.Invoke();
        }
        else
        {
            ShowPanel();
        }
    }

    private void ShowPanel()
    {
        if (_panelRoot == null) return;
        _panelRoot.SetActive(true);
        if (_helpButton != null) _helpButton.gameObject.SetActive(false);

        if (_titleText != null) _titleText.text = _title;
        if (_descriptionText != null) _descriptionText.text = _description;
        if (_controlsText != null) _controlsText.text = $"【操作】{_controls}";
        if (_goalText != null) _goalText.text = $"【ゴール】{_goal}";

        Time.timeScale = 0f;
    }

    private void OnStartButtonClicked()
    {
        PlayerPrefs.SetInt(PrefsKey, 1);
        PlayerPrefs.Save();

        if (_panelRoot != null) _panelRoot.SetActive(false);
        if (_helpButton != null) _helpButton.gameObject.SetActive(true);

        Time.timeScale = 1f;
        OnDismissed?.Invoke();
    }

    private void OnDestroy()
    {
        // シーン破棄時にTimeScaleを確実に戻す
        Time.timeScale = 1f;
    }
}
