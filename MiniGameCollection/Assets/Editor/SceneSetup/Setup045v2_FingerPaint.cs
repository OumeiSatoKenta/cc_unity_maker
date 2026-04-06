using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game045v2_FingerPaint;

public static class Setup045v2_FingerPaint
{
    [MenuItem("Assets/Setup/045v2 FingerPaint")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup045v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game045v2_FingerPaint/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.15f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Canvas.png", sp+"PaletteFrame.png",
            sp+"BrushIcon.png", sp+"EraserIcon.png", sp+"PerfectEffect.png",
            sp+"TemplateOverlay.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spCanvas   = LoadSprite(sp + "Canvas.png");
        Sprite spTemplate = LoadSprite(sp + "TemplateOverlay.png");
        Sprite spBrush    = LoadSprite(sp + "BrushIcon.png");
        Sprite spEraser   = LoadSprite(sp + "EraserIcon.png");

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

        // Calculate canvas world size
        // Top margin: 1.5 units for HUD, Bottom: 2.5 units for palette UI
        float topMargin = 1.5f;
        float bottomMargin = 2.5f;
        float availH = camSize * 2f - topMargin - bottomMargin;
        float canvasSize = Mathf.Min(availH, camWidth * 1.8f);
        float canvasY = (camSize - topMargin) - canvasSize / 2f; // centered in game area

        // Canvas SpriteRenderer (drawing surface)
        var canvasGO = new GameObject("DrawingCanvas");
        var canvasSr = canvasGO.AddComponent<SpriteRenderer>();
        if (spCanvas != null) canvasSr.sprite = spCanvas;
        canvasSr.sortingOrder = 0;
        float spriteU = spCanvas != null ? spCanvas.rect.width / spCanvas.pixelsPerUnit : 1f;
        float canvasScale = canvasSize / spriteU;
        canvasGO.transform.position = new Vector3(0f, canvasY, 0f);
        canvasGO.transform.localScale = Vector3.one * canvasScale;

        // Template overlay SpriteRenderer
        var templateGO = new GameObject("TemplateOverlay");
        var templateSr = templateGO.AddComponent<SpriteRenderer>();
        if (spTemplate != null) templateSr.sprite = spTemplate;
        templateSr.sortingOrder = 1;
        templateSr.color = new Color(1f, 1f, 1f, 0.35f);
        float tmplU = spTemplate != null ? spTemplate.rect.width / spTemplate.pixelsPerUnit : 1f;
        float tmplScale = canvasSize / tmplU;
        templateGO.transform.position = new Vector3(0f, canvasY, 0f);
        templateGO.transform.localScale = Vector3.one * tmplScale;

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<FingerPaintGameManager>();

        // StageManager (child)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // FingerPaintCanvas (child)
        var fpCanvasObj = new GameObject("FingerPaintCanvas");
        fpCanvasObj.transform.SetParent(gmObj.transform);
        var fpCanvas = fpCanvasObj.AddComponent<FingerPaintCanvas>();

        // Wire FingerPaintCanvas
        var fpCanvasSO = new SerializedObject(fpCanvas);
        fpCanvasSO.FindProperty("_gameManager").objectReferenceValue = gm;
        fpCanvasSO.FindProperty("_canvasRenderer").objectReferenceValue = canvasSr;
        fpCanvasSO.FindProperty("_templateRenderer").objectReferenceValue = templateSr;
        fpCanvasSO.ApplyModifiedProperties();

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.7f);

        var matchText = CT(canvasUIObj.transform, "MatchRateText", "0%", 44, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(200,60), new Vector2(0,-15));
        matchText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        matchText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var targetText = CT(canvasUIObj.transform, "TargetText", "目標: 50%", 28, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(200,40), new Vector2(0,-65));
        targetText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        targetText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        var timerText = CT(canvasUIObj.transform, "TimerText", "60", 40, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(120,55), new Vector2(-10,-15));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var comboText = CT(canvasUIObj.transform, "ComboText", "COMBO x3!", 38, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(320,60), new Vector2(0,0));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        comboText.SetActive(false);

