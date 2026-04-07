using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game068v2_CloudFarm;

public static class Setup068v2_CloudFarm
{
    [MenuItem("Assets/Setup/068v2 CloudFarm")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup068v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game068v2_CloudFarm/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.5f, 0.8f, 1.0f);
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
            // 256x512 → scale to fit 12x24 world units
            bgObj.transform.localScale = new Vector3(0.047f, 0.047f, 1f);
        }

        // --- Farm Grid (world space, 2 cols x 6 rows) ---
        float camSize = 6f;
        float camWidth = camSize * (9f / 16f); // approx
        float topMargin = 1.5f;
        float bottomMargin = 3.0f;
        float availableH = camSize * 2f - topMargin - bottomMargin;
        int rows = 6;
        int cols = 2;
        float cellSize = Mathf.Min(availableH / rows, camWidth * 2f / cols, 1.6f);
        float startX = -cellSize * (cols - 1) / 2f;
        float startY = camSize - topMargin - cellSize / 2f;

        Sprite plotEmptySprite = LoadSprite(sp + "plot_empty.png");
        Sprite plotGrowingSprite = LoadSprite(sp + "plot_growing.png");
        Sprite plotReadySprite = LoadSprite(sp + "plot_ready.png");
        Sprite cropCarrotSprite = LoadSprite(sp + "crop_carrot.png");
        Sprite cropCabbageSprite = LoadSprite(sp + "crop_cabbage.png");
        Sprite cropTomatoSprite = LoadSprite(sp + "crop_tomato.png");
        Sprite cropStarMelonSprite = LoadSprite(sp + "crop_star_melon.png");
        Sprite weatherSunnySprite = LoadSprite(sp + "weather_sunny.png");
        Sprite weatherRainySprite = LoadSprite(sp + "weather_rainy.png");
        Sprite weatherStormySprite = LoadSprite(sp + "weather_stormy.png");

        int totalPlots = rows * cols;
        var plotRenderers = new SpriteRenderer[totalPlots];
        var plotColliders = new Collider2D[totalPlots];
        var farmGridObj = new GameObject("FarmGrid");

        for (int i = 0; i < totalPlots; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = startX + col * cellSize;
            float y = startY - row * cellSize;

            var plotObj = new GameObject($"Plot_{i:D2}");
            plotObj.transform.SetParent(farmGridObj.transform);
            plotObj.transform.position = new Vector3(x, y, 0f);

            var sr = plotObj.AddComponent<SpriteRenderer>();
            sr.sprite = plotEmptySprite;
            sr.sortingOrder = 1;
            float sprScale = cellSize / 1.28f; // 128px → 1.28 world units at PPU=100
            plotObj.transform.localScale = new Vector3(sprScale, sprScale, 1f);

            var col2d = plotObj.AddComponent<BoxCollider2D>();
            col2d.size = new Vector2(1f, 1f);

            plotRenderers[i] = sr;
            plotColliders[i] = col2d;
        }

        // Weather Icon (top right world space)
        var weatherObj = new GameObject("WeatherIcon");
        var weatherSr = weatherObj.AddComponent<SpriteRenderer>();
        weatherSr.sprite = weatherSunnySprite;
        weatherSr.sortingOrder = 5;
        weatherObj.transform.position = new Vector3(camWidth - 0.8f, camSize - 0.8f, 0f);
        weatherObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        weatherObj.SetActive(false); // enabled when stage 2+

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CloudFarmGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.2f },
            new StageManager.StageConfig { speedMultiplier = 1.3f, countMultiplier = 2, complexityFactor = 0.4f },
            new StageManager.StageConfig { speedMultiplier = 1.6f, countMultiplier = 2, complexityFactor = 0.6f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.8f },
            new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 4, complexityFactor = 1.0f },
        };
        sm.SetConfigs(stages);

        // FarmManager
        var fmObj = new GameObject("FarmManager");
        fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<FarmManager>();

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.4f, 0.1f);

        // HUD: Coins (top left)
        var coinsText = CT(canvasObj.transform, "CoinsText", "コイン: 0G", 36, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(380, 55), new Vector2(15, -90));
        coinsText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 0f);

        // HUD: Progress (top right)
        var progressText = CT(canvasObj.transform, "ProgressText", "出荷: 0 / 500G", 30, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(480, 55), new Vector2(-15, -90));
        progressText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        progressText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.7f, 0.2f);

        // HUD: Weather (below coins)
        var weatherText = CT(canvasObj.transform, "WeatherText", "☀晴れ", 30, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(200, 44), new Vector2(15, -148));
        weatherText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0f);

        // HUD: Market price (below progress)
        var marketText = CT(canvasObj.transform, "MarketText", "市場: 1.0x", 30, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(250, 44), new Vector2(-15, -148));
        marketText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        marketText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // HUD: Auto rate
        var autoRateText = CT(canvasObj.transform, "AutoRateText", "", 26, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(300, 44), new Vector2(15, -196));
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.9f, 0.5f);

        // HUD: Selected crop
        var selectedCropText = CT(canvasObj.transform, "SelectedCropText", "種: にんじん", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 44), new Vector2(0, -90));
        selectedCropText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        selectedCropText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.7f, 0.3f);

        // HUD: Inventory
        var inventoryText = CT(canvasObj.transform, "InventoryText", "在庫: 0G", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 44), new Vector2(0, -138));
        inventoryText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        inventoryText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "", 48, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(700, 65), new Vector2(0, -248));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        // === BOTTOM BUTTONS ===
        // Sell button (bottom center)
        var sellBtn = CB(canvasObj.transform, "SellButton", "出荷", 38, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(300, 80), new Vector2(0, 95),
            new Color(0.2f, 0.65f, 0.2f));
        var sellBtnText = sellBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        // Crop select buttons (bottom left, horizontal)
        var carrotBtn = CB(canvasObj.transform, "CarrotButton", "にんじん\n50G/30s", 20, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 70), new Vector2(10, 185),
            new Color(0.9f, 0.45f, 0f));

        var cabbageBtn = CB(canvasObj.transform, "CabbageButton", "キャベツ\n80G/50s", 20, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 70), new Vector2(185, 185),
            new Color(0.2f, 0.6f, 0.2f));

        var tomatoBtn = CB(canvasObj.transform, "TomatoButton", "トマト\n120G/80s", 20, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(170, 70), new Vector2(360, 185),
            new Color(0.8f, 0.2f, 0.1f));

        var starMelonBtn = CB(canvasObj.transform, "StarMelonButton", "スターメロン\n1000G/120s", 18, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(200, 70), new Vector2(535, 185),
            new Color(0.8f, 0.7f, 0f));
        starMelonBtn.SetActive(false); // shown when stage 5+

        // Upgrade buttons (right side, bottom area)
        var autoUpgradeBtn = CB(canvasObj.transform, "AutoUpgradeButton", "自動強化\n200G", 22, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(230, 80), new Vector2(-10, 185),
            new Color(0.3f, 0.3f, 0.7f));
        autoUpgradeBtn.SetActive(false); // shown when stage 2+

        var growthUpgradeBtn = CB(canvasObj.transform, "GrowthUpgradeButton", "成長強化\n300G", 22, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(230, 80), new Vector2(-10, 265),
            new Color(0.4f, 0.2f, 0.6f));

        // Back to menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 28, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(220, 65), new Vector2(-10, 15),
            new Color(0.2f, 0.2f, 0.2f, 0.85f));
        menuBtn.GetComponent<Button>().onClick.AddListener(() => { });
        menuBtn.AddComponent<BackToMenuButton>();

        // === STAGE CLEAR PANEL ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 400);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.15f, 0.05f, 0.95f);

        var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 80), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.4f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(350, 80), Vector2.zero,
            new Color(0.2f, 0.7f, 0.2f));
        scPanel.SetActive(false);

        // === ALL CLEAR PANEL ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 500);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.02f, 0.1f, 0.02f, 0.98f);

        var acTitle = CT(acPanel.transform, "AllClearTitle", "農場王になった！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(650, 80), Vector2.zero);
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "AllClearScoreText", "総出荷額: ---", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 80), Vector2.zero);
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acMenuBtn = CB(acPanel.transform, "BackToMenuButton", "メニューへ", 36, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f), new Vector2(300, 80), Vector2.zero,
            new Color(0.2f, 0.2f, 0.5f));
        acMenuBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === INSTRUCTION PANEL ===
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
        var ipRT = ipBg.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;
        var ipImg = ipBg.AddComponent<Image>();
        ipImg.color = new Color(0.02f, 0.1f, 0.02f, 0.97f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "CloudFarm", 72, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f), new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "雲の上の農場で作物を育てて出荷しよう", 40, jpFont,
            new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "タップで種まき・収穫、ボタンで出荷", 36, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "出荷目標を達成してステージクリア", 36, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 48, jpFont,
            new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), new Vector2(380, 100), Vector2.zero,
            new Color(0.2f, 0.6f, 0.2f));

        // Help button (bottom right)
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-10, 90),
            new Color(0.1f, 0.4f, 0.1f));

        // === CloudFarmUI ===
        var uiObj = new GameObject("CloudFarmUI");
        var ui = uiObj.AddComponent<CloudFarmUI>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === Wire Fields ===
        // FarmManager
        SetField(fm, "_gameManager",         gm);
        SetField(fm, "_camera",              camera);
        SetField(fm, "_plotRenderers",       plotRenderers);
        SetField(fm, "_plotColliders",       plotColliders);
        SetField(fm, "_plotEmptySprite",     plotEmptySprite);
        SetField(fm, "_plotGrowingSprite",   plotGrowingSprite);
        SetField(fm, "_plotReadySprite",     plotReadySprite);
        SetField(fm, "_cropCarrotSprite",    cropCarrotSprite);
        SetField(fm, "_cropCabbageSprite",   cropCabbageSprite);
        SetField(fm, "_cropTomatoSprite",    cropTomatoSprite);
        SetField(fm, "_cropStarMelonSprite", cropStarMelonSprite);
        SetField(fm, "_weatherIconRenderer", weatherSr);
        SetField(fm, "_weatherSunnySprite",  weatherSunnySprite);
        SetField(fm, "_weatherRainySprite",  weatherRainySprite);
        SetField(fm, "_weatherStormySprite", weatherStormySprite);

        // CloudFarmUI
        SetField(ui, "_stageText",          stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_coinsText",          coinsText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_inventoryText",      inventoryText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_weatherText",        weatherText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_marketText",         marketText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_progressText",       progressText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",          comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText",       autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_selectedCropText",   selectedCropText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",    scPanel);
        SetField(ui, "_stageClearText",     scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",      acPanel);
        SetField(ui, "_allClearScoreText",  acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoUpgradeBtn",     autoUpgradeBtn.GetComponent<Button>());
        SetField(ui, "_autoUpgradeCostText",autoUpgradeBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_growthUpgradeBtn",   growthUpgradeBtn.GetComponent<Button>());
        SetField(ui, "_growthUpgradeCostText", growthUpgradeBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_sellBtn",            sellBtn.GetComponent<Button>());
        SetField(ui, "_sellBtnText",        sellBtnText);

        // CloudFarmGameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_farmManager",      fm);
        SetField(gm, "_ui",               ui);

        // InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire button OnClick events
        sellBtn.GetComponent<Button>().onClick.AddListener(() => fm.Sell());
        carrotBtn.GetComponent<Button>().onClick.AddListener(() => fm.SelectCrop(0));
        cabbageBtn.GetComponent<Button>().onClick.AddListener(() => fm.SelectCrop(1));
        tomatoBtn.GetComponent<Button>().onClick.AddListener(() => fm.SelectCrop(2));
        starMelonBtn.GetComponent<Button>().onClick.AddListener(() => fm.SelectCrop(3));
        autoUpgradeBtn.GetComponent<Button>().onClick.AddListener(() => fm.UpgradeAutoHarvest());
        growthUpgradeBtn.GetComponent<Button>().onClick.AddListener(() => fm.UpgradeGrowth());
        nextBtn.GetComponent<Button>().onClick.AddListener(() => {
            ui.HideStageClear();
            gm.NextStage();
        });

        // Save scene
        string scenePath = "Assets/Scenes/068v2_CloudFarm.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup068v2] CloudFarm シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup068v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
