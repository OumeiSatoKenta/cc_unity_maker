using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TopMenuのメイン制御。カテゴリタブの切り替えとゲームカードの生成を管理する。
/// GameRegistry からゲーム一覧を取得し、選択されたカテゴリのカードを表示する。
/// </summary>
public class TopMenuManager : MonoBehaviour
{
    [SerializeField, Tooltip("ゲームカードを並べるコンテナ（GridLayoutGroup付き）")]
    private Transform _cardContainer;

    [SerializeField, Tooltip("カテゴリタブボタンを並べるコンテナ")]
    private Transform _tabContainer;

    [SerializeField, Tooltip("ゲームカードのプレハブ")]
    private GameObject _cardPrefab;

    [SerializeField, Tooltip("日本語フォントアセット（未設定ならデフォルト使用）")]
    private TMP_FontAsset _japaneseFont;

    private string _currentCategory = "puzzle";
    private readonly List<GameObject> _currentCards = new List<GameObject>();

    private static readonly (string id, string label)[] Categories =
    {
        ("puzzle", "パズル"),
        ("action", "アクション"),
        ("casual", "カジュアル"),
        ("idle", "放置"),
        ("rhythm", "リズム"),
        ("simulation", "育成"),
        ("unique", "ユニーク")
    };

    private void Start()
    {
        CreateTabs();
        ShowCategory(_currentCategory);
    }

    private void CreateTabs()
    {
        foreach (var (id, label) in Categories)
        {
            var tabObj = new GameObject($"Tab_{id}", typeof(RectTransform));
            tabObj.transform.SetParent(_tabContainer, false);

            var button = tabObj.AddComponent<Button>();
            var image = tabObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // ボタンサイズ
            var rect = tabObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 50);

            // ラベル
            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(tabObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 26;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            if (_japaneseFont != null) text.font = _japaneseFont;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            string categoryId = id;
            button.onClick.AddListener(() => ShowCategory(categoryId));
        }
    }

    /// <summary>
    /// 指定カテゴリのゲームカードを表示する。既存カードはクリアされる。
    /// </summary>
    public void ShowCategory(string category)
    {
        _currentCategory = category;
        ClearCards();
        UpdateTabHighlight();

        if (GameRegistry.Instance == null)
        {
            Debug.LogError("[TopMenuManager] GameRegistry が初期化されていません");
            return;
        }

        var games = GameRegistry.Instance.GetGamesByCategory(category);
        foreach (var game in games)
        {
            CreateCard(game);
        }
    }

    private void CreateCard(GameEntry game)
    {
        if (_cardPrefab == null)
        {
            Debug.LogError("[TopMenuManager] cardPrefab が設定されていません");
            return;
        }

        var cardObj = Instantiate(_cardPrefab, _cardContainer);
        cardObj.SetActive(true);
        var card = cardObj.GetComponent<GameCardUI>();
        if (card != null)
        {
            card.Setup(game);
        }
        _currentCards.Add(cardObj);
    }

    private void ClearCards()
    {
        foreach (var card in _currentCards)
        {
            if (card != null) Destroy(card);
        }
        _currentCards.Clear();
    }

    private void UpdateTabHighlight()
    {
        if (_tabContainer == null) return;

        for (int i = 0; i < _tabContainer.childCount && i < Categories.Length; i++)
        {
            var tabImage = _tabContainer.GetChild(i).GetComponent<Image>();
            if (tabImage == null) continue;

            bool isSelected = Categories[i].id == _currentCategory;
            tabImage.color = isSelected
                ? new Color(0.1f, 0.5f, 0.9f, 1f)
                : new Color(0.2f, 0.2f, 0.2f, 0.9f);
        }
    }
}
