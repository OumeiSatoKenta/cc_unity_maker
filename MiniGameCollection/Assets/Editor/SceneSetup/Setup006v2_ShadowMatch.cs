using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game006v2_ShadowMatch;

public static class Setup006v2_ShadowMatch
{
    [MenuItem("Assets/Setup/006v2 ShadowMatch")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup006v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game006v2_ShadowMatch/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.05f, 0.15f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.gameObject.name = "Main Camera";
        }

        // Sprites
        Sprite spBg        = LoadSprite(sp + "background.png");
        Sprite spCube      = LoadSprite(sp + "object_cube.png");
        Sprite spLShape    = LoadSprite(sp + "object_l_shape.png");
        Sprite spShadow    = LoadSprite(sp + "shadow_shape.png");
        Sprite spTarget    = LoadSprite(sp + "target_silhouette.png");
        Sprite spHintArrow = LoadSprite(sp + "hint_arrow.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = spBg;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = spBg != null ? new Vector3(0.025f, 0.025f, 1f) : new Vector3(16f, 14f, 1f);

        // ---- World Objects ----
        float camSize = 6f;
        // ObjectDisplay root
        var displayRoot = new GameObject("ObjectDisplay");
        displayRoot.transform.position = Vector3.zero;

        // Main Object (shown at upper center)
        var mainObjGO = new GameObject("MainObject");
        mainObjGO.transform.SetParent(displayRoot.transform);
        mainObjGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        var mainSr = mainObjGO.AddComponent<SpriteRenderer>();
        mainSr.sprite = spCube;
        mainSr.sortingOrder = 5;
        mainObjGO.transform.localScale = Vector3.one * 1.2f;

        // Shadow Display (center)
        var shadowGO = new GameObject("ShadowDisplay");
        shadowGO.transform.SetParent(displayRoot.transform);
        shadowGO.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        var shadowSr = shadowGO.AddComponent<SpriteRenderer>();
        shadowSr.sprite = spShadow;
        shadowSr.color = new Color(0.5f, 0.5f, 0.7f, 0.85f);
        shadowSr.sortingOrder = 3;
        shadowGO.transform.localScale = Vector3.one * 1.0f;

        // Target Silhouette (lower)
        var targetGO = new GameObject("TargetSilhouette");
        targetGO.transform.SetParent(displayRoot.transform);
        targetGO.transform.localPosition = new Vector3(0f, -2.6f, 0f);
        var targetSr = targetGO.AddComponent<SpriteRenderer>();
        targetSr.sprite = spTarget;
        targetSr.color = new Color(1f, 1f, 1f, 0.8f);
        targetSr.sortingOrder = 2;
        targetGO.transform.localScale = Vector3.one * 0.9f;

        // Hint Arrow
        var hintArrowGO = new GameObject("HintArrow");
        hintArrowGO.transform.SetParent(displayRoot.transform);
        hintArrowGO.transform.localPosition = new Vector3(2.5f, 1.8f, 0f);
        var hintSr = hintArrowGO.AddComponent<SpriteRenderer>();
        hintSr.sprite = spHintArrow;
        hintSr.sortingOrder = 10;
        hintArrowGO.transform.localScale = Vector3.one * 0.5f;
        hintArrowGO.SetActive(false);

        // "Target:" label
        var labelGO = new GameObject("TargetLabel");
        labelGO.transform.SetParent(displayRoot.transform);
        labelGO.transform.localPosition = new Vector3(-2.2f, -1.8f, 0f);

        // ---- GameManager ----
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<ShadowMatchGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // ShadowObjectController
        var socObj = new GameObject("ShadowObjectController");
        socObj.transform.SetParent(gmObj.transform);
        var soc = socObj.AddComponent<ShadowObjectController>();
        var socSO = new SerializedObject(soc);
        socSO.FindProperty("_objectRenderer").objectReferenceValue = mainSr;
        socSO.FindProperty("_shadowRenderer").objectReferenceValue = shadowSr;
        socSO.FindProperty("_targetRenderer").objectReferenceValue = targetSr;
        socSO.FindProperty("_hintArrow").objectReferenceValue = hintArrowGO;
        socSO.FindProperty("_mainCamera").objectReferenceValue = camera;
        socSO.FindProperty("_spriteObjectCube").objectReferenceValue = spCube;
        socSO.FindProperty("_spriteObjectLShape").objectReferenceValue = spLShape;
        socSO.FindProperty("_spriteShadow").objectReferenceValue = spShadow;
        socSO.FindProperty("_spriteTarget").objectReferenceValue = spTarget;
        socSO.FindProperty("_spriteHintArrow").objectReferenceValue = spHintArrow;
        socSO.ApplyModifiedProperties();

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD top
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 30, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(400, 40), new Vector2(0, -15));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 40), new Vector2(0, -55));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        var matchText = CT(canvasObj.transform, "MatchText", "一致度: 0%", 26, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1),
            new Vector2(200, 35), new Vector2(-15, -50));
        matchText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.7f);
        matchText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        var judgeText = CT(canvasObj.transform, "JudgeCountText", "判定: 0回", 24, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1),
            new Vector2(200, 35), new Vector2(15, -50));
        judgeText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.9f);

        var hintCountText = CT(canvasObj.transform, "HintCountText", "ヒント残り: 3回", 22, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1),
            new Vector2(220, 35), new Vector2(15, -85));
        hintCountText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.4f);

        // Buttons (bottom)
        var judgeBtn = CB(canvasObj.transform, "JudgeButton", "判定", 32, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(180, 65), new Vector2(-110, 80),
            new Color(0.1f, 0.45f, 0.8f));

        var hintBtn = CB(canvasObj.transform, "HintButton", "ヒント(3)", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(160, 65), new Vector2(100, 80),
            new Color(0.5f, 0.3f, 0.1f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 22, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(150, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f));
        var scStageText = CT(scPanel.transform, "SCStageText", "", 36, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero);
        scStageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStageText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 1f);
        var scScoreText = CT(scPanel.transform, "SCScoreText", "", 28, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 45), Vector2.zero);
        scScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var scStarsText = CT(scPanel.transform, "SCStarsText", "", 42, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 55), Vector2.zero);
        scStarsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStarsText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 30, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 65), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));
        scPanel.SetActive(false);

        // Game Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.12f, 0.18f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "全ステージクリア！", 40, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScore", "", 30, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 50), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var clearMenuBtn = CB(clearPanel.transform, "ClearMenuButton", "メニューへ戻る", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // InstructionPanel
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.02f, 0.05f, 0.15f, 0.97f));
        var ipRect = ipPanel.GetComponent<RectTransform>();
        ipRect.anchorMin = Vector2.zero; ipRect.anchorMax = Vector2.one;
        ipRect.offsetMin = ipRect.offsetMax = Vector2.zero;

        var ipTitle = CT(ipPanel.transform, "IPTitle", "", 42, jpFont,
            new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.8f, 1f);
        var ipDesc = CT(ipPanel.transform, "IPDesc", "", 28, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 50), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var ipControls = CT(ipPanel.transform, "IPControls", "", 24, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);
        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 24, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.6f);
        var ipStartBtn = CB(ipPanel.transform, "StartButton", "はじめる", 34, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 70), Vector2.zero, new Color(0.1f, 0.5f, 0.8f));
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 28, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(55, 55), new Vector2(-20, 155), new Color(0.3f, 0.3f, 0.5f, 0.8f));

        // InstructionPanel component
        var ipObj = new GameObject("InstructionPanelController");
        ipObj.transform.SetParent(gmObj.transform);
        var ip = ipObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = helpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ShadowMatchUI
        var uiObj = new GameObject("ShadowMatchUI");
        uiObj.transform.SetParent(gmObj.transform);
        var smUI = uiObj.AddComponent<ShadowMatchUI>();
        var uiSO = new SerializedObject(smUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_matchText").objectReferenceValue = matchText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_judgeCountText").objectReferenceValue = judgeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_hintCountText").objectReferenceValue = hintCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearStageText").objectReferenceValue = scStageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStarsText").objectReferenceValue = scStarsText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_shadowObjectController").objectReferenceValue = soc;
        gmSO.FindProperty("_ui").objectReferenceValue = smUI;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(judgeBtn.GetComponent<Button>().onClick, gm.OnJudgeButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(hintBtn.GetComponent<Button>().onClick, gm.OnHintButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStageButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/006v2_ShadowMatch.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup006v2] ShadowMatch シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.05f, 0.25f);
        r.anchorMax = new Vector2(0.95f, 0.75f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg;
        o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        return o;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
