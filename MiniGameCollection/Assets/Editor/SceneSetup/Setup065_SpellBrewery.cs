using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game065_SpellBrewery;

public static class Setup065_SpellBrewery
{
    [MenuItem("Assets/Setup/065 SpellBrewery")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup065] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game065_SpellBrewery/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.12f, 0.08f, 0.16f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite cauldronSprite = LoadSprite(sp + "cauldron.png");
        Sprite herbSprite = LoadSprite(sp + "herb.png");
        Sprite crystalSprite = LoadSprite(sp + "crystal.png");
        Sprite mushroomSprite = LoadSprite(sp + "mushroom.png");
        Sprite potionSprite = LoadSprite(sp + "potion.png");

        // Background
        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Cauldron visual
        var cauldronObj = new GameObject("Cauldron");
        cauldronObj.transform.position = new Vector3(0f, -1f, 0f);
        var cSr = cauldronObj.AddComponent<SpriteRenderer>();
        cSr.sprite = cauldronSprite; cSr.sortingOrder = 2;
        cauldronObj.transform.localScale = Vector3.one * 1.5f;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SpellBreweryGameManager>();

        var bmObj = new GameObject("BrewManager"); bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BrewManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_herbSprite").objectReferenceValue = herbSprite;
        bmSO.FindProperty("_crystalSprite").objectReferenceValue = crystalSprite;
        bmSO.FindProperty("_mushroomSprite").objectReferenceValue = mushroomSprite;
        bmSO.FindProperty("_potionSprite").objectReferenceValue = potionSprite;
        bmSO.FindProperty("_gatherInterval").floatValue = 3f;
        bmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ingredient text
        var ingredientText = CT(canvasObj.transform, "IngredientText", "", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 40), new Vector2(0, -15));
        ingredientText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ingredientText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 0.7f);

        // Recipe text
        var recipeText = CT(canvasObj.transform, "RecipeText", "レシピ: 0/5", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(300, 40), new Vector2(0, -55));
        recipeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        recipeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.4f);

        // Ingredient buttons
        var herbBtn = CB(canvasObj.transform, "HerbButton", "ハーブ", 24, jpFont,
            new Vector2(0.2f, 0), new Vector2(0.2f, 0), new Vector2(0.5f, 0), new Vector2(160, 70), new Vector2(0, 100), new Color(0.2f, 0.5f, 0.2f, 0.9f));
        var crystalBtn = CB(canvasObj.transform, "CrystalButton", "クリスタル", 24, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(160, 70), new Vector2(0, 100), new Color(0.4f, 0.3f, 0.6f, 0.9f));
        var mushroomBtn = CB(canvasObj.transform, "MushroomButton", "キノコ", 24, jpFont,
            new Vector2(0.8f, 0), new Vector2(0.8f, 0), new Vector2(0.5f, 0), new Vector2(160, 70), new Vector2(0, 100), new Color(0.6f, 0.2f, 0.2f, 0.9f));
        var resetBtn = CB(canvasObj.transform, "ResetButton", "リセット", 22, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(140, 50), new Vector2(0, 175), new Color(0.4f, 0.4f, 0.4f, 0.9f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear panel
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.15f, 0.1f, 0.25f, 0.95f));
        CT(clearPanel.transform, "CT", "全レシピ発見！", 48, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero, new Color(0.4f, 0.3f, 0.5f));
        clearPanel.SetActive(false);

        // UI component
        var uiObj = new GameObject("SpellBreweryUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<SpellBreweryUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_ingredientText").objectReferenceValue = ingredientText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_recipeText").objectReferenceValue = recipeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_herbButton").objectReferenceValue = herbBtn.GetComponent<Button>();
        uiSO.FindProperty("_crystalButton").objectReferenceValue = crystalBtn.GetComponent<Button>();
        uiSO.FindProperty("_mushroomButton").objectReferenceValue = mushroomBtn.GetComponent<Button>();
        uiSO.FindProperty("_resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        uiSO.FindProperty("_brewManager").objectReferenceValue = bm;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_brewManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalRecipes").intValue = 5;
        gmSO.ApplyModifiedProperties();

        // Button events - ingredient selection
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(herbBtn.GetComponent<Button>().onClick, bm.SelectIngredient, 0);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(crystalBtn.GetComponent<Button>().onClick, bm.SelectIngredient, 1);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(mushroomBtn.GetComponent<Button>().onClick, bm.SelectIngredient, 2);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, bm.ClearSelection);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/065_SpellBrewery.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup065] SpellBrewery シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
