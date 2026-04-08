using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game091v2_TimeBlender;

public static class Setup091v2_TimeBlender
{
    [MenuItem("Assets/Setup/091v2 TimeBlender")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup091v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game091v2_TimeBlender/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.04f, 0.02f, 0.12f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // === Background ===
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

        // === Load sprites ===
        Sprite sprEmpty     = LoadSprite(sp + "tile_empty.png");
        Sprite sprWall      = LoadSprite(sp + "tile_wall.png");
        Sprite sprWallFuture= LoadSprite(sp + "tile_wall_future.png");
        Sprite sprBridge    = LoadSprite(sp + "tile_bridge.png");
        Sprite sprTree      = LoadSprite(sp + "tile_tree.png");
        Sprite sprGoal      = LoadSprite(sp + "tile_goal.png");
        Sprite sprStart     = LoadSprite(sp + "tile_start.png");
        Sprite sprParadox   = LoadSprite(sp + "tile_paradox.png");
        Sprite sprPlayer    = LoadSprite(sp + "player.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("TimeBlenderGameManager");
        var gm = gmObj.AddComponent<TimeBlenderGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.3f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.5f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.8f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // PuzzleManager (child of GM)
        var pmObj = new GameObject("PuzzleManager");
        pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<PuzzleManager>();

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
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 48), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 48), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        var moveTextGo = CT(canvasObj.transform, "MoveText", "Moves: 0", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 44), new Vector2(0, -15));
        moveTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        moveTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 0.7f);

        var paradoxTextGo = CT(canvasObj.transform, "ParadoxText", "Paradox: 3 残り", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 44), new Vector2(0, -65));
        paradoxTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        paradoxTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 1f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 44), new Vector2(0, -115));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        // === Era Indicator (upper-middle) ===
        var eraTextGo = CT(canvasObj.transform, "EraText", "過去", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 60), new Vector2(0, -165));
        eraTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        eraTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.2f);

        // === Era Switch Buttons (bottom area) ===
        var pastBtn = CB(canvasObj.transform, "PastButton", "過去", 44, jpFont,
            new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 70), new Vector2(0, 170), new Color(0.7f, 0.35f, 0.05f));

        var futureBtn = CB(canvasObj.transform, "FutureButton", "未来", 44, jpFont,
            new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 70), new Vector2(0, 170), new Color(0.1f, 0.25f, 0.6f));

        var presentBtn = CB(canvasObj.transform, "PresentButton", "現在", 44, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 70), new Vector2(0, 170), new Color(0.1f, 0.5f, 0.2f));
        presentBtn.SetActive(false);

        // === BackToMenu Button ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 32, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 90), new Color(0.3f, 0.3f, 0.3f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.1f, 0.3f); scRT.anchorMax = new Vector2(0.9f, 0.75f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.05f, 0.25f, 0.97f);

        var scTitleGo = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 58, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 80), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var scScoreGo = CT(scPanel.transform, "StageClearScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        scScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.2f, 0.5f, 0.8f));

        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.1f, 0.3f); acRT.anchorMax = new Vector2(0.9f, 0.75f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.02f, 0.06f, 0.18f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearTitle", "ALL CLEAR!", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 100), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 1f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "Total Score: 0", 46, jpFont,
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 70), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acBackBtn = CB(acPanel.transform, "BackToMenuButton2", "メニューへ戻る", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.3f, 0.3f, 0.4f));
        acBackBtn.AddComponent<BackToMenuButton>();

        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.3f); goRT.anchorMax = new Vector2(0.9f, 0.75f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.15f, 0.02f, 0.15f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverTitle", "タイムパラドックス！", 52, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 1f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 72), Vector2.zero, new Color(0.5f, 0.2f, 0.5f));

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
        ipBgImg.color = new Color(0.04f, 0.02f, 0.18f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "TimeBlender", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.85f, 0.9f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 0.5f);

        var startBtnGo = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.15f, 0.35f, 0.6f));

        // Help button ("?") on main canvas
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-15, 260), new Color(0.2f, 0.2f, 0.35f));

        // === Wire InstructionPanel ===
        SetField(ip, "_panelRoot",       ipBg);
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtnGo.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());

        // === TimeBlenderUI ===
        var uiObj = new GameObject("TimeBlenderUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<TimeBlenderUI>();

        SetField(ui, "_stageText",       stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",       scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_moveText",        moveTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_paradoxText",     paradoxTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",       comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_eraText",         eraTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_pastButton",      pastBtn.GetComponent<Button>());
        SetField(ui, "_futureButton",    futureBtn.GetComponent<Button>());
        SetField(ui, "_presentButton",   presentBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearScoreText", scScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton", scNextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",   acPanel);
        SetField(ui, "_allClearScoreText", acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",   goPanel);
        SetField(ui, "_gameOverScoreText", goScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_retryButton",     goRetryBtn.GetComponent<Button>());
        SetField(ui, "_gameManager",     gm);
        SetField(ui, "_puzzleManager",   pm);

        // === Wire PuzzleManager sprites ===
        SetField(pm, "_gameManager",     gm);
        SetField(pm, "_ui",              ui);
        SetField(pm, "_sprEmpty",        sprEmpty);
        SetField(pm, "_sprWall",         sprWall);
        SetField(pm, "_sprWallFuture",   sprWallFuture);
        SetField(pm, "_sprBridge",       sprBridge);
        SetField(pm, "_sprTree",         sprTree);
        SetField(pm, "_sprGoal",         sprGoal);
        SetField(pm, "_sprStart",        sprStart);
        SetField(pm, "_sprParadox",      sprParadox);
        SetField(pm, "_sprPlayer",       sprPlayer);

        // === Wire GameManager ===
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_puzzleManager",    pm);
        SetField(gm, "_ui",              ui);

        // === EventSystem ===
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // === Save scene ===
        string scenePath = "Assets/Scenes/091v2_TimeBlender.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup091v2] TimeBlender シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup091v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
