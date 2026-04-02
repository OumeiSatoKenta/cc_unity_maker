using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// コレクション選択画面の制御。Classic / Remake / お気に入りの3択。
/// </summary>
public class CollectionSelectManager : MonoBehaviour
{
    [SerializeField, Tooltip("Classicボタン")] private Button _classicButton;
    [SerializeField, Tooltip("Remakeボタン")] private Button _remakeButton;
    [SerializeField, Tooltip("お気に入りボタン")] private Button _favoriteButton;
    [SerializeField, Tooltip("Classicゲーム数テキスト")] private TextMeshProUGUI _classicCountText;
    [SerializeField, Tooltip("Remakeゲーム数テキスト")] private TextMeshProUGUI _remakeCountText;
    [SerializeField, Tooltip("お気に入りゲーム数テキスト")] private TextMeshProUGUI _favoriteCountText;

    private void Start()
    {
        if (_classicButton != null)
            _classicButton.onClick.AddListener(() => OnCollectionSelected("classic"));
        if (_remakeButton != null)
            _remakeButton.onClick.AddListener(() => OnCollectionSelected("remake"));
        if (_favoriteButton != null)
            _favoriteButton.onClick.AddListener(() => OnCollectionSelected("favorite"));

        UpdateCounts();
    }

    private void OnCollectionSelected(string collection)
    {
        if (collection == "favorite")
        {
            SceneLoader.CurrentCollection = "favorite";
        }
        else
        {
            SceneLoader.CurrentCollection = collection;
        }
        SceneLoader.LoadCollectionMenu(SceneLoader.CurrentCollection);
    }

    private void UpdateCounts()
    {
        if (GameRegistry.Instance == null) return;

        var classic = GameRegistry.Instance.GetGamesByCollection("classic");
        var remake = GameRegistry.Instance.GetGamesByCollection("remake");
        int classicImpl = 0, remakeImpl = 0;
        foreach (var g in classic) if (g.implemented) classicImpl++;
        foreach (var g in remake) if (g.implemented) remakeImpl++;

        if (_classicCountText != null)
            _classicCountText.text = $"{classicImpl} ゲーム";
        if (_remakeCountText != null)
        {
            int remakeTotal = remake.Count;
            _remakeCountText.text = remakeImpl > 0 ? $"{remakeImpl}/{remakeTotal} ゲーム" : $"{remakeTotal} ゲーム予定";
        }
        if (_favoriteCountText != null)
        {
            int favCount = FavoriteManager.Instance != null ? FavoriteManager.Instance.Count : 0;
            _favoriteCountText.text = $"{favCount} ゲーム";
        }
    }
}
