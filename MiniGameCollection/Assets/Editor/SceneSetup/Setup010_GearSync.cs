using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game010_GearSync;

public class Setup010_GearSync
{
    [MenuItem("Assets/Setup/010 GearSync")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Exit Play Mode before running setup.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──────────────────────────────────────────────────
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string sprDir = "Assets/Resources/Sprites/Game010_GearSync";
        Directory.CreateDirectory(sprDir);

        Sprite sprGear      = GetOrCreateSprite(sprDir + "/gear.png",       MakeGearTex());
        Sprite sprArrow     = GetOrCreateSprite(sprDir + "/arrow.png",      MakeArrowTex());
        Sprite sprPanelBg   = GetOrCreateSprite(sprDir + "/panel_bg.png",   MakePanelBgTex());
        Sprite sprButtonBg  = GetOrCreateSprite(sprDir + "/button_bg.png",  MakeButtonBgTex());

        // ── Canvas ───────────────────────────────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Header ───────────────────────────────────────────────────
        var levelTextGo = MakeText(canvasGo.transform, "LevelText", "Level 1 / 5", 34, Color.white);
        SetRT(levelTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -45f), new Vector2(400f, 60f));

        var menuHeaderBtn = MakeButton(canvasGo.transform, "MenuHeaderButton", "MENU",
            new Color(0.25f, 0.25f, 0.30f), sprButtonBg);
        SetRT(menuHeaderBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(160f, 60f));

