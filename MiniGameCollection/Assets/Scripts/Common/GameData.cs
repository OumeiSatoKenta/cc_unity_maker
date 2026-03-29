using System;
using System.Collections.Generic;

/// <summary>
/// GameRegistry.json のデシリアライズ用データクラス群。
/// JsonUtility で読み込むため [Serializable] 属性が必要。
/// </summary>
[Serializable]
public class GameEntry
{
    public string id;
    public string title;
    public string category;
    public string size;
    public string sceneName;
    public string description;
    public bool implemented;
}

[Serializable]
public class GameRegistryData
{
    public List<GameEntry> games = new List<GameEntry>();
}
