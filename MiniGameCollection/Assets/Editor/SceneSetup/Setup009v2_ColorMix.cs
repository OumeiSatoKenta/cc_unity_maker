using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game009v2_ColorMix;

public static class Setup009v2_ColorMix
{
    [MenuItem("Assets/Setup/009v2 ColorMix")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup009v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game009v2_ColorMix/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.gameObject.name = "Main Camera";
        }

        // Sprites
        Sprite spBg = LoadSprite(sp + "background.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.024f, 0.024f, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<ColorMixGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // ColorMixManager
        var mixObj = new GameObject("ColorMixManager");
        mixObj.transform.SetParent(gmObj.transform);
        var mixMgr = mixObj.AddComponent<ColorMixManager>();

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage text (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(350, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score text (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(350, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Combo text (top center)
        var comboText = CT(canvasObj.transform, "ComboText", "", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 45), new Vector2(0, -65));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.2f);
        comboText.SetActive(false);

        // Judge count text (under stage text)
        var judgeText = CT(canvasObj.transform, "JudgeCountText", "残り判定: 3回", 28, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 45), new Vector2(20, -65));
        judgeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);
        judgeText.SetActive(false);

        // ---- Color preview area (center-upper) ----
        // Target color panel (left)
        var targetLabel = CT(canvasObj.transform, "TargetLabel", "目標色", 28, jpFont,
            new Vector2(0.15f, 0.65f), new Vector2(0.15f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 40), Vector2.zero);
        targetLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        targetLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        var targetPanel = new GameObject("TargetColorPanel", typeof(RectTransform));
        targetPanel.transform.SetParent(canvasObj.transform, false);
        var targetImg = targetPanel.AddComponent<Image>();
        targetImg.color = Color.red;
        var targetR = targetPanel.GetComponent<RectTransform>();
        targetR.anchorMin = new Vector2(0.05f, 0.52f);
        targetR.anchorMax = new Vector2(0.45f, 0.64f);
        targetR.offsetMin = targetR.offsetMax = Vector2.zero;

        // Mix color panel (right)
        var mixLabel = CT(canvasObj.transform, "MixLabel", "混色", 28, jpFont,
            new Vector2(0.85f, 0.65f), new Vector2(0.85f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 40), Vector2.zero);
        mixLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        mixLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        var mixPanel = new GameObject("MixColorPanel", typeof(RectTransform));
        mixPanel.transform.SetParent(canvasObj.transform, false);
        var mixImg = mixPanel.AddComponent<Image>();
        mixImg.color = Color.black;
        var mixR = mixPanel.GetComponent<RectTransform>();
        mixR.anchorMin = new Vector2(0.55f, 0.52f);
        mixR.anchorMax = new Vector2(0.95f, 0.64f);
        mixR.offsetMin = mixR.offsetMax = Vector2.zero;

        // ---- Sliders area ----
        // R Slider
        var (rSlider, rSliderGo) = CreateSlider(canvasObj.transform, "SliderR", "R",
            new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.45f),
            new Vector2(0, 0), new Vector2(0, 40), jpFont, new Color(1f, 0.3f, 0.3f));

