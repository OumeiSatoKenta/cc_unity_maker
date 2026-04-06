using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game066v2_RoboFactory;

public static class Setup066v2_RoboFactory
{
    [MenuItem("Assets/Setup/066v2 RoboFactory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup066v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game066v2_RoboFactory/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.1f, 0.05f, 0.18f);
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
            bgObj.transform.localScale = new Vector3(0.047f, 0.024f, 1f);
        }

        // Load bot sprites
        Sprite workerBotSpr  = LoadSprite(sp + "worker_bot.png");
        Sprite minerBotSpr   = LoadSprite(sp + "miner_bot.png");
        Sprite builderBotSpr = LoadSprite(sp + "builder_bot.png");
        Sprite repairBotSpr  = LoadSprite(sp + "worker_bot.png");  // reuse worker for repair
        Sprite powerBotSpr   = LoadSprite(sp + "power_bot.png");
        Sprite aiBotSpr      = LoadSprite(sp + "ai_bot.png");
        Sprite brokenWarnSpr = LoadSprite(sp + "broken_warning.png");

        Sprite houseSprite    = LoadSprite(sp + "building_house.png");
        Sprite factorySprite  = LoadSprite(sp + "building_factory.png");
        Sprite powerPlantSprite = LoadSprite(sp + "building_powerplant.png");
        Sprite labSprite      = LoadSprite(sp + "building_lab.png");
        Sprite drillSprite    = LoadSprite(sp + "building_mining_drill.png");
        Sprite aiCoreSprite   = LoadSprite(sp + "building_ai_core.png");
        Sprite emptyCellSprite = LoadSprite(sp + "empty_cell.png");

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<RoboFactoryGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 1.0f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 1.2f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 2, complexityFactor = 1.5f },
            new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 3, complexityFactor = 2.0f },
            new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 3, complexityFactor = 3.0f },
        };
        sm.SetConfigs(stages);

        // FactoryManager
        var fmObj = new GameObject("FactoryManager");
        fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<FactoryManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage (top center)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 1f);

        // HUD: Score (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(360, 55), new Vector2(-15, -90));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // HUD: City Level (top left)
        var cityLevelText = CT(canvasObj.transform, "CityLevelText", "City Lv.1 / 5", 34, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(420, 55), new Vector2(15, -90));
        cityLevelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        cityLevelText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        // Resources row (below city level)
        var oreText = CT(canvasObj.transform, "OreText", "Ore: 50", 28, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(280, 48), new Vector2(15, -148));
        oreText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 1f);

        var energyText = CT(canvasObj.transform, "EnergyText", "Energy: 30", 28, jpFont,
            new Vector2(0.33f, 1), new Vector2(0.33f, 1), new Vector2(0f, 1), new Vector2(280, 48), new Vector2(0, -148));
        energyText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.4f);

        var partsText = CT(canvasObj.transform, "PartsText", "Parts: 20", 28, jpFont,
            new Vector2(0.66f, 1), new Vector2(0.66f, 1), new Vector2(0f, 1), new Vector2(280, 48), new Vector2(0, -148));
        partsText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.8f);

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "COMBO x2 (1.5x)", 44, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(700, 65), new Vector2(0, -200));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        comboText.SetActive(false);

        // Collect rate
        var collectRateText = CT(canvasObj.transform, "CollectRateText", "", 26, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(400, 40), new Vector2(15, -200));
        collectRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 0.7f);

        // === ROBOT BUY BUTTONS (bottom section) ===
        // Row 1: Worker, Miner, Builder
        var buyWorkerBtn = CB(canvasObj.transform, "BuyWorkerButton", "Worker\n0/20/10", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 80), new Vector2(10, 800),
            new Color(0.5f, 0.1f, 0.6f));

        var buyMinerBtn = CB(canvasObj.transform, "BuyMinerButton", "Miner\n30/20/10", 22, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 80), new Vector2(0, 800),
            new Color(0.6f, 0.3f, 0.05f));
        buyMinerBtn.SetActive(false);

        var buyBuilderBtn = CB(canvasObj.transform, "BuyBuilderButton", "Builder\n50/30/20", 22, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(170, 80), new Vector2(-10, 800),
            new Color(0.1f, 0.4f, 0.15f));
        buyBuilderBtn.SetActive(false);

        // Row 2: Repair, Power, AI
        var buyRepairBtn = CB(canvasObj.transform, "BuyRepairButton", "Repair\n40/25/15", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 80), new Vector2(10, 710),
            new Color(0.5f, 0.1f, 0.15f));
        buyRepairBtn.SetActive(false);

        var buyPowerBtn = CB(canvasObj.transform, "BuyPowerButton", "Power\n60/40/30", 22, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 80), new Vector2(0, 710),
            new Color(0.05f, 0.3f, 0.6f));
        buyPowerBtn.SetActive(false);

        var buyAIBtn = CB(canvasObj.transform, "BuyAIButton", "AI Bot\n200/150/100", 20, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(170, 80), new Vector2(-10, 710),
            new Color(0f, 0.4f, 0.45f));
        buyAIBtn.SetActive(false);

        // === BUILDING BUTTONS ===
        // Row 3: House, Factory, PowerPlant
        var buildHouseBtn = CB(canvasObj.transform, "BuildHouseButton", "House\n20/10/10", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 80), new Vector2(10, 620),
            new Color(0.4f, 0.1f, 0.5f));

        var buildFactoryBtn = CB(canvasObj.transform, "BuildFactoryButton", "Factory\n40/20/30", 22, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 80), new Vector2(0, 620),
            new Color(0.5f, 0.1f, 0.25f));

        var buildPowerPlantBtn = CB(canvasObj.transform, "BuildPowerPlantButton", "PowerPlant\n50/10/40", 20, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(170, 80), new Vector2(-10, 620),
            new Color(0.05f, 0.25f, 0.5f));

        // Row 4: Lab, Drill, AICore
        var buildLabBtn = CB(canvasObj.transform, "BuildLabButton", "Lab\n60/40/50", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 80), new Vector2(10, 530),
            new Color(0f, 0.35f, 0.4f));

        var buildDrillBtn = CB(canvasObj.transform, "BuildMiningDrillButton", "Drill\n35/25/20", 22, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 80), new Vector2(0, 530),
            new Color(0.5f, 0.25f, 0f));

        var buildAICoreBtn = CB(canvasObj.transform, "BuildAICoreButton", "AI Core\n150/100/120", 20, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(170, 80), new Vector2(-10, 530),
            new Color(0.2f, 0.1f, 0.45f));
        buildAICoreBtn.SetActive(false);

        // === RESEARCH PANEL ===
        var researchPanel = new GameObject("ResearchPanel", typeof(RectTransform));
        researchPanel.transform.SetParent(canvasObj.transform, false);
        var rpRT = researchPanel.GetComponent<RectTransform>();
        rpRT.anchorMin = new Vector2(0f, 0); rpRT.anchorMax = new Vector2(1f, 0);
        rpRT.pivot = new Vector2(0.5f, 0);
        rpRT.sizeDelta = new Vector2(-20, 120);
        rpRT.anchoredPosition = new Vector2(0, 435);
        var rpImg = researchPanel.AddComponent<Image>();
        rpImg.color = new Color(0.15f, 0.05f, 0.25f, 0.9f);

        var resEffBtn = CB(researchPanel.transform, "ResearchEfficiencyButton", "研究\n効率UP\n(20P)", 20, jpFont,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(200, 100), new Vector2(10, 0),
            new Color(0.3f, 0.1f, 0.5f));

        var resRobotBtn = CB(researchPanel.transform, "ResearchRobotButton", "研究\n新Robot\n(20P)", 20, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200, 100), new Vector2(0, 0),
            new Color(0.3f, 0.15f, 0.5f));

        var resBuildingBtn = CB(researchPanel.transform, "ResearchBuildingButton", "研究\n新Building\n(20P)", 20, jpFont,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(200, 100), new Vector2(-10, 0),
            new Color(0.25f, 0.1f, 0.5f));

        // Research progress slider
        var sliderObj = new GameObject("ResearchProgressSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(researchPanel.transform, false);
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.05f, 0); sliderRT.anchorMax = new Vector2(0.95f, 0);
        sliderRT.pivot = new Vector2(0.5f, 0);
        sliderRT.sizeDelta = new Vector2(0, 12);
        sliderRT.anchoredPosition = new Vector2(0, 5);
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
        // Background
        var slBg = new GameObject("Background", typeof(RectTransform)); slBg.transform.SetParent(sliderObj.transform, false);
        var slBgRT = slBg.GetComponent<RectTransform>();
        slBgRT.anchorMin = Vector2.zero; slBgRT.anchorMax = Vector2.one; slBgRT.offsetMin = slBgRT.offsetMax = Vector2.zero;
        var slBgImg = slBg.AddComponent<Image>(); slBgImg.color = new Color(0.2f, 0.1f, 0.3f);
        // Fill area
        var slFillArea = new GameObject("Fill Area", typeof(RectTransform)); slFillArea.transform.SetParent(sliderObj.transform, false);
        var slFaRT = slFillArea.GetComponent<RectTransform>();
        slFaRT.anchorMin = Vector2.zero; slFaRT.anchorMax = Vector2.one; slFaRT.offsetMin = slFaRT.offsetMax = Vector2.zero;
        var slFill = new GameObject("Fill", typeof(RectTransform)); slFill.transform.SetParent(slFillArea.transform, false);
        var slFillRT = slFill.GetComponent<RectTransform>();
        slFillRT.anchorMin = Vector2.zero; slFillRT.anchorMax = Vector2.one; slFillRT.offsetMin = slFillRT.offsetMax = Vector2.zero;
        var slFillImg = slFill.AddComponent<Image>(); slFillImg.color = new Color(0.6f, 0.2f, 1f);
        slider.fillRect = slFillRT;
        slider.targetGraphic = slBgImg;
        researchPanel.SetActive(false);

        // === BROKEN ROBOT PANEL ===
        var brokenPanel = new GameObject("BrokenPanel", typeof(RectTransform));
        brokenPanel.transform.SetParent(canvasObj.transform, false);
        var bpRT = brokenPanel.GetComponent<RectTransform>();
        bpRT.anchorMin = new Vector2(0f, 0); bpRT.anchorMax = new Vector2(0.5f, 0);
        bpRT.pivot = new Vector2(0f, 0);
        bpRT.sizeDelta = new Vector2(0, 130);
        bpRT.anchoredPosition = new Vector2(0, 345);
        var bpImg = brokenPanel.AddComponent<Image>();
        bpImg.color = new Color(0.5f, 0.05f, 0.05f, 0.95f);

        var brokenText = CT(brokenPanel.transform, "BrokenText", "Worker が故障！\nパーツ10で修理", 26, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(400, 70), Vector2.zero);
        brokenText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        brokenText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var repairBtn = CB(brokenPanel.transform, "RepairButton", "修理 (10P)", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 60), Vector2.zero,
            new Color(0.6f, 0.2f, 0.1f));
        brokenPanel.SetActive(false);

        // === ENERGY WARNING PANEL ===
        var energyWarningPanel = new GameObject("EnergyWarningPanel", typeof(RectTransform));
        energyWarningPanel.transform.SetParent(canvasObj.transform, false);
        var ewRT = energyWarningPanel.GetComponent<RectTransform>();
        ewRT.anchorMin = new Vector2(0.5f, 0); ewRT.anchorMax = new Vector2(1f, 0);
        ewRT.pivot = new Vector2(1f, 0);
        ewRT.sizeDelta = new Vector2(0, 60);
        ewRT.anchoredPosition = new Vector2(0, 345);
        var ewImg = energyWarningPanel.AddComponent<Image>();
        ewImg.color = new Color(0.6f, 0.3f, 0f, 0.9f);
        var ewText = CT(energyWarningPanel.transform, "EnergyWarningText", "⚡ エネルギー不足！", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 50), Vector2.zero);
        ewText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ewText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        energyWarningPanel.SetActive(false);

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 28, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(180, 65), new Vector2(20, 290),
            new Color(0.2f, 0.1f, 0.3f));
        menuBtn.AddComponent<BackToMenuButton>();

        // === STAGE CLEAR PANEL ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scpRT = scPanel.GetComponent<RectTransform>();
        scpRT.anchorMin = new Vector2(0.1f, 0.3f); scpRT.anchorMax = new Vector2(0.9f, 0.7f);
        scpRT.offsetMin = scpRT.offsetMax = Vector2.zero;
        var scpImg = scPanel.AddComponent<Image>();
        scpImg.color = new Color(0.08f, 0.04f, 0.18f, 0.95f);

        var scText = CT(scPanel.transform, "StageClearText", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var nextStageBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(420, 85), Vector2.zero,
            new Color(0.3f, 0.15f, 0.6f));
        scPanel.SetActive(false);

        // === ALL CLEAR PANEL ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acpRT = acPanel.GetComponent<RectTransform>();
        acpRT.anchorMin = new Vector2(0.1f, 0.25f); acpRT.anchorMax = new Vector2(0.9f, 0.75f);
        acpRT.offsetMin = acpRT.offsetMax = Vector2.zero;
        var acpImg = acPanel.AddComponent<Image>();
        acpImg.color = new Color(0.05f, 0.02f, 0.15f, 0.97f);

        var acText = CT(acPanel.transform, "AllClearText", "RoboFactory 完全制覇！", 42, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        acText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var finalScoreText = CT(acPanel.transform, "FinalScoreText", "最終スコア: 0", 38, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(800, 70), Vector2.zero);
        finalScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        finalScoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acMenuBtn = CB(acPanel.transform, "MenuButton2", "メニューへ", 34, jpFont,
            new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(320, 80), Vector2.zero,
            new Color(0.3f, 0.2f, 0.6f));
        acMenuBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === INSTRUCTION PANEL ===
        var ipCanvas = new GameObject("InstructionPanelCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<GraphicRaycaster>();
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0f, 0f, 0f, 0.88f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "RoboFactory", 64, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 90), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "ロボットを作って都市を建設しよう", 38, jpFont,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "ボタンでロボット製造・建設・研究を指示", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "都市レベル目標を達成してステージクリア", 32, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 44, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(380, 90), Vector2.zero,
            new Color(0.3f, 0.15f, 0.65f));

        // Help button
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-20, 290),
            new Color(0.2f, 0.2f, 0.4f));

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- RoboFactoryUI ---
        var uiObj = new GameObject("RoboFactoryUI");
        uiObj.transform.SetParent(canvasObj.transform, false);
        var ui = uiObj.AddComponent<RoboFactoryUI>();

        // Wire UI fields
        SetField(ui, "_stageText",     stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",     scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_cityLevelText", cityLevelText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_oreText",       oreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_energyText",    energyText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_partsText",     partsText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",     comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_collectRateText", collectRateText.GetComponent<TextMeshProUGUI>());

        SetField(ui, "_buyWorkerBtn",  buyWorkerBtn.GetComponent<Button>());
        SetField(ui, "_buyMinerBtn",   buyMinerBtn.GetComponent<Button>());
        SetField(ui, "_buyBuilderBtn", buyBuilderBtn.GetComponent<Button>());
        SetField(ui, "_buyRepairBtn",  buyRepairBtn.GetComponent<Button>());
        SetField(ui, "_buyPowerBtn",   buyPowerBtn.GetComponent<Button>());
        SetField(ui, "_buyAIBtn",      buyAIBtn.GetComponent<Button>());

        SetField(ui, "_buildHouseBtn",      buildHouseBtn.GetComponent<Button>());
        SetField(ui, "_buildFactoryBtn",    buildFactoryBtn.GetComponent<Button>());
        SetField(ui, "_buildPowerPlantBtn", buildPowerPlantBtn.GetComponent<Button>());
        SetField(ui, "_buildLabBtn",        buildLabBtn.GetComponent<Button>());
        SetField(ui, "_buildMiningDrillBtn", buildDrillBtn.GetComponent<Button>());
        SetField(ui, "_buildAICoreBtn",     buildAICoreBtn.GetComponent<Button>());

        SetField(ui, "_researchPanel",           researchPanel);
        SetField(ui, "_researchEfficiencyBtn",   resEffBtn.GetComponent<Button>());
        SetField(ui, "_researchRobotBtn",        resRobotBtn.GetComponent<Button>());
        SetField(ui, "_researchBuildingBtn",     resBuildingBtn.GetComponent<Button>());
        SetField(ui, "_researchProgressSlider",  slider);

        SetField(ui, "_brokenPanel",       brokenPanel);
        SetField(ui, "_brokenText",        brokenText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_repairBtn",         repairBtn.GetComponent<Button>());

        SetField(ui, "_energyWarningPanel", energyWarningPanel);
        SetField(ui, "_stageClearPanel",    scPanel);
        SetField(ui, "_stageClearText",     scText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageBtn",       nextStageBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",      acPanel);
        SetField(ui, "_finalScoreText",     finalScoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_menuBtn",            menuBtn.GetComponent<Button>());
        SetField(ui, "_factoryManager",     fm);
        SetField(ui, "_gameManager",        gm);

        // Wire FactoryManager fields
        SetField(fm, "_gameManager", gm);
        SetField(fm, "_ui", ui);
        SetField(fm, "_workerBotSprite",   workerBotSpr);
        SetField(fm, "_minerBotSprite",    minerBotSpr);
        SetField(fm, "_builderBotSprite",  builderBotSpr);
        SetField(fm, "_repairBotSprite",   repairBotSpr);
        SetField(fm, "_powerBotSprite",    powerBotSpr);
        SetField(fm, "_aiBotSprite",       aiBotSpr);
        SetField(fm, "_brokenWarningSprite", brokenWarnSpr);
        SetField(fm, "_buildingHouseSprite",    houseSprite);
        SetField(fm, "_buildingFactorySprite",  factorySprite);
        SetField(fm, "_buildingPowerPlantSprite", powerPlantSprite);
        SetField(fm, "_buildingLabSprite",      labSprite);
        SetField(fm, "_buildingMiningDrillSprite", drillSprite);
        SetField(fm, "_buildingAICoreSprite",   aiCoreSprite);
        SetField(fm, "_emptyCellSprite",        emptyCellSprite);

        // Wire GameManager
        SetField(gm, "_stageManager",    sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_factoryManager",   fm);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Save scene
        string scenePath = "Assets/Scenes/066v2_RoboFactory.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup066v2] RoboFactory シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup066v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
