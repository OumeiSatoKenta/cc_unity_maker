using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game025v2_TowerDefend;

public static class Setup025v2_TowerDefend
{
    [MenuItem("Assets/Setup/025v2 TowerDefend")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup025v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game025v2_TowerDefend/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.09f, 0.18f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Ensure sprites imported
        string[] spritePaths = {
            sp+"Background.png", sp+"Wall.png",
            sp+"EnemyNormal.png", sp+"EnemyFast.png", sp+"EnemyFlying.png", sp+"EnemyBreaker.png",
            sp+"StartMarker.png", sp+"GoalMarker.png", sp+"InkBar.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg = LoadSprite(sp + "Background.png");
        Sprite spWall = LoadSprite(sp + "Wall.png");
        Sprite spNormal = LoadSprite(sp + "EnemyNormal.png");
        Sprite spFast = LoadSprite(sp + "EnemyFast.png");
        Sprite spFlying = LoadSprite(sp + "EnemyFlying.png");
        Sprite spBreaker = LoadSprite(sp + "EnemyBreaker.png");
        Sprite spStart = LoadSprite(sp + "StartMarker.png");
        Sprite spGoal = LoadSprite(sp + "GoalMarker.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float camH = 10f;
            float camAsp = camera != null ? camera.aspect : 9f / 16f;
            float camW = 5f * camAsp * 2f;
            float scaleX = camW / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camH / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Grid field markers (Start / Goal)
        // camSize=5, topMargin=1.2, bottomMargin=2.8 -> available height=6
        // Grid: cellSize=0.5, gridW=10, gridH=12, origin=(-2.5, -4.3+bottomMargin/2)
        float camSize = 5f;
        float gridOriginX = -2.5f;
        float gridOriginY = -camSize + 2.8f; // bottom of game area

        Vector3 startPos1 = new Vector3(gridOriginX + 0.25f, 0f, 0f);  // left side center
        Vector3 startPos2 = new Vector3(0f, camSize - 1.4f, 0f);       // top center (stage 5)
        Vector3 goalPos = new Vector3(-gridOriginX - 0.25f, 0f, 0f);   // right side center

        var startMarker1 = new GameObject("StartMarker1");
        startMarker1.transform.position = startPos1;
        startMarker1.transform.localScale = Vector3.one * 0.5f;
        var smSr1 = startMarker1.AddComponent<SpriteRenderer>();
        smSr1.sprite = spStart;
        smSr1.sortingOrder = 2;

        var startMarker2 = new GameObject("StartMarker2");
        startMarker2.transform.position = startPos2;
        startMarker2.transform.localScale = Vector3.one * 0.5f;
        var smSr2 = startMarker2.AddComponent<SpriteRenderer>();
        smSr2.sprite = spStart;
        smSr2.sortingOrder = 2;
        startMarker2.SetActive(false); // visible only in stage 5

        var goalMarker = new GameObject("GoalMarker");
        goalMarker.transform.position = goalPos;
        goalMarker.transform.localScale = Vector3.one * 0.5f;
        var gmSr = goalMarker.AddComponent<SpriteRenderer>();
        gmSr.sprite = spGoal;
        gmSr.sortingOrder = 2;

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<TowerDefendGameManager>();

        // StageManager (child)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var stageMgr = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(stageMgr);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // WallManager (child)
        var wallObj = new GameObject("WallManager");
        wallObj.transform.SetParent(gmObj.transform);
        var wallMgr = wallObj.AddComponent<WallManager>();
        var wallSO = new SerializedObject(wallMgr);
        wallSO.FindProperty("_wallSprite").objectReferenceValue = spWall;
        wallSO.ApplyModifiedProperties();

        // WaveManager (child)
        var waveObj = new GameObject("WaveManager");
        waveObj.transform.SetParent(gmObj.transform);
        var waveMgr = waveObj.AddComponent<WaveManager>();
        var waveSO = new SerializedObject(waveMgr);
        waveSO.FindProperty("_spriteNormal").objectReferenceValue = spNormal;
        waveSO.FindProperty("_spriteFast").objectReferenceValue = spFast;
        waveSO.FindProperty("_spriteFlying").objectReferenceValue = spFlying;
        waveSO.FindProperty("_spriteBreaker").objectReferenceValue = spBreaker;
        waveSO.ApplyModifiedProperties();

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var evObj = new GameObject("EventSystem");
        evObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evObj.AddComponent<InputSystemUIInputModule>();

        // HUD: Stage (top-left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // HUD: Wave (top-center)
        var waveText = CT(canvasObj.transform, "WaveText", "Wave 0 / 2", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(280, 55), new Vector2(0, -30));
        waveText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        waveText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        // HUD: Score (top-right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        // HUD: Breach (below stage)
        var breachText = CT(canvasObj.transform, "BreachText", "突破 0 / 5", 30, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280, 48), new Vector2(20, -82));
        breachText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Ink bar (below wave text)
        var inkBarBg = new GameObject("InkBarBg");
        inkBarBg.transform.SetParent(canvasObj.transform, false);
        var inkBgRt = inkBarBg.AddComponent<RectTransform>();
        inkBgRt.anchorMin = new Vector2(0.5f, 1f);
        inkBgRt.anchorMax = new Vector2(0.5f, 1f);
        inkBgRt.pivot = new Vector2(0.5f, 1f);
        inkBgRt.sizeDelta = new Vector2(320, 28);
        inkBgRt.anchoredPosition = new Vector2(0, -82);
        inkBarBg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.8f);

        var inkSliderObj = new GameObject("InkSlider");
        inkSliderObj.transform.SetParent(canvasObj.transform, false);
        var inkSliderRt = inkSliderObj.AddComponent<RectTransform>();
        inkSliderRt.anchorMin = new Vector2(0.5f, 1f);
        inkSliderRt.anchorMax = new Vector2(0.5f, 1f);
        inkSliderRt.pivot = new Vector2(0.5f, 1f);
        inkSliderRt.sizeDelta = new Vector2(320, 28);
        inkSliderRt.anchoredPosition = new Vector2(0, -82);
        var inkSlider = inkSliderObj.AddComponent<Slider>();
        inkSlider.value = 1f;
        inkSlider.interactable = false;
        var inkFillArea = new GameObject("Fill Area"); inkFillArea.transform.SetParent(inkSliderObj.transform, false);
        var inkFillAreaRt = inkFillArea.AddComponent<RectTransform>();
        inkFillAreaRt.anchorMin = Vector2.zero; inkFillAreaRt.anchorMax = Vector2.one;
        inkFillAreaRt.offsetMin = Vector2.zero; inkFillAreaRt.offsetMax = Vector2.zero;
        var inkFill = new GameObject("Fill"); inkFill.transform.SetParent(inkFillArea.transform, false);
        var inkFillRt = inkFill.AddComponent<RectTransform>();
        inkFillRt.anchorMin = Vector2.zero; inkFillRt.anchorMax = Vector2.one;
        inkFillRt.offsetMin = Vector2.zero; inkFillRt.offsetMax = Vector2.zero;
        var inkFillImg = inkFill.AddComponent<Image>();
        inkFillImg.color = new Color(0f, 0.74f, 0.83f, 0.9f);
        inkSlider.fillRect = inkFillRt;
        inkSlider.targetGraphic = inkFillImg;

        var inkLabel = CT(canvasObj.transform, "InkLabel", "インク", 24, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(100, 28), new Vector2(-180, -82));
        inkLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 1f);

