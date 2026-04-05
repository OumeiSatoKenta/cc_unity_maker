using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game015v2_TileTurn;

public static class Setup015v2_TileTurn
{
    [MenuItem("Assets/Setup/015v2 TileTurn")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup015v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game015v2_TileTurn/";

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
        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spNormal   = LoadSprite(sp + "TileNormal.png");
        Sprite spLinked   = LoadSprite(sp + "TileLinked.png");
        Sprite spLocked   = LoadSprite(sp + "TileLocked.png");
        Sprite spFlipped  = LoadSprite(sp + "TileFlipped.png");
        Sprite spCorrect  = LoadSprite(sp + "TileCorrect.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.02f, 0.02f, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<TileTurnGameManager>();

        // StageManager (child)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // TileManager (child)
        var tmObj = new GameObject("TileManager");
        tmObj.transform.SetParent(gmObj.transform);
        var tm = tmObj.AddComponent<TileManager>();
        var tmSO = new SerializedObject(tm);
        tmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        tmSO.FindProperty("_tileNormalSprite").objectReferenceValue = spNormal;
        tmSO.FindProperty("_tileLinkSprite").objectReferenceValue = spLinked;
        tmSO.FindProperty("_tileLockedSprite").objectReferenceValue = spLocked;
        tmSO.FindProperty("_tileFlippedSprite").objectReferenceValue = spFlipped;
        tmSO.FindProperty("_tileCorrectSprite").objectReferenceValue = spCorrect;
        // _ui will be wired after TileTurnUI is created
        tmSO.ApplyModifiedProperties();

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

        // HUD: Stage text (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(360, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(360, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Rotations (top center)
        var rotText = CT(canvasObj.transform, "RotationsText", "残り: 16/16", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 55), new Vector2(0, -30));
        rotText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rotText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.75f, 0.3f);

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 55), new Vector2(0, -90));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.2f);

        // ---- Bottom buttons ----
        var restartBtn = CB(canvasObj.transform, "RestartButton", "リスタート", jpFont,
            new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 65), new Vector2(0, 80),
            new Color(0.55f, 0.35f, 0.15f));

        var previewBtn = CB(canvasObj.transform, "PreviewButton", "プレビュー", jpFont,
            new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 65), new Vector2(0, 80),
            new Color(0.15f, 0.5f, 0.65f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 55), new Vector2(0, 10),
            new Color(0.3f, 0.3f, 0.4f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.05f, 0.15f, 0.93f),
            new Vector2(700, 480));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(640, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scScore = CT(scPanel.transform, "StageClearScoreText", "+0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 30));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        var scCombo = CT(scPanel.transform, "StageClearComboText", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 50), new Vector2(0, -30));
        scCombo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scCombo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30),
            new Color(0.2f, 0.55f, 0.9f));
        scPanel.SetActive(false);

        // ---- Game Over Panel ----
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.03f, 0.03f, 0.93f),
            new Vector2(680, 420));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(620, 70), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var retryBtn = CB(goPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 70), new Vector2(0, 70),
            new Color(0.6f, 0.35f, 0.15f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 60), new Vector2(0, 10),
            new Color(0.25f, 0.25f, 0.4f));
        goPanel.SetActive(false);

        // ---- Full Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.04f, 0.12f, 0.05f, 0.95f),
            new Vector2(700, 480));
        var gcTitle = CT(gcPanel.transform, "ClearTitle", "全ステージクリア！", 50, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 70), new Vector2(0, -30));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);
        var gcTotal = CT(gcPanel.transform, "ClearTotalScoreText", "Total: 0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        gcTotal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTotal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
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

        var ipTitle = CT(ipPanel.transform, "IPTitle", "TileTurn", 60, jpFont,
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
            new Vector2(75, 75), new Vector2(-10, 155),
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

        // ---- TileTurnUI ----
        var uiObj = new GameObject("TileTurnUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ttUI = uiObj.AddComponent<TileTurnUI>();
        var uiSO = new SerializedObject(ttUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_rotationsText").objectReferenceValue = rotText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearComboText").objectReferenceValue = scCombo.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_clearTotalScoreText").objectReferenceValue = gcTotal.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_tileManager").objectReferenceValue = tm;
        uiSO.ApplyModifiedProperties();

        // Wire TileManager._ui (must be done after TileTurnUI is created)
        tmSO.FindProperty("_ui").objectReferenceValue = ttUI;
        tmSO.ApplyModifiedProperties();

        // ---- Wire GameManager ----
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_tileManager").objectReferenceValue = tm;
        gmSO.FindProperty("_ui").objectReferenceValue = ttUI;
        gmSO.ApplyModifiedProperties();

        // ---- Button Events ----
        AddButtonOnClick(restartBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(previewBtn.GetComponent<Button>(), ttUI, "OnPreviewDown");
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(nextBtn.GetComponent<Button>(), gm, "OnNextStage");
        AddButtonOnClick(retryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), gm, "ShowInstructions");

        // ---- Save Scene ----
        string scenePath = "Assets/Scenes/015v2_TileTurn.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup015v2] Scene created: " + scenePath);
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
            Debug.LogWarning($"[Setup015v2] Sprite not found: {path}");
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
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
