using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game092v2_MirrorWorld;

public static class Setup092v2_MirrorWorld
{
    [MenuItem("Assets/Setup/092v2 MirrorWorld")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup092v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game092v2_MirrorWorld/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.04f, 0.00f, 0.1f);
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
            float camS = 6f;
            float camW = camS * (16f / 9f);
            float scX = camW * 2f / bgSprite.bounds.size.x;
            float scY = camS * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scX, scY, 1f);
        }

        // === Load sprites ===
        Sprite sprWall     = LoadSprite(sp + "wall.png");
        Sprite sprTrap     = LoadSprite(sp + "trap.png");
        Sprite sprGoalTop  = LoadSprite(sp + "goal_top.png");
        Sprite sprGoalBot  = LoadSprite(sp + "goal_bot.png");
        Sprite sprSwitch   = LoadSprite(sp + "switch.png");
        Sprite sprDoor     = LoadSprite(sp + "door.png");
        Sprite sprPlayerTop = LoadSprite(sp + "player_top.png");
        Sprite sprPlayerBot = LoadSprite(sp + "player_bot.png");

        // === Mirror line sprite ===
        var mirrorLineObj = new GameObject("MirrorLine");
        var mlSr = mirrorLineObj.AddComponent<SpriteRenderer>();
        mlSr.sortingOrder = 3;
        // Create a simple line texture
        var lineTex = new Texture2D(1, 4);
        lineTex.SetPixels(new Color[]{ new Color(0.46f,1f,0.01f,0.4f), new Color(0.46f,1f,0.01f,0.8f),
                                        new Color(0.46f,1f,0.01f,0.8f), new Color(0.46f,1f,0.01f,0.4f) });
        lineTex.Apply();
        mlSr.sprite = Sprite.Create(lineTex, new Rect(0,0,1,4), new Vector2(0.5f,0.5f), 1f);
        mirrorLineObj.transform.position = new Vector3(0f, 0f, 0f);
        mirrorLineObj.transform.localScale = new Vector3(12f, 0.05f, 1f);

        // === GameManager hierarchy ===
        var gmObj = new GameObject("MirrorWorldGameManager");
        var gm = gmObj.AddComponent<MirrorWorldGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.3f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.5f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.7f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // PuzzleManager (child of GM)
        var pmObj = new GameObject("MirrorPuzzleManager");
        pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<MirrorPuzzleManager>();

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

        // === HUD (top) ===
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 48), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.46f, 1f, 0.01f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 48), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.46f, 1f, 0.01f);

        var movesTextGo = CT(canvasObj.transform, "MovesText", "Moves: 0", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 44), new Vector2(0, -15));
        movesTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        movesTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.83f, 0f, 0.98f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 44), new Vector2(0, -65));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        // === BackToMenu Button ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 32, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.3f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f); scRT.sizeDelta = new Vector2(700, 400);
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);

        var scTitleGo = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.46f, 1f, 0.01f);

        var scScoreGo = CT(scPanel.transform, "StageClearScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        scScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 70), Vector2.zero, new Color(0.1f, 0.4f, 0.1f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f); acRT.sizeDelta = new Vector2(760, 440);
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.03f, 0.1f, 0.03f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearTitle", "全ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 80), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.46f, 1f, 0.01f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "Final Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acBackBtn = CB(acPanel.transform, "BackToMenuButton2", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), Vector2.zero, new Color(0.2f, 0.3f, 0.2f));
        acBackBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goPanelRT = goPanel.GetComponent<RectTransform>();
        goPanelRT.anchorMin = new Vector2(0.5f, 0.5f); goPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRT.pivot = new Vector2(0.5f, 0.5f); goPanelRT.sizeDelta = new Vector2(700, 420);
        var goPanelImg = goPanel.AddComponent<Image>();
        goPanelImg.color = new Color(0.15f, 0.02f, 0.02f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 80), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 72), Vector2.zero, new Color(0.5f, 0.15f, 0.15f));
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
        ipBgImg.color = new Color(0.04f, 0.01f, 0.12f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "MirrorWorld", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.46f, 1f, 0.01f);

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
            new Vector2(380, 72), Vector2.zero, new Color(0.1f, 0.3f, 0.1f));

        // Help button ("?") on main canvas
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-15, 90), new Color(0.15f, 0.15f, 0.25f));

        // === Wire InstructionPanel ===
        SetField(ip, "_panelRoot",       ipBg);
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtnGo.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());

        // === MirrorWorldUI ===
        var uiObj = new GameObject("MirrorWorldUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<MirrorWorldUI>();

        SetField(ui, "_stageText",           stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",           scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_movesText",           movesTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",           comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",     scPanel);
        SetField(ui, "_stageClearScoreText", scScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",       goPanel);
        SetField(ui, "_gameOverScoreText",   goScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",       acPanel);
        SetField(ui, "_allClearScoreText",   acScoreGo.GetComponent<TextMeshProUGUI>());

        // === Wire PuzzleManager ===
        SetField(pm, "_gameManager",   gm);
        SetField(pm, "_ui",            ui);
        SetField(pm, "_sprWall",       sprWall);
        SetField(pm, "_sprTrap",       sprTrap);
        SetField(pm, "_sprGoalTop",    sprGoalTop);
        SetField(pm, "_sprGoalBot",    sprGoalBot);
        SetField(pm, "_sprSwitch",     sprSwitch);
        SetField(pm, "_sprDoor",       sprDoor);
        SetField(pm, "_sprPlayerTop",  sprPlayerTop);
        SetField(pm, "_sprPlayerBot",  sprPlayerBot);

        // === Wire button events ===
        scNextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        goRetryBtn.GetComponent<Button>().onClick.AddListener(() => gm.RestartGame());

        // === Wire GameManager ===
        SetField(gm, "_stageManager",    sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_puzzleManager",   pm);
        SetField(gm, "_ui",              ui);

        // === EventSystem ===
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // === Save scene ===
        string scenePath = "Assets/Scenes/092v2_MirrorWorld.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup092v2] MirrorWorld シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup092v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
