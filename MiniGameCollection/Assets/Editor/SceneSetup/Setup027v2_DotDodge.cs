using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game027v2_DotDodge;

public static class Setup027v2_DotDodge
{
    [MenuItem("Assets/Setup/027v2 DotDodge")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup027v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game027v2_DotDodge/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Ensure sprites imported
        string[] spritePaths = {
            sp+"Background.png", sp+"Player.png",
            sp+"DotNormal.png", sp+"DotChaser.png", sp+"DotExpander.png", sp+"SafeZone.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "Background.png");
        Sprite spPlayer   = LoadSprite(sp + "Player.png");
        Sprite spNormal   = LoadSprite(sp + "DotNormal.png");
        Sprite spChaser   = LoadSprite(sp + "DotChaser.png");
        Sprite spExpander = LoadSprite(sp + "DotExpander.png");
        Sprite spSafeZone = LoadSprite(sp + "SafeZone.png");

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

        // Player
        var playerObj = new GameObject("Player");
        playerObj.transform.position = Vector3.zero;
        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = spPlayer;
        playerSr.sortingOrder = 10;
        float playerRadius = 0.25f;
        float playerSpriteSize = spPlayer != null ? spPlayer.rect.width / spPlayer.pixelsPerUnit : 0.5f;
        float playerScale = (playerRadius * 2f) / playerSpriteSize;
        playerObj.transform.localScale = new Vector3(playerScale, playerScale, 1f);

        var playerCtrl = playerObj.AddComponent<PlayerController>();

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DotDodgeGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // DotSpawner (child of GameManager)
        var spawnerObj = new GameObject("DotSpawner");
        spawnerObj.transform.SetParent(gmObj.transform);
        var spawner = spawnerObj.AddComponent<DotSpawner>();

        var spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("_gameManager").objectReferenceValue = gm;
        spawnerSO.FindProperty("_player").objectReferenceValue = playerCtrl;
        spawnerSO.FindProperty("_spriteNormal").objectReferenceValue = spNormal;
        spawnerSO.FindProperty("_spriteChaser").objectReferenceValue = spChaser;
        spawnerSO.FindProperty("_spriteExpander").objectReferenceValue = spExpander;
        spawnerSO.FindProperty("_spriteSafeZone").objectReferenceValue = spSafeZone;
        spawnerSO.ApplyModifiedProperties();

        // Wire PlayerController
        var playerSO = new SerializedObject(playerCtrl);
        playerSO.FindProperty("_gameManager").objectReferenceValue = gm;
        playerSO.FindProperty("_mainCamera").objectReferenceValue = camera;
        playerSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // HUD elements
        // Stage text - top left
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 55), new Vector2(15, -35));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        // Score text - top right
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(280, 50), new Vector2(-15, -35));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Time text - top center
        var timeText = CT(canvasObj.transform, "TimeText", "0.0s", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(200, 60), new Vector2(0, -32));
        timeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timeText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.8f);

        // Combo text - center
        var comboText = CT(canvasObj.transform, "ComboText", "", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, 200));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        comboText.SetActive(false);

        // Menu button - bottom left
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 55), new Vector2(15, 15), new Color(0.15f, 0.15f, 0.3f, 0.85f));

        // Re-show instruction button - bottom right
        var reShowBtn = CB(canvasObj.transform, "HelpButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(55, 55), new Vector2(-15, 15), new Color(0.2f, 0.35f, 0.6f, 0.85f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0f, 0f, 0f, 0.75f), new Vector2(700, 300));
        var scTitle = CT(scPanel.transform, "StageClearText", "Stage Clear!", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 80), new Vector2(0, 50));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);
        var scBonus = CT(scPanel.transform, "StageBonusText", "+500 Bonus!", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, -30));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        scPanel.SetActive(false);

        // Final Clear Panel
        var gcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0f, 0f, 0f, 0.88f), new Vector2(750, 480));
        CT(gcPanel.transform, "ClearTitle", "全ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(680, 80), new Vector2(0, 130)).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcScoreText = CT(gcPanel.transform, "FinalScoreText", "Score: 0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 65), new Vector2(0, 40));
        gcScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcTimeText = CT(gcPanel.transform, "FinalTimeText", "Time: 0.0s", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 55), new Vector2(0, -25));
        gcTimeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcMenuBtn = CB(gcPanel.transform, "ClearMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 65), new Vector2(0, -135), new Color(0.1f, 0.4f, 0.8f));
        gcPanel.SetActive(false);

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.88f), new Vector2(750, 500));
        CT(goPanel.transform, "GOTitle", "GAME OVER", 60, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(680, 90), new Vector2(0, 150)).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GOScoreText", "Score: 0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 65), new Vector2(0, 40));
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goTimeText = CT(goPanel.transform, "GOTimeText", "Time: 0.0s", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 55), new Vector2(0, -25));
        goTimeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 65), new Vector2(-145, -130), new Color(0.6f, 0.1f, 0.1f));
        var goMenuBtn2 = CB(goPanel.transform, "GOMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 65), new Vector2(145, -130), new Color(0.2f, 0.2f, 0.4f));
        goPanel.SetActive(false);

        // InstructionPanel (overlay canvas)
        var ipCanvasObj = new GameObject("InstructionPanelCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        ipCanvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipCanvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
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

        var ipTitle = CT(ipPanel.transform, "IPTitle", "DotDodge", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 250));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.6f, 1f);

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
            new Vector2(320, 80), new Vector2(0, -270), new Color(0.1f, 0.4f, 0.8f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // DotDodgeUI (child of GameManager)
        var uiObj = new GameObject("DotDodgeUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ddUI = uiObj.AddComponent<DotDodgeUI>();
        var uiSO = new SerializedObject(ddUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageBonusText").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearTimeText").objectReferenceValue = gcTimeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverTimeText").objectReferenceValue = goTimeText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_dotSpawner").objectReferenceValue = spawner;
        gmSO.FindProperty("_ui").objectReferenceValue = ddUI;
        gmSO.FindProperty("_playerController").objectReferenceValue = playerCtrl;
        gmSO.ApplyModifiedProperties();

        // Button events
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(goMenuBtn2.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");

        // Initialize UI
        ddUI.Initialize(gm);

        // Save Scene
        string scenePath = "Assets/Scenes/027v2_DotDodge.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup027v2] Scene created: " + scenePath);
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
            else Debug.LogWarning($"[Setup027v2] Sprite not found: {path}");
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
