using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game048v2_GlassBall;

public static class Setup048v2_GlassBall
{
    [MenuItem("Assets/Setup/048v2 GlassBall")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup048v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game048v2_GlassBall/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite paths exist and import
        string[] spritePaths = {
            sp+"Background.png", sp+"GlassBall.png", sp+"Goal.png",
            sp+"Nail.png", sp+"Hammer.png", sp+"Coin.png",
            sp+"ThinIce.png", sp+"WindEffect.png", sp+"CrackEffect.png", sp+"StartArea.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg     = LoadSprite(sp + "Background.png");
        Sprite spBall   = LoadSprite(sp + "GlassBall.png");
        Sprite spGoal   = LoadSprite(sp + "Goal.png");
        Sprite spNail   = LoadSprite(sp + "Nail.png");
        Sprite spHammer = LoadSprite(sp + "Hammer.png");
        Sprite spCoin   = LoadSprite(sp + "Coin.png");
        Sprite spThinIce= LoadSprite(sp + "ThinIce.png");
        Sprite spStart  = LoadSprite(sp + "StartArea.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camWidth = camera != null ? camSize * camera.aspect : camSize * 0.5625f;

        // --- Background ---
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camWidth * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // --- Goal area (bottom center) ---
        // Game area: top y=3.5, bottom y=-2.0, UI buttons occupy y < -2.5
        float goalY = -1.8f;
        var goalObj = new GameObject("Goal");
        goalObj.tag = "Goal";
        var goalSr = goalObj.AddComponent<SpriteRenderer>();
        if (spGoal != null) goalSr.sprite = spGoal;
        goalSr.color = new Color(0.3f, 1f, 0.3f, 0.8f);
        goalSr.sortingOrder = 1;
        goalObj.transform.position = new Vector3(0f, goalY, 0f);
        if (spGoal != null)
        {
            float gw = 1.5f;
            float gh = 1.5f;
            float pw = spGoal.rect.width / spGoal.pixelsPerUnit;
            float ph = spGoal.rect.height / spGoal.pixelsPerUnit;
            goalObj.transform.localScale = new Vector3(gw / pw, gh / ph, 1f);
        }
        var goalCol = goalObj.AddComponent<CircleCollider2D>();
        goalCol.isTrigger = true;
        goalCol.radius = 0.6f;

        // --- Start Area marker ---
        var startAreaObj = new GameObject("StartArea");
        var startSr = startAreaObj.AddComponent<SpriteRenderer>();
        if (spStart != null) startSr.sprite = spStart;
        startSr.sortingOrder = 1;
        startAreaObj.transform.position = new Vector3(0f, 3.2f, 0f);
        if (spStart != null)
        {
            float pw = spStart.rect.width / spStart.pixelsPerUnit;
            float ph = spStart.rect.height / spStart.pixelsPerUnit;
            startAreaObj.transform.localScale = new Vector3(2f / pw, 0.6f / ph, 1f);
        }

        // --- GlassBall ---
        var ballObj = new GameObject("GlassBall");
        var ballSr = ballObj.AddComponent<SpriteRenderer>();
        if (spBall != null) ballSr.sprite = spBall;
        ballSr.color = new Color(0.7f, 0.9f, 1f, 0.85f);
        ballSr.sortingOrder = 5;
        ballObj.transform.position = new Vector3(0f, 3.0f, 0f);
        if (spBall != null)
        {
            float radius = 0.35f;
            float pw = spBall.rect.width / spBall.pixelsPerUnit;
            float ph = spBall.rect.height / spBall.pixelsPerUnit;
            ballObj.transform.localScale = new Vector3(radius * 2f / pw, radius * 2f / ph, 1f);
        }
        var ballRb = ballObj.AddComponent<Rigidbody2D>();
        ballRb.gravityScale = 0f;
        ballRb.freezeRotation = true;
        var ballCol = ballObj.AddComponent<CircleCollider2D>();
        ballCol.radius = 0.35f;
        var ballCtrl = ballObj.AddComponent<GlassBallController>();

        // Note: full GlassBallController wiring done after UI is set up below

        // --- Stage 2: 3 Nails ---
        var nailsParent = new GameObject("Nails_Stage2");
        nailsParent.SetActive(false); // activated by SetupStage
        CreateNail(nailsParent.transform, spNail, new Vector3(-1.5f, 0.5f, 0f));
        CreateNail(nailsParent.transform, spNail, new Vector3(1.2f, 1.0f, 0f));
        CreateNail(nailsParent.transform, spNail, new Vector3(0f, -0.5f, 0f));

        // --- Stage 4+: Hammer ---
        var hammerObj = new GameObject("Hammer");
        hammerObj.SetActive(false);
        var hammerSr2 = hammerObj.AddComponent<SpriteRenderer>();
        if (spHammer != null) hammerSr2.sprite = spHammer;
        hammerSr2.sortingOrder = 3;
        hammerObj.transform.position = new Vector3(1.8f, 0.2f, 0f);
        if (spHammer != null)
        {
            float pw = spHammer.rect.width / spHammer.pixelsPerUnit;
            float ph = spHammer.rect.height / spHammer.pixelsPerUnit;
            hammerObj.transform.localScale = new Vector3(1.2f / pw, 1.2f / ph, 1f);
        }
        var hammerCol = hammerObj.AddComponent<BoxCollider2D>();
        hammerCol.size = new Vector2(1.0f, 0.4f);

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GlassBallGameManager>();

        // StageManager (child)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // RailManager (child of GameManager, with LineRenderer)
        var railObj = new GameObject("RailManager");
        railObj.transform.SetParent(gmObj.transform);
        var railMgr = railObj.AddComponent<RailManager>();
        var lineRenderer = railObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.12f;
        lineRenderer.endWidth = 0.12f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0.3f, 0.6f, 1f, 0.9f);
        lineRenderer.endColor = new Color(0.1f, 0.4f, 0.9f, 0.7f);
        lineRenderer.sortingOrder = 10;

        // === Canvas (UI) ===
        var canvasUIObj = new GameObject("Canvas");
        var canvasUI = canvasUIObj.AddComponent<Canvas>();
        canvasUI.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasUI.sortingOrder = 10;
        var scaler = canvasUIObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasUIObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === InstructionPanel ===
        var ip = CreateInstructionPanel(canvasUIObj.transform, jpFont);

        // === GlassBallUI on Canvas ===
        var uiComp = canvasUIObj.AddComponent<GlassBallUI>();

        // --- HUD: Stage text (top left) ---
        var stageText = CT(canvasUIObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(260, 50), new Vector2(10, -15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.4f, 0.8f);

        // --- HUD: Score (top right) ---
        var scoreText = CT(canvasUIObj.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(260, 50), new Vector2(-10, -15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.5f);

        // --- HUD: Coin (top center) ---
        var coinText = CT(canvasUIObj.transform, "CoinText", "", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(200, 50), new Vector2(0, -15));
        coinText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        coinText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 0f);

        // --- Impact Slider (top, below stage text) ---
        var impactSliderObj = new GameObject("ImpactSlider");
        impactSliderObj.transform.SetParent(canvasUIObj.transform, false);
        var impactRt = impactSliderObj.AddComponent<RectTransform>();
        impactRt.anchorMin = new Vector2(0f, 1f);
        impactRt.anchorMax = new Vector2(1f, 1f);
        impactRt.pivot = new Vector2(0.5f, 1f);
        impactRt.sizeDelta = new Vector2(-20, 30);
        impactRt.anchoredPosition = new Vector2(0, -65);
        var impactSlider = impactSliderObj.AddComponent<Slider>();
        impactSlider.value = 0f;
        // Background
        var impactBgObj = new GameObject("Background");
        impactBgObj.transform.SetParent(impactSliderObj.transform, false);
        var impactBgRt = impactBgObj.AddComponent<RectTransform>();
        impactBgRt.anchorMin = Vector2.zero; impactBgRt.anchorMax = Vector2.one;
        impactBgRt.offsetMin = Vector2.zero; impactBgRt.offsetMax = Vector2.zero;
        var impactBgImg = impactBgObj.AddComponent<Image>();
        impactBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        impactSlider.targetGraphic = impactBgImg;
        // Fill Area
        var fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(impactSliderObj.transform, false);
        var fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = Vector2.zero; fillAreaRt.offsetMax = Vector2.zero;
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        var fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        fillRt.sizeDelta = new Vector2(0, 0);
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f);
        impactSlider.fillRect = fillRt;

        // Impact label
        var impactLabelObj = CT(canvasUIObj.transform, "ImpactLabel", "衝撃", 24, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(80, 30), new Vector2(10, -62));
        impactLabelObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.2f, 0.1f);

        // --- Ink Slider (right side) ---
        var inkSliderObj = new GameObject("InkSlider");
        inkSliderObj.transform.SetParent(canvasUIObj.transform, false);
        var inkRt = inkSliderObj.AddComponent<RectTransform>();
        inkRt.anchorMin = new Vector2(1f, 0.5f);
        inkRt.anchorMax = new Vector2(1f, 0.5f);
        inkRt.pivot = new Vector2(1f, 0.5f);
        inkRt.sizeDelta = new Vector2(30, 400);
        inkRt.anchoredPosition = new Vector2(-10, 0);
        var inkSlider = inkSliderObj.AddComponent<Slider>();
        inkSlider.direction = Slider.Direction.BottomToTop;
        inkSlider.value = 1f;
        // Background
        var inkBgObj = new GameObject("Background");
        inkBgObj.transform.SetParent(inkSliderObj.transform, false);
        var inkBgRt = inkBgObj.AddComponent<RectTransform>();
        inkBgRt.anchorMin = Vector2.zero; inkBgRt.anchorMax = Vector2.one;
        inkBgRt.offsetMin = Vector2.zero; inkBgRt.offsetMax = Vector2.zero;
        var inkBgImg = inkBgObj.AddComponent<Image>();
        inkBgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
        inkSlider.targetGraphic = inkBgImg;
        // Fill Area
        var inkFillAreaObj = new GameObject("Fill Area");
        inkFillAreaObj.transform.SetParent(inkSliderObj.transform, false);
        var inkFillAreaRt = inkFillAreaObj.AddComponent<RectTransform>();
        inkFillAreaRt.anchorMin = Vector2.zero; inkFillAreaRt.anchorMax = Vector2.one;
        inkFillAreaRt.offsetMin = Vector2.zero; inkFillAreaRt.offsetMax = Vector2.zero;
        var inkFillObj = new GameObject("Fill");
        inkFillObj.transform.SetParent(inkFillAreaObj.transform, false);
        var inkFillRt = inkFillObj.AddComponent<RectTransform>();
        inkFillRt.anchorMin = Vector2.zero; inkFillRt.anchorMax = new Vector2(1f, 0f);
        inkFillRt.offsetMin = Vector2.zero; inkFillRt.offsetMax = Vector2.zero;
        inkFillRt.sizeDelta = new Vector2(0, 0);
        var inkFillImg = inkFillObj.AddComponent<Image>();
        inkFillImg.color = new Color(0.2f, 0.5f, 1f);
        inkSlider.fillRect = inkFillRt;

        // Ink label
        var inkLabelObj = CT(canvasUIObj.transform, "InkLabel", "インク", 22, jpFont,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(70, 30), new Vector2(-45, 210));
        inkLabelObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.4f, 0.9f);

