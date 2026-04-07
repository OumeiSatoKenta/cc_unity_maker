using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game047v2_SpinBalance;

public static class Setup047v2_SpinBalance
{
    [MenuItem("Assets/Setup/047v2 SpinBalance")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup047v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game047v2_SpinBalance/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.78f, 0.92f, 0.78f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Platform.png",
            sp+"Coin.png", sp+"HeavyCoin.png", sp+"LightCoin.png",
            sp+"BounceCoin.png", sp+"MagnetCoin.png",
            sp+"BrakeIcon.png", sp+"DangerEffect.png", sp+"PerfectEffect.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spPlatform = LoadSprite(sp + "Platform.png");
        Sprite spCoin     = LoadSprite(sp + "Coin.png");
        Sprite spHeavy    = LoadSprite(sp + "HeavyCoin.png");
        Sprite spLight    = LoadSprite(sp + "LightCoin.png");
        Sprite spBounce   = LoadSprite(sp + "BounceCoin.png");
        Sprite spMagnet   = LoadSprite(sp + "MagnetCoin.png");
        Sprite spBrake    = LoadSprite(sp + "BrakeIcon.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camWidth = camera != null ? camSize * camera.aspect : camSize * 0.5625f;

        // Background
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

        // === Platform ===
        var platformObj = new GameObject("Platform");
        var platformSr = platformObj.AddComponent<SpriteRenderer>();
        platformSr.sprite = spPlatform;
        platformSr.sortingOrder = 2;
        // Platform width = 6u, height = 0.3u
        float platformWidth = 6f;
        float platformHeight = 0.3f;
        if (spPlatform != null)
        {
            float pw = spPlatform.rect.width / spPlatform.pixelsPerUnit;
            float ph = spPlatform.rect.height / spPlatform.pixelsPerUnit;
            platformObj.transform.localScale = new Vector3(platformWidth / pw, platformHeight / ph, 1f);
        }
        else
        {
            platformObj.transform.localScale = new Vector3(platformWidth, platformHeight, 1f);
        }
        platformObj.transform.position = new Vector3(0f, 0f, 0f);

        var platformRb = platformObj.AddComponent<Rigidbody2D>();
        platformRb.bodyType = RigidbodyType2D.Kinematic;
        platformRb.constraints = RigidbodyConstraints2D.FreezePosition;

        var platformCol = platformObj.AddComponent<BoxCollider2D>();
        platformCol.size = new Vector2(1f / (platformObj.transform.localScale.x != 0 ? platformObj.transform.localScale.x : 1f) * platformWidth,
                                       1f / (platformObj.transform.localScale.y != 0 ? platformObj.transform.localScale.y : 1f) * platformHeight);

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SpinBalanceGameManager>();

        // StageManager (child)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // BalanceManager (child)
        var bmObj = new GameObject("BalanceManager");
        bmObj.transform.SetParent(gmObj.transform);
        var balanceMgr = bmObj.AddComponent<BalanceManager>();

        // Wire BalanceManager
        var bmSO = new SerializedObject(balanceMgr);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.FindProperty("_platform").objectReferenceValue = platformObj.transform;
        if (spCoin   != null) bmSO.FindProperty("_coinSprite").objectReferenceValue       = spCoin;
        if (spHeavy  != null) bmSO.FindProperty("_heavyCoinSprite").objectReferenceValue  = spHeavy;
        if (spLight  != null) bmSO.FindProperty("_lightCoinSprite").objectReferenceValue  = spLight;
        if (spBounce != null) bmSO.FindProperty("_bounceCoinSprite").objectReferenceValue = spBounce;
        if (spMagnet != null) bmSO.FindProperty("_magnetCoinSprite").objectReferenceValue = spMagnet;
        bmSO.ApplyModifiedProperties();

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

        // === HUD ===
        var stageText = CT(canvasUIObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(300,50), new Vector2(10,-15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.6f, 0.2f);

        var timerText = CT(canvasUIObj.transform, "TimerText", "20", 52, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(180,65), new Vector2(0,-15));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scoreText = CT(canvasUIObj.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(240,50), new Vector2(-10,-15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        var coinCountText = CT(canvasUIObj.transform, "CoinCountText", "コマ: 0/3", 30, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(200,45), new Vector2(10,-60));
        coinCountText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.6f, 0.3f);

        var multiplierText = CT(canvasUIObj.transform, "MultiplierText", "x2", 46, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(0,60));
        multiplierText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        multiplierText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        multiplierText.SetActive(false);

        // Brake icon (bottom right)
        var brakeIconObj = new GameObject("BrakeIcon");
        brakeIconObj.transform.SetParent(canvasUIObj.transform, false);
        var brakeIconRt = brakeIconObj.AddComponent<RectTransform>();
        brakeIconRt.anchorMin = new Vector2(1f, 0f);
        brakeIconRt.anchorMax = new Vector2(1f, 0f);
        brakeIconRt.pivot = new Vector2(1f, 0f);
        brakeIconRt.sizeDelta = new Vector2(90, 90);
        brakeIconRt.anchoredPosition = new Vector2(-10, 80);
        var brakeIconImg = brakeIconObj.AddComponent<Image>();
        brakeIconImg.sprite = spBrake;
        brakeIconImg.color = Color.yellow;

        var brakeCdText = CT(canvasUIObj.transform, "BrakeCooldownText", "5s", 26, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(90,30), new Vector2(-10, 175));
        brakeCdText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        brakeCdText.GetComponent<TextMeshProUGUI>().color = Color.gray;
        brakeCdText.SetActive(false);

        // Menu button (always visible, bottom left)
        var menuBtn = CB(canvasUIObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0f,0f), new Vector2(0f,0f), new Vector2(0f,0f),
            new Vector2(160, 55), new Vector2(10, 15), new Color(0.3f, 0.3f, 0.35f));

        // === Stage Clear Panel ===
        var scPanel = CreatePanel(canvasUIObj.transform, "StageClearPanel", new Color(0.05f,0.15f,0.05f,0.95f), new Vector2(540,380));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,70), new Vector2(0,140));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.5f);

