using UnityEngine;
using System.IO; 
using System.Linq; 

public class AssetBundleLoader : MonoBehaviour
{
    // IMPORTANT: Replace this with the *exact* name of the prefab you want to load from your bundle.
    // This is the name you gave it in Unity's Project window when creating the prefab,
    // or the name of the main FBX asset if you're loading it directly.
    public string prefabNameToLoad = "apple";

    void Start()
    {
        // 1. Get all command-line arguments
        string[] args = System.Environment.GetCommandLineArgs();

        string bundlePath = "Assets/AssetBundles/testassets";

        LoadAssetBundleFromFile(bundlePath);
    }

    private void LoadAssetBundleFromFile(string path)
    {
        // Ensure the file exists before trying to load it
        if (!File.Exists(path))
        {
            Debug.LogError($"Asset Bundle file not found at: {path}");
            return;
        }

        AssetBundle loadedBundle = null;
        try
        {
            // Load the Asset Bundle from the specified file path
            loadedBundle = AssetBundle.LoadFromFile(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load Asset Bundle from {path}. Error: {e.Message}");
            return;
        }

        if (loadedBundle == null)
        {
            Debug.LogError("Failed to load Asset Bundle! Is the path correct and the bundle valid?");
            return;
        }

        Debug.Log($"Asset Bundle '{loadedBundle.name}' loaded successfully.");

        // 3. Load and instantiate the prefab from the loaded bundle
        GameObject prefab = loadedBundle.LoadAsset<GameObject>(prefabNameToLoad);

        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab '{prefabNameToLoad}' from Asset Bundle '{loadedBundle.name}'. " +
                           "Please ensure the prefab name is correct and it was included in the bundle.");
            // Useful for debugging: list all asset names within the bundle
            Debug.Log("Assets found in bundle: " + string.Join(", ", loadedBundle.GetAllAssetNames()));
            loadedBundle.Unload(false); // Unload the bundle if the target prefab isn't found
            return;
        }

        // Instantiate the loaded prefab into the scene
        Instantiate(prefab, Vector3.zero, Quaternion.identity); // Place at origin, no rotation
        Debug.Log($"Prefab '{prefab.name}' instantiated from bundle.");

        // IMPORTANT: Decide if you want to unload the bundle immediately.
        // If you unload with 'true', all assets loaded from it will also be destroyed.
        // If you unload with 'false', instantiated objects will remain, but their references
        // to the bundle's internal data might be broken if you later try to load more.
        // For a simple viewer, you might keep it loaded or unload if no more assets are needed.
        // loadedBundle.Unload(false);
    }
}