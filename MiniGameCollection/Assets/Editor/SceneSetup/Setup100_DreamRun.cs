using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game100_DreamRun;

public static class Setup100_DreamRun
{
    [MenuItem("Assets/Setup/100 DreamRun")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup100] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game100_DreamRun/";

        // カメラ設定
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.3f, 0.15f, 0.5f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite charSprite = LoadSprite(sp + "character.png");
        Sprite obsSprite = LoadSprite(sp + "obstacle.png");
        Sprite orbSprite = LoadSprite(sp + "orb.png");
        Sprite groundSprite = LoadSprite(sp + "ground.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        // 地面ライン（3レーン）
        float[] laneY = { 2f, 0f, -2f };
        foreach (float y in laneY)
        {
            var line = new GameObject($"GroundLine_{y}");
            line.transform.position = new Vector3(0f, y - 0.7f, 0.1f);
            line.transform.localScale = new Vector3(20f, 0.3f, 1f);
            var sr = line.AddComponent<SpriteRenderer>();
            sr.sprite = groundSprite;
            sr.color = new Color(1f, 1f, 1f, 0.15f);
            sr.sortingOrder = 0;
        }

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DreamRunGameManager>();

        // RunManager
        var rmObj = new GameObject("RunManager");
        rmObj.transform.SetParent(gmObj.transform);
        var rm = rmObj.AddComponent<RunManager>();

        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        rmSO.FindProperty("_characterSprite").objectReferenceValue = charSprite;
        rmSO.FindProperty("_obstacleSprite").objectReferenceValue = obsSprite;
        rmSO.FindProperty("_orbSprite").objectReferenceValue = orbSprite;
        rmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 距離テキスト（上部左）
        var distText = CT(canvasObj.transform, "DistanceText", "0m", 34, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(200, 45), new Vector2(20, -20));
        distText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 1f);

        // 断片テキスト（上部右）
        var fragText = CT(canvasObj.transform, "FragmentText", "断片 0/5", 30, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(200, 40), new Vector2(-20, -25));
        fragText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);
        fragText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // ライフテキスト（上部中央）
        var lifeText = CT(canvasObj.transform, "LifeText", "♥ ♥ ♥", 36, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(250, 50), new Vector2(0, -20));
        lifeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);
        lifeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ストーリーテキスト（中央）
        var storyText = CT(canvasObj.transform, "StoryText", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, 100));
        storyText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 0.8f);
        storyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        storyText.SetActive(false);

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.1f, 0.05f, 0.2f, 0.95f));
        var clearText = CT(clearPanel.transform, "ClearText", "", 34, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 180), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度夢を見る", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 60), Vector2.zero, new Color(0.3f, 0.2f, 0.5f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.1f, 0.95f));
        var goText = CT(goPanel.transform, "GOText", "", 34, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 180), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 30, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero, new Color(0.5f, 0.15f, 0.1f));
        goPanel.SetActive(false);

        // DreamRunUI
        var uiObj = new GameObject("DreamRunUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DreamRunUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_distanceText").objectReferenceValue = distText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_fragmentText").objectReferenceValue = fragText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_lifeText").objectReferenceValue = lifeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_storyText").objectReferenceValue = storyText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // GameManager の配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_runManager").objectReferenceValue = rm;
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

        string scenePath = "Assets/Scenes/100_DreamRun.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup100] DreamRun シーンを作成しました: " + scenePath);
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
