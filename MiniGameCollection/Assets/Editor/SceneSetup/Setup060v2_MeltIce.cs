using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game060v2_MeltIce;

public static class Setup060v2_MeltIce
{
    [MenuItem("Assets/Setup/060v2 MeltIce")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup060v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game060v2_MeltIce/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.025f, 0.025f, 1f);
        }

        // Sun object (visual only, top center)
        Sprite sunSprite = LoadSprite(sp + "Sun.png");
        if (sunSprite != null)
        {
            var sunObj = new GameObject("SunVisual");
            var sunSr = sunObj.AddComponent<SpriteRenderer>();
            sunSr.sprite = sunSprite;
            sunSr.sortingOrder = 1;
            sunObj.transform.position = new Vector3(0f, 5.0f, 0f);
            sunObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }

        // Sprites
        Sprite mirrorSprite = LoadSprite(sp + "Mirror.png");
        Sprite iceSprite = LoadSprite(sp + "IceBlock.png");
        Sprite forbiddenSprite = LoadSprite(sp + "ForbiddenIce.png");
        Sprite wallSprite = LoadSprite(sp + "Wall.png");
        Sprite prismSprite = LoadSprite(sp + "Prism.png");
        Sprite gridCellSprite = LoadSprite(sp + "GridCell.png");

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<MeltIceGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // LightRaySystem
        var lrsObj = new GameObject("LightRaySystem");
        lrsObj.transform.SetParent(gmObj.transform);
        var lrs = lrsObj.AddComponent<LightRaySystem>();

        // BoardContainer
        var boardContainerObj = new GameObject("BoardContainer");
        boardContainerObj.transform.SetParent(gmObj.transform);

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD top
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 55), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 30, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(280, 50), new Vector2(20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var mirrorCountText = CT(canvasObj.transform, "MirrorCountText", "鏡: 2", 30, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(200, 50), new Vector2(-20, -30));
        mirrorCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        mirrorCountText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);

        // Bottom button row
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 55), new Vector2(0, 15),
            new Color(0.3f, 0.3f, 0.3f, 0.85f));

        // Stage Clear Panel
        var clearPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        var cpImg = clearPanel.AddComponent<Image>(); cpImg.color = new Color(0f, 0.15f, 0.3f, 0.88f);
        var cpRT = clearPanel.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.1f, 0.35f); cpRT.anchorMax = new Vector2(0.9f, 0.65f);
        cpRT.offsetMin = cpRT.offsetMax = Vector2.zero;

        var cpTitle = CT(clearPanel.transform, "Title", "ステージクリア！", 46, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 75), Vector2.zero);
        cpTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cpTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 1f);

        var cpScore = CT(clearPanel.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        cpScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var cpNextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(300, 65), Vector2.zero,
            new Color(0.1f, 0.45f, 0.75f));
        clearPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = new GameObject("GameClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcImg = gcPanel.AddComponent<Image>(); gcImg.color = new Color(0f, 0.1f, 0.25f, 0.92f);
        var gcRT = gcPanel.GetComponent<RectTransform>();
        gcRT.anchorMin = new Vector2(0.05f, 0.3f); gcRT.anchorMax = new Vector2(0.95f, 0.7f);
        gcRT.offsetMin = gcRT.offsetMax = Vector2.zero;

        var gcTitle = CT(gcPanel.transform, "Title", "全ステージクリア！", 46, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(700, 75), Vector2.zero);
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var gcScore = CT(gcPanel.transform, "ScoreText", "Total Score: 0", 38, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(600, 60), Vector2.zero);
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var gcRetry = CB(gcPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.3f, 0.2f), new Vector2(0.3f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.15f, 0.45f, 0.7f));
        var gcMenu = CB(gcPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.7f, 0.2f), new Vector2(0.7f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        gcPanel.SetActive(false);

        // Game Over Panel
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goImg = goPanel.AddComponent<Image>(); goImg.color = new Color(0.3f, 0.02f, 0.0f, 0.9f);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.35f); goRT.anchorMax = new Vector2(0.9f, 0.65f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;

        var goTitle = CT(goPanel.transform, "Title", "ゲームオーバー！", 44, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);

        var goMsg = CT(goPanel.transform, "Message", "赤い氷を溶かしてしまった！", 30, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(700, 55), Vector2.zero);
        goMsg.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goRetry = CB(goPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.15f, 0.45f, 0.7f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.7f, 0.22f), new Vector2(0.7f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        goPanel.SetActive(false);

        // InstructionPanel overlay canvas
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipCanvasComp = ipCanvas.AddComponent<Canvas>();
        ipCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvasComp.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipRoot = new GameObject("InstructionPanel", typeof(RectTransform));
        ipRoot.transform.SetParent(ipCanvas.transform, false);
        var ipImg = ipRoot.AddComponent<Image>(); ipImg.color = new Color(0.04f, 0.12f, 0.22f, 0.95f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one; ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "", 54, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(900, 85), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 1f);

        var ipDesc = CT(ipRoot.transform, "DescriptionText", "", 34, jpFont,
            new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipRoot.transform, "ControlsText", "", 30, jpFont,
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.75f);

        var ipGoal = CT(ipRoot.transform, "GoalText", "", 30, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.7f);

        var ipStartBtn = CB(ipRoot.transform, "StartButton", "はじめる", 38, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(270, 75), Vector2.zero,
            new Color(0.1f, 0.4f, 0.65f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(65, 65), new Vector2(-20, 90),
            new Color(0.2f, 0.35f, 0.5f, 0.9f));

        var ip = ipRoot.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipRoot;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = ipHelpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // MeltIceUI
        var uiObj = new GameObject("MeltIceUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<MeltIceUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_mirrorCountText").objectReferenceValue = mirrorCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = cpScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = cpNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_retryButton").objectReferenceValue = gcRetry.GetComponent<Button>();
        uiSO.FindProperty("_retryButtonGO").objectReferenceValue = goRetry.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager wiring
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_boardContainer").objectReferenceValue = boardContainerObj.transform;
        gmSO.FindProperty("_lightRaySystem").objectReferenceValue = lrs;
        gmSO.FindProperty("_mirrorSprite").objectReferenceValue = mirrorSprite;
        gmSO.FindProperty("_iceSprite").objectReferenceValue = iceSprite;
        gmSO.FindProperty("_forbiddenIceSprite").objectReferenceValue = forbiddenSprite;
        gmSO.FindProperty("_wallSprite").objectReferenceValue = wallSprite;
        gmSO.FindProperty("_prismSprite").objectReferenceValue = prismSprite;
        gmSO.FindProperty("_gridCellSprite").objectReferenceValue = gridCellSprite;
        gmSO.ApplyModifiedProperties();

        // Button onClick wiring
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cpNextBtn.GetComponent<Button>().onClick, gm.OnNextStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);

        // EventSystem
        var evSys = new GameObject("EventSystem");
        evSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evSys.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/060v2_MeltIce.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup060v2] Scene created: " + scenePath);
    }

    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            if (!File.Exists(path)) return null;
            AssetDatabase.ImportAsset(path);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        if (tex == null) return null;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CB(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = bgColor;
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var label = new GameObject("Text", typeof(RectTransform));
        label.transform.SetParent(go.transform, false);
        var tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;
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
