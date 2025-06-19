using UnityEngine;
using UnityEditor;

public class AssetBundleScript
{
    [MenuItem("Assets/Build Test Asset Bundle")]
    static void BuildAssetBundle()
    {
        string assetBundleDirectory = "Assets/AssetBundles/";
        if (!System.IO.Directory.Exists(assetBundleDirectory))
        {
            System.IO.Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }
}
