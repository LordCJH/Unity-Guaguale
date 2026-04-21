using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BatchTestRunner
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Play mode exited. Shutting down editor.");
                EditorApplication.Exit(0);
            }
        };

        Debug.Log("Entering Play Mode for self-test...");
        EditorApplication.EnterPlaymode();
    }
}
