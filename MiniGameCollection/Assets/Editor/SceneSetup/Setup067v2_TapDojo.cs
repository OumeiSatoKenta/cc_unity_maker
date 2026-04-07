using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game067v2_TapDojo;

public static class Setup067v2_TapDojo
{
    [MenuItem("Assets/Setup/067v2 TapDojo")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup067v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game067v2_TapDojo/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.15f, 0.08f, 0.22f);
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
            bgObj.transform.localScale = new Vector3(0.047f, 0.047f, 1f);
        }

        // Martial Artist (world space, center)
        Sprite artistSprite = LoadSprite(sp + "martial_artist.png");
        var artistObj = new GameObject("MartialArtist");
        SpriteRenderer artistSr = null;
        if (artistSprite != null)
        {
            artistSr = artistObj.AddComponent<SpriteRenderer>();
            artistSr.sprite = artistSprite;
            artistSr.sortingOrder = 1;
            artistObj.transform.position = new Vector3(0f, 0.5f, 0f);
            artistObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }

        // Tap Area (world space, behind martial artist)
        Sprite tapAreaSprite = LoadSprite(sp + "tap_area.png");
        if (tapAreaSprite != null)
        {
            var tapObj = new GameObject("TapArea");
            var tapSr = tapObj.AddComponent<SpriteRenderer>();
            tapSr.sprite = tapAreaSprite;
            tapSr.sortingOrder = 0;
            tapObj.transform.position = new Vector3(0f, 0.3f, 0f);
            tapObj.transform.localScale = new Vector3(0.028f, 0.028f, 1f);
        }

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<TapDojoGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 0f,  countMultiplier = 1, complexityFactor = 0.2f },
            new StageManager.StageConfig { speedMultiplier = 1f,  countMultiplier = 2, complexityFactor = 0.4f },
            new StageManager.StageConfig { speedMultiplier = 2f,  countMultiplier = 3, complexityFactor = 0.6f },
            new StageManager.StageConfig { speedMultiplier = 3.5f, countMultiplier = 5, complexityFactor = 0.8f },
            new StageManager.StageConfig { speedMultiplier = 5f,  countMultiplier = 8, complexityFactor = 1.0f },
        };
        sm.SetConfigs(stages);

        // DojoManager
        var dmObj = new GameObject("DojoManager");
        dmObj.transform.SetParent(gmObj.transform);
        var dm = dmObj.AddComponent<DojoManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage (top center)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 1f);

        // HUD: Rank (top left)
        var rankText = CT(canvasObj.transform, "RankText", "白帯", 36, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(280, 55), new Vector2(15, -90));
        rankText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // HUD: MP (top right)
        var mpText = CT(canvasObj.transform, "MPText", "修行PT: 0 / 1.0K", 30, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(500, 55), new Vector2(-15, -90));
        mpText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        mpText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.8f);

        // HUD: AutoRate (below rank)
        var autoRateText = CT(canvasObj.transform, "AutoRateText", "", 26, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(320, 44), new Vector2(15, -148));
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 0.7f);

        // Combo text (center, below HUD)
        var comboText = CT(canvasObj.transform, "ComboText", "", 48, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(700, 65), new Vector2(0, -195));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        // === TECH BUTTONS (bottom-left area) ===
        var seikenBtn = CB(canvasObj.transform, "SeikenButton", "正拳突き (50MP)", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(250, 65), new Vector2(10, 470),
            new Color(0.5f, 0.1f, 0.65f));

        var mawashiBtn = CB(canvasObj.transform, "MawashiButton", "回し蹴り (200MP)", 22, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(250, 65), new Vector2(10, 395),
            new Color(0.7f, 0.1f, 0.3f));
        mawashiBtn.SetActive(false);

        var tohouBtn = CB(canvasObj.transform, "TohouButton", "虎砲 (特訓で習得)", 20, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(250, 65), new Vector2(10, 320),
            new Color(0.7f, 0.4f, 0f));
        tohouBtn.SetActive(false);

        var shihanTestBtn = CB(canvasObj.transform, "ShihanTestButton", "師範試験 (5000MP)", 20, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(250, 65), new Vector2(10, 245),
            new Color(0.7f, 0.55f, 0f));
        shihanTestBtn.SetActive(false);

        // === FEATURE BUTTONS (bottom-right area) ===
        var tournamentBtn = CB(canvasObj.transform, "TournamentButton", "大会参加\n(500MP消費)", 22, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(250, 130), new Vector2(-10, 395),
            new Color(0.55f, 0.05f, 0.2f));
        tournamentBtn.SetActive(false);

        var trainingBtn = CB(canvasObj.transform, "TrainingButton", "特訓イベント\n15秒30タップ！", 20, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(250, 130), new Vector2(-10, 255),
            new Color(0.55f, 0.4f, 0f));
        trainingBtn.SetActive(false);

        // Back to menu button (bottom right corner)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 28, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(220, 65), new Vector2(-10, 15),
            new Color(0.2f, 0.2f, 0.2f, 0.85f));
        menuBtn.GetComponent<Button>().onClick.AddListener(() => { });
        var backScript = menuBtn.AddComponent<BackToMenuButton>();

        // === TRAINING TIMER PANEL ===
        var ttPanel = new GameObject("TrainingTimerPanel", typeof(RectTransform));
        ttPanel.transform.SetParent(canvasObj.transform, false);
        var ttRT = ttPanel.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0.5f, 1); ttRT.anchorMax = new Vector2(0.5f, 1);
        ttRT.pivot = new Vector2(0.5f, 1);
        ttRT.sizeDelta = new Vector2(700, 70);
        ttRT.anchoredPosition = new Vector2(0, -250);
        var ttImg = ttPanel.AddComponent<Image>();
        ttImg.color = new Color(0.6f, 0.4f, 0f, 0.92f);
        var ttText = CT(ttPanel.transform, "TrainingTimerText", "特訓！ 15.0秒  0/30タップ", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(650, 55), Vector2.zero);
        ttText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ttText.GetComponent<TextMeshProUGUI>().color = Color.white;
        ttPanel.SetActive(false);

        // === STAGE CLEAR PANEL ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 400);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.1f, 0.05f, 0.2f, 0.95f);

        var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 80), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(350, 80), Vector2.zero,
            new Color(0.2f, 0.7f, 0.2f));
        scPanel.SetActive(false);

        // === ALL CLEAR PANEL ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 500);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.05f, 0.05f, 0.15f, 0.98f);

        var acTitle = CT(acPanel.transform, "AllClearTitle", "師範への道、完成！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(650, 80), Vector2.zero);
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "AllClearScoreText", "総修行PT: ---", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 80), Vector2.zero);
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acMenuBtn = CB(acPanel.transform, "BackToMenuButton", "メニューへ", 36, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f), new Vector2(300, 80), Vector2.zero,
            new Color(0.2f, 0.2f, 0.5f));
        acMenuBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === INSTRUCTION PANEL ===
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipRT = ipBg.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;
        var ipImg = ipBg.AddComponent<Image>();
        ipImg.color = new Color(0.05f, 0.02f, 0.12f, 0.97f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "TapDojo", 72, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f), new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "タップで修行して最強の武道家を目指そう", 40, jpFont,
            new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "道場をタップして修行、ボタンで技習得・大会参加", 36, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "段位目標の修行ポイントを達成してステージクリア", 36, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 48, jpFont,
            new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), new Vector2(380, 100), Vector2.zero,
            new Color(0.5f, 0.1f, 0.65f));

        // Help button (bottom right)
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-10, 90),
            new Color(0.3f, 0.1f, 0.45f));

        // === TapDojoUI ===
        var uiObj = new GameObject("TapDojoUI");
        var ui = uiObj.AddComponent<TapDojoUI>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === Wire Fields ===
        // DojoManager
        SetField(dm, "_gameManager",           gm);
        SetField(dm, "_martialArtistRenderer", artistSr);
        SetField(dm, "_camera",                camera);

        // TapDojoUI
        SetField(ui, "_stageText",       stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_mpText",          mpText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",       comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText",    autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_rankText",        rankText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_seikenBtn",       seikenBtn.GetComponent<Button>());
        SetField(ui, "_mawashiBtn",      mawashiBtn.GetComponent<Button>());
        SetField(ui, "_tohouBtn",        tohouBtn.GetComponent<Button>());
        SetField(ui, "_shihanTestBtn",   shihanTestBtn.GetComponent<Button>());
        SetField(ui, "_tournamentBtn",   tournamentBtn.GetComponent<Button>());
        SetField(ui, "_trainingBtn",     trainingBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText",  scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",   acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_trainingTimerPanel", ttPanel);
        SetField(ui, "_trainingTimerText",  ttText.GetComponent<TextMeshProUGUI>());

        // TapDojoGameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_dojoManager",      dm);
        SetField(gm, "_ui",               ui);

        // InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire button OnClick events
        nextBtn.GetComponent<Button>().onClick.AddListener(() => {
            ui.HideStageClear();
            gm.NextStage();
        });
        seikenBtn.GetComponent<Button>().onClick.AddListener(() => dm.BuyTech(0));
        mawashiBtn.GetComponent<Button>().onClick.AddListener(() => dm.BuyTech(1));
        tournamentBtn.GetComponent<Button>().onClick.AddListener(() => dm.EnterTournament());
        trainingBtn.GetComponent<Button>().onClick.AddListener(() => dm.StartIntensiveTraining());
        shihanTestBtn.GetComponent<Button>().onClick.AddListener(() => dm.StartShihanTest());

        // Save scene
        string scenePath = "Assets/Scenes/067v2_TapDojo.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup067v2] TapDojo シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup067v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
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
