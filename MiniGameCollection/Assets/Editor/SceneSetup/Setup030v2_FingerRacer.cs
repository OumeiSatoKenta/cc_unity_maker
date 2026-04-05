using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game030v2_FingerRacer;

public static class Setup030v2_FingerRacer
{
    [MenuItem("Assets/Setup/030v2 FingerRacer")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup030v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game030v2_FingerRacer/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Car.png", sp+"RivalCar.png",
            sp+"Checkpoint.png", sp+"StartFlag.png", sp+"GoalFlag.png",
            sp+"Obstacle.png", sp+"BoostIcon.png", sp+"SandArea.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg        = LoadSprite(sp + "Background.png");
        Sprite spCar       = LoadSprite(sp + "Car.png");
        Sprite spRivalCar  = LoadSprite(sp + "RivalCar.png");
        Sprite spCheckpt   = LoadSprite(sp + "Checkpoint.png");
        Sprite spStart     = LoadSprite(sp + "StartFlag.png");
        Sprite spGoal      = LoadSprite(sp + "GoalFlag.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camW    = camSize * (camera != null ? camera.aspect : 0.5625f);

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camW * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Start Marker
        var startObj = new GameObject("StartMarker");
        startObj.transform.position = new Vector3(-camW * 0.65f, -camSize + 2.8f, 0f);
        var startSr = startObj.AddComponent<SpriteRenderer>();
        startSr.sprite = spStart;
        startSr.sortingOrder = 5;
        if (spStart != null)
        {
            float s = 0.7f / (spStart.rect.width / spStart.pixelsPerUnit);
            startObj.transform.localScale = Vector3.one * s;
        }

        // Goal Marker
        var goalObj = new GameObject("GoalMarker");
        goalObj.transform.position = new Vector3(camW * 0.65f, camSize - 1.8f, 0f);
        var goalSr = goalObj.AddComponent<SpriteRenderer>();
        goalSr.sprite = spGoal;
        goalSr.sortingOrder = 5;
        if (spGoal != null)
        {
            float s = 0.7f / (spGoal.rect.width / spGoal.pixelsPerUnit);
            goalObj.transform.localScale = Vector3.one * s;
        }
        var goalCol = goalObj.AddComponent<CircleCollider2D>();
        goalCol.isTrigger = true;
        goalCol.radius = 0.6f;

        // Car GameObject
        var carObj = new GameObject("Car");
        carObj.transform.position = startObj.transform.position;
        var carSr = carObj.AddComponent<SpriteRenderer>();
        carSr.sprite = spCar;
        carSr.sortingOrder = 10;
        if (spCar != null)
        {
            float targetSize = 0.5f;
            float s = targetSize / (spCar.rect.width / spCar.pixelsPerUnit);
            carObj.transform.localScale = Vector3.one * s;
        }
        var carCol = carObj.AddComponent<CircleCollider2D>();
        carCol.isTrigger = true;
        carCol.radius = 0.25f;

        // Rival Car (hidden by default - Stage 5 only)
        var rivalObj = new GameObject("RivalCar");
        rivalObj.transform.position = startObj.transform.position + Vector3.left * 0.5f;
        var rivalSr = rivalObj.AddComponent<SpriteRenderer>();
        rivalSr.sprite = spRivalCar;
        rivalSr.sortingOrder = 9;
        if (spRivalCar != null)
        {
            float targetSize = 0.5f;
            float s = targetSize / (spRivalCar.rect.width / spRivalCar.pixelsPerUnit);
            rivalObj.transform.localScale = Vector3.one * s;
        }
        rivalObj.SetActive(false);

        // Course Line Renderer
        var lineObj = new GameObject("CourseLine");
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.3f, 0.8f, 1f, 0.85f);
        lr.endColor = new Color(0.1f, 0.5f, 0.9f, 0.85f);
        lr.positionCount = 0;
        lr.sortingOrder = 2;
        lr.useWorldSpace = true;

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<FingerRacerGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // CarController (on Car GameObject)
        var carCtrl = carObj.AddComponent<CarController>();
        var carSO = new SerializedObject(carCtrl);
        carSO.FindProperty("_gameManager").objectReferenceValue = gm;
        carSO.FindProperty("_carSr").objectReferenceValue = carSr;
        carSO.FindProperty("_goalMarker").objectReferenceValue = goalObj.transform;
        carSO.ApplyModifiedProperties();

        // CourseDrawer (child of GameManager)
        var cdObj = new GameObject("CourseDrawer");
        cdObj.transform.SetParent(gmObj.transform);
        var courseDrawer = cdObj.AddComponent<CourseDrawer>();
        var cdSO = new SerializedObject(courseDrawer);
        cdSO.FindProperty("_gameManager").objectReferenceValue = gm;
        cdSO.FindProperty("_lineRenderer").objectReferenceValue = lr;
        cdSO.FindProperty("_startMarker").objectReferenceValue = startObj.transform;
        cdSO.FindProperty("_goalMarker").objectReferenceValue = goalObj.transform;
        cdSO.FindProperty("_carController").objectReferenceValue = carCtrl;
        cdSO.ApplyModifiedProperties();

        // RivalCarController (on Rival GameObject)
        var rivalCtrl = rivalObj.AddComponent<RivalCarController>();
        var rivalSO2 = new SerializedObject(rivalCtrl);
        rivalSO2.FindProperty("_gameManager").objectReferenceValue = gm;
        rivalSO2.FindProperty("_rivalSr").objectReferenceValue = rivalSr;
        rivalSO2.ApplyModifiedProperties();

        // Canvas (main HUD)
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- HUD Elements ---
        // Stage text (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score text (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 55), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Time text (top left, 2nd row)
        var timeText = CT(canvasObj.transform, "TimeText", "0.0s", 30, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(200, 45), new Vector2(20, -70));
        timeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        timeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.6f);

