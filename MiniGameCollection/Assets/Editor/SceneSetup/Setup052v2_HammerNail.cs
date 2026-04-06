using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using Game052v2_HammerNail;

public static class Setup052v2_HammerNail
{
    [MenuItem("Assets/Setup/052v2 HammerNail")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup052v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game052v2_HammerNail/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.35f, 0.62f, 0.42f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite imports
        string[] spritePaths = {
            sp+"Background.png", sp+"Board.png",
            sp+"Nail_Normal.png", sp+"Nail_Hard.png", sp+"Nail_Boss.png",
            sp+"Hammer.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spBoard    = LoadSprite(sp + "Board.png");
        Sprite spNailNorm = LoadSprite(sp + "Nail_Normal.png");
        Sprite spNailHard = LoadSprite(sp + "Nail_Hard.png");
        Sprite spNailBoss = LoadSprite(sp + "Nail_Boss.png");
        Sprite spHammer   = LoadSprite(sp + "Hammer.png");

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

        // Board
        var boardObj = new GameObject("Board");
        boardObj.transform.position = new Vector3(0f, 0f, 0f);
        if (spBoard != null)
        {
            var boardSr = boardObj.AddComponent<SpriteRenderer>();
            boardSr.sprite = spBoard;
            boardSr.sortingOrder = 2;
            float bw = camWidth * 1.8f;
            float bh = 0.6f;
            boardSr.drawMode = SpriteDrawMode.Sliced;
            boardSr.size = new Vector2(bw, bh);
        }
        // Board collider
        var boardCol = boardObj.AddComponent<BoxCollider2D>();
        boardCol.size = new Vector2(camWidth * 1.8f, 0.3f);
        boardCol.offset = new Vector2(0f, 0f);

        // Hammer visual (hidden by default, NailManager controls it)
        var hammerObj = new GameObject("Hammer");
        hammerObj.transform.position = new Vector3(0f, 2f, 0f);
        hammerObj.SetActive(false);
        var hammerSr = hammerObj.AddComponent<SpriteRenderer>();
        if (spHammer != null)
        {
            hammerSr.sprite = spHammer;
            hammerSr.sortingOrder = 20;
            hammerObj.transform.localScale = Vector3.one * 0.7f;
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<HammerNailGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform, false);
        var sm = smObj.AddComponent<StageManager>();

        // NailManager
        var nmObj = new GameObject("NailManager");
        nmObj.transform.SetParent(gmObj.transform, false);
        var nm = nmObj.AddComponent<NailManager>();

        // TimingGauge (placeholder until Canvas built)
        var tgObj = new GameObject("TimingGauge");
        tgObj.transform.SetParent(gmObj.transform, false);
        var tg = tgObj.AddComponent<TimingGauge>();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<GraphicRaycaster>();
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // HUD - top area
        var stageObj = CreateText(canvasObj.transform, "StageText", "Stage 1 / 5", jpFont, 36,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300f, 50f), new Vector2(20f, -20f), Color.white);

        var scoreObj = CreateText(canvasObj.transform, "ScoreText", "Score: 0", jpFont, 36,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300f, 50f), new Vector2(-20f, -20f), Color.white);

        var missObj = CreateText(canvasObj.transform, "MissText", "Miss: 0 / 3", jpFont, 30,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(250f, 45f), new Vector2(20f, -75f), new Color(1f, 0.4f, 0.4f));

        var nailsObj = CreateText(canvasObj.transform, "RemainingNailsText", "釘: 3", jpFont, 30,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(200f, 45f), new Vector2(-20f, -75f), new Color(0.9f, 0.9f, 0.5f));

