using UnityEngine;
using System.IO;

public class RuntimeAssetLoader : MonoBehaviour
{
    [Tooltip("Adjustable spacing between loaded assets.")]
    [SerializeField]
    private float spacing = 5f; // Private variable for adjustable spacing

    private const string folderPath = "Assets/Resources/RuntimeLoadedObjs/";

    void Start()
    {
        LoadAndPlaceAssets();
    }

    void LoadAndPlaceAssets()
    {
        if (!Directory.Exists(Application.dataPath + "/" + folderPath.Replace("Assets/", "")))
        {
            Debug.LogError("RuntimeLoadedObjs folder not found at: " + Application.dataPath + "/" + folderPath.Replace("Assets/", ""));
            return;
        }

        string[] assetPaths = Directory.GetFiles(Application.dataPath + "/" + folderPath.Replace("Assets/", ""));
        int assetCount = 0;

        foreach (string path in assetPaths)
        {
            
            // Only consider valid Unity asset files (filter out .meta files and other non-model files)
            if (path.EndsWith(".fbx") || path.EndsWith(".obj") || path.EndsWith(".prefab") || path.EndsWith(".blend"))
            {
                // Get the path relative to the Assets folder for Resources.Load
                string relativePath = path.Replace(Application.dataPath + "/", "");
                relativePath = relativePath.Replace(Path.GetExtension(relativePath), ""); // Remove extension for Resources.Load
                relativePath = relativePath.Replace("Resources/", ""); // Remove "Resources" 

                // Load the asset
                Debug.Log("relative path is " + relativePath);
                GameObject loadedAsset = Resources.Load<GameObject>(relativePath);

                if (loadedAsset != null)
                {
                    // Instantiate the loaded asset in the scene
                    GameObject instantiatedAsset = Instantiate(loadedAsset);
                    instantiatedAsset.name = loadedAsset.name; // Keep original name

                    // Position the asset in a row
                    instantiatedAsset.transform.position = new Vector3(assetCount * spacing, 0, 0);

                    assetCount++;
                }
                else
                {
                    Debug.LogWarning("Failed to load asset at path: " + relativePath + ". Make sure it's a valid GameObject or Prefab.");
                }
            }
        }

        if (assetCount == 0)
        {
            Debug.LogWarning("No loadable assets found in " + folderPath + ". Supported types: .fbx, .obj, .prefab, .blend.");
        }
    }
}