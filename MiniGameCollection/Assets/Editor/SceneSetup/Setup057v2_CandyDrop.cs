using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game057v2_CandyDrop;

public static class Setup057v2_CandyDrop
{
    [MenuItem("Assets/Setup/057v2 CandyDrop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup057v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game057v2_CandyDrop/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.78f, 0.94f, 0.78f);
            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.024f, 0.062f, 1f);
        }

        // Load sprites
        Sprite groundSprite = LoadSprite(sp + "Ground.png");
        Sprite[] circleSprites = new Sprite[4];
        Sprite[] squareSprites = new Sprite[4];
        Sprite[] triangleSprites = new Sprite[4];
        Sprite[] starSprites = new Sprite[4];
        string[] colorNames = { "Red", "Blue", "Green", "Yellow" };
        for (int i = 0; i < 4; i++)
        {
            circleSprites[i] = LoadSprite(sp + $"Candy_Circle_{colorNames[i]}.png");
            squareSprites[i] = LoadSprite(sp + $"Candy_Square_{colorNames[i]}.png");
            triangleSprites[i] = LoadSprite(sp + $"Candy_Triangle_{colorNames[i]}.png");
            starSprites[i] = LoadSprite(sp + $"Candy_Star_{colorNames[i]}.png");
        }
        Sprite meltSprite = LoadSprite(sp + "Candy_Melt.png");
        Sprite giantSprite = LoadSprite(sp + "Candy_Giant.png");

        // Ground
        float camSize = 6.0f;
        float groundY = -camSize + 1.8f;
        var groundObj = new GameObject("Ground");
        groundObj.tag = "Ground";
        groundObj.transform.position = new Vector3(0, groundY, 0);
        groundObj.transform.localScale = new Vector3(1f, 0.4f, 1f);
        var groundSr = groundObj.AddComponent<SpriteRenderer>();
        groundSr.sprite = groundSprite;
        groundSr.sortingOrder = 5;
        var groundCol = groundObj.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(10f, 1f);

        // Walls (invisible)
        float camWidth = camSize * (9f / 16f); // approximate
        CreateWall("WallLeft", new Vector3(-camWidth - 0.5f, 0, 0), new Vector2(1f, camSize * 2f));
        CreateWall("WallRight", new Vector3(camWidth + 0.5f, 0, 0), new Vector2(1f, camSize * 2f));

        // Goal line indicator
        var goalLine = new GameObject("GoalLine");
        goalLine.transform.position = new Vector3(0, groundY + 5.0f, 0); // Stage 1 default
        var goalLineSr = goalLine.AddComponent<SpriteRenderer>();
        goalLineSr.color = new Color(1f, 0.8f, 0f, 0.6f);
        goalLineSr.sortingOrder = 3;
        // Simple thin sprite using a white texture
        goalLineSr.sprite = CreateWhiteSprite();
        goalLine.transform.localScale = new Vector3(camWidth * 2f * 1.2f, 0.05f, 1f);

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CandyDropGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        ConfigureStageManager(sm);

        // TowerChecker
        var tcObj = new GameObject("TowerChecker");
        tcObj.transform.SetParent(gmObj.transform);
        var tc = tcObj.AddComponent<TowerChecker>();
        var tcSO = new SerializedObject(tc);
        tcSO.FindProperty("_gameManager").objectReferenceValue = gm;
        tcSO.FindProperty("_groundTransform").objectReferenceValue = groundObj.transform;
        tcSO.FindProperty("_goalLineTransform").objectReferenceValue = goalLine.transform;
        tcSO.ApplyModifiedProperties();

