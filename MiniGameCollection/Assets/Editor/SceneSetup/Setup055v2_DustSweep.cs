using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game055v2_DustSweep;

public static class Setup055v2_DustSweep
{
    [MenuItem("Assets/Setup/055v2 DustSweep")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup055v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game055v2_DustSweep/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.28f, 0.49f, 0.28f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.022f, 0.022f, 1f);
        }

        // Item sprites
        Sprite[] itemSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
            itemSprites[i] = LoadSprite(sp + $"HiddenItem{i+1}.png");

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DustSweepGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager"); smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // DustBoard
        var boardObj = new GameObject("DustBoard");
        boardObj.transform.position = new Vector3(0f, 0.8f, 0f);
        var dustRenderer = new GameObject("DustSpriteRenderer"); dustRenderer.transform.SetParent(boardObj.transform);
        var dustSr = dustRenderer.AddComponent<SpriteRenderer>(); dustSr.sortingOrder = 2;
        var hardRenderer = new GameObject("HardDustRenderer"); hardRenderer.transform.SetParent(boardObj.transform);
        var hardSr = hardRenderer.AddComponent<SpriteRenderer>(); hardSr.sortingOrder = 3;
        var dangerRenderer = new GameObject("DangerZoneRenderer"); dangerRenderer.transform.SetParent(boardObj.transform);
        var dangerSr = dangerRenderer.AddComponent<SpriteRenderer>(); dangerSr.sortingOrder = 1;
        var itemContainer = new GameObject("ItemContainer"); itemContainer.transform.SetParent(boardObj.transform);

        var db = boardObj.AddComponent<DustBoard>();
        var dbSO = new SerializedObject(db);
        dbSO.FindProperty("_gm").objectReferenceValue = gm;
        dbSO.FindProperty("_dustRenderer").objectReferenceValue = dustSr;
        dbSO.FindProperty("_hardDustRenderer").objectReferenceValue = hardSr;
        dbSO.FindProperty("_dangerRenderer").objectReferenceValue = dangerSr;
        dbSO.FindProperty("_itemContainer").objectReferenceValue = itemContainer.transform;
        var itemsProp = dbSO.FindProperty("_hiddenItemSprites");
        itemsProp.arraySize = 5;
        for (int i = 0; i < 5; i++)
            itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemSprites[i];
        dbSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD - top area
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(400,50), new Vector2(0,-25));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var timerText = CT(canvasObj.transform, "TimerText", "60", 44, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(150,60), new Vector2(-20,-20));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 30, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(300,45), new Vector2(20,-25));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Cleanliness slider
        var sliderLabel = CT(canvasObj.transform, "CleanLabel", "清潔度", 26, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(120,36), new Vector2(20,-80));
        sliderLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,1f,0.8f);

        var sliderObj = new GameObject("CleanlinessSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(canvasObj.transform, false);
        var cleanSlider = sliderObj.AddComponent<Slider>();
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0,1); sliderRT.anchorMax = new Vector2(1,1);
        sliderRT.pivot = new Vector2(0.5f,1); sliderRT.sizeDelta = new Vector2(-40,24);
        sliderRT.anchoredPosition = new Vector2(0,-110);
        var sliderBg = new GameObject("Background", typeof(RectTransform)); sliderBg.transform.SetParent(sliderObj.transform, false);
        sliderBg.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.8f);
        var bgRT = sliderBg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(sliderObj.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one; faRT.offsetMin = faRT.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.2f,0.85f,0.2f);
        cleanSlider.fillRect = fill.GetComponent<RectTransform>();
        cleanSlider.targetGraphic = fill.GetComponent<Image>();
        cleanSlider.minValue = 0f; cleanSlider.maxValue = 1f; cleanSlider.value = 0f;
        cleanSlider.interactable = false;

        var cleanPctText = CT(canvasObj.transform, "CleanlinessText", "0%", 26, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(100,36), new Vector2(-20,-80));
        cleanPctText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        cleanPctText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,1f,0.8f);

        // Combo text (center)
        var comboText = CT(canvasObj.transform, "ComboText", "", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(600,80), new Vector2(0,80));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        comboText.SetActive(false);

        // Penalty text (center)
        var penaltyText = CT(canvasObj.transform, "PenaltyText", "-5秒！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(400,70), new Vector2(0,0));
        penaltyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        penaltyText.GetComponent<TextMeshProUGUI>().color = Color.red;
        penaltyText.SetActive(false);

        // Brush size buttons - bottom bar
        var brushPanel = new GameObject("BrushPanel", typeof(RectTransform));
        brushPanel.transform.SetParent(canvasObj.transform, false);
        var bpRT = brushPanel.GetComponent<RectTransform>();
        bpRT.anchorMin = new Vector2(0.5f,0); bpRT.anchorMax = new Vector2(0.5f,0); bpRT.pivot = new Vector2(0.5f,0);
        bpRT.sizeDelta = new Vector2(400,60); bpRT.anchoredPosition = new Vector2(0, 80);

        var brushSmallBtn = CB(brushPanel.transform, "BrushSmallBtn", "小", 28, jpFont,
            new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(110,55), new Vector2(0,0),
            new Color(0.2f,0.5f,0.7f,0.9f));
        var brushMedBtn = CB(brushPanel.transform, "BrushMedBtn", "中", 28, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(110,55), new Vector2(0,0),
            new Color(0.2f,0.4f,0.7f,0.9f));
        var brushLargeBtn = CB(brushPanel.transform, "BrushLargeBtn", "大", 28, jpFont,
            new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(110,55), new Vector2(0,0),
            new Color(0.1f,0.3f,0.7f,0.9f));

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20),
            new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Stage Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.85f,0.95f,0.85f,0.95f));
        var clearText = CT(clearPanel.transform, "ClearText", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(650,100), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f,0.5f,0.1f);
        var stageClearScoreText = CT(clearPanel.transform, "StageClearScore", "Score: 0", 34, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        stageClearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var nextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(300,65), Vector2.zero,
            new Color(0.2f,0.6f,0.2f));
        clearPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.9f,0.95f,0.8f,0.95f));
        CT(gcPanel.transform, "GCTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.72f), new Vector2(0.5f,0.72f), new Vector2(0.5f,0.5f), new Vector2(700,80), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcScoreText = CT(gcPanel.transform, "GCScore", "最終スコア: 0", 34, jpFont,
            new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        gcScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetryBtn = CB(gcPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.35f,0.25f), new Vector2(0.35f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.3f,0.6f,0.2f));
        var gcMenuBtn = CB(gcPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.65f,0.25f), new Vector2(0.65f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.3f,0.3f,0.5f));
        gcPanel.SetActive(false);

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.85f,0.85f,0.95f));
        CT(goPanel.transform, "GOTitle", "タイムアップ！", 48, jpFont,
            new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.35f,0.25f), new Vector2(0.35f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.6f,0.2f,0.2f));
        var goMenuBtn = CB(goPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.65f,0.25f), new Vector2(0.65f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.3f,0.3f,0.5f));
        goPanel.SetActive(false);

        // InstructionPanel
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
        ipRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "DustSweep", 64, jpFont,
            new Vector2(0.5f,0.8f), new Vector2(0.5f,0.8f), new Vector2(0.5f,0.5f), new Vector2(800,100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.4f);

        var ipDesc = CT(ipRoot.transform, "DescriptionText", "", 34, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(900,80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipRoot.transform, "ControlsText", "", 30, jpFont,
            new Vector2(0.5f,0.52f), new Vector2(0.5f,0.52f), new Vector2(0.5f,0.5f), new Vector2(900,120), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.9f,0.9f,0.7f);

        var ipGoal = CT(ipRoot.transform, "GoalText", "", 30, jpFont,
            new Vector2(0.5f,0.38f), new Vector2(0.5f,0.38f), new Vector2(0.5f,0.5f), new Vector2(900,80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,1f,0.8f);

        var ipStartBtn = CB(ipRoot.transform, "StartButton", "はじめる", 36, jpFont,
            new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(260,70), Vector2.zero,
            new Color(0.2f,0.6f,0.2f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1,0), new Vector2(1,0), new Vector2(1,0), new Vector2(65,65), new Vector2(-20,90),
            new Color(0.3f,0.5f,0.3f,0.9f));

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

        // DustSweepUI
        var uiObj = new GameObject("DustSweepUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DustSweepUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_cleanlinessSlider").objectReferenceValue = cleanSlider;
        uiSO.FindProperty("_cleanlinessText").objectReferenceValue = cleanPctText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_penaltyText").objectReferenceValue = penaltyText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = stageClearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearRetryButton").objectReferenceValue = gcRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearMenuButton").objectReferenceValue = gcMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_gm").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_dustBoard").objectReferenceValue = db;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Brush button listeners
        UnityEditor.Events.UnityEventTools.AddPersistentListener(brushSmallBtn.GetComponent<Button>().onClick,
            () => db.SetBrushSize(0));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(brushMedBtn.GetComponent<Button>().onClick,
            () => db.SetBrushSize(1));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(brushLargeBtn.GetComponent<Button>().onClick,
            () => db.SetBrushSize(2));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcRetryBtn.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcMenuBtn.GetComponent<Button>().onClick, gm.OnBackToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/055v2_DustSweep.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup055v2] DustSweep シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.05f, 0.3f); r.anchorMax = new Vector2(0.95f, 0.7f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg;
        o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return o;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
