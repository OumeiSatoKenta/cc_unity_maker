using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game056v2_InflateFloat;

public static class Setup056v2_InflateFloat
{
    [MenuItem("Assets/Setup/056v2 InflateFloat")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup056v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game056v2_InflateFloat/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
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
            bgObj.transform.localScale = new Vector3(0.022f, 0.055f, 1f);
        }

        // Load sprites
        Sprite balloonSprite = LoadSprite(sp + "Balloon.png");
        Sprite obstacleSprite = LoadSprite(sp + "Obstacle.png");
        Sprite coinSprite = LoadSprite(sp + "Coin.png");
        Sprite goalSprite = LoadSprite(sp + "GoalFlag.png");
        Sprite spikeSprite = LoadSprite(sp + "Spike.png");

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<InflateFloatGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager"); smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // Balloon
        var balloonObj = new GameObject("Balloon");
        balloonObj.transform.position = new Vector3(0f, -1.5f, 0f);
        var balloonSr = balloonObj.AddComponent<SpriteRenderer>();
        balloonSr.sprite = balloonSprite;
        balloonSr.sortingOrder = 10;
        var balloonCol = balloonObj.AddComponent<CircleCollider2D>();
        balloonCol.isTrigger = true;
        var balloonRb = balloonObj.AddComponent<Rigidbody2D>();
        balloonRb.gravityScale = 0f;
        balloonRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var balloon = balloonObj.AddComponent<BalloonController>();

        // CourseManager
        var courseObj = new GameObject("CourseManager"); courseObj.transform.SetParent(gmObj.transform);
        var course = courseObj.AddComponent<CourseManager>();
        var courseSO = new SerializedObject(course);
        courseSO.FindProperty("_gm").objectReferenceValue = gm;
        courseSO.FindProperty("_balloon").objectReferenceValue = balloon;
        courseSO.FindProperty("_obstacleSprite").objectReferenceValue = obstacleSprite;
        courseSO.FindProperty("_coinSprite").objectReferenceValue = coinSprite;
        courseSO.FindProperty("_goalSprite").objectReferenceValue = goalSprite;
        courseSO.FindProperty("_spikeSprite").objectReferenceValue = spikeSprite;
        courseSO.ApplyModifiedProperties();

