using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game054_FruitSlash;
public static class Setup054_FruitSlash
{
    [MenuItem("Assets/Setup/054 FruitSlash")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup054] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.05f,0.08f,0.15f,1f);camera.orthographic=true;camera.orthographicSize=5f;}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<FruitSlashGameManager>();
        var boardObj=new GameObject("FruitBoard");boardObj.transform.SetParent(gmObj.transform);var fm=boardObj.AddComponent<FruitManager>();
        var fmSO=new SerializedObject(fm);fmSO.FindProperty("_gameManager").objectReferenceValue=gm;fmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var scoreText=CT(canvasObj.transform,"ScoreText","スコア: 0",32,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(250,50),new Vector2(20,-20));
        var lifeText=CT(canvasObj.transform,"LifeText","\u2665 \u2665 \u2665",36,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(200,50),new Vector2(-20,-20));
        lifeText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.3f,0.3f);lifeText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Right;
        var comboText=CT(canvasObj.transform,"ComboText","",40,jpFont,new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(300,60),new Vector2(0,100));
        comboText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;comboText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.9f,0.3f);
        var hint=CT(canvasObj.transform,"HintText","スワイプでフルーツを切れ!",20,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(400,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.4f,0.4f,0.5f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(240,50),new Vector2(0,-70),new Color(0.3f,0.3f,0.4f,0.9f));

        var goPanel=new GameObject("GameOverPanel",typeof(RectTransform));goPanel.transform.SetParent(canvasObj.transform,false);goPanel.AddComponent<Image>().color=new Color(0,0,0,0.85f);
        var gr=goPanel.GetComponent<RectTransform>();gr.anchorMin=new Vector2(0.2f,0.25f);gr.anchorMax=new Vector2(0.8f,0.75f);gr.offsetMin=gr.offsetMax=Vector2.zero;
        CT(goPanel.transform,"T","ゲームオーバー",42,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(400,60),Vector2.zero).GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var finalText=CT(goPanel.transform,"FS","スコア: 0",36,jpFont,new Vector2(0.5f,0.45f),new Vector2(0.5f,0.45f),new Vector2(0.5f,0.5f),new Vector2(300,50),Vector2.zero);
        finalText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;finalText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.85f,0.3f);
        var retryBtn=CB(goPanel.transform,"RetryBtn","リトライ",28,jpFont,new Vector2(0.5f,0.15f),new Vector2(0.5f,0.15f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.7f,0.3f,0.2f,1f));
        goPanel.SetActive(false);

        var uiObj=new GameObject("FruitSlashUI");var fsUI=uiObj.AddComponent<FruitSlashUI>();
        var uiSO=new SerializedObject(fsUI);uiSO.FindProperty("_scoreText").objectReferenceValue=scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue=comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_lifeText").objectReferenceValue=lifeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue=goPanel;uiSO.FindProperty("_finalScoreText").objectReferenceValue=finalText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_fruitManager").objectReferenceValue=fm;gmSO.FindProperty("_ui").objectReferenceValue=fsUI;gmSO.ApplyModifiedProperties();
        retryBtn.GetComponent<Button>().onClick.AddListener(fsUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/054_FruitSlash.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup054_FruitSlash] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
