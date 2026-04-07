using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using Game050v2_BubbleSort;

public static class Setup050v2_BubbleSort
{
    [MenuItem("Assets/Setup/050v2 BubbleSort")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup050v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game050v2_BubbleSort/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.91f, 0.96f, 0.91f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite imports
        string[] spritePaths = {
            sp+"Background.png",
            sp+"Bubble_Green.png", sp+"Bubble_Yellow.png",
            sp+"Bubble_Blue.png", sp+"Bubble_Red.png", sp+"Bubble_Purple.png",
            sp+"Bubble_Fixed.png", sp+"Bubble_Timer.png", sp+"Bubble_Bomb.png",
            sp+"BubbleSelected.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spGreen    = LoadSprite(sp + "Bubble_Green.png");
        Sprite spYellow   = LoadSprite(sp + "Bubble_Yellow.png");
        Sprite spBlue     = LoadSprite(sp + "Bubble_Blue.png");
        Sprite spRed      = LoadSprite(sp + "Bubble_Red.png");
        Sprite spPurple   = LoadSprite(sp + "Bubble_Purple.png");
        Sprite spFixed    = LoadSprite(sp + "Bubble_Fixed.png");
        Sprite spTimer    = LoadSprite(sp + "Bubble_Timer.png");
        Sprite spBomb     = LoadSprite(sp + "Bubble_Bomb.png");
        Sprite spSelected = LoadSprite(sp + "BubbleSelected.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;

        // Background
        if (spBg != null)
        {
            float camWidth = camera != null ? camSize * camera.aspect : 2.8f;
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camWidth * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BubbleSortGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        // Stage configs are set at runtime via BubbleSortGameManager.StartGame() → SetConfigs()
        // Default configs from StageManager.InitializeDefaultConfigs() are used as fallback

        // BubbleGridManager
        var gridObj = new GameObject("BubbleGrid");
        var gridMgr = gridObj.AddComponent<BubbleGridManager>();

        var gridSO = new SerializedObject(gridMgr);
        gridSO.FindProperty("_gameManager").objectReferenceValue = gm;

        var colorSpritesArr = gridSO.FindProperty("_colorSprites");
        colorSpritesArr.arraySize = 5;
        colorSpritesArr.GetArrayElementAtIndex(0).objectReferenceValue = spGreen;
        colorSpritesArr.GetArrayElementAtIndex(1).objectReferenceValue = spYellow;
        colorSpritesArr.GetArrayElementAtIndex(2).objectReferenceValue = spBlue;
        colorSpritesArr.GetArrayElementAtIndex(3).objectReferenceValue = spRed;
        colorSpritesArr.GetArrayElementAtIndex(4).objectReferenceValue = spPurple;
        gridSO.FindProperty("_fixedSprite").objectReferenceValue = spFixed;
        gridSO.FindProperty("_timerSprite").objectReferenceValue = spTimer;
        gridSO.FindProperty("_bombSprite").objectReferenceValue = spBomb;
        gridSO.FindProperty("_selectedSprite").objectReferenceValue = spSelected;
        gridSO.ApplyModifiedProperties();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        var canvasTransform = canvasObj.GetComponent<RectTransform>();

        // --- HUD (top) ---
        var stageTextObj = CreateText(canvasTransform, "StageText", "Stage 1 / 5", jpFont, 36,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 50f), new Vector2(160f, -30f), Color.white);

        var movesTextObj = CreateText(canvasTransform, "MovesText", "手数: 10", jpFont, 36,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(250f, 50f), new Vector2(0f, -30f), Color.white);

        var scoreTextObj = CreateText(canvasTransform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(200f, 50f), new Vector2(-110f, -30f), Color.white);

        // Combo text (center)
        var comboTextObj = CreateText(canvasTransform, "ComboText", "×2 COMBO!", jpFont, 48,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 60f), new Vector2(0f, 200f), Color.yellow);
        comboTextObj.SetActive(false);

        // Bonus text (floating)
        var bonusTextObj = CreateText(canvasTransform, "BonusText", "", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, 50f), new Vector2(0f, 100f), Color.yellow);
        bonusTextObj.SetActive(false);

        // --- Bottom buttons ---
        // Menu button (bottom center-left)
        var menuBtnObj = CreateButton(canvasTransform, "MenuButton", "メニュー", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 60f), new Vector2(100f, 35f),
            new Color(0.2f, 0.2f, 0.3f, 0.9f));
        menuBtnObj.AddComponent<BackToMenuButton>();

