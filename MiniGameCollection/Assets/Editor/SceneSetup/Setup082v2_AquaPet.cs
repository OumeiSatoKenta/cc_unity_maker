using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game082v2_AquaPet;

public static class Setup082v2_AquaPet
{
    [MenuItem("Assets/Setup/082v2 AquaPet")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup082v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game082v2_AquaPet/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.02f, 0.05f, 0.15f);
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

        // Load fish sprites
        string[] fishSpriteNames = {
            "fish_goldfish.png",
            "fish_blue.png",
            "fish_green.png",
            "fish_tropical.png",
            "fish_clown.png",
            "fish_betta.png",
            "fish_guppy.png",
            "fish_saltwater.png",
            "fish_sea_angel.png",
            "fish_deep.png"
        };
        var fishSprites = new Sprite[fishSpriteNames.Length];
        for (int i = 0; i < fishSpriteNames.Length; i++)
            fishSprites[i] = LoadSprite(sp + fishSpriteNames[i]);

        Sprite foodSprite   = LoadSprite(sp + "food.png");
        Sprite bubbleSprite = LoadSprite(sp + "bubble.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("AquaPetGameManager");
        var gm = gmObj.AddComponent<AquaPetGameManager>();

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
        SetField(sm, "_stages", stageConfigs);

        // AquariumManager
        var amObj = new GameObject("AquariumManager");
        amObj.transform.SetParent(gmObj.transform);
        var am = amObj.AddComponent<AquariumManager>();
        SetField(am, "_fishSprites", fishSprites);
        SetField(am, "_foodSprite",   foodSprite);
        SetField(am, "_bubbleSprite", bubbleSprite);

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
            new Vector2(320, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.9f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(360, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 54, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 65), new Vector2(0, -100));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var collectionText = CT(canvasObj.transform, "CollectionText", "図鑑: 0/3", 42, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(460, 55), new Vector2(0, -165));
        collectionText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        collectionText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.8f);

