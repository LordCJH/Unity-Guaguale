using UnityEditor;

public class ForceRecompile
{
    [MenuItem("Tools/Force Recompile")]
    public static void Recompile()
    {
        AssetDatabase.ImportAsset("Assets/Scripts/ScratchCard/ScratchCardGame.cs", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Scripts/ScratchCard/ScratchCardSelfTest.cs", ImportAssetOptions.ForceUpdate);
    }
}
