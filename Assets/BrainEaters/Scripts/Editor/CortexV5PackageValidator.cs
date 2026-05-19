using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    [InitializeOnLoad]
    public static class CortexV5PackageValidator
    {
        private const string PackageName = "com.emotiv.cortex";
        private const string DefineSymbol = "EMOTIV_CORTEX_V5";
        private const string InstallInstructionsUrl = "https://github.com/Emotiv/unity-plugin-v5/blob/main/Src/README.md";
        private const string MissingPackageSessionKey = "BrainEaters.CortexV5PackageValidator.MissingPackageLogged";
        private static ListRequest listRequest;

        static CortexV5PackageValidator()
        {
            EditorApplication.delayCall += ValidatePackage;
        }

        [MenuItem("Brain Eaters/Cortex/Validate Cortex v5 Package")]
        public static void ValidatePackage()
        {
            if (listRequest != null && !listRequest.IsCompleted)
            {
                return;
            }

            listRequest = Client.List(true, true);
            EditorApplication.update += PollPackageList;
        }

        private static void PollPackageList()
        {
            if (listRequest == null || !listRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= PollPackageList;

            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogWarning($"Could not validate Cortex v5 package. Package Manager error: {listRequest.Error?.message}");
                return;
            }

            bool isInstalled = listRequest.Result.Any(package => string.Equals(package.name, PackageName, StringComparison.Ordinal));
            SetDefineSymbol(isInstalled);

            if (isInstalled)
            {
                SessionState.EraseString(MissingPackageSessionKey);
                UnityEditor.PackageManager.PackageInfo packageInfo = listRequest.Result.First(package => string.Equals(package.name, PackageName, StringComparison.Ordinal));
                Debug.Log($"Cortex v5 package detected: {packageInfo.name} {packageInfo.version}. Define `{DefineSymbol}` is enabled.");
                return;
            }

            LogMissingPackageError();
        }

        private static void LogMissingPackageError()
        {
            if (SessionState.GetBool(MissingPackageSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(MissingPackageSessionKey, true);
            Debug.LogError(
                $"Cortex v5 package `{PackageName}` is not installed. " +
                $"Install it through Unity Package Manager > Add package from tarball before enabling real Cortex integration. " +
                $"Instructions: {InstallInstructionsUrl}");
        }

        private static void SetDefineSymbol(bool shouldBeEnabled)
        {
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (targetGroup == BuildTargetGroup.Unknown)
            {
                return;
            }

            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            string[] symbols = currentSymbols
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(symbol => symbol.Trim())
                .Where(symbol => !string.IsNullOrEmpty(symbol))
                .Distinct()
                .ToArray();

            bool containsSymbol = symbols.Contains(DefineSymbol);
            if (shouldBeEnabled == containsSymbol)
            {
                return;
            }

            string[] updatedSymbols = shouldBeEnabled
                ? symbols.Concat(new[] { DefineSymbol }).ToArray()
                : symbols.Where(symbol => symbol != DefineSymbol).ToArray();

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", updatedSymbols));
        }
    }
}