        var scScore = CT(scPanel.transform, "StageClearScore", "スコア: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,55), new Vector2(0,60));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(280,60), new Vector2(0,-60), new Color(0.2f,0.6f,0.2f));
        var scMenuBtn = CB(scPanel.transform, "MenuButton2", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,55), new Vector2(0,-135), new Color(0.35f,0.35f,0.4f));
        scPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = CreatePanel(canvasUIObj.transform, "GameOverPanel", new Color(0.12f,0.05f,0.05f,0.95f), new Vector2(520,320));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,70), new Vector2(0,110));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.3f,0.3f);

        var goScore = CT(goPanel.transform, "GameOverScore", "スコア: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,60), new Vector2(0,30));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetry = CB(goPanel.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(-110,-65), new Color(0.2f,0.6f,0.2f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(110,-65), new Color(0.35f,0.35f,0.4f));
        goPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = CreatePanel(canvasUIObj.transform, "AllClearPanel", new Color(0.05f,0.12f,0.05f,0.95f), new Vector2(540,360));
        var acTitle = CT(acPanel.transform, "AllClearTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,70), new Vector2(0,135));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.2f);

        var acScore = CT(acPanel.transform, "AllClearScore", "最終スコア: 0", 40, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,60), new Vector2(0,55));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);

        var acMenu = CB(acPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(0,-75), new Color(0.35f,0.35f,0.4f));
        acPanel.SetActive(false);

        // === SpinBalanceUI ===
        var uiObj = new GameObject("SpinBalanceUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<SpinBalanceUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_timerText").objectReferenceValue       = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue       = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_coinCountText").objectReferenceValue   = coinCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_multiplierText").objectReferenceValue  = multiplierText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue       = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_brakeIcon").objectReferenceValue       = brakeIconImg;
        uiSO.FindProperty("_brakeCooldownText").objectReferenceValue = brakeCdText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue   = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue   = acPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue      = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue  = ip;
        gmSO.FindProperty("_balanceManager").objectReferenceValue    = balanceMgr;
        gmSO.FindProperty("_ui").objectReferenceValue                = ui;
        gmSO.ApplyModifiedProperties();

        // Button events
        scNextBtn.GetComponent<Button>().onClick.AddListener(() => ui.OnNextStageClicked());
        menuBtn.AddComponent<BackToMenuButton>();
        scMenuBtn.AddComponent<BackToMenuButton>();
        acMenu.AddComponent<BackToMenuButton>();
        goMenu.AddComponent<BackToMenuButton>();
        goRetry.GetComponent<Button>().onClick.AddListener(() => ui.OnRestartClicked());

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/047v2_SpinBalance.unity");
        AddSceneToBuildSettings("Assets/Scenes/047v2_SpinBalance.unity");
        Debug.Log("[Setup047v2] シーン生成完了: Assets/Scenes/047v2_SpinBalance.unity");
    }

    // --- Helpers ---

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
        => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static InstructionPanel CreateInstructionPanel(Transform canvasTransform, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasTransform, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);

        var canvas = panelObj.GetComponent<Canvas>();
        if (canvas == null) canvas = panelObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();

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
        titleTmp.color = new Color(0.5f, 1f, 0.6f);

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
        startBtnImg.color = new Color(0.2f, 0.65f, 0.25f);
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
        Debug.Log($"[Setup047v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
