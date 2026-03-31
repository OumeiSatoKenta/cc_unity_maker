using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game017_CrystalSort;

public static class Setup017_CrystalSort
{
    [MenuItem("Assets/Setup/017 CrystalSort")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup017_CrystalSort] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.13f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        string whiteTexPath = "Assets/Scripts/Game017_CrystalSort/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px); wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        string sp = "Assets/Resources/Sprites/Game017_CrystalSort/";
        var crystalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "crystal_red.png");
        var bottleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "bottle.png");

        string pd = "Assets/Scripts/Game017_CrystalSort/";

        // Crystal prefab
        var cObj = new GameObject("CrystalPrefab");
        var csr = cObj.AddComponent<SpriteRenderer>();
        csr.sprite = crystalSprite != null ? crystalSprite : whiteSprite;
        csr.sortingOrder = 8;
        cObj.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
        cObj.AddComponent<CrystalItem>();
        var crystalPrefab = PrefabUtility.SaveAsPrefabAsset(cObj, pd + "CrystalPrefab.prefab");
        Object.DestroyImmediate(cObj);

        // Bottle prefab
        var bObj = new GameObject("BottlePrefab");
        var bsr = bObj.AddComponent<SpriteRenderer>();
        bsr.sprite = bottleSprite != null ? bottleSprite : whiteSprite;
        bsr.sortingOrder = 3;
        bObj.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.5f);
        bObj.AddComponent<Bottle>();
        var textChild = new GameObject("CountText");
        textChild.transform.SetParent(bObj.transform, false);
        var tmp3d = textChild.AddComponent<TextMeshPro>();
        tmp3d.text = "0/3"; tmp3d.fontSize = 4; tmp3d.color = Color.white;
        tmp3d.alignment = TextAlignmentOptions.Center; tmp3d.sortingOrder = 10;
        textChild.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.5f);
        textChild.transform.localPosition = new Vector3(0, -1.2f, 0);
        var bottlePrefab = PrefabUtility.SaveAsPrefabAsset(bObj, pd + "BottlePrefab.prefab");
        Object.DestroyImmediate(bObj);

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CrystalSortGameManager>();
        var boardObj = new GameObject("SortBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var sm = boardObj.AddComponent<SortManager>();

        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_crystalPrefab").objectReferenceValue = crystalPrefab;
        smSO.FindProperty("_bottlePrefab").objectReferenceValue = bottlePrefab;
        smSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var sortedText = CT(canvasObj.transform, "SortedText", "分類: 0", 28, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-20));
        var missText = CT(canvasObj.transform, "MissText", "ミス: 0", 28, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-60));
        var stageText = CT(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var hint = CT(canvasObj.transform, "HintText", "クリスタルをタップ→同じ色の瓶をタップ", 20, jpFont,
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(600,40), new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        hint.GetComponent<TextMeshProUGUI>().color = new Color(0.5f,0.5f,0.6f);
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20),
            new Color(0.3f,0.3f,0.4f,0.9f));

        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CT(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CB(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero,
            new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero,
            new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("CrystalSortUI");
        var csUI = uiObj.AddComponent<CrystalSortUI>();
        var uiSO = new SerializedObject(csUI);
        uiSO.FindProperty("_sortedText").objectReferenceValue = sortedText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missText").objectReferenceValue = missText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_sortManager").objectReferenceValue = sm;
        gmSO.FindProperty("_ui").objectReferenceValue = csUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/017_CrystalSort.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup017_CrystalSort] CrystalSort シーンを作成しました: " + scenePath);
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }

    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
