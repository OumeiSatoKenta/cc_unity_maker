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

    [SerializeField, Tooltip("お気に入りボタン")]
    private Button _favoriteButton;

    [SerializeField, Tooltip("お気に入りアイコンテキスト")]
    private TextMeshProUGUI _favoriteIconText;

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
            if (_favoriteButton != null) _favoriteButton.gameObject.SetActive(false);
        }
        else
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnCardClicked);
            }
        }

        // お気に入りボタン設定
        if (_favoriteButton != null && entry.implemented)
        {
            _favoriteButton.onClick.AddListener(OnFavoriteClicked);
            UpdateFavoriteIcon();
        }
    }

    private void OnCardClicked()
    {
        if (_gameEntry == null || !_gameEntry.implemented) return;
        SceneLoader.LoadGame(_gameEntry.sceneName);
    }

    private void OnFavoriteClicked()
    {
        if (_gameEntry == null || FavoriteManager.Instance == null) return;
        FavoriteManager.Instance.ToggleFavorite(_gameEntry.id);
        UpdateFavoriteIcon();
    }

    private void UpdateFavoriteIcon()
    {
        if (_favoriteIconText == null || _gameEntry == null) return;
        bool isFav = FavoriteManager.Instance != null && FavoriteManager.Instance.IsFavorite(_gameEntry.id);
        _favoriteIconText.text = isFav ? "★" : "☆";
        _favoriteIconText.color = isFav ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
    }
}
