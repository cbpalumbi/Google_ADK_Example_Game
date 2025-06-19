using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic; 

public class AssetBundleLoader : MonoBehaviour
{
    // distance between spawned objects
    public float spacing = 3.0f;

    void Start()
    {
        string rootDirectoryForExternalBundles;

        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            // On macOS, Application.dataPath is YourGame.app/Contents/
            // Path.GetDirectoryName(Application.dataPath) gives YourGame.app/
            // Calling Path.GetDirectoryName again gives the folder containing YourGame.app
            rootDirectoryForExternalBundles = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
        }
        else
        {
            // On Windows, Application.dataPath is YourGame_Data/
            // Path.GetDirectoryName(Application.dataPath) gives the folder containing YourGame.exe
            rootDirectoryForExternalBundles = Path.GetDirectoryName(Application.dataPath);
        }

        string myBundlesFolder = Path.Combine(rootDirectoryForExternalBundles, "MyAssetBundles");


        List<string> bundlesToLoad = new List<string> { "assets", "userassets" }; 

        AssetBundle masterBundle = AssetBundle.LoadFromFile(Path.Combine(myBundlesFolder, bundlesToLoad[0]));

        string bundlePath = Path.Combine(myBundlesFolder, bundlesToLoad[1]);
        Debug.Log($"Loading AssetBundle from: {bundlePath}");
        LoadAndInstantiateAllGameObjects(bundlePath);
        
    }

    private void LoadAndInstantiateAllGameObjects(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"AssetBundle not found at: {path}");
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle.");
            return;
        }

        string[] assetNames = bundle.GetAllAssetNames();
        int count = 0;

        foreach (string assetName in assetNames)
        {
            GameObject obj = bundle.LoadAsset<GameObject>(assetName);
            if (obj != null)
            {
                Vector3 position = new Vector3(count * spacing, 0, 0);
                Instantiate(obj, position, Quaternion.identity);
                Debug.Log($"Instantiated '{obj.name}' at {position}");
                count++;
            }
            else
            {
                Debug.LogWarning($"Skipping non-GameObject asset: {assetName}");
            }
        }

        if (count == 0)
        {
            Debug.LogWarning("No GameObjects were loaded from the bundle.");
        }

        bundle.Unload(false);
    }
}
