using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void Build()
    {
        string[] scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/StandaloneLinux64/TestGuaguale", BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("Build complete: Builds/StandaloneLinux64/TestGuaguale");
    }
}
