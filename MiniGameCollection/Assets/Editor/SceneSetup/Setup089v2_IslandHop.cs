using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game089v2_IslandHop;

public static class Setup089v2_IslandHop
{
    [MenuItem("Assets/Setup/089v2 IslandHop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup089v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game089v2_IslandHop/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.1f, 0.3f, 0.6f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // === Background ===
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

        // === Load sprites ===
        Sprite sprIslandForest   = LoadSprite(sp + "island_forest.png");
        Sprite sprIslandRocky    = LoadSprite(sp + "island_rocky.png");
        Sprite sprIslandTropical = LoadSprite(sp + "island_tropical.png");
        Sprite sprIslandVolcanic = LoadSprite(sp + "island_volcanic.png");
        Sprite sprIslandCoral    = LoadSprite(sp + "island_coral.png");

        Sprite sprCottage     = LoadSprite(sp + "facility_cottage.png");
        Sprite sprPier        = LoadSprite(sp + "facility_pier.png");
        Sprite sprGarden      = LoadSprite(sp + "facility_garden.png");
        Sprite sprRestaurant  = LoadSprite(sp + "facility_restaurant.png");
        Sprite sprObservation = LoadSprite(sp + "facility_observation.png");
        Sprite sprSpa         = LoadSprite(sp + "facility_spa.png");
        Sprite sprMarina      = LoadSprite(sp + "facility_marina.png");
        Sprite sprHotel       = LoadSprite(sp + "facility_hotel.png");
        Sprite sprLighthouse  = LoadSprite(sp + "facility_lighthouse.png");
        Sprite sprCasino      = LoadSprite(sp + "facility_casino.png");
        Sprite sprAquarium    = LoadSprite(sp + "facility_aquarium.png");
        Sprite sprSlotEmpty   = LoadSprite(sp + "slot_empty.png");

        Sprite sprWood  = LoadSprite(sp + "resource_wood.png");
        Sprite sprStone = LoadSprite(sp + "resource_stone.png");
        Sprite sprFood  = LoadSprite(sp + "resource_food.png");
        Sprite sprGold  = LoadSprite(sp + "resource_gold.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("IslandHopGameManager");
        var gm = gmObj.AddComponent<IslandHopGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f,  stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 1, complexityFactor = 0.3f,  stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.6f,  stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.8f, countMultiplier = 2, complexityFactor = 0.8f,  stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 1.0f,  stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // IslandManager (child of GM)
        var imObj = new GameObject("IslandManager");
        imObj.transform.SetParent(gmObj.transform);
        var islandMgr = imObj.AddComponent<IslandManager>();

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
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 50), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.5f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.5f);

        var targetTextGo = CT(canvasObj.transform, "TargetText", "目標: 50pt", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 44), new Vector2(0, -15));
        targetTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        targetTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.7f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 46), new Vector2(0, -68));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        // === Feedback text (center) ===
        var feedbackTextGo = CT(canvasObj.transform, "FeedbackText", "", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 70), new Vector2(0, 100));
        feedbackTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        feedbackTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        // === Guest request text ===
        var guestTextGo = CT(canvasObj.transform, "GuestRequestText", "", 34, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(700, 50), new Vector2(0, 220));
        guestTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        guestTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.7f, 0.3f);

        // === Weather warning text ===
        var weatherTextGo = CT(canvasObj.transform, "WeatherWarningText", "", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 55), new Vector2(0, 200));
        weatherTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        weatherTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.2f);

        // === No resource text ===
        var noResTextGo = CT(canvasObj.transform, "NoResourceText", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 50), new Vector2(0, 30));
        noResTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        noResTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        // === Resource display (bottom area) ===
        var resPanel = new GameObject("ResourcePanel", typeof(RectTransform));
        resPanel.transform.SetParent(canvasObj.transform, false);
        var resPanelRT = resPanel.GetComponent<RectTransform>();
        resPanelRT.anchorMin = new Vector2(0f, 0f); resPanelRT.anchorMax = new Vector2(1f, 0f);
        resPanelRT.pivot = new Vector2(0.5f, 0f);
        resPanelRT.sizeDelta = new Vector2(0, 55);
        resPanelRT.anchoredPosition = new Vector2(0, 70);
        var resPanelBg = resPanel.AddComponent<Image>();
        resPanelBg.color = new Color(0.05f, 0.15f, 0.35f, 0.85f);

        var woodTextGo  = CT(resPanel.transform, "WoodText",  "[木]0", 34, jpFont,
            new Vector2(0.1f, 0.5f), new Vector2(0.1f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(160, 44), Vector2.zero);
        woodTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        woodTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.7f, 0.4f);

        var stoneTextGo = CT(resPanel.transform, "StoneText", "[石]0", 34, jpFont,
            new Vector2(0.35f, 0.5f), new Vector2(0.35f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(160, 44), Vector2.zero);
        stoneTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stoneTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.9f);

        var foodTextGo  = CT(resPanel.transform, "FoodText",  "[食]0", 34, jpFont,
            new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(160, 44), Vector2.zero);
        foodTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        foodTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.6f, 0.3f);

        var goldTextGo  = CT(resPanel.transform, "GoldText",  "[金]0", 34, jpFont,
            new Vector2(0.88f, 0.5f), new Vector2(0.88f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(160, 44), Vector2.zero);
        goldTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goldTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        // === Build Panel ===
        var buildPanel = new GameObject("BuildPanel", typeof(RectTransform));
        buildPanel.transform.SetParent(canvasObj.transform, false);
        var buildPanelRT = buildPanel.GetComponent<RectTransform>();
        buildPanelRT.anchorMin = new Vector2(0.1f, 0.2f); buildPanelRT.anchorMax = new Vector2(0.9f, 0.65f);
        buildPanelRT.offsetMin = buildPanelRT.offsetMax = Vector2.zero;
        var buildPanelBg = buildPanel.AddComponent<Image>();
        buildPanelBg.color = new Color(0.05f, 0.15f, 0.3f, 0.96f);

        var buildTitleGo = CT(buildPanel.transform, "BuildTitle", "施設を選択", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 55), new Vector2(0, -5));
        buildTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        buildTitleGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var buildBtnContainer = new GameObject("BuildButtonContainer", typeof(RectTransform));
        buildBtnContainer.transform.SetParent(buildPanel.transform, false);
        var buildContRT = buildBtnContainer.GetComponent<RectTransform>();
        buildContRT.anchorMin = new Vector2(0f, 0f); buildContRT.anchorMax = new Vector2(1f, 0.85f);
        buildContRT.offsetMin = new Vector2(10, 5); buildContRT.offsetMax = new Vector2(-10, -5);
        var buildLayout = buildBtnContainer.AddComponent<GridLayoutGroup>();
        buildLayout.cellSize = new Vector2(200, 60);
        buildLayout.spacing = new Vector2(8, 8);
        buildLayout.childAlignment = TextAnchor.UpperCenter;

        // Build button prefab (just a template — won't be used as real prefab)
        var buildBtnPrefab = new GameObject("BuildButtonPrefab", typeof(RectTransform));
        buildBtnPrefab.transform.SetParent(buildPanel.transform, false);
        buildBtnPrefab.SetActive(false);
        var bbImg = buildBtnPrefab.AddComponent<Image>();
        bbImg.color = new Color(0.2f, 0.4f, 0.6f, 0.9f);
        buildBtnPrefab.AddComponent<Button>().targetGraphic = bbImg;
        var bbTextGo = new GameObject("Text", typeof(RectTransform));
        bbTextGo.transform.SetParent(buildBtnPrefab.transform, false);
        var bbTextRT = bbTextGo.GetComponent<RectTransform>();
        bbTextRT.anchorMin = Vector2.zero; bbTextRT.anchorMax = Vector2.one;
        bbTextRT.offsetMin = bbTextRT.offsetMax = Vector2.zero;
        var bbTMP = bbTextGo.AddComponent<TextMeshProUGUI>();
        bbTMP.fontSize = 28; bbTMP.alignment = TextAlignmentOptions.Center;
        if (jpFont != null) bbTMP.font = jpFont;
        bbTMP.color = Color.white;

        buildPanel.SetActive(false);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.15f, 0.35f); scRT.anchorMax = new Vector2(0.85f, 0.65f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scBg = scPanel.AddComponent<Image>();
        scBg.color = new Color(0.05f, 0.2f, 0.1f, 0.96f);

        var scTextGo = CT(scPanel.transform, "StageClearText", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 80), Vector2.zero);
        scTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f),
            new Vector2(360, 65), Vector2.zero, new Color(0.15f, 0.4f, 0.2f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.1f, 0.3f); acRT.anchorMax = new Vector2(0.9f, 0.7f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acBg = acPanel.AddComponent<Image>();
        acBg.color = new Color(0.05f, 0.15f, 0.35f, 0.97f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "全クリア！\nFinalScore: 0", 50, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 160), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var acBackBtn = CB(acPanel.transform, "BackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 65), Vector2.zero, new Color(0.2f, 0.35f, 0.5f));
        acBackBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.3f); goRT.anchorMax = new Vector2(0.9f, 0.7f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0.25f, 0.05f, 0.05f, 0.96f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "ゲームオーバー\nScore: 0", 50, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 160), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        var goBackBtn = CB(goPanel.transform, "BackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 65), Vector2.zero, new Color(0.4f, 0.15f, 0.15f));
        goBackBtn.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === Persistent bottom buttons ===
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 30, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 55), new Vector2(15, 15), new Color(0.15f, 0.25f, 0.4f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-15, 135), new Color(0.2f, 0.3f, 0.45f, 0.9f));

        // === IslandHopUI component ===
        var uiGo = new GameObject("IslandHopUI");
        var ui = uiGo.AddComponent<IslandHopUI>();

        SetField(ui, "_stageText",         stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_targetText",        targetTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_woodText",          woodTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stoneText",         stoneTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_foodText",          foodTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_goldText",          goldTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",   scNextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_buildPanel",        buildPanel);
        SetField(ui, "_buildButtonContainer", buildBtnContainer.transform);
        SetField(ui, "_buildButtonPrefab", buildBtnPrefab);
        SetField(ui, "_buildPanelTitleText", buildTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_feedbackText",      feedbackTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_guestRequestText",  guestTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_weatherWarningText", weatherTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_noResourceText",    noResTextGo.GetComponent<TextMeshProUGUI>());

        // Next stage button onclick
        var nextStageButton = scNextBtn.GetComponent<Button>();
        nextStageButton.onClick.AddListener(() => gm.NextStage());

        // === Wire IslandManager ===
        SetField(islandMgr, "_gameManager",      gm);
        SetField(islandMgr, "_ui",               ui);
        SetField(islandMgr, "_sprIslandForest",   sprIslandForest);
        SetField(islandMgr, "_sprIslandRocky",    sprIslandRocky);
        SetField(islandMgr, "_sprIslandTropical", sprIslandTropical);
        SetField(islandMgr, "_sprIslandVolcanic", sprIslandVolcanic);
        SetField(islandMgr, "_sprIslandCoral",    sprIslandCoral);
        SetField(islandMgr, "_sprCottage",        sprCottage);
        SetField(islandMgr, "_sprPier",           sprPier);
        SetField(islandMgr, "_sprGarden",         sprGarden);
        SetField(islandMgr, "_sprRestaurant",     sprRestaurant);
        SetField(islandMgr, "_sprObservation",    sprObservation);
        SetField(islandMgr, "_sprSpa",            sprSpa);
        SetField(islandMgr, "_sprMarina",         sprMarina);
        SetField(islandMgr, "_sprHotel",          sprHotel);
        SetField(islandMgr, "_sprLighthouse",     sprLighthouse);
        SetField(islandMgr, "_sprCasino",         sprCasino);
        SetField(islandMgr, "_sprAquarium",       sprAquarium);
        SetField(islandMgr, "_sprSlotEmpty",      sprSlotEmpty);
        SetField(islandMgr, "_sprWood",           sprWood);
        SetField(islandMgr, "_sprStone",          sprStone);
        SetField(islandMgr, "_sprFood",           sprFood);
        SetField(islandMgr, "_sprGold",           sprGold);

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
        ipBgImg.color = new Color(0.05f, 0.15f, 0.35f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "IslandHop", 68, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.85f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.85f, 0.9f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 70), Vector2.zero, new Color(0.2f, 0.4f, 0.6f));

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire GameManager
        SetField(gm, "_stageManager",    sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_islandManager",   islandMgr);
        SetField(gm, "_ui",              ui);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/089v2_IslandHop.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup089v2] IslandHop シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup089v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
