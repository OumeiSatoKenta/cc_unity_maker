using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using Game051v2_DrawBridge;

public static class Setup051v2_DrawBridge
{
    [MenuItem("Assets/Setup/051v2 DrawBridge")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup051v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game051v2_DrawBridge/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.71f, 0.90f, 0.78f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite imports
        string[] spritePaths = {
            sp+"Background.png", sp+"LeftCliff.png", sp+"RightCliff.png",
            sp+"Ball.png", sp+"Goal.png", sp+"Rock.png",
            sp+"Wind.png", sp+"Coin.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spLeftCliff  = LoadSprite(sp + "LeftCliff.png");
        Sprite spRightCliff = LoadSprite(sp + "RightCliff.png");
        Sprite spBall     = LoadSprite(sp + "Ball.png");
        Sprite spGoal     = LoadSprite(sp + "Goal.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camWidth = camera != null ? camSize * camera.aspect : 2.8f;

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

        // Cliff positions (responsive)
        float cliffX = camWidth - 0.7f;
        float cliffY = -0.5f;
        float cliffWidth = 1.4f;
        float cliffHeight = 3.5f;
        float gapWidth = (cliffX - cliffWidth * 0.5f) * 2f; // gap between cliffs

        // Left Cliff
        var leftCliffObj = CreateCliff("LeftCliff", spLeftCliff, new Vector2(-cliffX, cliffY), new Vector2(cliffWidth, cliffHeight));

        // Right Cliff
        var rightCliffObj = CreateCliff("RightCliff", spRightCliff, new Vector2(cliffX, cliffY), new Vector2(cliffWidth, cliffHeight));

        // Ball
        float ballStartX = -cliffX + cliffWidth * 0.3f;
        float ballStartY = cliffY + cliffHeight * 0.5f + 0.3f;
        var ballObj = new GameObject("Ball");
        ballObj.tag = "Ball";
        if (spBall != null)
        {
            var ballSr = ballObj.AddComponent<SpriteRenderer>();
            ballSr.sprite = spBall;
            ballSr.sortingOrder = 10;
            ballObj.transform.localScale = Vector3.one * 0.5f;
        }
        ballObj.transform.position = new Vector3(ballStartX, ballStartY, 0f);
        var ballRb = ballObj.AddComponent<Rigidbody2D>();
        ballRb.gravityScale = 1f;
        ballRb.simulated = false;
        ballRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var ballCol = ballObj.AddComponent<CircleCollider2D>();
        ballCol.radius = 0.25f;
        ballCol.sharedMaterial = CreateBounceMaterial();

        // Goal
        float goalX = cliffX - cliffWidth * 0.3f;
        float goalY = cliffY + cliffHeight * 0.5f + 0.3f;
        var goalObj = new GameObject("Goal");
        goalObj.tag = "Goal";
        if (spGoal != null)
        {
            var goalSr = goalObj.AddComponent<SpriteRenderer>();
            goalSr.sprite = spGoal;
            goalSr.sortingOrder = 8;
            goalObj.transform.localScale = Vector3.one * 0.6f;
        }
        goalObj.transform.position = new Vector3(goalX, goalY, 0f);
        var goalTrigger = goalObj.AddComponent<CircleCollider2D>();
        goalTrigger.radius = 0.4f;
        goalTrigger.isTrigger = true;

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DrawBridgeGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // DrawingManager
        var dmObj = new GameObject("DrawingManager");
        var dm = dmObj.AddComponent<DrawingManager>();

        // BallController
        var bc = ballObj.AddComponent<BallController>();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<GraphicRaycaster>();
        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;

        // HUD: Stage text (top left)
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "Stage 1 / 5", jpFont, 36,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300f, 50f), new Vector2(160f, -30f), Color.white);

        // HUD: Score text (top right)
        var scoreTextObj = CreateText(canvasObj.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300f, 50f), new Vector2(-160f, -30f), Color.white);

