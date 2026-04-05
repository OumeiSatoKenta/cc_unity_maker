using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game028v2_RopeSwing;

public static class Setup028v2_RopeSwing
{
    [MenuItem("Assets/Setup/028v2 RopeSwing")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup028v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game028v2_RopeSwing/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.07f, 0.10f, 0.20f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Player.png",
            sp+"Platform.png", sp+"GoalPlatform.png",
            sp+"Anchor.png", sp+"RopeLine.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spPlayer   = LoadSprite(sp + "Player.png");
        Sprite spPlatform = LoadSprite(sp + "Platform.png");
        Sprite spGoal     = LoadSprite(sp + "GoalPlatform.png");
        Sprite spAnchor   = LoadSprite(sp + "Anchor.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            if (camera != null)
            {
                float camH = camera.orthographicSize * 2f;
                float camW = camH * camera.aspect;
                float scaleX = camW / (spBg.rect.width / spBg.pixelsPerUnit);
                float scaleY = camH / (spBg.rect.height / spBg.pixelsPerUnit);
                bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        // Player GameObject (just a visual holder)
        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(-3f, -1.8f, 0f);
        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = spPlayer;
        playerSr.sortingOrder = 10;
        if (spPlayer != null)
        {
            float sprSize = spPlayer.rect.width / spPlayer.pixelsPerUnit;
            float targetSize = 0.5f;
            float ps = targetSize / sprSize;
            playerObj.transform.localScale = new Vector3(ps, ps, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<RopeSwingGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // PlatformManager (child of GameManager)
        var pmObj = new GameObject("PlatformManager");
        pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<PlatformManager>();
        var pmSO = new SerializedObject(pm);
        pmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        pmSO.FindProperty("_platformSprite").objectReferenceValue = spPlatform;
        pmSO.FindProperty("_goalSprite").objectReferenceValue = spGoal;
        pmSO.FindProperty("_anchorSprite").objectReferenceValue = spAnchor;
        pmSO.ApplyModifiedProperties();

        // RopeController (child of GameManager)
        var rcObj = new GameObject("RopeController");
        rcObj.transform.SetParent(gmObj.transform);
        var rc = rcObj.AddComponent<RopeController>();
        var rcSO = new SerializedObject(rc);
        rcSO.FindProperty("_gameManager").objectReferenceValue = gm;
        rcSO.FindProperty("_platformManager").objectReferenceValue = pm;
        rcSO.FindProperty("_playerSr").objectReferenceValue = playerSr;
        rcSO.ApplyModifiedProperties();

        // Canvas (main HUD)
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // HUD: Stage - top left
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 55), new Vector2(15, -35));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        // HUD: Score - top right
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(280, 50), new Vector2(-15, -35));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Landing feedback text - center
        var feedbackText = CT(canvasObj.transform, "LandingFeedbackText", "", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 50));
        feedbackText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        feedbackText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        feedbackText.SetActive(false);

        // Menu button - bottom left
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(160, 55), new Vector2(10, 10), new Color(0.2f, 0.2f, 0.3f, 0.85f));

        // Re-show instruction button (?) - bottom right
        var reShowBtn = CB(canvasObj.transform, "ReShowButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-10, 10), new Color(0.15f, 0.15f, 0.25f, 0.85f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0f, 0f, 0f, 0.82f), new Vector2(700, 360));
        scPanel.SetActive(false);
        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 80));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scBonus = CT(scPanel.transform, "SCBonus", "ボーナス: +0", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 55), new Vector2(0, 0));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = Color.white;
        var scNextBtn = CB(scPanel.transform, "SCNextButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), new Vector2(0, -90), new Color(0.1f, 0.5f, 0.2f));

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.88f), new Vector2(700, 420));
        goPanel.SetActive(false);
        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 80), new Vector2(0, 110));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var goScore = CT(goPanel.transform, "GOScore", "スコア: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, 20));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), new Vector2(-125, -100), new Color(0.1f, 0.4f, 0.8f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), new Vector2(125, -100), new Color(0.3f, 0.3f, 0.4f));

        // Final Clear Panel
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0f, 0f, 0f, 0.88f), new Vector2(750, 460));
        fcPanel.SetActive(false);
        var fcTitle = CT(fcPanel.transform, "FCTitle", "全ステージクリア！", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(680, 80), new Vector2(0, 130));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        var fcScore = CT(fcPanel.transform, "FCScore", "最終スコア: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 65), new Vector2(0, 30));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var fcRetryBtn = CB(fcPanel.transform, "FCRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), new Vector2(-125, -110), new Color(0.1f, 0.4f, 0.8f));
        var fcMenuBtn = CB(fcPanel.transform, "FCMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), new Vector2(125, -110), new Color(0.3f, 0.3f, 0.4f));

        // Instruction Panel (separate canvas, sortOrder=100)
        var ipCanvasObj = new GameObject("InstructionCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        var ipScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasObj.AddComponent<GraphicRaycaster>();

        var ipPanel = new GameObject("InstructionPanel");
        ipPanel.transform.SetParent(ipCanvasObj.transform, false);
        var ipImg = ipPanel.AddComponent<Image>();
        ipImg.color = new Color(0f, 0f, 0f, 0.9f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "RopeSwing", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 250));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.2f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 130));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.9f, 0.85f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 140), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.75f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -130));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -270), new Color(0.7f, 0.4f, 0.1f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // RopeSwingUI (child of GameManager)
        var uiObj = new GameObject("RopeSwingUI");
        uiObj.transform.SetParent(gmObj.transform);
        var rsUI = uiObj.AddComponent<RopeSwingUI>();
        var uiSO = new SerializedObject(rsUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_landingFeedbackText").objectReferenceValue = feedbackText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finalRestartButton").objectReferenceValue = fcRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalMenuButton").objectReferenceValue = fcMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_ropeController").objectReferenceValue = rc;
        gmSO.FindProperty("_platformManager").objectReferenceValue = pm;
        gmSO.FindProperty("_ui").objectReferenceValue = rsUI;
        gmSO.ApplyModifiedProperties();

        // Button onClick wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(fcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");

        // Save scene
        string scenePath = "Assets/Scenes/028v2_RopeSwing.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup028v2] Scene created: " + scenePath);
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { AssetDatabase.ImportAsset(path); importer = AssetImporter.GetAtPath(path) as TextureImporter; }
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
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
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;
        var labelObj = new GameObject("Label"); labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28; if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 sizeDelta)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = color;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            else Debug.LogWarning($"[Setup028v2] Sprite not found: {path}");
        }
        return sprite;
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
