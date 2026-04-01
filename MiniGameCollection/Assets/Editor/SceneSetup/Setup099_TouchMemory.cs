using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game099_TouchMemory;

public static class Setup099_TouchMemory
{
    [MenuItem("Assets/Setup/099 TouchMemory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup099] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game099_TouchMemory/";

        // カメラ設定
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.08f, 0.08f, 0.15f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite panelSprite = LoadSprite(sp + "panel.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<TouchMemoryGameManager>();

        // MemoryManager
        var mmObj = new GameObject("MemoryManager");
        mmObj.transform.SetParent(gmObj.transform);
        var mm = mmObj.AddComponent<MemoryManager>();

        var mmSO = new SerializedObject(mm);
        mmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        mmSO.FindProperty("_panelSprite").objectReferenceValue = panelSprite;
        mmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ラウンドテキスト（上部中央）
        var roundText = CT(canvasObj.transform, "RoundText", "ラウンド 1 / 10", 38, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(400, 50), new Vector2(0, -20));
        roundText.GetComponent<TextMeshProUGUI>().color = Color.white;
        roundText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ステータステキスト（中央上）
        var statusText = CT(canvasObj.transform, "StatusText", "見て覚えて！", 32, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 50), Vector2.zero);
        statusText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);
        statusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.05f, 0.15f, 0.1f, 0.95f));
        var clearText = CT(clearPanel.transform, "ClearText", "", 36, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 30, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f));
        var goText = CT(goPanel.transform, "GOText", "", 36, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 30, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero, new Color(0.5f, 0.15f, 0.1f));
        goPanel.SetActive(false);

        // TouchMemoryUI
        var uiObj = new GameObject("TouchMemoryUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<TouchMemoryUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_roundText").objectReferenceValue = roundText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // GameManager の配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_memoryManager").objectReferenceValue = mm;
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

        string scenePath = "Assets/Scenes/099_TouchMemory.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup099] TouchMemory シーンを作成しました: " + scenePath);
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
