using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game099v2_TouchMemory;

public static class Setup099v2_TouchMemory
{
    [MenuItem("Assets/Setup/099v2 TouchMemory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup099v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game099v2_TouchMemory/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.07f, 0.04f, 0.15f);
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
            float bgCamS = 6f;
            float bgCamW = bgCamS * (camera != null ? camera.aspect : (16f / 9f));
            bgObj.transform.localScale = new Vector3(bgCamW * 2f / bgSprite.bounds.size.x, bgCamS * 2f / bgSprite.bounds.size.y, 1f);
        }

        // === Load panel sprites ===
        string[] panelNames = {
            "panel_blue", "panel_red", "panel_green", "panel_purple",
            "panel_yellow", "panel_cyan", "panel_pink", "panel_indigo", "panel_lime"
        };
        var panelSprites = new Sprite[panelNames.Length];
        var panelLitSprites = new Sprite[panelNames.Length];
        for (int i = 0; i < panelNames.Length; i++)
        {
            panelSprites[i] = LoadSprite(sp + panelNames[i] + ".png");
            panelLitSprites[i] = LoadSprite(sp + panelNames[i] + "_lit.png");
        }

        // === GameManager hierarchy ===
        var gmObj = new GameObject("TouchMemoryGameManager");
        var gm = gmObj.AddComponent<TouchMemoryGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1", customData = "4,2,5,false,false" },
            new StageManager.StageConfig { speedMultiplier = 0.8f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 2", customData = "4,3,5,false,false" },
            new StageManager.StageConfig { speedMultiplier = 0.8f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 3", customData = "6,3,5,false,false" },
            new StageManager.StageConfig { speedMultiplier = 0.7f, countMultiplier = 1, complexityFactor = 0.5f, stageName = "Stage 4", customData = "6,4,5,true,false" },
            new StageManager.StageConfig { speedMultiplier = 0.6f, countMultiplier = 1, complexityFactor = 1.0f, stageName = "Stage 5", customData = "9,4,5,true,true" },
        };
        sm.SetConfigs(stageConfigs);

        // TouchMemoryManager (child of GM)
        var tmObj = new GameObject("TouchMemoryManager");
        tmObj.transform.SetParent(gmObj.transform);
        var tm = tmObj.AddComponent<TouchMemoryManager>();
        SetPrivateField(tm, "_gameManager", gm);
        SetPrivateField(tm, "_panelSprites", panelSprites);
        SetPrivateField(tm, "_panelLitSprites", panelLitSprites);

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

        // === HUD ===
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 50), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 1f, 0.3f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 1f, 0.3f);

