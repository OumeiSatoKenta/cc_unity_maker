using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game012v2_BridgeBuilder;

public static class Setup012v2_BridgeBuilder
{
    [MenuItem("Assets/Setup/012v2 BridgeBuilder")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup012v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game012v2_BridgeBuilder/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.22f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Sprites
        Sprite spBg     = LoadSprite(sp + "Background.png");
        Sprite spAnchor = LoadSprite(sp + "Anchor.png");
        Sprite spWood   = LoadSprite(sp + "WoodPlank.png");
        Sprite spSteel  = LoadSprite(sp + "SteelBeam.png");
        Sprite spRope   = LoadSprite(sp + "Rope.png");
        Sprite spCar    = LoadSprite(sp + "Car.png");
        Sprite spGoal   = LoadSprite(sp + "Goal.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BridgeBuilderGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // BridgeManager
        var managerObj = new GameObject("BridgeManager");
        managerObj.transform.SetParent(gmObj.transform);
        var bridgeMgr = managerObj.AddComponent<BridgeManager>();

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

        // HUD: Stage (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(360, 55), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(360, 55), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Budget (top center)
        var budgetText = CT(canvasObj.transform, "BudgetText", "予算: $500", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 55), new Vector2(0, -20));
        budgetText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        budgetText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);

        // Combo (top center, below budget)
        var comboText = CT(canvasObj.transform, "ComboText", "", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 45), new Vector2(0, -75));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.2f);

        // Feedback text (center, hidden)
        var feedbackText = CT(canvasObj.transform, "FeedbackText", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 60), new Vector2(0, 100));
        feedbackText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        feedbackText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);
        feedbackText.SetActive(false);

        // ---- Part Button Container ----
        var partPanel = new GameObject("PartButtonContainer");
        partPanel.transform.SetParent(canvasObj.transform, false);
        var ppRt = partPanel.AddComponent<RectTransform>();
        ppRt.anchorMin = new Vector2(0f, 0f);
        ppRt.anchorMax = new Vector2(1f, 0f);
        ppRt.pivot = new Vector2(0.5f, 0f);
        ppRt.sizeDelta = new Vector2(0, 130);
        ppRt.anchoredPosition = new Vector2(0, 120);
        var ppImg = partPanel.AddComponent<Image>();
        ppImg.color = new Color(0.06f, 0.08f, 0.18f, 0.92f);

        // HorizontalLayoutGroup
        var hlg = partPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 20;
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Create 3 part buttons (Wood, Steel, Rope)
        string[] partNames = { "木材\n$50", "鉄骨\n$150", "ロープ\n$80" };
        Color[] partColors = {
            new Color(0.5f, 0.3f, 0.15f),
            new Color(0.25f, 0.35f, 0.55f),
            new Color(0.6f, 0.5f, 0.2f)
        };
        for (int i = 0; i < 3; i++)
        {
            var btn = CB(partPanel.transform, $"PartBtn_{i}", partNames[i], jpFont,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(200, 100), Vector2.zero, partColors[i]);
            var le = btn.AddComponent<LayoutElement>();
            le.preferredWidth = 200;
            le.preferredHeight = 100;
        }

        // ---- Bottom action buttons ----
        // Test button (bottom center-right)
        var testBtn = CB(canvasObj.transform, "TestButton", "テスト走行", jpFont,
            new Vector2(0.65f, 0f), new Vector2(0.65f, 0f), new Vector2(0.5f, 0f),
            new Vector2(250, 65), new Vector2(0, 270),
            new Color(0.2f, 0.7f, 0.3f));

        // Retry button (bottom center-left)
        var retryBtn = CB(canvasObj.transform, "RetryButton", "リセット", jpFont,
            new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 65), new Vector2(0, 270),
            new Color(0.6f, 0.4f, 0.2f));

        // Menu button (bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 55), new Vector2(0, 10),
            new Color(0.3f, 0.3f, 0.4f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.05f, 0.15f, 0.92f),
            new Vector2(700, 500));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scScore = CT(scPanel.transform, "StageClearScore", "+1000", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        var scStars = CT(scPanel.transform, "StageClearStars", "★★★", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, -40));
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30),
            new Color(0.2f, 0.6f, 0.9f));
        scPanel.SetActive(false);

        // ---- Game Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.1f, 0.05f, 0.95f),
            new Vector2(700, 500));
        var gcTitle = CT(gcPanel.transform, "GameClearTitle", "全ステージクリア！", 50, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 70), new Vector2(0, -30));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);
        var gcScore = CT(gcPanel.transform, "GameClearScore", "Total: 0", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var gcMenuBtn = CB(gcPanel.transform, "GCMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(0, 30),
            new Color(0.3f, 0.3f, 0.5f));
        gcPanel.SetActive(false);

        // ---- Instruction Panel ----
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.04f, 0.05f, 0.15f, 0.97f),
            new Vector2(0, 0));
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "BridgeBuilder", 60, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 200));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.8f, 1f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 60), new Vector2(0, 100));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 60), new Vector2(0, -80));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 75), new Vector2(0, -200),
            new Color(0.2f, 0.6f, 0.9f));

        // "?" button
        var reShowBtn = CB(canvasObj.transform, "ReShowInstructionBtn", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(75, 75), new Vector2(-10, 230),
            new Color(0.2f, 0.4f, 0.7f));

        // Wire InstructionPanel
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ---- BridgeBuilderUI ----
        var uiObj = new GameObject("BridgeBuilderUI");
        uiObj.transform.SetParent(gmObj.transform);
        var bbUI = uiObj.AddComponent<BridgeBuilderUI>();
        var uiSO = new SerializedObject(bbUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_budgetText").objectReferenceValue = budgetText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_feedbackText").objectReferenceValue = feedbackText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_partButtonContainer").objectReferenceValue = partPanel.transform;
        uiSO.FindProperty("_testButton").objectReferenceValue = testBtn.GetComponent<Button>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStarsText").objectReferenceValue = scStars.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // ---- Wire BridgeManager ----
        var bmSO = new SerializedObject(bridgeMgr);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.FindProperty("_ui").objectReferenceValue = bbUI;
        bmSO.FindProperty("_anchorSprite").objectReferenceValue = spAnchor;
        bmSO.FindProperty("_woodSprite").objectReferenceValue = spWood;
        bmSO.FindProperty("_steelSprite").objectReferenceValue = spSteel;
        bmSO.FindProperty("_ropeSprite").objectReferenceValue = spRope;
        bmSO.FindProperty("_carSprite").objectReferenceValue = spCar;
        bmSO.FindProperty("_goalSprite").objectReferenceValue = spGoal;
        bmSO.ApplyModifiedProperties();

        // ---- Wire GameManager ----
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_bridgeManager").objectReferenceValue = bridgeMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = bbUI;
        gmSO.ApplyModifiedProperties();

        // ---- Button Events ----
        AddButtonOnClick(testBtn.GetComponent<Button>(), gm, "OnTestStart");
        AddButtonOnClick(retryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(nextBtn.GetComponent<Button>(), gm, "OnNextStage");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");

        // ---- Save Scene ----
        string scenePath = "Assets/Scenes/012v2_BridgeBuilder.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup012v2] Scene created: " + scenePath);
    }

    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
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
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28;
        if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            Debug.LogWarning($"[Setup012v2] Sprite not found: {path}");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path)
            ?? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
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
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
