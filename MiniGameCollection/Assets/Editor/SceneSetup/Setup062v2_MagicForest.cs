using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game062v2_MagicForest;

public static class Setup062v2_MagicForest
{
    [MenuItem("Assets/Setup/062v2 MagicForest")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup062v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game062v2_MagicForest/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.55f, 0.78f, 0.45f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<MagicForestGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // ForestManager
        var fmObj = new GameObject("ForestManager");
        fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<ForestManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage text
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.4f, 0.1f);

        // HUD: Mana text (top left)
        var manaText = CT(canvasObj.transform, "ManaText", "✨ 0", 36, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(300, 55), new Vector2(20, -95));
        manaText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        manaText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.4f, 0.0f);

        // HUD: Area text (top right)
        var areaText = CT(canvasObj.transform, "AreaText", "🌳 0 / 10", 36, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(350, 55), new Vector2(-20, -95));
        areaText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        areaText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.5f, 0.1f);

        // Combo text (center, hidden initially)
        var comboText = CT(canvasObj.transform, "ComboText", "Combo x1", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 55), new Vector2(0, -160));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.8f, 0.2f);
        comboText.SetActive(false);

        // World tree progress text (center, hidden)
        var wtText = CT(canvasObj.transform, "WorldTreeText", "🌳 世界樹が成長中...", 30, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, 55), new Vector2(0, -220));
        wtText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        wtText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0f, 0.8f);
        wtText.SetActive(false);

        // Storm warning (center, hidden)
        var stormPanel = new GameObject("StormWarning", typeof(RectTransform));
        stormPanel.transform.SetParent(canvasObj.transform, false);
        var swRT = stormPanel.GetComponent<RectTransform>();
        swRT.anchorMin = new Vector2(0.5f, 1); swRT.anchorMax = new Vector2(0.5f, 1);
        swRT.pivot = new Vector2(0.5f, 1);
        swRT.sizeDelta = new Vector2(500, 60);
        swRT.anchoredPosition = new Vector2(0, -280);
        var swImg = stormPanel.AddComponent<Image>();
        swImg.color = new Color(0.3f, 0.3f, 0.5f, 0.85f);
        var swText = CT(stormPanel.transform, "StormText", "⛈ 嵐！木を守れ！", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480, 55), Vector2.zero);
        swText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        swText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        stormPanel.SetActive(false);

        // Auto Grow Button (bottom area, hidden initially)
        var autoGrowBtn = CB(canvasObj.transform, "AutoGrowButton", "自動成長\n✨30", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(240, 80), new Vector2(0, 290),
            new Color(0.2f, 0.6f, 0.2f));
        autoGrowBtn.SetActive(false);
        var autoGrowCostText = autoGrowBtn.GetComponentInChildren<TextMeshProUGUI>();

        // Back to menu button (bottom left)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 28, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(180, 65), new Vector2(20, 175),
            new Color(0.3f, 0.5f, 0.3f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Stage Clear Panel
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scpRT = scPanel.GetComponent<RectTransform>();
        scpRT.anchorMin = new Vector2(0.1f, 0.3f); scpRT.anchorMax = new Vector2(0.9f, 0.7f);
        scpRT.offsetMin = scpRT.offsetMax = Vector2.zero;
        var scpImg = scPanel.AddComponent<Image>();
        scpImg.color = new Color(0.1f, 0.5f, 0.1f, 0.95f);
        var scText = CT(scPanel.transform, "StageClearText", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var nextStageBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(400, 80), Vector2.zero,
            new Color(0.2f, 0.7f, 0.2f));
        scPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = new GameObject("GameClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcpRT = gcPanel.GetComponent<RectTransform>();
        gcpRT.anchorMin = new Vector2(0.1f, 0.25f); gcpRT.anchorMax = new Vector2(0.9f, 0.75f);
        gcpRT.offsetMin = gcpRT.offsetMax = Vector2.zero;
        var gcpImg = gcPanel.AddComponent<Image>();
        gcpImg.color = new Color(0.3f, 0.1f, 0.5f, 0.97f);
        var gcText = CT(gcPanel.transform, "GameClearText", "魔法の森 完成！", 46, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 100), Vector2.zero);
        gcText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var retryBtn = CB(gcPanel.transform, "RetryButton", "もう一度", 34, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(320, 75), Vector2.zero,
            new Color(0.5f, 0.2f, 0.7f));
        gcPanel.SetActive(false);

        // --- InstructionPanel ---
        var ipCanvas = new GameObject("InstructionPanelCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<GraphicRaycaster>();
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0f, 0f, 0f, 0.88f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        // Title
        var ipTitle = CT(ipBg.transform, "TitleText", "MagicForest", 64, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 90), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.4f);

        // Description
        var ipDesc = CT(ipBg.transform, "DescriptionText", "木を育てて魔法の森を広げよう", 40, jpFont,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Controls
        var ipCtrl = CT(ipBg.transform, "ControlsText", "木をタップして育てる、魔力でアップグレードを購入", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        // Goal
        var ipGoal = CT(ipBg.transform, "GoalText", "森の面積を目標まで広げてステージクリア", 34, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        // Start button
        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 44, jpFont,
            new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(380, 90), Vector2.zero,
            new Color(0.2f, 0.7f, 0.2f));

        // Help button (bottom right)
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-20, 175),
            new Color(0.2f, 0.5f, 0.2f));

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- UI script ---
        var uiObj = new GameObject("MagicForestUI");
        var ui = uiObj.AddComponent<MagicForestUI>();

        // Wire MagicForestUI fields
        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_manaText", manaText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_areaText", areaText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_worldTreeText", wtText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoGrowBtn", autoGrowBtn.GetComponent<Button>());
        SetField(ui, "_autoGrowCostText", autoGrowCostText);
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText", scText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageBtn", nextStageBtn.GetComponent<Button>());
        SetField(ui, "_gameClearPanel", gcPanel);
        SetField(ui, "_gameClearText", gcText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_retryBtn", retryBtn.GetComponent<Button>());
        SetField(ui, "_stormWarning", stormPanel);
        SetField(ui, "_gameManager", gm);
        SetField(ui, "_forestManager", fm);

        // Wire GameManager fields
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_forestManager", fm);
        SetField(gm, "_ui", ui);

        // Wire ForestManager fields
        SetField(fm, "_gameManager", gm);
        SetField(fm, "_ui", ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText", ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText", ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText", ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton", startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton", helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot", ipBg);

        // Wire button OnClick events
        nextStageBtn.GetComponent<Button>().onClick.AddListener(ui.OnNextStageButtonClicked);
        retryBtn.GetComponent<Button>().onClick.AddListener(ui.OnRetryButtonClicked);
        autoGrowBtn.GetComponent<Button>().onClick.AddListener(ui.OnAutoGrowButtonClicked);

        // Save scene
        string scenePath = "Assets/Scenes/062v2_MagicForest.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup062v2] MagicForest シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup062v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
