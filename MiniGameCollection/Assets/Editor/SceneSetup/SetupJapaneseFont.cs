using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Noto Sans JP から日本語対応 TMP フォントアセットを生成する Editor スクリプト。
/// Assets > Setup > Generate Japanese Font から実行する。
/// </summary>
public static class SetupJapaneseFont
{
    // TopMenu で使用する日本語文字（カテゴリ名 + UI テキスト）
    private const string JAPANESE_CHARACTERS =
        "パズルアクションカジュアル放置リズム育成ユニークミニゲーム集" +
        "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
        "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ" +
        "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
        "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ" +
        "一二三四五六七八九十百千万選択開始終了戻る完成未着手作業中工数" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
        "0123456789" +
        "()[]{}:;.,!?-_/\\@#$%&*+= 　";

    [MenuItem("Assets/Setup/Generate Japanese Font")]
    public static void GenerateFont()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[SetupJapaneseFont] Play モード中は実行できません。");
            return;
        }

        // 既存のアセットがあれば削除
        string outputPath = "Assets/Fonts/NotoSansJP-Regular SDF.asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath) != null)
        {
            AssetDatabase.DeleteAsset(outputPath);
        }

        // フォントファイルを読み込み
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansJP-Regular.ttf");
        if (font == null)
        {
            Debug.LogError("[SetupJapaneseFont] Assets/Fonts/NotoSansJP-Regular.ttf が見つかりません");
            return;
        }

        // TMP フォントアセットを生成（サンプリングサイズ指定）
        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font,
            samplingPointSize: 36,
            atlasPadding: 5,
            renderMode: UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            atlasWidth: 2048,
            atlasHeight: 2048
        );
        if (fontAsset == null)
        {
            Debug.LogError("[SetupJapaneseFont] フォントアセットの生成に失敗しました");
            return;
        }

        // アトラステクスチャを readable に設定
        fontAsset.atlasTexture.name = "NotoSansJP-Regular Atlas";
        fontAsset.material.name = "NotoSansJP-Regular Material";

        // まずメインアセットを保存
        AssetDatabase.CreateAsset(fontAsset, outputPath);

        // サブアセットとしてテクスチャとマテリアルを追加
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);

        // ここで一度保存してアセットを確定させる
        AssetDatabase.SaveAssets();

        // 日本語文字を追加
        uint[] unicodes = new uint[JAPANESE_CHARACTERS.Length];
        for (int i = 0; i < JAPANESE_CHARACTERS.Length; i++)
        {
            unicodes[i] = JAPANESE_CHARACTERS[i];
        }
        fontAsset.TryAddCharacters(unicodes, out uint[] missing);

        int addedCount = JAPANESE_CHARACTERS.Length - (missing?.Length ?? 0);
        if (missing != null && missing.Length > 0)
        {
            Debug.LogWarning($"[SetupJapaneseFont] {missing.Length} 文字が追加できませんでした（{addedCount} 文字を追加）");
        }

        // 最終保存
        EditorUtility.SetDirty(fontAsset);
        EditorUtility.SetDirty(fontAsset.atlasTexture);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SetupJapaneseFont] フォントアセットを作成しました: {outputPath}（{addedCount} 文字）");
    }
}
