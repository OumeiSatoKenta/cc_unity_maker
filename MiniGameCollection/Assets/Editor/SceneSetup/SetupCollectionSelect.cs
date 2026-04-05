using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;

public static class SetupCollectionSelect
{
    [MenuItem("Assets/Setup/CollectionSelect")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[SetupCS] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // カメラ
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);

        // GameRegistry
        var registryObj = new GameObject("GameRegistry");
        registryObj.AddComponent<GameRegistry>();

        // FavoriteManager
        var favObj = new GameObject("FavoriteManager");
        favObj.AddComponent<FavoriteManager>();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f; // 縦横バランスよくスケール
        canvasObj.AddComponent<GraphicRaycaster>();

        // タイトル（上部に相対配置）
        var title = CT(canvasObj.transform, "Title", "ミニゲームコレクション", 48, jpFont,
            new Vector2(0.1f, 0.92f), new Vector2(0.9f, 0.97f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().color = Color.white;
        title.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        title.GetComponent<TextMeshProUGUI>().fontSizeMin = 24;
        title.GetComponent<TextMeshProUGUI>().fontSizeMax = 48;

        // サブタイトル
        var subtitle = CT(canvasObj.transform, "Subtitle", "コレクションを選んでください", 28, jpFont,
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.92f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        subtitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        subtitle.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.8f);
        subtitle.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        subtitle.GetComponent<TextMeshProUGUI>().fontSizeMin = 18;
        subtitle.GetComponent<TextMeshProUGUI>().fontSizeMax = 28;

        // カードコンテナ（VerticalLayoutGroupで均等配置）
        var cardArea = new GameObject("CardArea", typeof(RectTransform));
        cardArea.transform.SetParent(canvasObj.transform, false);
        var cardAreaRect = cardArea.GetComponent<RectTransform>();
        cardAreaRect.anchorMin = new Vector2(0.08f, 0.05f);
        cardAreaRect.anchorMax = new Vector2(0.92f, 0.85f);
        cardAreaRect.offsetMin = Vector2.zero;
        cardAreaRect.offsetMax = Vector2.zero;
        var vlg = cardArea.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(0, 0, 10, 10);

        // Classic ボタン（大きなカード）
        var classicBtn = CreateCollectionCard(cardArea.transform, "ClassicButton",
            "Classic", "オリジナル100+1本のミニゲーム",
            new Color(0.12f, 0.2f, 0.35f, 0.95f), jpFont);
        var classicCountText = classicBtn.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        // Remake ボタン
        var remakeBtn = CreateCollectionCard(cardArea.transform, "RemakeButton",
            "Remake", "進化版ミニゲーム（5ステージ・高品質）",
            new Color(0.25f, 0.12f, 0.35f, 0.95f), jpFont);
        var remakeCountText = remakeBtn.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        // お気に入りボタン
        var favBtn = CreateCollectionCard(cardArea.transform, "FavoriteButton",
            "★ お気に入り", "お気に入り登録したゲーム",
            new Color(0.35f, 0.25f, 0.1f, 0.95f), jpFont);
        var favCountText = favBtn.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        // CollectionSelectManager
        var mgrObj = new GameObject("CollectionSelectManager");
        var mgr = mgrObj.AddComponent<CollectionSelectManager>();
        var mgrSO = new SerializedObject(mgr);
        mgrSO.FindProperty("_classicButton").objectReferenceValue = classicBtn.GetComponent<Button>();
        mgrSO.FindProperty("_remakeButton").objectReferenceValue = remakeBtn.GetComponent<Button>();
        mgrSO.FindProperty("_favoriteButton").objectReferenceValue = favBtn.GetComponent<Button>();
        mgrSO.FindProperty("_classicCountText").objectReferenceValue = classicCountText;
        mgrSO.FindProperty("_remakeCountText").objectReferenceValue = remakeCountText;
        mgrSO.FindProperty("_favoriteCountText").objectReferenceValue = favCountText;
        mgrSO.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/CollectionSelect.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // Build Settingsの先頭に追加（起動シーン）
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) { scenes.Remove(s); break; }
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log("[SetupCS] CollectionSelect シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateCollectionCard(Transform parent, string name,
        string titleText, string descText, Color bgColor, TMP_FontAsset font)
    {
        var card = new GameObject(name, typeof(RectTransform));
        card.transform.SetParent(parent, false);
        var btn = card.AddComponent<Button>();
        var img = card.AddComponent<Image>();
        img.color = bgColor;

        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(bgColor.r - 0.05f, bgColor.g - 0.05f, bgColor.b - 0.05f, 1f);
        btn.colors = colors;

        // LayoutElementで高さを指定（幅は親のLayoutGroupが制御）
        var le = card.AddComponent<LayoutElement>();
        le.preferredHeight = 180;
        le.minHeight = 120;
        le.flexibleWidth = 1;

        // タイトル（上半分）
        var t = CT(card.transform, "TitleText", titleText, 38, font,
            new Vector2(0, 0.45f), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var tRect = t.GetComponent<RectTransform>();
        tRect.offsetMin = new Vector2(25, 5);
        tRect.offsetMax = new Vector2(-25, -12);
        t.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        t.GetComponent<TextMeshProUGUI>().color = Color.white;
        t.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        t.GetComponent<TextMeshProUGUI>().fontSizeMin = 22;
        t.GetComponent<TextMeshProUGUI>().fontSizeMax = 38;

        // 説明（下半分）
        var d = CT(card.transform, "DescText", descText, 22, font,
            new Vector2(0, 0), new Vector2(0.7f, 0.45f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var dRect = d.GetComponent<RectTransform>();
        dRect.offsetMin = new Vector2(25, 10);
        dRect.offsetMax = new Vector2(-10, -5);
        d.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        d.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.8f);
        d.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        d.GetComponent<TextMeshProUGUI>().fontSizeMin = 14;
        d.GetComponent<TextMeshProUGUI>().fontSizeMax = 22;

        // ゲーム数（右下）
        var c = CT(card.transform, "CountText", "", 24, font,
            new Vector2(0.7f, 0), new Vector2(1, 0.45f), new Vector2(1, 0),
            Vector2.zero, Vector2.zero);
        var cRect = c.GetComponent<RectTransform>();
        cRect.offsetMin = new Vector2(5, 10);
        cRect.offsetMax = new Vector2(-20, -5);
        c.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        c.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 1f);
        c.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
        c.GetComponent<TextMeshProUGUI>().fontSizeMin = 16;
        c.GetComponent<TextMeshProUGUI>().fontSizeMax = 24;

        return card;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }
}
