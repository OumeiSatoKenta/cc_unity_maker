using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game094v2_GravityPainter;

public static class Setup094v2_GravityPainter
{
    [MenuItem("Assets/Setup/094v2 GravityPainter")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup094v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game094v2_GravityPainter/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.04f, 0.02f, 0.08f);
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
            float camS = 6f;
            float camW = camS * (16f / 9f);
            float scX = camW * 2f / bgSprite.bounds.size.x;
            float scY = camS * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scX, scY, 1f);
        }

        // === Load paint sprites (order: red=0, blue=1, green=2, yellow=3) ===
        Sprite sprCellEmpty   = LoadSprite(sp + "cell_empty.png");
        Sprite sprCellWall    = LoadSprite(sp + "cell_wall.png");
        Sprite sprCellAbsorb  = LoadSprite(sp + "cell_absorb.png");
        Sprite sprTargetOverlay = LoadSprite(sp + "target_overlay.png");
        Sprite sprPaintRed    = LoadSprite(sp + "paint_red.png");
        Sprite sprPaintBlue   = LoadSprite(sp + "paint_blue.png");
        Sprite sprPaintGreen  = LoadSprite(sp + "paint_green.png");
        Sprite sprPaintYellow = LoadSprite(sp + "paint_yellow.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("GravityPainterGameManager");
        var gm = gmObj.AddComponent<GravityPainterGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.2f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.5f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.7f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 4, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // PaintManager (child of GM)
        var pmObj = new GameObject("PaintManager");
        pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<PaintManager>();

        // === Canvas (main HUD) ===
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
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 48), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.4f, 1f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 48), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.4f, 1f);

        var matchRateTextGo = CT(canvasObj.transform, "MatchRateText", "一致率: 0%", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 52), new Vector2(0, -15));
        matchRateTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        matchRateTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var paintCountTextGo = CT(canvasObj.transform, "PaintCountText", "絵の具: 8", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280, 44), new Vector2(15, -65));
        paintCountTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.6f, 0.2f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(380, 44), new Vector2(-15, -65));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var timeTextGo = CT(canvasObj.transform, "TimeText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(320, 44), new Vector2(0, -65));
        timeTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timeTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.3f);

        // === Color palette (bottom area, horizontal) ===
        // Colors: Red, Blue, Green, Yellow
        Color[] paintUIColors = new Color[]
        {
            new Color(0.86f, 0.2f, 0.2f),
            new Color(0.2f, 0.39f, 0.86f),
            new Color(0.2f, 0.78f, 0.31f),
            new Color(0.9f, 0.78f, 0.12f),
        };
        string[] colorLabels = { "赤", "青", "緑", "黄" };
        float[] paletteXOffsets = { -150f, -50f, 50f, 150f };

        var colorButtons = new Button[4];
        var colorButtonImages = new Image[4];

        for (int i = 0; i < 4; i++)
        {
            var colorBtnGo = CB(canvasObj.transform, $"ColorButton{i}", colorLabels[i], 30, jpFont,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(90, 90), new Vector2(paletteXOffsets[i], 350), paintUIColors[i]);
            colorButtons[i] = colorBtnGo.GetComponent<Button>();

            // Highlight frame child
            var frameGo = new GameObject("HighlightFrame", typeof(RectTransform));
            frameGo.transform.SetParent(colorBtnGo.transform, false);
            var frameRT = frameGo.GetComponent<RectTransform>();
            frameRT.anchorMin = Vector2.zero; frameRT.anchorMax = Vector2.one;
            frameRT.offsetMin = new Vector2(-4, -4); frameRT.offsetMax = new Vector2(4, 4);
            var frameImg = frameGo.AddComponent<Image>();
            frameImg.color = new Color(1f, 1f, 1f, 0.3f);
            frameImg.raycastTarget = false;
            colorButtonImages[i] = frameImg;
        }

        // === Gravity Direction Buttons (4 directional) ===
        var gravUpBtn = CB(canvasObj.transform, "GravUpButton", "↑重力", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(150, 58), new Vector2(0, 285), new Color(0.15f, 0.1f, 0.25f));
        var gravDownBtn = CB(canvasObj.transform, "GravDownButton", "↓重力", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(150, 58), new Vector2(0, 220), new Color(0.15f, 0.1f, 0.25f));
        var gravLeftBtn = CB(canvasObj.transform, "GravLeftButton", "←重力", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(150, 58), new Vector2(-165, 253), new Color(0.15f, 0.1f, 0.25f));
        var gravRightBtn = CB(canvasObj.transform, "GravRightButton", "→重力", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(150, 58), new Vector2(165, 253), new Color(0.15f, 0.1f, 0.25f));

        // === Back to Menu Button ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 32, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.3f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f); scRT.sizeDelta = new Vector2(700, 380);
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.02f, 0.12f, 0.95f);

        var scTitleGo = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.4f, 1f);

        var scTextGo = CT(scPanel.transform, "StageClearText", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), Vector2.zero);
        scTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTextGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(360, 70), Vector2.zero, new Color(0.3f, 0.1f, 0.4f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f); acRT.sizeDelta = new Vector2(760, 420);
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.04f, 0.02f, 0.1f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearTitle", "全ステージクリア！", 58, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(720, 80), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.4f, 1f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "Final Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acBackBtn = CB(acPanel.transform, "BackToMenuButton2", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), Vector2.zero, new Color(0.25f, 0.15f, 0.35f));
        acBackBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goPanelRT = goPanel.GetComponent<RectTransform>();
        goPanelRT.anchorMin = new Vector2(0.5f, 0.5f); goPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRT.pivot = new Vector2(0.5f, 0.5f); goPanelRT.sizeDelta = new Vector2(700, 400);
        var goPanelImg = goPanel.AddComponent<Image>();
        goPanelImg.color = new Color(0.15f, 0.02f, 0.02f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 80), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 72), Vector2.zero, new Color(0.5f, 0.15f, 0.15f));
        goPanel.SetActive(false);

        // === InstructionPanel (separate canvas, highest sort order) ===
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
        ipBgImg.color = new Color(0.04f, 0.02f, 0.1f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "GravityPainter", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.4f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.85f, 0.9f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.6f, 1f);

        var startBtnGo = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.25f, 0.1f, 0.35f));

        // Help button ("?") on main canvas
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-15, 90), new Color(0.2f, 0.1f, 0.3f));

        // === Wire InstructionPanel ===
        SetField(ip, "_panelRoot",       ipBg);
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtnGo.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());

        // === GravityPainterUI ===
        var uiObj = new GameObject("GravityPainterUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<GravityPainterUI>();

        SetField(ui, "_stageText",         stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_matchRateText",     matchRateTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_paintCountText",    paintCountTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_timeText",          timeTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_colorButtons",      colorButtons);
        SetField(ui, "_colorButtonImages", colorButtonImages);
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScoreGo.GetComponent<TextMeshProUGUI>());

        // === Wire PaintManager ===
        var sprPaintDrops = new Sprite[] { sprPaintRed, sprPaintBlue, sprPaintGreen, sprPaintYellow };
        SetField(pm, "_gameManager",     gm);
        SetField(pm, "_ui",              ui);
        SetField(pm, "_sprCellEmpty",    sprCellEmpty);
        SetField(pm, "_sprCellWall",     sprCellWall);
        SetField(pm, "_sprCellAbsorb",   sprCellAbsorb);
        SetField(pm, "_sprTargetOverlay", sprTargetOverlay);
        SetField(pm, "_sprPaintDrops",   sprPaintDrops);

        // === Wire GameManager ===
        SetField(gm, "_stageManager",    sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_paintManager",    pm);
        SetField(gm, "_ui",              ui);

        // === Wire button events ===
        scNextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        goRetryBtn.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());

        // Gravity buttons
        gravUpBtn.GetComponent<Button>().onClick.AddListener(() => pm.ApplyGravity(GravityDir.Up));
        gravDownBtn.GetComponent<Button>().onClick.AddListener(() => pm.ApplyGravity(GravityDir.Down));
        gravLeftBtn.GetComponent<Button>().onClick.AddListener(() => pm.ApplyGravity(GravityDir.Left));
        gravRightBtn.GetComponent<Button>().onClick.AddListener(() => pm.ApplyGravity(GravityDir.Right));

        // Color palette buttons
        for (int i = 0; i < 4; i++)
        {
            int colorIndex = i + 1; // 1-based
            int uiIndex = i;
            colorButtons[i].onClick.AddListener(() =>
            {
                pm.SelectColor(colorIndex);
                ui.HighlightColor(uiIndex);
            });
        }

        // === EventSystem ===
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // === Save scene ===
        string scenePath = "Assets/Scenes/094v2_GravityPainter.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup094v2] GravityPainter シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup094v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
