using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game033_AimSniper;

public static class Setup033_AimSniper
{
    [MenuItem("Assets/Setup/033 AimSniper")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup033] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        string sp = "Assets/Resources/Sprites/Game033_AimSniper/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.14f, 0.08f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite crosshairSprite = LoadSprite(sp + "crosshair.png");
        Sprite targetSprite = LoadSprite(sp + "target.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);
        if (bgSprite == null) bgSr.color = new Color(0.08f, 0.14f, 0.08f);

        // Scope（十字線）
        var scopeObj = new GameObject("Scope");
        scopeObj.transform.position = new Vector3(0f, 0f, 0f);
        scopeObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        var scopeSr = scopeObj.AddComponent<SpriteRenderer>();
        scopeSr.sprite = crosshairSprite;
        scopeSr.sortingOrder = 10;
        scopeSr.color = new Color(1f, 1f, 1f, 0.8f);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<AimSniperGameManager>();

        // SniperManager（GameManagerの子）
        var smObj = new GameObject("SniperManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<SniperManager>();

        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_gameManager").objectReferenceValue = gm;
        smSO.FindProperty("_scopeTransform").objectReferenceValue = scopeObj.transform;
        smSO.FindProperty("_crosshairSprite").objectReferenceValue = crosshairSprite;
        smSO.FindProperty("_targetSprite").objectReferenceValue = targetSprite;
        smSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 弾数テキスト（左上）
        var bulletsText = CT(canvasObj.transform, "BulletsText", "弾: 8", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200, 50), new Vector2(20, -20));
        bulletsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // 命中率テキスト（右上）
        var accuracyText = CT(canvasObj.transform, "AccuracyText", "命中率: 0%", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(280, 50), new Vector2(-20, -20));
        accuracyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0f, 0.25f, 0f, 0.9f));
        var clearTitle = CT(clearPanel.transform, "ClearTitle", "ミッション完了！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero);
        clearTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearStarText = CT(clearPanel.transform, "ClearStarText", "\u2605\u2605\u2605", 52, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 80), Vector2.zero);
        clearStarText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.1f, 0.5f, 0.1f, 1f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0f, 0.9f));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "弾切れ...", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // AimSniperUI（GameManagerの子）
        var uiObj = new GameObject("AimSniperUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<AimSniperUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_bulletsText").objectReferenceValue = bulletsText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_accuracyText").objectReferenceValue = accuracyText.GetComponent<TextMeshProUGUI>();
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
        gmSO.FindProperty("_totalTargets").intValue = 5;
        gmSO.FindProperty("_maxBullets").intValue = 8;
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
        string scenePath = "Assets/Scenes/033_AimSniper.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup033] AimSniper シーンを作成しました: " + scenePath);
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
