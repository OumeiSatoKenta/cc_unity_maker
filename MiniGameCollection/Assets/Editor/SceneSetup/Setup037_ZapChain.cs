using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game037_ZapChain;
public static class Setup037_ZapChain
{
    [MenuItem("Assets/Setup/037 ZapChain")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup037_ZapChain] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.02f,0.02f,0.08f,1f);camera.orthographic=true;camera.orthographicSize=5f;}
        string whiteTexPath="Assets/Scripts/Game037_ZapChain/WhiteSquare.png";
        if(!System.IO.File.Exists(whiteTexPath)){var wTex=new Texture2D(4,4);var px=new Color[16];for(int i=0;i<16;i++)px[i]=Color.white;wTex.SetPixels(px);wTex.Apply();System.IO.File.WriteAllBytes(whiteTexPath,wTex.EncodeToPNG());Object.DestroyImmediate(wTex);AssetDatabase.ImportAsset(whiteTexPath);var imp=AssetImporter.GetAtPath(whiteTexPath)as TextureImporter;if(imp!=null){imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=1;imp.SaveAndReimport();}}
        var whiteSprite=AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);string sp="Assets/Resources/Sprites/Game037_ZapChain/";string pd="Assets/Scripts/Game037_ZapChain/";
        var playerPrefab=SP(pd+"PlayerPrefab.prefab",AssetDatabase.LoadAssetAtPath<Sprite>(sp+"player.png"),whiteSprite,10);
        var enemyPrefab=SP(pd+"EnemyPrefab.prefab",AssetDatabase.LoadAssetAtPath<Sprite>(sp+"enemy.png"),whiteSprite,5);

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<ZapChainGameManager>();
        var boardObj=new GameObject("ZapBoard");boardObj.transform.SetParent(gmObj.transform);var zm=boardObj.AddComponent<ZapManager>();
        var zmSO=new SerializedObject(zm);zmSO.FindProperty("_playerPrefab").objectReferenceValue=playerPrefab;zmSO.FindProperty("_enemyPrefab").objectReferenceValue=enemyPrefab;
        zmSO.FindProperty("_spawnInterval").floatValue=1.5f;zmSO.FindProperty("_chainRadius").floatValue=2f;zmSO.FindProperty("_gameTime").floatValue=30f;zmSO.FindProperty("_maxEnemies").intValue=12;zmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var scoreText=CT(canvasObj.transform,"ScoreText","スコア: 0",28,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(220,40),new Vector2(20,-20));
        var chainText=CT(canvasObj.transform,"ChainText","",48,jpFont,new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(300,80),Vector2.zero);
        chainText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;chainText.GetComponent<TextMeshProUGUI>().color=new Color(1f,1f,0.3f);
        var timeText=CT(canvasObj.transform,"TimeText","残り: 30秒",32,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(250,50),new Vector2(0,-20));
        timeText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.3f,0.3f,0.4f,0.9f));

        var resultPanel=new GameObject("ResultPanel",typeof(RectTransform));resultPanel.transform.SetParent(canvasObj.transform,false);resultPanel.AddComponent<Image>().color=new Color(0,0,0,0.85f);
        var rr=resultPanel.GetComponent<RectTransform>();rr.anchorMin=new Vector2(0.2f,0.2f);rr.anchorMax=new Vector2(0.8f,0.8f);rr.offsetMin=rr.offsetMax=Vector2.zero;
        var resultText=CT(resultPanel.transform,"ResultText","",40,jpFont,new Vector2(0.5f,0.6f),new Vector2(0.5f,0.6f),new Vector2(0.5f,0.5f),new Vector2(400,250),Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var retryBtn=CB(resultPanel.transform,"RetryButton","リトライ",28,jpFont,new Vector2(0.5f,0.1f),new Vector2(0.5f,0.1f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.2f,0.5f,0.8f,1f));
        resultPanel.SetActive(false);

        var uiObj=new GameObject("ZapChainUI");var zcUI=uiObj.AddComponent<ZapChainUI>();
        var uiSO=new SerializedObject(zcUI);uiSO.FindProperty("_scoreText").objectReferenceValue=scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_chainText").objectReferenceValue=chainText.GetComponent<TextMeshProUGUI>();uiSO.FindProperty("_timeText").objectReferenceValue=timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_resultPanel").objectReferenceValue=resultPanel;uiSO.FindProperty("_resultText").objectReferenceValue=resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue=retryBtn.GetComponent<Button>();uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_zapManager").objectReferenceValue=zm;gmSO.FindProperty("_ui").objectReferenceValue=zcUI;gmSO.ApplyModifiedProperties();

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/037_ZapChain.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup037_ZapChain] ZapChain シーンを作成しました: "+scenePath);
    }
    private static GameObject SP(string path,Sprite sprite,Sprite fallback,int order){var o=new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));var sr=o.AddComponent<SpriteRenderer>();sr.sprite=sprite??fallback;sr.sortingOrder=order;var p=PrefabUtility.SaveAsPrefabAsset(o,path);Object.DestroyImmediate(o);return p;}
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
