using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game003_GravitySwitch;

public static class Setup003_GravitySwitch
{
    [MenuItem("Assets/Setup/003 GravitySwitch")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup003] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game003_GravitySwitch/";

        // カメラ
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.03f, 0.05f, 0.12f); camera.orthographic = true; camera.orthographicSize = 6f; }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite wallSprite = LoadSprite(sp + "wall.png");
        Sprite ballSprite = LoadSprite(sp + "ball.png");
        Sprite goalSprite = LoadSprite(sp + "goal.png");
        Sprite holeSprite = LoadSprite(sp + "hole.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.025f, 0.025f, 1f) : new Vector3(14f, 14f, 1f);

        // プレハブ作成
        string prefabDir = "Assets/Resources/Sprites/Game003_GravitySwitch/";
        var wallPrefab = CreateSpritePrefab(prefabDir + "WallPrefab.prefab", "WallPrefab", wallSprite, new Color(0.2f, 0.5f, 0.8f), 5);
        var goalPrefab = CreateSpritePrefab(prefabDir + "GoalPrefab.prefab", "GoalPrefab", goalSprite, new Color(0.2f, 0.9f, 0.4f), 3);
        var holePrefab = CreateSpritePrefab(prefabDir + "HolePrefab.prefab", "HolePrefab", holeSprite, new Color(0.4f, 0.1f, 0.1f), 2);

        // ボールプレハブ（BallController付き）
        var ballTmp = new GameObject("BallPrefab");
        var ballSr = ballTmp.AddComponent<SpriteRenderer>();
        ballSr.sprite = ballSprite;
        ballSr.color = Color.white;
        ballSr.sortingOrder = 10;
        ballTmp.AddComponent<BallController>();
        var ballPrefab = PrefabUtility.SaveAsPrefabAsset(ballTmp, prefabDir + "BallPrefabV2.prefab");
        Object.DestroyImmediate(ballTmp);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GravitySwitchGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // GravityManager
        var gravObj = new GameObject("GravityManager");
        gravObj.transform.SetParent(gmObj.transform);
        var gravMgr = gravObj.AddComponent<GravityManager>();

        // GravityManager 配線
        var gravSO = new SerializedObject(gravMgr);
        gravSO.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
        gravSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        gravSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        gravSO.FindProperty("_holePrefab").objectReferenceValue = holePrefab;
        gravSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD - ステージ（上部中央）
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 40), new Vector2(0, -15));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // スコア（上部右）
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 26, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 40), new Vector2(-15, -55));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        // 手数（上部左）
        var moveText = CT(canvasObj.transform, "MoveText", "手数: 0", 26, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(200, 40), new Vector2(15, -55));
        moveText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.9f);

        // 方向ボタン（十字配置、下部エリア）
        var upBtn = CB(canvasObj.transform, "UpButton", "↑", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(110, 90), new Vector2(0, 270),
            new Color(0.15f, 0.45f, 0.75f));

        var downBtn = CB(canvasObj.transform, "DownButton", "↓", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(110, 90), new Vector2(0, 75),
            new Color(0.15f, 0.45f, 0.75f));

        var leftBtn = CB(canvasObj.transform, "LeftButton", "←", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(110, 90), new Vector2(-130, 170),
            new Color(0.15f, 0.45f, 0.75f));

        var rightBtn = CB(canvasObj.transform, "RightButton", "→", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(110, 90), new Vector2(130, 170),
            new Color(0.15f, 0.45f, 0.75f));

        // リセットボタン（左下）
        var resetBtn = CB(canvasObj.transform, "ResetButton", "リセット", 22, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(160, 55), new Vector2(20, 15),
            new Color(0.3f, 0.3f, 0.5f, 0.9f));

        // メニューボタン（右下）
        var menuBtn2 = CB(canvasObj.transform, "MenuButton2", "メニュー", 22, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(160, 55), new Vector2(-20, 15),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // ステージクリアパネル
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f));
        var scText = CT(scPanel.transform, "SCText", "", 34, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 80), Vector2.zero);
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 30, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 65), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));
        scPanel.SetActive(false);

        // ゲームクリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.12f, 0.18f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "全ステージクリア！", 38, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScore", "", 30, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 50), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);
        var clearMenuBtn = CB(clearPanel.transform, "ClearMenuButton", "メニューへ戻る", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f));
        CT(goPanel.transform, "GOText", "ゲームオーバー", 38, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 30, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), Vector2.zero, new Color(0.5f, 0.15f, 0.1f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 55), Vector2.zero, new Color(0.3f, 0.3f, 0.4f));
        goPanel.SetActive(false);

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
            new Vector2(600, 50), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipPanel.transform, "IPControls", "", 24, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 24, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "StartButton", "はじめる", 34, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 70), Vector2.zero, new Color(0.1f, 0.5f, 0.8f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 28, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(55, 55), new Vector2(-20, 80), new Color(0.3f, 0.3f, 0.5f, 0.8f));

        // InstructionPanel コンポーネント
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

        // GravitySwitchUI
        var uiObj = new GameObject("GravitySwitchUI");
        uiObj.transform.SetParent(gmObj.transform);
        var gsUI = uiObj.AddComponent<GravitySwitchUI>();
        var uiSO = new SerializedObject(gsUI);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_moveText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton2").objectReferenceValue = menuBtn2.GetComponent<Button>();
        uiSO.FindProperty("_resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        uiSO.FindProperty("_upButton").objectReferenceValue = upBtn.GetComponent<Button>();
        uiSO.FindProperty("_downButton").objectReferenceValue = downBtn.GetComponent<Button>();
        uiSO.FindProperty("_leftButton").objectReferenceValue = leftBtn.GetComponent<Button>();
        uiSO.FindProperty("_rightButton").objectReferenceValue = rightBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_gravityManager").objectReferenceValue = gravMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = gsUI;
        gmSO.ApplyModifiedProperties();

        // ボタンイベント
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStageButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, gm.RestartStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn2.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, gm.RestartStage);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/003_GravitySwitch.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup003] GravitySwitch シーンを作成しました: " + scenePath);
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

    private static GameObject CreateSpritePrefab(string path, string name, Sprite sprite, Color fallbackColor, int sortOrder)
    {
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = sprite != null ? Color.white : fallbackColor;
        sr.sortingOrder = sortOrder;
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
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
