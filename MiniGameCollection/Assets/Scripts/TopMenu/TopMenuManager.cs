using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TopMenuのメイン制御。コレクションタブ・カテゴリタブの切り替えとゲームカードの生成を管理する。
/// GameRegistry からゲーム一覧を取得し、選択されたコレクション×カテゴリのカードを表示する。
/// </summary>
public class TopMenuManager : MonoBehaviour
{
    [SerializeField, Tooltip("ゲームカードを並べるコンテナ（GridLayoutGroup付き）")]
    private Transform _cardContainer;

    [SerializeField, Tooltip("カテゴリタブボタンを並べるコンテナ")]
    private Transform _tabContainer;

    [SerializeField, Tooltip("コレクションタブボタンを並べるコンテナ")]
    private Transform _collectionTabContainer;

    [SerializeField, Tooltip("ゲームカードのプレハブ")]
    private GameObject _cardPrefab;

    [SerializeField, Tooltip("日本語フォントアセット（未設定ならデフォルト使用）")]
    private TMP_FontAsset _japaneseFont;

    private string _currentCategory = "puzzle";
    private string _currentCollection = "classic";
    private readonly List<GameObject> _currentCards = new List<GameObject>();

    private const string CategoryPrefsKey = "last_selected_category";
    private const string CollectionPrefsKey = "last_selected_collection";

    private static readonly (string id, string label)[] Categories =
    {
        ("favorite", "★お気に入り"),
        ("puzzle", "パズル"),
        ("action", "アクション"),
        ("casual", "カジュアル"),
        ("idle", "放置"),
        ("rhythm", "リズム"),
        ("simulation", "育成"),
        ("unique", "ユニーク")
    };

    private static readonly (string id, string label)[] Collections =
    {
        ("classic", "Classic"),
        ("remake", "Remake"),
        ("favorite", "★お気に入り"),
    };

    private void Start()
    {
        _currentCollection = PlayerPrefs.GetString(CollectionPrefsKey, SceneLoader.CurrentCollection ?? "classic");
        _currentCategory = PlayerPrefs.GetString(CategoryPrefsKey, "puzzle");
        CreateCollectionTabs();
        CreateTabs();
        ShowCategory(_currentCategory);
    }

    private void CreateCollectionTabs()
    {
        if (_collectionTabContainer == null) return;

        foreach (var (id, label) in Collections)
        {
            var tabObj = new GameObject($"CollectionTab_{id}", typeof(RectTransform));
            tabObj.transform.SetParent(_collectionTabContainer, false);

            var button = tabObj.AddComponent<Button>();
            var image = tabObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

            var rect = tabObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 44);

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

            string collectionId = id;
            button.onClick.AddListener(() => SwitchCollection(collectionId));
        }

        UpdateCollectionTabHighlight();
    }

    private void SwitchCollection(string collection)
    {
        _currentCollection = collection;
        SceneLoader.CurrentCollection = collection;
        PlayerPrefs.SetString(CollectionPrefsKey, collection);
        PlayerPrefs.Save();
        UpdateCollectionTabHighlight();
        ShowCategory(_currentCategory);
    }

    private void UpdateCollectionTabHighlight()
    {
        if (_collectionTabContainer == null) return;

        for (int i = 0; i < _collectionTabContainer.childCount && i < Collections.Length; i++)
        {
            var tabImage = _collectionTabContainer.GetChild(i).GetComponent<Image>();
            if (tabImage == null) continue;

            bool isSelected = Collections[i].id == _currentCollection;
            tabImage.color = isSelected
                ? new Color(0.8f, 0.5f, 0.1f, 1f)
                : new Color(0.15f, 0.15f, 0.25f, 0.9f);
        }
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
            rect.sizeDelta = new Vector2(140, 44);

            // ラベル
            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(tabObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 22;
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
        PlayerPrefs.SetString(CategoryPrefsKey, category);
        PlayerPrefs.Save();
        ClearCards();
        UpdateTabHighlight();

        if (GameRegistry.Instance == null)
        {
            Debug.LogError("[TopMenuManager] GameRegistry が初期化されていません");
            return;
        }

        string collection = _currentCollection;

        List<GameEntry> games;
        if (category == "favorite" || collection == "favorite")
        {
            games = new List<GameEntry>();
            if (FavoriteManager.Instance != null)
            {
                var favoriteIds = FavoriteManager.Instance.GetFavoriteIds();
                foreach (string favId in favoriteIds)
                {
                    var game = GameRegistry.Instance.GetGameById(favId);
                    if (game == null) continue;
                    if (category == "favorite" || game.category == category)
                        games.Add(game);
                }
            }
        }
        else
        {
            games = GameRegistry.Instance.GetGamesByCategoryAndCollection(category, collection);
        }

        Debug.Log($"[TopMenuManager] collection={collection} category={category} → {games.Count} 件");
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
