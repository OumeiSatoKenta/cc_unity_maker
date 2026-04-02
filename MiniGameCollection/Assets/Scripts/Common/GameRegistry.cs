using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GameRegistry.json を読み込み、ゲーム一覧データを提供するシングルトン。
/// DontDestroyOnLoad で全シーンを通じて存続する。
/// </summary>
public class GameRegistry : MonoBehaviour
{
    public static GameRegistry Instance { get; private set; }

    private GameRegistryData _registryData = new GameRegistryData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadRegistry();
    }

    private void LoadRegistry()
    {
        var json = Resources.Load<TextAsset>("GameRegistry");
        if (json == null)
        {
            Debug.LogError("[GameRegistry] GameRegistry.json が Resources/ に見つかりません");
            _registryData = new GameRegistryData();
            return;
        }

        var parsed = JsonUtility.FromJson<GameRegistryData>(json.text);
        if (parsed == null || parsed.games == null)
        {
            Debug.LogError("[GameRegistry] GameRegistry.json のパースに失敗しました");
            return;
        }

        _registryData = parsed;
        Debug.Log($"[GameRegistry] {_registryData.games.Count} ゲームを読み込みました");
    }

    /// <summary>
    /// 全ゲーム一覧を返す。
    /// </summary>
    public List<GameEntry> GetAllGames()
    {
        return _registryData.games;
    }

    /// <summary>
    /// 指定カテゴリのゲーム一覧を返す。
    /// </summary>
    public List<GameEntry> GetGamesByCategory(string category)
    {
        return _registryData.games.Where(g => g.category == category).ToList();
    }

    /// <summary>
    /// 実装済みゲームのみ返す。
    /// </summary>
    public List<GameEntry> GetImplementedGames()
    {
        return _registryData.games.Where(g => g.implemented).ToList();
    }

    /// <summary>
    /// IDでゲームを検索する。見つからない場合は null。
    /// </summary>
    public GameEntry GetGameById(string id)
    {
        return _registryData.games.FirstOrDefault(g => g.id == id);
    }

    /// <summary>
    /// 指定コレクションのゲーム一覧を返す。
    /// </summary>
    public List<GameEntry> GetGamesByCollection(string collection)
    {
        return _registryData.games.Where(g => g.collection == collection).ToList();
    }

    /// <summary>
    /// 指定コレクション+カテゴリのゲーム一覧を返す。
    /// </summary>
    public List<GameEntry> GetGamesByCategoryAndCollection(string category, string collection)
    {
        return _registryData.games.Where(g => g.category == category && g.collection == collection).ToList();
    }

    /// <summary>
    /// コレクション一覧を取得する。
    /// </summary>
    public List<string> GetCollections()
    {
        return _registryData.games.Select(g => g.collection ?? "classic").Distinct().ToList();
    }
}
