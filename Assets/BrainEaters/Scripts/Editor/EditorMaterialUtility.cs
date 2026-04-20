using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrainEaters.EditorTools
{
    internal static class EditorMaterialUtility
    {
        private const string MaterialsFolderPath = "Assets/BrainEaters/Materials";
        private const string GeneratedMaterialsFolderPath = "Assets/BrainEaters/Materials/Generated";

        public static Material GetOrCreateLitMaterialAsset(string materialName, Color color)
        {
            EnsureFolders();

            string assetPath = $"{GeneratedMaterialsFolderPath}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                Shader shader = FindLitShader();
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            Shader targetShader = FindLitShader();
            if (material.shader != targetShader)
            {
                material.shader = targetShader;
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(MaterialsFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedMaterialsFolderPath))
            {
                AssetDatabase.CreateFolder(MaterialsFolderPath, "Generated");
            }
        }

        private static Shader FindLitShader()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                Shader pipelineShader = Shader.Find("Universal Render Pipeline/Lit");
                if (pipelineShader != null)
                {
                    return pipelineShader;
                }
            }

            Shader standardShader = Shader.Find("Standard");
            if (standardShader != null)
            {
                return standardShader;
            }

            Shader diffuseShader = Shader.Find("Legacy Shaders/Diffuse");
            if (diffuseShader != null)
            {
                return diffuseShader;
            }

            return Shader.Find("Sprites/Default");
        }
    }
}