        // CandySpawner
        var spawnerObj = new GameObject("CandySpawner");
        spawnerObj.transform.SetParent(gmObj.transform);
        var spawner = spawnerObj.AddComponent<CandySpawner>();
        var spSO = new SerializedObject(spawner);
        spSO.FindProperty("_gameManager").objectReferenceValue = gm;
        // sprites arrays
        SetSpriteArray(spSO, "_circleSprites", circleSprites);
        SetSpriteArray(spSO, "_squareSprites", squareSprites);
        SetSpriteArray(spSO, "_triangleSprites", triangleSprites);
        SetSpriteArray(spSO, "_starSprites", starSprites);
        spSO.FindProperty("_meltSprite").objectReferenceValue = meltSprite;
        spSO.FindProperty("_giantSprite").objectReferenceValue = giantSprite;
        spSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage text
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 50), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // HUD: Score
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 30, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 45), new Vector2(20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Height gauge (right side)
        var gaugeLabel = CT(canvasObj.transform, "GaugeLabel", "高さ", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(80, 36), new Vector2(-105, -30));
        var heightSliderObj = new GameObject("HeightGauge", typeof(RectTransform));
        heightSliderObj.transform.SetParent(canvasObj.transform, false);
        var heightSlider = heightSliderObj.AddComponent<Slider>();
        heightSlider.direction = Slider.Direction.BottomToTop;
        heightSlider.minValue = 0f; heightSlider.maxValue = 1f; heightSlider.value = 0f;
        var hsRT = heightSliderObj.GetComponent<RectTransform>();
        hsRT.anchorMin = new Vector2(1, 0.1f); hsRT.anchorMax = new Vector2(1, 0.9f);
        hsRT.pivot = new Vector2(1, 0.5f); hsRT.sizeDelta = new Vector2(30, 0); hsRT.anchoredPosition = new Vector2(-20, 0);
        var hsBg = new GameObject("Background", typeof(RectTransform)); hsBg.transform.SetParent(heightSliderObj.transform, false);
        var hsBgImg = hsBg.AddComponent<Image>(); hsBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        var hsBgRT = hsBg.GetComponent<RectTransform>(); hsBgRT.anchorMin = Vector2.zero; hsBgRT.anchorMax = Vector2.one; hsBgRT.offsetMin = hsBgRT.offsetMax = Vector2.zero;
        heightSlider.targetGraphic = hsBgImg;
        var hsFillArea = new GameObject("Fill Area", typeof(RectTransform)); hsFillArea.transform.SetParent(heightSliderObj.transform, false);
        var hsFillAreaRT = hsFillArea.GetComponent<RectTransform>(); hsFillAreaRT.anchorMin = Vector2.zero; hsFillAreaRT.anchorMax = Vector2.one; hsFillAreaRT.offsetMin = hsFillAreaRT.offsetMax = Vector2.zero;
        var hsFill = new GameObject("Fill", typeof(RectTransform)); hsFill.transform.SetParent(hsFillArea.transform, false);
        var hsFillImg = hsFill.AddComponent<Image>(); hsFillImg.color = new Color(0.3f, 0.9f, 0.3f);
        var hsFillRT = hsFill.GetComponent<RectTransform>(); hsFillRT.anchorMin = Vector2.zero; hsFillRT.anchorMax = Vector2.one; hsFillRT.offsetMin = hsFillRT.offsetMax = Vector2.zero;
        heightSlider.fillRect = hsFillRT;

        // Next candy preview
        var nextLabel = CT(canvasObj.transform, "NextLabel", "NEXT", 24, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, 30), new Vector2(20, -90));
        var nextCandyObj = new GameObject("NextCandyImage", typeof(RectTransform));
        nextCandyObj.transform.SetParent(canvasObj.transform, false);
        var nextImg = nextCandyObj.AddComponent<Image>();
        nextImg.color = Color.clear;
        var nextRT = nextCandyObj.GetComponent<RectTransform>();
        nextRT.anchorMin = new Vector2(0, 1); nextRT.anchorMax = new Vector2(0, 1);
        nextRT.pivot = new Vector2(0, 1); nextRT.sizeDelta = new Vector2(80, 80); nextRT.anchoredPosition = new Vector2(20, -125);

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "", 42, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(500, 70), Vector2.zero);
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Menu button (bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 60), new Vector2(0, 15),
            new Color(0.3f, 0.3f, 0.3f, 0.8f));

        // Stage Clear Panel
        var clearPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        var cpImg = clearPanel.AddComponent<Image>(); cpImg.color = new Color(0, 0, 0, 0.75f);
        var cpRT = clearPanel.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.1f, 0.35f); cpRT.anchorMax = new Vector2(0.9f, 0.65f);
        cpRT.offsetMin = cpRT.offsetMax = Vector2.zero;
        var cpTitle = CT(clearPanel.transform, "Title", "ステージクリア！", 44, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        cpTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cpTitle.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var cpScore = CT(clearPanel.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        cpScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var cpNextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(280, 65), Vector2.zero,
            new Color(0.2f, 0.6f, 0.2f));
        clearPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = new GameObject("GameClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcImg = gcPanel.AddComponent<Image>(); gcImg.color = new Color(0, 0, 0, 0.8f);
        var gcRT = gcPanel.GetComponent<RectTransform>();
        gcRT.anchorMin = new Vector2(0.05f, 0.25f); gcRT.anchorMax = new Vector2(0.95f, 0.75f);
        gcRT.offsetMin = gcRT.offsetMax = Vector2.zero;
        var gcTitle = CT(gcPanel.transform, "Title", "ゲームクリア！", 48, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = Color.green;
        var gcScore = CT(gcPanel.transform, "ScoreText", "Final Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), Vector2.zero);
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetry = CB(gcPanel.transform, "RetryButton", "もう一度", 34, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
        var gcMenu = CB(gcPanel.transform, "MenuButton", "メニューへ", 34, jpFont,
            new Vector2(0.7f, 0.22f), new Vector2(0.7f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        gcPanel.SetActive(false);

        // Game Over Panel
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goImg = goPanel.AddComponent<Image>(); goImg.color = new Color(0.5f, 0, 0, 0.8f);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.35f); goRT.anchorMax = new Vector2(0.9f, 0.65f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goTitle = CT(goPanel.transform, "Title", "ゲームオーバー", 44, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScore = CT(goPanel.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetry = CB(goPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.7f, 0.22f), new Vector2(0.7f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        goPanel.SetActive(false);

        // InstructionPanel overlay canvas
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipCanvasComp = ipCanvas.AddComponent<Canvas>();
        ipCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvasComp.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipRoot = new GameObject("InstructionPanel", typeof(RectTransform));
        ipRoot.transform.SetParent(ipCanvas.transform, false);
        var ipImg = ipRoot.AddComponent<Image>(); ipImg.color = new Color(0f, 0.1f, 0f, 0.95f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one; ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "", 52, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.4f);

        var ipDesc = CT(ipRoot.transform, "DescriptionText", "", 34, jpFont,
            new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipRoot.transform, "ControlsText", "", 30, jpFont,
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipRoot.transform, "GoalText", "", 30, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.8f);

        var ipStartBtn = CB(ipRoot.transform, "StartButton", "はじめる", 36, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(260, 70), Vector2.zero,
            new Color(0.2f, 0.6f, 0.2f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(65, 65), new Vector2(-20, 90),
            new Color(0.3f, 0.4f, 0.3f, 0.9f));

        var ip = ipRoot.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipRoot;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = ipHelpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // CandyDropUI
        var uiObj = new GameObject("CandyDropUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<CandyDropUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_heightGauge").objectReferenceValue = heightSlider;
        uiSO.FindProperty("_nextCandyImage").objectReferenceValue = nextImg;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = cpScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = cpNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetry.GetComponent<Button>();
        uiSO.FindProperty("_backToMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.FindProperty("_backToMenuButton2").objectReferenceValue = goMenu.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire spawner UI ref
        spSO = new SerializedObject(spawner);
        spSO.FindProperty("_ui").objectReferenceValue = ui;
        spSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_spawner").objectReferenceValue = spawner;
        gmSO.FindProperty("_towerChecker").objectReferenceValue = tc;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button onClick events
        cpNextBtn.GetComponent<Button>().onClick.AddListener(() => { });
        var cpNextSO = new SerializedObject(cpNextBtn.GetComponent<Button>());
        AddButtonClick(cpNextBtn.GetComponent<Button>(), gm, "OnNextStage");
        AddButtonClick(menuBtn.GetComponent<Button>(), gm, "OnBackToMenu");
        AddButtonClick(goRetry.GetComponent<Button>(), gm, "OnRetry");
        AddButtonClick(goMenu.GetComponent<Button>(), gm, "OnBackToMenu");
        AddButtonClick(gcRetry.GetComponent<Button>(), gm, "OnRetry");
        AddButtonClick(gcMenu.GetComponent<Button>(), gm, "OnBackToMenu");

        // EventSystem
        var evSys = new GameObject("EventSystem");
        evSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evSys.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/057v2_CandyDrop.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup057v2] Scene created: " + scenePath);
    }

    static void ConfigureStageManager(StageManager sm)
    {
        var so = new SerializedObject(sm);
        var stagesProp = so.FindProperty("_stages");
        stagesProp.arraySize = 5;

        float[] speeds = { 0.8f, 0.9f, 1.0f, 1.0f, 1.1f };
        int[] counts = { 1, 1, 1, 1, 1 };
        float[] complexities = { 0.0f, 0.3f, 0.6f, 0.8f, 1.0f };

        for (int i = 0; i < 5; i++)
        {
            var stage = stagesProp.GetArrayElementAtIndex(i);
            stage.FindPropertyRelative("speedMultiplier").floatValue = speeds[i];
            stage.FindPropertyRelative("countMultiplier").intValue = counts[i];
            stage.FindPropertyRelative("complexityFactor").floatValue = complexities[i];
        }
        so.ApplyModifiedProperties();
    }

    static void CreateWall(string name, Vector3 pos, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.position = pos;
        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(4, 4);
        var colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    static void SetSpriteArray(SerializedObject so, string propName, Sprite[] sprites)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        so.ApplyModifiedProperties();
    }

    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            if (!File.Exists(path)) return null;
            AssetDatabase.ImportAsset(path);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        if (tex == null) return null;
        string spritePath = path;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null) return sprite;
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CB(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var label = new GameObject("Text", typeof(RectTransform));
        label.transform.SetParent(go.transform, false);
        var tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;
        return go;
    }

    static void AddButtonClick(Button btn, MonoBehaviour target, string methodName)
    {
        var entry = new UnityEngine.Events.UnityAction(
            () => target.GetType().GetMethod(methodName)?.Invoke(target, null)
        );
        btn.onClick.AddListener(entry);
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
