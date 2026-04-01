using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game032_SpinCutter;

public static class Setup032_SpinCutter
{
    [MenuItem("Assets/Setup/032 SpinCutter")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup032] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        string sp = "Assets/Resources/Sprites/Game032_SpinCutter/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.02f, 0.12f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite bladeSprite = LoadSprite(sp + "blade.png");
        Sprite enemySprite = LoadSprite(sp + "enemy.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);
        if (bgSprite == null) bgSr.color = new Color(0.05f, 0.02f, 0.12f);

        // Pivot（刃の回転中心）
        var pivotObj = new GameObject("Pivot");
        pivotObj.transform.position = new Vector3(0f, 0.5f, 0f);

        // OrbitPreview（軌道プレビュー）
        var previewObj = new GameObject("OrbitPreview");
        var lr = previewObj.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.startColor = new Color(0.5f, 0.8f, 1.0f, 0.5f);
        lr.endColor = new Color(0.5f, 0.8f, 1.0f, 0.5f);
        lr.useWorldSpace = true;
        lr.positionCount = 65;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SpinCutterGameManager>();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 発射残数テキスト（左上）
        var launchesText = CT(canvasObj.transform, "LaunchesText", "発射: 3", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200, 50), new Vector2(20, -20));
        launchesText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // 撃破数テキスト（右上）
        var killsText = CT(canvasObj.transform, "KillsText", "撃破: 0/8", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220, 50), new Vector2(-20, -20));
        killsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // 半径スライダーラベル
        CT(canvasObj.transform, "RadiusLabel", "半径", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120, 40), new Vector2(-200, 320));

        // 半径スライダー
        var radiusSliderObj = CreateSlider(canvasObj.transform, "RadiusSlider",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350, 50), new Vector2(-80, 270));
        var radiusSlider = radiusSliderObj.GetComponent<Slider>();
        radiusSlider.minValue = 0.5f;
        radiusSlider.maxValue = 4.0f;
        radiusSlider.value = 2.0f;

        // 速度スライダーラベル
        CT(canvasObj.transform, "SpeedLabel", "速度", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120, 40), new Vector2(-200, 210));

        // 速度スライダー
        var speedSliderObj = CreateSlider(canvasObj.transform, "SpeedSlider",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350, 50), new Vector2(-80, 160));
        var speedSlider = speedSliderObj.GetComponent<Slider>();
        speedSlider.minValue = 1.0f;
        speedSlider.maxValue = 6.0f;
        speedSlider.value = 3.0f;

        // 発射ボタン
        var launchBtn = CB(canvasObj.transform, "LaunchButton", "発射！", 36, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280, 70), new Vector2(0, 80),
            new Color(0.8f, 0.2f, 0.1f, 1f));

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0f, 0.25f, 0f, 0.9f));
        var clearTitle = CT(clearPanel.transform, "ClearTitle", "クリア！", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(500, 100), Vector2.zero);
        clearTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearStarText = CT(clearPanel.transform, "ClearStarText", "★★★", 52, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 80), Vector2.zero);
        clearStarText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.1f, 0.5f, 0.1f, 1f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0f, 0.9f));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // SpinCutterManager（GameManagerの子）
        var smObj = new GameObject("SpinCutterManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<SpinCutterManager>();

        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_gameManager").objectReferenceValue = gm;
        smSO.FindProperty("_pivot").objectReferenceValue = pivotObj.transform;
        smSO.FindProperty("_radiusSlider").objectReferenceValue = radiusSlider;
        smSO.FindProperty("_speedSlider").objectReferenceValue = speedSlider;
        smSO.FindProperty("_launchButton").objectReferenceValue = launchBtn.GetComponent<Button>();
        smSO.FindProperty("_orbitPreview").objectReferenceValue = lr;
        smSO.FindProperty("_bladeSprite").objectReferenceValue = bladeSprite;
        smSO.FindProperty("_enemySprite").objectReferenceValue = enemySprite;
        smSO.ApplyModifiedProperties();

        // SpinCutterUI（GameManagerの子）
        var uiObj = new GameObject("SpinCutterUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<SpinCutterUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_launchesText").objectReferenceValue = launchesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_killsText").objectReferenceValue = killsText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearStarText").objectReferenceValue = clearStarText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_manager").objectReferenceValue = sm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalEnemies").intValue = 8;
        gmSO.FindProperty("_maxLaunches").intValue = 3;
        gmSO.ApplyModifiedProperties();

        // リトライボタン配線
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

        // Save
        string scenePath = "Assets/Scenes/032_SpinCutter.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup032] SpinCutter シーンを作成しました: " + scenePath);
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

    private static GameObject CreateSlider(Transform parent, string name,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var slider = obj.AddComponent<Slider>();
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;

        // Background
        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(obj.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = new Vector2(0f, 0.25f); bgR.anchorMax = new Vector2(1f, 0.75f);
        bgR.offsetMin = bgR.offsetMax = Vector2.zero;

        // Fill Area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(obj.transform, false);
        var faR = fillArea.GetComponent<RectTransform>();
        faR.anchorMin = new Vector2(0f, 0.25f); faR.anchorMax = new Vector2(1f, 0.75f);
        faR.offsetMin = new Vector2(5, 0); faR.offsetMax = new Vector2(-15, 0);

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.6f, 1f, 1f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero; fillR.anchorMax = new Vector2(0.5f, 1f);
        fillR.offsetMin = fillR.offsetMax = Vector2.zero;

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(obj.transform, false);
        var haR = handleArea.GetComponent<RectTransform>();
        haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one;
        haR.offsetMin = new Vector2(10, 0); haR.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.9f, 0.9f, 1f, 1f);
        var handleR = handle.GetComponent<RectTransform>();
        handleR.sizeDelta = new Vector2(20, 0);

        slider.fillRect = fillR;
        slider.handleRect = handleR;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

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