        var comboObj = CreateText(canvasObj.transform, "ComboText", "", jpFont, 40,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 60f), new Vector2(0f, 200f), new Color(1f, 0.85f, 0.2f));
        comboObj.SetActive(false);

        var judgmentObj = CreateText(canvasObj.transform, "JudgmentText", "", jpFont, 60,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 80f), new Vector2(0f, 80f), Color.white);
        judgmentObj.SetActive(false);

        // Timing Gauge UI (bottom area, above menu button)
        var gaugeContainerObj = new GameObject("GaugeContainer");
        gaugeContainerObj.transform.SetParent(canvasObj.transform, false);
        var gaugeContainerRt = gaugeContainerObj.AddComponent<RectTransform>();
        gaugeContainerRt.anchorMin = new Vector2(0.5f, 0f);
        gaugeContainerRt.anchorMax = new Vector2(0.5f, 0f);
        gaugeContainerRt.pivot = new Vector2(0.5f, 0f);
        gaugeContainerRt.sizeDelta = new Vector2(900f, 80f);
        gaugeContainerRt.anchoredPosition = new Vector2(0f, 110f);
        var gaugeContainerImg = gaugeContainerObj.AddComponent<Image>();
        gaugeContainerImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // MISS zone (red - left/right edges)
        var missZoneL = CreateGaugeZone(gaugeContainerObj.transform, "MissZoneL",
            new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(0f,0.5f),
            new Vector2(162f, 0f), new Vector2(81f, 0f), new Color(0.8f, 0.2f, 0.2f, 0.6f));
        var missZoneR = CreateGaugeZone(gaugeContainerObj.transform, "MissZoneR",
            new Vector2(1f,0f), new Vector2(1f,1f), new Vector2(1f,0.5f),
            new Vector2(162f, 0f), new Vector2(-81f, 0f), new Color(0.8f, 0.2f, 0.2f, 0.6f));

        // GOOD zone (green) - centered, wider
        var goodZoneObj = new GameObject("GoodZone");
        goodZoneObj.transform.SetParent(gaugeContainerObj.transform, false);
        var goodZoneRt = goodZoneObj.AddComponent<RectTransform>();
        goodZoneRt.anchorMin = new Vector2(0.5f, 0f); goodZoneRt.anchorMax = new Vector2(0.5f, 1f);
        goodZoneRt.pivot = new Vector2(0.5f, 0.5f);
        goodZoneRt.sizeDelta = new Vector2(324f, 0f); goodZoneRt.anchoredPosition = Vector2.zero;
        var goodZoneImg = goodZoneObj.AddComponent<Image>();
        goodZoneImg.color = new Color(0.3f, 0.85f, 0.3f, 0.5f);

        // PERFECT zone (yellow) - centered, narrower
        var perfectZoneObj = new GameObject("PerfectZone");
        perfectZoneObj.transform.SetParent(gaugeContainerObj.transform, false);
        var perfectZoneRt = perfectZoneObj.AddComponent<RectTransform>();
        perfectZoneRt.anchorMin = new Vector2(0.5f, 0f); perfectZoneRt.anchorMax = new Vector2(0.5f, 1f);
        perfectZoneRt.pivot = new Vector2(0.5f, 0.5f);
        perfectZoneRt.sizeDelta = new Vector2(270f, 0f); perfectZoneRt.anchoredPosition = Vector2.zero;
        var perfectZoneImg = perfectZoneObj.AddComponent<Image>();
        perfectZoneImg.color = new Color(1f, 0.92f, 0.2f, 0.6f);

        // PERFECT label
        var perfectLabelObj = CreateText(gaugeContainerObj.transform, "PerfectLabel", "PERFECT", jpFont, 22,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 30f), new Vector2(0f, -4f), new Color(0.3f, 0.2f, 0f));

        // Indicator (vertical line/marker)
        var indicatorObj = new GameObject("Indicator");
        indicatorObj.transform.SetParent(gaugeContainerObj.transform, false);
        var indicatorRt = indicatorObj.AddComponent<RectTransform>();
        indicatorRt.anchorMin = new Vector2(0f, 0f); indicatorRt.anchorMax = new Vector2(0f, 1f);
        indicatorRt.pivot = new Vector2(0.5f, 0.5f);
        indicatorRt.sizeDelta = new Vector2(8f, 0f); indicatorRt.anchoredPosition = new Vector2(0f, 0f);
        var indicatorImg = indicatorObj.AddComponent<Image>();
        indicatorImg.color = Color.white;

        // Gauge label
        CreateText(canvasObj.transform, "GaugeLabelText", "タイミング", jpFont, 26,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200f, 40f), new Vector2(0f, 195f), new Color(0.9f, 0.9f, 0.9f));

        // Menu button (bottom)
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200f, 60f), new Vector2(0f, 20f), new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Stage Clear Panel
        var scPanelObj = CreatePanel(canvasObj.transform, "StageClearPanel",
            new Color(0f, 0f, 0f, 0.8f), new Vector2(700f, 400f));
        scPanelObj.SetActive(false);
        CreateText(scPanelObj.transform, "StageClearTitle", "ステージクリア！", jpFont, 54,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 100f), new Color(1f, 0.9f, 0.2f));
        var scScoreObj = CreateText(scPanelObj.transform, "StageClearScore", "Score: 0", jpFont, 38,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(500f, 55f), new Vector2(0f, 20f), Color.white);
        var scNextBtnObj = CreateButton(scPanelObj.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300f, 65f), new Vector2(0f, -80f), new Color(0.15f, 0.55f, 0.15f));

        // Game Over Panel
        var goPanelObj = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0.1f, 0f, 0f, 0.88f), new Vector2(700f, 400f));
        goPanelObj.SetActive(false);
        CreateText(goPanelObj.transform, "GameOverTitle", "ゲームオーバー", jpFont, 54,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 100f), new Color(1f, 0.3f, 0.3f));
        var goScoreObj = CreateText(goPanelObj.transform, "GameOverScore", "Score: 0", jpFont, 38,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(500f, 55f), new Vector2(0f, 20f), Color.white);
        var goRetryBtnObj = CreateButton(goPanelObj.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250f, 65f), new Vector2(0f, -80f), new Color(0.55f, 0.15f, 0.15f));

        // All Clear Panel
        var acPanelObj = CreatePanel(canvasObj.transform, "AllClearPanel",
            new Color(0f, 0.05f, 0.1f, 0.88f), new Vector2(700f, 440f));
        acPanelObj.SetActive(false);
        CreateText(acPanelObj.transform, "AllClearTitle", "全ステージクリア！", jpFont, 54,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(620f, 70f), new Vector2(0f, 120f), new Color(0.3f, 1f, 0.5f));
        var acScoreObj = CreateText(acPanelObj.transform, "AllClearScore", "Final Score: 0", jpFont, 38,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(550f, 55f), new Vector2(0f, 30f), Color.white);
        var acRetryBtnObj = CreateButton(acPanelObj.transform, "AllClearRetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250f, 65f), new Vector2(0f, -80f), new Color(0.15f, 0.35f, 0.65f));

        // InstructionPanel
        var ip = BuildInstructionPanel(jpFont);

        // Wire up TimingGauge
        var tgSO = new SerializedObject(tg);
        tgSO.FindProperty("_indicatorImage").objectReferenceValue = indicatorImg;
        tgSO.FindProperty("_perfectZoneImage").objectReferenceValue = perfectZoneImg;
        tgSO.FindProperty("_goodZoneImage").objectReferenceValue = goodZoneImg;
        tgSO.ApplyModifiedProperties();

        // Wire up NailManager
        var nmSO = new SerializedObject(nm);
        nmSO.FindProperty("_normalNailSprite").objectReferenceValue = spNailNorm;
        nmSO.FindProperty("_hardNailSprite").objectReferenceValue = spNailHard;
        nmSO.FindProperty("_bossNailSprite").objectReferenceValue = spNailBoss;
        nmSO.FindProperty("_hammerRenderer").objectReferenceValue = hammerSr;
        nmSO.ApplyModifiedProperties();

        // Wire up HammerNailUI
        var uiObj = new GameObject("HammerNailUI");
        uiObj.transform.SetParent(gmObj.transform, false);
        var ui = uiObj.AddComponent<HammerNailUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missText").objectReferenceValue = missObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_remainingNailsText").objectReferenceValue = nailsObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_judgmentText").objectReferenceValue = judgmentObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanelObj;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanelObj;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanelObj;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_allClearRetryButton").objectReferenceValue = acRetryBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtnObj.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire up HammerNailGameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_nailManager").objectReferenceValue = nm;
        gmSO.FindProperty("_timingGauge").objectReferenceValue = tg;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button events
        var goNextBtn = scNextBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            goNextBtn.onClick, gm.GoNextStage);

        var retryBtn = goRetryBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            retryBtn.onClick, gm.RestartGame);

        var acRetryBtn = acRetryBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            acRetryBtn.onClick, gm.RestartGame);

        var menuBtn = menuBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            menuBtn.onClick, gm.GoToMenu);

        // Save scene
        string scenePath = "Assets/Scenes/052v2_HammerNail.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup052v2] HammerNail シーン作成完了: " + scenePath);
    }

    static GameObject CreateGaugeZone(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = pos;
        var img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
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
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.1f, 0.08f, 0.92f);

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
        startBtnImg.color = new Color(0.15f, 0.55f, 0.2f);
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
        Debug.Log($"[Setup052v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
