using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game034_DropZone;

public static class Setup034_DropZone
{
    [MenuItem("Assets/Setup/034 DropZone")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup034] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game034_DropZone/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.24f, 0.2f, 0.16f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite apple = LoadSprite(sp + "apple.png");
        Sprite banana = LoadSprite(sp + "banana.png");
        Sprite paper = LoadSprite(sp + "paper.png");
        Sprite can = LoadSprite(sp + "can.png");
        Sprite bottle = LoadSprite(sp + "bottle.png");
        Sprite glass = LoadSprite(sp + "glass.png");
        Sprite zoneGreen = LoadSprite(sp + "zone_green.png");
        Sprite zoneGray = LoadSprite(sp + "zone_gray.png");
        Sprite zoneBlue = LoadSprite(sp + "zone_blue.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DropZoneGameManager>();

        // DropManager
        var dmObj = new GameObject("DropManager");
        dmObj.transform.SetParent(gmObj.transform);
        var dm = dmObj.AddComponent<DropManager>();

        var dmSO = new SerializedObject(dm);
        dmSO.FindProperty("_gameManager").objectReferenceValue = gm;

        var itemSpritesProp = dmSO.FindProperty("_itemSprites");
        itemSpritesProp.arraySize = 6;
        itemSpritesProp.GetArrayElementAtIndex(0).objectReferenceValue = apple;
        itemSpritesProp.GetArrayElementAtIndex(1).objectReferenceValue = banana;
        itemSpritesProp.GetArrayElementAtIndex(2).objectReferenceValue = paper;
        itemSpritesProp.GetArrayElementAtIndex(3).objectReferenceValue = can;
        itemSpritesProp.GetArrayElementAtIndex(4).objectReferenceValue = bottle;
        itemSpritesProp.GetArrayElementAtIndex(5).objectReferenceValue = glass;

        var zoneSpritesProp = dmSO.FindProperty("_zoneSprites");
        zoneSpritesProp.arraySize = 3;
        zoneSpritesProp.GetArrayElementAtIndex(0).objectReferenceValue = zoneGreen;
        zoneSpritesProp.GetArrayElementAtIndex(1).objectReferenceValue = zoneGray;
        zoneSpritesProp.GetArrayElementAtIndex(2).objectReferenceValue = zoneBlue;
        dmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // スコアテキスト
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250, 50), new Vector2(20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // ミス数テキスト
        var missesText = CT(canvasObj.transform, "MissesText", "Miss: 0/3", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220, 50), new Vector2(-20, -20));
        missesText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // 残りテキスト
        var remainingText = CT(canvasObj.transform, "RemainingText", "残り: 20", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(200, 50), new Vector2(0, -20));
        remainingText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // メニューボタン
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // ゾーンラベル（テキスト）
        string[] zoneLabels = { "フルーツ", "ゴミ", "リサイクル" };
        float[] zoneXs = { -2.5f, 0f, 2.5f };
        for (int i = 0; i < 3; i++)
        {
            // ワールド空間に直接ラベル配置（Canvas外）
            // SceneSetup時のみ — 実行時はDropManagerがゾーンを生成する
        }

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0f, 0.25f, 0f, 0.9f));
        CT(clearPanel.transform, "ClearTitle", "全仕分け完了！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.1f, 0.5f, 0.1f, 1f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0f, 0.9f));
        CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GameOverScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // DropZoneUI
        var uiObj = new GameObject("DropZoneUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DropZoneUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missesText").objectReferenceValue = missesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_remainingText").objectReferenceValue = remainingText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_manager").objectReferenceValue = dm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalItems").intValue = 20;
        gmSO.FindProperty("_maxMisses").intValue = 3;
        gmSO.ApplyModifiedProperties();

        // リトライボタン
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/034_DropZone.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup034] DropZone シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.1f, 0.3f);
        r.anchorMax = new Vector2(0.9f, 0.7f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
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

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg;
        o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return o;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