        var roundTextGo = CT(canvasObj.transform, "RoundText", "Round 1", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 60), new Vector2(0, -15));
        roundTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        roundTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var reverseTextGo = CT(canvasObj.transform, "ReverseText", "【逆順】", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 55), new Vector2(0, -75));
        reverseTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        reverseTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.1f);
        reverseTextGo.SetActive(false);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "コンボ x2！", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 80), new Vector2(0, 100));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        comboTextGo.SetActive(false);

        // === Back To Menu button (bottom left) ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニューへ", 30, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 55), new Vector2(15, 15), new Color(0.15f, 0.1f, 0.25f));

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scPanelRT = scPanel.GetComponent<RectTransform>();
        scPanelRT.anchorMin = new Vector2(0.5f, 0.5f); scPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        scPanelRT.pivot = new Vector2(0.5f, 0.5f); scPanelRT.sizeDelta = new Vector2(700, 440);
        var scPanelImg = scPanel.AddComponent<Image>();
        scPanelImg.color = new Color(0.04f, 0.14f, 0.08f, 0.97f);

        var scTitleGo = CT(scPanel.transform, "StageClearText", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 200), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);

        var nextStageBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ▶", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 80), new Vector2(0, -120), new Color(0.1f, 0.5f, 0.2f));
        scPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goPanelRT = goPanel.GetComponent<RectTransform>();
        goPanelRT.anchorMin = new Vector2(0.5f, 0.5f); goPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRT.pivot = new Vector2(0.5f, 0.5f); goPanelRT.sizeDelta = new Vector2(700, 440);
        var goPanelImg = goPanel.AddComponent<Image>();
        goPanelImg.color = new Color(0.14f, 0.04f, 0.04f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverText", "ミス...", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 200), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        var restartBtn = CB(goPanel.transform, "RestartButton", "再挑戦", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 80), new Vector2(0, -120), new Color(0.4f, 0.1f, 0.1f));
        goPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acPanelRT = acPanel.GetComponent<RectTransform>();
        acPanelRT.anchorMin = new Vector2(0.5f, 0.5f); acPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        acPanelRT.pivot = new Vector2(0.5f, 0.5f); acPanelRT.sizeDelta = new Vector2(700, 440);
        var acPanelImg = acPanel.AddComponent<Image>();
        acPanelImg.color = new Color(0.06f, 0.06f, 0.18f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearText", "全ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(660, 260), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        acPanel.SetActive(false);

        // === TouchMemoryUI ===
        var uiGo = new GameObject("TouchMemoryUI");
        uiGo.transform.SetParent(canvasObj.transform, false);
        var ui = uiGo.AddComponent<TouchMemoryUI>();
        SetPrivateField(ui, "_gameManager", gm);
        SetPrivateField(ui, "_stageText", stageTextGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_scoreText", scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_roundText", roundTextGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_comboText", comboTextGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_reverseText", reverseTextGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_backToMenuButton", backBtn.GetComponent<Button>());
        SetPrivateField(ui, "_stageClearPanel", scPanel);
        SetPrivateField(ui, "_stageClearText", scTitleGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_nextStageButton", nextStageBtn.GetComponent<Button>());
        SetPrivateField(ui, "_gameOverPanel", goPanel);
        SetPrivateField(ui, "_gameOverText", goTitleGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ui, "_restartButton", restartBtn.GetComponent<Button>());
        SetPrivateField(ui, "_allClearPanel", acPanel);
        SetPrivateField(ui, "_allClearText", acTitleGo.GetComponent<TextMeshProUGUI>());

        // === InstructionPanel ===
        var ipCanvasObj = new GameObject("InstructionPanelCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        var ipScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipScaler.matchWidthOrHeight = 0.5f;
        ipCanvasObj.AddComponent<GraphicRaycaster>();

        var ipPanelGo = new GameObject("InstructionPanelRoot", typeof(RectTransform));
        ipPanelGo.transform.SetParent(ipCanvasObj.transform, false);
        var ipPanelRT = ipPanelGo.GetComponent<RectTransform>();
        ipPanelRT.anchorMin = Vector2.zero; ipPanelRT.anchorMax = Vector2.one;
        ipPanelRT.offsetMin = ipPanelRT.offsetMax = Vector2.zero;
        var ipBg = ipPanelGo.AddComponent<Image>();
        ipBg.color = new Color(0f, 0f, 0f, 0.88f);

        var ipTitleGo = CT(ipPanelGo.transform, "IPTitle", "TouchMemory", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 1f);

        var ipDescGo = CT(ipPanelGo.transform, "IPDescription", "光るパターンを記憶して再現しよう", 48, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDescGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDescGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipControlsGo = CT(ipPanelGo.transform, "IPControls", "光った順番にパネルをタップ", 44, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), Vector2.zero);
        ipControlsGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControlsGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.8f);

        var ipGoalGo = CT(ipPanelGo.transform, "IPGoal", "できるだけ多くのラウンドをクリアしよう", 40, jpFont,
            new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), Vector2.zero);
        ipGoalGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoalGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 0.7f);

        var ipBtnGo = CB(ipPanelGo.transform, "IPStartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(420, 90), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));

        var ip = ipPanelGo.AddComponent<InstructionPanel>();
        SetPrivateField(ip, "_titleText", ipTitleGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ip, "_descriptionText", ipDescGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ip, "_controlsText", ipControlsGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ip, "_goalText", ipGoalGo.GetComponent<TextMeshProUGUI>());
        SetPrivateField(ip, "_startButton", ipBtnGo.GetComponent<Button>());
        SetPrivateField(ip, "_panelRoot", ipPanelGo);

        // ? button (re-show instruction)
        var questionBtn = CB(canvasObj.transform, "QuestionButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-15, 15), new Color(0.15f, 0.15f, 0.35f));
        SetPrivateField(ip, "_helpButton", questionBtn.GetComponent<Button>());

        // === EventSystem ===
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // === Wire GameManager fields ===
        SetPrivateField(gm, "_stageManager", sm);
        SetPrivateField(gm, "_instructionPanel", ip);
        SetPrivateField(gm, "_touchMemoryManager", tm);
        SetPrivateField(gm, "_ui", ui);
        SetPrivateField(tm, "_ui", ui);

        // === Save scene ===
        string scenePath = "Assets/Scenes/099v2_TouchMemory.unity";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup099v2] TouchMemory scene created: " + scenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            var bytes = File.Exists(path) ? File.ReadAllBytes(path) : null;
            if (bytes == null) { Debug.LogWarning("[Setup099v2] Sprite not found: " + path); return null; }
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null && tex != null)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return sprite;
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
        btn.colors = colors;

        var txtGo = new GameObject("Text", typeof(RectTransform));
        txtGo.transform.SetParent(go.transform, false);
        var txtRT = txtGo.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup099v2] Field '{fieldName}' not found on {obj.GetType().Name}");
    }
}