        var inkPercentText = CT(canvasObj.transform, "InkPercentText", "100%", 24, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(80, 28), new Vector2(185, -82));
        inkPercentText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 1f);

        // Bonus pop text (center)
        var bonusPopText = CT(canvasObj.transform, "BonusPop", "", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 100), new Vector2(0, 200));
        bonusPopText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        bonusPopText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        bonusPopText.SetActive(false);

        // Screen flash overlay
        var flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(canvasObj.transform, false);
        var flashRt = flashObj.AddComponent<RectTransform>();
        flashRt.anchorMin = Vector2.zero; flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = Vector2.zero; flashRt.offsetMax = Vector2.zero;
        var flashImg = flashObj.AddComponent<Image>();
        flashImg.color = Color.clear;
        flashImg.raycastTarget = false;
        flashObj.SetActive(false);

        // Bottom buttons
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(-185, 55), new Color(0.15f, 0.25f, 0.4f));

        var waveStartBtn = CB(canvasObj.transform, "WaveStartButton", "Wave開始！", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(90, 55), new Color(0.1f, 0.5f, 0.2f));

        var reShowBtn = CB(canvasObj.transform, "ReShowInstructionBtn", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(75, 75), new Vector2(-10, 55), new Color(0.2f, 0.35f, 0.55f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f), new Vector2(720, 420));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);
        var scScoreText = CT(scPanel.transform, "StageClearScoreText", "スコア: 0", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 55), new Vector2(0, 50));
        scScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var scBonusText = CT(scPanel.transform, "StageClearBonusText", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 55), new Vector2(0, -10));
        scBonusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonusText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30), new Color(0.1f, 0.5f, 0.8f));
        scPanel.SetActive(false);

        // ---- Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.05f, 0.15f, 0.05f, 0.95f), new Vector2(720, 500));
        var gcTitle = CT(gcPanel.transform, "ClearTitle", "ゲームクリア！", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 80), new Vector2(0, -35));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);
        var gcScoreText = CT(gcPanel.transform, "ClearScoreText", "最終スコア: 0", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 60), new Vector2(0, 40));
        gcScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcMenuBtn = CB(gcPanel.transform, "ClearMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 70), new Vector2(0, 30), new Color(0.15f, 0.25f, 0.4f));
        gcPanel.SetActive(false);

        // ---- GameOver Panel ----
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f), new Vector2(720, 500));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 80), new Vector2(0, -35));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);
        var goScoreText = CT(goPanel.transform, "GameOverScoreText", "スコア: 0", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 90), new Vector2(0, 40));
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(-145, 30), new Color(0.5f, 0.15f, 0.1f));
        var goMenuBtn = CB(goPanel.transform, "GameOverMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(145, 30), new Color(0.15f, 0.25f, 0.4f));
        goPanel.SetActive(false);

        // ---- InstructionPanel ----
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipScaler.matchWidthOrHeight = 0.5f;
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipPanel = new GameObject("InstructionPanel");
        ipPanel.transform.SetParent(ipCanvas.transform, false);
        var ipPanelImg = ipPanel.AddComponent<Image>();
        ipPanelImg.color = new Color(0.05f, 0.1f, 0.2f, 0.97f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "TowerDefend", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 220));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 110));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.95f, 1f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 120), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.85f, 1f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -110));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -240), new Color(0.1f, 0.4f, 0.7f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ---- TowerDefendUI component ----
        var uiObj = new GameObject("TowerDefendUI");
        uiObj.transform.SetParent(gmObj.transform);
        var tdUI = uiObj.AddComponent<TowerDefendUI>();
        var uiSO = new SerializedObject(tdUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_waveText").objectReferenceValue = waveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_breachText").objectReferenceValue = breachText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider;
        uiSO.FindProperty("_inkPercentText").objectReferenceValue = inkPercentText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_waveStartButton").objectReferenceValue = waveStartBtn.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearBonusText").objectReferenceValue = scBonusText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = gcScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearMenuButton").objectReferenceValue = gcMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_bonusPopText").objectReferenceValue = bonusPopText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_screenFlash").objectReferenceValue = flashImg;
        uiSO.ApplyModifiedProperties();

        // Wire WaveManager
        var waveSO2 = new SerializedObject(waveMgr);
        // WaveManager gets references via Initialize() at runtime
        waveSO2.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_waveManager").objectReferenceValue = waveMgr;
        gmSO.FindProperty("_wallManager").objectReferenceValue = wallMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = tdUI;
        gmSO.ApplyModifiedProperties();

        // Button Events
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), gm, "ShowInstructions");
        AddButtonOnClick(waveStartBtn.GetComponent<Button>(), gm, "OnStartWaveButton");
        AddButtonOnClick(nextBtn.GetComponent<Button>(), gm, "OnNextStage");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");

        // Save Scene
        string scenePath = "Assets/Scenes/025v2_TowerDefend.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup025v2] Scene created: " + scenePath);
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
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
            else Debug.LogWarning($"[Setup025v2] Sprite not found: {path}");
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
