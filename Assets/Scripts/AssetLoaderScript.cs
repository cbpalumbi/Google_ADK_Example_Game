using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic; 

public class AssetBundleLoader : MonoBehaviour
{
    // distance between spawned objects
    public float spacing = 3.0f;
    public Vector3 colliderCenterOffset = Vector3.zero;
    public float colliderRadiusMultiplier = 1.0f;

    void Start()
    {
#if UNITY_EDITOR
        Debug.Log("Exiting asset loader early because we are running in the editor");
        return;
#endif
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
                Vector3 position = new Vector3(count * spacing, 1, 0);
                GameObject instantiated = Instantiate(obj, position, Quaternion.identity);
                Debug.Log($"Instantiated '{obj.name}' at {position}");
                count++;

                Rigidbody rb = instantiated.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = instantiated.AddComponent<Rigidbody>();
                }

                SphereCollider sc = instantiated.GetComponent<SphereCollider>();
                if (sc == null)
                {
                    sc = instantiated.AddComponent<SphereCollider>();
                }

                Renderer[] renderers = instantiated.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    // Initialize bounds with the first renderer's bounds
                    Bounds combinedBounds = renderers[0].bounds;

                    // Encapsulate all other renderers' bounds into the combined bounds
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }

                    float maxDimension = combinedBounds.extents.magnitude;

                    // Set the SphereCollider's radius and center
                    sc.radius = maxDimension * colliderRadiusMultiplier;
                    // The center of the collider should be relative to the GameObject's local origin.
                    // We need to convert the world-space combinedBounds.center to local space.
                    sc.center = transform.InverseTransformPoint(combinedBounds.center) + colliderCenterOffset;
                }
                else
                {
                    // If no renderers are found, the SphereCollider will keep its default size (radius = 0.5, center = 0,0,0)
                    // You might want to set a default sensible size here if renderers are optional.
                    sc.radius = 0.5f * colliderRadiusMultiplier; // Example default
                    sc.center = colliderCenterOffset;
                }


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
