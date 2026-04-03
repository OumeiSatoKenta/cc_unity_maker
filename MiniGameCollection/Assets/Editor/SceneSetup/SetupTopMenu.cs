using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// TopMenuシーンを自動構成するEditorスクリプト。
/// Assets > Setup > TopMenu から実行する。
/// </summary>
public static class SetupTopMenu
{
    [MenuItem("Assets/Setup/TopMenu")]
    public static void CreateTopMenuScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[SetupTopMenu] Play モード中は実行できません。停止してから再度実行してください。");
            return;
        }

        // 新規シーンを作成
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- GameRegistry オブジェクト ---
        var registryObj = new GameObject("GameRegistry");
        registryObj.AddComponent<GameRegistry>();

        // --- FavoriteManager オブジェクト ---
        var favoriteObj = new GameObject("FavoriteManager");
        favoriteObj.AddComponent<FavoriteManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- コレクション選択に戻るボタン ---
        var backBtn = new GameObject("BackToCollectionButton", typeof(RectTransform));
        backBtn.transform.SetParent(canvasObj.transform, false);
        backBtn.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        backBtn.AddComponent<Button>();
        backBtn.AddComponent<BackToCollectionButton>();
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1);
        backRect.anchorMax = new Vector2(0, 1);
        backRect.pivot = new Vector2(0, 1);
        backRect.sizeDelta = new Vector2(50, 50);
        backRect.anchoredPosition = new Vector2(10, -10);
        var backText = CreateText(backBtn.transform, "Text", "←", 32,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var btRect = backText.GetComponent<RectTransform>();
        btRect.offsetMin = Vector2.zero; btRect.offsetMax = Vector2.zero;
        backText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // --- タイトル（アンカーストレッチ）---
        var titleObj = CreateText(canvasObj.transform, "Title", "ミニゲーム集", 48,
            new Vector2(0.1f, 0.96f), new Vector2(0.9f, 1f), new Vector2(0.5f, 1f),
            Vector2.zero, Vector2.zero);
        var titleRect2 = titleObj.GetComponent<RectTransform>();
        titleRect2.offsetMin = new Vector2(0, -60);
        titleRect2.offsetMax = new Vector2(0, -10);
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        titleObj.GetComponent<TextMeshProUGUI>().fontSizeMin = 28;
        titleObj.GetComponent<TextMeshProUGUI>().fontSizeMax = 48;

        // --- コレクションタブコンテナ（Classic / Remake / お気に入り）---
        var collectionTabContainerObj = new GameObject("CollectionTabContainer", typeof(RectTransform));
        collectionTabContainerObj.transform.SetParent(canvasObj.transform, false);
        var collectionTabRect = collectionTabContainerObj.GetComponent<RectTransform>();
        collectionTabRect.anchorMin = new Vector2(0, 1);
        collectionTabRect.anchorMax = new Vector2(1, 1);
        collectionTabRect.pivot = new Vector2(0.5f, 1f);
        collectionTabRect.anchoredPosition = new Vector2(0, -75);
        collectionTabRect.sizeDelta = new Vector2(-20, 55);
        var collectionTabLayout = collectionTabContainerObj.AddComponent<HorizontalLayoutGroup>();
        collectionTabLayout.spacing = 12;
        collectionTabLayout.childAlignment = TextAnchor.MiddleCenter;
        collectionTabLayout.childForceExpandWidth = false;
        collectionTabLayout.childForceExpandHeight = true;
        collectionTabLayout.padding = new RectOffset(10, 10, 4, 4);

        // --- カテゴリタブコンテナ（横スクロール対応）---
        var tabScrollObj = new GameObject("TabScroll", typeof(RectTransform));
        tabScrollObj.transform.SetParent(canvasObj.transform, false);
        var tabScrollRect = tabScrollObj.GetComponent<RectTransform>();
        tabScrollRect.anchorMin = new Vector2(0, 1);
        tabScrollRect.anchorMax = new Vector2(1, 1);
        tabScrollRect.pivot = new Vector2(0.5f, 1f);
        tabScrollRect.anchoredPosition = new Vector2(0, -138);
        tabScrollRect.sizeDelta = new Vector2(-20, 55);

        var tabScroll = tabScrollObj.AddComponent<ScrollRect>();
        tabScroll.horizontal = true;
        tabScroll.vertical = false;
        tabScroll.movementType = ScrollRect.MovementType.Elastic;
        tabScrollObj.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        tabScrollObj.AddComponent<Mask>().showMaskGraphic = false;

        var tabContainerObj = new GameObject("TabContainer", typeof(RectTransform));
        tabContainerObj.transform.SetParent(tabScrollObj.transform, false);
        var tabRect = tabContainerObj.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 0);
        tabRect.anchorMax = new Vector2(0, 1);
        tabRect.pivot = new Vector2(0, 0.5f);
        tabRect.anchoredPosition = Vector2.zero;

        var tabLayout = tabContainerObj.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 8;
        tabLayout.childAlignment = TextAnchor.MiddleCenter;
        tabLayout.childForceExpandWidth = false;
        tabLayout.childForceExpandHeight = true;
        tabLayout.padding = new RectOffset(10, 10, 4, 4);

        var tabFitter = tabContainerObj.AddComponent<ContentSizeFitter>();
        tabFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        tabScroll.content = tabRect;
        tabScroll.viewport = tabScrollObj.GetComponent<RectTransform>();

        // --- スクロールエリア ---
        var scrollObj = new GameObject("ScrollView", typeof(RectTransform));
        scrollObj.transform.SetParent(canvasObj.transform, false);
        var scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 20);
        scrollRect.offsetMax = new Vector2(-20, -200);

        var scrollView = scrollObj.AddComponent<ScrollRect>();
        scrollView.horizontal = false;
        scrollView.vertical = true;

        var scrollImage = scrollObj.AddComponent<Image>();
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        scrollObj.AddComponent<Mask>().showMaskGraphic = true;

        // Viewport
        var viewportObj = new GameObject("Viewport", typeof(RectTransform));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        var viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        // Content（カードコンテナ）
        var contentObj = new GameObject("CardContainer", typeof(RectTransform));
        contentObj.transform.SetParent(viewportObj.transform, false);
        var contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        var gridLayout = contentObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(260, 150);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

        var contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollView.content = contentRect;
        scrollView.viewport = viewportRect;

        // --- カードプレハブ（シーン内プレハブとして作成）---
        var cardPrefab = CreateCardPrefab();

        // --- TopMenuManager ---
        var managerObj = new GameObject("TopMenuManager");
        var manager = managerObj.AddComponent<TopMenuManager>();

        // SerializeField をリフレクションで設定
        var managerSO = new SerializedObject(manager);
        managerSO.FindProperty("_cardContainer").objectReferenceValue = contentObj.transform;
        managerSO.FindProperty("_tabContainer").objectReferenceValue = tabContainerObj.transform;
        managerSO.FindProperty("_collectionTabContainer").objectReferenceValue = collectionTabContainerObj.transform;
        var jpFont = LoadJapaneseFont();
        if (jpFont != null)
        {
            managerSO.FindProperty("_japaneseFont").objectReferenceValue = jpFont;
        }
        managerSO.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
        managerSO.ApplyModifiedProperties();

        // --- EventSystem ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // --- カメラ背景色を暗めに設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        }

        // カードプレハブを非アクティブにする（テンプレートとして保持）
        cardPrefab.SetActive(false);

        // シーンを保存
        string scenePath = "Assets/Scenes/TopMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // Build Settings にシーンを追加
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[SetupTopMenu] TopMenuシーンを作成しました: " + scenePath);
    }

    private static GameObject CreateCardPrefab()
    {
        var cardObj = new GameObject("CardPrefab", typeof(RectTransform));
        var button = cardObj.AddComponent<Button>();
        var bg = cardObj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // ボタンカラー設定
        var colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        colors.highlightedColor = new Color(0.2f, 0.3f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.4f, 1f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        button.colors = colors;

        // ID テキスト（左上）
        var idObj = CreateText(cardObj.transform, "IdText", "001", 20,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(60, 30), new Vector2(10, -8));
        idObj.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.7f, 1f, 1f);

        // タイトルテキスト（中央）
        var titleObj = CreateText(cardObj.transform, "TitleText", "GameTitle", 28,
            new Vector2(0, 0.3f), new Vector2(1, 0.8f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.offsetMin = new Vector2(8, 0);
        titleRect.offsetMax = new Vector2(-8, 0);
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 工数テキスト（右下）
        var sizeObj = CreateText(cardObj.transform, "SizeText", "S", 20,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(50, 30), new Vector2(-10, 8));
        sizeObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        sizeObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.4f, 1f);

        // お気に入りボタン（右上）
        var favBtnObj = new GameObject("FavoriteButton", typeof(RectTransform));
        favBtnObj.transform.SetParent(cardObj.transform, false);
        var favBtn = favBtnObj.AddComponent<Button>();
        var favBtnImg = favBtnObj.AddComponent<Image>();
        favBtnImg.color = new Color(0, 0, 0, 0); // 透明背景
        var favBtnRect = favBtnObj.GetComponent<RectTransform>();
        favBtnRect.anchorMin = new Vector2(1, 1);
        favBtnRect.anchorMax = new Vector2(1, 1);
        favBtnRect.pivot = new Vector2(1, 1);
        favBtnRect.sizeDelta = new Vector2(44, 44);
        favBtnRect.anchoredPosition = new Vector2(-4, -4);

        var favIconObj = CreateText(favBtnObj.transform, "FavIcon", "☆", 28,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var favIconRect = favIconObj.GetComponent<RectTransform>();
        favIconRect.offsetMin = Vector2.zero;
        favIconRect.offsetMax = Vector2.zero;
        favIconObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        favIconObj.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f);

        // GameCardUI コンポーネント
        var card = cardObj.AddComponent<GameCardUI>();
        var cardSO = new SerializedObject(card);
        cardSO.FindProperty("_idText").objectReferenceValue = idObj.GetComponent<TextMeshProUGUI>();
        cardSO.FindProperty("_titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
        cardSO.FindProperty("_sizeText").objectReferenceValue = sizeObj.GetComponent<TextMeshProUGUI>();
        cardSO.FindProperty("_button").objectReferenceValue = button;
        cardSO.FindProperty("_background").objectReferenceValue = bg;
        cardSO.FindProperty("_favoriteButton").objectReferenceValue = favBtn;
        cardSO.FindProperty("_favoriteIconText").objectReferenceValue = favIconObj.GetComponent<TextMeshProUGUI>();
        cardSO.ApplyModifiedProperties();

        return cardObj;
    }

    private static TMP_FontAsset LoadJapaneseFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        if (font == null)
        {
            Debug.LogWarning("[SetupTopMenu] 日本語フォントが見つかりません。先に Assets > Setup > Generate Japanese Font を実行してください。デフォルトフォントを使用します。");
        }
        return font;
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;

        var jpFont = LoadJapaneseFont();
        if (jpFont != null) tmp.font = jpFont;

        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        return obj;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // 既に追加済みなら何もしない
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }

        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[SetupTopMenu] Build Settings にTopMenuシーンを追加しました");
    }
}