        // Ink slider (top center)
        var inkSliderObj = new GameObject("InkSlider");
        inkSliderObj.transform.SetParent(canvasObj.transform, false);
        var inkSliderRt = inkSliderObj.AddComponent<RectTransform>();
        inkSliderRt.anchorMin = new Vector2(0.5f, 1f);
        inkSliderRt.anchorMax = new Vector2(0.5f, 1f);
        inkSliderRt.pivot = new Vector2(0.5f, 1f);
        inkSliderRt.sizeDelta = new Vector2(400f, 30f);
        inkSliderRt.anchoredPosition = new Vector2(0f, -65f);
        var inkSlider = inkSliderObj.AddComponent<Slider>();
        inkSlider.minValue = 0f;
        inkSlider.maxValue = 1f;
        inkSlider.value = 1f;
        inkSlider.interactable = false;

        // Slider background
        var inkBgObj = new GameObject("Background");
        inkBgObj.transform.SetParent(inkSliderObj.transform, false);
        var inkBgRt = inkBgObj.AddComponent<RectTransform>();
        inkBgRt.anchorMin = Vector2.zero;
        inkBgRt.anchorMax = Vector2.one;
        inkBgRt.offsetMin = Vector2.zero;
        inkBgRt.offsetMax = Vector2.zero;
        var inkBgImg = inkBgObj.AddComponent<Image>();
        inkBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        inkSlider.targetGraphic = inkBgImg;

