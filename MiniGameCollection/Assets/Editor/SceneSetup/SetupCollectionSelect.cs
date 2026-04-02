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
        canvasObj.AddComponent<GraphicRaycaster>();

        // タイトル
        var title = CT(canvasObj.transform, "Title", "ミニゲームコレクション", 48, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(600, 70), new Vector2(0, -60));
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().color = Color.white;

        // サブタイトル
        var subtitle = CT(canvasObj.transform, "Subtitle", "コレクションを選んでください", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(500, 40), new Vector2(0, -130));
        subtitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        subtitle.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.8f);

        // Classic ボタン（大きなカード）
        var classicBtn = CreateCollectionCard(canvasObj.transform, "ClassicButton",
            "Classic", "オリジナル100+1本のミニゲーム",
            new Vector2(0.5f, 0.65f), new Color(0.12f, 0.2f, 0.35f, 0.95f), jpFont);
        var classicCountText = classicBtn.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        // Remake ボタン
        var remakeBtn = CreateCollectionCard(canvasObj.transform, "RemakeButton",
            "Remake", "進化版ミニゲーム（5ステージ・高品質）",
            new Vector2(0.5f, 0.42f), new Color(0.25f, 0.12f, 0.35f, 0.95f), jpFont);
        var remakeCountText = remakeBtn.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        // お気に入りボタン
        var favBtn = CreateCollectionCard(canvasObj.transform, "FavoriteButton",
            "★ お気に入り", "お気に入り登録したゲーム",
            new Vector2(0.5f, 0.19f), new Color(0.35f, 0.25f, 0.1f, 0.95f), jpFont);
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
        string titleText, string descText, Vector2 anchorPos, Color bgColor, TMP_FontAsset font)
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

        var rect = card.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchorPos;
        rect.sizeDelta = new Vector2(700, 180);
        rect.anchoredPosition = Vector2.zero;

        // タイトル
        var t = CT(card.transform, "TitleText", titleText, 38, font,
            new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var tRect = t.GetComponent<RectTransform>();
        tRect.offsetMin = new Vector2(30, 50);
        tRect.offsetMax = new Vector2(-30, -15);
        t.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        t.GetComponent<TextMeshProUGUI>().color = Color.white;

        // 説明
        var d = CT(card.transform, "DescText", descText, 22, font,
            new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var dRect = d.GetComponent<RectTransform>();
        dRect.offsetMin = new Vector2(30, 40);
        dRect.offsetMax = new Vector2(-30, -5);
        d.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        d.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.8f);

        // ゲーム数
        var c = CT(card.transform, "CountText", "", 24, font,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(200, 30), new Vector2(-20, 15));
        c.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        c.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 1f);

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