        // G Slider
        var (gSlider, gSliderGo) = CreateSlider(canvasObj.transform, "SliderG", "G",
            new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.38f),
            new Vector2(0, 0), new Vector2(0, 40), jpFont, new Color(0.3f, 1f, 0.3f));

        // B Slider
        var (bSlider, bSliderGo) = CreateSlider(canvasObj.transform, "SliderB", "B",
            new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.31f),
            new Vector2(0, 0), new Vector2(0, 40), jpFont, new Color(0.3f, 0.5f, 1f));

        // V Slider (brightness, stage 3+) - grouped
        var vGroup = new GameObject("BrightnessSliderGroup", typeof(RectTransform));
        vGroup.transform.SetParent(canvasObj.transform, false);
        var vGroupR = vGroup.GetComponent<RectTransform>();
        vGroupR.anchorMin = new Vector2(0.1f, 0.24f);
        vGroupR.anchorMax = new Vector2(0.9f, 0.24f);
        vGroupR.offsetMin = new Vector2(0, 0);
        vGroupR.offsetMax = new Vector2(0, 40);

        var (vSlider, _) = CreateSlider(vGroup.transform, "SliderV", "明度",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, jpFont, new Color(1f, 0.9f, 0.3f));
        vSlider.minValue = 0f;
        vSlider.maxValue = 1f;
        vSlider.value = 1f;
        vGroup.SetActive(false);

        // Set R/G/B slider range to 0-255
        rSlider.minValue = 0f; rSlider.maxValue = 255f; rSlider.value = 0f;
        gSlider.minValue = 0f; gSlider.maxValue = 255f; gSlider.value = 0f;
        bSlider.minValue = 0f; bSlider.maxValue = 255f; bSlider.value = 0f;

        // Judge button
        var judgeBtn = CB(canvasObj.transform, "JudgeButton", "判定", 36, jpFont,
            new Vector2(0.1f, 0f), new Vector2(0.6f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 70), new Vector2(0, 150), new Color(0.1f, 0.4f, 0.8f));

        // Reset button
        var resetBtn = CB(canvasObj.transform, "ResetButton", "リセット", 28, jpFont,
            new Vector2(0.65f, 0f), new Vector2(0.95f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 65), new Vector2(0, 150), new Color(0.35f, 0.25f, 0.25f, 0.9f));

        // Menu button (bottom right)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 24, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(180, 55), new Vector2(-20, 20), new Color(0.2f, 0.25f, 0.35f, 0.85f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f));
        var scTitle = CT(scPanel.transform, "SCTitle", "", 36, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 1f);
        var scScore = CT(scPanel.transform, "SCScore", "", 28, jpFont,
            new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 45), Vector2.zero);
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var scDeltaE = CT(scPanel.transform, "SCDeltaE", "", 26, jpFont,
            new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 42), Vector2.zero);
        scDeltaE.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scDeltaE.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
        var scStars = CT(scPanel.transform, "SCStars", "★★★", 42, jpFont,
            new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 55), Vector2.zero);
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 30, jpFont,
            new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 65), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));
        scPanel.SetActive(false);

        // ---- Game Clear Panel ----
        var clearPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.12f, 0.18f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "全ステージクリア！", 40, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScore = CT(clearPanel.transform, "ClearScore", "", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 50), Vector2.zero);
        clearScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var clearMenuBtn = CB(clearPanel.transform, "ClearMenuButton", "メニューへ", 30, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 65), Vector2.zero, new Color(0.2f, 0.4f, 0.6f));
        clearPanel.SetActive(false);

        // ---- Game Over Panel ----
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f));
        CT(goPanel.transform, "GOTitle", "ゲームオーバー", 40, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        CT(goPanel.transform, "GOMessage", "判定回数を超過しました", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 45), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 30, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 65), Vector2.zero, new Color(0.5f, 0.15f, 0.15f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 60), Vector2.zero, new Color(0.2f, 0.25f, 0.35f));
        goPanel.SetActive(false);

        // ---- InstructionPanel ----
        var ip = CreateInstructionPanel(canvasObj, jpFont, gm);

        // ---- Wire ColorMixUI ----
        var uiComp = canvasObj.AddComponent<ColorMixUI>();
        var uiSO = new SerializedObject(uiComp);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_judgeCountText").objectReferenceValue = judgeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_brightnessSliderGroup").objectReferenceValue = vGroup;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearTitle").objectReferenceValue = scTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearScore").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStars").objectReferenceValue = scStars.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearDeltaE").objectReferenceValue = scDeltaE.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_gameClearScore").objectReferenceValue = clearScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.ApplyModifiedProperties();

        // ---- Wire ColorMixManager ----
        var mixSO = new SerializedObject(mixMgr);
        mixSO.FindProperty("_gameManager").objectReferenceValue = gm;
        mixSO.FindProperty("_targetColorImage").objectReferenceValue = targetImg;
        mixSO.FindProperty("_mixColorImage").objectReferenceValue = mixImg;
        mixSO.FindProperty("_sliderR").objectReferenceValue = rSlider;
        mixSO.FindProperty("_sliderG").objectReferenceValue = gSlider;
        mixSO.FindProperty("_sliderB").objectReferenceValue = bSlider;
        mixSO.FindProperty("_sliderV").objectReferenceValue = vSlider;
        mixSO.FindProperty("_mainCamera").objectReferenceValue = camera;
        mixSO.ApplyModifiedProperties();

        // ---- Wire GameManager ----
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_colorMixManager").objectReferenceValue = mixMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = uiComp;
        gmSO.ApplyModifiedProperties();

        // ---- Button Events ----
        UnityEditor.Events.UnityEventTools.AddPersistentListener(judgeBtn.GetComponent<Button>().onClick, mixMgr.OnJudgePressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, mixMgr.ResetSliders);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStageButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, gm.OnRetryButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/009v2_ColorMix.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup009v2] ColorMix シーンを作成しました: " + scenePath);
    }

    private static InstructionPanel CreateInstructionPanel(GameObject canvasObj, TMP_FontAsset jpFont, ColorMixGameManager gm)
    {
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipPanelObj = new GameObject("Panel", typeof(RectTransform));
        ipPanelObj.transform.SetParent(ipCanvas.transform, false);
        var ipImg = ipPanelObj.AddComponent<Image>();
        ipImg.color = new Color(0.05f, 0.05f, 0.15f, 0.97f);
        var ipR = ipPanelObj.GetComponent<RectTransform>();
        ipR.anchorMin = Vector2.zero;
        ipR.anchorMax = Vector2.one;
        ipR.offsetMin = ipR.offsetMax = Vector2.zero;

        CT(ipPanelObj.transform, "TitleText", "ColorMix", 52, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 70), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        CT(ipPanelObj.transform, "DescText", "スライダーで色を混ぜて目標の色を再現するパズル", 30, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 55), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        CT(ipPanelObj.transform, "ControlsText", "R/G/Bスライダーをドラッグして色を調整", 28, jpFont,
            new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 55), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        CT(ipPanelObj.transform, "GoalText", "目標色にできるだけ近い色を作ろう", 28, jpFont,
            new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 55), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var startBtn = CB(ipPanelObj.transform, "StartButton", "はじめる", 36, jpFont,
            new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 75), Vector2.zero, new Color(0.15f, 0.45f, 0.8f));

        var ip = ipCanvas.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanelObj;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipPanelObj.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipPanelObj.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipPanelObj.transform.Find("ControlsText").GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipPanelObj.transform.Find("GoalText").GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = startBtn.GetComponent<Button>();

        // "?" button on main canvas
        var questionBtn = CB(canvasObj.transform, "QuestionButton", "?", 32, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(70, 70), new Vector2(-100, 20), new Color(0.2f, 0.3f, 0.5f, 0.85f));
        ipSO.FindProperty("_helpButton").objectReferenceValue = questionBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        return ip;
    }

    private static (Slider slider, GameObject go) CreateSlider(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        TMP_FontAsset jpFont, Color labelColor)
    {
        var group = new GameObject(name + "Group", typeof(RectTransform));
        group.transform.SetParent(parent, false);
        var gr = group.GetComponent<RectTransform>();
        gr.anchorMin = anchorMin;
        gr.anchorMax = anchorMax;
        gr.offsetMin = offsetMin;
        gr.offsetMax = offsetMax;

        // Label
        var labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(group.transform, false);
        var lt = labelObj.AddComponent<TextMeshProUGUI>();
        lt.text = label;
        lt.fontSize = 28;
        lt.color = labelColor;
        lt.alignment = TextAlignmentOptions.MidlineRight;
        if (jpFont != null) lt.font = jpFont;
        var lr = labelObj.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0f);
        lr.anchorMax = new Vector2(0.1f, 1f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        // Slider
        var sliderObj = new GameObject("Slider", typeof(RectTransform));
        sliderObj.transform.SetParent(group.transform, false);
        var sr = sliderObj.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.12f, 0f);
        sr.anchorMax = new Vector2(1f, 1f);
        sr.offsetMin = sr.offsetMax = Vector2.zero;

        var slider = sliderObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;

        // Background
        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(sliderObj.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = new Vector2(0f, 0.25f);
        bgR.anchorMax = new Vector2(1f, 0.75f);
        bgR.offsetMin = bgR.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faR = fillArea.GetComponent<RectTransform>();
        faR.anchorMin = new Vector2(0f, 0.25f);
        faR.anchorMax = new Vector2(1f, 0.75f);
        faR.offsetMin = new Vector2(5f, 0);
        faR.offsetMax = new Vector2(-15f, 0);

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(labelColor.r, labelColor.g, labelColor.b, 0.7f);
        var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = new Vector2(0, 1f);
        fillR.offsetMin = fillR.offsetMax = Vector2.zero;
        slider.fillRect = fillR;

        // Handle slide area
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        var haR = handleArea.GetComponent<RectTransform>();
        haR.anchorMin = Vector2.zero;
        haR.anchorMax = Vector2.one;
        haR.offsetMin = new Vector2(10f, 0);
        haR.offsetMax = new Vector2(-10f, 0);

        var handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        var hImg = handle.AddComponent<Image>();
        hImg.color = Color.white;
        var hR = handle.GetComponent<RectTransform>();
        hR.sizeDelta = new Vector2(30f, 40f);
        slider.handleRect = hR;

        return (slider, group);
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
        r.anchorMin = new Vector2(0.05f, 0.25f);
        r.anchorMax = new Vector2(0.95f, 0.75f);
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
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
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
