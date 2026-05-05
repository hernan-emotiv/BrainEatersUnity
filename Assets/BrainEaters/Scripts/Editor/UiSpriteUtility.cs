using TMPro;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    internal static class UiSpriteUtility
    {
        public const string WoodBorderPath = "Assets/BrainEaters/Textures/UI references/Generated/wood_border_clean.png";
        public const string WoodButtonYellowPath = "Assets/BrainEaters/Textures/UI references/Generated/wood_button_yellow.png";
        public const string WoodButtonGreenPath = "Assets/BrainEaters/Textures/UI references/Generated/wood_button_green.png";
        public const string HudFontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset";

        public static Sprite EnsureSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool importerChanged = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importerChanged = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    importerChanged = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    importerChanged = true;
                }

                if (importerChanged)
                {
                    importer.SaveAndReimport();
                }
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"Could not load UI sprite at {path}.");
            }

            return sprite;
        }

        public static TMP_FontAsset LoadHudFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HudFontPath);
            return font != null ? font : TMP_Settings.defaultFontAsset;
        }
    }
}
