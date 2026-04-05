using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game096_DualControl;

public static class Setup096_DualControl
{
    [MenuItem("Assets/Setup/096 DualControl")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup096] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game096_DualControl/";

        // カメラ設定
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        // スプライト読み込み
        Sprite bgSprite       = LoadSprite(sp + "background.png");
        Sprite playerSprite   = LoadSprite(sp + "player.png");
        Sprite obstacleSprite = LoadSprite(sp + "obstacle.png");
        Sprite tileSprite     = LoadSprite(sp + "tile.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DualControlGameManager>();

        // DualManager
        var dmObj = new GameObject("DualManager");
        dmObj.transform.SetParent(gmObj.transform);
        var dm = dmObj.AddComponent<DualManager>();

        // 列背景と中央仕切り
        if (tileSprite != null)
        {
            float[] leftColX  = { -2.4f, -1.6f, -0.8f };
            float[] rightColX = {  0.8f,  1.6f,  2.4f };
            Color leftBg  = new Color(0.2f, 0.4f, 0.8f, 0.12f);
            Color rightBg = new Color(0.8f, 0.4f, 0.1f, 0.12f);
            foreach (float x in leftColX)  CreateColBg(tileSprite, x, leftBg,  dmObj.transform);
            foreach (float x in rightColX) CreateColBg(tileSprite, x, rightBg, dmObj.transform);

            var div = new GameObject("Divider");
            div.transform.SetParent(dmObj.transform);
            div.transform.position = new Vector3(0f, 0f, 0.1f);
            div.transform.localScale = new Vector3(0.03f, 11f, 1f);
            var divSr = div.AddComponent<SpriteRenderer>();
            divSr.sprite = tileSprite;
            divSr.color = new Color(1f, 1f, 1f, 0.25f);
            divSr.sortingOrder = 0;
        }

        // DualManager の SerializedObject 配線
        var dmSO = new SerializedObject(dm);
        dmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        dmSO.FindProperty("_characterSprite").objectReferenceValue = playerSprite;
        dmSO.FindProperty("_obstacleSprite").objectReferenceValue = obstacleSprite;
        dmSO.FindProperty("_goalCount").intValue = 8;
        dmSO.FindProperty("_initialSpeed").floatValue = 2.5f;
        dmSO.FindProperty("_maxSpeed").floatValue = 5.0f;
        dmSO.FindProperty("_initialInterval").floatValue = 1.8f;
        dmSO.FindProperty("_minInterval").floatValue = 0.7f;
        dmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // タイマー（上部中央）
        var timerText = CT(canvasObj.transform, "TimerText", "00:00.0", 38, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(240, 50), new Vector2(0, -20));
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 左ステージカウンター（上部左）
        var stageLeftText = CT(canvasObj.transform, "StageLeftText", "左 0/8", 30, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(180, 40), new Vector2(20, -80));
        stageLeftText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.7f, 1.0f);

        // 右ステージカウンター（上部右）
        var stageRightText = CT(canvasObj.transform, "StageRightText", "右 0/8", 30, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(180, 40), new Vector2(-20, -80));
        stageRightText.GetComponent<TextMeshProUGUI>().color = new Color(1.0f, 0.6f, 0.2f);
        stageRightText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.06f, 0.14f, 0.18f, 0.95f));
        var clearScoreText = CT(clearPanel.transform, "CS", "", 38, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 120), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.06f, 0.06f, 0.95f));
        CT(goPanel.transform, "GT", "ぶつかった！", 42, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), Vector2.zero, new Color(0.5f, 0.15f, 0.1f));
        goPanel.SetActive(false);

        // DualControlUI
        var uiObj = new GameObject("DualControlUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DualControlUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageLeftText").objectReferenceValue = stageLeftText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageRightText").objectReferenceValue = stageRightText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager の SerializedObject 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_dualManager").objectReferenceValue = dm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // ボタンイベント登録
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/096_DualControl.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup096] DualControl シーンを作成しました: " + scenePath);
    }

    private static void CreateColBg(Sprite tileSp, float x, Color color, Transform parent)
    {
        var obj = new GameObject($"ColBg_{x:F1}");
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(x, 0f, 0.2f);
        obj.transform.localScale = new Vector3(0.7f, 11f, 1f);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = tileSp;
        sr.color = color;
        sr.sortingOrder = 0;
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
        r.anchorMin = new Vector2(0.1f, 0.3f);
        r.anchorMax = new Vector2(0.9f, 0.7f);
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
