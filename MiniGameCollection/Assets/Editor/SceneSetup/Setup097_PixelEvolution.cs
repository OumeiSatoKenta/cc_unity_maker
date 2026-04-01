using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game097_PixelEvolution;

public static class Setup097_PixelEvolution
{
    [MenuItem("Assets/Setup/097 PixelEvolution")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup097] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game097_PixelEvolution/";

        // カメラ設定
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.02f, 0.03f, 0.08f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<PixelEvolutionGameManager>();

        // EvolutionManager
        var emObj = new GameObject("EvolutionManager");
        emObj.transform.SetParent(gmObj.transform);
        var em = emObj.AddComponent<EvolutionManager>();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 世代テキスト（上部中央）
        var genText = CT(canvasObj.transform, "GenerationText", "世代 1 / 10", 42, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(400, 60), new Vector2(0, -30));
        genText.GetComponent<TextMeshProUGUI>().color = Color.white;
        genText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 生命体名テキスト（中央やや下）
        var nameText = CT(canvasObj.transform, "CreatureNameText", "原始ピクセル", 34, jpFont,
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 50), Vector2.zero);
        nameText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);
        nameText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 世代交代ボタン（下部中央）
        var evolveBtn = CB(canvasObj.transform, "EvolveButton", "世代交代", 34, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), Vector2.zero, new Color(0.2f, 0.4f, 0.6f, 0.95f));

        // 分岐パネル
        var branchPanel = CreatePanel(canvasObj.transform, "BranchPanel", new Color(0.08f, 0.1f, 0.2f, 0.95f));
        CT(branchPanel.transform, "BranchTitle", "進化の分岐点！", 36, jpFont,
            new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 50), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var optABtn = CB(branchPanel.transform, "OptionAButton", "選択肢A", 30, jpFont,
            new Vector2(0.25f, 0.35f), new Vector2(0.25f, 0.35f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 70), Vector2.zero, new Color(0.2f, 0.5f, 0.7f, 0.95f));
        var optAText = optABtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        var optBBtn = CB(branchPanel.transform, "OptionBButton", "選択肢B", 30, jpFont,
            new Vector2(0.75f, 0.35f), new Vector2(0.75f, 0.35f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 70), Vector2.zero, new Color(0.5f, 0.3f, 0.2f, 0.95f));
        var optBText = optBBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        branchPanel.SetActive(false);

        // クリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.05f, 0.1f, 0.15f, 0.95f));
        var clearText = CT(clearPanel.transform, "ClearText", "", 34, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 200), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 30, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // メニューボタン（左下）
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // PixelEvolutionUI
        var uiObj = new GameObject("PixelEvolutionUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<PixelEvolutionUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_generationText").objectReferenceValue = genText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_creatureNameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_evolveButton").objectReferenceValue = evolveBtn.GetComponent<Button>();
        uiSO.FindProperty("_branchPanel").objectReferenceValue = branchPanel;
        uiSO.FindProperty("_branchOptionAButton").objectReferenceValue = optABtn.GetComponent<Button>();
        uiSO.FindProperty("_branchOptionBButton").objectReferenceValue = optBBtn.GetComponent<Button>();
        uiSO.FindProperty("_branchOptionAText").objectReferenceValue = optAText;
        uiSO.FindProperty("_branchOptionBText").objectReferenceValue = optBText;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // GameManager の SerializedObject 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_evolutionManager").objectReferenceValue = em;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // ボタンイベント登録
        UnityEditor.Events.UnityEventTools.AddPersistentListener(evolveBtn.GetComponent<Button>().onClick, gm.OnEvolveButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(optABtn.GetComponent<Button>().onClick, gm.OnBranchSelectedA);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(optBBtn.GetComponent<Button>().onClick, gm.OnBranchSelectedB);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/097_PixelEvolution.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup097] PixelEvolution シーンを作成しました: " + scenePath);
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
