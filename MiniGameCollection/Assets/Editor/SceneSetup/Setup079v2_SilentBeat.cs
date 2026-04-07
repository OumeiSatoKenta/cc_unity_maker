using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game079v2_SilentBeat;

public static class Setup079v2_SilentBeat
{
    [MenuItem("Assets/Setup/079v2 SilentBeat")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup079v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game079v2_SilentBeat/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
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
            float camSize = 6f;
            float camWidth = camSize * (16f / 9f);
            float scaleX = camWidth * 2f / bgSprite.bounds.size.x;
            float scaleY = camSize * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Load sprites
        Sprite tapAreaSprite       = LoadSprite(sp + "tap_area.png");
        Sprite tapAreaActiveSprite = LoadSprite(sp + "tap_area_active.png");
        Sprite accuracyBarSprite   = LoadSprite(sp + "accuracy_bar.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("SilentBeatGameManager");
        var gm = gmObj.AddComponent<SilentBeatGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 20, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.5f,  countMultiplier = 25, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 2.0f,  countMultiplier = 30, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.33f, countMultiplier = 40, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.5f,  countMultiplier = 50, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // RhythmManager
        var rmObj = new GameObject("RhythmManager");
        rmObj.transform.SetParent(gmObj.transform);
        var rm = rmObj.AddComponent<RhythmManager>();
        SetField(rm, "_gameManager", gm);

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // === HUD (top area) ===
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(350, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var bpmText = CT(canvasObj.transform, "BpmText", "BPM: 60", 42, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 55), new Vector2(0, -30));
        bpmText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        bpmText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);
        bpmText.SetActive(false);

        var guideText = CT(canvasObj.transform, "GuidePhaseText", "リズムを覚えよう！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 70), new Vector2(0, 0));
        guideText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        guideText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);
        guideText.SetActive(false);

        // Progress text
        var progressText = CT(canvasObj.transform, "ProgressText", "0 / 20", 44, jpFont,
            new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 55), new Vector2(0, 0));
        progressText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        progressText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 1f);

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, 0));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Judgement text (mid-screen)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 90), new Vector2(0, 0));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        judgementText.SetActive(false);

        // === Tap Area (center, large) ===
        var tapAreaObj = new GameObject("TapArea", typeof(RectTransform));
        tapAreaObj.transform.SetParent(canvasObj.transform, false);
        var tapAreaRT = tapAreaObj.GetComponent<RectTransform>();
        tapAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
        tapAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
        tapAreaRT.pivot = new Vector2(0.5f, 0.5f);
        tapAreaRT.sizeDelta = new Vector2(600, 600);
        tapAreaRT.anchoredPosition = new Vector2(0, -50);
        var tapAreaImg = tapAreaObj.AddComponent<Image>();
        tapAreaImg.sprite = tapAreaSprite;
        tapAreaImg.color = Color.white;
        tapAreaImg.raycastTarget = true;
        var tapAreaBtn = tapAreaObj.AddComponent<Button>();
        tapAreaBtn.targetGraphic = tapAreaImg;

        // === Accuracy Bar ===
        var accBarObj = new GameObject("AccuracyBar", typeof(RectTransform));
        accBarObj.transform.SetParent(canvasObj.transform, false);
        var accBarRT = accBarObj.GetComponent<RectTransform>();
        accBarRT.anchorMin = new Vector2(0.5f, 0.35f);
        accBarRT.anchorMax = new Vector2(0.5f, 0.35f);
        accBarRT.pivot = new Vector2(0.5f, 0.5f);
        accBarRT.sizeDelta = new Vector2(600, 32);
        accBarRT.anchoredPosition = Vector2.zero;
        var accBarImg = accBarObj.AddComponent<Image>();
        accBarImg.sprite = accuracyBarSprite;
        accBarImg.color = Color.white;

        // Accuracy dot (indicator)
        var accDotObj = new GameObject("AccuracyDot", typeof(RectTransform));
        accDotObj.transform.SetParent(accBarObj.transform, false);
        var accDotRT = accDotObj.GetComponent<RectTransform>();
        accDotRT.anchorMin = new Vector2(0.5f, 0.5f);
        accDotRT.anchorMax = new Vector2(0.5f, 0.5f);
        accDotRT.pivot = new Vector2(0.5f, 0.5f);
        accDotRT.sizeDelta = new Vector2(20, 28);
        accDotRT.anchoredPosition = Vector2.zero;
        var accDotImg = accDotObj.AddComponent<Image>();
        accDotImg.color = new Color(1f, 0.85f, 0f);

        // Back button (always visible bottom)
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(0, 15), new Color(0.08f, 0.06f, 0.18f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 360);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.03f, 0.08f, 0.2f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 62, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(620, 80), new Vector2(0, -25));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);

        var scStageNum = CT(scPanel.transform, "SCStageNum", "Stage 1 クリア！", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, 20));
        scStageNum.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStageNum.GetComponent<TextMeshProUGUI>().color = Color.white;

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(420, 70), new Vector2(0, 60), new Color(0.05f, 0.3f, 0.55f));
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 380);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.03f, 0.12f, 0.05f, 0.95f);

        var acTitle = CT(acPanel.transform, "ACTitle", "完全内部時計！", 62, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -25));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);

        var acScore = CT(acPanel.transform, "ACScore", "最終スコア: 0", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.1f, 0.2f, 0.1f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 380);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.15f, 0.03f, 0.05f, 0.95f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScore = CT(goPanel.transform, "GOScore", "スコア: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
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
        ipBgImg.color = new Color(0.02f, 0.02f, 0.1f, 0.95f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "SilentBeat", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.88f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 160), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0f, 0.2f, 0.45f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 80), new Color(0.1f, 0.1f, 0.25f, 0.9f));

        // === SilentBeatUI ===
        var uiObj = new GameObject("SilentBeatUI");
        var ui = uiObj.AddComponent<SilentBeatUI>();

        SetField(ui, "_stageText",           stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",           scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",           comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",       judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_progressText",        progressText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_bpmText",             bpmText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_guidePhaseText",      guideText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_tapAreaImage",        tapAreaImg);
        SetField(ui, "_tapAreaIdleSprite",   tapAreaSprite);
        SetField(ui, "_tapAreaActiveSprite", tapAreaActiveSprite);
        SetField(ui, "_accuracyIndicator",   accBarRT);
        SetField(ui, "_accuracyDot",         accDotImg);
        SetField(ui, "_stageClearPanel",     scPanel);
        SetField(ui, "_stageClearText",      scStageNum.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",       acPanel);
        SetField(ui, "_allClearScoreText",   acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",       goPanel);
        SetField(ui, "_gameOverScoreText",   goScore.GetComponent<TextMeshProUGUI>());

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_rhythmManager",    rm);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/079v2_SilentBeat.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup079v2] SilentBeat シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup079v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var lbl = CT(go.transform, "Label", label, fontSize, font,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        lbl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        lbl.GetComponent<TextMeshProUGUI>().color = Color.white;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = list.ToArray();
    }
}