        // === Status panel (mid-lower area - water quality & health) ===
        var statusPanel = new GameObject("StatusPanel", typeof(RectTransform));
        statusPanel.transform.SetParent(canvasObj.transform, false);
        var spRT = statusPanel.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0f, 0f); spRT.anchorMax = new Vector2(1f, 0f);
        spRT.pivot = new Vector2(0.5f, 0f);
        spRT.sizeDelta = new Vector2(0, 120);
        spRT.anchoredPosition = new Vector2(0, 310);

        var wqLabel = CT(statusPanel.transform, "WQLabel", "水質: 100%", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f),
            new Vector2(0, 50), new Vector2(20, -5));
        wqLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.7f);

        var feedLabel = CT(statusPanel.transform, "FeedLabel", "餌: 5", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(0, 50), new Vector2(-20, -5));
        feedLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        feedLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.5f);

        // WaterQuality Slider
        var wqSliderObj = new GameObject("WaterQualitySlider", typeof(RectTransform));
        wqSliderObj.transform.SetParent(statusPanel.transform, false);
        var wqSlRT = wqSliderObj.GetComponent<RectTransform>();
        wqSlRT.anchorMin = new Vector2(0f, 0f); wqSlRT.anchorMax = new Vector2(0.5f, 0f);
        wqSlRT.pivot = new Vector2(0f, 0f);
        wqSlRT.sizeDelta = new Vector2(-30, 30);
        wqSlRT.anchoredPosition = new Vector2(15, 10);
        var wqSlider = SetupSlider(wqSliderObj, new Color(0.2f, 0.8f, 0.5f));

        // Health Slider
        var hlSliderObj = new GameObject("HealthSlider", typeof(RectTransform));
        hlSliderObj.transform.SetParent(statusPanel.transform, false);
        var hlSlRT = hlSliderObj.GetComponent<RectTransform>();
        hlSlRT.anchorMin = new Vector2(0.5f, 0f); hlSlRT.anchorMax = new Vector2(1f, 0f);
        hlSlRT.pivot = new Vector2(0f, 0f);
        hlSlRT.sizeDelta = new Vector2(-30, 30);
        hlSlRT.anchoredPosition = new Vector2(15, 10);
        var hlSlider = SetupSlider(hlSliderObj, new Color(0.9f, 0.3f, 0.3f));

        // Health label
        var hlLabel = CT(statusPanel.transform, "HLLabel", "健康度", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(0, 50), new Vector2(-20, -55));
        hlLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        hlLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.6f);

        // === Buttons (bottom area, horizontal layout) ===
        // Feed button
        var feedBtn = CB(canvasObj.transform, "FeedButton", "🐟 餌やり", 42, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(280, 80), new Vector2(30, 200), new Color(0.1f, 0.35f, 0.6f));
        feedBtn.GetComponent<Button>().onClick.AddListener(() => am.OnFeedPressed());

        // Clean button
        var cleanBtn = CB(canvasObj.transform, "CleanButton", "🧹 掃除", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(240, 80), new Vector2(0, 200), new Color(0.05f, 0.35f, 0.3f));
        cleanBtn.GetComponent<Button>().onClick.AddListener(() => am.OnCleanPressed());

        // Breed button
        var breedBtn = CB(canvasObj.transform, "BreedButton", "💕 繁殖", 42, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(240, 80), new Vector2(-30, 200), new Color(0.4f, 0.1f, 0.5f));
        breedBtn.GetComponent<Button>().onClick.AddListener(() => am.OnBreedPressed());

        // Back button (always visible bottom)
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(0, 15), new Color(0.08f, 0.12f, 0.2f, 0.9f));
        backBtn.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 340);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.02f, 0.1f, 0.2f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(640, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 1f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(420, 70), new Vector2(0, 55), new Color(0.05f, 0.3f, 0.55f));
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
        acImg.color = new Color(0.02f, 0.1f, 0.05f, 0.95f);

        var acTitle = CT(acPanel.transform, "ACTitle", "図鑑コンプリート！", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 80), new Vector2(0, -25));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.1f, 0.2f, 0.1f));
        acBack.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
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

        var goTitle = CT(goPanel.transform, "GOTitle", "全滅...", 62, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 80), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScore = CT(goPanel.transform, "GOScore", "Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.4f, 0.1f, 0.1f));
        goBack.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
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
        ipBgImg.color = new Color(0.02f, 0.05f, 0.15f, 0.96f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "AquaPet", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 140), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.7f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0.05f, 0.25f, 0.45f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 290), new Color(0.1f, 0.15f, 0.25f, 0.9f));

        // === AquaPetUI ===
        var uiObj = new GameObject("AquaPetUI");
        var ui = uiObj.AddComponent<AquaPetUI>();

        SetField(ui, "_stageText",          stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",          scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",          comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_waterQualityText",   wqLabel.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_feedCountText",      feedLabel.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_collectionText",     collectionText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_waterQualitySlider", wqSlider);
        SetField(ui, "_healthSlider",       hlSlider);
        SetField(ui, "_stageClearPanel",    scPanel);
        SetField(ui, "_stageClearText",     scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",    nextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",      acPanel);
        SetField(ui, "_allClearScoreText",  acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",      goPanel);
        SetField(ui, "_gameOverScoreText",  goScore.GetComponent<TextMeshProUGUI>());

        // Wire AquariumManager
        SetField(am, "_gameManager", gm);
        SetField(am, "_ui",          ui);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_aquariumManager",  am);
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
        string scenePath = "Assets/Scenes/082v2_AquaPet.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup082v2] AquaPet シーン作成完了: " + scenePath);
    }

    static Slider SetupSlider(GameObject sliderObj, Color fillColor)
    {
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        // Background
        var bgGo = new GameObject("Background", typeof(RectTransform));
        bgGo.transform.SetParent(sliderObj.transform, false);
        var bgRT = bgGo.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        slider.targetGraphic = bgImg;

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = Vector2.zero; faRT.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;

        slider.fillRect = fillRT;
        return slider;
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
        else Debug.LogWarning($"[Setup082v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
