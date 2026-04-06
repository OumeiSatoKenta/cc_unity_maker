using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game072v2_DrumKit;

public static class Setup072v2_DrumKit
{
    [MenuItem("Assets/Setup/072v2 DrumKit")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup072v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game072v2_DrumKit/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
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
            bgObj.transform.localScale = new Vector3(0.022f, 0.0234f, 1f);
        }

        // Load sprites
        Sprite spriteBass    = LoadSprite(sp + "pad_bassdrum.png");
        Sprite spriteSnare   = LoadSprite(sp + "pad_snare.png");
        Sprite spriteHihat   = LoadSprite(sp + "pad_hihat.png");
        Sprite spriteCymbal  = LoadSprite(sp + "pad_cymbal.png");
        Sprite spriteTomHigh = LoadSprite(sp + "pad_tom_high.png");
        Sprite spriteTomLow  = LoadSprite(sp + "pad_tom_low.png");
        Sprite spriteRing    = LoadSprite(sp + "ring_guide.png");
        Sprite spriteRingJ   = LoadSprite(sp + "ring_judge.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("DrumKitGameManager");
        var gm = gmObj.AddComponent<DrumKitGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<Common.StageManager>();

        var stageConfigs = new Common.StageManager.StageConfig[]
        {
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 0.0f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 0.3f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 0.6f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 0.8f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 1.0f },
        };
        SetField(sm, "_stages", stageConfigs);

        // DrumPadManager
        var dpmObj = new GameObject("DrumPadManager");
        dpmObj.transform.SetParent(gmObj.transform);
        var dpm = dpmObj.AddComponent<DrumPadManager>();

        SetField(dpm, "_spriteBass",    spriteBass);
        SetField(dpm, "_spriteSnare",   spriteSnare);
        SetField(dpm, "_spriteHihat",   spriteHihat);
        SetField(dpm, "_spriteCymbal",  spriteCymbal);
        SetField(dpm, "_spriteTomHigh", spriteTomHigh);
        SetField(dpm, "_spriteTomLow",  spriteTomLow);
        SetField(dpm, "_spriteRingGuide", spriteRing);
        SetField(dpm, "_spriteRingJudge", spriteRingJ);
        SetField(dpm, "_gameManager",   gm);

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 42, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 50, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 60), new Vector2(-15, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0f, 1f, 1f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 250));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.5f, 1f);

        var missText = CT(canvasObj.transform, "MissText", "Miss: 0/10", 40, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 60), new Vector2(15, -30));
        missText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        var judgementText = CT(canvasObj.transform, "JudgementText", "", 72, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 100), new Vector2(0, 120));
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.gameObject.SetActive(false);

        // Back button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニュー", 34, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(170, 55), new Vector2(15, 15), new Color(0.2f, 0.2f, 0.3f, 0.9f));
        backBtn.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 350);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.05f, 0.2f, 0.95f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 1f, 1f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 50), new Color(0f, 0.6f, 0.8f));
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
        acImg.color = new Color(0.02f, 0.1f, 0.05f, 0.96f);

        CT(acPanel.transform, "ACTitle", "全ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30))
            .GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.5f);

        var acScoreText = CT(acPanel.transform, "ACScoreText", "Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 20));
        acScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBackBtn = CB(acPanel.transform, "ACBackButton", "メニューへ戻る", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 45), new Color(0.2f, 0.5f, 0.3f));
        acBackBtn.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 400);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.15f, 0.02f, 0.02f, 0.96f);

        CT(goPanel.transform, "GOTitle", "ゲームオーバー", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(650, 80), new Vector2(0, -30))
            .GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScoreText = CT(goPanel.transform, "GOScoreText", "Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 20));
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goBackBtn = CB(goPanel.transform, "GOBackButton", "メニューへ戻る", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(380, 65), new Vector2(0, 45), new Color(0.5f, 0.2f, 0.2f));
        goBackBtn.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
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
        ipBgImg.color = new Color(0f, 0f, 0f, 0.92f);

        var ip = ipBg.AddComponent<Common.InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "DrumKit", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0f, 1f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 44, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 38, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 38, jpFont,
            new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0f, 0.5f, 0.7f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 15), new Color(0.2f, 0.3f, 0.5f, 0.9f));

        // === DrumKitUI ===
        var uiObj = new GameObject("DrumKitUI");
        var ui = uiObj.AddComponent<DrumKitUI>();

        SetField(ui, "_scoreText",        scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",        comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageText",        stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_missText",         missText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_judgementText",    judgementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",  scPanel);
        SetField(ui, "_stageClearText",   scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",    acPanel);
        SetField(ui, "_allClearScoreText", acScoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",    goPanel);
        SetField(ui, "_gameOverScoreText", goScoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameManager",      gm);

        // Wire GameManager
        SetField(gm, "_stageManager",    sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_drumPadManager",  dpm);
        SetField(gm, "_ui",              ui);

        // Wire InstructionPanel fields
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Button events
        nextBtn.GetComponent<Button>().onClick.AddListener(ui.OnNextStageButton);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/072v2_DrumKit.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup072v2] DrumKit シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup072v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
