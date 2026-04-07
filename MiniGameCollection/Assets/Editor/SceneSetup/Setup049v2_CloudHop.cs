using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game049v2_CloudHop;

public static class Setup049v2_CloudHop
{
    [MenuItem("Assets/Setup/049v2 CloudHop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup049v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game049v2_CloudHop/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite imports
        string[] spritePaths = {
            sp+"Background.png", sp+"Player.png",
            sp+"Cloud_Normal.png", sp+"Cloud_Spring.png",
            sp+"Cloud_Thunder.png", sp+"Cloud_Moving.png",
            sp+"Coin.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spPlayer   = LoadSprite(sp + "Player.png");
        Sprite spNormal   = LoadSprite(sp + "Cloud_Normal.png");
        Sprite spSpring   = LoadSprite(sp + "Cloud_Spring.png");
        Sprite spThunder  = LoadSprite(sp + "Cloud_Thunder.png");
        Sprite spMoving   = LoadSprite(sp + "Cloud_Moving.png");
        Sprite spCoin     = LoadSprite(sp + "Coin.png");

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

        // Player
        var playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");
        playerObj.transform.position = new Vector3(0f, -camSize + 2.5f, 0f);

        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = spPlayer;
        playerSr.sortingOrder = 5;

        float playerSize = 0.7f;
        if (spPlayer != null)
        {
            float pw = spPlayer.rect.width / spPlayer.pixelsPerUnit;
            float ph = spPlayer.rect.height / spPlayer.pixelsPerUnit;
            playerObj.transform.localScale = new Vector3(playerSize / pw, playerSize / ph, 1f);
        }

        var playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 2f;
        playerRb.freezeRotation = true;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var playerCol = playerObj.AddComponent<CapsuleCollider2D>();
        playerCol.size = new Vector2(0.5f, 0.65f);
        playerCol.direction = CapsuleDirection2D.Vertical;

        var controller = playerObj.AddComponent<CloudHopController>();

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CloudHopGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var smSO = new SerializedObject(sm);
        var stagesArr = smSO.FindProperty("_configs");
        stagesArr.arraySize = 5;
        float[] speeds = { 1.0f, 1.2f, 1.4f, 1.6f, 2.0f };
        float[] counts = { 1.3f, 1.2f, 1.0f, 0.9f, 0.8f };
        float[] complexities = { 0.0f, 0.2f, 0.5f, 0.7f, 1.0f };
        for (int i = 0; i < 5; i++)
        {
            var stage = stagesArr.GetArrayElementAtIndex(i);
            stage.FindPropertyRelative("stageName").stringValue = $"Stage {i + 1}";
            stage.FindPropertyRelative("speedMultiplier").floatValue = speeds[i];
            stage.FindPropertyRelative("countMultiplier").floatValue = counts[i];
            stage.FindPropertyRelative("complexityFactor").floatValue = complexities[i];
        }
        smSO.ApplyModifiedProperties();

        // CloudSpawner (child of GameManager)
        var spawnerObj = new GameObject("CloudSpawner");
        spawnerObj.transform.SetParent(gmObj.transform);
        var spawner = spawnerObj.AddComponent<CloudSpawner>();

        var spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("_spriteNormal").objectReferenceValue = spNormal;
        spawnerSO.FindProperty("_spriteSpring").objectReferenceValue = spSpring;
        spawnerSO.FindProperty("_spriteThunder").objectReferenceValue = spThunder;
        spawnerSO.FindProperty("_spriteMoving").objectReferenceValue = spMoving;
        spawnerSO.FindProperty("_spriteCoin").objectReferenceValue = spCoin;
        spawnerSO.ApplyModifiedProperties();

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
        // Stage text (top left)
        var stageTextObj = CreateText(canvasTransform, "StageText", "Stage 1 / 5", jpFont, 36,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 50f), new Vector2(160f, -30f), Color.white);

        // Score text (top right)
        var scoreTextObj = CreateText(canvasTransform, "ScoreText", "0", jpFont, 36,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(200f, 50f), new Vector2(-110f, -30f), Color.white);

        // Altitude text (top center)
        var altTextObj = CreateText(canvasTransform, "AltitudeText", "0m / 100m", jpFont, 30,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(400f, 50f), new Vector2(0f, -30f), Color.white);

        // Combo text (center)
        var comboTextObj = CreateText(canvasTransform, "ComboText", "COMBO x2!", jpFont, 48,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 60f), new Vector2(0f, 200f), Color.yellow);
        comboTextObj.SetActive(false);

        // Bonus text (floating)
        var bonusTextObj = CreateText(canvasTransform, "BonusText", "", jpFont, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 50f), new Vector2(0f, 100f), Color.yellow);
        bonusTextObj.SetActive(false);

