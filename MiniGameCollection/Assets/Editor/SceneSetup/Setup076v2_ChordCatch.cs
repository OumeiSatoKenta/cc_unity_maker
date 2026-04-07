using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game076v2_ChordCatch;

public static class Setup076v2_ChordCatch
{
    [MenuItem("Assets/Setup/076v2 ChordCatch")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup076v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game076v2_ChordCatch/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0f, 0.12f, 0.24f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.023f, 0.023f, 1f);
        }

        // Load sprites
        Sprite beatNormal = LoadSprite(sp + "beat_indicator.png");
        Sprite beatActive = LoadSprite(sp + "beat_indicator_active.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("ChordCatchGameManager");
        var gm = gmObj.AddComponent<ChordCatchGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.33f, countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.67f, countMultiplier = 1, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 2.0f,  countMultiplier = 1, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.33f, countMultiplier = 1, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // ChordController
        var ccObj = new GameObject("ChordController");
        ccObj.transform.SetParent(gmObj.transform);
        var cc = ccObj.AddComponent<ChordController>();
        SetField(cc, "_gameManager", gm);

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HUD top bar ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 55), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.95f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score\n0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(280, 90), new Vector2(-15, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);

        var progressText = CT(canvasObj.transform, "ProgressText", "0/8", 42, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(120, 55), new Vector2(15, -30));
        progressText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        progressText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var missText = CT(canvasObj.transform, "MissText", "Miss 0/5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(220, 55), new Vector2(15, -90));
        missText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        missText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        // Beat indicator (metronome visual, near top center)
        var beatObj = new GameObject("BeatIndicator", typeof(RectTransform));
        beatObj.transform.SetParent(canvasObj.transform, false);
        var beatRT = beatObj.GetComponent<RectTransform>();
        beatRT.anchorMin = new Vector2(0.5f, 1f);
        beatRT.anchorMax = new Vector2(0.5f, 1f);
        beatRT.pivot = new Vector2(0.5f, 1f);
        beatRT.sizeDelta = new Vector2(60, 60);
        beatRT.anchoredPosition = new Vector2(0, -95);
        var beatImg = beatObj.AddComponent<Image>();
        if (beatNormal != null) beatImg.sprite = beatNormal;
        else beatImg.color = new Color(0f, 0.74f, 0.83f);

        // Combo text (center)
        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 80), new Vector2(0, 350));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(0.88f, 0.25f, 0.98f);
        comboText.gameObject.SetActive(false);

        // Judgement text (center)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 100), new Vector2(0, 220));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.gameObject.SetActive(false);

        // --- Chord button container (lower half) ---
        var btnContainerObj = new GameObject("ChordButtonContainer", typeof(RectTransform));
        btnContainerObj.transform.SetParent(canvasObj.transform, false);
        var btnContainerRT = btnContainerObj.GetComponent<RectTransform>();
        btnContainerRT.anchorMin = new Vector2(0.5f, 0f);
        btnContainerRT.anchorMax = new Vector2(0.5f, 0f);
        btnContainerRT.pivot = new Vector2(0.5f, 0f);
        btnContainerRT.sizeDelta = new Vector2(1040, 700);
        btnContainerRT.anchoredPosition = new Vector2(0, 80);

        // Wire button container to ChordController
        SetField(cc, "_buttonContainer", btnContainerObj.transform);

        // Replay button
        var replayObj = CB(canvasObj.transform, "ReplayButton", "▶ Replay", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(-160, 810), new Color(0.35f, 0.1f, 0.5f, 0.9f));

        var replayCountText = CT(replayObj.transform, "CountText", "∞", 32, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(50, 40), new Vector2(-2, -2));
        replayCountText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);
        replayCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Back button (bottom)
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ戻る", 36, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 55), new Vector2(0, 15), new Color(0.1f, 0.15f, 0.2f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 380);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.15f, 0.3f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.9f, 1f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 50), new Color(0.1f, 0.3f, 0.5f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 450);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.02f, 0.1f, 0.25f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "全ステージクリア！", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.9f, 1f);

        var acScore = CT(acPanel.transform, "ACScore", "Total Score\n0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 100), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.1f, 0.2f, 0.4f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 400);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.15f, 0.03f, 0.05f, 0.95f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScore = CT(goPanel.transform, "GOScore", "Score\n0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 100), new Vector2(0, 30));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.4f, 0.1f, 0.1f));
        goBack.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === InstructionPanel ===
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0f, 0.05f, 0.12f, 0.93f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "ChordCatch", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.88f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 42, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 36, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 120), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 36, jpFont,
            new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0f, 0.3f, 0.5f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 80), new Color(0.1f, 0.15f, 0.25f, 0.9f));

        // === ChordCatchUI ===
        var uiObj = new GameObject("ChordCatchUI");
        var ui = uiObj.AddComponent<ChordCatchUI>();

        SetField(ui, "_stageText",         stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_missText",          missText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_progressText",      progressText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",     judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_beatIndicator",     beatImg);
        SetField(ui, "_beatNormal",        beatNormal);
        SetField(ui, "_beatActive",        beatActive);
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearTitle",   scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScore.GetComponent<TextMeshProUGUI>());

        // Wire ChordController replay button
        cc.SetReplayButton(replayObj.GetComponent<Button>(), replayCountText.GetComponent<TextMeshProUGUI>());

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_chordController",  cc);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire NextStage button
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/076v2_ChordCatch.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup076v2] ChordCatch シーン作成完了: " + scenePath);
    }

    static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup076v2] Field not found: {fieldName} on {obj.GetType().Name}");
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<Button>().targetGraphic = img;
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tRT = textGo.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
