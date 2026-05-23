using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class CJKFontCreator
{
    private const string FontAssetPath = "Assets/Fonts/Deng SDF.asset";
    private const string SourceFontPath = "Assets/Fonts/Deng.ttf";
    private const string CharsFilePath = "Assets/Fonts/CJK_Characters.txt";

    [MenuItem("Tools/Add CJK Fallback to All Fonts")]
    public static void AddCJKFallbackToAll()
    {
        // Always recreate to ensure latest characters are baked
        AssetDatabase.DeleteAsset(FontAssetPath);
        TMP_FontAsset fontAsset = CreateFontAsset();
        if (fontAsset == null)
            return;

        // Wire fallback chain
        int count = WireFallbacks(fontAsset);
        Debug.Log($"[CJKFontCreator] Done. Fallback wired to {count} fonts.");
    }

    private static TMP_FontAsset CreateFontAsset()
    {
        // Step 1: Configure the TrueType font importer to include all Unicode
        // Default is ASCII-only, so CJK glyphs aren't available to TMP
        AssetImporter importer = AssetImporter.GetAtPath(SourceFontPath);
        if (importer is TrueTypeFontImporter fontImporter)
        {
            fontImporter.fontTextureCase = FontTextureCase.Dynamic;
            fontImporter.fontRenderingMode = FontRenderingMode.Smooth;
            fontImporter.SaveAndReimport();

            // Must flush the async reimport so the Font object has the full char set
            AssetDatabase.Refresh();
            EditorApplication.delayCall += () => {};
        }
        AssetDatabase.ImportAsset(SourceFontPath, ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[CJKFontCreator] Font importer set to Dynamic character mode.");

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError("[CJKFontCreator] Source font not found.");
            return null;
        }

        // Read CJK characters
        string characterSet = "";
        if (File.Exists(CharsFilePath))
        {
            characterSet = File.ReadAllText(CharsFilePath);
        }

        // Create with dynamic atlas — CJK glyphs loaded on demand
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            36,
            9,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            1024, 1024,
            AtlasPopulationMode.Dynamic,
            true
        );

        if (fontAsset == null)
        {
            Debug.LogError("[CJKFontCreator] CreateFontAsset returned null.");
            return null;
        }

        // Note: atlasTexture is read-only. TryAddCharacters below will
        // initialize the atlas internally when baking the character set.

        // Add the CJK characters — this bakes them into the atlas
        if (!string.IsNullOrEmpty(characterSet))
        {
            fontAsset.TryAddCharacters(characterSet);
            Debug.Log($"[CJKFontCreator] Baked {characterSet.Length} characters into atlas.");
        }

        // Save main asset
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // Save material as sub-asset
        if (fontAsset.material != null)
        {
            fontAsset.material.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        // Save atlas texture as sub-asset
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        // Also save any atlas textures in the list
        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                Texture2D tex = fontAsset.atlasTextures[i];
                if (tex != null && !AssetDatabase.IsSubAsset(tex))
                {
                    tex.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CJKFontCreator] Font asset created.");
        return fontAsset;
    }

    private static int WireFallbacks(TMP_FontAsset dengFont)
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset target = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (target == null || target == dengFont)
                continue;

            if (target.fallbackFontAssetTable == null)
                target.fallbackFontAssetTable = new List<TMP_FontAsset>();

            if (target.fallbackFontAssetTable.Contains(dengFont))
            {
                count++;
                continue;
            }

            target.fallbackFontAssetTable.Add(dengFont);
            EditorUtility.SetDirty(target);
            count++;
            Debug.Log($"[CJKFontCreator] Fallback: {target.name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return count;
    }

    [MenuItem("Tools/Remove CJK Fallback")]
    public static void RemoveCJKFallback()
    {
        TMP_FontAsset deng = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset target = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (target == null || target.fallbackFontAssetTable == null)
                continue;

            int removed = target.fallbackFontAssetTable.RemoveAll(f =>
                f != null && (f == deng || f.name.Contains("Deng")));
            if (removed > 0)
            {
                EditorUtility.SetDirty(target);
                Debug.Log($"[CJKFontCreator] Removed from: {target.name}");
            }
        }

        // Delete the font asset if it exists
        if (deng != null)
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
            Debug.Log("[CJKFontCreator] Deleted Deng SDF.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CJKFontCreator] Cleanup complete.");
    }
}