        // Ink slider
        var inkSliderObj = CreateSlider(canvasUIObj.transform, "InkSlider",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(700, 28), new Vector2(0, 205),
            new Color(0.25f, 0.55f, 0.9f, 0.9f));
        var inkSlider = inkSliderObj.GetComponent<Slider>();

        // Ink label
        var inkLabel = CT(canvasUIObj.transform, "InkLabel", "インク残量", 24, jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(200,36), new Vector2(0,237));
        inkLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        inkLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);

        // === Palette (5 color buttons, bottom) ===
        // Buttons positioned horizontally at the bottom
        Color[] paletteColors = {
            new Color(0.20f, 0.60f, 0.86f),
            new Color(0.95f, 0.35f, 0.25f),
            new Color(0.25f, 0.75f, 0.35f),
            new Color(0.95f, 0.75f, 0.10f),
            new Color(0.65f, 0.30f, 0.85f),
        };
        Button[] colorBtns = new Button[5];
        float[] colorBtnX = { -360f, -180f, 0f, 180f, 360f };
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            var btn = CB(canvasUIObj.transform, $"ColorButton{i}", "", jpFont,
                new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
                new Vector2(100,100), new Vector2(colorBtnX[i], 95),
                paletteColors[i]);
            colorBtns[i] = btn.GetComponent<Button>();
        }

        // Thin brush toggle button
        var thinBrushBtn = CB(canvasUIObj.transform, "ThinBrushButton", "細", jpFont,
            new Vector2(0f,0f), new Vector2(0f,0f), new Vector2(0f,0f),
            new Vector2(90,60), new Vector2(10, 15),
            new Color(0.3f, 0.5f, 0.7f));
        thinBrushBtn.SetActive(false); // shown from Stage3

        // Eraser button
        var eraserBtn = CB(canvasUIObj.transform, "EraserButton", "消しゴム", jpFont,
            new Vector2(1f,0f), new Vector2(1f,0f), new Vector2(1f,0f),
            new Vector2(150,60), new Vector2(-10, 15),
            new Color(0.7f, 0.3f, 0.3f));
        eraserBtn.SetActive(false); // shown from Stage4

        // Menu button (always visible, bottom center below palette)
        var menuBtn = CB(canvasUIObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
            new Vector2(160,55), new Vector2(0, 15), new Color(0.3f,0.3f,0.35f));
        // Reposition menu button lower
        menuBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-350, 15);
        var menuBtnComp = menuBtn.GetComponent<Button>();

        // === Stage Clear Panel ===
        var scPanel = CreatePanel(canvasUIObj.transform, "StageClearPanel", new Color(0.05f,0.15f,0.08f,0.95f), new Vector2(520,380));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,70), new Vector2(0,140));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.5f);
        var scScore = CT(scPanel.transform, "StageClearScore", "スコア: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,55), new Vector2(0,60));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var scStars = CT(scPanel.transform, "StageClearStars", "★★★", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,65), new Vector2(0,-20));
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.1f);
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(260,60), new Vector2(0,-90), new Color(0.2f,0.6f,0.2f));
        var scMenuBtn2 = CB(scPanel.transform, "MenuButton2", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,55), new Vector2(0,-160), new Color(0.35f,0.35f,0.4f));
        scPanel.SetActive(false);

        // === Final Clear Panel ===
        var fcPanel = CreatePanel(canvasUIObj.transform, "FinalClearPanel", new Color(0.05f,0.12f,0.05f,0.95f), new Vector2(540,380));
        var fcTitle = CT(fcPanel.transform, "FinalClearTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,70), new Vector2(0,145));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.2f);
        var fcScore = CT(fcPanel.transform, "FinalScore", "最終スコア: 0", 40, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,60), new Vector2(0,60));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var fcMenu = CB(fcPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(0,-80), new Color(0.35f,0.35f,0.4f));
        fcPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = CreatePanel(canvasUIObj.transform, "GameOverPanel", new Color(0.12f,0.05f,0.05f,0.95f), new Vector2(520,340));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,70), new Vector2(0,120));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.3f,0.3f);
        var goMatchText = CT(goPanel.transform, "GameOverMatch", "一致率: 0%", 34, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,80), new Vector2(0,30));
        goMatchText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goMatchText.GetComponent<TextMeshProUGUI>().color = Color.white;
        var goRetry = CB(goPanel.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(-110,-70), new Color(0.2f,0.6f,0.2f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(110,-70), new Color(0.35f,0.35f,0.4f));
        goPanel.SetActive(false);

        // === FingerPaintUI (child of GameManager) ===
        var uiObj = new GameObject("FingerPaintUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<FingerPaintUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_canvas").objectReferenceValue = fpCanvas;
        uiSO.FindProperty("_matchRateText").objectReferenceValue = matchText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_targetText").objectReferenceValue = targetText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider;
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        // Color buttons array
        var colorBtnsProp = uiSO.FindProperty("_colorButtons");
        colorBtnsProp.arraySize = 5;
        for (int i = 0; i < 5; i++)
            colorBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = colorBtns[i];
        uiSO.FindProperty("_thinBrushButton").objectReferenceValue = thinBrushBtn.GetComponent<Button>();
        uiSO.FindProperty("_eraserButton").objectReferenceValue = eraserBtn.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStarsText").objectReferenceValue = scStars.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverMatchText").objectReferenceValue = goMatchText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_canvas").objectReferenceValue = fpCanvas;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button events
        // Color buttons
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            var btn = colorBtns[i];
            btn.onClick.AddListener(() => ui.OnColorButtonPressed(idx));
        }
        thinBrushBtn.GetComponent<Button>().onClick.AddListener(() => ui.OnThinBrushToggle());
        eraserBtn.GetComponent<Button>().onClick.AddListener(() => ui.OnEraserToggle());
        scNextBtn.GetComponent<Button>().onClick.AddListener(() => ui.OnNextStageButtonPressed());

        // Back to menu buttons
        AddBackToMenuListener(menuBtnComp);
        AddBackToMenuListener(scMenuBtn2.GetComponent<Button>());
        AddBackToMenuListener(fcMenu.GetComponent<Button>());
        AddBackToMenuListener(goMenu.GetComponent<Button>());

        // Retry: just reload scene
        goRetry.GetComponent<Button>().onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        // Save
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/045v2_FingerPaint.unity");
        AddSceneToBuildSettings("Assets/Scenes/045v2_FingerPaint.unity");
        Debug.Log("[Setup045v2] シーン生成完了: Assets/Scenes/045v2_FingerPaint.unity");
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

        // Title
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

        // Description
        var descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(panelObj.transform, false);
        var descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f);
        descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(800, 60);
        descRt.anchoredPosition = new Vector2(0, 100);
        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.font = font;
        descTmp.fontSize = 34;
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = Color.white;

        // Controls
        var ctrlObj = new GameObject("ControlsText");
        ctrlObj.transform.SetParent(panelObj.transform, false);
        var ctrlRt = ctrlObj.AddComponent<RectTransform>();
        ctrlRt.anchorMin = new Vector2(0.5f, 0.5f);
        ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRt.sizeDelta = new Vector2(800, 80);
        ctrlRt.anchoredPosition = new Vector2(0, 0);
        var ctrlTmp = ctrlObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.font = font;
        ctrlTmp.fontSize = 28;
        ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.9f, 0.9f, 0.7f);
        ctrlTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        // Goal
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

        // Start button
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

        // ? button (re-show panel)
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

        // Wire InstructionPanel
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
        var btn = obj.AddComponent<Button>();

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

    static GameObject CreateSlider(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color fillColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(obj.transform, false);
        var bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = Vector2.zero;
        fillAreaRt.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;

        var slider = obj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        return obj;
    }

    static void AddBackToMenuListener(Button btn)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => SceneLoader.BackToMenu());
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = UnityEditor.EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;

        var newScenes = new UnityEditor.EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++) newScenes[i] = scenes[i];
        newScenes[scenes.Length] = new UnityEditor.EditorBuildSettingsScene(scenePath, true);
        UnityEditor.EditorBuildSettings.scenes = newScenes;
    }
}