        // BalloonController wire
        var balloonSO = new SerializedObject(balloon);
        balloonSO.FindProperty("_gm").objectReferenceValue = gm;
        balloonSO.FindProperty("_spriteRenderer").objectReferenceValue = balloonSr;
        balloonSO.FindProperty("_collider").objectReferenceValue = balloonCol;
        balloonSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 50), new Vector2(0, -25));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 30, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 45), new Vector2(20, -25));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Inflate Gauge
        var gaugeLabel = CT(canvasObj.transform, "GaugeLabel", "膨張", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(80, 36), new Vector2(-100, -25));

        var inflateGaugeObj = new GameObject("InflateGauge", typeof(RectTransform));
        inflateGaugeObj.transform.SetParent(canvasObj.transform, false);
        var slider = inflateGaugeObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
        var gaugeRT = inflateGaugeObj.GetComponent<RectTransform>();
        gaugeRT.anchorMin = new Vector2(1, 1); gaugeRT.anchorMax = new Vector2(1, 1);
        gaugeRT.pivot = new Vector2(1, 1); gaugeRT.sizeDelta = new Vector2(200, 30); gaugeRT.anchoredPosition = new Vector2(-20, -60);
        // Bg
        var gaugeBg = new GameObject("Background", typeof(RectTransform)); gaugeBg.transform.SetParent(inflateGaugeObj.transform, false);
        var gaugeBgImg = gaugeBg.AddComponent<Image>(); gaugeBgImg.color = new Color(0.3f, 0.3f, 0.3f);
        var gaugeBgRT = gaugeBg.GetComponent<RectTransform>(); gaugeBgRT.anchorMin = Vector2.zero; gaugeBgRT.anchorMax = Vector2.one; gaugeBgRT.offsetMin = gaugeBgRT.offsetMax = Vector2.zero;
        slider.targetGraphic = gaugeBgImg;
        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(inflateGaugeObj.transform, false);
        var fillAreaRT = fillArea.GetComponent<RectTransform>(); fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one; fillAreaRT.offsetMin = fillAreaRT.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>(); fillImg.color = new Color(0.3f, 0.9f, 0.3f);
        var fillRT = fill.GetComponent<RectTransform>(); fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one; fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        slider.fillRect = fillRT;

        // Distance slider
        var distLabel = CT(canvasObj.transform, "DistLabel", "ゴール", 24, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(80, 36), new Vector2(20, 80));
        var distSliderObj = new GameObject("DistanceSlider", typeof(RectTransform));
        distSliderObj.transform.SetParent(canvasObj.transform, false);
        var distSlider = distSliderObj.AddComponent<Slider>();
        distSlider.direction = Slider.Direction.LeftToRight;
        distSlider.minValue = 0f; distSlider.maxValue = 1f; distSlider.value = 0f;
        var distRT = distSliderObj.GetComponent<RectTransform>();
        distRT.anchorMin = new Vector2(0, 0); distRT.anchorMax = new Vector2(0, 0);
        distRT.pivot = new Vector2(0, 0); distRT.sizeDelta = new Vector2(300, 30); distRT.anchoredPosition = new Vector2(100, 80);
        var dBg = new GameObject("Background", typeof(RectTransform)); dBg.transform.SetParent(distSliderObj.transform, false);
        var dBgImg = dBg.AddComponent<Image>(); dBgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var dBgRT = dBg.GetComponent<RectTransform>(); dBgRT.anchorMin = Vector2.zero; dBgRT.anchorMax = Vector2.one; dBgRT.offsetMin = dBgRT.offsetMax = Vector2.zero;
        distSlider.targetGraphic = dBgImg;
        var dFillArea = new GameObject("Fill Area", typeof(RectTransform)); dFillArea.transform.SetParent(distSliderObj.transform, false);
        var dFillAreaRT = dFillArea.GetComponent<RectTransform>(); dFillAreaRT.anchorMin = Vector2.zero; dFillAreaRT.anchorMax = Vector2.one; dFillAreaRT.offsetMin = dFillAreaRT.offsetMax = Vector2.zero;
        var dFill = new GameObject("Fill", typeof(RectTransform)); dFill.transform.SetParent(dFillArea.transform, false);
        var dFillImg = dFill.AddComponent<Image>(); dFillImg.color = new Color(1f, 0.8f, 0f);
        var dFillRT = dFill.GetComponent<RectTransform>(); dFillRT.anchorMin = Vector2.zero; dFillRT.anchorMax = Vector2.one; dFillRT.offsetMin = dFillRT.offsetMax = Vector2.zero;
        distSlider.fillRect = dFillRT;

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "COMBO x3!", 48, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(500, 80), Vector2.zero);
        comboText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.SetActive(false);

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
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        cpTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cpTitle.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var cpScore = CT(clearPanel.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        cpScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var cpNextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 34, jpFont,
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
        var gcScore = CT(gcPanel.transform, "ScoreText", "Total Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), Vector2.zero);
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetry = CB(gcPanel.transform, "RetryButton", "もう一度", 34, jpFont,
            new Vector2(0.3f, 0.25f), new Vector2(0.3f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
        var gcMenu = CB(gcPanel.transform, "MenuButton", "メニューへ", 34, jpFont,
            new Vector2(0.7f, 0.25f), new Vector2(0.7f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
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
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetry = CB(goPanel.transform, "RetryButton", "もう一度", 34, jpFont,
            new Vector2(0.3f, 0.25f), new Vector2(0.3f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", 34, jpFont,
            new Vector2(0.7f, 0.25f), new Vector2(0.7f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        goPanel.SetActive(false);

        // InstructionPanel overlay
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipCanvasComp = ipCanvas.AddComponent<Canvas>();
        ipCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvasComp.sortingOrder = 100;
        ipCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)ipCanvas.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipRoot = new GameObject("InstructionPanel", typeof(RectTransform));
        ipRoot.transform.SetParent(ipCanvas.transform, false);
        var ipImg = ipRoot.AddComponent<Image>(); ipImg.color = new Color(0, 0.1f, 0.2f, 0.95f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one; ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "", 52, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = Color.cyan;

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
            new Color(0.2f, 0.6f, 0.6f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(65, 65), new Vector2(-20, 90),
            new Color(0.3f, 0.4f, 0.5f, 0.9f));

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

        // InflateFloatUI
        var uiObj = new GameObject("InflateFloatUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<InflateFloatUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gm").objectReferenceValue = gm;
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_inflateGauge").objectReferenceValue = slider;
        uiSO.FindProperty("_inflateGaugeFill").objectReferenceValue = fillImg;
        uiSO.FindProperty("_distanceSlider").objectReferenceValue = distSlider;
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = cpScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageBtn").objectReferenceValue = cpNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_retryBtn").objectReferenceValue = goRetry.GetComponent<Button>();
        uiSO.FindProperty("_retryBtn2").objectReferenceValue = gcRetry.GetComponent<Button>();
        uiSO.FindProperty("_menuBtn").objectReferenceValue = goMenu.GetComponent<Button>();
        uiSO.FindProperty("_menuBtn2").objectReferenceValue = gcMenu.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_balloon").objectReferenceValue = balloon;
        gmSO.FindProperty("_courseManager").objectReferenceValue = course;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button persistent listeners
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cpNextBtn.GetComponent<Button>().onClick, gm.OnNextStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.OnBackToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/056v2_InflateFloat.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup056v2] InflateFloat シーンを作成しました: " + scenePath);
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
