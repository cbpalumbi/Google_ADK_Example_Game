using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading
using System.Linq; // Required for parsing command line arguments

public class SceneLoader : MonoBehaviour
{
    // This is the scene that will load if no specific scene is requested via command line.
    [SerializeField] private string defaultSceneName = "Mission1"; // Set your desired default here

    void Start()
    {
        Debug.Log("[SceneLoader] Bootstrapper scene loaded.");

        string sceneToLoad = defaultSceneName;

        // Check command line arguments for a specific scene to load
        string[] args = System.Environment.GetCommandLineArgs();
        Debug.Log($"[SceneLoader] Command line arguments received: {string.Join(" ", args)}");

        // Look for the argument "-startScene <SceneName>"
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-startScene" && i + 1 < args.Length)
            {
                sceneToLoad = args[i + 1];
                Debug.Log($"[SceneLoader] Found -startScene argument. Requesting to load: {sceneToLoad}");
                break; // Found the scene, no need to check further
            }
        }

        // Ensure the scene exists in Build Settings before trying to load it
        // (Optional but good for robust error checking)
        bool sceneExistsInBuildSettings = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneToLoad)
            {
                sceneExistsInBuildSettings = true;
                break;
            }
        }

        if (sceneExistsInBuildSettings)
        {
            // Load the specified scene
            Debug.Log($"[SceneLoader] Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"[SceneLoader] ERROR: Scene '{sceneToLoad}' not found in Build Settings or not enabled. Loading default scene: {defaultSceneName} instead.");
            SceneManager.LoadScene(defaultSceneName);
        }
    }
}