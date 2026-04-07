using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game078v2_EchoBack;

public static class Setup078v2_EchoBack
{
    [MenuItem("Assets/Setup/078v2 EchoBack")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup078v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game078v2_EchoBack/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.06f, 0.02f, 0.12f);
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
        Sprite keyWhite       = LoadSprite(sp + "key_white.png");
        Sprite keyWhiteActive = LoadSprite(sp + "key_white_active.png");
        Sprite dotFilled      = LoadSprite(sp + "dot_filled.png");
        Sprite dotEmpty       = LoadSprite(sp + "dot_empty.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("EchoBackGameManager");
        var gm = gmObj.AddComponent<EchoBackGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.29f, countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.57f, countMultiplier = 1, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.86f, countMultiplier = 1, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.14f, countMultiplier = 1, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // EchoManager
        var emObj = new GameObject("EchoManager");
        emObj.transform.SetParent(gmObj.transform);
        var em = emObj.AddComponent<EchoManager>();
        var audioSrc = emObj.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        SetField(em, "_audioSource", audioSrc);
        SetField(em, "_gameManager", gm);

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var phaseText = CT(canvasObj.transform, "PhaseText", "聴取中...", 48, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 60), new Vector2(0, -100));
        phaseText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        phaseText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 70), new Vector2(0, -175));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Replay count text
        var replayText = CT(canvasObj.transform, "ReplayCountText", "Replay: ∞", 40, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 50), new Vector2(-20, -90));
        replayText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        replayText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.8f, 1f);

        // Judgement text (mid-screen)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 90), new Vector2(0, 0));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        judgementText.SetActive(false);

        // Progress dots row
        int maxDots = 12;
        var progressDots = new Image[maxDots];
        var dotsContainer = new GameObject("ProgressDots", typeof(RectTransform));
        dotsContainer.transform.SetParent(canvasObj.transform, false);
        var dcRT = dotsContainer.GetComponent<RectTransform>();
        dcRT.anchorMin = new Vector2(0.5f, 0.5f); dcRT.anchorMax = new Vector2(0.5f, 0.5f);
        dcRT.pivot = new Vector2(0.5f, 0.5f);
        dcRT.sizeDelta = new Vector2(800, 50);
        dcRT.anchoredPosition = new Vector2(0, 200);
        var hLayout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        for (int i = 0; i < maxDots; i++)
        {
            var dot = new GameObject($"Dot{i}", typeof(RectTransform));
            dot.transform.SetParent(dotsContainer.transform, false);
            var dotRT = dot.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(30, 30);
            var dotImg = dot.AddComponent<Image>();
            dotImg.sprite = dotEmpty;
            progressDots[i] = dotImg;
            dot.SetActive(false);
        }

        // === Keyboard area (bottom of screen, Canvas UI) ===
        // 7 keys max, shown/hidden by EchoManager
        int totalKeys = 7;
        var keyButtons = new Button[totalKeys];
        var keyImages = new Image[totalKeys];
        string[] noteNames = { "ド", "レ", "ミ", "ファ", "ソ", "ラ", "シ" };
        Color[] keyColors = {
            new Color(0f, 0.8f, 1f),
            new Color(0.8f, 0.2f, 1f),
            new Color(0.2f, 1f, 0.6f),
            new Color(1f, 0.5f, 0.1f),
            new Color(1f, 0.2f, 0.4f),
            new Color(0.3f, 0.5f, 1f),
            new Color(1f, 0.9f, 0.2f),
        };

        var keysContainer = new GameObject("KeysContainer", typeof(RectTransform));
        keysContainer.transform.SetParent(canvasObj.transform, false);
        var kcRT = keysContainer.GetComponent<RectTransform>();
        kcRT.anchorMin = new Vector2(0f, 0f); kcRT.anchorMax = new Vector2(1f, 0f);
        kcRT.pivot = new Vector2(0.5f, 0f);
        kcRT.sizeDelta = new Vector2(0, 220);
        kcRT.anchoredPosition = new Vector2(0, 80);
        var hKeys = keysContainer.AddComponent<HorizontalLayoutGroup>();
        hKeys.spacing = 12;
        hKeys.padding = new RectOffset(20, 20, 10, 10);
        hKeys.childAlignment = TextAnchor.MiddleCenter;
        hKeys.childControlWidth = true;
        hKeys.childControlHeight = true;
        hKeys.childForceExpandWidth = true;
        hKeys.childForceExpandHeight = true;

        for (int i = 0; i < totalKeys; i++)
        {
            var keyObj = new GameObject($"Key{i}", typeof(RectTransform));
            keyObj.transform.SetParent(keysContainer.transform, false);
            var keyRT = keyObj.GetComponent<RectTransform>();
            keyRT.sizeDelta = new Vector2(130, 180);
            var keyImg = keyObj.AddComponent<Image>();
            keyImg.sprite = keyWhite;
            keyImg.color = keyColors[i];
            keyImg.type = Image.Type.Sliced;
            var keyBtn = keyObj.AddComponent<Button>();
            keyBtn.targetGraphic = keyImg;
            var keyLabel = CT(keyObj.transform, "Label", noteNames[i], 42, jpFont,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(120, 50), new Vector2(0, 10));
            keyLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            keyLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
            int keyIndex = i;
            keyBtn.onClick.AddListener(() => em.OnKeyPressed(keyIndex));
            keyButtons[i] = keyBtn;
            keyImages[i] = keyImg;
        }

        // Replay button
        var replayBtn = CB(canvasObj.transform, "ReplayButton", ">> リプレイ", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 65), new Vector2(0, 315), new Color(0.1f, 0.3f, 0.5f, 0.9f));
        replayBtn.GetComponent<Button>().onClick.AddListener(() => em.OnReplayPressed());

        // Back button (always visible bottom)
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(0, 15), new Color(0.12f, 0.08f, 0.2f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 360);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.1f, 0.25f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 62, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(620, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.9f, 1f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 46, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(420, 70), new Vector2(0, 60), new Color(0.05f, 0.3f, 0.55f));
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 380);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.05f, 0.15f, 0.05f, 0.95f);

        var acTitle = CT(acPanel.transform, "ACTitle", "マスターエコー！", 62, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -25));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.1f, 0.2f, 0.1f));
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
        ipBgImg.color = new Color(0.04f, 0.02f, 0.1f, 0.95f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "EchoBack", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.88f, 1f);

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

        // === EchoBackUI ===
        var uiObj = new GameObject("EchoBackUI");
        var ui = uiObj.AddComponent<EchoBackUI>();

        SetField(ui, "_stageText",         stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_phaseText",         phaseText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",     judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_replayCountText",   replayText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_progressDots",      progressDots);
        SetField(ui, "_dotFilled",         dotFilled);
        SetField(ui, "_dotEmpty",          dotEmpty);
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",   nextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScore.GetComponent<TextMeshProUGUI>());

        // Wire EchoManager
        SetField(em, "_ui",         ui);
        SetField(em, "_keyButtons", keyButtons);
        SetField(em, "_keyImages",  keyImages);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_echoManager",      em);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/078v2_EchoBack.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup078v2] EchoBack シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup078v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