        // Undo button (bottom right)
        var undoBtnObj = CreateButton(canvasTransform, "UndoButton", "Undo", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(160f, 60f), new Vector2(-90f, 35f),
            new Color(0.4f, 0.2f, 0.1f, 0.9f));

        // --- Stage Clear Panel ---
        var scPanel = CreatePanel(canvasTransform, "StageClearPanel", new Color(0f, 0f, 0f, 0.75f), new Vector2(600f, 400f));
        CreateText(scPanel.transform, "TitleText", "ステージクリア！", jpFont, 52,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 70f), new Vector2(0f, 100f), Color.yellow);
        var scScoreObj = CreateText(scPanel.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 50f), new Vector2(0f, 20f), Color.white);
        var nextBtnObj = CreateButton(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, 65f), new Vector2(0f, -80f),
            new Color(0.1f, 0.5f, 0.8f));
        scPanel.SetActive(false);

        // --- All Clear Panel ---
        var acPanel = CreatePanel(canvasTransform, "AllClearPanel", new Color(0f, 0.1f, 0.3f, 0.9f), new Vector2(700f, 500f));
        CreateText(acPanel.transform, "TitleText", "ALL CLEAR!!", jpFont, 60,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600f, 80f), new Vector2(0f, 130f), Color.yellow);
        var acScoreObj = CreateText(acPanel.transform, "ScoreText", "Total Score: 0", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 50f), new Vector2(0f, 30f), Color.white);
        var acRestartBtnObj = CreateButton(acPanel.transform, "RestartButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 60f), new Vector2(-130f, -100f),
            new Color(0.1f, 0.5f, 0.2f));
        var acMenuBtnObj = CreateButton(acPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 60f), new Vector2(130f, -100f),
            new Color(0.3f, 0.3f, 0.4f));
        acMenuBtnObj.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // --- Game Over Panel ---
        var goPanel = CreatePanel(canvasTransform, "GameOverPanel", new Color(0.3f, 0f, 0f, 0.85f), new Vector2(600f, 450f));
        CreateText(goPanel.transform, "TitleText", "GAME OVER", jpFont, 52,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 70f), new Vector2(0f, 110f), Color.red);
        var goScoreObj = CreateText(goPanel.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 50f), new Vector2(0f, 30f), Color.white);
        var goRestartBtnObj = CreateButton(goPanel.transform, "RestartButton", "リトライ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 60f), new Vector2(-130f, -90f),
            new Color(0.1f, 0.5f, 0.2f));
        var goMenuBtnObj = CreateButton(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 60f), new Vector2(130f, -90f),
            new Color(0.3f, 0.3f, 0.4f));
        goMenuBtnObj.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // --- InstructionPanel ---
        var ip = BuildInstructionPanel(jpFont);

        // --- UI component ---
        var uiObj = new GameObject("BubbleSortUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<BubbleSortUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_movesText").objectReferenceValue = movesTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_bonusText").objectReferenceValue = bonusTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreObj.GetComponent<TMP_Text>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager fields
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_gridManager").objectReferenceValue = gridMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Wire button callbacks
        nextBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.GoNextStage());
        acRestartBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());
        goRestartBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());
        undoBtnObj.GetComponent<Button>().onClick.AddListener(() => gridMgr.UndoLastSwap());

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/050v2_BubbleSort.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup050v2] BubbleSort シーン作成完了: " + scenePath);
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
        Debug.Log($"[Setup050v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