        // Slider fill area
        var fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(inkSliderObj.transform, false);
        var fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = Vector2.zero;
        fillAreaRt.offsetMax = Vector2.zero;

        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        var fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 0.3f);
        inkSlider.fillRect = fillRt;

        // Ink label
        CreateText(inkSliderObj.transform, "InkLabel", "インク", jpFont, 22,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(60f, 30f), new Vector2(-5f, 0f), Color.white);

        // GO Button (bottom right)
        var goButtonObj = CreateButton(canvasObj.transform, "GOButton", "GO!", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(160f, 60f), new Vector2(-90f, 75f), new Color(0.15f, 0.6f, 0.2f));

        // Erase Button (bottom left)
        var eraseButtonObj = CreateButton(canvasObj.transform, "EraseButton", "消しゴム", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(160f, 60f), new Vector2(90f, 75f), new Color(0.6f, 0.3f, 0.1f));

        // Back to menu (bottom center)
        var backBtnObj = CreateButton(canvasObj.transform, "BackToMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200f, 55f), new Vector2(0f, 15f), new Color(0.3f, 0.3f, 0.4f));

        // Stage Clear Panel
        var stageClearPanel = CreatePanel(canvasObj.transform, "StageClearPanel",
            new Color(0.05f, 0.15f, 0.05f, 0.92f), new Vector2(600f, 400f));
        CreateText(stageClearPanel.transform, "Title", "ステージクリア！", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 70f), new Vector2(0f, 100f), Color.yellow);
        var scScoreTextObj = CreateText(stageClearPanel.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 50f), new Vector2(0f, 20f), Color.white);
        var nextBtnObj = CreateButton(stageClearPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(280f, 65f), new Vector2(0f, -80f), new Color(0.1f, 0.55f, 0.15f));
        stageClearPanel.SetActive(false);

        // All Clear Panel
        var allClearPanel = CreatePanel(canvasObj.transform, "AllClearPanel",
            new Color(0.05f, 0.1f, 0.05f, 0.95f), new Vector2(600f, 450f));
        CreateText(allClearPanel.transform, "Title", "全ステージクリア！", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 70f), new Vector2(0f, 130f), Color.yellow);
        var acScoreTextObj = CreateText(allClearPanel.transform, "ScoreText", "Total Score: 0", jpFont, 40,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 55f), new Vector2(0f, 30f), Color.white);
        var acRetryObj = CreateButton(allClearPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220f, 60f), new Vector2(-120f, -80f), new Color(0.2f, 0.5f, 0.2f));
        var acMenuObj = CreateButton(allClearPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220f, 60f), new Vector2(120f, -80f), new Color(0.3f, 0.3f, 0.4f));
        allClearPanel.SetActive(false);

        // Game Over Panel
        var gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0.15f, 0.05f, 0.05f, 0.92f), new Vector2(600f, 400f));
        CreateText(gameOverPanel.transform, "Title", "ゲームオーバー", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 70f), new Vector2(0f, 100f), Color.red);
        var goScoreTextObj = CreateText(gameOverPanel.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 50f), new Vector2(0f, 20f), Color.white);
        var retryBtnObj = CreateButton(gameOverPanel.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220f, 65f), new Vector2(0f, -70f), new Color(0.6f, 0.2f, 0.1f));
        gameOverPanel.SetActive(false);

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // InstructionPanel
        var ip = BuildInstructionPanel(jpFont);

        // Wire up DrawBridgeUI
        var ui = canvasObj.AddComponent<DrawBridgeUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider;
        uiSO.FindProperty("_inkSliderFill").objectReferenceValue = fillImg;
        uiSO.FindProperty("_goButton").objectReferenceValue = goButtonObj.GetComponent<Button>();
        uiSO.FindProperty("_eraseButton").objectReferenceValue = eraseButtonObj.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = stageClearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = allClearPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_allClearRetryButton").objectReferenceValue = acRetryObj.GetComponent<Button>();
        uiSO.FindProperty("_allClearMenuButton").objectReferenceValue = acMenuObj.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_backToMenuButton").objectReferenceValue = backBtnObj.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire up DrawingManager
        var dmSO = new SerializedObject(dm);
        dmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        dmSO.FindProperty("_ui").objectReferenceValue = ui;
        dmSO.ApplyModifiedProperties();

        // Wire up BallController
        var bcSO = new SerializedObject(bc);
        bcSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bcSO.FindProperty("_drawingManager").objectReferenceValue = dm;
        bcSO.ApplyModifiedProperties();

        // Wire up GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_drawingManager").objectReferenceValue = dm;
        gmSO.FindProperty("_ballController").objectReferenceValue = bc;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Save scene
        string scenePath = "Assets/Scenes/051v2_DrawBridge.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup051v2] シーン作成完了: " + scenePath);
    }

    static GameObject CreateCliff(string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.position = position;

        if (sprite != null)
        {
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
            float scaleX = size.x / (sprite.rect.width / sprite.pixelsPerUnit);
            float scaleY = size.y / (sprite.rect.height / sprite.pixelsPerUnit);
            obj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Box collider for cliff surface
        var col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f); // normalized, actual size via scale
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        return obj;
    }

    static PhysicsMaterial2D CreateBounceMaterial()
    {
        string matPath = "Assets/Resources/Sprites/Game051v2_DrawBridge/BallMaterial.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (existing != null) return existing;

        var mat = new PhysicsMaterial2D("BallMaterial");
        mat.bounciness = 0.1f;
        mat.friction = 0.5f;
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    static InstructionPanel BuildInstructionPanel(TMP_FontAsset font)
    {
        var ipCanvasObj = new GameObject("InstructionPanelCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        ipCanvasObj.AddComponent<GraphicRaycaster>();

        var ipCanvasScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipCanvasScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasScaler.matchWidthOrHeight = 0.5f;

        var ip = ipCanvasObj.AddComponent<InstructionPanel>();

        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(ipCanvasObj.transform, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.12f, 0.05f, 0.92f);

        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(800f, 90f); titleRt.anchoredPosition = new Vector2(0f, 250f);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.font = font; titleTmp.fontSize = 60; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = Color.white;

        var descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(panelObj.transform, false);
        var descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f); descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(800f, 80f); descRt.anchoredPosition = new Vector2(0f, 140f);
        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.font = font; descTmp.fontSize = 34; descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = Color.white; descTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = new GameObject("ControlsText");
        ctrlObj.transform.SetParent(panelObj.transform, false);
        var ctrlRt = ctrlObj.AddComponent<RectTransform>();
        ctrlRt.anchorMin = new Vector2(0.5f, 0.5f); ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRt.sizeDelta = new Vector2(800f, 120f); ctrlRt.anchoredPosition = new Vector2(0f, 0f);
        var ctrlTmp = ctrlObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.font = font; ctrlTmp.fontSize = 28; ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.9f, 0.9f, 0.7f); ctrlTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(panelObj.transform, false);
        var goalRt = goalObj.AddComponent<RectTransform>();
        goalRt.anchorMin = new Vector2(0.5f, 0.5f); goalRt.anchorMax = new Vector2(0.5f, 0.5f);
        goalRt.sizeDelta = new Vector2(800f, 80f); goalRt.anchoredPosition = new Vector2(0f, -100f);
        var goalTmp = goalObj.AddComponent<TextMeshProUGUI>();
        goalTmp.font = font; goalTmp.fontSize = 30; goalTmp.alignment = TextAlignmentOptions.Center;
        goalTmp.color = new Color(1f, 0.85f, 0.3f); goalTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(panelObj.transform, false);
        var startBtnRt = startBtnObj.AddComponent<RectTransform>();
        startBtnRt.anchorMin = new Vector2(0.5f, 0.5f); startBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRt.sizeDelta = new Vector2(260f, 70f); startBtnRt.anchoredPosition = new Vector2(0f, -220f);
        var startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0.1f, 0.55f, 0.15f);
        var startBtn = startBtnObj.AddComponent<Button>();
        var startLabelObj = new GameObject("Label");
        startLabelObj.transform.SetParent(startBtnObj.transform, false);
        var startLabelRt = startLabelObj.AddComponent<RectTransform>();
        startLabelRt.anchorMin = Vector2.zero; startLabelRt.anchorMax = Vector2.one;
        startLabelRt.offsetMin = Vector2.zero; startLabelRt.offsetMax = Vector2.zero;
        var startLabelTmp = startLabelObj.AddComponent<TextMeshProUGUI>();
        startLabelTmp.font = font; startLabelTmp.text = "はじめる";
        startLabelTmp.fontSize = 38; startLabelTmp.alignment = TextAlignmentOptions.Center; startLabelTmp.color = Color.white;

        var qBtnObj = new GameObject("QuestionButton");
        qBtnObj.transform.SetParent(ipCanvasObj.transform, false);
        var qBtnRt = qBtnObj.AddComponent<RectTransform>();
        qBtnRt.anchorMin = new Vector2(1f, 0f); qBtnRt.anchorMax = new Vector2(1f, 0f);
        qBtnRt.pivot = new Vector2(1f, 0f);
        qBtnRt.sizeDelta = new Vector2(70f, 70f); qBtnRt.anchoredPosition = new Vector2(-10f, 10f);
        var qBtnImg = qBtnObj.AddComponent<Image>();
        qBtnImg.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        var qBtn = qBtnObj.AddComponent<Button>();
        var qLabelObj = new GameObject("Label");
        qLabelObj.transform.SetParent(qBtnObj.transform, false);
        var qLabelRt = qLabelObj.AddComponent<RectTransform>();
        qLabelRt.anchorMin = Vector2.zero; qLabelRt.anchorMax = Vector2.one;
        qLabelRt.offsetMin = Vector2.zero; qLabelRt.offsetMax = Vector2.zero;
        var qLabelTmp = qLabelObj.AddComponent<TextMeshProUGUI>();
        qLabelTmp.font = font; qLabelTmp.text = "?";
        qLabelTmp.fontSize = 40; qLabelTmp.alignment = TextAlignmentOptions.Center; qLabelTmp.color = Color.white;
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

    static GameObject CreateText(Transform parent, string name, string text, TMP_FontAsset font, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = font; tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = color;
        return obj;
    }

    static GameObject CreateButton(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color bgColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        obj.AddComponent<Button>();
        if (!string.IsNullOrEmpty(label))
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font; tmp.text = label; tmp.fontSize = 30;
            tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        }
        return obj;
    }

    static GameObject CreatePanel(Transform parent, string name, Color bgColor, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
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
        var list = new List<EditorBuildSettingsScene>(scenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Setup051v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
