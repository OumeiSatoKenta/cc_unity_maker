using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game058v2_ThreadNeedle;

public static class Setup058v2_ThreadNeedle
{
    [MenuItem("Assets/Setup/058v2 ThreadNeedle")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup058v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game058v2_ThreadNeedle/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.04f, 0.18f);
            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
        }

        float camSize = 6.0f;

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.024f, 0.062f, 1f);
        }

        // Load sprites
        Sprite needleSprite = LoadSprite(sp + "Needle.png");
        Sprite needleHoleSprite = LoadSprite(sp + "NeedleHole.png");
        Sprite threadSprite = LoadSprite(sp + "Thread.png");

        // --- Needle ---
        float needleY = camSize * 0.3f;
        var needleObj = new GameObject("Needle");
        needleObj.transform.position = new Vector3(0, needleY, 0);
        var needleSr = needleObj.AddComponent<SpriteRenderer>();
        if (needleSprite != null) { needleSr.sprite = needleSprite; needleSr.sortingOrder = 5; }
        needleObj.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

        // NeedleHole child
        var holeObj = new GameObject("NeedleHole");
        holeObj.transform.SetParent(needleObj.transform);
        holeObj.transform.localPosition = new Vector3(0, 0.5f, 0); // top of needle
        var holeSr = holeObj.AddComponent<SpriteRenderer>();
        if (needleHoleSprite != null) { holeSr.sprite = needleHoleSprite; holeSr.sortingOrder = 6; }
        holeObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f); // initial Stage 1 hole size

        // --- Thread launch point ---
        var launchObj = new GameObject("ThreadLaunchPoint");
        float launchY = -camSize * 0.5f;
        launchObj.transform.position = new Vector3(0, launchY, 0);

        // --- Thread visual (SpriteRenderer with size control) ---
        var threadObj = new GameObject("Thread");
        threadObj.transform.position = launchObj.transform.position;
        var threadSr = threadObj.AddComponent<SpriteRenderer>();
        if (threadSprite != null)
        {
            threadSr.sprite = threadSprite;
            threadSr.drawMode = SpriteDrawMode.Tiled;
            threadSr.size = new Vector2(0.1f, 0f);
            threadSr.sortingOrder = 4;
        }
        threadObj.SetActive(false);

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<ThreadNeedleGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // NeedleController
        var ncObj = new GameObject("NeedleController");
        ncObj.transform.SetParent(gmObj.transform);
        var nc = ncObj.AddComponent<NeedleController>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD top
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 50), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 30, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(280, 45), new Vector2(20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var roundText = CT(canvasObj.transform, "RoundText", "Round 0 / 3", 28, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(260, 45), new Vector2(-20, -30));
        roundText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Miss indicators (3 circles top-center-left area)
        var missParent = new GameObject("MissIndicators", typeof(RectTransform));
        missParent.transform.SetParent(canvasObj.transform, false);
        var missRT = missParent.GetComponent<RectTransform>();
        missRT.anchorMin = new Vector2(0.5f, 1); missRT.anchorMax = new Vector2(0.5f, 1);
        missRT.pivot = new Vector2(0.5f, 1); missRT.sizeDelta = new Vector2(120, 40); missRT.anchoredPosition = new Vector2(0, -80);
        var missImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var mObj = new GameObject($"Miss{i}", typeof(RectTransform));
            mObj.transform.SetParent(missParent.transform, false);
            var mImg = mObj.AddComponent<Image>();
            mImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            var mRT = mObj.GetComponent<RectTransform>();
            mRT.anchorMin = new Vector2(0, 0.5f); mRT.anchorMax = new Vector2(0, 0.5f);
            mRT.pivot = new Vector2(0, 0.5f);
            mRT.sizeDelta = new Vector2(32, 32);
            mRT.anchoredPosition = new Vector2(i * 40, 0);
            missImages[i] = mImg;
        }

        // Combo text (center-ish)
        var comboText = CT(canvasObj.transform, "ComboText", "", 42, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Judgement text (center)
        var judgementText = CT(canvasObj.transform, "JudgementText", "", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 100), Vector2.zero);
        judgementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        judgementText.SetActive(false);

        // Menu button (bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 60), new Vector2(0, 15),
            new Color(0.3f, 0.3f, 0.3f, 0.8f));

        // Stage Clear Panel
        var clearPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        var cpImg = clearPanel.AddComponent<Image>(); cpImg.color = new Color(0, 0, 0, 0.75f);
        var cpRT = clearPanel.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.1f, 0.35f); cpRT.anchorMax = new Vector2(0.9f, 0.65f);
        cpRT.offsetMin = cpRT.offsetMax = Vector2.zero;
        var cpTitle = CT(clearPanel.transform, "Title", "ステージクリア！", 44, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        cpTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cpTitle.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var cpScore = CT(clearPanel.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        cpScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var cpNextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(280, 65), Vector2.zero,
            new Color(0.2f, 0.6f, 0.2f));
        clearPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = new GameObject("GameClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcImg = gcPanel.AddComponent<Image>(); gcImg.color = new Color(0, 0, 0, 0.8f);
        var gcRT = gcPanel.GetComponent<RectTransform>();
        gcRT.anchorMin = new Vector2(0.05f, 0.25f); gcRT.anchorMax = new Vector2(0.95f, 0.75f);
        gcRT.offsetMin = gcRT.offsetMax = Vector2.zero;
        var gcTitle = CT(gcPanel.transform, "Title", "ゲームクリア！", 48, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = Color.green;
        var gcScore = CT(gcPanel.transform, "ScoreText", "Total Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), Vector2.zero);
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetry = CB(gcPanel.transform, "RetryButton", "もう一度", 34, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
        var gcMenu = CB(gcPanel.transform, "MenuButton", "メニューへ", 34, jpFont,
            new Vector2(0.7f, 0.22f), new Vector2(0.7f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        gcPanel.SetActive(false);

        // Game Over Panel
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goImg = goPanel.AddComponent<Image>(); goImg.color = new Color(0.5f, 0, 0, 0.8f);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.35f); goRT.anchorMax = new Vector2(0.9f, 0.65f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goTitle = CT(goPanel.transform, "Title", "ゲームオーバー", 44, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 70), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScore = CT(goPanel.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 55), Vector2.zero);
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetry = CB(goPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.2f, 0.5f, 0.8f));
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
        var ipImg = ipRoot.AddComponent<Image>(); ipImg.color = new Color(0f, 0.05f, 0.15f, 0.95f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one; ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "", 52, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var ipDesc = CT(ipRoot.transform, "DescriptionText", "", 34, jpFont,
            new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipRoot.transform, "ControlsText", "", 30, jpFont,
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipRoot.transform, "GoalText", "", 30, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.8f);

        var ipStartBtn = CB(ipRoot.transform, "StartButton", "はじめる", 36, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(260, 70), Vector2.zero,
            new Color(0.2f, 0.4f, 0.8f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(65, 65), new Vector2(-20, 90),
            new Color(0.2f, 0.3f, 0.5f, 0.9f));

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

        // ThreadNeedleUI
        var uiObj = new GameObject("ThreadNeedleUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<ThreadNeedleUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_roundText").objectReferenceValue = roundText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_judgementText").objectReferenceValue = judgementText.GetComponent<TextMeshProUGUI>();
        // Miss indicators array
        var missArrProp = uiSO.FindProperty("_missIndicators");
        missArrProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            missArrProp.GetArrayElementAtIndex(i).objectReferenceValue = missImages[i];
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = cpScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // NeedleController wiring
        var ncSO = new SerializedObject(nc);
        ncSO.FindProperty("_gameManager").objectReferenceValue = gm;
        ncSO.FindProperty("_ui").objectReferenceValue = ui;
        ncSO.FindProperty("_needleTransform").objectReferenceValue = needleObj.transform;
        ncSO.FindProperty("_threadLaunchPoint").objectReferenceValue = launchObj.transform;
        ncSO.FindProperty("_needleHoleRenderer").objectReferenceValue = holeSr;
        ncSO.FindProperty("_threadVisual").objectReferenceValue = threadObj.transform;
        ncSO.FindProperty("_threadRenderer").objectReferenceValue = threadSr;
        ncSO.ApplyModifiedProperties();

        // GameManager wiring
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_needleController").objectReferenceValue = nc;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button onClick (persistent listeners)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cpNextBtn.GetComponent<Button>().onClick, gm.OnNextStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcRetry.GetComponent<Button>().onClick, gm.OnRetry);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gcMenu.GetComponent<Button>().onClick, gm.OnBackToMenu);

        // EventSystem
        var evSys = new GameObject("EventSystem");
        evSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evSys.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/058v2_ThreadNeedle.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup058v2] Scene created: " + scenePath);
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
