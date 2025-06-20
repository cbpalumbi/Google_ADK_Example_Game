using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AssetBundleScript
{
    public static void BuildAssetBundleForUploadedGLBs()
    {
        
        // 1. Read args to get session ID
        string[] args = System.Environment.GetCommandLineArgs();
        
        string sessionId = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-session" && i + 1 < args.Length)
            {
                sessionId = args[i + 1];
                break;
            }
        }


        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogError("No session ID provided! Use -session=session123 as a CLI arg.");
            return;
        }

        // 2. Define paths
        string sourcePathWithSessionId = Path.Combine(Directory.GetCurrentDirectory(), "UserGlbFiles", sessionId);
        string sourcePath = Path.Combine(sourcePathWithSessionId, "assets");
        string importPath = Path.Combine("Assets", "UserUploaded", sessionId);
        string bundleOutputPathWithSessionId = Path.Combine("UserAssetBundles", sessionId);
        string bundleOutputPath = Path.Combine(bundleOutputPathWithSessionId, "assets");

        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"Source folder does not exist: {sourcePath}");
            return;
        }

        if (!Directory.EnumerateFiles(sourcePath).Any()) 
        {
            Debug.LogError($"Source folder '{sourcePath}' contains no files. Cannot proceed with build.");
            return;
        }

        Directory.CreateDirectory(importPath);
        Directory.CreateDirectory(bundleOutputPath);

        // 3. Copy .glb files to Unity project folder
        foreach (string file in Directory.GetFiles(sourcePath, "*.glb"))
        {
            string dest = Path.Combine(importPath, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        AssetDatabase.Refresh();

        string USER_ASSETS_BUNDLE_NAME = "userassets";
        Debug.Log($"Cleaning up existing AssetBundle assignments for '{USER_ASSETS_BUNDLE_NAME}'...");
        string[] assetsCurrentlyInBundle = AssetDatabase.GetAssetPathsFromAssetBundle(USER_ASSETS_BUNDLE_NAME);

        foreach (string assetPath in assetsCurrentlyInBundle)
        {
            // If the asset's path does NOT start with the current session's import path, clear its bundle name.
            // Append a '/' to importPath to ensure we match a folder, not just a prefix.
            if (!assetPath.StartsWith(importPath + "/"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && importer.assetBundleName == USER_ASSETS_BUNDLE_NAME)
                {
                    importer.assetBundleName = ""; // Set to empty string to remove from any bundle
                    Debug.Log($"Cleared '{USER_ASSETS_BUNDLE_NAME}' from old asset: {assetPath}");
                }
            }
        }
        
        // Save the changes to the meta files after clearing assignments.
        AssetDatabase.SaveAssets();
        // Remove any bundle names that no longer have assigned assets (optional, good practice)
        AssetDatabase.RemoveUnusedAssetBundleNames(); 
        Debug.Log("Cleanup of old AssetBundle assignments complete.");

        // 4. Import and label .glb assets for the bundle
        string[] assetPaths = Directory.GetFiles(importPath, "*.glb")
            .Select(path => path.Replace("\\", "/"))
            .ToArray();

        foreach (var path in assetPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            importer.assetBundleName = "userassets";
            Debug.Log($"Assigned {path} to AssetBundle 'uploadedassets'");
        }

        // 5. Build the AssetBundle
        BuildPipeline.BuildAssetBundles(
            bundleOutputPath,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64 // StandaloneOSX or StandaloneWindows64
        );

        Debug.Log($"AssetBundle built at: {Path.Combine(bundleOutputPath, "uploadedassets")}");
        AssetDatabase.Refresh();
    }
}
