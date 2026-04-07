using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game077v2_BeatRunner;

public static class Setup077v2_BeatRunner
{
    [MenuItem("Assets/Setup/077v2 BeatRunner")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup077v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game077v2_BeatRunner/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.04f, 0.10f);
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
        Sprite runnerIdle  = LoadSprite(sp + "runner_idle.png");
        Sprite runnerJump  = LoadSprite(sp + "runner_jump.png");
        Sprite runnerSlide = LoadSprite(sp + "runner_slide.png");
        Sprite obsJump     = LoadSprite(sp + "obstacle_jump.png");
        Sprite obsSlide    = LoadSprite(sp + "obstacle_slide.png");
        Sprite coinSprite  = LoadSprite(sp + "coin.png");
        Sprite groundSprite= LoadSprite(sp + "ground_tile.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("BeatRunnerGameManager");
        var gm = gmObj.AddComponent<BeatRunnerGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.2f,  countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.4f,  countMultiplier = 1, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.6f,  countMultiplier = 1, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 1.8f,  countMultiplier = 1, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // BeatManager
        var bmObj = new GameObject("BeatManager");
        bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BeatManager>();
        SetField(bm, "_gameManager", gm);

        // Runner
        float camSz = 6f;
        float camW = camSz * 1.777778f;
        float groundY = -camSz + 3.0f;
        float runnerBaseY = groundY + 0.6f;
        float runnerX = -camW * 0.35f;

        var runnerObj = new GameObject("Runner");
        runnerObj.transform.SetParent(bmObj.transform);
        runnerObj.transform.position = new Vector3(runnerX, runnerBaseY, 0f);
        var runnerSR = runnerObj.AddComponent<SpriteRenderer>();
        runnerSR.sprite = runnerIdle;
        runnerSR.sortingOrder = 5;
        runnerObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        SetField(bm, "_runnerRenderer", runnerSR);

        // Obstacle pool
        var obsPool = new GameObject("ObstaclePool");
        obsPool.transform.SetParent(bmObj.transform);
        SetField(bm, "_obstaclePool", obsPool.transform);

        // Ground tiles container
        var groundContainer = new GameObject("GroundContainer");
        groundContainer.transform.SetParent(bmObj.transform);
        SetField(bm, "_groundContainer", groundContainer.transform);

        // Wire sprites
        SetField(bm, "_runnerIdleSprite",   runnerIdle);
        SetField(bm, "_runnerJumpSprite",   runnerJump);
        SetField(bm, "_runnerSlideSprite",  runnerSlide);
        SetField(bm, "_obstacleJumpSprite", obsJump);
        SetField(bm, "_obstacleSlideSprite",obsSlide);
        SetField(bm, "_coinSprite",         coinSprite);
        SetField(bm, "_groundTileSprite",   groundSprite);

        // Beat marker (world space visual)
        var beatMarkerSprite = LoadSprite(sp + "beat_marker.png");
        if (beatMarkerSprite != null)
        {
            var bmLine = new GameObject("BeatMarkerLine");
            bmLine.transform.SetParent(bmObj.transform);
            bmLine.transform.position = new Vector3(runnerX + 2.5f, 0f, 0f);
            var bmSR = bmLine.AddComponent<SpriteRenderer>();
            bmSR.sprite = beatMarkerSprite;
            bmSR.sortingOrder = 3;
            bmSR.color = new Color(0f, 0.9f, 1f, 0.4f);
        }

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 55), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.95f, 1f);

        // HUD: Score (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score\n0", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(260, 90), new Vector2(-15, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);

        // HUD: Life (top left)
        var lifeText = CT(canvasObj.transform, "LifeText", "♥ ♥ ♥", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(260, 55), new Vector2(15, -30));
        lifeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.5f);

        // HUD: Progress (top left, below life)
        var progressText = CT(canvasObj.transform, "ProgressText", "0/20", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(160, 50), new Vector2(15, -90));
        progressText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.8f, 1f);

        // Combo text (center-upper)
        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 80), new Vector2(0, 400));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(0.88f, 0.25f, 0.98f);
        comboText.gameObject.SetActive(false);

        // Judgement text (center)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 100), new Vector2(0, 200));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.gameObject.SetActive(false);

        // Back button (bottom center)
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
        scImg.color = new Color(0.02f, 0.08f, 0.18f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.9f, 1f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 50), new Color(0.05f, 0.25f, 0.5f));
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
        ipBgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.93f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "BeatRunner", 72, jpFont,
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
            new Vector2(900, 130), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 36, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0f, 0.25f, 0.5f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 80), new Color(0.1f, 0.15f, 0.25f, 0.9f));

        // === BeatRunnerUI ===
        var uiObj = new GameObject("BeatRunnerUI");
        var ui = uiObj.AddComponent<BeatRunnerUI>();

        SetField(ui, "_stageText",         stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_lifeText",          lifeText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_progressText",      progressText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",     judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearTitle",   scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScore.GetComponent<TextMeshProUGUI>());

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_beatManager",      bm);
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
        string scenePath = "Assets/Scenes/077v2_BeatRunner.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup077v2] BeatRunner シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup077v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