        // --- Bottom buttons ---
        // Launch button (right bottom)
        var launchBtnObj = CB(canvasUIObj.transform, "LaunchButton", "発射", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(160, 60), new Vector2(-55, 75), new Color(0.1f, 0.6f, 0.9f));

        // Clear Rail button (left bottom)
        var clearBtnObj = CB(canvasUIObj.transform, "ClearRailButton", "リセット", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(160, 60), new Vector2(55, 75), new Color(0.7f, 0.4f, 0.1f));

        // Menu button (center bottom)
        var menuBtnObj = CB(canvasUIObj.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(160, 55), new Vector2(0, 15), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtnObj.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = CreatePanel(canvasUIObj.transform, "StageClearPanel", new Color(0f, 0.1f, 0.3f, 0.88f), new Vector2(700, 500));
        scPanel.SetActive(false);
        var scTitle = CT(scPanel.transform, "Title", "ステージクリア！", 54, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), new Vector2(0, 170));
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var scScore = CT(scPanel.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), new Vector2(0, 70));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var nextBtnObj = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 65), new Vector2(0, -60), new Color(0.1f, 0.6f, 0.2f));

        // === All Clear Panel ===
        var acPanel = CreatePanel(canvasUIObj.transform, "AllClearPanel", new Color(0.05f, 0f, 0.2f, 0.92f), new Vector2(700, 500));
        acPanel.SetActive(false);
        var acTitle = CT(acPanel.transform, "Title", "全ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620, 70), new Vector2(0, 170));
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var acScore = CT(acPanel.transform, "ScoreText", "Total Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), new Vector2(0, 70));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var acMenuBtn = CB(acPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), new Vector2(0, -60), new Color(0.3f, 0.3f, 0.5f));
        acMenuBtn.AddComponent<BackToMenuButton>();

        // === Game Over Panel ===
        var goPanel = CreatePanel(canvasUIObj.transform, "GameOverPanel", new Color(0.2f, 0f, 0f, 0.92f), new Vector2(700, 500));
        goPanel.SetActive(false);
        var goTitle = CT(goPanel.transform, "Title", "ガラスが割れた！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620, 70), new Vector2(0, 170));
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.1f);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScore = CT(goPanel.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), new Vector2(0, 70));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(180, 65), new Vector2(-100, -60), new Color(0.1f, 0.5f, 0.9f));
        var goMenuBtn2 = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(180, 65), new Vector2(100, -60), new Color(0.3f, 0.3f, 0.5f));
        goMenuBtn2.AddComponent<BackToMenuButton>();

        // === Wire GlassBallUI ===
        var uiSO = new SerializedObject(uiComp);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_coinText").objectReferenceValue = coinText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_impactSlider").objectReferenceValue = impactSlider;
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider;
        uiSO.FindProperty("_impactFill").objectReferenceValue = fillImg;
        uiSO.FindProperty("_launchButton").objectReferenceValue = launchBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_clearRailButton").objectReferenceValue = clearBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // === Wire RailManager ===
        var railSO = new SerializedObject(railMgr);
        railSO.FindProperty("_gameManager").objectReferenceValue = gm;
        railSO.FindProperty("_ballController").objectReferenceValue = ballCtrl;
        railSO.FindProperty("_ui").objectReferenceValue = uiComp;
        railSO.FindProperty("_lineRenderer").objectReferenceValue = lineRenderer;
        railSO.ApplyModifiedProperties();

        // === Wire GlassBallController ===
        var ballSO2 = new SerializedObject(ballCtrl);
        ballSO2.FindProperty("_gameManager").objectReferenceValue = gm;
        ballSO2.FindProperty("_ui").objectReferenceValue = uiComp;
        ballSO2.FindProperty("_spriteRenderer").objectReferenceValue = ballSr;
        ballSO2.FindProperty("_railManager").objectReferenceValue = railMgr;
        ballSO2.ApplyModifiedProperties();

        // === Wire GameManager ===
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_railManager").objectReferenceValue = railMgr;
        gmSO.FindProperty("_ballController").objectReferenceValue = ballCtrl;
        gmSO.FindProperty("_ui").objectReferenceValue = uiComp;
        gmSO.ApplyModifiedProperties();

        // === Button listeners ===
        launchBtnObj.GetComponent<Button>().onClick.AddListener(() => railMgr.LaunchBall());
        clearBtnObj.GetComponent<Button>().onClick.AddListener(() => railMgr.ClearRail());
        nextBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.GoNextStage());
        goRetryBtn.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());

        // === StageManager: 5 stages ===
        var stageSO = new SerializedObject(stageMgr);
        var stagesProp = stageSO.FindProperty("_configs");
        stagesProp.arraySize = 5;
        // Stage 1: basic
        SetStage(stagesProp.GetArrayElementAtIndex(0), 1.0f, 1.0f, 1.0f, 30f);
        // Stage 2: nails, coins
        SetStage(stagesProp.GetArrayElementAtIndex(1), 1.0f, 1.0f, 1.5f, 30f);
        // Stage 3: slope acceleration
        SetStage(stagesProp.GetArrayElementAtIndex(2), 1.2f, 1.3f, 2.0f, 30f);
        // Stage 4: moving hammer
        SetStage(stagesProp.GetArrayElementAtIndex(3), 1.3f, 1.4f, 2.5f, 30f);
        // Stage 5: wind + thin ice
        SetStage(stagesProp.GetArrayElementAtIndex(4), 1.5f, 1.5f, 3.0f, 30f);
        stageSO.ApplyModifiedProperties();

        // === Save scene ===
        string scenePath = "Assets/Scenes/048v2_GlassBall.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup048v2] シーン作成完了: " + scenePath);
    }

    static void SetStage(SerializedProperty prop, float speed, float complexity, float impact, float duration)
    {
        prop.FindPropertyRelative("speedMultiplier").floatValue = speed;
        prop.FindPropertyRelative("complexityFactor").floatValue = complexity;
        prop.FindPropertyRelative("countMultiplier").floatValue = impact;
        prop.FindPropertyRelative("timeLimit").floatValue = duration;
    }

    static void CreateNail(Transform parent, Sprite sprite, Vector3 position)
    {
        var nailObj = new GameObject("Nail");
        nailObj.transform.SetParent(parent);
        nailObj.transform.position = position;
        var sr = nailObj.AddComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sr.sprite = sprite;
            float w = 0.25f; float h = 0.5f;
            float pw = sprite.rect.width / sprite.pixelsPerUnit;
            float ph = sprite.rect.height / sprite.pixelsPerUnit;
            nailObj.transform.localScale = new Vector3(w / pw, h / ph, 1f);
        }
        sr.sortingOrder = 3;
        var col = nailObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.2f, 0.4f);
        // NailImpact tag
        // No custom tag needed; OnCollisionEnter2D on ball handles all collisions
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasTransform, TMP_FontAsset font)
    {
        var ipObj = new GameObject("InstructionPanel");
        ipObj.transform.SetParent(canvasTransform, false);
        var ip = ipObj.AddComponent<InstructionPanel>();
        var ipCanvas = ipObj.AddComponent<Canvas>();
        ipCanvas.overrideSorting = true;
        ipCanvas.sortingOrder = 100;
        ipObj.AddComponent<GraphicRaycaster>();

        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(ipObj.transform, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.2f, 0.95f);

        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(800, 80);
        titleRt.anchoredPosition = new Vector2(0, 200);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.font = font;
        titleTmp.fontSize = 56;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(0.5f, 0.9f, 1f);

        var descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(panelObj.transform, false);
        var descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f);
        descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(800, 60);
        descRt.anchoredPosition = new Vector2(0, 110);
        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.font = font;
        descTmp.fontSize = 34;
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = Color.white;
        descTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = new GameObject("ControlsText");
        ctrlObj.transform.SetParent(panelObj.transform, false);
        var ctrlRt = ctrlObj.AddComponent<RectTransform>();
        ctrlRt.anchorMin = new Vector2(0.5f, 0.5f);
        ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRt.sizeDelta = new Vector2(800, 90);
        ctrlRt.anchoredPosition = new Vector2(0, 0);
        var ctrlTmp = ctrlObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.font = font;
        ctrlTmp.fontSize = 28;
        ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.9f, 0.9f, 0.7f);
        ctrlTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(panelObj.transform, false);
        var goalRt = goalObj.AddComponent<RectTransform>();
        goalRt.anchorMin = new Vector2(0.5f, 0.5f);
        goalRt.anchorMax = new Vector2(0.5f, 0.5f);
        goalRt.sizeDelta = new Vector2(800, 60);
        goalRt.anchoredPosition = new Vector2(0, -90);
        var goalTmp = goalObj.AddComponent<TextMeshProUGUI>();
        goalTmp.font = font;
        goalTmp.fontSize = 30;
        goalTmp.alignment = TextAlignmentOptions.Center;
        goalTmp.color = new Color(1f, 0.85f, 0.3f);
        goalTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(panelObj.transform, false);
        var startBtnRt = startBtnObj.AddComponent<RectTransform>();
        startBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        startBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRt.sizeDelta = new Vector2(260, 70);
        startBtnRt.anchoredPosition = new Vector2(0, -200);
        var startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0.1f, 0.5f, 0.8f);
        var startBtn = startBtnObj.AddComponent<Button>();
        var startLabelObj = new GameObject("Label");
        startLabelObj.transform.SetParent(startBtnObj.transform, false);
        var startLabelRt = startLabelObj.AddComponent<RectTransform>();
        startLabelRt.anchorMin = Vector2.zero;
        startLabelRt.anchorMax = Vector2.one;
        startLabelRt.offsetMin = Vector2.zero;
        startLabelRt.offsetMax = Vector2.zero;
        var startLabelTmp = startLabelObj.AddComponent<TextMeshProUGUI>();
        startLabelTmp.font = font;
        startLabelTmp.text = "はじめる";
        startLabelTmp.fontSize = 38;
        startLabelTmp.alignment = TextAlignmentOptions.Center;
        startLabelTmp.color = Color.white;

        var qBtnObj = new GameObject("QuestionButton");
        qBtnObj.transform.SetParent(canvasTransform, false);
        var qBtnRt = qBtnObj.AddComponent<RectTransform>();
        qBtnRt.anchorMin = new Vector2(1f, 0f);
        qBtnRt.anchorMax = new Vector2(1f, 0f);
        qBtnRt.pivot = new Vector2(1f, 0f);
        qBtnRt.sizeDelta = new Vector2(70, 70);
        qBtnRt.anchoredPosition = new Vector2(-10, 10);
        var qBtnImg = qBtnObj.AddComponent<Image>();
        qBtnImg.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        var qBtn = qBtnObj.AddComponent<Button>();
        var qLabelObj = new GameObject("Label");
        qLabelObj.transform.SetParent(qBtnObj.transform, false);
        var qLabelRt = qLabelObj.AddComponent<RectTransform>();
        qLabelRt.anchorMin = Vector2.zero;
        qLabelRt.anchorMax = Vector2.one;
        qLabelRt.offsetMin = Vector2.zero;
        qLabelRt.offsetMax = Vector2.zero;
        var qLabelTmp = qLabelObj.AddComponent<TextMeshProUGUI>();
        qLabelTmp.font = font;
        qLabelTmp.text = "?";
        qLabelTmp.fontSize = 40;
        qLabelTmp.alignment = TextAlignmentOptions.Center;
        qLabelTmp.color = Color.white;
        qBtn.onClick.AddListener(() => panelObj.SetActive(true));

        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = panelObj;
        ipSO.FindProperty("_titleText").objectReferenceValue = titleTmp;
        ipSO.FindProperty("_descriptionText").objectReferenceValue = descTmp;
        ipSO.FindProperty("_controlsText").objectReferenceValue = ctrlTmp;
        ipSO.FindProperty("_goalText").objectReferenceValue = goalTmp;
        ipSO.FindProperty("_startButton").objectReferenceValue = startBtn;
        ipSO.FindProperty("_helpButton").objectReferenceValue = qBtn;
        ipSO.ApplyModifiedProperties();

        return ip;
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        return obj;
    }

    static GameObject CB(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color bgColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        obj.AddComponent<Button>();

        if (!string.IsNullOrEmpty(label))
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = label;
            tmp.fontSize = 30;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
        return obj;
    }

    static GameObject CreatePanel(Transform parent, string name, Color bgColor, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        return obj;
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
        Debug.Log($"[Setup048v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
