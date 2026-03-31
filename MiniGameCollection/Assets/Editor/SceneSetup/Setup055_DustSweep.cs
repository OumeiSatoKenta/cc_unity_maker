using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game055_DustSweep;
public static class Setup055_DustSweep
{
    [MenuItem("Assets/Setup/055 DustSweep")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup055] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.85f,0.82f,0.75f,1f);camera.orthographic=true;camera.orthographicSize=4f;}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<DustSweepGameManager>();
        var boardObj=new GameObject("SweepBoard");boardObj.transform.SetParent(gmObj.transform);var sm=boardObj.AddComponent<SweepManager>();
        var smSO=new SerializedObject(sm);smSO.FindProperty("_gameManager").objectReferenceValue=gm;smSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var cleanText=CT(canvasObj.transform,"CleanText","0%",40,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(150,50),new Vector2(0,-15));
        cleanText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;cleanText.GetComponent<TextMeshProUGUI>().color=new Color(0.3f,0.3f,0.3f);
        var timerText=CT(canvasObj.transform,"TimerText","0.0s",24,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(120,35),new Vector2(20,-20));
        timerText.GetComponent<TextMeshProUGUI>().color=new Color(0.4f,0.4f,0.4f);
        var starText=CT(canvasObj.transform,"StarText","\u2605 x0",28,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(120,40),new Vector2(-20,-20));
        starText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Right;starText.GetComponent<TextMeshProUGUI>().color=new Color(0.8f,0.7f,0.2f);
        var hint=CT(canvasObj.transform,"HintText","スワイプで砂埃を払おう!",18,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(400,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.5f,0.5f,0.45f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(240,50),new Vector2(20,20),new Color(0.4f,0.35f,0.3f,0.9f));

        var clrPanel=new GameObject("ClearPanel",typeof(RectTransform));clrPanel.transform.SetParent(canvasObj.transform,false);clrPanel.AddComponent<Image>().color=new Color(0,0,0,0.8f);
        var cr=clrPanel.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0.2f,0.3f);cr.anchorMax=new Vector2(0.8f,0.7f);cr.offsetMin=cr.offsetMax=Vector2.zero;
        CT(clrPanel.transform,"T","ピカピカ!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero).GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var resultText=CT(clrPanel.transform,"R","",32,jpFont,new Vector2(0.5f,0.45f),new Vector2(0.5f,0.45f),new Vector2(0.5f,0.5f),new Vector2(400,50),Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;resultText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.9f,0.3f);
        var retryBtn=CB(clrPanel.transform,"RetryBtn","もう一度",28,jpFont,new Vector2(0.5f,0.15f),new Vector2(0.5f,0.15f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.5f,0.4f,0.25f,1f));
        clrPanel.SetActive(false);

        var uiObj=new GameObject("DustSweepUI");var dsUI=uiObj.AddComponent<DustSweepUI>();
        var uiSO=new SerializedObject(dsUI);uiSO.FindProperty("_timerText").objectReferenceValue=timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_cleanText").objectReferenceValue=cleanText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_starText").objectReferenceValue=starText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue=clrPanel;uiSO.FindProperty("_clearResultText").objectReferenceValue=resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_sweepManager").objectReferenceValue=sm;gmSO.FindProperty("_ui").objectReferenceValue=dsUI;gmSO.ApplyModifiedProperties();
        retryBtn.GetComponent<Button>().onClick.AddListener(dsUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/055_DustSweep.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup055_DustSweep] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
