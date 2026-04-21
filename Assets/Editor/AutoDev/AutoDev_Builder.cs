using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoDev
{
    public static class Builder
    {
        private static readonly string ResultPath = "Automation/build_result.json";

        public static void BuildStandaloneLinux64()
        {
            Build(BuildTarget.StandaloneLinux64, "Builds/StandaloneLinux64");
        }

        public static void BuildStandaloneWindows64()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/StandaloneWindows64");
        }

        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, "Builds/Android");
        }

        private static void Build(BuildTarget target, string outputDir)
        {
            Directory.CreateDirectory("Automation");

            var result = new BuildResult
            {
                target = target.ToString(),
                outputPath = outputDir,
                timestamp = DateTime.Now.ToString("O"),
                success = false,
                messages = new System.Collections.Generic.List<string>()
            };

            try
            {
                string[] scenes = GetEnabledScenes();
                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = Path.Combine(outputDir, GetExecutableName(target)),
                    target = target,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(options);
                result.success = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
                if (!result.success)
                {
                    result.messages.Add($"Build failed with result: {report.summary.result}");
                    result.messages.Add($"Total errors: {report.summary.totalErrors}");
                    result.messages.Add($"Total warnings: {report.summary.totalWarnings}");
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.messages.Add($"Exception: {ex.Message}\n{ex.StackTrace}");
            }

            string json = JsonUtility.ToJson(result, true);
            File.WriteAllText(ResultPath, json);

            EditorApplication.Exit(result.success ? 0 : 1);
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            if (scenes.Count == 0)
            {
                scenes.Add("Assets/Scenes/SampleScene.unity");
            }
            return scenes.ToArray();
        }

        private static string GetExecutableName(BuildTarget target)
        {
            string product = PlayerSettings.productName;
            if (string.IsNullOrEmpty(product)) product = "Game";
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return product + ".exe";
                case BuildTarget.Android:
                    return product + ".apk";
                case BuildTarget.StandaloneLinux64:
                    return product;
                default:
                    return product;
            }
        }
    }

    [Serializable]
    public class BuildResult
    {
        public string target;
        public string outputPath;
        public string timestamp;
        public bool success;
        public System.Collections.Generic.List<string> messages;
    }
}
