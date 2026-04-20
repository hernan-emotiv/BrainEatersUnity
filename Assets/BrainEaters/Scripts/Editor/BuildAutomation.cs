using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    [InitializeOnLoad]
    public static class BuildAutomation
    {
        private const string QueueSessionKey = "BrainEaters.BuildAutomation.Queue";
        private const string BuildsRoot = "Builds";
        private const string AndroidBuildPath = BuildsRoot + "/Android/BrainEaters.apk";
        private const string IosBuildPath = BuildsRoot + "/iOS";

        [Serializable]
        private class BuildQueueState
        {
            public List<BuildTarget> targets = new List<BuildTarget>();
        }

        static BuildAutomation()
        {
            EditorApplication.delayCall += ProcessPendingQueue;
        }

        [MenuItem("Brain Eaters/Build/Build Android APK To Default Folder")]
        public static void BuildAndroidApk()
        {
            StartQueue(new[] { BuildTarget.Android });
        }

        [MenuItem("Brain Eaters/Build/Build iOS To Default Folder")]
        public static void BuildIos()
        {
            StartQueue(new[] { BuildTarget.iOS });
        }

        [MenuItem("Brain Eaters/Build/Build iOS And Android To Default Folders")]
        public static void BuildIosAndAndroid()
        {
            StartQueue(new[] { BuildTarget.iOS, BuildTarget.Android });
        }

        [MenuItem("Brain Eaters/Build/Open Builds Folder")]
        public static void OpenBuildsFolder()
        {
            Directory.CreateDirectory(BuildsRoot);
            EditorUtility.RevealInFinder(Path.GetFullPath(BuildsRoot));
        }

        private static void StartQueue(IEnumerable<BuildTarget> targets)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("No enabled scenes found in Build Settings. Add LevelSelectScene and GameScene first.");
                return;
            }

            BuildQueueState queueState = new BuildQueueState();
            queueState.targets.AddRange(targets);
            SaveQueue(queueState);

            Debug.Log($"Queued build targets: {string.Join(", ", queueState.targets)}");
            ProcessPendingQueue();
        }

        private static void ProcessPendingQueue()
        {
            if (BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            BuildQueueState queueState = LoadQueue();
            if (queueState == null || queueState.targets.Count == 0)
            {
                ClearQueue();
                return;
            }

            BuildTarget nextTarget = queueState.targets[0];
            if (EditorUserBuildSettings.activeBuildTarget != nextTarget)
            {
                Debug.Log($"Switching active build target to {nextTarget}. Unity will reimport/compile as needed.");
                EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(nextTarget), nextTarget);
                return;
            }

            BuildReport report = BuildForTarget(nextTarget);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed for {nextTarget}: {report.summary.result}");
                ClearQueue();
                return;
            }

            Debug.Log($"Build succeeded for {nextTarget}. Output: {GetLocationPathName(nextTarget)}");

            queueState.targets.RemoveAt(0);
            if (queueState.targets.Count == 0)
            {
                ClearQueue();
                return;
            }

            SaveQueue(queueState);
            EditorApplication.delayCall += ProcessPendingQueue;
        }

        private static BuildReport BuildForTarget(BuildTarget target)
        {
            EnsureOutputFolderExists(target);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray(),
                target = target,
                targetGroup = GetBuildTargetGroup(target),
                locationPathName = GetLocationPathName(target),
                options = BuildOptions.None
            };

            return BuildPipeline.BuildPlayer(options);
        }

        private static void EnsureOutputFolderExists(BuildTarget target)
        {
            string path = GetLocationPathName(target);
            string directory = target == BuildTarget.iOS
                ? path
                : Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string GetLocationPathName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.Android => AndroidBuildPath,
                BuildTarget.iOS => IosBuildPath,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported build target.")
            };
        }

        private static BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.Android => BuildTargetGroup.Android,
                BuildTarget.iOS => BuildTargetGroup.iOS,
                _ => BuildTargetGroup.Unknown
            };
        }

        private static void SaveQueue(BuildQueueState queueState)
        {
            SessionState.SetString(QueueSessionKey, JsonUtility.ToJson(queueState));
        }

        private static BuildQueueState LoadQueue()
        {
            string raw = SessionState.GetString(QueueSessionKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            return JsonUtility.FromJson<BuildQueueState>(raw);
        }

        private static void ClearQueue()
        {
            SessionState.EraseString(QueueSessionKey);
        }
    }
}
