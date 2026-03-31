using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game051_DrawBridge;
public static class Setup051_DrawBridge
{
    [MenuItem("Assets/Setup/051 DrawBridge")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup051] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.6f,0.8f,1f,1f);camera.orthographic=true;camera.orthographicSize=5f;}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<DrawBridgeGameManager>();
        var boardObj=new GameObject("BridgeBoard");boardObj.transform.SetParent(gmObj.transform);var bm=boardObj.AddComponent<BridgeDrawManager>();
        var bmSO=new SerializedObject(bm);bmSO.FindProperty("_gameManager").objectReferenceValue=gm;bmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var stageText=CT(canvasObj.transform,"StageText","ステージ 1",32,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(300,50),new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;stageText.GetComponent<TextMeshProUGUI>().color=new Color(0.2f,0.2f,0.4f);
        var hint=CT(canvasObj.transform,"HintText","描いて橋を作り、スペースでボール発射!",18,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(500,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.3f,0.3f,0.5f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.3f,0.4f,0.5f,0.9f));

        // Clear panel
        var clrPanel=new GameObject("ClearPanel",typeof(RectTransform));clrPanel.transform.SetParent(canvasObj.transform,false);clrPanel.AddComponent<Image>().color=new Color(0,0,0,0.8f);
        var cr=clrPanel.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0.25f,0.3f);cr.anchorMax=new Vector2(0.75f,0.7f);cr.offsetMin=cr.offsetMax=Vector2.zero;
        CT(clrPanel.transform,"T","クリア!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero).GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var nextBtn=CB(clrPanel.transform,"NextBtn","次のステージ",28,jpFont,new Vector2(0.5f,0.25f),new Vector2(0.5f,0.25f),new Vector2(0.5f,0.5f),new Vector2(220,60),Vector2.zero,new Color(0.2f,0.6f,0.3f,1f));
        clrPanel.SetActive(false);

        // Fail panel
        var failPanel=new GameObject("FailPanel",typeof(RectTransform));failPanel.transform.SetParent(canvasObj.transform,false);failPanel.AddComponent<Image>().color=new Color(0,0,0,0.8f);
        var fr=failPanel.GetComponent<RectTransform>();fr.anchorMin=new Vector2(0.25f,0.3f);fr.anchorMax=new Vector2(0.75f,0.7f);fr.offsetMin=fr.offsetMax=Vector2.zero;
        CT(failPanel.transform,"T","落下!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero).GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var retryBtn=CB(failPanel.transform,"RetryBtn","リトライ",28,jpFont,new Vector2(0.5f,0.25f),new Vector2(0.5f,0.25f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.7f,0.3f,0.2f,1f));
        failPanel.SetActive(false);

        var uiObj=new GameObject("DrawBridgeUI");var dbUI=uiObj.AddComponent<DrawBridgeUI>();
        var uiSO=new SerializedObject(dbUI);uiSO.FindProperty("_stageText").objectReferenceValue=stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue=clrPanel;uiSO.FindProperty("_failPanel").objectReferenceValue=failPanel;
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_bridgeManager").objectReferenceValue=bm;gmSO.FindProperty("_ui").objectReferenceValue=dbUI;gmSO.ApplyModifiedProperties();

        nextBtn.GetComponent<Button>().onClick.AddListener(dbUI.OnNextButton);
        retryBtn.GetComponent<Button>().onClick.AddListener(dbUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/051_DrawBridge.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup051_DrawBridge] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
