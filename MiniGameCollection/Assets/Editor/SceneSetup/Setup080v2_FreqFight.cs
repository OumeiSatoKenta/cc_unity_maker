using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game080v2_FreqFight;

public static class Setup080v2_FreqFight
{
    [MenuItem("Assets/Setup/080v2 FreqFight")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup080v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game080v2_FreqFight/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.01f, 0.10f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            float camSize = 6f;
            float camWidth = camSize * (16f / 9f);
            float scaleX = camWidth * 2f / bgSprite.bounds.size.x;
            float scaleY = camSize * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Load sprites
        Sprite enemyNormal  = LoadSprite(sp + "enemy_normal.png");
        Sprite beatGuideImg = LoadSprite(sp + "beat_guide.png");
        Sprite hpBgImg      = LoadSprite(sp + "hp_bar_bg.png");
        Sprite hpPlayerFill = LoadSprite(sp + "hp_bar_fill_player.png");
        Sprite hpEnemyFill  = LoadSprite(sp + "hp_bar_fill_enemy.png");
        Sprite sliderBgImg  = LoadSprite(sp + "slider_bg.png");
        Sprite sliderHandle = LoadSprite(sp + "slider_handle.png");
        Sprite freqMarkerImg = LoadSprite(sp + "freq_marker.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("FreqFightGameManager");
        var gm = gmObj.AddComponent<FreqFightGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.33f, countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.67f, countMultiplier = 1, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 2.0f,  countMultiplier = 2, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.33f, countMultiplier = 1, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // FreqFightManager
        var fmObj = new GameObject("FreqFightManager");
        fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<FreqFightManager>();

        // === Canvas ===
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
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(350, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 60), new Vector2(0, -100));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var phaseText = CT(canvasObj.transform, "PhaseText", "攻撃フェーズ", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 50), new Vector2(0, -165));
        phaseText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        phaseText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.3f);

        // Judgement text (center)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 100), new Vector2(0, 0));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        judgementText.SetActive(false);

        // === Enemy area (upper-center) ===
        // Enemy 1
        var enemy1Obj = new GameObject("Enemy1");
        enemy1Obj.transform.SetParent(canvasObj.transform, false);
        var enemy1RT = enemy1Obj.AddComponent<RectTransform>();
        enemy1RT.anchorMin = new Vector2(0.5f, 0.75f);
        enemy1RT.anchorMax = new Vector2(0.5f, 0.75f);
        enemy1RT.pivot = new Vector2(0.5f, 0.5f);
        enemy1RT.sizeDelta = new Vector2(160, 160);
        enemy1RT.anchoredPosition = new Vector2(-100, 0);
        var enemy1Img = enemy1Obj.AddComponent<Image>();
        if (enemyNormal != null) enemy1Img.sprite = enemyNormal;
        enemy1Img.preserveAspect = true;

        // Enemy 1 HP bar
        var enemyHp1Bar = CreateHpSlider(canvasObj.transform, "EnemyHp1Slider",
            new Vector2(0.5f, 0.75f), new Vector2(-100, -90), 300, 28,
            hpBgImg, hpEnemyFill, jpFont);

        // Enemy 2 (hidden initially)
        var enemy2Obj = new GameObject("Enemy2");
        enemy2Obj.transform.SetParent(canvasObj.transform, false);
        var enemy2RT = enemy2Obj.AddComponent<RectTransform>();
        enemy2RT.anchorMin = new Vector2(0.5f, 0.75f);
        enemy2RT.anchorMax = new Vector2(0.5f, 0.75f);
        enemy2RT.pivot = new Vector2(0.5f, 0.5f);
        enemy2RT.sizeDelta = new Vector2(160, 160);
        enemy2RT.anchoredPosition = new Vector2(100, 0);
        var enemy2Img = enemy2Obj.AddComponent<Image>();
        if (enemyNormal != null) enemy2Img.sprite = enemyNormal;
        enemy2Img.preserveAspect = true;

        var enemyHp2Bar = CreateHpSlider(canvasObj.transform, "EnemyHp2Slider",
            new Vector2(0.5f, 0.75f), new Vector2(100, -90), 300, 28,
            hpBgImg, hpEnemyFill, jpFont);

        // === Beat Guide (center) ===
        var beatGuideObj = new GameObject("BeatGuide");
        beatGuideObj.transform.SetParent(canvasObj.transform, false);
        var bgRT = beatGuideObj.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.5f, 0.55f);
        bgRT.anchorMax = new Vector2(0.5f, 0.55f);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = new Vector2(100, 100);
        bgRT.anchoredPosition = Vector2.zero;
        var bgImg = beatGuideObj.AddComponent<Image>();
        if (beatGuideImg != null) bgImg.sprite = beatGuideImg;
        bgImg.preserveAspect = true;

        // === Freq Slider 1 (player) ===
        // Track bg
        var sliderTrackObj = new GameObject("SliderTrack1", typeof(RectTransform));
        sliderTrackObj.transform.SetParent(canvasObj.transform, false);
        var trackRT = sliderTrackObj.GetComponent<RectTransform>();
        trackRT.anchorMin = new Vector2(0.5f, 0f);
        trackRT.anchorMax = new Vector2(0.5f, 0f);
        trackRT.pivot = new Vector2(0.5f, 0f);
        trackRT.sizeDelta = new Vector2(900, 60);
        trackRT.anchoredPosition = new Vector2(0, 280);
        var trackImg = sliderTrackObj.AddComponent<Image>();
        if (sliderBgImg != null) trackImg.sprite = sliderBgImg;

        // Freq Marker 1 (enemy indicator on slider)
        var freqMarker1Obj = new GameObject("EnemyFreqMarker1", typeof(RectTransform));
        freqMarker1Obj.transform.SetParent(canvasObj.transform, false);
        var fm1RT = freqMarker1Obj.GetComponent<RectTransform>();
        fm1RT.anchorMin = new Vector2(0.5f, 0f);
        fm1RT.anchorMax = new Vector2(0.5f, 0f);
        fm1RT.pivot = new Vector2(0.5f, 0f);
        fm1RT.sizeDelta = new Vector2(24, 48);
        fm1RT.anchoredPosition = new Vector2(0, 335);
        var fm1Img = freqMarker1Obj.AddComponent<Image>();
        if (freqMarkerImg != null) fm1Img.sprite = freqMarkerImg;
        fm1Img.preserveAspect = true;

        // Player Slider 1
        var slider1Obj = new GameObject("PlayerFreqSlider1", typeof(RectTransform));
        slider1Obj.transform.SetParent(canvasObj.transform, false);
        var s1RT = slider1Obj.GetComponent<RectTransform>();
        s1RT.anchorMin = new Vector2(0.5f, 0f);
        s1RT.anchorMax = new Vector2(0.5f, 0f);
        s1RT.pivot = new Vector2(0.5f, 0f);
        s1RT.sizeDelta = new Vector2(900, 60);
        s1RT.anchoredPosition = new Vector2(0, 280);
        var slider1 = slider1Obj.AddComponent<Slider>();
        slider1.minValue = 0f;
        slider1.maxValue = 1f;
        slider1.value = 0.5f;
        slider1.direction = Slider.Direction.LeftToRight;

        // Slider fill area
        var fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObj.transform.SetParent(slider1Obj.transform, false);
        var fillAreaRT = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-15, 0);

        var fillObj = new GameObject("Fill", typeof(RectTransform));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        var fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.74f, 0.83f, 0.4f);
        slider1.fillRect = fillRT;

        // Slider handle
        var handleAreaObj = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleAreaObj.transform.SetParent(slider1Obj.transform, false);
        var handleAreaRT = handleAreaObj.GetComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(5, 0);
        handleAreaRT.offsetMax = new Vector2(-5, 0);

        var handleObj = new GameObject("Handle", typeof(RectTransform));
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        var handleRT = handleObj.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0f, 0f);
        handleRT.anchorMax = new Vector2(0f, 1f);
        handleRT.pivot = new Vector2(0.5f, 0.5f);
        handleRT.sizeDelta = new Vector2(50, 50);
        var handleImg = handleObj.AddComponent<Image>();
        if (sliderHandle != null) handleImg.sprite = sliderHandle;
        else handleImg.color = new Color(0f, 0.88f, 1f);
        handleImg.preserveAspect = true;
        slider1.handleRect = handleRT;
        slider1.targetGraphic = handleImg;

        // SliderTrack 2 (dual enemy, initially hidden)
        var sliderTrackObj2 = new GameObject("SliderTrack2", typeof(RectTransform));
        sliderTrackObj2.transform.SetParent(canvasObj.transform, false);
        var trackRT2 = sliderTrackObj2.GetComponent<RectTransform>();
        trackRT2.anchorMin = new Vector2(0.5f, 0f);
        trackRT2.anchorMax = new Vector2(0.5f, 0f);
        trackRT2.pivot = new Vector2(0.5f, 0f);
        trackRT2.sizeDelta = new Vector2(900, 60);
        trackRT2.anchoredPosition = new Vector2(0, 210);
        var trackImg2 = sliderTrackObj2.AddComponent<Image>();
        if (sliderBgImg != null) trackImg2.sprite = sliderBgImg;
        sliderTrackObj2.SetActive(false);

        // Slider 2 (dual enemy, initially hidden)
        var slider2Obj = new GameObject("PlayerFreqSlider2", typeof(RectTransform));
        slider2Obj.transform.SetParent(canvasObj.transform, false);
        var s2RT = slider2Obj.GetComponent<RectTransform>();
        s2RT.anchorMin = new Vector2(0.5f, 0f);
        s2RT.anchorMax = new Vector2(0.5f, 0f);
        s2RT.pivot = new Vector2(0.5f, 0f);
        s2RT.sizeDelta = new Vector2(900, 60);
        s2RT.anchoredPosition = new Vector2(0, 210);
        var slider2 = slider2Obj.AddComponent<Slider>();
        slider2.minValue = 0f; slider2.maxValue = 1f; slider2.value = 0.5f;
        slider2.direction = Slider.Direction.LeftToRight;

        var fillArea2Obj = new GameObject("Fill Area", typeof(RectTransform));
        fillArea2Obj.transform.SetParent(slider2Obj.transform, false);
        var fillArea2RT = fillArea2Obj.GetComponent<RectTransform>();
        fillArea2RT.anchorMin = new Vector2(0f, 0.25f); fillArea2RT.anchorMax = new Vector2(1f, 0.75f);
        fillArea2RT.offsetMin = new Vector2(5, 0); fillArea2RT.offsetMax = new Vector2(-15, 0);
        var fill2Obj = new GameObject("Fill", typeof(RectTransform));
        fill2Obj.transform.SetParent(fillArea2Obj.transform, false);
        var fill2RT = fill2Obj.GetComponent<RectTransform>();
        fill2RT.anchorMin = Vector2.zero; fill2RT.anchorMax = Vector2.one;
        fill2RT.offsetMin = fill2RT.offsetMax = Vector2.zero;
        var fill2Img = fill2Obj.AddComponent<Image>();
        fill2Img.color = new Color(0.88f, 0.25f, 0.98f, 0.4f);
        slider2.fillRect = fill2RT;

        var handleArea2Obj = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea2Obj.transform.SetParent(slider2Obj.transform, false);
        var handleArea2RT = handleArea2Obj.GetComponent<RectTransform>();
        handleArea2RT.anchorMin = Vector2.zero; handleArea2RT.anchorMax = Vector2.one;
        handleArea2RT.offsetMin = new Vector2(5, 0); handleArea2RT.offsetMax = new Vector2(-5, 0);
        var handle2Obj = new GameObject("Handle", typeof(RectTransform));
        handle2Obj.transform.SetParent(handleArea2Obj.transform, false);
        var handle2RT = handle2Obj.GetComponent<RectTransform>();
        handle2RT.anchorMin = new Vector2(0f, 0f); handle2RT.anchorMax = new Vector2(0f, 1f);
        handle2RT.pivot = new Vector2(0.5f, 0.5f); handle2RT.sizeDelta = new Vector2(50, 50);
        var handle2Img = handle2Obj.AddComponent<Image>();
        if (sliderHandle != null) handle2Img.sprite = sliderHandle;
        else handle2Img.color = new Color(0.88f, 0.25f, 0.98f);
        handle2Img.preserveAspect = true;
        slider2.handleRect = handle2RT;
        slider2.targetGraphic = handle2Img;
        slider2Obj.SetActive(false);

        // Freq Marker 2
        var freqMarker2Obj = new GameObject("EnemyFreqMarker2", typeof(RectTransform));
        freqMarker2Obj.transform.SetParent(canvasObj.transform, false);
        var fm2RT = freqMarker2Obj.GetComponent<RectTransform>();
        fm2RT.anchorMin = new Vector2(0.5f, 0f); fm2RT.anchorMax = new Vector2(0.5f, 0f);
        fm2RT.pivot = new Vector2(0.5f, 0f);
        fm2RT.sizeDelta = new Vector2(24, 48);
        fm2RT.anchoredPosition = new Vector2(0, 265);
        var fm2Img = freqMarker2Obj.AddComponent<Image>();
        if (freqMarkerImg != null) fm2Img.sprite = freqMarkerImg;
        fm2Img.color = new Color(0.88f, 0.25f, 0.98f, 1f);
        fm2Img.preserveAspect = true;
        freqMarker2Obj.SetActive(false);

        // === Player HP Bar ===
        var playerHpBar = CreateHpSlider(canvasObj.transform, "PlayerHpSlider",
            new Vector2(0f, 0f), new Vector2(20, 160), 400, 28,
            hpBgImg, hpPlayerFill, jpFont, "HP");

        // HP Labels
        var playerHpLabel = CT(canvasObj.transform, "PlayerHpLabel", "自分HP", 30, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(120, 35), new Vector2(20, 195));
        playerHpLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);

        var enemyHpLabel = CT(canvasObj.transform, "EnemyHpLabel", "敵HP", 30, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(100, 35), new Vector2(-100, -120));
        enemyHpLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        // Back button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニュー", 32, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(160, 55), new Vector2(20, 15), new Color(0.15f, 0.05f, 0.25f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 350);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.04f, 0.02f, 0.15f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.88f, 1f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 55), new Color(0.0f, 0.2f, 0.4f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 400);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.04f, 0.04f, 0.20f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "全クリア！周波数マスター！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.1f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.15f, 0.05f, 0.25f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 380);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.15f, 0.03f, 0.05f, 0.95f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 58, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScore = CT(goPanel.transform, "GOScore", "Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.4f, 0.1f, 0.1f));
        goBack.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === InstructionPanel ===
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
        ipBgImg.color = new Color(0.04f, 0.01f, 0.12f, 0.95f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "FreqFight", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.74f, 0.83f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 140), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0f, 0.2f, 0.45f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 80), new Color(0.1f, 0.1f, 0.25f, 0.9f));

        // === FreqFightUI ===
        var uiObj = new GameObject("FreqFightUI");
        var ui = uiObj.AddComponent<FreqFightUI>();

        SetField(ui, "_stageText",         stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_phaseText",         phaseText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",     judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_beatGuideImage",    bgImg);
        SetField(ui, "_playerHpSlider",    playerHpBar.GetComponent<Slider>());
        SetField(ui, "_enemyHpSlider1",    enemyHp1Bar.GetComponent<Slider>());
        SetField(ui, "_enemyHpSlider2",    enemyHp2Bar.GetComponent<Slider>());
        SetField(ui, "_enemyFreqMarker1",  fm1RT);
        SetField(ui, "_enemyFreqMarker2",  fm2RT);
        SetField(ui, "_sliderTrackRect",   trackRT);
        SetField(ui, "_sliderTrackRect2",  trackRT2);
        SetField(ui, "_enemyImage1",       enemy1Img);
        SetField(ui, "_enemyImage2",       enemy2Img);
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());

        // Wire FreqFightManager
        SetField(fm, "_playerFreqSlider",  slider1);
        SetField(fm, "_playerFreqSlider2", slider2);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_freqManager",      fm);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire Next Stage button
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/080v2_FreqFight.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup080v2] FreqFight シーン作成完了: " + scenePath);
    }

    static GameObject CreateHpSlider(Transform parent, string name,
        Vector2 anchor, Vector2 pos, float width, float height,
        Sprite bgSprite, Sprite fillSprite, TMP_FontAsset font, string label = null)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = pos;

        var slider = obj.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        // Background
        var bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(obj.transform, false);
        var bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        if (bgSprite != null) bgImg.sprite = bgSprite;
        else bgImg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);

        // Fill area
        var fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObj.transform.SetParent(obj.transform, false);
        var faRT = fillAreaObj.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.05f); faRT.anchorMax = new Vector2(1f, 0.95f);
        faRT.offsetMin = new Vector2(3, 0); faRT.offsetMax = new Vector2(-3, 0);

        var fillObj = new GameObject("Fill", typeof(RectTransform));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        var fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        if (fillSprite != null) fillImg.sprite = fillSprite;
        else fillImg.color = new Color(0.2f, 0.8f, 0.3f);
        slider.fillRect = fillRT;

        return obj;
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
        else Debug.LogWarning($"[Setup080v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
