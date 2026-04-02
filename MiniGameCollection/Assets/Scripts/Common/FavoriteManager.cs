using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ゲームのお気に入りをPlayerPrefsで管理するシングルトン。
/// DontDestroyOnLoadで全シーンを通じて存続する。
/// </summary>
public class FavoriteManager : MonoBehaviour
{
    public static FavoriteManager Instance { get; private set; }

    private const string PrefsKey = "favorite_game_ids";
    private readonly HashSet<string> _favorites = new HashSet<string>();

    /// <summary>お気に入り状態変更時に発火（引数: gameId, isFavorite）</summary>
    public event Action<string, bool> OnFavoriteChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>お気に入りをトグルする</summary>
    public void ToggleFavorite(string gameId)
    {
        if (_favorites.Contains(gameId))
        {
            _favorites.Remove(gameId);
            OnFavoriteChanged?.Invoke(gameId, false);
        }
        else
        {
            _favorites.Add(gameId);
            OnFavoriteChanged?.Invoke(gameId, true);
        }
        Save();
    }

    /// <summary>お気に入りかどうか判定</summary>
    public bool IsFavorite(string gameId)
    {
        return _favorites.Contains(gameId);
    }

    /// <summary>お気に入りゲームIDリストを取得</summary>
    public List<string> GetFavoriteIds()
    {
        return _favorites.ToList();
    }

    /// <summary>お気に入り数を取得</summary>
    public int Count => _favorites.Count;

    private void Load()
    {
        _favorites.Clear();
        string saved = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(saved)) return;

        foreach (string id in saved.Split(','))
        {
            string trimmed = id.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                _favorites.Add(trimmed);
        }
    }

    private void Save()
    {
        string value = string.Join(",", _favorites);
        PlayerPrefs.SetString(PrefsKey, value);
        PlayerPrefs.Save();
    }
}
