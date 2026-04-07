using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game084v2_GardenZen;

public static class Setup084v2_GardenZen
{
    [MenuItem("Assets/Setup/084v2 GardenZen")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup084v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game084v2_GardenZen/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.78f, 0.69f, 0.55f);
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
        Sprite gridCellSprite   = LoadSprite(sp + "grid_cell.png");
        Sprite gridTargetSprite = LoadSprite(sp + "grid_target.png");
        Sprite stone1Sprite     = LoadSprite(sp + "stone_1.png");
        Sprite stone2Sprite     = LoadSprite(sp + "stone_2.png");
        Sprite stone3Sprite     = LoadSprite(sp + "stone_3.png");
        Sprite plant1Sprite     = LoadSprite(sp + "plant_1.png");
        Sprite plant2Sprite     = LoadSprite(sp + "plant_2.png");
        Sprite decoSprite       = LoadSprite(sp + "decoration_1.png");
        Sprite sandPatternSprite = LoadSprite(sp + "sand_pattern.png");

        Sprite paletteStoneSprite = LoadSprite(sp + "palette_stone.png");
        Sprite palettePlantSprite = LoadSprite(sp + "palette_plant.png");
        Sprite paletteDecoSprite  = LoadSprite(sp + "palette_deco.png");
        Sprite paletteSandSprite  = LoadSprite(sp + "palette_sand.png");
        Sprite eraserSprite       = LoadSprite(sp + "eraser.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("GardenZenGameManager");
        var gm = gmObj.AddComponent<GardenZenGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 4, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 5, complexityFactor = 1.0f },
        };
        sm.SetConfigs(stageConfigs);

        // GardenManager
        var gardenObj = new GameObject("GardenManager");
        gardenObj.transform.SetParent(gmObj.transform);
        var gardenMgr = gardenObj.AddComponent<GardenManager>();
        SetField(gardenMgr, "_gridCellSprite",    gridCellSprite);
        SetField(gardenMgr, "_gridTargetSprite",  gridTargetSprite);
        SetField(gardenMgr, "_stone1Sprite",      stone1Sprite);
        SetField(gardenMgr, "_stone2Sprite",      stone2Sprite);
        SetField(gardenMgr, "_stone3Sprite",      stone3Sprite);
        SetField(gardenMgr, "_plant1Sprite",      plant1Sprite);
        SetField(gardenMgr, "_plant2Sprite",      plant2Sprite);
        SetField(gardenMgr, "_decorationSprite",  decoSprite);
        SetField(gardenMgr, "_sandPatternSprite", sandPatternSprite);

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

        // === HUD (top) ===
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.25f, 0.1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(340, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.3f, 0.1f);

        var matchRateText = CT(canvasObj.transform, "MatchRateText", "一致度: 0%", 42, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 55), new Vector2(0, -90));
        matchRateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        matchRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.5f, 0.2f);

        var messageText = CT(canvasObj.transform, "MessageText", "", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(800, 60), new Vector2(0, -155));
        messageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        messageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.3f, 0.1f);
        messageText.SetActive(false);

        // === Palette buttons (bottom, horizontal) ===
        // Stone
        var stoneBtn = CIB(canvasObj.transform, "StoneBtn", paletteStoneSprite,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(130, 130), new Vector2(30, 310));
        // Plant
        var plantBtn = CIB(canvasObj.transform, "PlantBtn", palettePlantSprite,
            new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f),
            new Vector2(130, 130), new Vector2(0, 310));
        // Deco
        var decoBtn = CIB(canvasObj.transform, "DecoBtn", paletteDecoSprite,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(130, 130), new Vector2(0, 310));
        // Sand
        var sandBtn = CIB(canvasObj.transform, "SandBtn", paletteSandSprite,
            new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.5f, 0f),
            new Vector2(130, 130), new Vector2(0, 310));
        // Eraser
        var eraserBtn = CIB(canvasObj.transform, "EraserBtn", eraserSprite,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(130, 130), new Vector2(-30, 310));

        // Submit & Reset buttons
        var submitBtn = CB(canvasObj.transform, "SubmitButton", "提出", 46, jpFont,
            new Vector2(0.3f, 0f), new Vector2(0.3f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 75), new Vector2(0, 200), new Color(0.3f, 0.55f, 0.25f));

        var resetBtn = CB(canvasObj.transform, "ResetButton", "リセット", 42, jpFont,
            new Vector2(0.7f, 0f), new Vector2(0.7f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 75), new Vector2(0, 200), new Color(0.55f, 0.3f, 0.15f));

        // Back button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(0, 15), new Color(0.25f, 0.18f, 0.1f, 0.9f));
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
        scImg.color = new Color(0.9f, 0.85f, 0.7f, 0.97f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(640, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.25f, 0.05f);

        var scStars = CT(scPanel.transform, "SCStars", "★★★", 72, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), new Vector2(0, 30));
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.65f, 0.1f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(420, 70), new Vector2(0, 55), new Color(0.3f, 0.5f, 0.2f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 400);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.92f, 0.88f, 0.72f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "庭園コンプリート！", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 80), new Vector2(0, -25));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.3f, 0.05f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.25f, 0.05f);

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.35f, 0.22f, 0.08f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

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
        ipBgImg.color = new Color(0.88f, 0.82f, 0.68f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "GardenZen", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.25f, 0.07f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.2f, 0.05f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 140), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.3f, 0.1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.45f, 0.1f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0.3f, 0.5f, 0.15f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 450), new Color(0.3f, 0.22f, 0.1f, 0.9f));

        // === GardenZenUI ===
        var uiObj = new GameObject("GardenZenUI");
        var ui = uiObj.AddComponent<GardenZenUI>();

        SetField(ui, "_stageText",     stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",     scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_matchRateText", matchRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_messageText",   messageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stoneBtn",      stoneBtn.GetComponent<Button>());
        SetField(ui, "_plantBtn",      plantBtn.GetComponent<Button>());
        SetField(ui, "_decoBtn",       decoBtn.GetComponent<Button>());
        SetField(ui, "_sandBtn",       sandBtn.GetComponent<Button>());
        SetField(ui, "_eraserBtn",     eraserBtn.GetComponent<Button>());
        SetField(ui, "_submitBtn",     submitBtn.GetComponent<Button>());
        SetField(ui, "_resetBtn",      resetBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel",      scPanel);
        SetField(ui, "_stageClearText",       scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearStarsText",  scStars.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",      nextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",        acPanel);
        SetField(ui, "_allClearScoreText",    acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gardenManager",        gardenMgr);

        // Wire GardenManager
        SetField(gardenMgr, "_gameManager", gm);
        SetField(gardenMgr, "_ui",          ui);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_gardenManager",    gardenMgr);
        SetField(gm, "_ui",               ui);

        // Wire buttons needing gm reference
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());

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
        string scenePath = "Assets/Scenes/084v2_GardenZen.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup084v2] GardenZen シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup084v2] Field not found: {fieldName} on {obj.GetType().Name}");
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

    // Image Button (icon only)
    static GameObject CIB(Transform parent, string name, Sprite icon,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        if (icon != null) img.sprite = icon;
        img.color = Color.white;
        go.AddComponent<Button>().targetGraphic = img;
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
