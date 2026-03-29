using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゲームカード1枚のUI制御。
/// ゲーム情報の表示とタップ時のシーン遷移を担当する。
/// </summary>
public class GameCardUI : MonoBehaviour
{
    [SerializeField, Tooltip("ゲームIDを表示するテキスト")]
    private TextMeshProUGUI _idText;

    [SerializeField, Tooltip("ゲームタイトルを表示するテキスト")]
    private TextMeshProUGUI _titleText;

    [SerializeField, Tooltip("工数（S/M/L）を表示するテキスト")]
    private TextMeshProUGUI _sizeText;

    [SerializeField, Tooltip("カードのボタンコンポーネント")]
    private Button _button;

    [SerializeField, Tooltip("カードの背景画像")]
    private Image _background;

    private GameEntry _gameEntry;

    /// <summary>
    /// ゲーム情報でカードを初期化する。
    /// </summary>
    public void Setup(GameEntry entry)
    {
        _gameEntry = entry;

        if (_idText != null) _idText.text = entry.id;
        if (_titleText != null) _titleText.text = entry.title;
        if (_sizeText != null) _sizeText.text = entry.size;

        // 未実装ゲームはグレーアウトしてタップ不可にする
        if (!entry.implemented)
        {
            if (_background != null) _background.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            if (_button != null) _button.interactable = false;
            if (_titleText != null) _titleText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnCardClicked);
            }
        }
    }

    private void OnCardClicked()
    {
        if (_gameEntry == null || !_gameEntry.implemented) return;
        SceneLoader.LoadGame(_gameEntry.sceneName);
    }
}
