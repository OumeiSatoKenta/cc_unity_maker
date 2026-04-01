using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game035_WaveRider;

public static class Setup035_WaveRider
{
    [MenuItem("Assets/Setup/035 WaveRider")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup035] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game035_WaveRider/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.1f, 0.3f, 0.6f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite surferSprite = LoadSprite(sp + "surfer.png");
        Sprite rockSprite = LoadSprite(sp + "rock.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Surfer
        var surferObj = new GameObject("Surfer");
        surferObj.transform.position = new Vector3(0f, -1.5f, 0f);
        surferObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        var surferSr = surferObj.AddComponent<SpriteRenderer>();
        surferSr.sprite = surferSprite;
        surferSr.sortingOrder = 5;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<WaveRiderGameManager>();

        // WaveManager
        var wmObj = new GameObject("WaveManager");
        wmObj.transform.SetParent(gmObj.transform);
        var wm = wmObj.AddComponent<WaveManager>();

        var wmSO = new SerializedObject(wm);
        wmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        wmSO.FindProperty("_surferTransform").objectReferenceValue = surferObj.transform;
        wmSO.FindProperty("_rockSprite").objectReferenceValue = rockSprite;
        wmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250, 50), new Vector2(20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var distText = CT(canvasObj.transform, "DistanceText", "0m / 200m", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(280, 50), new Vector2(-20, -20));
        distText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Balance Slider
        var balSliderObj = CreateSlider(canvasObj.transform, "BalanceSlider",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(400, 30), new Vector2(0, 350));
        var balSlider = balSliderObj.GetComponent<Slider>();
        balSlider.minValue = 0f; balSlider.maxValue = 1f; balSlider.value = 0.5f;

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0f, 0.25f, 0.4f, 0.9f));
        CT(clearPanel.transform, "ClearTitle", "ゴール！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.1f, 0.4f, 0.6f, 1f));
        clearPanel.SetActive(false);

        // GameOver Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0f, 0.9f));
        CT(goPanel.transform, "GOTitle", "ワイプアウト！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GOScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // WaveRiderUI
        var uiObj = new GameObject("WaveRiderUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<WaveRiderUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_distanceText").objectReferenceValue = distText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_balanceSlider").objectReferenceValue = balSlider;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_manager").objectReferenceValue = wm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_goalDistance").floatValue = 200f;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/035_WaveRider.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup035] WaveRider シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f);
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

        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(obj.transform, false);
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);
        var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = new Vector2(0f, 0.25f); bgR.anchorMax = new Vector2(1f, 0.75f);
        bgR.offsetMin = bgR.offsetMax = Vector2.zero;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(obj.transform, false);
        var faR = fillArea.GetComponent<RectTransform>();
        faR.anchorMin = new Vector2(0f, 0.25f); faR.anchorMax = new Vector2(1f, 0.75f);
        faR.offsetMin = new Vector2(5, 0); faR.offsetMax = new Vector2(-15, 0);
        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f, 1f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero; fillR.anchorMax = new Vector2(0.5f, 1f);
        fillR.offsetMin = fillR.offsetMax = Vector2.zero;

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

        slider.fillRect = fillR; slider.handleRect = handleR;
        slider.targetGraphic = handleImg; slider.interactable = false;
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
        o.AddComponent<Image>().color = bg; o.AddComponent<Button>();
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
