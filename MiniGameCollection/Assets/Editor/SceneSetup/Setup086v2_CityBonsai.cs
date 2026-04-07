using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game086v2_CityBonsai;

public static class Setup086v2_CityBonsai
{
    [MenuItem("Assets/Setup/086v2 CityBonsai")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup086v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game086v2_CityBonsai/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.30f, 0.22f, 0.18f);
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
        Sprite trunkSprite = LoadSprite(sp + "trunk.png");
        Sprite emptySlotSprite = LoadSprite(sp + "branch_empty.png");
        Sprite houseSprite = LoadSprite(sp + "house.png");
        Sprite shopSprite = LoadSprite(sp + "shop.png");
        Sprite publicSprite = LoadSprite(sp + "public.png");
        Sprite shrineSprite = LoadSprite(sp + "shrine.png");
        Sprite parkSprite = LoadSprite(sp + "park.png");
        Sprite flowerSprite = LoadSprite(sp + "flower.png");

        Sprite[] buildingSprites = { houseSprite, shopSprite, publicSprite, shrineSprite, parkSprite };

        // === GameManager hierarchy ===
        var gmObj = new GameObject("CityBonsaiGameManager");
        var gm = gmObj.AddComponent<CityBonsaiGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 3, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 4, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 5, complexityFactor = 1.0f },
        };
        sm.SetConfigs(stageConfigs);

        // BonsaiManager
        var bmObj = new GameObject("CityBonsaiManager");
        bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<CityBonsaiManager>();

        // Trunk
        var trunkObj = new GameObject("Trunk");
        trunkObj.transform.SetParent(bmObj.transform);
        var trunkSR = trunkObj.AddComponent<SpriteRenderer>();
        trunkSR.sprite = trunkSprite;
        trunkSR.sortingOrder = 0;
        trunkObj.transform.localPosition = new Vector3(0, 0.9f, 0);
        trunkObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        // Branch Slots (max 12)
        const int maxSlots = 12;
        var slotRenderers = new SpriteRenderer[maxSlots];
        var slotColliders = new BoxCollider2D[maxSlots];

        float camSz = 6f;
        float centerY = camSz * 0.15f;
        float radius = camSz * 0.45f;
        float slotScale = Mathf.Min(camSz * 0.15f, 0.8f);

        for (int i = 0; i < maxSlots; i++)
        {
            float angle = (360f / maxSlots) * i - 90f;
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius + centerY;

            var slotObj = new GameObject($"Slot_{i:D2}");
            slotObj.transform.SetParent(bmObj.transform);
            slotObj.transform.localPosition = new Vector3(x, y, 0);
            slotObj.transform.localScale = new Vector3(slotScale, slotScale, 1f);

            var sr = slotObj.AddComponent<SpriteRenderer>();
            sr.sprite = emptySlotSprite;
            sr.sortingOrder = 1;
            slotRenderers[i] = sr;

            var col = slotObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.2f);
            slotColliders[i] = col;
        }

        // Wire BonsaiManager
        SetField(bm, "_gameManager", gm);
        SetField(bm, "_trunkRenderer", trunkSR);
        SetField(bm, "_slotRenderers", slotRenderers);
        SetField(bm, "_slotColliders", slotColliders);
        SetField(bm, "_buildingSprites", buildingSprites);
        SetField(bm, "_flowerSprite", flowerSprite);
        SetField(bm, "_emptySlotSprite", emptySlotSprite);

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
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 50), new Vector2(15, -15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.6f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.6f);

        var popText = CT(canvasObj.transform, "PopulationText", "人口: 0/8", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280, 45), new Vector2(15, -60));
        popText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var beautyText = CT(canvasObj.transform, "BeautyText", "美しさ: 10/30", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 45), new Vector2(-15, -60));
        beautyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        beautyText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var satText = CT(canvasObj.transform, "SatisfactionText", "満足度: 50%", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(260, 42), new Vector2(15, -105));
        satText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var turnText = CT(canvasObj.transform, "TurnText", "ターン: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220, 42), new Vector2(-15, -105));
        turnText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        turnText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var seasonText = CT(canvasObj.transform, "SeasonText", "", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(200, 40), new Vector2(0, -105));
        seasonText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        seasonText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 0.7f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 45), new Vector2(0, -150));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var demandText = CT(canvasObj.transform, "DemandText", "", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 42), new Vector2(0, -190));
        demandText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        demandText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.3f);
        demandText.SetActive(false);

        var messageText = CT(canvasObj.transform, "MessageText", "", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), new Vector2(0, 150));
        messageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        messageText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.2f);
        messageText.SetActive(false);

        // === Building Buttons (bottom area, horizontal) ===
        string[] buildNames = { "住宅", "商業", "公共", "神社", "公園" };
        Color[] buildColors = {
            new Color(0.26f, 0.65f, 0.96f),  // House blue
            new Color(1f, 0.65f, 0.15f),      // Shop orange
            new Color(0.4f, 0.73f, 0.42f),    // Public green
            new Color(0.94f, 0.33f, 0.31f),   // Shrine red
            new Color(0.16f, 0.71f, 0.96f)    // Park cyan
        };
        float[] btnXPositions = { -420f, -210f, 0f, 210f, 420f };

        var buildingButtons = new Button[5];
        var buildingButtonTexts = new TextMeshProUGUI[5];
        var buildingButtonImages = new Image[5];

        for (int i = 0; i < 5; i++)
        {
            var btn = CB(canvasObj.transform, $"Build{buildNames[i]}Btn", buildNames[i], 32, jpFont,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(180, 65), new Vector2(btnXPositions[i], 310), buildColors[i]);
            buildingButtons[i] = btn.GetComponent<Button>();
            buildingButtonImages[i] = btn.GetComponent<Image>();
            buildingButtonTexts[i] = btn.GetComponentInChildren<TextMeshProUGUI>();

            int capturedIdx = i;
            buildingButtons[i].onClick.AddListener(() => bm.SelectBuilding(capturedIdx));
        }

        // Prune + Turn buttons
        var pruneBtn = CB(canvasObj.transform, "PruneButton", "剪定", 36, jpFont,
            new Vector2(0.3f, 0f), new Vector2(0.3f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 70), new Vector2(0, 210), new Color(0.6f, 0.3f, 0.15f));
        pruneBtn.GetComponent<Button>().onClick.AddListener(() => bm.TogglePruneMode());

        var turnBtn = CB(canvasObj.transform, "TurnButton", "次のターン", 36, jpFont,
            new Vector2(0.7f, 0f), new Vector2(0.7f, 0f), new Vector2(0.5f, 0f),
            new Vector2(240, 70), new Vector2(0, 210), new Color(0.3f, 0.5f, 0.35f));
        turnBtn.GetComponent<Button>().onClick.AddListener(() => bm.AdvanceTurn());

        // Back button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 34, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(240, 55), new Vector2(0, 15), new Color(0.3f, 0.22f, 0.18f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        // === UI Component ===
        var uiObj = new GameObject("CityBonsaiUI");
        var ui = uiObj.AddComponent<CityBonsaiUI>();

        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText", scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_populationText", popText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_beautyText", beautyText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_satisfactionText", satText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_turnText", turnText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_seasonText", seasonText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_demandText", demandText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_messageText", messageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_buildingButtons", buildingButtons);
        SetField(ui, "_buildingButtonTexts", buildingButtonTexts);
        SetField(ui, "_buildingButtonImages", buildingButtonImages);
        SetField(ui, "_pruneButton", pruneBtn.GetComponent<Button>());
        SetField(ui, "_pruneButtonImage", pruneBtn.GetComponent<Image>());
        SetField(ui, "_turnButton", turnBtn.GetComponent<Button>());

        // Wire BonsaiManager UI ref
        SetField(bm, "_ui", ui);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 340);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.20f, 0.15f, 0.10f, 0.97f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(640, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.4f);

        var scScore = CT(scPanel.transform, "SCScore", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 60), new Vector2(0, 20));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.7f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(400, 65), new Vector2(0, 50), new Color(0.45f, 0.35f, 0.25f));
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        scPanel.SetActive(false);

        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearScoreText", scScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton", nextBtn.GetComponent<Button>());

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 380);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.15f, 0.12f, 0.08f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "盆栽都市\nコンプリート！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 120), new Vector2(0, -30));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 46, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.7f);

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 60), new Vector2(0, 50), new Color(0.35f, 0.25f, 0.18f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        SetField(ui, "_allClearPanel", acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 340);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.25f, 0.08f, 0.05f, 0.97f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 60), Vector2.zero, new Color(0.4f, 0.15f, 0.1f));
        goBack.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        SetField(ui, "_gameOverPanel", goPanel);

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
        ipBgImg.color = new Color(0.15f, 0.10f, 0.08f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "CityBonsai", 68, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.75f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.75f, 0.65f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 70), Vector2.zero, new Color(0.45f, 0.35f, 0.25f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 42, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-15, 400), new Color(0.3f, 0.22f, 0.18f, 0.9f));

        // Wire InstructionPanel
        SetField(ip, "_titleText", ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText", ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText", ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton", startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton", helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot", ipBg);

        // Wire GameManager
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_bonsaiManager", bm);
        SetField(gm, "_ui", ui);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/086v2_CityBonsai.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup086v2] CityBonsai シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup086v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