        // --- Bottom buttons ---
        // Menu button (bottom left)
        var menuBtnObj = CreateButton(canvasTransform, "MenuButton", "メニュー", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 60f), new Vector2(100f, 40f),
            new Color(0.2f, 0.2f, 0.3f, 0.9f));
        menuBtnObj.AddComponent<BackToMenuButton>();

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
        var ip = BuildInstructionPanel(canvasTransform, jpFont, canvas);

        // --- UI component ---
        var uiObj = new GameObject("CloudHopUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<CloudHopUI>();

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_altitudeText").objectReferenceValue = altTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_bonusText").objectReferenceValue = bonusTextObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreObj.GetComponent<TMP_Text>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        // Wire GameManager fields
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_controller").objectReferenceValue = controller;
        gmSO.FindProperty("_spawner").objectReferenceValue = spawner;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Wire Controller fields
        var ctrlSO = new SerializedObject(controller);
        ctrlSO.FindProperty("_gameManager").objectReferenceValue = gm;
        ctrlSO.FindProperty("_playerSprite").objectReferenceValue = playerSr;
        ctrlSO.ApplyModifiedProperties();

        // Wire Spawner GameManager
        var spawnerSO2 = new SerializedObject(spawner);
        spawnerSO2.FindProperty("_gameManager").objectReferenceValue = gm;
        spawnerSO2.ApplyModifiedProperties();

        // Wire button callbacks (stage clear next, restart)
        nextBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.GoNextStage());
        acRestartBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());
        goRestartBtnObj.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // InstructionPanel Canvas sortOrder fix
        var ipCanvas = ip.GetComponentInParent<Canvas>();
        if (ipCanvas != null) ipCanvas.sortingOrder = 100;

        // Save scene
        string scenePath = "Assets/Scenes/049v2_CloudHop.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup049v2] CloudHop シーン作成完了: " + scenePath);
    }

    static InstructionPanel BuildInstructionPanel(Transform canvasTransform, TMP_FontAsset font, Canvas canvas)
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

        // Fullscreen overlay panel
        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(ipCanvasObj.transform, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.1f, 0.2f, 0.92f);

        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(800f, 90f);
        titleRt.anchoredPosition = new Vector2(0f, 250f);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.font = font;
        titleTmp.fontSize = 60;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;

        var descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(panelObj.transform, false);
        var descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f);
        descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(800f, 80f);
        descRt.anchoredPosition = new Vector2(0f, 130f);
        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.font = font;
        descTmp.fontSize = 34;
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = Color.white;
        descTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = new GameObject("ControlsText");
        ctrlObj.transform.SetParent(panelObj.transform, false);
        var ctrlRt = ctrlObj.AddComponent<RectTransform>();
        ctrlRt.anchorMin = new Vector2(0.5f, 0.5f);
        ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRt.sizeDelta = new Vector2(800f, 90f);
        ctrlRt.anchoredPosition = new Vector2(0f, 0f);
        var ctrlTmp = ctrlObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.font = font;
        ctrlTmp.fontSize = 28;
        ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.9f, 0.9f, 0.7f);
        ctrlTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(panelObj.transform, false);
        var goalRt = goalObj.AddComponent<RectTransform>();
        goalRt.anchorMin = new Vector2(0.5f, 0.5f);
        goalRt.anchorMax = new Vector2(0.5f, 0.5f);
        goalRt.sizeDelta = new Vector2(800f, 60f);
        goalRt.anchoredPosition = new Vector2(0f, -90f);
        var goalTmp = goalObj.AddComponent<TextMeshProUGUI>();
        goalTmp.font = font;
        goalTmp.fontSize = 30;
        goalTmp.alignment = TextAlignmentOptions.Center;
        goalTmp.color = new Color(1f, 0.85f, 0.3f);
        goalTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(panelObj.transform, false);
        var startBtnRt = startBtnObj.AddComponent<RectTransform>();
        startBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        startBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRt.sizeDelta = new Vector2(260f, 70f);
        startBtnRt.anchoredPosition = new Vector2(0f, -200f);
        var startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0.1f, 0.5f, 0.8f);
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

        var qBtnObj = new GameObject("QuestionButton");
        qBtnObj.transform.SetParent(ipCanvasObj.transform, false);
        var qBtnRt = qBtnObj.AddComponent<RectTransform>();
        qBtnRt.anchorMin = new Vector2(1f, 0f);
        qBtnRt.anchorMax = new Vector2(1f, 0f);
        qBtnRt.pivot = new Vector2(1f, 0f);
        qBtnRt.sizeDelta = new Vector2(70f, 70f);
        qBtnRt.anchoredPosition = new Vector2(-10f, 10f);
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
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return obj;
    }

    static GameObject CreateButton(Transform parent, string name, string label, TMP_FontAsset font,
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
        obj.AddComponent<Button>();

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
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Setup049v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