        // ── Rotation count ────────────────────────────────────────────
        var rotTextGo = MakeText(canvasGo.transform, "RotationText", "Rotations: 0", 30,
            new Color(0.9f, 0.85f, 0.5f));
        SetRT(rotTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -105f), new Vector2(400f, 50f));

        // ── Instruction label ─────────────────────────────────────────
        var instrTextGo = MakeText(canvasGo.transform, "InstructionText",
            "Tap gears to rotate. Match all arrows!", 26,
            new Color(0.7f, 0.7f, 0.8f));
        SetRT(instrTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -158f), new Vector2(600f, 50f));

        // ── Clear Panel ──────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var clearPanelRt = clearPanel.GetComponent<RectTransform>();
        clearPanelRt.anchorMin = Vector2.zero;
        clearPanelRt.anchorMax = Vector2.one;
        clearPanelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "GEARS SYNCED!", 55,
            new Color(0.4f, 0.9f, 0.5f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(580f, 80f));

        var clearRotText = MakeText(clearPanel.transform, "ClearRotationText", "Rotations: 0", 40,
            new Color(1f, 0.9f, 0.3f));
        SetRT(clearRotText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(400f, 60f));

        var restartBtn = MakeButton(clearPanel.transform, "RestartButton", "RETRY",
            new Color(0.2f, 0.55f, 0.35f), sprButtonBg);
        SetRT(restartBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -40f), new Vector2(220f, 70f));

        var nextBtn = MakeButton(clearPanel.transform, "NextButton", "NEXT LEVEL",
            new Color(0.15f, 0.45f, 0.80f), sprButtonBg);
        SetRT(nextBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -40f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(clearPanel.transform, "MenuButton", "MENU",
            new Color(0.35f, 0.35f, 0.35f), sprButtonBg);
        SetRT(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(200f, 60f));

        clearPanel.SetActive(false);

        // ── GameManager ───────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<GearSyncGameManager>();

        // ── GearManager ───────────────────────────────────────────────
        var gearMgrGo = new GameObject("GearManager");
        var gearManager = gearMgrGo.AddComponent<GearManager>();

        // スプライトを SerializedObject 経由でセット
        {
            var so = new SerializedObject(gearManager);
            so.FindProperty("_gearSprite").objectReferenceValue  = sprGear;
            so.FindProperty("_arrowSprite").objectReferenceValue = sprArrow;
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── GearSyncUI ────────────────────────────────────────────────
        var uiGo = new GameObject("GearSyncUI");
        var ui = uiGo.AddComponent<GearSyncUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue        = levelTextGo.GetComponent<Text>();
            so.FindProperty("_rotationText").objectReferenceValue     = rotTextGo.GetComponent<Text>();
            so.FindProperty("_clearPanel").objectReferenceValue       = clearPanel;
            so.FindProperty("_clearRotationText").objectReferenceValue = clearRotText.GetComponent<Text>();
            so.FindProperty("_gameManager").objectReferenceValue      = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire GameManager ──────────────────────────────────────────
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_gearManager").objectReferenceValue = gearManager;
            so.FindProperty("_ui").objectReferenceValue          = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire buttons ──────────────────────────────────────────────
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            restartBtn.GetComponent<Button>().onClick, ui.OnRestartClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick, ui.OnNextClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick, ui.OnMenuClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuHeaderBtn.GetComponent<Button>().onClick, gameManager.LoadMenu);

        // ── EventSystem ──────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ─────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/010_GearSync.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup010] GearSync scene saved to " + scenePath);
    }

    // ── Texture generators ────────────────────────────────────────────

    /// <summary>歯車スプライト: 8歯の歯車形状</summary>
    private static Texture2D MakeGearTex()
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        float cx = sz / 2f;
        float cy = sz / 2f;
        float outerR = sz * 0.46f;
        float innerR = sz * 0.32f;
        float holeR  = sz * 0.12f;
        int toothCount = 8;
        Color gearColor = Color.white;
        Color borderColor = new Color(0.5f, 0.5f, 0.55f, 1f);
        int bw = 2;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx); // -PI ~ PI

                // 歯の判定: 歯車の角度範囲内かつ外周
                bool inTooth = false;
                float toothAngleHalf = Mathf.PI / toothCount * 0.6f;
                for (int t = 0; t < toothCount; t++)
                {
                    float ta = t * (2f * Mathf.PI / toothCount);
                    float diff = Mathf.Abs(Mathf.DeltaAngle(angle * Mathf.Rad2Deg, ta * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                    if (diff <= toothAngleHalf && dist <= outerR) { inTooth = true; break; }
                }

                bool inBody = dist <= innerR;
                bool inHole = dist <= holeR;

                if ((inTooth || inBody) && !inHole)
                {
                    // ボーダー
                    bool isBorder = false;
                    float borderDist = (inTooth && !inBody) ? (dist - (innerR - bw * 2f)) : 0f;
                    if (dist >= innerR - bw && dist <= innerR + bw) isBorder = true;
                    if (dist >= holeR && dist <= holeR + bw) isBorder = true;
                    px[y * sz + x] = isBorder ? borderColor : gearColor;
                }

                // 穴の縁
                if (dist >= holeR && dist <= holeR + bw)
                    px[y * sz + x] = borderColor;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>矢印スプライト: 上向き三角形</summary>
    private static Texture2D MakeArrowTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        Color arrowColor = Color.white;
        float cx = sz / 2f;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                // 上向き矢印: y=10(底) → y=50(頂点)
                float normY = (y - 8f) / (sz - 16f); // 0~1
                float halfW = (1f - normY) * (sz * 0.38f);
                if (normY >= 0 && normY <= 1 && Mathf.Abs(x - cx) <= halfW)
                {
                    px[y * sz + x] = arrowColor;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakePanelBgTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        Color fill = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeButtonBgTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        Color fill = Color.white;
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        return tex;
    }

    private static Sprite GetOrCreateSprite(string path, Texture2D tex)
    {
        if (!File.Exists(path))
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path);
            }
        }
        Object.DestroyImmediate(tex);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ── UI helpers ─────────────────────────────────────────────────────

    private static GameObject MakeText(Transform parent, string name, string text,
        int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    private static GameObject MakeButton(Transform parent, string name, string label,
        Color bg, Sprite bgSprite = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        if (bgSprite != null) img.sprite = bgSprite;
        go.AddComponent<Button>();
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var t = lbl.AddComponent<Text>();
        t.text = label;
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = lbl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        return go;
    }

    private static void SetRT(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pivot,
        Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == scenePath) return;
        var n = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, n, scenes.Length);
        n[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = n;
    }
}