        // CourseOut text (top right 2nd row)
        var courseOutText = CT(canvasObj.transform, "CourseOutText", "OUT: 0/3", 30, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(250, 45), new Vector2(-20, -70));
        courseOutText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        courseOutText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);

        // Boost text (left lower HUD)
        var boostText = CT(canvasObj.transform, "BoostText", "⚡3/3", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(200, 50), new Vector2(20, -120));
        boostText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        boostText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);

        // Combo text (center)
        var comboText = CT(canvasObj.transform, "ComboText", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 300));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        // Drawing UI panel (center bottom - "Start Race" button)
        var drawingUIObj = new GameObject("DrawingUI");
        drawingUIObj.transform.SetParent(canvasObj.transform, false);
        var duiRt = drawingUIObj.AddComponent<RectTransform>();
        duiRt.anchorMin = new Vector2(0.5f, 0f);
        duiRt.anchorMax = new Vector2(0.5f, 0f);
        duiRt.pivot = new Vector2(0.5f, 0f);
        duiRt.sizeDelta = new Vector2(400, 80);
        duiRt.anchoredPosition = new Vector2(0, 85);

        var startRaceBtn = CB(drawingUIObj.transform, "StartRaceButton", "スタート！", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), Vector2.zero, new Color(0.2f, 0.7f, 0.2f));

        // Menu Button (left bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(180, 65), new Vector2(20, 20), new Color(0.2f, 0.2f, 0.3f, 0.9f));

        // Question Button (right bottom)
        var reShowBtn = CB(canvasObj.transform, "QuestionButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-20, 20), new Color(0.3f, 0.3f, 0.5f, 0.9f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 400));
        scPanel.SetActive(false);
        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), new Vector2(0, 80));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        var scBonus = CT(scPanel.transform, "SCBonus", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, -10));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = Color.white;
        scBonus.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        // Final Clear Panel
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 500));
        fcPanel.SetActive(false);
        var fcTitle = CT(fcPanel.transform, "FCTitle", "全コース制覇！", 60, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), new Vector2(0, 120));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        var fcScore = CT(fcPanel.transform, "FCScore", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 90), new Vector2(0, 10));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        fcScore.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;
        var fcRetryBtn = CB(fcPanel.transform, "FCRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(-130, -130), new Color(0.2f, 0.5f, 0.8f));
        var fcMenuBtn = CB(fcPanel.transform, "FCMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(130, -130), new Color(0.3f, 0.3f, 0.5f));

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 500));
        goPanel.SetActive(false);
        var goTitle = CT(goPanel.transform, "GOTitle", "GAME OVER", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), new Vector2(0, 120));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var goScore = CT(goPanel.transform, "GOScore", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 65), new Vector2(0, 20));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(-130, -130), new Color(0.8f, 0.3f, 0.2f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(130, -130), new Color(0.3f, 0.3f, 0.5f));

        // InstructionPanel Canvas (front)
        var ipCanvasObj = new GameObject("InstructionCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        var ipScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasObj.AddComponent<GraphicRaycaster>();

        var ipPanel = new GameObject("InstructionPanel");
        ipPanel.transform.SetParent(ipCanvasObj.transform, false);
        var ipImg = ipPanel.AddComponent<Image>();
        ipImg.color = new Color(0f, 0.03f, 0.08f, 0.93f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "FingerRacer", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 260));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 140));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.9f, 0.85f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 160), new Vector2(0, -10));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.75f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -160));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -300), new Color(0.2f, 0.4f, 0.8f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // FingerRacerUI (child of GameManager)
        var uiObj = new GameObject("FingerRacerUI");
        uiObj.transform.SetParent(gmObj.transform);
        var frUI = uiObj.AddComponent<FingerRacerUI>();
        var uiSO = new SerializedObject(frUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_courseOutText").objectReferenceValue = courseOutText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_boostText").objectReferenceValue = boostText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_drawingUI").objectReferenceValue = drawingUIObj;
        uiSO.FindProperty("_startRaceButton").objectReferenceValue = startRaceBtn.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalClearScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_courseDrawer").objectReferenceValue = courseDrawer;
        gmSO.FindProperty("_carController").objectReferenceValue = carCtrl;
        gmSO.FindProperty("_rivalCarController").objectReferenceValue = rivalCtrl;
        gmSO.FindProperty("_ui").objectReferenceValue = frUI;
        gmSO.ApplyModifiedProperties();

        // Button onClick wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");
        AddButtonOnClick(startRaceBtn.GetComponent<Button>(), gm, "OnStartRacePressed");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(fcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");

        // Save scene
        string scenePath = "Assets/Scenes/030v2_FingerRacer.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup030v2] Scene created: " + scenePath);
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists("MiniGameCollection/" + path) && !File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { AssetDatabase.ImportAsset(path); importer = AssetImporter.GetAtPath(path) as TextureImporter; }
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        if (font) tmp.font = font;
        tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;
        var labelObj = new GameObject("Label"); labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28; if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 sizeDelta)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = color;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            else Debug.LogWarning($"[Setup030v2] Sprite not found: {path}");
        }
        return sprite;
    }

    static void AddButtonOnClick(Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var onClick = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        onClick.arraySize++;
        var call = onClick.GetArrayElementAtIndex(onClick.arraySize - 1);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").intValue = 1;
        call.FindPropertyRelative("m_CallState").intValue = 2;
        so.ApplyModifiedProperties();
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
