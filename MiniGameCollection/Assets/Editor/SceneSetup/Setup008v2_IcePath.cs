using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game008v2_IcePath;

public static class Setup008v2_IcePath
{
    [MenuItem("Assets/Setup/008v2 IcePath")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup008v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game008v2_IcePath/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.10f, 0.22f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.gameObject.name = "Main Camera";
        }

        // Sprites
        Sprite spBg       = LoadSprite(sp + "background.png");
        Sprite spIce      = LoadSprite(sp + "ice.png");
        Sprite spVisited  = LoadSprite(sp + "visited.png");
        Sprite spRock     = LoadSprite(sp + "rock.png");
        Sprite spCrack    = LoadSprite(sp + "crack.png");
        Sprite spHole     = LoadSprite(sp + "hole.png");
        Sprite spRedirect = LoadSprite(sp + "redirect.png");
        Sprite spFriction = LoadSprite(sp + "friction.png");
        Sprite spPlayer   = LoadSprite(sp + "player.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = spBg;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = spBg != null ? new Vector3(0.024f, 0.024f, 1f) : new Vector3(16f, 14f, 1f);

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<IcePathGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // IceBoardManager
        var boardObj = new GameObject("IceBoardManager");
        boardObj.transform.SetParent(gmObj.transform);
        var board = boardObj.AddComponent<IceBoardManager>();
        var boardSO = new SerializedObject(board);
        boardSO.FindProperty("_iceSprite").objectReferenceValue = spIce;
        boardSO.FindProperty("_visitedSprite").objectReferenceValue = spVisited;
        boardSO.FindProperty("_rockSprite").objectReferenceValue = spRock;
        boardSO.FindProperty("_crackSprite").objectReferenceValue = spCrack;
        boardSO.FindProperty("_holeSprite").objectReferenceValue = spHole;
        boardSO.FindProperty("_redirectSprite").objectReferenceValue = spRedirect;
        boardSO.FindProperty("_frictionSprite").objectReferenceValue = spFriction;
        boardSO.FindProperty("_playerSprite").objectReferenceValue = spPlayer;
        boardSO.ApplyModifiedProperties();

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage text (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(350, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score text (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(350, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Move count text (top center-left)
        var moveText = CT(canvasObj.transform, "MoveCountText", "手数: 0", 28, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(220, 45), new Vector2(20, -65));
        moveText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.8f);

        // Remaining cells text
        var remainText = CT(canvasObj.transform, "RemainingText", "残り: 0", 28, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220, 45), new Vector2(-20, -65));
        remainText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        remainText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.8f);

        // Bottom buttons row (y=75)
        // Undo button (left)
        var undoBtn = CB(canvasObj.transform, "UndoButton", "↩ 戻す", 26, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 65), new Vector2(20, 75), new Color(0.3f, 0.35f, 0.5f, 0.9f));

        // Reset button (center-left)
        var resetBtn = CB(canvasObj.transform, "ResetButton", "↺ リセット", 26, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 65), new Vector2(-110, 75), new Color(0.4f, 0.3f, 0.3f, 0.9f));

        // Menu button (bottom right)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 24, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(180, 55), new Vector2(-20, 20), new Color(0.2f, 0.25f, 0.35f, 0.85f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f));
        var scTitle = CT(scPanel.transform, "SCTitle", "", 36, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 1f);
        var scScore = CT(scPanel.transform, "SCScore", "", 28, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 45), Vector2.zero);
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var scStars = CT(scPanel.transform, "SCStars", "★★★", 42, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 55), Vector2.zero);
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 30, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 65), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));
        scPanel.SetActive(false);

        // Game Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.12f, 0.18f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "全ステージクリア！", 40, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScore = CT(clearPanel.transform, "ClearScore", "", 30, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 50), Vector2.zero);
        clearScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var clearMenuBtn = CB(clearPanel.transform, "ClearMenuButton", "メニューへ戻る", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.05f, 0.05f, 0.95f));
        CT(goPanel.transform, "GOTitle", "行き詰まり！", 40, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 65), Vector2.zero, new Color(0.5f, 0.2f, 0.2f));
        goPanel.SetActive(false);

        // InstructionPanel overlay
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.02f, 0.05f, 0.15f, 0.97f));
        var ipRect = ipPanel.GetComponent<RectTransform>();
        ipRect.anchorMin = Vector2.zero; ipRect.anchorMax = Vector2.one;
        ipRect.offsetMin = ipRect.offsetMax = Vector2.zero;

        var ipTitle = CT(ipPanel.transform, "IPTitle", "", 42, jpFont,
            new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);
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
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(55, 55), new Vector2(-20, 155), new Color(0.3f, 0.3f, 0.5f, 0.8f));

        // InstructionPanel component
        var ipControllerObj = new GameObject("InstructionPanelController");
        ipControllerObj.transform.SetParent(gmObj.transform);
        var ip = ipControllerObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = helpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // IcePathUI
        var uiObj = new GameObject("IcePathUI");
        uiObj.transform.SetParent(gmObj.transform);
        var iceUI = uiObj.AddComponent<IcePathUI>();
        var uiSO = new SerializedObject(iceUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_remainingText").objectReferenceValue = remainText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearTitleText").objectReferenceValue = scTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_starsText").objectReferenceValue = scStars.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_boardManager").objectReferenceValue = board;
        gmSO.FindProperty("_ui").objectReferenceValue = iceUI;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(undoBtn.GetComponent<Button>().onClick, board.UndoMove);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, gm.OnRetryButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStageButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, gm.OnRetryButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/008v2_IcePath.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup008v2] IcePath シーンを作成しました: " + scenePath);
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
