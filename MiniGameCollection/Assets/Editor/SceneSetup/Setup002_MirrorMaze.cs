using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game002_MirrorMaze;

/// <summary>
/// MirrorMaze のゲームシーンを自動構成する Editor スクリプト。
/// Assets > Setup > 002 MirrorMaze から実行する。
/// </summary>
public static class Setup002_MirrorMaze
{
    private const int   GRID_W    = 7;
    private const int   GRID_H    = 7;
    private const float CELL_SIZE = 1.0f;
    private static readonly Vector2 GRID_ORIGIN = new Vector2(-3f, -3f);

    [MenuItem("Assets/Setup/002 MirrorMaze")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup002_MirrorMaze] Play モード中は実行できません。");
            return;
        }

        var scene  = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- Camera ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor  = new Color(0.05f, 0.05f, 0.12f);
            camera.orthographic     = true;
            camera.orthographicSize = 6f;
        }

        // --- Sprites ---
        var whiteSprite = GetOrCreateWhiteSprite("Assets/Scripts/Game002_MirrorMaze/WhiteSquare.png");

        Directory.CreateDirectory("Assets/Resources/Sprites/Game002_MirrorMaze");
        var mirrorSlash     = CreateMirrorSprite(true,  "Assets/Resources/Sprites/Game002_MirrorMaze/mirror_slash.png");
        var mirrorBackslash = CreateMirrorSprite(false, "Assets/Resources/Sprites/Game002_MirrorMaze/mirror_backslash.png");

        // --- Grid cells (7x7) ---
        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                var cell = new GameObject("Cell_" + x + "_" + y);
                var sr   = cell.AddComponent<SpriteRenderer>();
                sr.sprite       = whiteSprite;
                sr.color        = new Color(0.1f, 0.12f, 0.2f);
                sr.sortingOrder = -10;
                cell.transform.position   = GridToWorld(x, y);
                cell.transform.localScale = Vector3.one * (CELL_SIZE * 0.93f);
            }
        }

        // --- Grid lines ---
        for (int i = 0; i <= GRID_W; i++)
        {
            float xPos = GRID_ORIGIN.x - CELL_SIZE * 0.5f + i * CELL_SIZE;
            var v  = new GameObject("VLine_" + i);
            var sr = v.AddComponent<SpriteRenderer>();
            sr.sprite       = whiteSprite;
            sr.color        = new Color(0.2f, 0.24f, 0.35f, 0.5f);
            sr.sortingOrder = -5;
            v.transform.position   = new Vector3(xPos, 0f, 0f);
            v.transform.localScale = new Vector3(0.03f, 8f, 1f);
        }
        for (int j = 0; j <= GRID_H; j++)
        {
            float yPos = GRID_ORIGIN.y - CELL_SIZE * 0.5f + j * CELL_SIZE;
            var h  = new GameObject("HLine_" + j);
            var sr = h.AddComponent<SpriteRenderer>();
            sr.sprite       = whiteSprite;
            sr.color        = new Color(0.2f, 0.24f, 0.35f, 0.5f);
            sr.sortingOrder = -5;
            h.transform.position   = new Vector3(0f, yPos, 0f);
            h.transform.localScale = new Vector3(8f, 0.03f, 1f);
        }

        // --- Emitter indicator (orange square) at grid(-1,3) = world(-4, 0) ---
        var emitterGo = new GameObject("Emitter_Right_Row3");
        var emSr      = emitterGo.AddComponent<SpriteRenderer>();
        emSr.sprite       = whiteSprite;
        emSr.color        = new Color(1f, 0.55f, 0.1f);
        emSr.sortingOrder = 1;
        emitterGo.transform.position   = GridToWorld(-1, 3);
        emitterGo.transform.localScale = Vector3.one * 0.8f;

        // --- Receiver indicator (red → green when hit) at grid(3,7) = world(0, 4) ---
        var receiverGo = new GameObject("Receiver_Top_Col3");
        var recSr      = receiverGo.AddComponent<SpriteRenderer>();
        recSr.sprite       = whiteSprite;
        recSr.color        = new Color(1f, 0.3f, 0.3f);
        recSr.sortingOrder = 1;
        receiverGo.transform.position   = GridToWorld(3, 7);
        receiverGo.transform.localScale = Vector3.one * 0.8f;

        // --- GameManager ---
        var gmGo = new GameObject("GameManager");
        var gm   = gmGo.AddComponent<MirrorMazeGameManager>();

        // --- LaserManager ---
        var lmGo = new GameObject("LaserManager");
        var lm   = lmGo.AddComponent<LaserManager>();

        var lmSO = new SerializedObject(lm);
        lmSO.FindProperty("_gameManager").objectReferenceValue           = gm;
        lmSO.FindProperty("_mirrorSlashSprite").objectReferenceValue     = mirrorSlash;
        lmSO.FindProperty("_mirrorBackslashSprite").objectReferenceValue = mirrorBackslash;
        var recArr = lmSO.FindProperty("_receiverRenderers");
        recArr.arraySize = 1;
        recArr.GetArrayElementAtIndex(0).objectReferenceValue = recSr;
        lmSO.ApplyModifiedProperties();

        // --- Canvas ---
        var canvasGo = new GameObject("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var titleText = CreateText(canvasGo.transform, "TitleText", "MirrorMaze", 36, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 50), new Vector2(0, -20));
        titleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var hintText = CreateText(canvasGo.transform, "HintText",
            "クリックで鏡を配置（/ → \\ → 消去）", 20, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(600, 40), new Vector2(0, 20));
        var hintTmp = hintText.GetComponent<TextMeshProUGUI>();
        hintTmp.alignment = TextAlignmentOptions.Center;
        hintTmp.color     = new Color(0.7f, 0.7f, 0.8f);

        var menuBtn = CreateButton(canvasGo.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // --- Clear panel ---
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        var cp = clearPanel.GetComponent<RectTransform>();
        cp.anchorMin = new Vector2(0.2f, 0.25f);
        cp.anchorMax = new Vector2(0.8f, 0.75f);
        cp.offsetMin = cp.offsetMax = Vector2.zero;

        var clearTitle = CreateText(clearPanel.transform, "ClearTitle", "クリア！", 64, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 100), Vector2.zero);
        var ct = clearTitle.GetComponent<TextMeshProUGUI>();
        ct.alignment = TextAlignmentOptions.Center;
        ct.color     = new Color(0.3f, 1f, 0.5f);

        var restartBtn = CreateButton(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f, 0.2f), new Vector2(0.3f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero, new Color(0.1f, 0.5f, 0.3f, 1f));

        var clearMenuBtn = CreateButton(clearPanel.transform, "ClearMenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.7f, 0.2f), new Vector2(0.7f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero, new Color(0.3f, 0.3f, 0.5f, 1f));

        clearPanel.SetActive(false);

        // --- MirrorMazeUI ---
        var uiGo = new GameObject("MirrorMazeUI");
        var ui   = uiGo.AddComponent<MirrorMazeUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_clearPanel").objectReferenceValue    = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue     = clearTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue    = clearMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue   = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        // --- GameManager refs ---
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_laserManager").objectReferenceValue = lm;
        gmSO.FindProperty("_ui").objectReferenceValue           = ui;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/002_MirrorMaze.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup002_MirrorMaze] シーンを作成しました: " + scenePath);
    }

    private static Sprite GetOrCreateWhiteSprite(string path)
    {
        if (!File.Exists(path))
        {
            var tex = new Texture2D(4, 4);
            var px  = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType        = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = 1;
                imp.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite CreateMirrorSprite(bool isSlash, string path)
    {
        int size = 64;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = isSlash
                    ? Mathf.Abs(x - y)
                    : Mathf.Abs(x + y - (size - 1));
                tex.SetPixel(x, y, dist <= 4 ? Color.white : Color.clear);
            }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType        = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = size;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Vector3 GridToWorld(int gx, int gy)
        => new Vector3(GRID_ORIGIN.x + gx * CELL_SIZE, GRID_ORIGIN.y + gy * CELL_SIZE, 0f);

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 pos)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = Color.white;
        if (font != null) tmp.font = font;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin       = anchorMin;
        r.anchorMax       = anchorMax;
        r.pivot           = pivot;
        r.sizeDelta       = sizeDelta;
        r.anchoredPosition = pos;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 pos, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<Button>();
        var r = go.GetComponent<RectTransform>();
        r.anchorMin       = anchorMin;
        r.anchorMax       = anchorMax;
        r.pivot           = pivot;
        r.sizeDelta       = sizeDelta;
        r.anchoredPosition = pos;

        var t  = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(go.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        return go;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list) if (s.path == scenePath) return;
        list.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
