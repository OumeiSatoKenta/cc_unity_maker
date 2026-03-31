using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game007_NumberFlow;

public static class Setup007_NumberFlow
{
    [MenuItem("Assets/Setup/007 NumberFlow")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup007_NumberFlow] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.12f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
        }

        string whiteTexPath = "Assets/Scripts/Game007_NumberFlow/WhiteSquare.png";
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

        string sp = "Assets/Resources/Sprites/Game007_NumberFlow/";
        var normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "cell_normal.png");

        // Cell prefab
        string pd = "Assets/Scripts/Game007_NumberFlow/";
        var cellObj = new GameObject("CellPrefab");
        var csr = cellObj.AddComponent<SpriteRenderer>();
        csr.sprite = normalSprite != null ? normalSprite : whiteSprite;
        csr.sortingOrder = 5;
        var col = cellObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        cellObj.AddComponent<NumberCell>();

        var textObj = new GameObject("NumberText");
        textObj.transform.SetParent(cellObj.transform, false);
        var tmp3d = textObj.AddComponent<TextMeshPro>();
        tmp3d.text = ""; tmp3d.fontSize = 6; tmp3d.color = Color.white;
        tmp3d.alignment = TextAlignmentOptions.Center; tmp3d.sortingOrder = 10;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 1f);

        var cellPrefab = PrefabUtility.SaveAsPrefabAsset(cellObj, pd + "CellPrefab.prefab");
        Object.DestroyImmediate(cellObj);

        // GameManager + NumberFlowManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<NumberFlowGameManager>();
        var boardObj = new GameObject("NumberBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var fm = boardObj.AddComponent<NumberFlowManager>();

        var fmSO = new SerializedObject(fm);
        fmSO.FindProperty("_gridSize").intValue = 4;
        fmSO.FindProperty("_cellSize").floatValue = 1.0f;
        fmSO.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
        fmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var progressText = CreateText(canvasObj.transform, "ProgressText", "", 32, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,50), new Vector2(20,-20));
        var stageText = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtn = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20),
            new Color(0.3f,0.3f,0.4f,0.9f));
        var resetBtn = CreateButton(canvasObj.transform, "ResetButton", "リセット", 24, jpFont,
            new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(160,50), new Vector2(20,20),
            new Color(0.5f,0.3f,0.2f,0.9f));

        // Clear panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CreateText(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CreateButton(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero,
            new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CreateButton(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero,
            new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        // NumberFlowUI
        var uiObj = new GameObject("NumberFlowUI");
        var nfUI = uiObj.AddComponent<NumberFlowUI>();
        var uiSO = new SerializedObject(nfUI);
        uiSO.FindProperty("_progressText").objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        // Reset button → RestartGame
        // Note: ResetButton uses BackToMenuButton pattern but calls RestartGame
        // We wire it via NumberFlowUI's restart
        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_flowManager").objectReferenceValue = fm;
        gmSO.FindProperty("_ui").objectReferenceValue = nfUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/007_NumberFlow.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup007_NumberFlow] NumberFlow シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = Color.white;
        if (font != null) tmp.font = font;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = bgColor;
        obj.AddComponent<Button>();
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        var tObj = new GameObject("Text", typeof(RectTransform));
        tObj.transform.SetParent(obj.transform, false);
        var tmp = tObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var tr = tObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return obj;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
